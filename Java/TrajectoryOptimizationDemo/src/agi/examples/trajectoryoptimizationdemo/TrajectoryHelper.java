package agi.examples.trajectoryoptimizationdemo;

import java.util.ArrayList;

import agi.foundation.Constants;
import agi.foundation.Motion1;
import agi.foundation.Trig;
import agi.foundation.celestial.TwoBodyGravity;
import agi.foundation.celestial.WorldGeodeticSystem1984;
import agi.foundation.coordinates.Cartesian;
import agi.foundation.coordinates.KeplerianElement;
import agi.foundation.coordinates.ModifiedKeplerianElements;
import agi.foundation.geometry.AxesVelocityOrbitNormal;
import agi.foundation.geometry.ParameterizedOnStateScalar;
import agi.foundation.geometry.Scalar;
import agi.foundation.geometry.ScalarFixed;
import agi.foundation.geometry.ScalarModifiedKeplerianElement;
import agi.foundation.infrastructure.ValueDefinition;
import agi.foundation.numericalmethods.ActiveSetSequentialQuadraticProgrammingOptimizer;
import agi.foundation.numericalmethods.CombinedCostFunction;
import agi.foundation.numericalmethods.CostFunctionGoal;
import agi.foundation.numericalmethods.DelegateBasedConstraint;
import agi.foundation.numericalmethods.DelegateBasedConstraintCallback;
import agi.foundation.numericalmethods.DelegateBasedInequalityConstraint;
import agi.foundation.numericalmethods.DelegateBasedInequalityConstraintCallback;
import agi.foundation.numericalmethods.DelegateBasedVariable;
import agi.foundation.numericalmethods.InequalityBoundType;
import agi.foundation.numericalmethods.NoScalingOnCostFunction;
import agi.foundation.numericalmethods.ParameterizedDoubleVariable;
import agi.foundation.numericalmethods.RungeKuttaVerner89Integrator;
import agi.foundation.numericalmethods.ScalarConstraintDifference;
import agi.foundation.numericalmethods.ScalarDifferenceOfSegmentCostFunction;
import agi.foundation.numericalmethods.SegmentPropagatorCostFunction;
import agi.foundation.numericalmethods.SetVariableCallback;
import agi.foundation.numericalmethods.TargetedSegmentListDifferentialCorrector;
import agi.foundation.numericalmethods.TargetedSegmentListParameterOptimizer;
import agi.foundation.numericalmethods.advanced.ConvergenceCriteria;
import agi.foundation.numericalmethods.advanced.LineSearchSettings;
import agi.foundation.propagators.NumericalPropagatorDefinition;
import agi.foundation.propagators.PropagationNewtonianPoint;
import agi.foundation.propagators.PropagationScalar;
import agi.foundation.segmentpropagation.ImpulsiveManeuverInformation;
import agi.foundation.segmentpropagation.ImpulsiveManeuverSegment;
import agi.foundation.segmentpropagation.ImpulsiveManeuverSegmentConfiguration;
import agi.foundation.segmentpropagation.InvalidFuelStateBehavior;
import agi.foundation.segmentpropagation.NumericalInitialStateSegment;
import agi.foundation.segmentpropagation.NumericalPropagatorSegment;
import agi.foundation.segmentpropagation.TargetedSegmentList;
import agi.foundation.segmentpropagation.TargetedSegmentListOperatorBehavior;
import agi.foundation.stoppingconditions.CircularRange;
import agi.foundation.stoppingconditions.DurationStoppingCondition;
import agi.foundation.stoppingconditions.ScalarStoppingCondition;
import agi.foundation.stoppingconditions.StopType;
import agi.foundation.time.Duration;
import agi.foundation.time.JulianDate;
import agi.foundation.time.TimeConstants;
import agi.foundation.celestial.EarthCentralBody;
import agi.foundation.celestial.CentralBodiesFacet;

public final class TrajectoryHelper {

