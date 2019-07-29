package com.agi.satellitetracker;

import java.util.ArrayList;
import java.util.List;

import agi.foundation.Constants;
import agi.foundation.IEvaluator1;
import agi.foundation.MotionEvaluator1;
import agi.foundation.Trig;
import agi.foundation.access.AccessEvaluator;
import agi.foundation.access.AccessQueryResult;
import agi.foundation.access.LinkInstantaneous;
import agi.foundation.access.LinkRole;
import agi.foundation.access.constraints.ElevationAngleConstraint;
import agi.foundation.celestial.CentralBodiesFacet;
import agi.foundation.celestial.EarthCentralBody;
import agi.foundation.coordinates.AzimuthElevationRange;
import agi.foundation.coordinates.Cartographic;
import agi.foundation.geometry.Axes;
import agi.foundation.geometry.Point;
import agi.foundation.geometry.PointCartographic;
import agi.foundation.geometry.VectorTrueDisplacement;
import agi.foundation.platforms.Platform;
import agi.foundation.propagators.Sgp4Propagator;
import agi.foundation.propagators.TwoLineElementSet;
import agi.foundation.time.Duration;
import agi.foundation.time.JulianDate;
import agi.foundation.time.TimeInterval;
import agi.foundation.time.TimeIntervalCollection;

/**
 * Stores input data and results for a single computation.
 */
public class Computation {
    /**
     * Construct a computation, and perform the calculation. After construction,
     * the results will be available by using the accessors on this instance.
     *
     * @param userInput
     *            The user input to use for the computation.
     */
    public Computation(UserInput userInput) {
        // convert the input start/end dates to JulianDates for ease of use
        m_startDate = userInput.getStartDate().toJulianDate();
        m_endDate = userInput.getEndDate().toJulianDate();

        createSatellite(userInput);
        createFacility(userInput);

        AccessEvaluator accessEval = createAccessEvaluator(userInput);

        calculateAccess(accessEval);
        calculateApproachAndDeparture();
        calculateGroundTrack();
    }

    /**
     * Create a Platform representing the satellite.
     */
    private void createSatellite(UserInput userInput) {
        // Create an SGP4 propagator to propagate the TLE.
        Sgp4Propagator propagator = new Sgp4Propagator(new TwoLineElementSet(userInput.getTle()));
        m_satellite = new Platform();

        // Create a Point representing the position as reported by the propagator.
        // The propagator produces raw ephemeris, while the Point enables the
        // results of propagation to work with the GeometryTransformer in order
        // to observe the ephemeris in different reference frames.
        m_satellite.setLocationPoint(propagator.createPoint());
        m_satellite.setOrientationAxes(Axes.getRoot());
    }

    /**
     * Create a Platform representing the ground facility.
     */
    private void createFacility(UserInput userInput) {
        EarthCentralBody earth = CentralBodiesFacet.getFromContext().getEarth();

        // Create a facility at the viewing location. The longitude and latitude
        // of the facility's location are specified using radians, so convert
        // degrees to radians.
        double longitude = Trig.degreesToRadians(userInput.getLongitude());
        double latitude = Trig.degreesToRadians(userInput.getLatitude());

        m_facility = new Platform();
        m_facility.setLocationPoint(new PointCartographic(earth, new Cartographic(longitude, latitude, 0.0)));
        m_facility.setOrientationAxes(Axes.getRoot());
    }

    /**
     * Create an AccessEvaluator from an appropriate ElevationAngleConstraint.
     */
    private AccessEvaluator createAccessEvaluator(UserInput userInput) {
        // Create an Access constraint requiring that the satellite be above a particular
        // elevation angle relative to the local horizontal plane of the facility.
        ElevationAngleConstraint elevationAngleConstraint = new ElevationAngleConstraint();
        elevationAngleConstraint.setMinimumValue(Trig.degreesToRadians(userInput.getElevation()));
        elevationAngleConstraint.setMaximumValue(Constants.HalfPi);

        // Create the link for the access constraint. It does not matter which is the
        // transmitter and which is the receiver, but the elevation angle
        // constraint must be applied to the facility.

        elevationAngleConstraint.setConstrainedLink(new LinkInstantaneous(m_satellite, m_facility));
        elevationAngleConstraint.setConstrainedLinkEnd(LinkRole.RECEIVER);

        // Create the access evaluator. An access evaluator generally needs either a specified
        // observer or an AccessQueryOption with a specified observer in it. This is because
        // access computations with light time delays can cause different platforms to have
        // different time intervals for the same constraints. For simple AccessQueries with
        // only one LinkInstantaneous (like this one) a default AccessQueryOption can be used,
        // but generally a platform will need to be specified as we've done here.
        return elevationAngleConstraint.getEvaluator(m_facility);
    }

