# Hohmann Transfer using DME Component Libraries and Python-----------------------------

licensePath = input('Enter the complete file path (no quotes) to the DME Component Libraries license: ')
classPath = input('Enter the complete file path (no quotes) to the folder containing the DME Component Libraries JARs: ')

classPath = classPath + "\*"

# Setup JVM and start JVM
import jpype
import jpype.imports

jpype.addClassPath(classPath)
if not jpype.isJVMStarted():
    jpype.startJVM("-ea", convertStrings = False)

import java
import java.lang
import java.util

# Alias base agi package
agi = jpype.JPackage('agi')

# Read and activate license
with open(licensePath) as f:
    licenseFile = f.read()

agi.foundation.Licensing.activateLicense(licenseFile)

## Configuration/Creating of Hohmann Transfer ---------------------------------

# Names and constants used throughout the example:
# Creating aliases for use in Python-
JulianDate = agi.foundation.time.JulianDate
GregorianDate = agi.foundation.time.GregorianDate
TimeStandard = agi.foundation.time.TimeStandard
TimeConstants = agi.foundation.time.TimeConstants
KeplerianElements = agi.foundation.coordinates.KeplerianElements
Cartesian = agi.foundation.coordinates.Cartesian

motionID = "satellite"
fuelMassName = "fuel mass"
dryMassName = "dry mass"

earth = agi.foundation.celestial.CentralBodiesFacet.getFromContext().getEarth()
gravitationalParameter = agi.foundation.celestial.WorldGeodeticSystem1984.GravitationalParameter
epoch = TimeConstants.J2000.toTimeStandard(TimeStandard.getCoordinatedUniversalTime())

## Set up initial state and satellite: 
# Creating aliases for use in Python-
Propagators = agi.foundation.propagators
PropagationScalar = Propagators.PropagationScalar
AuxiliaryStateScalar = Propagators.AuxiliaryStateScalar
PropagationNewtonianPoint = Propagators.PropagationNewtonianPoint
ScalarFixed = agi.foundation.geometry.ScalarFixed
RungeKutta4Integrator = agi.foundation.numericalmethods.RungeKutta4Integrator
Scalar = agi.foundation.geometry.Scalar
TwoBodyGravity = agi.foundation.celestial.TwoBodyGravity
Trig = agi.foundation.Trig

ra = 10000.0 * 1000
rp = 7071.0 * 1000
e = (ra - rp) / (ra + rp)
a = (ra + rp) / 2
i = Trig.degreesToRadians(45)
trueAnomaly = Trig.degreesToRadians(30)
initialOrbitKeplerianElements = KeplerianElements(a, e, i, 0.0, 0.0, trueAnomaly, gravitationalParameter)
initialOrbit = initialOrbitKeplerianElements.toCartesian()

# fuel mass
fuelMass = PropagationScalar(900.0)
fuelMass.setIdentification(fuelMassName)
fuelMass.setScalarDerivative(ScalarFixed(0.0))

# dry mass
dryMass = AuxiliaryStateScalar(Scalar.toScalar(100.0))
dryMass.setIdentification(dryMassName)

# Create integrator
integrator = RungeKutta4Integrator()

# Create satellite
satellite = PropagationNewtonianPoint(motionID, earth.getInertialFrame(),
                                      initialOrbit.getValue(), initialOrbit.getFirstDerivative())
earthGravity = TwoBodyGravity(satellite.getIntegrationPoint(), earth, gravitationalParameter)
satellite.setMass(Scalar.add(fuelMass.getIntegrationValue(), dryMass.getAuxiliaryScalar()))
satellite.getAppliedForces().add(earthGravity)

# Create propagator used for transfer:
# Alias-
NumericalPropagatorDefinition = Propagators.NumericalPropagatorDefinition

propagator = NumericalPropagatorDefinition()
propagator.getIntegrationElements().add(satellite)
propagator.getIntegrationElements().add(fuelMass)
propagator.getAuxiliaryElements().add(dryMass)
propagator.setIntegrator(integrator)
propagator.getIntegrator().setInitialStepSize(60.0)
propagator.setEpoch(epoch)

# Create initial state of satellite:
# Aliases-
NumericalInitialStateSegment = agi.foundation.segmentpropagation.NumericalInitialStateSegment
NumericalPropagatorSegment = agi.foundation.segmentpropagation.NumericalPropagatorSegment

initialState = NumericalInitialStateSegment()
initialState.setName("Initial State Segment")
initialState.setPropagatorDefinition(propagator)

