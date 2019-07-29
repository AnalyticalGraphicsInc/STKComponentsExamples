package agi.examples;

import agi.foundation.access.AccessEvaluator;
import agi.foundation.access.AccessQueryResult;
import agi.foundation.access.LinkSpeedOfLight;
import agi.foundation.access.constraints.CentralBodyObstructionConstraint;
import agi.foundation.celestial.CentralBodiesFacet;
import agi.foundation.celestial.EarthCentralBody;
import agi.foundation.celestial.SunCentralBody;
import agi.foundation.geometry.Point;
import agi.foundation.time.JulianDate;
import agi.foundation.time.TimeInterval;
import agi.foundation.time.TimeIntervalCollection;

/**
 * Contains helper methods used in multiple demos to calculate viewing times.
 */
public final class VisibilityHelper {
    private VisibilityHelper() {}

    /**
     * Finds the first time in a given interval that a given location is in sunlight.
     * This is used in multiple demos to set the animation time.
     * @param target The target location to view in sunlight.
     * @param consideredInterval The interval to consider.
     * @return The first time within the given date range where the target location is in sunlight,
     * or the start of the given interval if it is never in sunlight.
     */
    public static JulianDate viewPointInSunlight(Point target, TimeInterval consideredInterval) {
        EarthCentralBody earth = CentralBodiesFacet.getFromContext().getEarth();
        SunCentralBody sun = CentralBodiesFacet.getFromContext().getSun();

        LinkSpeedOfLight sunlight = new LinkSpeedOfLight(sun, target, earth.getInertialFrame());
        CentralBodyObstructionConstraint sunlightConstraint = new CentralBodyObstructionConstraint(sunlight, earth);

        // The target is in sunlight when the sunlight link is not obstructed by the Earth.
        try (AccessEvaluator accessEvaluator = sunlightConstraint.getEvaluator(target)) {
            AccessQueryResult accessResult = accessEvaluator.evaluate(consideredInterval);
            TimeIntervalCollection satisfactionIntervals = accessResult.getSatisfactionIntervals();

            if (satisfactionIntervals.isEmpty()) {
                // No valid times (unlikely).  Just use the start date.
                return consideredInterval.getStart();
            }

            return satisfactionIntervals.getStart();
        }
    }
}