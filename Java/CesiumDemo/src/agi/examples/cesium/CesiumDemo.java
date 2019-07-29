package agi.examples.cesium;

import static agi.foundation.cesium.ConstantCesiumProperty.toConstantCesiumProperty;
import static agi.foundation.cesium.advanced.CesiumProperty.toCesiumProperty;

import java.awt.Color;
import java.io.Writer;
import java.net.MalformedURLException;
import java.net.URI;
import java.net.URISyntaxException;
import java.net.URL;
import java.util.Arrays;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import agi.examples.TwoLineElementSetHelper;
import agi.foundation.Bounds;
import agi.foundation.Constants;
import agi.foundation.Trig;
import agi.foundation.access.AccessQuery;
import agi.foundation.access.LinkInstantaneous;
import agi.foundation.access.LinkRole;
import agi.foundation.access.constraints.AzimuthElevationMaskConstraint;
import agi.foundation.access.constraints.CentralBodyObstructionConstraint;
import agi.foundation.celestial.CentralBodiesFacet;
import agi.foundation.celestial.EarthCentralBody;
import agi.foundation.cesium.AccessQueryCesiumProperty;
import agi.foundation.cesium.AxesCesiumProperty;
import agi.foundation.cesium.AzimuthElevationMaskGraphics;
import agi.foundation.cesium.AzimuthElevationMaskGraphicsExtension;
import agi.foundation.cesium.AzimuthElevationMaskGraphicsProjection;
import agi.foundation.cesium.CesiumHeightReference;
import agi.foundation.cesium.CesiumInterpolationAlgorithm;
import agi.foundation.cesium.CesiumPositionExtension;
import agi.foundation.cesium.CesiumResource;
import agi.foundation.cesium.CesiumResourceBehavior;
import agi.foundation.cesium.Clock;
import agi.foundation.cesium.CzmlDocument;
import agi.foundation.cesium.Description;
import agi.foundation.cesium.DescriptionExtension;
import agi.foundation.cesium.FieldOfViewGraphicsExtension;
import agi.foundation.cesium.GraphicalVector;
import agi.foundation.cesium.GridMaterialGraphics;
import agi.foundation.cesium.LabelGraphics;
import agi.foundation.cesium.LabelGraphicsExtension;
import agi.foundation.cesium.LinkGraphics;
import agi.foundation.cesium.LinkGraphicsExtension;
import agi.foundation.cesium.ModelGraphics;
import agi.foundation.cesium.ModelGraphicsExtension;
import agi.foundation.cesium.NodeTransformationGraphics;
import agi.foundation.cesium.PathGraphics;
import agi.foundation.cesium.PathGraphicsExtension;
import agi.foundation.cesium.PolylineOutlineMaterialGraphics;
import agi.foundation.cesium.SensorFieldOfViewGraphics;
import agi.foundation.cesium.SolidColorMaterialGraphics;
import agi.foundation.cesium.StripeMaterialGraphics;
import agi.foundation.cesium.VectorGraphics;
import agi.foundation.coordinates.AngleAxisRotation;
import agi.foundation.coordinates.AxisIndicator;
import agi.foundation.coordinates.Cartesian;
import agi.foundation.coordinates.CartesianElement;
import agi.foundation.coordinates.Cartographic;
import agi.foundation.coordinates.ElementaryRotation;
import agi.foundation.coordinates.UnitCartesian;
import agi.foundation.coordinates.UnitQuaternion;
import agi.foundation.geometry.Axes;
import agi.foundation.geometry.AxesAlignedConstrained;
import agi.foundation.geometry.AxesEastNorthUp;
import agi.foundation.geometry.AxesFixedOffset;
import agi.foundation.geometry.AxesInAxes;
import agi.foundation.geometry.AxesLinearRate;
import agi.foundation.geometry.AxesNorthEastDown;
import agi.foundation.geometry.AxesTargetingLink;
import agi.foundation.geometry.AxesVehicleVelocityLocalHorizontal;
import agi.foundation.geometry.Point;
import agi.foundation.geometry.PointCartographic;
import agi.foundation.geometry.PointFixedOffset;
import agi.foundation.geometry.Vector;
import agi.foundation.geometry.VectorFixed;
import agi.foundation.geometry.VectorTrueDisplacement;
import agi.foundation.geometry.shapes.ComplexConic;
import agi.foundation.geometry.shapes.RectangularPyramid;
import agi.foundation.infrastructure.IdentifierExtension;
import agi.foundation.platforms.AzimuthElevationMaskExtension;
import agi.foundation.platforms.FieldOfViewExtension;
import agi.foundation.platforms.Platform;
import agi.foundation.propagators.Sgp4Propagator;
import agi.foundation.propagators.TwoLineElementSet;
import agi.foundation.propagators.Waypoint;
import agi.foundation.propagators.WaypointPropagator;
import agi.foundation.terrain.AGIProcessedDataTerrain;
import agi.foundation.terrain.AzimuthElevationMask;
import agi.foundation.terrain.TerrainAzimuthElevationMask;
import agi.foundation.terrain.TerrainProvider;
import agi.foundation.time.Duration;
import agi.foundation.time.JulianDate;
import agi.foundation.time.TimeInterval;
import agi.foundation.time.TimeInterval1;
import agi.foundation.time.TimeIntervalCollection1;

/**
 * This class builds the set of objects which will be
 * written out to a CZML document and sent to the client.
 */
