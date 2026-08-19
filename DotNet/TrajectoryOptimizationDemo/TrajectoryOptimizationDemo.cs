using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AGI.Foundation;
using AGI.Foundation.Graphics;
using AGI.Foundation.Graphics.Advanced;
using AGI.Foundation.Platforms;
using AGI.Foundation.Time;
using AGI.Foundation.Celestial;
using AGI.Foundation.Geometry;
using AGI.Foundation.Coordinates;
using AGI.Foundation.SegmentPropagation;
using AGI.Foundation.NumericalMethods;
using AGI.Foundation.Infrastructure.Threading;
using AGI.Foundation.NumericalMethods.Advanced;
using AGI.Foundation.Graphics.Renderer;

namespace AGI.Examples
{
    public partial class TrajectoryOptimizationDemo : Form
    {
        public TrajectoryOptimizationDemo()
        {
            InitializeComponent();

            m_insight3D = new Insight3D();
            m_insight3D.Dock = DockStyle.Fill;
            this.splitContainer1.Panel2.Controls.Add(m_insight3D);

            m_defaultStart = GregorianDate.Now;
            m_defaultEnd = m_defaultStart.AddDays(3);

            m_animation = new SimulationAnimation();
            SceneManager.Animation = m_animation;

            m_display = new ServiceProviderDisplay();

            // Create overlay toolbar and panels.
            m_overlayToolbar = new OverlayToolbar(m_insight3D);
            m_overlayToolbar.Overlay.Origin = ScreenOverlayOrigin.BottomCenter;
            m_overlayToolbar.AddButton(GetDataFilePath("Textures/OverlayToolbar/fewsatellites.png"), Satellite);

            // Initialize the text panel
            TextureScreenOverlay textPanel = new TextureScreenOverlay(0, 0, 220, 35)
            {
                Origin = ScreenOverlayOrigin.TopRight,
                BorderSize = 0,
                BorderColor = Color.Transparent,
                BorderTranslucency = 1.0f,
                Color = Color.Transparent,
                Translucency = 1.0f
            };
            SceneManager.ScreenOverlays.Add(textPanel);

            m_dateTimeFont = new Font("Courier New", 12, FontStyle.Bold);
            Size textSize = Insight3DHelper.MeasureString(m_defaultStart.ToString(), m_dateTimeFont);
            m_textOverlay = new TextureScreenOverlay(0, 0, textSize.Width, textSize.Height)
            {
                Origin = ScreenOverlayOrigin.Center,
                BorderSize = 0
            };
            textPanel.Overlays.Add(m_textOverlay);

            // Show label for the moon
            m_insight3D.Scene.CentralBodies[CentralBodiesFacet.GetFromContext().Moon].ShowLabel = true;

            // Set the name for the element that will get propagated
            m_elementID = "Satellite";

            // Subscribe to the time changed event
            SceneManager.TimeChanged += OnTimeChanged;

            m_animation.Time = m_defaultStart.ToJulianDate();
            m_animation.StartTime = m_defaultStart.ToJulianDate();
            m_animation.EndTime = m_defaultEnd.ToJulianDate();

            // Dynamically set the camera's position and direction so that the camera will always be pointed at the daylit portion of the earth.
            EarthCentralBody earth = CentralBodiesFacet.GetFromContext().Earth;
            SunCentralBody sun = CentralBodiesFacet.GetFromContext().Sun;
            VectorTrueDisplacement earthToSunVector = new VectorTrueDisplacement(earth.CenterOfMassPoint, sun.CenterOfMassPoint);
            VectorEvaluator earthToSunEvaluator = earthToSunVector.GetEvaluator();
            Cartesian earthToSunCartesian = earthToSunEvaluator.Evaluate(new JulianDate(m_defaultStart));
            UnitCartesian earthToSunUnitCartesian = new UnitCartesian(earthToSunCartesian);
            UnitCartesian cameraDirection = new UnitCartesian(earthToSunUnitCartesian.Invert());
            Cartesian cameraPosition = new Cartesian(earthToSunUnitCartesian.X * 50000000, earthToSunUnitCartesian.Y * 50000000, earthToSunUnitCartesian.Z * 50000000);
            m_insight3D.Scene.Camera.Position = cameraPosition;
            m_insight3D.Scene.Camera.Direction = cameraDirection;

            // Call method that creates and sets up targeted segment list with optimizer.
            m_targetedSegmentList = TrajectoryHelper.CreateTrajectoryOptimizer(m_animation.StartTime);
            m_segmentPropagator = m_targetedSegmentList.GetSegmentPropagator(new EvaluatorGroup());
        }