    /**
     * Calculate the time intervals when the satellite is visible from the
     * viewing location.
     */
    private void calculateAccess(AccessEvaluator evaluator) {
        AccessQueryResult accessResult = evaluator.evaluate(m_startDate, m_endDate);
        m_intervals = accessResult.getSatisfactionIntervals();
    }

    /**
     * Calculate the approach and departure azimuth, elevation, and range.
     */
    private void calculateApproachAndDeparture() {
        // Get the Earth central body. We will use it below to compute the
        // approach and departure azimuths and elevations.
        EarthCentralBody earth = CentralBodiesFacet.getFromContext().getEarth();

        // Get an evaluator to find the topographic azimuth, elevation, and
        // range of the satellite as observed by the viewing location. We'll use
        // this evaluator to evaluate the AER at the start and end of each
        // viewing opportunity interval.
        Point facilityLocationPoint = m_facility.getLocationPoint();
        Point satelliteLocationPoint = m_satellite.getLocationPoint();
        VectorTrueDisplacement vector = new VectorTrueDisplacement(facilityLocationPoint, satelliteLocationPoint);
        MotionEvaluator1<AzimuthElevationRange> aerEvaluator = earth.getAzimuthElevationRangeEvaluator(vector);

        m_approachAers = new ArrayList<>(m_intervals.size());
        m_departureAers = new ArrayList<>(m_intervals.size());
        for (TimeInterval interval : m_intervals) {
            // Compute the approach and departure azimuth, elevation, and range.
            m_approachAers.add(aerEvaluator.evaluate(interval.getStart()));
            m_departureAers.add(aerEvaluator.evaluate(interval.getStop()));
        }
    }

    /**
     * Calculate the list of ground track positions that will be used to draw
     * the ground track on the map.
     */
    private void calculateGroundTrack() {
        EarthCentralBody earth = CentralBodiesFacet.getFromContext().getEarth();

        // Get an evaluator to tell us the location of the satellite in
        // longitude, latitude, and altitude coordinates relative to the Earth.
        IEvaluator1<Cartographic> evaluator = earth.observeCartographicPoint(m_satellite.getLocationPoint());

        m_groundTracks = new ArrayList<>();

        // if we have no access intervals, create ground tracks with no access
        // for the entire computation period.
        if (m_intervals.isEmpty()) {
            addGroundTracks(m_startDate, m_endDate, evaluator, false);
            return;
        }

        // otherwise, add ground tracks with no access for the period between
        // the start date of the computation and the start of the first access
        addGroundTracks(m_startDate, m_intervals.getStart(), evaluator, false);

        // then for each interval with access...
        for (int i = 0; i < m_intervals.size(); ++i) {
            TimeInterval interval = m_intervals.get(i);

            // add ground tracks with access for the interval
            addGroundTracks(interval.getStart(), interval.getStop(), evaluator, true);

            // add ground tracks with no access for the gap between this
            // interval and the next (if there is one)
            if (i + 1 < m_intervals.size()) {
                TimeInterval nextInterval = m_intervals.get(i + 1);
                addGroundTracks(interval.getStop(), nextInterval.getStart(), evaluator, false);
            }
        }

        // add ground tracks with no access for the period between the end date
        // of the last access and the end date of the computation.
        addGroundTracks(m_intervals.getStop(), m_endDate, evaluator, false);
    }

    /**
     * Add ground track position objects, all with a particular value for
     * access, for each point in time between start and end, stepping by
     * groundTrackTimeStep.
     */
    private void addGroundTracks(JulianDate start, JulianDate end, IEvaluator1<Cartographic> evaluator, boolean hasAccess) {
        JulianDate date = start;
        while (JulianDate.lessThan(date, end)) {
            // calculate the ground position at the current time and add a new
            // GroundTrackPosition
            m_groundTracks.add(new GroundTrackPosition(evaluator.evaluate(date), hasAccess));

            date = date.add(s_roundTrackTimeStep);
        }
        // add a final ground position for the end time
        m_groundTracks.add(new GroundTrackPosition(evaluator.evaluate(end), hasAccess));
    }

    public List<AzimuthElevationRange> getApproachAers() {
        return m_approachAers;
    }

    public List<AzimuthElevationRange> getDepartureAers() {
        return m_departureAers;
    }

    public Platform getFacility() {
        return m_facility;
    }

    public TimeIntervalCollection getIntervals() {
        return m_intervals;
    }

    public Platform getSatellite() {
        return m_satellite;
    }

    public List<GroundTrackPosition> getGroundTracks() {
        return m_groundTracks;
    }

    /**
     * The duration of each ground track segment.
     */
    private static final Duration s_roundTrackTimeStep = Duration.fromSeconds(60.0);
    private final JulianDate m_startDate;
    private final JulianDate m_endDate;
    private Platform m_satellite;
    private Platform m_facility;
    private TimeIntervalCollection m_intervals;
    private List<AzimuthElevationRange> m_approachAers;
    private List<AzimuthElevationRange> m_departureAers;
    private List<GroundTrackPosition> m_groundTracks;
}