# Create propagation to perigee segment
propagateToPerigee = NumericalPropagatorSegment()
propagateToPerigee.setPropagatorDefinition(propagator)
propagateToPerigee.setName("Propagate to Perigee")

# Create perigee stopping conditions 
# Aliases-
ScalarStoppingCondition = agi.foundation.stoppingconditions.ScalarStoppingCondition
ScalarModifiedKeplerianElement = agi.foundation.geometry.ScalarModifiedKeplerianElement
KeplerianElement = agi.foundation.coordinates.KeplerianElement
CircularRange = agi.foundation.stoppingconditions.CircularRange
StopType = agi.foundation.stoppingconditions.StopType

trueAnomaly = ScalarModifiedKeplerianElement(gravitationalParameter,satellite.getIntegrationPoint(), 
                                                    KeplerianElement.TRUE_ANOMALY, earth.getInertialFrame())

perigeeStoppingCondition = ScalarStoppingCondition(trueAnomaly, 0.0, 0.0000000001, StopType.ANY_THRESHOLD)
perigeeStoppingCondition.setAngularSetting(CircularRange.NEGATIVE_PI_TO_PI)
perigeeStoppingCondition.setName("Initial Orbit Perigee Stopping Condition")
propagateToPerigee.getStoppingConditions().add(perigeeStoppingCondition)

# Create target segment list for transfer orbit
# Alias-
TargetedSegmentList = agi.foundation.segmentpropagation.TargetedSegmentList

targetSegment = TargetedSegmentList()
targetSegment.setName("Target List")

# Create first deltaV maneuver (intial orbit --> transfer orbit)
# Aliases-
ImpulsiveManeuverSegment = agi.foundation.segmentpropagation.ImpulsiveManeuverSegment
ImpulsiveManeuverInformation = agi.foundation.segmentpropagation.ImpulsiveManeuverInformation
InvalidFuelStateBehavior = agi.foundation.segmentpropagation.InvalidFuelStateBehavior
AxesVelocityOrbitNormal = agi.foundation.geometry.AxesVelocityOrbitNormal

deltaV1 = ImpulsiveManeuverSegment()
deltaV1.setName("deltaV1")

deltaV1ManeuverInfo = ImpulsiveManeuverInformation(motionID, Cartesian(200.0, 0.0, 0.0), 
                                                   fuelMassName, dryMassName,
                                                   Scalar.toScalar(5000.0), InvalidFuelStateBehavior.THROW_EXCEPTION)
deltaV1.setManeuver(deltaV1ManeuverInfo)
deltaV1.getManeuver().setOrientation(AxesVelocityOrbitNormal(deltaV1.getManeuver().getPropagationPoint(), earth))

# Transfer orbit
transferOrbit = NumericalPropagatorSegment()
transferOrbit.setName("Transfer Orbit")
transferOrbit.setPropagatorDefinition(propagator)

# Create apogee stopping condition for transfer orbit
import math
apogee = ScalarModifiedKeplerianElement(gravitationalParameter, satellite.getIntegrationPoint(),
                                        KeplerianElement.TRUE_ANOMALY, earth.getInertialFrame())

apogeeStoppingCondition = ScalarStoppingCondition(apogee, math.pi, 0.00000001, StopType.ANY_THRESHOLD)
apogeeStoppingCondition.setAngularSetting(CircularRange.ZERO_TO_TWO_PI)
apogeeStoppingCondition.setName("Transfer Orbit Apogee Stopping Condition")

transferOrbit.getStoppingConditions().add(apogeeStoppingCondition)

# Create second deltaV maneuver (transfer orbit --> final orbit)
deltaV2 = ImpulsiveManeuverSegment()
deltaV2ManeuverInfo = ImpulsiveManeuverInformation(java.lang.String(motionID), Cartesian(500.0,0.0,0.0), 
                                                   java.lang.String(fuelMassName), java.lang.String(dryMassName), 
                                                   Scalar.toScalar(5000.0), InvalidFuelStateBehavior.DO_FULL_DELTA_V)
deltaV2.setManeuver(deltaV2ManeuverInfo)
deltaV2.getManeuver().setOrientation(AxesVelocityOrbitNormal(deltaV2.getManeuver().getPropagationPoint(), earth))
deltaV2.setName("deltaV2")

# add burns and transfer orbit to target segment
targetSegment.getSegments().add(deltaV1)
targetSegment.getSegments().add(transferOrbit)
targetSegment.getSegments().add(deltaV2)

