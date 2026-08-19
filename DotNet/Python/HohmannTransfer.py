# Hohmann Transfer using DME Component Libraries and Python-----------------------------

licensePath = input('Enter the complete file path (no quotes) to the DME Component Libraries license: ')
classPath = input('Enter the complete file path (no quotes) to the folder containing the DME Component Libraries DLLs: ')

# Add references and packages
import os, clr, sys

sys.path.append(classPath)
clr.AddReference("AGI.Foundation.Core")
clr.AddReference("AGI.Foundation.Models")
clr.AddReference("AGI.Foundation.OrbitPropagation")
clr.AddReference("AGI.Foundation.SegmentPropagation")
  
from AGI.Foundation import *
from AGI.Foundation.Celestial import *
from AGI.Foundation.Time import *
from AGI.Foundation.Coordinates import *
from AGI.Foundation.Propagators import *
from AGI.Foundation.Geometry import *
from AGI.Foundation.NumericalMethods import *
from AGI.Foundation.SegmentPropagation import *
from AGI.Foundation.StoppingConditions import *

# Read and activate license
with open(licensePath) as f:
    licenseFile = f.read()

Licensing.ActivateLicense(licenseFile)

## Configuration/Creating of Hohmann Transfer ---------------------------------

motionID = "satellite"
fuelMassName = "fuel mass"
dryMassName = "dry mass"

earth = CentralBodiesFacet.GetFromContext().Earth
gravitationalParameter = WorldGeodeticSystem1984.GravitationalParameter
epoch = TimeConstants.J2000.ToTimeStandard(TimeStandard.CoordinatedUniversalTime)

## Set up initial state and satellite: 
ra = 10000.0 * 1000
rp = 7071.0 * 1000
e = (ra - rp) / (ra + rp)
a = (ra + rp) / 2
i = Trig.DegreesToRadians(45)
trueAnomaly = Trig.DegreesToRadians(30)
initialOrbitKeplerianElements = KeplerianElements(a, e, i, 0.0, 0.0, trueAnomaly, gravitationalParameter)
initialOrbit = initialOrbitKeplerianElements.ToCartesian()

# fuel mass
fuelMass = PropagationScalar(900.0)
fuelMass.Identification = fuelMassName
fuelMass.ScalarDerivative = ScalarFixed(0.0)

# dry mass
# Need to pass in a ScalarFixed value below in order to avoid ArgumentNullException when adding values for satellite.mass
dryMass = AuxiliaryStateScalar(ScalarFixed(100.0))
dryMass.Identification = dryMassName

# Create integrator
integrator = RungeKutta4Integrator()

# Create satellite
satellite = PropagationNewtonianPoint(motionID, earth.InertialFrame,
                                      initialOrbit.Value, initialOrbit.FirstDerivative)
earthGravity = TwoBodyGravity(satellite.IntegrationPoint, earth, gravitationalParameter)
satellite.Mass = fuelMass.IntegrationValue + dryMass.AuxiliaryScalar
satellite.AppliedForces.Add(earthGravity)

# Create propagator used for transfer:
propagator = NumericalPropagatorDefinition()
propagator.IntegrationElements.Add(satellite)
propagator.IntegrationElements.Add(fuelMass)
propagator.AuxiliaryElements.Add(dryMass)
propagator.Integrator = integrator
propagator.Integrator.InitialStepSize = 60.0
propagator.Epoch = epoch

# Create initial state of satellite:
initialState = NumericalInitialStateSegment()
initialState.Name = "Initial State Segment"
initialState.PropagatorDefinition = propagator

# Create propagation to perigee segment
propagateToPerigee = NumericalPropagatorSegment()
propagateToPerigee.PropagatorDefinition = propagator
propagateToPerigee.Name = "Propagate to Perigee"

# Create perigee stopping conditions 
trueAnomaly = ScalarModifiedKeplerianElement(gravitationalParameter,satellite.IntegrationPoint, 
                                                    KeplerianElement.TrueAnomaly, earth.InertialFrame)

perigeeStoppingCondition = ScalarStoppingCondition(trueAnomaly, 0.0, 0.0000000001, StopType.AnyThreshold)
perigeeStoppingCondition.AngularSetting = CircularRange.NegativePiToPi
perigeeStoppingCondition.Name = "Initial Orbit Perigee Stopping Condition"
propagateToPerigee.StoppingConditions.Add(perigeeStoppingCondition)