public class CesiumDemo {
    /**
     * Creates all objects in the demonstration.
     * @param satelliteIdentifier The NORAD identifier for the satellite to propagate.
     */
    public CesiumDemo(String satelliteIdentifier, String requestUrl) {
        m_satelliteIdentifier = satelliteIdentifier;
        m_requestUrl = requestUrl;
        m_earth = CentralBodiesFacet.getFromContext().getEarth();

        // Create all objects for our scenario.

        // Each of these helper methods create some of the objects
        // which are stored in fields.
        // The work is divided up across methods to make the code easier to follow.
        createSatellite();
        createFacility();
        createSatelliteAccessLink();
        createFacilitySensor();
        createSensorDome();
        createAzimuthElevationMask();
        createAircraft();
    }

    /**
     * Create a Platform for the requested satellite using a TLE for position.
     * The satellite will be visually represented by a labeled glTF model,
     * the satellite's orbit will be shown, and vectors will be drawn for
     * the body axes of the satellite, plus a vector indicating the direction
     * of the sun.
     */
    private void createSatellite() {
        // Get the current TLE for the given satellite identifier.
        List<TwoLineElementSet> tleList = TwoLineElementSetHelper.getTles(m_satelliteIdentifier, JulianDate.getNow());

        // Use the epoch of the first TLE, since the TLE may have been loaded from offline data.
        m_epoch = tleList.get(0).getEpoch();

        // Propagate the TLE and use that as the satellite's location point.
        Point locationPoint = new Sgp4Propagator(tleList).createPoint();
        m_satellite = new Platform();
        m_satellite.setName("Satellite " + m_satelliteIdentifier);
        m_satellite.setLocationPoint(locationPoint);

        // Orient the satellite using Vehicle Velocity Local Horizontal (VVLH) axes.
        m_satellite.setOrientationAxes(new AxesVehicleVelocityLocalHorizontal(m_earth.getFixedFrame(), locationPoint));

        // Set the identifier for the satellite in the CZML document.
        m_satellite.getExtensions().add(new IdentifierExtension(m_satelliteIdentifier));

        // Configure a glTF model for the satellite.
        ModelGraphics modelGraphics = new ModelGraphics();

        // Link to a binary glTF file.
        modelGraphics.setModel(toCesiumProperty(new CesiumResource(getModelUri("satellite.glb"), CesiumResourceBehavior.LINK_TO)));

        // By default, Cesium plays all animations in the model simultaneously, which is not desirable.
        modelGraphics.setRunAnimations(toCesiumProperty(false));

        m_satellite.getExtensions().add(new ModelGraphicsExtension(modelGraphics));

        // Configure a label for the satellite.
        LabelGraphics labelGraphics = new LabelGraphics();

        // Use the name of the satellite as the text of the label.
        labelGraphics.setText(toCesiumProperty(m_satellite.getName()));

        // Change the color of the label after 12 hours. This demonstrates specifying that 
        // a value varies over time using intervals.
        TimeIntervalCollection1<Color> fillColor = new TimeIntervalCollection1<>();

        // Green for the first half day...
        fillColor.add(new TimeInterval1<>(JulianDate.getMinValue(), m_epoch.addDays(0.5), GREEN, true, false));

        // Red thereafter.
        fillColor.add(new TimeInterval1<>(m_epoch.addDays(0.5), JulianDate.getMaxValue(), RED, false, true));

        labelGraphics.setFillColor(toCesiumProperty(fillColor));

        // Only show label when camera is far enough from the satellite,
        // to avoid visually clashing with the model.
        labelGraphics.setDistanceDisplayCondition(toCesiumProperty(new Bounds(1000.0, Double.MAX_VALUE)));

        m_satellite.getExtensions().add(new LabelGraphicsExtension(labelGraphics));

        // Configure graphical display of the orbital path of the satellite.
        PathGraphics pathGraphics = new PathGraphics();

        // Configure the visual appearance of the line.
        PolylineOutlineMaterialGraphics pathGraphicsMaterial = new PolylineOutlineMaterialGraphics();
        pathGraphicsMaterial.setColor(toCesiumProperty(WHITE));
        pathGraphicsMaterial.setOutlineWidth(toCesiumProperty(1.0));
        pathGraphicsMaterial.setOutlineColor(toCesiumProperty(BLACK));
        pathGraphics.setMaterial(toCesiumProperty(pathGraphicsMaterial));

        pathGraphics.setWidth(toCesiumProperty(2.0));

        // Lead and Trail time indicate how much of the path to render.
        pathGraphics.setLeadTime(toCesiumProperty(Duration.fromMinutes(44.0).getTotalSeconds()));
        pathGraphics.setTrailTime(toCesiumProperty(Duration.fromMinutes(44.0).getTotalSeconds()));

        m_satellite.getExtensions().add(new PathGraphicsExtension(pathGraphics));

        // Create vectors for the X, Y, and Z axes of the satellite.
        m_satelliteXAxis = createAxesVector(m_satellite, CartesianElement.X, GREEN, "SatelliteX");
        m_satelliteYAxis = createAxesVector(m_satellite, CartesianElement.Y, RED, "SatelliteY");
        m_satelliteZAxis = createAxesVector(m_satellite, CartesianElement.Z, BLUE, "SatelliteZ");

        // Create a vector from the satellite to the Sun.

        // Compute the vector from the satellite's location to the Sun's center of mass.
        Point sunCenterOfMassPoint = CentralBodiesFacet.getFromContext().getSun().getCenterOfMassPoint();
        VectorTrueDisplacement vectorSatelliteToSun = new VectorTrueDisplacement(m_satellite.getLocationPoint(), sunCenterOfMassPoint);

        // Create the visual vector.
        m_satelliteSunVector = new GraphicalVector();
        m_satelliteSunVector.setLocationPoint(m_satellite.getLocationPoint());
        m_satelliteSunVector.setVector(vectorSatelliteToSun);

        VectorGraphics satelliteSunVectorGraphics = new VectorGraphics();
        satelliteSunVectorGraphics.setLength(toCesiumProperty(5.0));
        satelliteSunVectorGraphics.setColor(toCesiumProperty(YELLOW));

        m_satelliteSunVector.setVectorGraphics(satelliteSunVectorGraphics);

        // Set the identifier for the vector in the CZML document. 
        m_satelliteSunVector.getExtensions().add(new IdentifierExtension("SunVector"));

        // Orient the solar panels on the satellite model to point at the sun.
        Vector satelliteYVector = m_satellite.getOrientationAxes().getVectorElement(CartesianElement.Y);

        // allow only Z axis to rotate to follow sun vector. Constrain sun vector to Y, and satellite Y vector to X.
        Axes constrainedAxes = new AxesAlignedConstrained(satelliteYVector, AxisIndicator.FIRST, vectorSatelliteToSun, AxisIndicator.SECOND);

        // Satellite axes are Vehicle Velocity Local Horizontal (VVLH) axes, where X is forward and Z is down,
        // but Cesium model axes are Z forward, Y up. So, create an axes rotates to the Cesium model axes.
        UnitQuaternion offset = new UnitQuaternion(new ElementaryRotation(AxisIndicator.FIRST, -Math.PI / 2))
                .multiply(new UnitQuaternion(new ElementaryRotation(AxisIndicator.THIRD, Math.PI / 2)));
        Axes cesiumModelAxes = new AxesFixedOffset(m_satellite.getOrientationAxes(), offset);

        // The rotation will be from the Cesium model axes to the constrained axes.
        Axes solarPanelRotationAxes = new AxesInAxes(constrainedAxes, cesiumModelAxes);

        // Add a node transformation to rotate the SolarPanels node of the model.
        NodeTransformationGraphics solarPanelsNodeTransformation = new NodeTransformationGraphics();
        solarPanelsNodeTransformation.setRotation(new AxesCesiumProperty(solarPanelRotationAxes));

        Map<String, NodeTransformationGraphics> nodeTransformations = new HashMap<>();
        nodeTransformations.put("SolarPanels", solarPanelsNodeTransformation);

        modelGraphics.setNodeTransformations(nodeTransformations);
    }

