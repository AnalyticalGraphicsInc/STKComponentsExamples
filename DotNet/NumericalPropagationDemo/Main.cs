using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AGI.Foundation;
using AGI.Foundation.Celestial;
using AGI.Foundation.Coordinates;
using AGI.Foundation.Geometry;
using AGI.Foundation.Graphics;
using AGI.Foundation.Graphics.Advanced;
using AGI.Foundation.Graphics.Renderer;
using AGI.Foundation.Infrastructure.Threading;
using AGI.Foundation.NumericalMethods;
using AGI.Foundation.NumericalMethods.Advanced;
using AGI.Foundation.Platforms;
using AGI.Foundation.Propagators;
using AGI.Foundation.Time;

namespace AGI.Examples
{
    /// <summary>
    /// The main class for this demo.
    /// </summary>
    public partial class Main : Form
    {
        /// <summary>
        /// Construct a default instance.
        /// </summary>
        public Main()
        {
            InitializeComponent();

            m_insight3D = new Insight3D();
            m_insight3D.Dock = DockStyle.Fill;
            m_insight3DPanel.Controls.Add(m_insight3D);

            m_defaultStart = GregorianDate.Now;
            m_defaultEnd = m_defaultStart.AddDays(1);

            m_animation = new SimulationAnimation();
            SceneManager.Animation = m_animation;

            m_display = new ServiceProviderDisplay();

            m_forceModelSettings = new ForceModelSettings(s_jplData, GetDataFilePath("EarthGravityFile_EGM2008.grv"));
            m_integratorSettings = new IntegratorSettings();
            m_area.Text = "20";
            m_mass.Text = "500";

            // Create overlay toolbar and panels
            m_overlayToolbar = new OverlayToolbar(m_insight3D);
            m_overlayToolbar.Overlay.Origin = ScreenOverlayOrigin.BottomCenter;

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

            // Set the start and stop times
            m_start.CustomFormat = DateFormat;
            m_end.CustomFormat = DateFormat;

            m_start.Text = m_defaultStart.ToString(DateFormat);
            m_end.Text = m_defaultEnd.ToString(DateFormat);

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
        /// This gets called every time the time is changed in Insight3D.
        /// </summary>
        private void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            m_display.Update(e.Time);
            SetText(e.Time.ToGregorianDate().ToString(DateFormat));
        }

        /// <summary>
        /// Display the NumericalIntegrator properties.
        /// </summary>
        /// <param name="sender">What fired this event.</param>
        /// <param name="e">Additional information about this event.</param>
        private void OnIntegratorClick(object sender, EventArgs e)
        {
            m_integratorSettings.ShowDialog(this);
            BringToFront();
        }

        /// <summary>
        /// Display the ForceModel's properties.
        /// </summary>
        /// <param name="sender">What fired this event.</param>
        /// <param name="e">Additional information about this event.</param>
        private void OnForceModelsClick(object sender, EventArgs e)
        {
            m_forceModelSettings.ShowDialog(this);
            BringToFront();
        }

        /// <summary>
        /// Propagate the satellite and displays it in the Insight3D window.
        /// </summary>
        /// <param name="sender">What fired this event.</param>
        /// <param name="e">Additional information about this event.</param>
        private void OnPropagateClick(object sender, EventArgs e)
        {
            try
            {
                m_animation.StartTime = new JulianDate(m_start.Value);
                m_animation.EndTime = new JulianDate(m_end.Value);

                m_propagationProgress.Value = 0;
                PropagateSatellite();
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error propagating the entered elements. Make sure your satellite " +
                                "does not enter the earth and your inputs are all valid. \n" +
                                "Exception: " + ex.Message);
            }
        }

        /// <summary>
        /// Propagate a Platform using a NumericalPropagator configured with the entered KeplerianElements,
        /// ForceModels, NumericalIntegrator, and start and stop dates.
        /// </summary>
        private void PropagateSatellite()
        {
            m_keplerianOrbitalElementsEntry.GravitationalConstant =
                m_forceModelSettings.CurrentCentralBodysGravitationalParameter;
            KeplerianElements orbitalElements = m_keplerianOrbitalElementsEntry.KeplerianElementValues;
            if (orbitalElements == null)
                return;

            Motion<Cartesian> initialMotion = orbitalElements.ToCartesian();
            PropagationNewtonianPoint point = new PropagationNewtonianPoint(m_elementID, m_forceModelSettings.CurrentCentralBody.InertialFrame, initialMotion.Value, initialMotion.FirstDerivative);
            point.Mass = new ScalarFixed(double.Parse(m_mass.Text));
            m_forceModelSettings.SetForceModelsOnPoint(point, new ScalarFixed(double.Parse(m_area.Text)));
            CentralBody primaryCentralBody = m_forceModelSettings.CurrentCentralBody;

            NumericalPropagatorDefinition state = new NumericalPropagatorDefinition();
            state.IntegrationElements.Add(point);
            state.Integrator = m_integratorSettings.GetIntegrator();

            JulianDate start = new JulianDate(GregorianDate.ParseExact(m_start.Text, DateFormat, null));
            JulianDate end = new JulianDate(GregorianDate.ParseExact(m_end.Text, DateFormat, null));
            state.Epoch = start;
            NumericalPropagator propagator = state.CreatePropagator();
            propagator.StepTaken += (sender, args) =>
            {
                // Telling the propagator to stop if we get too close to the central body
                Cartesian position = propagator.Converter.ConvertState<Cartesian>(m_elementID, args.CurrentState).Value;
                if (position.Magnitude <= primaryCentralBody.Shape.SemimajorAxisLength + 10000)
                {
                    args.Indication = PropagationEventIndication.StopPropagationAfterStep;
                }
            };

            DateMotionCollection<Cartesian> answer = null;

            var backgroundCalculation = new BackgroundCalculation();
            backgroundCalculation.DoWork += (sender, e) =>
            {
                // actually propagate
                var result = propagator.Propagate(end.Subtract(start), 1, backgroundCalculation);
                answer = result.GetDateMotionCollection<Cartesian>(m_elementID);
            };
            backgroundCalculation.ProgressChanged += (sender, e) => m_propagationProgress.Value = e.ProgressPercentage;
            backgroundCalculation.RunWorkerCompleted += (sender, e) =>
            {
                // when finished, draw the satellite
                DrawSatellite(answer, primaryCentralBody.InertialFrame);
                m_propagate.Enabled = true;
            };

            m_propagate.Enabled = false;
            backgroundCalculation.RunWorkerAsync();
        }