# Set up differential corrector
# Aliases-
TargetedSegmentListDifferentialCorrector = agi.foundation.numericalmethods.TargetedSegmentListDifferentialCorrector
FiniteDifferenceMethod = agi.foundation.numericalmethods.FiniteDifferenceMethod

differentialCorrector = TargetedSegmentListDifferentialCorrector()
differentialCorrector.setOnlyStoreFinalResults(False)
differentialCorrector.setName("solve for transfer")
differentialCorrector.setDifferenceMethod(FiniteDifferenceMethod.CENTRAL)

# Create implementation of SetVariableCallback Function's interface
# Aliases-
DelegateBasedVariable = agi.foundation.numericalmethods.DelegateBasedVariable
SetVariableCallback = agi.foundation.numericalmethods.SetVariableCallback


@jpype.JImplements(SetVariableCallback.Function)
class SVCFunctionImplementation:
    @jpype.JOverride
    def invoke(self, currentValue, configurationToModify):
        configurationToModify.getManeuver().setX(configurationToModify.getManeuver().getX() + currentValue)


# Create variable used for configuring deltaV1 to achieve desired transfer orbit apogee
# Aliases-
ImpulsiveManeuverSegmentConfiguration = agi.foundation.segmentpropagation.ImpulsiveManeuverSegmentConfiguration
ImpulsiveManeuverInformation = agi.foundation.segmentpropagation.ImpulsiveManeuverInformation

svc1 = SetVariableCallback.of(SVCFunctionImplementation())

deltaV1VelocityXVariable = deltaV1.createVariable(jpype.JDouble(200.0), jpype.JDouble(0.1), svc1)
deltaV1VelocityXVariable.setName("X Thrust Vector for deltaV1")

# Create radius of apogee constraint for transfer orbit
# Alias-
ScalarAtEndOfNumericalSegmentConstraint = agi.foundation.numericalmethods.ScalarAtEndOfNumericalSegmentConstraint

radiusOfApogee = ScalarModifiedKeplerianElement(gravitationalParameter, satellite.getIntegrationPoint(),
                                                KeplerianElement.RADIUS_OF_APOAPSIS, earth.getInertialFrame())
radiusApogeePostDv1Constraint = ScalarAtEndOfNumericalSegmentConstraint(radiusOfApogee, transferOrbit, 42000000.0, 0.0001)
radiusApogeePostDv1Constraint.setName("Radius of Apogee post deltaV1")

# Create variable used for configuring deltaV2 to achieve desired final orbit eccentricity
svc2 = SetVariableCallback.of(SVCFunctionImplementation())

dv2VelXVar = deltaV2.createVariable(jpype.JDouble(200.0), jpype.JDouble(0.1), svc2)
dv2VelXVar.setName("X Thrust Vector for deltaV2")

# Create eccentricity constraint for final orbit
eccentricity = ScalarModifiedKeplerianElement(gravitationalParameter, satellite.getIntegrationPoint(), 
                                              KeplerianElement.ECCENTRICITY, earth.getInertialFrame())
eccentricityPostDv2Constraint = ScalarAtEndOfNumericalSegmentConstraint(eccentricity, deltaV2, 0.1, 0.000001)
eccentricityPostDv2Constraint.setName("Eccentricity post deltaV2")

# Add variables and constraints to differential corrector
differentialCorrector.getVariables().add(deltaV1VelocityXVariable)
differentialCorrector.getVariables().add(dv2VelXVar)
differentialCorrector.getConstraints().add(radiusApogeePostDv1Constraint)
differentialCorrector.getConstraints().add(eccentricityPostDv2Constraint)

# Add differential corrector to target segment list
targetSegment.getOperators().add(differentialCorrector)
targetSegment.setOperatorAction(agi.foundation.segmentpropagation.TargetedSegmentListOperatorBehavior.RUN_ACTIVE_OPERATORS)

# Create final orbit
finalOrbit = NumericalPropagatorSegment()
finalOrbit.setName("Final Orbit")
finalOrbit.setPropagatorDefinition(propagator)

# Create duration stopping condition for final orbit (3 hours)
# Aliases-
DurationStoppingCondition = agi.foundation.stoppingconditions.DurationStoppingCondition
Duration = agi.foundation.time.Duration

finalOrbit.getStoppingConditions().add(DurationStoppingCondition(Duration.fromHours(3.0)))
finalOrbit.getStoppingConditions().get(0).setName("Final Orbit Duration Stopping Condition")
finalOrbit.getStoppingConditions().get(0).setEnabled(agi.foundation.stoppingconditions.StoppingConditionEnabled.ENABLED)