    /**
     * Create a vector that will draw one axis of the satellite's body axes.
     */
    private static GraphicalVector createAxesVector(Platform platform, CartesianElement cartesianElement, Color color, String identifier) {
        GraphicalVector graphicalVector = new GraphicalVector();

        // Compute the vector for the given element in the satellite's body axes.
        graphicalVector.setVector(platform.getOrientationAxes().getVectorElement(cartesianElement));

        // Specify the visual appearance of the vector.
        VectorGraphics vectorGraphics = new VectorGraphics();
        vectorGraphics.setLength(toCesiumProperty(10.0));
        vectorGraphics.setColor(toCesiumProperty(color));

        graphicalVector.setVectorGraphics(vectorGraphics);

        graphicalVector.setLocationPoint(platform.getLocationPoint());

        // Set the identifier for the vector in the CZML document. 
        graphicalVector.getExtensions().add(new IdentifierExtension(identifier));

        return graphicalVector;
    }

    /**
     * Create a facility at the location of AGI headquarters.
     * The facility will be visually represented by a labeled glTF model.
     */
    private void createFacility() {
        // Define the location of the facility using cartographic coordinates.
        Cartographic location = new Cartographic(Trig.degreesToRadians(-75.596766667), Trig.degreesToRadians(40.0388333333), 0.0);
        Point locationPoint = new PointCartographic(m_earth, location);
        m_facility = new Platform();
        m_facility.setName("AGI HQ");
        m_facility.setLocationPoint(locationPoint);

        // Orient the facility using East-North-Up (ENU) axes.
        m_facility.setOrientationAxes(new AxesEastNorthUp(m_earth, locationPoint));

        // Set the identifier for the facility in the CZML document. 
        m_facility.getExtensions().add(new IdentifierExtension("AGI"));

        // Configure a glTF model for the facility.
        ModelGraphics modelGraphics = new ModelGraphics();

        // Link to a binary glTF file.
        modelGraphics.setModel(toCesiumProperty(new CesiumResource(getModelUri("facility.glb"), CesiumResourceBehavior.LINK_TO)));
        modelGraphics.setRunAnimations(toCesiumProperty(false));
        modelGraphics.setHeightReference(toCesiumProperty(CesiumHeightReference.CLAMP_TO_GROUND));

        m_facility.getExtensions().add(new ModelGraphicsExtension(modelGraphics));

        // Configure label for AGI HQ.
        LabelGraphics labelGraphics = new LabelGraphics();
        labelGraphics.setText(toCesiumProperty(m_facility.getName()));
        labelGraphics.setFillColor(toCesiumProperty(WHITE));
        labelGraphics.setHeightReference(toCesiumProperty(CesiumHeightReference.CLAMP_TO_GROUND));

        // Only show label when camera is far enough from the satellite,
        // to avoid visually clashing with the model.
        labelGraphics.setDistanceDisplayCondition(toCesiumProperty(new Bounds(1000.0, Double.MAX_VALUE)));

        m_facility.getExtensions().add(new LabelGraphicsExtension(labelGraphics));
    }

