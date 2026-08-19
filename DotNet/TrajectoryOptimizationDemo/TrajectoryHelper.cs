using System;
using System.Collections.Generic;
using AGI.Foundation.NumericalMethods;
using AGI.Foundation.Time;
using AGI.Foundation.NumericalMethods.Advanced;
using AGI.Foundation.Propagators;
using AGI.Foundation.SegmentPropagation;
using AGI.Foundation.StoppingConditions;
using AGI.Foundation;
using AGI.Foundation.Celestial;
using AGI.Foundation.Coordinates;
using AGI.Foundation.Geometry;

namespace AGI.Examples
{
    public static class TrajectoryHelper
    {
        public static TargetedSegmentList CreateTrajectoryOptimizer(JulianDate epoch)
        {
            #region Create Trajectory Optimizer

            const string motionID = "Satellite";
            const string fuelMassName = "Fuel_Mass";
            const string dryMassName = "Dry_Mass";

            EarthCentralBody earth = CentralBodiesFacet.GetFromContext().Earth;
            double earthGravitationalParameter = WorldGeodeticSystem1984.GravitationalParameter;

            // Initial state has periapsis 300 km above Earth's surface and apoapsis at approximately GEO radius.
            const double initialRadiusPeriapsis = 6678000.0; // meters.
            double initialRadiusApoapsis = Math.Pow(earthGravitationalParameter
                                                    * Math.Pow(TimeConstants.SecondsPerDay / Constants.TwoPi, 2.0),
                                                    1.0 / 3.0); // meters.
            double initialSemimajorAxis = 0.5 * (initialRadiusPeriapsis + initialRadiusApoapsis);
            double initialInclination = Trig.DegreesToRadians(57.0);

            var initialState = new ModifiedKeplerianElements(initialRadiusPeriapsis,
                                                             1.0 / initialSemimajorAxis,
                                                             initialInclination,
                                                             0.0,
                                                             0.0,
                                                             0.0,
                                                             earthGravitationalParameter).ToCartesian();

            // Specific impulse for impulsive maneuvers.
            const double isp = 300.0; // seconds
            double exhaustVelocity = isp * Constants.EarthSurfaceGravity;

            // Fuel mass.
            PropagationScalar fuelMass = new PropagationScalar(1000.0)
            {
                Identification = fuelMassName,
                ScalarDerivative = new ScalarFixed(0.0),
            };

            // Dry mass.
            PropagationScalar dryMass = new PropagationScalar(500.0)
            {
                Identification = dryMassName,
                ScalarDerivative = new ScalarFixed(0.0),
            };

            // Create the integrator.
            var integrator = new RungeKuttaVerner89Integrator
            {
                InitialStepSize = 60.0,
            };

            // The propagation point and propagator.
            var propagationPoint = new PropagationNewtonianPoint(motionID,
                                                                 earth.InertialFrame,
                                                                 initialState.Value,
                                                                 initialState.FirstDerivative);

            propagationPoint.Mass = fuelMass.IntegrationValue + dryMass.IntegrationValue;

            TwoBodyGravity earthGravity =
                new TwoBodyGravity(propagationPoint.IntegrationPoint,
                                   earth,
                                   earthGravitationalParameter);
            propagationPoint.AppliedForces.Add(earthGravity);

            var numericalPropagator = new NumericalPropagatorDefinition();

            numericalPropagator.IntegrationElements.Add(propagationPoint);
            numericalPropagator.IntegrationElements.Add(fuelMass);
            numericalPropagator.IntegrationElements.Add(dryMass);

            numericalPropagator.Integrator = integrator;
            numericalPropagator.Epoch = epoch;

            // Define initial state segment.
            NumericalInitialStateSegment initialStateSegment = new NumericalInitialStateSegment();
            initialStateSegment.Name = "Initial_State_Segment";
            initialStateSegment.PropagatorDefinition = numericalPropagator;

            // Define coasting segment that stops at first apogee.
            NumericalPropagatorSegment coastToManeuver1 = new NumericalPropagatorSegment();
            coastToManeuver1.Name = "Coast_To_Maneuver1_Start";
            coastToManeuver1.PropagatorDefinition = numericalPropagator;

            const double apogeeTrueAnomaly = Math.PI; // 180 degrees in radians.

            var apogee1StoppingCondition = new ScalarStoppingCondition
            {
                Threshold = apogeeTrueAnomaly,
                FunctionTolerance = Constants.Epsilon6,
                AngularSetting = CircularRange.ZeroToTwoPi,
                TypeOfStoppingCondition = StopType.ThresholdIncreasing,
            };

            apogee1StoppingCondition.Scalar = new ScalarModifiedKeplerianElement(earthGravitationalParameter,
                                                                                 propagationPoint.IntegrationPoint,
                                                                                 KeplerianElement.TrueAnomaly,
                                                                                 earth.InertialFrame);

            coastToManeuver1.StoppingConditions.Add(apogee1StoppingCondition);

            // Define impulsive maneuver segment for first maneuver.
            ImpulsiveManeuverSegment maneuverSegment1 = new ImpulsiveManeuverSegment();

            ImpulsiveManeuverInformation maneuver1 = new ImpulsiveManeuverInformation(motionID, Cartesian.Zero, fuelMassName, dryMassName,
                                                                                      exhaustVelocity, InvalidFuelStateBehavior.DoAsMuchAsPossible);

            maneuver1.Orientation = new AxesVelocityOrbitNormal(maneuver1.PropagationPoint, earth);
            maneuverSegment1.Maneuver = maneuver1;

            // Set up variable for delta-v in the velocity direction.
            var deltaV1XVariable = maneuverSegment1.CreateVariable(100.0, 1.0, (deltaVx, configuration) =>
            {
                configuration.Maneuver.X += deltaVx; // m/s.
            });

            // Set up variable for delta-v in the normal direction.
            var deltaV1YVariable = maneuverSegment1.CreateVariable(100.0, 1.0, (deltaVy, configuration) =>
            {
                configuration.Maneuver.Y += deltaVy; // m/s.
            });

            // Set up a variable for the post-maneuver radius of perigee.
            const double postManeuver1RadiusPeriapsisGuess = initialRadiusPeriapsis + 1000000.0;
            var postManeuver1RadiusPerigeeVariable = new ParameterizedDoubleVariable(postManeuver1RadiusPeriapsisGuess,
                                                                                     1000000.0, 100000.0, maneuverSegment1);
            postManeuver1RadiusPerigeeVariable.Settings.VariableTolerance = Constants.Epsilon1;

            // Set up a variable for the post-maneuver inclination.
            double postManeuver1InclinationGuess = Trig.DegreesToRadians(45.0);
            var postManeuver1InclinationVariable = new ParameterizedDoubleVariable(postManeuver1InclinationGuess,
                                                                                   Trig.DegreesToRadians(5.0),
                                                                                   Trig.DegreesToRadians(1.0), maneuverSegment1);
            postManeuver1InclinationVariable.Settings.VariableTolerance = Constants.Epsilon1;

            // Set up a constraint for the post-maneuver radius of perigee.
            var postManeuver1RadiusPerigeeEquality = new DelegateBasedConstraint(results =>
            {
                var motion = results.StateForNextSegment.GetMotion<Cartesian>(motionID);
                return new ModifiedKeplerianElements(motion, WorldGeodeticSystem1984.GravitationalParameter).RadiusOfPeriapsis;
            }, maneuverSegment1, double.NaN, Constants.Epsilon3);

            postManeuver1RadiusPerigeeEquality.DesiredValue = postManeuver1RadiusPerigeeVariable.Value;

            // Set up a constraint for the post-maneuver inclination.
            var postManeuver1InclinationEquality = new DelegateBasedConstraint(results =>
            {
                var motion = results.StateForNextSegment.GetMotion<Cartesian>(motionID);
                return new ModifiedKeplerianElements(motion, WorldGeodeticSystem1984.GravitationalParameter).Inclination;
            }, maneuverSegment1, double.NaN, Constants.Epsilon3);

            postManeuver1InclinationEquality.DesiredValue = postManeuver1InclinationVariable.Value;

            // Define coasting segment that stops at second perigee.
            NumericalPropagatorSegment coastToPerigee2 = new NumericalPropagatorSegment();
            coastToPerigee2.Name = "Coast_To_Perigee2";
            coastToPerigee2.PropagatorDefinition = numericalPropagator;

            var perigee2StoppingCondition = new ScalarStoppingCondition
            {
                Threshold = 0.0,
                FunctionTolerance = Constants.Epsilon6,
                AngularSetting = CircularRange.NegativePiToPi,
                TypeOfStoppingCondition = StopType.ThresholdIncreasing,
            };

            perigee2StoppingCondition.Scalar = new ScalarModifiedKeplerianElement(earthGravitationalParameter,
                                                                                  propagationPoint.IntegrationPoint,
                                                                                  KeplerianElement.TrueAnomaly,
                                                                                  earth.InertialFrame);

            coastToPerigee2.StoppingConditions.Add(perigee2StoppingCondition);

            // Define coasting segment that stops at second apogee.
            NumericalPropagatorSegment coastToManeuver2 = new NumericalPropagatorSegment();
            coastToManeuver2.Name = "Coast_To_Maneuver2_Start";
            coastToManeuver2.PropagatorDefinition = numericalPropagator;

            var apogee2StoppingCondition = new ScalarStoppingCondition
            {
                Threshold = apogeeTrueAnomaly,
                FunctionTolerance = Constants.Epsilon6,
                AngularSetting = CircularRange.ZeroToTwoPi,
                TypeOfStoppingCondition = StopType.ThresholdIncreasing,
            };

            apogee2StoppingCondition.Scalar = new ScalarModifiedKeplerianElement(earthGravitationalParameter,
                                                                                 propagationPoint.IntegrationPoint,
                                                                                 KeplerianElement.TrueAnomaly,
                                                                                 earth.InertialFrame);

            coastToManeuver2.StoppingConditions.Add(apogee2StoppingCondition);

            // Define impulsive maneuver segment for second maneuver.
            ImpulsiveManeuverSegment maneuverSegment2 = new ImpulsiveManeuverSegment();
            ImpulsiveManeuverInformation maneuver2 = new ImpulsiveManeuverInformation(motionID, Cartesian.Zero, fuelMassName, dryMassName,
                                                                                      exhaustVelocity, InvalidFuelStateBehavior.DoAsMuchAsPossible);

            maneuver2.Orientation = new AxesVelocityOrbitNormal(maneuver2.PropagationPoint, earth);
            maneuverSegment2.Maneuver = maneuver2;

            // Set up variable for delta-v in the velocity direction.
            var deltaV2XVariable = maneuverSegment2.CreateVariable(100.0, 1.0, (deltaVx, configuration) =>
            {
                configuration.Maneuver.X += deltaVx; // m/s.
            });

            // Set up variable for delta-v in the normal direction.
            var deltaV2YVariable = maneuverSegment2.CreateVariable(100.0, 1.0, (deltaVy, configuration) =>
            {
                configuration.Maneuver.Y += deltaVy; // m/s.
            });

            // Set up a variable for the post-maneuver radius of perigee.
            const double postManeuver2RadiusPeriapsisGuess = initialRadiusPeriapsis + 10000000.0;
            var postManeuver2RadiusPerigeeVariable = new ParameterizedDoubleVariable(postManeuver2RadiusPeriapsisGuess,
                                                                                     1000000.0, 100000.0, maneuverSegment1);
            postManeuver2RadiusPerigeeVariable.Settings.VariableTolerance = Constants.Epsilon1;

            // Set up a variable for the post-maneuver inclination.
            double postManeuver2InclinationGuess = Trig.DegreesToRadians(30.0);
            var postManeuver2InclinationVariable = new ParameterizedDoubleVariable(postManeuver2InclinationGuess,
                                                                                   Trig.DegreesToRadians(5.0),
                                                                                   Trig.DegreesToRadians(1.0), maneuverSegment1);
            postManeuver2InclinationVariable.Settings.VariableTolerance = Constants.Epsilon1;

            // Set up a constraint for the post-maneuver radius of perigee.
            var postManeuver2RadiusPerigeeEquality = new DelegateBasedConstraint(results =>
            {
                var motion = results.StateForNextSegment.GetMotion<Cartesian>(motionID);
                return new ModifiedKeplerianElements(motion, WorldGeodeticSystem1984.GravitationalParameter).RadiusOfPeriapsis;
            }, maneuverSegment2, double.NaN, Constants.Epsilon6);

            postManeuver2RadiusPerigeeEquality.DesiredValue = postManeuver2RadiusPerigeeVariable.Value;

            // Set up a constraint for the post-maneuver inclination.
            var postManeuver2InclinationEquality = new DelegateBasedConstraint(results =>
            {
                var motion = results.StateForNextSegment.GetMotion<Cartesian>(motionID);
                return new ModifiedKeplerianElements(motion, WorldGeodeticSystem1984.GravitationalParameter).Inclination;
            }, maneuverSegment2, double.NaN, Constants.Epsilon6);

            postManeuver2InclinationEquality.DesiredValue = postManeuver2InclinationVariable.Value;

            // Define coasting segment that stops at second perigee.
            NumericalPropagatorSegment coastToPerigee3 = new NumericalPropagatorSegment();
            coastToPerigee3.Name = "Coast_To_Perigee3";
            coastToPerigee3.PropagatorDefinition = numericalPropagator;

            var perigee3StoppingCondition = new ScalarStoppingCondition
            {
                Threshold = 0.0,
                FunctionTolerance = Constants.Epsilon6,
                AngularSetting = CircularRange.NegativePiToPi,
                TypeOfStoppingCondition = StopType.ThresholdIncreasing,
            };

            perigee3StoppingCondition.Scalar = new ScalarModifiedKeplerianElement(earthGravitationalParameter,
                                                                                  propagationPoint.IntegrationPoint,
                                                                                  KeplerianElement.TrueAnomaly,
                                                                                  earth.InertialFrame);

            coastToPerigee3.StoppingConditions.Add(perigee3StoppingCondition);

            // Define coasting segment that stops at second apogee.
            NumericalPropagatorSegment coastToManeuver3 = new NumericalPropagatorSegment();
            coastToManeuver3.Name = "Coast_To_Maneuver3_Start";
            coastToManeuver3.PropagatorDefinition = numericalPropagator;

            var apogee3StoppingCondition = new ScalarStoppingCondition
            {
                Threshold = apogeeTrueAnomaly,
                FunctionTolerance = Constants.Epsilon6,
                AngularSetting = CircularRange.ZeroToTwoPi,
                TypeOfStoppingCondition = StopType.ThresholdIncreasing,
            };

            apogee3StoppingCondition.Scalar = new ScalarModifiedKeplerianElement(earthGravitationalParameter,
                                                                                 propagationPoint.IntegrationPoint,
                                                                                 KeplerianElement.TrueAnomaly,
                                                                                 earth.InertialFrame);

            coastToManeuver3.StoppingConditions.Add(apogee3StoppingCondition);

            // Define impulsive maneuver segment for third and final maneuver.
            ImpulsiveManeuverSegment maneuverSegment3 = new ImpulsiveManeuverSegment();
            ImpulsiveManeuverInformation maneuver3 = new ImpulsiveManeuverInformation(motionID, Cartesian.Zero, fuelMassName, dryMassName,
                                                                                       exhaustVelocity, InvalidFuelStateBehavior.DoAsMuchAsPossible);

            maneuver3.Orientation = new AxesVelocityOrbitNormal(maneuver3.PropagationPoint, earth);
            maneuverSegment3.Maneuver = maneuver3;

            // Set up variable for delta-v in the velocity direction.
            var deltaV3XVariable = maneuverSegment3.CreateVariable(100.0, 1.0, (deltaVx, configuration) =>
            {
                configuration.Maneuver.X += deltaVx; // m/s.
            });

            // Set up variable for delta-v in the normal direction.
            var deltaV3YVariable = maneuverSegment3.CreateVariable(100.0, 1.0, (deltaVy, configuration) =>
            {
                configuration.Maneuver.Y += deltaVy; // m/s.
            });

            // Set up a constraint for the post-maneuver semi-major axis to be the GEO radius.
            var postManeuver3SemimajorAxisEquality = new DelegateBasedConstraint(results =>
            {
                var motion = results.StateForNextSegment.GetMotion<Cartesian>(motionID);
                return 1.0 / new ModifiedKeplerianElements(motion, WorldGeodeticSystem1984.GravitationalParameter).InverseSemimajorAxis;
            }, maneuverSegment3, initialRadiusApoapsis, Constants.Epsilon2);

            // Set up a constraint for the post-maneuver inclination to be zero.
            var postManeuver3InclinationEquality = new DelegateBasedConstraint(results =>
            {
                var motion = results.StateForNextSegment.GetMotion<Cartesian>(motionID);
                return new ModifiedKeplerianElements(motion, WorldGeodeticSystem1984.GravitationalParameter).Inclination;
            }, maneuverSegment3, 0.0, Constants.Epsilon5);

            // Define coasting segment that stops one day later.
            NumericalPropagatorSegment finalGeostationaryOrbit = new NumericalPropagatorSegment();
            finalGeostationaryOrbit.Name = "Final Geostationary Orbit";
            finalGeostationaryOrbit.PropagatorDefinition = numericalPropagator;

            var oneDayStoppingCondition = new DurationStoppingCondition
            {
                Threshold = Duration.FromDays(1.0),
                FunctionTolerance = Constants.Epsilon4,
            };

            finalGeostationaryOrbit.StoppingConditions.Add(oneDayStoppingCondition);

            // Build targeted segment list optimizer and nested differential correctors.
            var outerTargetedSegmentList = new TargetedSegmentList();
            outerTargetedSegmentList.Segments.Add(initialStateSegment);
            outerTargetedSegmentList.Segments.Add(coastToManeuver1);

            var maneuver1TargetedSegmentList = new TargetedSegmentList();
            maneuver1TargetedSegmentList.Segments.Add(maneuverSegment1);
            var maneuver1DifferentialCorrector = new TargetedSegmentListDifferentialCorrector();
            maneuver1DifferentialCorrector.Variables.Add(deltaV1XVariable);
            maneuver1DifferentialCorrector.Variables.Add(deltaV1YVariable);
            maneuver1DifferentialCorrector.Constraints.Add(postManeuver1RadiusPerigeeEquality);
            maneuver1DifferentialCorrector.Constraints.Add(postManeuver1InclinationEquality);

            // All optimizers and solvers must not be multithreaded in this example or exceptions occur.
            maneuver1DifferentialCorrector.Solver.Multithreaded = false;
            maneuver1TargetedSegmentList.Operators.Add(maneuver1DifferentialCorrector);
            outerTargetedSegmentList.Segments.Add(maneuver1TargetedSegmentList);

            var fuelUsageCostFunction1 = new ScalarDifferenceOfSegmentCostFunction(maneuver1TargetedSegmentList,
                                                                                   CostFunctionGoal.Minimize,
                                                                                   Constants.Epsilon1,
                                                                                   ScalarConstraintDifference.InitialMinusFinal);

            fuelUsageCostFunction1.Scalar = new ParameterizedOnStateScalar(fuelUsageCostFunction1.Parameter, fuelMassName);

            // Set up an inequality constraint that ensures that the radius of perigee increases with the maneuver.
            var postManeuver1RadiusPerigeeLowerBound = new DelegateBasedInequalityConstraint(results =>
            {
                var motion = results.StateForNextSegment.GetMotion<Cartesian>(motionID);
                return new ModifiedKeplerianElements(motion, WorldGeodeticSystem1984.GravitationalParameter).RadiusOfPeriapsis;
            }, maneuver1TargetedSegmentList, InequalityBoundType.LowerBound, initialRadiusPeriapsis, Constants.Epsilon3);

            // Set up an inequality constraint that ensures that the inclination decreases with the maneuver.
            var postManeuver1InclinationUpperBound = new DelegateBasedInequalityConstraint(results =>
            {
                var motion = results.StateForNextSegment.GetMotion<Cartesian>(motionID);
                return new ModifiedKeplerianElements(motion, WorldGeodeticSystem1984.GravitationalParameter).Inclination;
            }, maneuver1TargetedSegmentList, InequalityBoundType.UpperBound, initialInclination, Constants.Epsilon3);

            outerTargetedSegmentList.Segments.Add(coastToPerigee2);
            outerTargetedSegmentList.Segments.Add(coastToManeuver2);

            var maneuver2TargetedSegmentList = new TargetedSegmentList();
            maneuver2TargetedSegmentList.Segments.Add(maneuverSegment2);
            var maneuver2DifferentialCorrector = new TargetedSegmentListDifferentialCorrector();
            maneuver2DifferentialCorrector.Variables.Add(deltaV2XVariable);
            maneuver2DifferentialCorrector.Variables.Add(deltaV2YVariable);
            maneuver2DifferentialCorrector.Constraints.Add(postManeuver2RadiusPerigeeEquality);
            maneuver2DifferentialCorrector.Constraints.Add(postManeuver2InclinationEquality);

            // All optimizers and solvers must not be multithreaded in this example or exceptions occur.
            maneuver2DifferentialCorrector.Solver.Multithreaded = false;
            maneuver2TargetedSegmentList.Operators.Add(maneuver2DifferentialCorrector);
            outerTargetedSegmentList.Segments.Add(maneuver2TargetedSegmentList);

            var fuelUsageCostFunction2 = new ScalarDifferenceOfSegmentCostFunction(maneuver2TargetedSegmentList,
                                                                                   CostFunctionGoal.Minimize,
                                                                                   Constants.Epsilon1,
                                                                                   ScalarConstraintDifference.InitialMinusFinal);

            fuelUsageCostFunction2.Scalar = new ParameterizedOnStateScalar(fuelUsageCostFunction2.Parameter, fuelMassName);

            // Set up an inequality constraint that ensures that the radius of perigee increases with the maneuver.
            var postManeuver2RadiusPerigeeLowerBound = new DelegateBasedInequalityConstraint(results =>
            {
                var motion = results.StateForNextSegment.GetMotion<Cartesian>(motionID);
                return new ModifiedKeplerianElements(motion, WorldGeodeticSystem1984.GravitationalParameter).RadiusOfPeriapsis;
            }, maneuver2TargetedSegmentList, InequalityBoundType.LowerBound, double.NaN, Constants.Epsilon3);

            postManeuver2RadiusPerigeeLowerBound.BoundValue = postManeuver1RadiusPerigeeVariable.Value;

            // Set up an inequality constraint that ensures that the radius of perigee does not increase beyond GEO radius.
            var postManeuver2RadiusPerigeeUpperBound = new DelegateBasedInequalityConstraint(results =>
            {
                var motion = results.StateForNextSegment.GetMotion<Cartesian>(motionID);
                return new ModifiedKeplerianElements(motion, WorldGeodeticSystem1984.GravitationalParameter).RadiusOfPeriapsis;
            }, maneuver2TargetedSegmentList, InequalityBoundType.UpperBound, initialRadiusApoapsis, Constants.Epsilon3);

            // Set up an inequality constraint that ensures that the inclination decreases with the maneuver.
            var postManeuver2InclinationUpperBound = new DelegateBasedInequalityConstraint(results =>
            {
                var motion = results.StateForNextSegment.GetMotion<Cartesian>(motionID);
                return new ModifiedKeplerianElements(motion, WorldGeodeticSystem1984.GravitationalParameter).Inclination;
            }, maneuver2TargetedSegmentList, InequalityBoundType.UpperBound, double.NaN, Constants.Epsilon3);

            postManeuver2InclinationUpperBound.BoundValue = postManeuver1InclinationVariable.Value;

            outerTargetedSegmentList.Segments.Add(coastToPerigee3);
            outerTargetedSegmentList.Segments.Add(coastToManeuver3);

            var maneuver3TargetedSegmentList = new TargetedSegmentList();
            maneuver3TargetedSegmentList.Segments.Add(maneuverSegment3);
            var maneuver3DifferentialCorrector = new TargetedSegmentListDifferentialCorrector();
            maneuver3DifferentialCorrector.Variables.Add(deltaV3XVariable);
            maneuver3DifferentialCorrector.Variables.Add(deltaV3YVariable);
            maneuver3DifferentialCorrector.Constraints.Add(postManeuver3SemimajorAxisEquality);
            maneuver3DifferentialCorrector.Constraints.Add(postManeuver3InclinationEquality);

            // All optimizers and solvers must not be multithreaded in this example or exceptions occur.
            maneuver3DifferentialCorrector.Solver.Multithreaded = false;
            maneuver3TargetedSegmentList.Operators.Add(maneuver3DifferentialCorrector);
            outerTargetedSegmentList.Segments.Add(maneuver3TargetedSegmentList);
            outerTargetedSegmentList.Segments.Add(finalGeostationaryOrbit);

            var fuelUsageCostFunction3 = new ScalarDifferenceOfSegmentCostFunction(maneuver3TargetedSegmentList,
                                                                                   CostFunctionGoal.Minimize,
                                                                                   Constants.Epsilon1,
                                                                                   ScalarConstraintDifference.InitialMinusFinal);

            fuelUsageCostFunction3.Scalar = new ParameterizedOnStateScalar(fuelUsageCostFunction3.Parameter, fuelMassName);

            var fuelUsageCombinedCostFunction = new CombinedCostFunction(CostFunctionGoal.Minimize, Constants.Epsilon3, new NoScalingOnCostFunction(),
                                                                         1.0, new List<SegmentPropagatorCostFunction>
                                                                         {
                                                                             fuelUsageCostFunction1,
                                                                             fuelUsageCostFunction2,
                                                                             fuelUsageCostFunction3,
                                                                         });

            var outerOptimizer = new TargetedSegmentListParameterOptimizer();
            outerOptimizer.Variables.Add(postManeuver1RadiusPerigeeVariable);
            outerOptimizer.Variables.Add(postManeuver1InclinationVariable);
            outerOptimizer.Variables.Add(postManeuver2RadiusPerigeeVariable);
            outerOptimizer.Variables.Add(postManeuver2InclinationVariable);

            outerOptimizer.CostFunction = fuelUsageCombinedCostFunction;

            outerOptimizer.Inequalities.Add(postManeuver1RadiusPerigeeLowerBound);
            outerOptimizer.Inequalities.Add(postManeuver1InclinationUpperBound);
            outerOptimizer.Inequalities.Add(postManeuver2RadiusPerigeeLowerBound);
            outerOptimizer.Inequalities.Add(postManeuver2RadiusPerigeeUpperBound);
            outerOptimizer.Inequalities.Add(postManeuver2InclinationUpperBound);

            var internalOptimizer = (ActiveSetSequentialQuadraticProgrammingOptimizer)outerOptimizer.Optimizer;

            // All optimizers and solvers must not be multithreaded in this example or exceptions occur.
            internalOptimizer.Multithreaded = false;
            internalOptimizer.LineSearchSettings = new LineSearchSettings(Constants.Epsilon6, Constants.Epsilon6,
                                                                          ConvergenceCriteria.Either, 10);
            outerOptimizer.Optimizer = internalOptimizer;
            outerTargetedSegmentList.Operators.Add(outerOptimizer);

            // Change this to TargetedSegmentListOperatorBehavior.RunActiveOperators to run until optimizer completes.
            outerTargetedSegmentList.OperatorAction = TargetedSegmentListOperatorBehavior.RunActiveOperatorsOnce;

            #endregion

            return outerTargetedSegmentList;
        }
    }
}