        public static string DataPath
        {
            get { return Path.Combine(Application.StartupPath, "Data"); }
        }

        public static string GetDataFilePath(string path)
        {
            return Path.Combine(DataPath, path);
        }

        /// <summary>
        /// Switch the camera to view the primary satellite.
        /// </summary>
        public void Satellite()
        {
            if (m_platform == null)
                return;
            var fixedOffset = new PointFixedOffset(m_platform.ReferenceFrame, new Cartesian(10000000.0, 10000000.0, 0.0));
            m_insight3D.Scene.Camera.View(m_platform.OrientationAxes, fixedOffset, m_platform.LocationPoint);
        }

        /// <summary>
        /// This gets called every time the time is changed in Insight3D.
        /// </summary>
        private void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            m_display.Update(e.Time);
            SetText(e.Time.ToGregorianDate().ToString(DateFormat));
        }

        /// <summary>
        /// Update date on the text panel.
        /// </summary>
        private void SetText(string text)
        {
            Size textSize = Insight3DHelper.MeasureString(text, m_dateTimeFont);
            Bitmap textBitmap = new Bitmap(textSize.Width, textSize.Height);
            Graphics gfx = Graphics.FromImage(textBitmap);
            gfx.DrawString(text, m_dateTimeFont, Brushes.LightGreen, new PointF(0, 0));
            m_textOverlay.Texture = SceneManager.Textures.FromBitmap(textBitmap);
        }

        /// <summary>
        /// Runs one iteration of the optimizer and displays the results below.
        /// </summary>
        /// <param name="sender">What fired this event.</param>
        /// <param name="e">Additional information about this event.</param>
        private void runIterationButton_Click(object sender, EventArgs e)
        {
            RunOneIteration();
            m_iterationNumber++;
        }

        /// <summary>
        /// Resets the optimizer back to its original state and clears all displayed results.
        /// </summary>
        /// <param name="sender">What fired this event.</param>
        /// <param name="e">Additional information about this event.</param>
        private void resetButton_Click(object sender, EventArgs e)
        {
            // Clean up the previous run's graphics.
            foreach (Primitive primitive in m_primitivesAddedToScene)
            {
                SceneManager.Primitives.Remove(primitive);
            }
            m_primitivesAddedToScene.Clear();
            if (m_platform != null)
            {
                m_display.ServiceProviders.Remove(m_platform);
            }

            // Get new segment propagator from targeted segment list to reset.
            m_segmentPropagator = m_targetedSegmentList.GetSegmentPropagator(new EvaluatorGroup());
            runIterationButton.Enabled = true;
            m_iterationNumber = 0;
        }

        /// <summary>
        /// Takes a screenshot of the Insight3D window and copies it to the clipboard.
        /// </summary>
        /// <param name="sender">What fired this event.</param>
        /// <param name="e">Additional information about this event.</param>
        private void screenshotButton_Click(object sender, EventArgs e)
        {
            m_insight3D.Scene.Camera.Snapshot.SaveToClipboard();
        }