    /**
     * Create an access link between AGI HQ and the satellite.
     */
    private void createSatelliteAccessLink() {
        m_satelliteFacilityLink = new LinkInstantaneous(m_facility, m_satellite);

        // Set the identifier for the link in the CZML document. 
        m_satelliteFacilityLink.getExtensions().add(new IdentifierExtension("SatelliteFacilityAccess"));

        // Specify how access should be constrained.  In this case, 
        // access will only exist when no part of the earth is between AGI HQ and the satellite.
        m_accessQuery = new CentralBodyObstructionConstraint(m_satelliteFacilityLink, m_earth);

        // Configure graphical display of the access link.
        LinkGraphics linkGraphics = new LinkGraphics();

        // Show the access link only when access is satisfied.
        linkGraphics.setShow(new AccessQueryCesiumProperty<>(m_accessQuery, true, false, false));

        linkGraphics.setMaterial(toCesiumProperty(new SolidColorMaterialGraphics(YELLOW)));

        m_satelliteFacilityLink.getExtensions().add(new LinkGraphicsExtension(linkGraphics));
    }

    /**
     * Create a sensor, attached to the facility, oriented to target the satellite.
     */
    private void createFacilitySensor() {
        m_facilitySensor = new Platform();
        // Use the same location point as the facility.
        m_facilitySensor.setLocationPoint(m_facility.getLocationPoint());
        // Orient the sensor to target the link with the satellite.
        m_facilitySensor.setOrientationAxes(new AxesTargetingLink(m_satelliteFacilityLink, LinkRole.TRANSMITTER,
                new VectorFixed(m_satellite.getOrientationAxes(), Cartesian.toCartesian(UnitCartesian.getUnitZ()))));

        // Set the identifier for the facility in the CZML document. 
        m_facilitySensor.getExtensions().add(new IdentifierExtension("Sensor"));

        // Define the sensor geometry.
        RectangularPyramid fieldOfView = new RectangularPyramid();
        fieldOfView.setXHalfAngle(Trig.degreesToRadians(8.0));
        fieldOfView.setYHalfAngle(Trig.degreesToRadians(4.5));
        fieldOfView.setRadius(500000.0);

        m_facilitySensor.getExtensions().add(new FieldOfViewExtension(fieldOfView));

        // Configure graphical display of the sensor.
        SensorFieldOfViewGraphics sensorFieldOfViewGraphics = new SensorFieldOfViewGraphics();

        // Show the sensor only when access is satisfied.
        sensorFieldOfViewGraphics.setShow(new AccessQueryCesiumProperty<>(m_accessQuery, true, false, false));

        GridMaterialGraphics domeSurfaceMaterial = new GridMaterialGraphics();
        domeSurfaceMaterial.setColor(toCesiumProperty(WHITE));
        domeSurfaceMaterial.setCellAlpha(toCesiumProperty(0.0));
        sensorFieldOfViewGraphics.setDomeSurfaceMaterial(toCesiumProperty(domeSurfaceMaterial));

        SolidColorMaterialGraphics lateralSurfaceMaterial = new SolidColorMaterialGraphics(colorWithAlpha(GREEN, 128));
        sensorFieldOfViewGraphics.setLateralSurfaceMaterial(toCesiumProperty(lateralSurfaceMaterial));

        m_facilitySensor.getExtensions().add(new FieldOfViewGraphicsExtension(sensorFieldOfViewGraphics));
    }