# Add all segments to an overall mission sequence list
# Alias-
SegmentList = agi.foundation.segmentpropagation.SegmentList

missionSequenceList = SegmentList()
missionSequenceList.setName("Mission Sequence List")

missionSequenceList.getSegments().add(initialState)
missionSequenceList.getSegments().add(propagateToPerigee)
missionSequenceList.getSegments().add(targetSegment)
missionSequenceList.getSegments().add(finalOrbit)

# Get the mission propagator and propagate
# Alias-
EvaluatorGroup = agi.foundation.EvaluatorGroup

missionPropagator = missionSequenceList.getSegmentPropagator(EvaluatorGroup())
missionResults = missionPropagator.propagate()

## Post-processing and Analysis of Results ------------------------------------

# Get satellite's ephemeris and parse it 
ephemeris = missionResults.getDateMotionCollectionOfOverallTrajectory(motionID, earth.getInertialFrame()) 
velocity = ephemeris.getFirstDerivatives() 
dates = ephemeris.getDates()
location = ephemeris.getValues() 

# Calculate speed, altitude and fuel over course of mission
# Alias-
TimeConstants = agi.foundation.time.TimeConstants

earthShape = earth.getShape()

velocityMagnitude = [(v.getMagnitude() / 1000) for v in velocity] # [km/s]
timeSinceStart = [dates.get(0).daysDifference(d) * TimeConstants.HoursPerDay for d in dates] # [hours]
altitude = [earthShape.cartesianToCartographic(l).getHeight() / 1000 for l in location] # [km]

fuelStateOverMission = missionResults.getDateMotionCollectionOfOverallTrajectory(fuelMassName)
fuel = fuelStateOverMission.getValues() # [kg]

# Plots:
import matplotlib
import matplotlib.pyplot as plt
from matplotlib import animation
import numpy as np
import os

# Uncomment the following 2 lines if you are running this in an IDE with
# iPython support (Spyder, PyCharm, Python Tools for Visual Studio, Jupyter Notebook)
#from IPython import get_ipython
#get_ipython().run_line_magic('matplotlib', 'qt5')

# Velocity vs Time 
fig, ax = plt.subplots(2, 2)

ax[0, 0].plot(timeSinceStart, velocityMagnitude)
ax[0, 0].set_xlabel('Time [hours]')
ax[0, 0].set_ylabel('Velocity [km/s]')
ax[0, 0].set_title('Velocity Over Time')

# Altitude vs Time
ax[0, 1].plot(timeSinceStart, altitude)
ax[0, 1].set_xlabel('Time [hours]')
ax[0, 1].set_ylabel('Altitude [km]')
ax[0, 1].set_title('Altitude Over Time')

# Fuel vs Time 
ax[1, 0].plot(timeSinceStart, fuel)
ax[1, 0].set_xlabel('Time [hours]')
ax[1, 0].set_ylabel('Fuel [kg]')
ax[1, 0].set_title('Fuel Use Over Time')

# Creating ground track-------------------------------------------------------

# Get list of results for each segment of propagation:
# Alias-
Constants = agi.foundation.Constants
segmentResults = missionResults.getSegmentResults()

initialStateResult = segmentResults.get(0)
propagateToPerigeeResult = segmentResults.get(1)
transferOrbitResult = segmentResults.get(2)
finalOrbitResult = segmentResults.get(3)

def ProjectionLatLon(ellipsoid, segmentResult):
    """

    Parameters
    ----------
    ellipsoid : Ellipsoid
        the shape of the Earth
    segmentResult : PropagateSegmentResults
        the results to project

    Returns
    -------
    float, float
        latitude and longitude as numpy.array with units of degrees

    """
    ephemeris = segmentResult.getDateMotionCollectionOfOverallTrajectory(motionID, earth.getFixedFrame())
    cartesianLocation = ephemeris.getValues()
    projection = [ellipsoid.surfaceProjection(c) for c in cartesianLocation]
    projectionCartographic = [ellipsoid.cartesianToCartographic(p) for p in projection]
    lat = [p.getLatitude() * Constants.DegreesPerRadian for p in projectionCartographic]
    lon = [p.getLongitude() * Constants.DegreesPerRadian for p in projectionCartographic]
    return np.array(lat), np.array(lon)