        /// <summary>
        /// Draw the ephemeris in the Insight3D window.
        /// </summary>
        /// <param name="ephemeris">The date, position (and velocity) information of the object being displayed.</param>
        /// <param name="inertialFrame">The inertial frame to display the graphics in.</param>
        private void DrawSatellite(DateMotionCollection<Cartesian> ephemeris, ReferenceFrame inertialFrame)
        {
            // Clean up the previous run's graphics
            foreach (Primitive primitive in m_primitivesAddedToScene)
            {
                SceneManager.Primitives.Remove(primitive);
            }
            m_primitivesAddedToScene.Clear();
            if (m_platform != null)
            {
                m_display.ServiceProviders.Remove(m_platform);
            }

            // Draw the orbit
            List<PathPoint> points = new List<PathPoint>();
            for (int i = 0; i < ephemeris.Count; i++)
            {
                points.Add(new PathPointBuilder(ephemeris.Values[i], ephemeris.Dates[i]).ToPathPoint());
            }
            PathPrimitive path = new PathPrimitive { ReferenceFrame = inertialFrame };
            path.AddRangeToFront(points);
            SceneManager.Primitives.Add(path);
            m_primitivesAddedToScene.Add(path);

            // Put a marker where the satellite is at a given time
            LagrangePolynomialApproximation interpolationAlgorithm = new LagrangePolynomialApproximation();
            TranslationalMotionInterpolator interpolator = new TranslationalMotionInterpolator(interpolationAlgorithm, 2, ephemeris);
            m_platform = new Platform
            {
                LocationPoint = new PointInterpolator(inertialFrame, interpolator)
            };

            Texture2D texture = SceneManager.Textures.FromUri(@"Data\Markers\Satellite.png");
            m_platform.Extensions.Add(new MarkerGraphicsExtension(new MarkerGraphics
            {
                Texture = new ConstantGraphicsParameter<Texture2D>(texture)
            }));
            m_display.ServiceProviders.Add(m_platform);
            m_display.ApplyChanges();

            // Set the date to the start of the ephemeris
            SceneManager.Animation.Time = ephemeris.Dates[0];
        }

        /// <summary>
        /// Update date on the text panel
        /// </summary>
        private void SetText(string text)
        {
            Size textSize = Insight3DHelper.MeasureString(text, m_dateTimeFont);
            Bitmap textBitmap = new Bitmap(textSize.Width, textSize.Height);
            Graphics gfx = Graphics.FromImage(textBitmap);
            gfx.DrawString(text, m_dateTimeFont, Brushes.LightGreen, new PointF(0, 0));
            m_textOverlay.Texture = SceneManager.Textures.FromBitmap(textBitmap);
        }

        private static readonly JplDE430 s_jplData;

        static Main()
        {
            // Load data for central bodies besides the earth, moon, and sun.
            s_jplData = new JplDE430(Path.Combine(DataPath, "plneph.430"));
            s_jplData.UseForCentralBodyPositions(CentralBodiesFacet.GetFromContext());
        }

        private readonly Insight3D m_insight3D;
        private readonly OverlayToolbar m_overlayToolbar;
        private readonly TextureScreenOverlay m_textOverlay;
        private readonly ServiceProviderDisplay m_display;
        private readonly ForceModelSettings m_forceModelSettings;
        private readonly IntegratorSettings m_integratorSettings;
        private readonly Font m_dateTimeFont;
        private readonly string m_elementID;
        private readonly List<Primitive> m_primitivesAddedToScene = new List<Primitive>();
        private Platform m_platform;
        private readonly GregorianDate m_defaultStart;
        private readonly GregorianDate m_defaultEnd;
        private const string DateFormat = "MMM dd, yyyy HH:mm:ss";
        private readonly SimulationAnimation m_animation;
    }
}