    /**
     * Create another ground facility with a sensor dome, and a rotating sensor inside the dome.
     */
    private void createSensorDome() {
        // Define the location of the facility using cartographic coordinates.
        Point locationPoint = new PointCartographic(m_earth, new Cartographic(Trig.degreesToRadians(-122.3), Trig.degreesToRadians(46), 456.359));

        m_sensorDome = new Platform();
        m_sensorDome.setName("Sensor Dome");
        m_sensorDome.setLocationPoint(locationPoint);
        m_sensorDome.setOrientationAxes(new AxesEastNorthUp(m_earth, locationPoint));

        // Set the identifier for the facility in the CZML document. 
        m_sensorDome.getExtensions().add(new IdentifierExtension("SensorDome"));

        // Define the sensor geometry.
        ComplexConic dome = new ComplexConic();
        dome.setHalfAngles(0.0, Math.PI);
        dome.setClockAngles(0.0, Math.PI * 2);
        dome.setRadius(10000.0);
        m_sensorDome.getExtensions().add(new FieldOfViewExtension(dome));

        // Configure graphical display of the sensor dome.
        GridMaterialGraphics domeSurfaceMaterial = new GridMaterialGraphics();
        domeSurfaceMaterial.setColor(toCesiumProperty(WHITE));
        domeSurfaceMaterial.setCellAlpha(toCesiumProperty(0.1));

        SensorFieldOfViewGraphics sensorFieldOfViewGraphics = new SensorFieldOfViewGraphics();
        sensorFieldOfViewGraphics.setDomeSurfaceMaterial(toCesiumProperty(domeSurfaceMaterial));

        m_sensorDome.getExtensions().add(new FieldOfViewGraphicsExtension(sensorFieldOfViewGraphics));

        // Define a rotating axes.
        AxesLinearRate rotatingAxes = new AxesLinearRate();
        rotatingAxes.setReferenceAxes(new AxesEastNorthUp(m_earth, locationPoint));
        rotatingAxes.setReferenceEpoch(m_epoch);
        rotatingAxes.setInitialRotation(UnitQuaternion.getIdentity());
        rotatingAxes.setSpinAxis(UnitCartesian.getUnitZ());
        rotatingAxes.setInitialRotationalVelocity(Trig.degreesToRadians(5.0)); // 5 degrees per second
        rotatingAxes.setRotationalAcceleration(0.0);

        // Define a rotation around X.
        UnitQuaternion quaternion = new UnitQuaternion(new AngleAxisRotation(Math.PI / 3.0, UnitCartesian.getUnitX()));
        // Define an angular offset for the rotating axes.
        Axes rotatedOffsetAxes = new AxesFixedOffset(rotatingAxes, quaternion);

        m_rotatingSensor = new Platform();
        m_rotatingSensor.setName("Rotating Sensor");
        m_rotatingSensor.setLocationPoint(locationPoint);
        m_rotatingSensor.setOrientationAxes(rotatedOffsetAxes);

        // Set the identifier for the sensor in the CZML document. 
        m_rotatingSensor.getExtensions().add(new IdentifierExtension("RotatingSensor"));

        // Define the sensor geometry.
        RectangularPyramid pyramid = new RectangularPyramid();
        pyramid.setXHalfAngle(Trig.degreesToRadians(30));
        pyramid.setYHalfAngle(Trig.degreesToRadians(30));
        pyramid.setRadius(10000.0);
        m_rotatingSensor.getExtensions().add(new FieldOfViewExtension(pyramid));

        // Configure graphical display of the sensor.
        domeSurfaceMaterial = new GridMaterialGraphics();
        domeSurfaceMaterial.setColor(toCesiumProperty(GREEN));
        domeSurfaceMaterial.setCellAlpha(toCesiumProperty(0.5));

        GridMaterialGraphics lateralSurfaceMaterial = new GridMaterialGraphics();
        lateralSurfaceMaterial.setColor(toCesiumProperty(PINK));
        lateralSurfaceMaterial.setCellAlpha(toCesiumProperty(0.5));

        sensorFieldOfViewGraphics = new SensorFieldOfViewGraphics();
        sensorFieldOfViewGraphics.setDomeSurfaceMaterial(toCesiumProperty(domeSurfaceMaterial));
        sensorFieldOfViewGraphics.setLateralSurfaceMaterial(toCesiumProperty(lateralSurfaceMaterial));
        sensorFieldOfViewGraphics.setIntersectionColor(toCesiumProperty(WHITE));
        sensorFieldOfViewGraphics.setShowIntersection(toCesiumProperty(true));
        sensorFieldOfViewGraphics.setShowEllipsoidHorizonSurfaces(toCesiumProperty(true));

        m_rotatingSensor.getExtensions().add(new FieldOfViewGraphicsExtension(sensorFieldOfViewGraphics));
    }

    /**
     * Create a fan to visualize an azimuth-elevation mask.
     */
    private void createAzimuthElevationMask() {
        // load terrain data for Mount St. Helens.
        TerrainProvider terrain = new AGIProcessedDataTerrain(new ClasspathStreamFactory("/Data/Terrain/StHelens.pdtt"));

        // Calculate the surface position at the center of the terrain
        double longitude = (terrain.getBoundingExtent().getWestLongitude() + terrain.getBoundingExtent().getEastLongitude()) / 2.0;
        double latitude = (terrain.getBoundingExtent().getNorthLatitude() + terrain.getBoundingExtent().getSouthLatitude()) / 2.0;
        Cartographic observerPosition = new Cartographic(longitude, latitude, terrain.getHeight(longitude, latitude));

        // Sample using 360 azimuth rays at 0.000275 degrees
        final int numberOfAzimuthSteps = 360;
        double stepSize = Trig.degreesToRadians(0.000275);
        final double maxSearchAngle = 0.025;

        // Compute the mask.
        AzimuthElevationMask mask = TerrainAzimuthElevationMask.compute(terrain, observerPosition, numberOfAzimuthSteps, stepSize, maxSearchAngle);

        Point locationPoint = new PointCartographic(m_earth, observerPosition);
        m_maskPlatform = new Platform();
        m_maskPlatform.setName("Azimuth Elevation Mask");
        m_maskPlatform.setLocationPoint(locationPoint);
        m_maskPlatform.setOrientationAxes(new AxesNorthEastDown(m_earth, locationPoint));

        // Set the identifier for the mask in the CZML document. 
        m_maskPlatform.getExtensions().add(new IdentifierExtension("Mask"));

        // Attach the computed mask.
        m_maskPlatform.getExtensions().add(new AzimuthElevationMaskExtension(mask));

        // Define the graphics of the mask.
        StripeMaterialGraphics material = new StripeMaterialGraphics();
        material.setEvenColor(toCesiumProperty(BLUE));
        material.setOddColor(toCesiumProperty(WHITE));
        material.setRepeat(toCesiumProperty(16.0));

        AzimuthElevationMaskGraphics azimuthElevationMaskGraphics = new AzimuthElevationMaskGraphics();
        azimuthElevationMaskGraphics.setProjectionRange(toCesiumProperty(5000.0));
        azimuthElevationMaskGraphics.setMaterial(toCesiumProperty(material));
        azimuthElevationMaskGraphics.setProjection(toConstantCesiumProperty(AzimuthElevationMaskGraphicsProjection.PROJECT_TO_RANGE));
        azimuthElevationMaskGraphics.setNumberOfRings(toCesiumProperty(8));
        azimuthElevationMaskGraphics.setOutline(toCesiumProperty(true));
        azimuthElevationMaskGraphics.setOutlineColor(toCesiumProperty(BLACK));
        azimuthElevationMaskGraphics.setOutlineWidth(toCesiumProperty(2.0));

        m_maskPlatform.getExtensions().add(new AzimuthElevationMaskGraphicsExtension(azimuthElevationMaskGraphics));
    }