# Parse latitude/longitude for intial orbit, propogation to perigee, transfer orbit and final orbit
initialOrbitLat, initialOrbitLon = ProjectionLatLon(earthShape, initialStateResult)
propagateToPerigeeLat, propagateToPerigeeLon = ProjectionLatLon(earthShape, propagateToPerigeeResult)
transferOrbitLat, transferOrbitLon = ProjectionLatLon(earthShape, transferOrbitResult)
finalOrbitLat, finalOrbitLon = ProjectionLatLon(earthShape, finalOrbitResult)

# Plot results on scatter plot
ax[1, 1].set_xlim(-180, 180)
ax[1, 1].set_ylim(-90, 90)
ax[1, 1].set_ylabel('Latitude [degrees]')
ax[1, 1].set_xlabel('Longitude [degrees]')
ax[1, 1].set_title('Ground Track')
fig.tight_layout(pad=1.0)

#fig,ax = plt.subplots()
ax[1, 1].scatter(initialOrbitLon, initialOrbitLat, c='blue', label='Initial')
ax[1, 1].scatter(propagateToPerigeeLon, propagateToPerigeeLat, c='coral',label='To Perigee') 
ax[1, 1].scatter(transferOrbitLon, transferOrbitLat, c='purple', label='Transfer')
ax[1, 1].scatter(finalOrbitLon, finalOrbitLat, c='green', label='Final')

ax[1, 1].legend(loc='lower left')

# Add background image of Earth:
imagePath = os.path.join(os.getcwd(), "EarthMap.png")
img = plt.imread(imagePath)

ax[1, 1].imshow(img, extent=[-180, 180, -90, 90])

## Animated Ground Track---------------------------------------------------------

# Setup:
fig,ax = plt.subplots()
ax.set_xlim(-180, 180)
ax.set_ylim(-90, 90)
ax.imshow(img, extent=[-180, 180, -90, 90])
ax.title.set_text('Animated Ground Track')
ax.set_xlabel('Longitude [degrees]')
ax.set_ylabel('Latitude [degrees]')

scat = ax.scatter([],[])

# Create legend
from matplotlib.lines import Line2D
legendElements = [Line2D([0], [0], marker='o', color='blue', label='Initial'),
                  Line2D([0], [0], marker='o', color='coral', label='To Perigee'),
                  Line2D([0], [0], marker='o', color='purple', label='Transfer'),
                  Line2D([0], [0], marker='o', color='green', label='Final')]

ax.legend(handles=legendElements, loc='lower left')

# Combine all segments into one array
longitude = np.concatenate([propagateToPerigeeLon, transferOrbitLon, finalOrbitLon])
latitude = np.concatenate([propagateToPerigeeLat, transferOrbitLat, finalOrbitLat])

# Array for colors to be associated for each segment:
segmentColors = np.array(['blue','coral','purple','green'])

# Array to hold how many points are in each section
lengths = np.zeros(4)
lengths[0] = len(initialOrbitLat) - 1 #for indexing 
lengths[1] = lengths[0] + len(propagateToPerigeeLon)
lengths[2] = lengths[1] + len(transferOrbitLon)
lengths[3] = lengths[2] + len(finalOrbitLon)

# Current segment 
current = 0

# Initializing scatter plot with initial state values:
def init():
    data = np.hstack((initialOrbitLon[:,np.newaxis],initialOrbitLat[:,np.newaxis]))
    scat.set_offsets(data)
    scat.set_facecolors(np.array(matplotlib.colors.to_rgb(segmentColors[current])))
    
    return (scat,)

# Function for animating plot
def animate(i):
    global current
    
    # At each time step (i), add next point to plot
    data = np.hstack((longitude[:i,np.newaxis], latitude[:i, np.newaxis]))
    scat.set_offsets(data)
   
    # If reach end of current segment, increment to next segment
    if i == lengths[current]:
        current = current +1

    # Get colors then add color of current segment
    colors = scat.get_facecolors()  
    newcolors = np.concatenate([colors, np.array(matplotlib.colors.to_rgba(segmentColors[current]), ndmin=2)])
    scat.set_facecolors(newcolors)
    
    return (scat,)

# Animate scatter plot
anim = animation.FuncAnimation(fig, animate, init_func=init, frames=len(latitude) , interval=20, blit=True, repeat=False)

plt.show()
# Uncomment the following lines to save the animated ground track as a gif:
#from matplotlib.animation import PillowWriter
#anim.save('groundtrack.gif',dpi=80, writer=PillowWriter(fps=20))