# Create target segment list for transfer orbit
targetSegment = TargetedSegmentList()
targetSegment.Name = "Target List"

# Create first deltaV maneuver (intial orbit --> transfer orbit)

deltaV1 = ImpulsiveManeuverSegment()
deltaV1.Name = "deltaV1"

deltaV1ManeuverInfo = ImpulsiveManeuverInformation(motionID, Cartesian(200.0, 0.0, 0.0), 
                                               fuelMassName, dryMassName,
                                               ScalarFixed(5000.0), InvalidFuelStateBehavior.ThrowException)

deltaV1.Maneuver = deltaV1ManeuverInfo
deltaV1.Maneuver.Orientation = AxesVelocityOrbitNormal(deltaV1.Maneuver.PropagationPoint, earth)

# Transfer orbit
transferOrbit = NumericalPropagatorSegment()
transferOrbit.Name = "Transfer Orbit"
transferOrbit.PropagatorDefinition = propagator

# Create apogee stopping condition for transfer orbit
import math
apogee = ScalarModifiedKeplerianElement(gravitationalParameter, satellite.IntegrationPoint,
                                        KeplerianElement.TrueAnomaly, earth.InertialFrame)

apogeeStoppingCondition = ScalarStoppingCondition(apogee, math.pi, 0.00000001, StopType.AnyThreshold)
apogeeStoppingCondition.AngularSetting = CircularRange.ZeroToTwoPi
apogeeStoppingCondition.Name = "Transfer Orbit Apogee Stopping Condition"

transferOrbit.StoppingConditions.Add(apogeeStoppingCondition)

# Create second deltaV maneuver (transfer orbit --> final orbit)
deltaV2 = ImpulsiveManeuverSegment()

deltaV2ManeuverInfo = ImpulsiveManeuverInformation(motionID,  Cartesian(500.0, 0.0, 0.0), fuelMassName, dryMassName, ScalarFixed(5000.0), InvalidFuelStateBehavior.DoFullDeltaV)
deltaV2.Maneuver = deltaV2ManeuverInfo
deltaV2.Maneuver.Orientation = AxesVelocityOrbitNormal(deltaV2.Maneuver.PropagationPoint, earth)
deltaV2.Name = "deltaV2"

# add burns and transfer orbit to target segment
targetSegment.Segments.Add(deltaV1)
targetSegment.Segments.Add(transferOrbit)
targetSegment.Segments.Add(deltaV2)

# Set up differential corrector
differentialCorrector = TargetedSegmentListDifferentialCorrector()
differentialCorrector.OnlyStoreFinalResults = False
differentialCorrector.Name = "solve for transfer"
differentialCorrector.DifferenceMethod = FiniteDifferenceMethod.Central

def setX(currentValue, configurationToModify):
    configurationToModify.Maneuver.X = configurationToModify.Maneuver.X + currentValue

# Create variable used for configuring deltaV2 to achieve desired final orbit eccentricity
deltaV1VelocityXVariable = deltaV1.CreateVariable(200.0, 0.1, SetVariableCallback[ImpulsiveManeuverSegmentConfiguration](setX))
deltaV1VelocityXVariable.Name = "X Thrust Vector for deltaV1"

# Create radius of apogee constraint for transfer orbit
radiusOfApogee = ScalarModifiedKeplerianElement(gravitationalParameter, satellite.IntegrationPoint,
                                                KeplerianElement.RadiusOfApoapsis, earth.InertialFrame)
radiusApogeePostDv1Constraint = ScalarAtEndOfNumericalSegmentConstraint(radiusOfApogee, transferOrbit, 42000000.0, 0.0001)
radiusApogeePostDv1Constraint.Name = "Radius of Apogee post deltaV1"

# Create variable used for configuring deltaV1 to achieve desired transfer orbit apogee
dv2VelXVar = deltaV2.CreateVariable(200.0, 0.1, SetVariableCallback[ImpulsiveManeuverSegmentConfiguration](setX))
dv2VelXVar.Name = "X Thrust Vector for deltaV2"