    /**
     * Create an aircraft with two sensors.
     * The aircraft will be visually represented by a labeled glTF model.
     */
    private void createAircraft() {
        // Define waypoints for the aircraft's path and use the propagated point as the location point.
        Cartographic point1 = new Cartographic(Trig.degreesToRadians(-122.0), Trig.degreesToRadians(46.3), 4000.0);
        Cartographic point2 = new Cartographic(Trig.degreesToRadians(-122.28), Trig.degreesToRadians(46.25), 4100.0);
        Cartographic point3 = new Cartographic(Trig.degreesToRadians(-122.2), Trig.degreesToRadians(46.1), 6000.0);
        Cartographic point4 = new Cartographic(Trig.degreesToRadians(-121.5), Trig.degreesToRadians(46.0), 7000.0);
        Waypoint waypoint1 = new Waypoint(m_epoch, point1, 20.0, 0.0);
        Waypoint waypoint2 = new Waypoint(waypoint1, m_earth.getShape(), point2, 20.0);
        Waypoint waypoint3 = new Waypoint(waypoint2, m_earth.getShape(), point3, 20.0);
        Waypoint waypoint4 = new Waypoint(waypoint3, m_earth.getShape(), point4, 20.0);
        WaypointPropagator waypointPropagator = new WaypointPropagator(m_earth, Arrays.asList(waypoint1, waypoint2, waypoint3, waypoint4));
        Point locationPoint = waypointPropagator.createPoint();

        m_aircraft = new Platform();
        m_aircraft.setName("Aircraft");
        m_aircraft.setLocationPoint(locationPoint);
        m_aircraft.setOrientationAxes(new AxesVehicleVelocityLocalHorizontal(m_earth.getFixedFrame(), locationPoint));

        // Set the identifier for the aircraft in the CZML document. 
        m_aircraft.getExtensions().add(new IdentifierExtension("Aircraft"));

        // Hermite interpolation works better for aircraft-like vehicles.
        CesiumPositionExtension cesiumPositionExtension = new CesiumPositionExtension();
        cesiumPositionExtension.setInterpolationAlgorithm(CesiumInterpolationAlgorithm.HERMITE);
        m_aircraft.getExtensions().add(cesiumPositionExtension);

        // Configure a glTF model for the aircraft.
        ModelGraphics modelGraphics = new ModelGraphics();

        // Link to a binary glTF file.
        modelGraphics.setModel(toCesiumProperty(new CesiumResource(getModelUri("aircraft.glb"), CesiumResourceBehavior.LINK_TO)));

        // Flip the model visually to point Z in the correct direction.
        NodeTransformationGraphics aircraftNodeTransformation = new NodeTransformationGraphics();
        aircraftNodeTransformation.setRotation(toCesiumProperty(new UnitQuaternion(new ElementaryRotation(AxisIndicator.THIRD, Math.PI))));

        Map<String, NodeTransformationGraphics> nodeTransformations = new HashMap<>();
        nodeTransformations.put("Aircraft", aircraftNodeTransformation);
        modelGraphics.setNodeTransformations(nodeTransformations);

        modelGraphics.setRunAnimations(toCesiumProperty(false));

        m_aircraft.getExtensions().add(new ModelGraphicsExtension(modelGraphics));

        // Show the path of the aircraft.
        PathGraphics pathGraphics = new PathGraphics();
        pathGraphics.setWidth(toCesiumProperty(2.0));
        pathGraphics.setLeadTime(toCesiumProperty(Duration.fromHours(1.0).getTotalSeconds()));
        pathGraphics.setTrailTime(toCesiumProperty(Duration.fromHours(1.0).getTotalSeconds()));

        PolylineOutlineMaterialGraphics pathGraphicsMaterial = new PolylineOutlineMaterialGraphics();
        pathGraphicsMaterial.setColor(toCesiumProperty(WHITE));
        pathGraphicsMaterial.setOutlineColor(toCesiumProperty(BLACK));
        pathGraphicsMaterial.setOutlineWidth(toCesiumProperty(1.0));

        pathGraphics.setMaterial(toCesiumProperty(pathGraphicsMaterial));

        m_aircraft.getExtensions().add(new PathGraphicsExtension(pathGraphics));

        // Configure label for the aircraft.
        LabelGraphics labelGraphics = new LabelGraphics();
        labelGraphics.setText(toCesiumProperty(m_aircraft.getName()));

        // Change label color over time.
        TimeIntervalCollection1<Color> fillColor = new TimeIntervalCollection1<Color>();
        // Green by default...
        fillColor.add(TimeInterval.getInfinite().addData(GREEN));
        // Red between first and second waypoints.
        fillColor.add(new TimeInterval1<Color>(waypoint1.getDate(), waypoint2.getDate(), RED));

        labelGraphics.setFillColor(toCesiumProperty(fillColor));

        // Only show label when camera is far enough from the aircraft,
        // to avoid visually clashing with the model.
        labelGraphics.setDistanceDisplayCondition(toCesiumProperty(new Bounds(1000.0, Double.MAX_VALUE)));

        m_aircraft.getExtensions().add(new LabelGraphicsExtension(labelGraphics));

        // Define a description for the aircraft which will be shown when selected in Cesium.
        m_aircraft.getExtensions().add(new DescriptionExtension(new Description("Aircraft with two offset sensors")));

        // Create 30 degree simple conic sensor definition
        ComplexConic sensorCone = new ComplexConic();
        sensorCone.setHalfAngles(0.0, Trig.degreesToRadians(15));
        sensorCone.setClockAngles(Trig.degreesToRadians(20), Trig.degreesToRadians(50));
        sensorCone.setRadius(Double.POSITIVE_INFINITY);

        // Create a sensor pointing "forward".
        // Position sensor underneath the wing.
        Point sensorOneLocationPoint = new PointFixedOffset(m_aircraft.getReferenceFrame(), new Cartesian(-3.0, 8.0, 0.0));
        Axes sensorAxesOne = new AxesAlignedConstrained(m_aircraft.getOrientationAxes().getVectorElement(CartesianElement.Z), AxisIndicator.THIRD,
                m_aircraft.getOrientationAxes().getVectorElement(CartesianElement.X), AxisIndicator.FIRST);
        // This rotation points the z-axis of the volume back along the x-axis of the ellipsoid.
        UnitQuaternion rotationOne = new UnitQuaternion(new ElementaryRotation(AxisIndicator.SECOND, Constants.HalfPi / 4));

        m_aircraftSensorOne = new Platform();
        m_aircraftSensorOne.setLocationPoint(sensorOneLocationPoint);
        m_aircraftSensorOne.setOrientationAxes(new AxesFixedOffset(sensorAxesOne, rotationOne));

        // Set the identifier for the sensor in the CZML document. 
        m_aircraftSensorOne.getExtensions().add(new IdentifierExtension("AircraftSensorOne"));

        cesiumPositionExtension = new CesiumPositionExtension();
        cesiumPositionExtension.setInterpolationAlgorithm(CesiumInterpolationAlgorithm.HERMITE);
        m_aircraftSensorOne.getExtensions().add(cesiumPositionExtension);

        // Define the sensor geometry.
        m_aircraftSensorOne.getExtensions().add(new FieldOfViewExtension(sensorCone));

        // Configure graphical display of the sensor.
        SensorFieldOfViewGraphics sensorFieldOfViewGraphics = new SensorFieldOfViewGraphics();

        // Configure the outline of the projection onto the earth.
        sensorFieldOfViewGraphics.setEllipsoidSurfaceMaterial(toCesiumProperty(new SolidColorMaterialGraphics(WHITE)));
        sensorFieldOfViewGraphics.setIntersectionWidth(toCesiumProperty(2.0));

        GridMaterialGraphics lateralSurfaceMaterial = new GridMaterialGraphics();
        lateralSurfaceMaterial.setColor(toCesiumProperty(colorWithAlpha(BLUE, 171)));

        sensorFieldOfViewGraphics.setLateralSurfaceMaterial(toCesiumProperty(lateralSurfaceMaterial));

        m_aircraftSensorOne.getExtensions().add(new FieldOfViewGraphicsExtension(sensorFieldOfViewGraphics));

        // Create sensor pointing to the "side".
        // Position sensor underneath the wing.
        Point sensorTwoLocationPoint = new PointFixedOffset(m_aircraft.getReferenceFrame(), new Cartesian(-3.0, -8.0, 0.0));
        Axes sensorAxesTwo = new AxesAlignedConstrained(m_aircraft.getOrientationAxes().getVectorElement(CartesianElement.Z), AxisIndicator.THIRD,
                m_aircraft.getOrientationAxes().getVectorElement(CartesianElement.Y), AxisIndicator.SECOND);

        // This rotation points the z-axis of the volume back along the x-axis of the ellipsoid.
        UnitQuaternion rotationTwo = new UnitQuaternion(new ElementaryRotation(AxisIndicator.FIRST, Constants.HalfPi / 2));

        m_aircraftSensorTwo = new Platform();
        m_aircraftSensorTwo.setLocationPoint(sensorTwoLocationPoint);
        m_aircraftSensorTwo.setOrientationAxes(new AxesFixedOffset(sensorAxesTwo, rotationTwo));

        // Set the identifier for the sensor in the CZML document. 
        m_aircraftSensorTwo.getExtensions().add(new IdentifierExtension("AircraftSensorTwo"));

        cesiumPositionExtension = new CesiumPositionExtension();
        cesiumPositionExtension.setInterpolationAlgorithm(CesiumInterpolationAlgorithm.HERMITE);
        m_aircraftSensorTwo.getExtensions().add(cesiumPositionExtension);

        // Define the sensor geometry.
        m_aircraftSensorTwo.getExtensions().add(new FieldOfViewExtension(sensorCone));

        // Configure graphical display of the sensor.
        sensorFieldOfViewGraphics = new SensorFieldOfViewGraphics();

        // Configure the outline of the projection onto the earth.
        sensorFieldOfViewGraphics.setEllipsoidSurfaceMaterial(toCesiumProperty(new SolidColorMaterialGraphics(WHITE)));
        sensorFieldOfViewGraphics.setIntersectionWidth(toCesiumProperty(2.0));

        lateralSurfaceMaterial = new GridMaterialGraphics();
        lateralSurfaceMaterial.setColor(toCesiumProperty(colorWithAlpha(RED, 171)));

        sensorFieldOfViewGraphics.setLateralSurfaceMaterial(toCesiumProperty(lateralSurfaceMaterial));

        m_aircraftSensorTwo.getExtensions().add(new FieldOfViewGraphicsExtension(sensorFieldOfViewGraphics));

        // Create an access link between the aircraft and the observer position
        // on Mount St. Helens, using the same azimuth elevation mask to constrain access.

        m_aircraftAzimuthElevationMaskLink = new LinkInstantaneous(m_maskPlatform, m_aircraft);

        // Set the identifier for the link in the CZML document. 
        m_aircraftAzimuthElevationMaskLink.getExtensions().add(new IdentifierExtension("AircraftMountStHelensAccess"));

        // Constrain access using the azimuth-elevation mask.
        AccessQuery query = new AzimuthElevationMaskConstraint(m_aircraftAzimuthElevationMaskLink, LinkRole.TRANSMITTER);

        // Configure graphical display of the access link.
        LinkGraphics linkGraphics = new LinkGraphics();

        // Show the access link only when access is satisfied.
        linkGraphics.setShow(new AccessQueryCesiumProperty<>(query, true, false, false));
        linkGraphics.setMaterial(toCesiumProperty(new SolidColorMaterialGraphics(YELLOW)));

        m_aircraftAzimuthElevationMaskLink.getExtensions().add(new LinkGraphicsExtension(linkGraphics));
    }