	public static TargetedSegmentList createTrajectoryOptimizer(JulianDate epoch) {
	    // #region Create Trajectory Optimizer
		final String motionID = "Satellite";
        final String fuelMassName = "Fuel_Mass";
        final String dryMassName = "Dry_Mass";
        double earthGravitationalParameter = WorldGeodeticSystem1984.GravitationalParameter;
        EarthCentralBody earth = CentralBodiesFacet.getFromContext().getEarth();
        
        // Initial state has periapsis 300 km above Earth's surface and apoapsis at approximately GEO radius.
        final double initialRadiusPeriapsis = 6678000.0; // meters.
        double initialRadiusApoapsis = Math.pow(earthGravitationalParameter * Math.pow(TimeConstants.SecondsPerDay / Constants.TwoPi, 2.0), 1.0 / 3.0); // meters.
        double initialSemimajorAxis = 0.5 * (initialRadiusPeriapsis + initialRadiusApoapsis);
        double initialInclination = Trig.degreesToRadians(57.0);
        Motion1<Cartesian> initialState = new ModifiedKeplerianElements(initialRadiusPeriapsis, 1.0 / initialSemimajorAxis, initialInclination, 0.0, 0.0, 0.0, earthGravitationalParameter)
                .toCartesian();
        
        // Specific impulse for impulsive maneuvers.
        final double isp = 300.0; // seconds
        double exhaustVelocity = isp * Constants.EarthSurfaceGravity;
        
        // Fuel mass.
        PropagationScalar fuelMass = new PropagationScalar(1000.0);
        fuelMass.setIdentification(fuelMassName);
        fuelMass.setScalarDerivative(new ScalarFixed(0.0));
        
        // Dry mass.
        PropagationScalar dryMass = new PropagationScalar(500.0);
        dryMass.setIdentification(dryMassName);
        dryMass.setScalarDerivative(new ScalarFixed(0.0));
        
        // Create the integrator.
        RungeKuttaVerner89Integrator integrator = new RungeKuttaVerner89Integrator();
        integrator.setInitialStepSize(60.0);
        
        // The propagation point and propagator.
        PropagationNewtonianPoint propagationPoint = new PropagationNewtonianPoint(motionID, earth.getInertialFrame(), initialState.getValue(), initialState.getFirstDerivative());
        propagationPoint.setMass(Scalar.add(fuelMass.getIntegrationValue(), dryMass.getIntegrationValue()));
        TwoBodyGravity earthGravity = new TwoBodyGravity(propagationPoint.getIntegrationPoint(), earth, earthGravitationalParameter);
        propagationPoint.getAppliedForces().add(earthGravity);
        
        NumericalPropagatorDefinition numericalPropagator = new NumericalPropagatorDefinition();
        numericalPropagator.getIntegrationElements().add(propagationPoint);
        numericalPropagator.getIntegrationElements().add(fuelMass);
        numericalPropagator.getIntegrationElements().add(dryMass);
        numericalPropagator.setIntegrator(integrator);
        numericalPropagator.setEpoch(epoch);
        
        // Define initial state segment.
        NumericalInitialStateSegment initialStateSegment = new NumericalInitialStateSegment();
        initialStateSegment.setName("Initial_State_Segment");
        initialStateSegment.setPropagatorDefinition(numericalPropagator);
        
        // Define coasting segment that stops at first apogee.
        NumericalPropagatorSegment coastToManeuver1 = new NumericalPropagatorSegment();
        coastToManeuver1.setName("Coast_To_Maneuver1_Start");
        coastToManeuver1.setPropagatorDefinition(numericalPropagator);
        
        // 180 degrees in radians.
        final double apogeeTrueAnomaly = Math.PI;
        ScalarStoppingCondition apogee1StoppingCondition = new ScalarStoppingCondition();
        apogee1StoppingCondition.setThreshold(ValueDefinition.<Double> toValueDefinition((double) apogeeTrueAnomaly));
        apogee1StoppingCondition.setFunctionTolerance(Constants.Epsilon6);
        apogee1StoppingCondition.setAngularSetting(CircularRange.ZERO_TO_TWO_PI);
        apogee1StoppingCondition.setTypeOfStoppingCondition(StopType.THRESHOLD_INCREASING);
        
        apogee1StoppingCondition
                .setScalar(new ScalarModifiedKeplerianElement(earthGravitationalParameter, propagationPoint.getIntegrationPoint(), KeplerianElement.TRUE_ANOMALY, earth.getInertialFrame()));
        coastToManeuver1.getStoppingConditions().add(apogee1StoppingCondition);
        
        // Define impulsive maneuver segment for first maneuver.
        ImpulsiveManeuverSegment maneuverSegment1 = new ImpulsiveManeuverSegment();
        ImpulsiveManeuverInformation maneuver1 = new ImpulsiveManeuverInformation(motionID, Cartesian.getZero(), fuelMassName, dryMassName, Scalar.toScalar(exhaustVelocity),
                InvalidFuelStateBehavior.DO_AS_MUCH_AS_POSSIBLE);
        maneuver1.setOrientation(new AxesVelocityOrbitNormal(maneuver1.getPropagationPoint(), earth));
        maneuverSegment1.setManeuver(maneuver1);
        
        // Set up variable for delta-v in the velocity direction.
        DelegateBasedVariable<ImpulsiveManeuverSegmentConfiguration> deltaV1XVariable = maneuverSegment1.createVariable(100.0, 1.0,
                SetVariableCallback.of((deltaVx, configuration) -> {
                    configuration.getManeuver().setX(configuration.getManeuver().getX() + deltaVx);// m/s.
                }));
        
        // Set up variable for delta-v in the normal direction.
        DelegateBasedVariable<ImpulsiveManeuverSegmentConfiguration> deltaV1YVariable = maneuverSegment1.createVariable(100.0, 1.0,
                SetVariableCallback.of((deltaVy, configuration) -> {
                    configuration.getManeuver().setY(configuration.getManeuver().getY() + deltaVy);// m/s.
                }));
        
        // Set up a variable for the post-maneuver radius of perigee.
        final double postManeuver1RadiusPeriapsisGuess = initialRadiusPeriapsis + 1000000.0;
        ParameterizedDoubleVariable postManeuver1RadiusPerigeeVariable = new ParameterizedDoubleVariable(postManeuver1RadiusPeriapsisGuess, 1000000.0, 100000.0, maneuverSegment1);
        postManeuver1RadiusPerigeeVariable.getSettings().setVariableTolerance(Constants.Epsilon1);
        
        // Set up a variable for the post-maneuver inclination.
        double postManeuver1InclinationGuess = Trig.degreesToRadians(45.0);
        ParameterizedDoubleVariable postManeuver1InclinationVariable = new ParameterizedDoubleVariable(postManeuver1InclinationGuess, Trig.degreesToRadians(5.0), Trig.degreesToRadians(1.0),
                maneuverSegment1);
        postManeuver1InclinationVariable.getSettings().setVariableTolerance(Constants.Epsilon1);
        
        // Set up a constraint for the post-maneuver radius of perigee.
        DelegateBasedConstraint postManeuver1RadiusPerigeeEquality = new DelegateBasedConstraint(DelegateBasedConstraintCallback.of((results) -> {
                Motion1<Cartesian> motion = results.getStateForNextSegment().<Cartesian> getMotion(motionID);
                return new ModifiedKeplerianElements(motion, WorldGeodeticSystem1984.GravitationalParameter).getRadiusOfPeriapsis();
            }), maneuverSegment1, Double.NaN, Constants.Epsilon3);
        postManeuver1RadiusPerigeeEquality.setDesiredValue(postManeuver1RadiusPerigeeVariable.getValue());
        
        // Set up a constraint for the post-maneuver inclination.
        DelegateBasedConstraint postManeuver1InclinationEquality = new DelegateBasedConstraint(DelegateBasedConstraintCallback.of((results) -> {
                Motion1<Cartesian> motion = results.getStateForNextSegment().<Cartesian> getMotion(motionID);
                return new ModifiedKeplerianElements(motion, WorldGeodeticSystem1984.GravitationalParameter).getInclination();
            }), maneuverSegment1, Double.NaN, Constants.Epsilon3);
        postManeuver1InclinationEquality.setDesiredValue(postManeuver1InclinationVariable.getValue());
        
        // Define coasting segment that stops at second perigee.
        NumericalPropagatorSegment coastToPerigee2 = new NumericalPropagatorSegment();
        coastToPerigee2.setName("Coast_To_Perigee2");
        coastToPerigee2.setPropagatorDefinition(numericalPropagator);
        
        ScalarStoppingCondition perigee2StoppingCondition = new ScalarStoppingCondition();
        perigee2StoppingCondition.setThreshold(ValueDefinition.<Double> toValueDefinition((double) 0.0));
        perigee2StoppingCondition.setFunctionTolerance(Constants.Epsilon6);
        perigee2StoppingCondition.setAngularSetting(CircularRange.NEGATIVE_PI_TO_PI);
        perigee2StoppingCondition.setTypeOfStoppingCondition(StopType.THRESHOLD_INCREASING);
        perigee2StoppingCondition
                .setScalar(new ScalarModifiedKeplerianElement(earthGravitationalParameter, propagationPoint.getIntegrationPoint(), KeplerianElement.TRUE_ANOMALY, earth.getInertialFrame()));
        coastToPerigee2.getStoppingConditions().add(perigee2StoppingCondition);
        
        // Define coasting segment that stops at second apogee.
        NumericalPropagatorSegment coastToManeuver2 = new NumericalPropagatorSegment();
        coastToManeuver2.setName("Coast_To_Maneuver2_Start");
        coastToManeuver2.setPropagatorDefinition(numericalPropagator);
        
        ScalarStoppingCondition apogee2StoppingCondition = new ScalarStoppingCondition();
        apogee2StoppingCondition.setThreshold(ValueDefinition.<Double> toValueDefinition((double) apogeeTrueAnomaly));
        apogee2StoppingCondition.setFunctionTolerance(Constants.Epsilon6);
        apogee2StoppingCondition.setAngularSetting(CircularRange.ZERO_TO_TWO_PI);
        apogee2StoppingCondition.setTypeOfStoppingCondition(StopType.THRESHOLD_INCREASING);
        apogee2StoppingCondition
                .setScalar(new ScalarModifiedKeplerianElement(earthGravitationalParameter, propagationPoint.getIntegrationPoint(), KeplerianElement.TRUE_ANOMALY, earth.getInertialFrame()));
        coastToManeuver2.getStoppingConditions().add(apogee2StoppingCondition);
        
        // Define impulsive maneuver segment for second maneuver.
        ImpulsiveManeuverSegment maneuverSegment2 = new ImpulsiveManeuverSegment();
        ImpulsiveManeuverInformation maneuver2 = new ImpulsiveManeuverInformation(motionID, Cartesian.getZero(), fuelMassName, dryMassName, Scalar.toScalar(exhaustVelocity),
                InvalidFuelStateBehavior.DO_AS_MUCH_AS_POSSIBLE);
        maneuver2.setOrientation(new AxesVelocityOrbitNormal(maneuver2.getPropagationPoint(), earth));
        maneuverSegment2.setManeuver(maneuver2);
        
        // Set up variable for delta-v in the velocity direction.
        DelegateBasedVariable<ImpulsiveManeuverSegmentConfiguration> deltaV2XVariable = maneuverSegment2.createVariable(100.0, 1.0,
                SetVariableCallback.of((deltaVx, configuration) -> {
                    configuration.getManeuver().setX(configuration.getManeuver().getX() + deltaVx);// m/s.
                }));
        
        // Set up variable for delta-v in the normal direction.
        DelegateBasedVariable<ImpulsiveManeuverSegmentConfiguration> deltaV2YVariable = maneuverSegment2.createVariable(100.0, 1.0,
                SetVariableCallback.of((deltaVy, configuration) -> {
                    configuration.getManeuver().setY(configuration.getManeuver().getY() + deltaVy);// m/s.
                }));
        
        // Set up a variable for the post-maneuver radius of perigee.
        final double postManeuver2RadiusPeriapsisGuess = initialRadiusPeriapsis + 10000000.0;
        ParameterizedDoubleVariable postManeuver2RadiusPerigeeVariable = new ParameterizedDoubleVariable(postManeuver2RadiusPeriapsisGuess, 1000000.0, 100000.0, maneuverSegment1);
        postManeuver2RadiusPerigeeVariable.getSettings().setVariableTolerance(Constants.Epsilon1);
        
        // Set up a variable for the post-maneuver inclination.
        double postManeuver2InclinationGuess = Trig.degreesToRadians(30.0);
        ParameterizedDoubleVariable postManeuver2InclinationVariable = new ParameterizedDoubleVariable(postManeuver2InclinationGuess, Trig.degreesToRadians(5.0), Trig.degreesToRadians(1.0),
                maneuverSegment1);
        postManeuver2InclinationVariable.getSettings().setVariableTolerance(Constants.Epsilon1);
        
        // Set up a constraint for the post-maneuver radius of perigee.
        DelegateBasedConstraint postManeuver2RadiusPerigeeEquality = new DelegateBasedConstraint(DelegateBasedConstraintCallback.of((results) -> {
                Motion1<Cartesian> motion = results.getStateForNextSegment().<Cartesian> getMotion(motionID);
                return new ModifiedKeplerianElements(motion, WorldGeodeticSystem1984.GravitationalParameter).getRadiusOfPeriapsis();
            }), maneuverSegment2, Double.NaN, Constants.Epsilon6);
        postManeuver2RadiusPerigeeEquality.setDesiredValue(postManeuver2RadiusPerigeeVariable.getValue());
        
        // Set up a constraint for the post-maneuver inclination.
        DelegateBasedConstraint postManeuver2InclinationEquality = new DelegateBasedConstraint(DelegateBasedConstraintCallback.of((results) -> {
                Motion1<Cartesian> motion = results.getStateForNextSegment().<Cartesian> getMotion(motionID);
                return new ModifiedKeplerianElements(motion, WorldGeodeticSystem1984.GravitationalParameter).getInclination();
            }), maneuverSegment2, Double.NaN, Constants.Epsilon6);
        postManeuver2InclinationEquality.setDesiredValue(postManeuver2InclinationVariable.getValue());
        
        // Define coasting segment that stops at second perigee.
        NumericalPropagatorSegment coastToPerigee3 = new NumericalPropagatorSegment();
        coastToPerigee3.setName("Coast_To_Perigee3");
        coastToPerigee3.setPropagatorDefinition(numericalPropagator);
        
        ScalarStoppingCondition perigee3StoppingCondition = new ScalarStoppingCondition();
        perigee3StoppingCondition.setThreshold(ValueDefinition.<Double> toValueDefinition((double) 0.0));
        perigee3StoppingCondition.setFunctionTolerance(Constants.Epsilon6);
        perigee3StoppingCondition.setAngularSetting(CircularRange.NEGATIVE_PI_TO_PI);
        perigee3StoppingCondition.setTypeOfStoppingCondition(StopType.THRESHOLD_INCREASING);
        perigee3StoppingCondition
                .setScalar(new ScalarModifiedKeplerianElement(earthGravitationalParameter, propagationPoint.getIntegrationPoint(), KeplerianElement.TRUE_ANOMALY, earth.getInertialFrame()));
        coastToPerigee3.getStoppingConditions().add(perigee3StoppingCondition);
        
        // Define coasting segment that stops at second apogee.
        NumericalPropagatorSegment coastToManeuver3 = new NumericalPropagatorSegment();
        coastToManeuver3.setName("Coast_To_Maneuver3_Start");
        coastToManeuver3.setPropagatorDefinition(numericalPropagator);
        
        ScalarStoppingCondition apogee3StoppingCondition = new ScalarStoppingCondition();
        apogee3StoppingCondition.setThreshold(ValueDefinition.<Double> toValueDefinition((double) apogeeTrueAnomaly));
        apogee3StoppingCondition.setFunctionTolerance(Constants.Epsilon6);
        apogee3StoppingCondition.setAngularSetting(CircularRange.ZERO_TO_TWO_PI);
        apogee3StoppingCondition.setTypeOfStoppingCondition(StopType.THRESHOLD_INCREASING);
        apogee3StoppingCondition
                .setScalar(new ScalarModifiedKeplerianElement(earthGravitationalParameter, propagationPoint.getIntegrationPoint(), KeplerianElement.TRUE_ANOMALY, earth.getInertialFrame()));
        coastToManeuver3.getStoppingConditions().add(apogee3StoppingCondition);
        
        // Define impulsive maneuver segment for third and final maneuver.
        ImpulsiveManeuverSegment maneuverSegment3 = new ImpulsiveManeuverSegment();
        ImpulsiveManeuverInformation maneuver3 = new ImpulsiveManeuverInformation(motionID, Cartesian.getZero(), fuelMassName, dryMassName, Scalar.toScalar(exhaustVelocity),
                InvalidFuelStateBehavior.DO_AS_MUCH_AS_POSSIBLE);
        maneuver3.setOrientation(new AxesVelocityOrbitNormal(maneuver3.getPropagationPoint(), earth));
        maneuverSegment3.setManeuver(maneuver3);
        
        // Set up variable for delta-v in the velocity direction.
        DelegateBasedVariable<ImpulsiveManeuverSegmentConfiguration> deltaV3XVariable = maneuverSegment3.createVariable(100.0, 1.0,
                SetVariableCallback.of((deltaVx, configuration) -> {
                    configuration.getManeuver().setX(configuration.getManeuver().getX() + deltaVx);// m/s.
                }));
        
        // Set up variable for delta-v in the normal direction.
        DelegateBasedVariable<ImpulsiveManeuverSegmentConfiguration> deltaV3YVariable = maneuverSegment3.createVariable(100.0, 1.0,
                SetVariableCallback.of((deltaVy, configuration) -> {
                    configuration.getManeuver().setY(configuration.getManeuver().getY() + deltaVy);// m/s.
                }));
        
        // Set up a constraint for the post-maneuver semi-major axis to be the GEO radius.
        DelegateBasedConstraint postManeuver3SemimajorAxisEquality = new DelegateBasedConstraint(DelegateBasedConstraintCallback.of((results) -> {
                Motion1<Cartesian> motion = results.getStateForNextSegment().<Cartesian> getMotion(motionID);
                return 1.0 / new ModifiedKeplerianElements(motion, WorldGeodeticSystem1984.GravitationalParameter).getInverseSemimajorAxis();
            }), maneuverSegment3, initialRadiusApoapsis, Constants.Epsilon2);
        
        // Set up a constraint for the post-maneuver inclination to be zero.
        DelegateBasedConstraint postManeuver3InclinationEquality = new DelegateBasedConstraint(DelegateBasedConstraintCallback.of((results) -> {
                Motion1<Cartesian> motion = results.getStateForNextSegment().<Cartesian> getMotion(motionID);
                return new ModifiedKeplerianElements(motion, WorldGeodeticSystem1984.GravitationalParameter).getInclination();
            }), maneuverSegment3, 0.0, Constants.Epsilon5);
        
        // Define coasting segment that stops one day later.
        NumericalPropagatorSegment finalGeostationaryOrbit = new NumericalPropagatorSegment();
        finalGeostationaryOrbit.setName("Final Geostationary Orbit");
        finalGeostationaryOrbit.setPropagatorDefinition(numericalPropagator);

        DurationStoppingCondition oneDayStoppingCondition = new DurationStoppingCondition();
        oneDayStoppingCondition.setThreshold(ValueDefinition.<Duration> toValueDefinition(Duration.fromDays(1.0)));
        oneDayStoppingCondition.setFunctionTolerance(Constants.Epsilon4);

        finalGeostationaryOrbit.getStoppingConditions().add(oneDayStoppingCondition);
        
        // Build targeted segment list optimizer and nested differential correctors.
        TargetedSegmentList outerTargetedSegmentList = new TargetedSegmentList();
        outerTargetedSegmentList.getSegments().add(initialStateSegment);
        outerTargetedSegmentList.getSegments().add(coastToManeuver1);
        
        TargetedSegmentList maneuver1TargetedSegmentList = new TargetedSegmentList();
        maneuver1TargetedSegmentList.getSegments().add(maneuverSegment1);
        
        TargetedSegmentListDifferentialCorrector maneuver1DifferentialCorrector = new TargetedSegmentListDifferentialCorrector();
        maneuver1DifferentialCorrector.getVariables().add(deltaV1XVariable);
        maneuver1DifferentialCorrector.getVariables().add(deltaV1YVariable);
        maneuver1DifferentialCorrector.getConstraints().add(postManeuver1RadiusPerigeeEquality);
        maneuver1DifferentialCorrector.getConstraints().add(postManeuver1InclinationEquality);
        maneuver1DifferentialCorrector.getSolver().setMultithreaded(false);
        maneuver1TargetedSegmentList.getOperators().add(maneuver1DifferentialCorrector);
        outerTargetedSegmentList.getSegments().add(maneuver1TargetedSegmentList);
        
        ScalarDifferenceOfSegmentCostFunction fuelUsageCostFunction1 = new ScalarDifferenceOfSegmentCostFunction(maneuver1TargetedSegmentList, CostFunctionGoal.MINIMIZE, Constants.Epsilon1,
                ScalarConstraintDifference.INITIAL_MINUS_FINAL);
        fuelUsageCostFunction1.setScalar(new ParameterizedOnStateScalar(fuelUsageCostFunction1.getParameter(), fuelMassName));
        
        // Set up an inequality constraint that ensures that the radius of perigee increases with the maneuver.
        DelegateBasedInequalityConstraint postManeuver1RadiusPerigeeLowerBound = new DelegateBasedInequalityConstraint(DelegateBasedInequalityConstraintCallback.of((results) -> {
                Motion1<Cartesian> motion = results.getStateForNextSegment().<Cartesian> getMotion(motionID);
                return new ModifiedKeplerianElements(motion, WorldGeodeticSystem1984.GravitationalParameter).getRadiusOfPeriapsis();
            }), maneuver1TargetedSegmentList, InequalityBoundType.LOWER_BOUND, initialRadiusPeriapsis, Constants.Epsilon3);
        
        // Set up an inequality constraint that ensures that the inclination decreases with the maneuver.
        DelegateBasedInequalityConstraint postManeuver1InclinationUpperBound = new DelegateBasedInequalityConstraint(DelegateBasedInequalityConstraintCallback.of((results) -> {
                Motion1<Cartesian> motion = results.getStateForNextSegment().<Cartesian> getMotion(motionID);
                return new ModifiedKeplerianElements(motion, WorldGeodeticSystem1984.GravitationalParameter).getInclination();
            }), maneuver1TargetedSegmentList, InequalityBoundType.UPPER_BOUND, initialInclination, Constants.Epsilon3);
        
        outerTargetedSegmentList.getSegments().add(coastToPerigee2);
        outerTargetedSegmentList.getSegments().add(coastToManeuver2);
        
        TargetedSegmentList maneuver2TargetedSegmentList = new TargetedSegmentList();
        maneuver2TargetedSegmentList.getSegments().add(maneuverSegment2);
        
        TargetedSegmentListDifferentialCorrector maneuver2DifferentialCorrector = new TargetedSegmentListDifferentialCorrector();
        maneuver2DifferentialCorrector.getVariables().add(deltaV2XVariable);
        maneuver2DifferentialCorrector.getVariables().add(deltaV2YVariable);
        maneuver2DifferentialCorrector.getConstraints().add(postManeuver2RadiusPerigeeEquality);
        maneuver2DifferentialCorrector.getConstraints().add(postManeuver2InclinationEquality);
        maneuver2DifferentialCorrector.getSolver().setMultithreaded(false);
        maneuver2TargetedSegmentList.getOperators().add(maneuver2DifferentialCorrector);
        outerTargetedSegmentList.getSegments().add(maneuver2TargetedSegmentList);
        
        ScalarDifferenceOfSegmentCostFunction fuelUsageCostFunction2 = new ScalarDifferenceOfSegmentCostFunction(maneuver2TargetedSegmentList, CostFunctionGoal.MINIMIZE, Constants.Epsilon1,
                ScalarConstraintDifference.INITIAL_MINUS_FINAL);
        fuelUsageCostFunction2.setScalar(new ParameterizedOnStateScalar(fuelUsageCostFunction2.getParameter(), fuelMassName));
        
        // Set up an inequality constraint that ensures that the radius of perigee increases with the maneuver.
        DelegateBasedInequalityConstraint postManeuver2RadiusPerigeeLowerBound = new DelegateBasedInequalityConstraint(DelegateBasedInequalityConstraintCallback.of((results) -> {
                Motion1<Cartesian> motion = results.getStateForNextSegment().<Cartesian> getMotion(motionID);
                return new ModifiedKeplerianElements(motion, WorldGeodeticSystem1984.GravitationalParameter).getRadiusOfPeriapsis();
            }), maneuver2TargetedSegmentList, InequalityBoundType.LOWER_BOUND, Double.NaN, Constants.Epsilon3);
        postManeuver2RadiusPerigeeLowerBound.setBoundValue(postManeuver1RadiusPerigeeVariable.getValue());
        
        // Set up an inequality constraint that ensures that the radius of perigee does not increase beyond GEO radius.
        DelegateBasedInequalityConstraint postManeuver2RadiusPerigeeUpperBound = new DelegateBasedInequalityConstraint(DelegateBasedInequalityConstraintCallback.of((results) -> {
                Motion1<Cartesian> motion = results.getStateForNextSegment().<Cartesian> getMotion(motionID);
                return new ModifiedKeplerianElements(motion, WorldGeodeticSystem1984.GravitationalParameter).getRadiusOfPeriapsis();
            }), maneuver2TargetedSegmentList, InequalityBoundType.UPPER_BOUND, initialRadiusApoapsis, Constants.Epsilon3);
        
        // Set up an inequality constraint that ensures that the inclination decreases with the maneuver.
        DelegateBasedInequalityConstraint postManeuver2InclinationUpperBound = new DelegateBasedInequalityConstraint(DelegateBasedInequalityConstraintCallback.of((results) -> {
                Motion1<Cartesian> motion = results.getStateForNextSegment().<Cartesian> getMotion(motionID);
                return new ModifiedKeplerianElements(motion, WorldGeodeticSystem1984.GravitationalParameter).getInclination();
            }), maneuver2TargetedSegmentList, InequalityBoundType.UPPER_BOUND, Double.NaN, Constants.Epsilon3);
        
        postManeuver2InclinationUpperBound.setBoundValue(postManeuver1InclinationVariable.getValue());
        outerTargetedSegmentList.getSegments().add(coastToPerigee3);
        outerTargetedSegmentList.getSegments().add(coastToManeuver3);
        
        TargetedSegmentList maneuver3TargetedSegmentList = new TargetedSegmentList();
        maneuver3TargetedSegmentList.getSegments().add(maneuverSegment3);
        
        TargetedSegmentListDifferentialCorrector maneuver3DifferentialCorrector = new TargetedSegmentListDifferentialCorrector();
        maneuver3DifferentialCorrector.getVariables().add(deltaV3XVariable);
        maneuver3DifferentialCorrector.getVariables().add(deltaV3YVariable);
        maneuver3DifferentialCorrector.getConstraints().add(postManeuver3SemimajorAxisEquality);
        maneuver3DifferentialCorrector.getConstraints().add(postManeuver3InclinationEquality);
        maneuver3DifferentialCorrector.getSolver().setMultithreaded(false);
        maneuver3TargetedSegmentList.getOperators().add(maneuver3DifferentialCorrector);
        outerTargetedSegmentList.getSegments().add(maneuver3TargetedSegmentList);
        outerTargetedSegmentList.getSegments().add(finalGeostationaryOrbit);
        
        ScalarDifferenceOfSegmentCostFunction fuelUsageCostFunction3 = new ScalarDifferenceOfSegmentCostFunction(maneuver3TargetedSegmentList, CostFunctionGoal.MINIMIZE, Constants.Epsilon1,
                ScalarConstraintDifference.INITIAL_MINUS_FINAL);
        fuelUsageCostFunction3.setScalar(new ParameterizedOnStateScalar(fuelUsageCostFunction3.getParameter(), fuelMassName));
        
        final ArrayList<SegmentPropagatorCostFunction> costFunctionCollection = new ArrayList<SegmentPropagatorCostFunction>();
        costFunctionCollection.add(fuelUsageCostFunction1);
        costFunctionCollection.add(fuelUsageCostFunction2);
        costFunctionCollection.add(fuelUsageCostFunction3);
        CombinedCostFunction fuelUsageCombinedCostFunction = new CombinedCostFunction(CostFunctionGoal.MINIMIZE, Constants.Epsilon3, new NoScalingOnCostFunction(), 1.0, costFunctionCollection);
        
        TargetedSegmentListParameterOptimizer outerOptimizer = new TargetedSegmentListParameterOptimizer();
        outerOptimizer.getVariables().add(postManeuver1RadiusPerigeeVariable);
        outerOptimizer.getVariables().add(postManeuver1InclinationVariable);
        outerOptimizer.getVariables().add(postManeuver2RadiusPerigeeVariable);
        outerOptimizer.getVariables().add(postManeuver2InclinationVariable);
        
        outerOptimizer.setCostFunction(fuelUsageCombinedCostFunction);
        outerOptimizer.getInequalities().add(postManeuver1RadiusPerigeeLowerBound);
        outerOptimizer.getInequalities().add(postManeuver1InclinationUpperBound);
        outerOptimizer.getInequalities().add(postManeuver2RadiusPerigeeLowerBound);
        outerOptimizer.getInequalities().add(postManeuver2RadiusPerigeeUpperBound);
        outerOptimizer.getInequalities().add(postManeuver2InclinationUpperBound);
        
        ActiveSetSequentialQuadraticProgrammingOptimizer internalOptimizer = (ActiveSetSequentialQuadraticProgrammingOptimizer) outerOptimizer.getOptimizer();
        internalOptimizer.setMultithreaded(false);
        internalOptimizer.setLineSearchSettings(new LineSearchSettings(Constants.Epsilon6, Constants.Epsilon6, ConvergenceCriteria.EITHER, 10));
        outerOptimizer.setOptimizer(internalOptimizer);
        outerTargetedSegmentList.getOperators().add(outerOptimizer);
        
        // Set this value to TargetedSegmentListOperatorBehavior.RUN_ACTIVE_OPERATORS to run the optimizer until convergence.
        outerTargetedSegmentList.setOperatorAction(TargetedSegmentListOperatorBehavior.RUN_ACTIVE_OPERATORS_ONCE);
        // #endregion
        
        return outerTargetedSegmentList;
	}

}