        private void RunOneIteration()
        {
            EarthCentralBody earth = CentralBodiesFacet.GetFromContext().Earth;
            var backgroundCalculation = new BackgroundCalculation();
            TargetedSegmentListResults propagationResults = null;
            backgroundCalculation.DoWork += (sender, e) =>
            {
                #region Propagate Optimizer Iteration
                // Actually propagate.
                propagationResults = (TargetedSegmentListResults)m_segmentPropagator.Propagate((SegmentListResults)null, m_segmentPropagator.OriginalConfiguration, backgroundCalculation);
                #endregion
            };
            backgroundCalculation.RunWorkerCompleted += (sender, e) =>
            {
                // When finished, draw the satellite and output the iteration results to the table.
                bool converged = DrawSatelliteAndOutputResults(propagationResults, earth.InertialFrame);

                // If not converged, allow the user to run another iteration.
                if (!converged)
                    runIterationButton.Enabled = true;
            };

            runIterationButton.Enabled = false;
            backgroundCalculation.RunWorkerAsync();
        }

        private bool DrawSatelliteAndOutputResults(TargetedSegmentListResults propagationResults, ReferenceFrame referenceFrame)
        {
            // Clean up the previous run's graphics.
            foreach (Primitive primitive in m_primitivesAddedToScene)
            {
                SceneManager.Primitives.Remove(primitive);
            }
            m_primitivesAddedToScene.Clear();
            if (m_platform != null)
            {
                m_display.ServiceProviders.Remove(m_platform);
            }

            // Clean up output table from previous run.
            OutputTable.Items.Clear();

            #region Extract Optimizer Results
            // Extract information from propagation results.
            // Outer optimizer results are extracted first.
            var outerOptimizerOperatorResults = (TargetedSegmentListParameterOptimizerResults)propagationResults.OperatorResults[0];
            bool converged = outerOptimizerOperatorResults.Converged;

            var outerOptimizerResults = outerOptimizerOperatorResults.OptimizerResults;
            var outerFinalIterationResults = outerOptimizerResults.FinalIteration;

            var outerFunctionResults = outerFinalIterationResults.FunctionResult;
            double? outerCostFunctionResults = outerFunctionResults.GetCostFunctionValue();

            double[] outerInequalityResults = outerFunctionResults.GetInequalityConstraintValues();
            double radiusPeriapsisAfterManeuver1 = outerInequalityResults[0];
            double targetedInclinationAfterManeuver1 = Trig.RadiansToDegrees(outerInequalityResults[1]);
            double radiusPeriapsisAfterManeuver2 = outerInequalityResults[2];
            double targetedInclinationAfterManeuver2 = Trig.RadiansToDegrees(outerInequalityResults[4]);

            // Extract results for nested maneuver 1 targeted segment list.
            var maneuver1TargetedSegmentResults = (TargetedSegmentListResults)propagationResults.AdditionalSegmentResults[2];
            var maneuver1OperatorResults = (TargetedSegmentListDifferentialCorrectorResults)maneuver1TargetedSegmentResults.OperatorResults[0];
            bool maneuver1Converged = maneuver1OperatorResults.Converged;

            var maneuver1FunctionSolverResults = maneuver1OperatorResults.FunctionSolverResults;
            var maneuver1FinalIterationResults = maneuver1FunctionSolverResults.FinalIteration;
            var maneuver1FunctionResult = maneuver1FinalIterationResults.FunctionResult;
            double[] deltaV1Results = maneuver1FunctionResult.GetVariablesUsed();
            double deltaV1Magnitude = Math.Sqrt(deltaV1Results[0] * deltaV1Results[0] + deltaV1Results[1] * deltaV1Results[1]);

            // Extract results for nested maneuver 2 targeted segment list.
            var maneuver2TargetedSegmentResults = (TargetedSegmentListResults)propagationResults.AdditionalSegmentResults[5];
            var maneuver2OperatorResults = (TargetedSegmentListDifferentialCorrectorResults)maneuver2TargetedSegmentResults.OperatorResults[0];
            bool maneuver2Converged = maneuver2OperatorResults.Converged;

            var maneuver2FunctionSolverResults = maneuver2OperatorResults.FunctionSolverResults;
            var maneuver2FinalIterationResults = maneuver2FunctionSolverResults.FinalIteration;
            var maneuver2FunctionResult = maneuver2FinalIterationResults.FunctionResult;
            double[] deltaV2Results = maneuver2FunctionResult.GetVariablesUsed();
            double deltaV2Magnitude = Math.Sqrt(deltaV2Results[0] * deltaV2Results[0] + deltaV2Results[1] * deltaV2Results[1]);

            // Extract results for nested maneuver 3 targeted segment list.
            var maneuver3TargetedSegmentResults = (TargetedSegmentListResults)propagationResults.AdditionalSegmentResults[8];
            var maneuver3OperatorResults = (TargetedSegmentListDifferentialCorrectorResults)maneuver3TargetedSegmentResults.OperatorResults[0];
            bool maneuver3Converged = maneuver3OperatorResults.Converged;

            var maneuver3FunctionSolverResults = maneuver3OperatorResults.FunctionSolverResults;
            var maneuver3FinalIterationResults = maneuver3FunctionSolverResults.FinalIteration;
            var maneuver3FunctionResult = maneuver3FinalIterationResults.FunctionResult;
            double[] deltaV3Results = maneuver3FunctionResult.GetVariablesUsed();
            double deltaV3Magnitude = Math.Sqrt(deltaV3Results[0] * deltaV3Results[0] + deltaV3Results[1] * deltaV3Results[1]);

            double totalDeltaV = deltaV1Magnitude + deltaV2Magnitude + deltaV3Magnitude;

            // Extract ephemeris results for each propagate segment.
            var overallTrajectoryResults = propagationResults.GetDateMotionCollectionOfOverallTrajectory<Cartesian>(m_elementID);
            var individualSegmentResults = new List<DateMotionCollection<Cartesian>>
            {
                propagationResults.AdditionalSegmentResults[1].GetDateMotionCollectionOfOverallTrajectory<Cartesian>(m_elementID),
                propagationResults.AdditionalSegmentResults[3].GetDateMotionCollectionOfOverallTrajectory<Cartesian>(m_elementID),
                propagationResults.AdditionalSegmentResults[4].GetDateMotionCollectionOfOverallTrajectory<Cartesian>(m_elementID),
                propagationResults.AdditionalSegmentResults[6].GetDateMotionCollectionOfOverallTrajectory<Cartesian>(m_elementID),
                propagationResults.AdditionalSegmentResults[7].GetDateMotionCollectionOfOverallTrajectory<Cartesian>(m_elementID),
                propagationResults.AdditionalSegmentResults[9].GetDateMotionCollectionOfOverallTrajectory<Cartesian>(m_elementID),
            };

            int ephemerisCount = overallTrajectoryResults.Count;
            Motion<Cartesian> finalState = overallTrajectoryResults.Motions[ephemerisCount - 1];
            double earthGravitationalParameter = WorldGeodeticSystem1984.GravitationalParameter;
            var finalKeplerianElements = new KeplerianElements(finalState, earthGravitationalParameter);

            #endregion

            // Draw the orbit using different colors for each propagated segment.
            var colorList = new List<Color> { Color.Chartreuse, Color.Red, Color.Cyan, Color.Yellow, Color.Magenta, Color.Blue };
            for (int i = 0; i < colorList.Count; i++)
            {
                PlotEphemeris(individualSegmentResults[i], colorList[i]);
            }

            // Set up platform and marker for satellite's dynamic positions.
            PlotSatellite(overallTrajectoryResults, individualSegmentResults);

            // Populate table with results.
            OutputTable.Items.Add(new ListViewItem(new[] { "Iteration Number:", m_iterationNumber.ToString() }));
            OutputTable.Items.Add(new ListViewItem(new[] { "Optimizer Converged?", converged.ToString() }));
            OutputTable.Items.Add(new ListViewItem(new[] { "Total propellant usage:", outerCostFunctionResults.Value.ToString() + " kg" }));
            OutputTable.Items.Add(new ListViewItem(new[] { "Total delta-v:", totalDeltaV.ToString() + " m/s" }));

            OutputTable.Items.Add(new ListViewItem(new[] { "", "" }));
            OutputTable.Items.Add(new ListViewItem(new[] { "Initial Rp:", (6678000.0 / 1000.0).ToString() + " km" }));
            OutputTable.Items.Add(new ListViewItem(new[] { "Initial i:", (57.0).ToString() + " deg." }));

            OutputTable.Items.Add(new ListViewItem(new[] { "", "" }));
            OutputTable.Items.Add(new ListViewItem(new[] { "Maneuver 1 delta-v:", deltaV1Magnitude.ToString() + " m/s" }));
            OutputTable.Items.Add(new ListViewItem(new[] { "Rp after Maneuver 1:", (radiusPeriapsisAfterManeuver1 / 1000.0).ToString() + " km" }));
            OutputTable.Items.Add(new ListViewItem(new[] { "i after Maneuver 1:", targetedInclinationAfterManeuver1.ToString() + " deg." }));

            OutputTable.Items.Add(new ListViewItem(new[] { "", "" }));
            OutputTable.Items.Add(new ListViewItem(new[] { "Maneuver 2 delta-v:", deltaV2Magnitude.ToString() + " m/s" }));
            OutputTable.Items.Add(new ListViewItem(new[] { "Rp after Maneuver 2:", (radiusPeriapsisAfterManeuver2 / 1000.0).ToString() + " km" }));
            OutputTable.Items.Add(new ListViewItem(new[] { "i after Maneuver 2:", targetedInclinationAfterManeuver2.ToString() + " deg." }));

            OutputTable.Items.Add(new ListViewItem(new[] { "", "" }));
            OutputTable.Items.Add(new ListViewItem(new[] { "Maneuver 3 delta-v:", deltaV3Magnitude.ToString() + " m/s" }));
            OutputTable.Items.Add(new ListViewItem(new[] { "a after Maneuver 3:", (finalKeplerianElements.SemimajorAxis / 1000.0).ToString() + " km" }));
            OutputTable.Items.Add(new ListViewItem(new[] 
            {
                "i after Maneuver 3:",
                Trig.RadiansToDegrees(finalKeplerianElements.Inclination).ToString() + " deg." 
            }));

            return converged;
        }