    /**
     * This method will be called after all the objects above are created.
     */
    public void writeDocument(Writer writer) {
        // Configure the interval over which to generate data.
        // In this case, compute 1 day of data.
        TimeInterval dataInterval = new TimeInterval(m_epoch, m_epoch.addDays(1));

        // Create and configure the CZML document.
        CzmlDocument czmlDocument = new CzmlDocument();

        czmlDocument.setName("CesiumDemo");
        czmlDocument.setDescription("Demonstrates CZML generation using STK Components");
        czmlDocument.setRequestedInterval(dataInterval);

        // For this demonstration, include whitespace in the CZML
        // to enable easy inspection of the contents. In a real application,
        // this would usually be false to reduce file size.
        czmlDocument.setPrettyFormatting(true);

        // Configure the clock on the client to reflect the time for which the data is computed.
        Clock clock = new Clock();
        clock.setInterval(dataInterval);
        clock.setCurrentTime(dataInterval.getStart());
        clock.setMultiplier(15.0);

        czmlDocument.setClock(clock);

        // Add all of our objects with graphical extensions.
        czmlDocument.getObjectsToWrite().add(m_satellite);
        czmlDocument.getObjectsToWrite().add(m_satelliteXAxis);
        czmlDocument.getObjectsToWrite().add(m_satelliteYAxis);
        czmlDocument.getObjectsToWrite().add(m_satelliteZAxis);
        czmlDocument.getObjectsToWrite().add(m_satelliteSunVector);
        czmlDocument.getObjectsToWrite().add(m_facility);
        czmlDocument.getObjectsToWrite().add(m_satelliteFacilityLink);
        czmlDocument.getObjectsToWrite().add(m_facilitySensor);
        czmlDocument.getObjectsToWrite().add(m_sensorDome);
        czmlDocument.getObjectsToWrite().add(m_rotatingSensor);
        czmlDocument.getObjectsToWrite().add(m_maskPlatform);
        czmlDocument.getObjectsToWrite().add(m_aircraft);
        czmlDocument.getObjectsToWrite().add(m_aircraftSensorOne);
        czmlDocument.getObjectsToWrite().add(m_aircraftSensorTwo);
        czmlDocument.getObjectsToWrite().add(m_aircraftAzimuthElevationMaskLink);

        // Write the CZML.
        czmlDocument.writeDocument(writer);
    }