# Create eccentricity constraint for final orbit
eccentricity = ScalarModifiedKeplerianElement(gravitationalParameter, satellite.IntegrationPoint, 
                                              KeplerianElement.Eccentricity, earth.InertialFrame)
eccentricityPostDv2Constraint = ScalarAtEndOfNumericalSegmentConstraint(eccentricity, deltaV2, 0.1, 0.000001)
eccentricityPostDv2Constraint.Name = "Eccentricity post deltaV2"

# Add variables and constraints to differential corrector
differentialCorrector.Variables.Add(deltaV1VelocityXVariable)
differentialCorrector.Variables.Add(dv2VelXVar)
differentialCorrector.Constraints.Add(radiusApogeePostDv1Constraint)
differentialCorrector.Constraints.Add(eccentricityPostDv2Constraint)

# Add differential corrector to target segment list
targetSegment.Operators.Add(differentialCorrector)
targetSegment.OperatorAction = TargetedSegmentListOperatorBehavior.RunActiveOperators

# Create final orbit
finalOrbit = NumericalPropagatorSegment()
finalOrbit.Name = "Final Orbit"
finalOrbit.PropagatorDefinition = propagator

# Create duration stopping condition for final orbit (3 hours)
finalOrbit.StoppingConditions.Add(DurationStoppingCondition(Duration.FromHours(3.0)))
finalOrbit.StoppingConditions[0].Name = "Final Orbit Duration Stopping Condition"
finalOrbit.StoppingConditions[0].Enabled = StoppingConditionEnabled.Enabled

# Add all segments to an overall mission sequence list
missionSequenceList = SegmentList()
missionSequenceList.Name = "Mission Sequence List"

missionSequenceList.Segments.Add(initialState)
missionSequenceList.Segments.Add(propagateToPerigee)
missionSequenceList.Segments.Add(targetSegment)
missionSequenceList.Segments.Add(finalOrbit)

# Get the mission propagator and propagate
evaluatorGroup = EvaluatorGroup()
missionPropagator = missionSequenceList.GetSegmentListPropagator(evaluatorGroup)
missionResults = missionPropagator.Propagate()

## Post-processing and Analysis of Results ------------------------------------

# Get satellite's ephemeris and parse it 
ephemeris = missionResults.GetDateMotionCollectionOfOverallTrajectory(motionID, earth.InertialFrame) 
velocity = ephemeris.FirstDerivatives
dates = ephemeris.Dates
location = ephemeris.Values

# Calculate speed, altitude and fuel over course of mission
earthShape = earth.Shape

velocityMagnitude = [(v.Magnitude / 1000) for v in velocity] # [km/s]
timeSinceStart = [dates[0].DaysDifference(d) * TimeConstants.HoursPerDay for d in dates] # [hours]
altitude = [earthShape.CartesianToCartographic(l).Height / 1000 for l in location] # [km]

fuelStateOverMission = missionResults.GetDateMotionCollectionOfOverallTrajectory[float](fuelMassName)
fuel = fuelStateOverMission.Values # [kg]
fuelList = []
for x in fuel:
    fuelList.append(x)

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
ax[1, 0].plot(timeSinceStart, fuelList)
ax[1, 0].set_xlabel('Time [hours]')
ax[1, 0].set_ylabel('Fuel [kg]')
ax[1, 0].set_title('Fuel Use Over Time')

# Creating ground track-------------------------------------------------------

# Get list of results for each segment of propagation:
segmentResults = missionResults.SegmentResults

initialStateResult = segmentResults[0]
propagateToPerigeeResult = segmentResults[1]
transferOrbitResult = segmentResults[2]
finalOrbitResult = segmentResults[3]

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
    ephemeris = segmentResult.GetDateMotionCollectionOfOverallTrajectory(motionID, earth.FixedFrame)
    cartesianLocation = ephemeris.Values
    projection = [ellipsoid.SurfaceProjection(c) for c in cartesianLocation]
    projectionCartographic = [ellipsoid.CartesianToCartographic(p) for p in projection]
    lat = [p.Latitude * Constants.DegreesPerRadian for p in projectionCartographic]
    lon = [p.Longitude * Constants.DegreesPerRadian for p in projectionCartographic]
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