        private void PlotEphemeris(DateMotionCollection<Cartesian> ephemeris, Color color)
        {
            EarthCentralBody earth = CentralBodiesFacet.GetFromContext().Earth;
            List<PathPoint> points = new List<PathPoint>();
            for (int i = 0; i < ephemeris.Count; i++)
            {
                points.Add(new PathPointBuilder(ephemeris.Values[i], ephemeris.Dates[i], color).ToPathPoint());
            }
            PathPrimitive path = new PathPrimitive { ReferenceFrame = earth.InertialFrame };
            path.AddRangeToFront(points);
            SceneManager.Primitives.Add(path);
            m_primitivesAddedToScene.Add(path);
        }

        private void PlotSatellite(DateMotionCollection<Cartesian> ephemeris, List<DateMotionCollection<Cartesian>> individualSegmentEphemeris)
        {
            // Put a marker where the satellite is at a given time.
            EarthCentralBody earth = CentralBodiesFacet.GetFromContext().Earth;
            LagrangePolynomialApproximation interpolationAlgorithm = new LagrangePolynomialApproximation();

            // Need to give interpolator segment boundary times to avoid issues with interpolating ephemeris from segment list.
            var segmentBoundaryTimes = new List<JulianDate>
            {
                individualSegmentEphemeris[0].Dates[0],
                individualSegmentEphemeris[1].Dates[0],
                individualSegmentEphemeris[2].Dates[0],
                individualSegmentEphemeris[3].Dates[0],
                individualSegmentEphemeris[4].Dates[0],
                individualSegmentEphemeris[5].Dates[0],
            };

            TranslationalMotionInterpolator interpolator = new TranslationalMotionInterpolator(interpolationAlgorithm, 12, ephemeris, segmentBoundaryTimes);
            m_platform = new Platform
            {
                LocationPoint = new PointInterpolator(earth.InertialFrame, interpolator),
                Name = "GEO satellite",
                OrientationAxes = earth.InertialFrame.Axes,
            };

            Texture2D satelliteTexture = SceneManager.Textures.FromUri(Path.Combine(Application.StartupPath, @"Data/Markers/smallsatellite.png"));

            Uri modelUri = new Uri(Path.Combine(Application.StartupPath, @"Models/satellite.mdl"));

            // Create a point for each satellite.
            PointGraphics pointGraphics = new PointGraphics
            {
                Color = new ConstantGraphicsParameter<Color>(Color.Red),
                DisplayOutline = new ConstantGraphicsParameter<bool>(true),
                PixelSize = new ConstantGraphicsParameter<float>(4.0f),
                OutlineWidth = new ConstantGraphicsParameter<float>(2.0f),
                // Show the point when the camera is very far away.
                DisplayParameters =
                {
                    MinimumDistance = new ConstantGraphicsParameter<double>(750000000.0),
                }
            };
            m_platform.Extensions.Add(new PointGraphicsExtension(pointGraphics));

            // Create a marker for each satellite.
            MarkerGraphics markerGraphics = new MarkerGraphics
            {
                Texture = new ConstantGraphicsParameter<Texture2D>(satelliteTexture),
                // Show the marker when the camera is at a medium distance. 
                DisplayParameters =
                {
                    MinimumDistance = new ConstantGraphicsParameter<double>(17500000.0),
                    MaximumDistance = pointGraphics.DisplayParameters.MinimumDistance,
                }
            };
            m_platform.Extensions.Add(new MarkerGraphicsExtension(markerGraphics));

            // Create a model for the satellite.
            ModelGraphics modelGraphics = new ModelGraphics
            {
                Scale = new ConstantGraphicsParameter<double>(50000.0),
                Uri = new ConstantGraphicsParameter<Uri>(modelUri),
                // Show the model when the camera is close.
                DisplayParameters =
                {
                    MaximumDistance = markerGraphics.DisplayParameters.MinimumDistance,
                }
            };
            m_platform.Extensions.Add(new ModelGraphicsExtension(modelGraphics));

            // Create a label for the satellite.
            TextGraphics textGraphics = new TextGraphics
            {
                Color = pointGraphics.Color,
                Font = new ConstantGraphicsParameter<Font>(new Font("Times New Roman", 8)),
                Outline = new ConstantGraphicsParameter<bool>(true),
                OutlineColor = new ConstantGraphicsParameter<Color>(Color.Black),
                Text = new ConstantGraphicsParameter<string>(m_platform.Name),
                Origin = new ConstantGraphicsParameter<Origin>(Origin.TopCenter),
                PixelOffset = new ConstantGraphicsParameter<PointF>(new PointF(0, -8)),
                // Show the label when at a medium distance.
                DisplayParameters =
                {
                    MaximumDistance = pointGraphics.DisplayParameters.MinimumDistance,
                }
            };
            m_platform.Extensions.Add(new TextGraphicsExtension(textGraphics));

            m_display.ServiceProviders.Add(m_platform);
            m_display.ApplyChanges();

            // Set the date to the start of the ephemeris.
            SceneManager.Animation.Time = ephemeris.Dates[0].AddSeconds(1.0);
        }

        private readonly Insight3D m_insight3D;
        private readonly OverlayToolbar m_overlayToolbar;
        private readonly TextureScreenOverlay m_textOverlay;
        private readonly ServiceProviderDisplay m_display;
        private readonly SimulationAnimation m_animation;

        private readonly List<Primitive> m_primitivesAddedToScene = new List<Primitive>();
        private Platform m_platform;

        private readonly GregorianDate m_defaultStart;
        private readonly GregorianDate m_defaultEnd;
        private readonly TargetedSegmentList m_targetedSegmentList;
        private SegmentPropagator m_segmentPropagator;
        private int m_iterationNumber;

        private readonly Font m_dateTimeFont;
        private const string DateFormat = "MMM dd, yyyy HH:mm:ss";
        private readonly string m_elementID;
    }
}