    private URI getModelUri(String modelName) {
        try {
            return new URL(new URL(m_requestUrl), "models/" + modelName).toURI();
        } catch (MalformedURLException | URISyntaxException e) {
            throw new RuntimeException(e);
        }
    }

    private static Color colorWithAlpha(Color color, int alpha) {
        return new Color(color.getRed(), color.getGreen(), color.getBlue(), alpha);
    }

    private static final Color BLACK = Color.BLACK;
    private static final Color WHITE = Color.WHITE;
    private static final Color RED = Color.RED;
    private static final Color GREEN = new Color(0x008000);
    private static final Color BLUE = Color.BLUE;
    private static final Color YELLOW = Color.YELLOW;
    private static final Color PINK = new Color(0xFFC0CB);

    private final String m_satelliteIdentifier;
    private final String m_requestUrl;
    private final EarthCentralBody m_earth;
    private JulianDate m_epoch;
    private Platform m_satellite;
    private GraphicalVector m_satelliteXAxis;
    private GraphicalVector m_satelliteYAxis;
    private GraphicalVector m_satelliteZAxis;
    private GraphicalVector m_satelliteSunVector;
    private Platform m_facility;
    private LinkInstantaneous m_satelliteFacilityLink;
    private AccessQuery m_accessQuery;
    private Platform m_facilitySensor;
    private Platform m_sensorDome;
    private Platform m_rotatingSensor;
    private Platform m_maskPlatform;
    private Platform m_aircraft;
    private Platform m_aircraftSensorOne;
    private Platform m_aircraftSensorTwo;
    private LinkInstantaneous m_aircraftAzimuthElevationMaskLink;

}
