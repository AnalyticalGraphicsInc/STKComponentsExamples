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
using AGI.Foundation.Propagators;
using AGI.Foundation.Stk;
using AGI.Foundation.Time;

namespace AGI.Examples.LotsOfSatellites
{
    public partial class LotsOfSatellites : Form
    {
        public LotsOfSatellites()
        {
            InitializeComponent();

            m_insight3D = new Insight3D();
            m_insight3D.Dock = DockStyle.Fill;
            m_insight3DPanel.Controls.Add(m_insight3D);

            m_animation = new SimulationAnimation();
            SceneManager.Animation = m_animation;

            SceneManager.TimeChanged += OnTimeChanged;
        }

        public static string DataPath
        {
            get { return Path.Combine(Application.StartupPath, "Data"); }
        }

        public static string GetDataFilePath(string path)
        {
            return Path.Combine(DataPath, path);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            // Create overlay toolbar and panels
            m_overlayToolbar = new OverlayToolbar(m_insight3D);
            m_overlayToolbar.Overlay.Origin = ScreenOverlayOrigin.BottomCenter;

            // Add additional toolbar buttons 

            // Number of Satellites Button
            m_overlayToolbar.AddButton(GetDataFilePath("Textures/OverlayToolbar/manysatellites.png"),
                                       GetDataFilePath("Textures/OverlayToolbar/fewsatellites.png"),
                                       ToggleNumberOfSatellites);

            // Show/Hide Access Button
            m_overlayToolbar.AddButton(GetDataFilePath("Textures/OverlayToolbar/noshowaccess.png"),
                                       GetDataFilePath("Textures/OverlayToolbar/showaccess.png"),
                                       ToggleComputeAccess);

            // Initialize the text panel
            m_textPanel = new TextureScreenOverlay(0, 0, 80, 35)
            {
                Origin = ScreenOverlayOrigin.TopRight,
                BorderSize = 2,
                BorderColor = Color.Transparent,
                BorderTranslucency = 0.6f,
                Color = Color.Transparent,
                Translucency = 0.4f
            };
            SceneManager.ScreenOverlays.Add(m_textPanel);

            // Show label for the moon
            Scene scene = m_insight3D.Scene;
            scene.CentralBodies[CentralBodiesFacet.GetFromContext().Moon].ShowLabel = true;

            // Create a marker primitive for the facility at Bells Beach Australia
            EarthCentralBody earth = CentralBodiesFacet.GetFromContext().Earth;

            Cartographic facilityPosition = new Cartographic(Trig.DegreesToRadians(144.2829), Trig.DegreesToRadians(-38.3697), 0.0);

            Texture2D facilityTexture = SceneManager.Textures.FromUri(GetDataFilePath(@"Markers\Facility.png"));

            MarkerBatchPrimitive marker = new MarkerBatchPrimitive(SetHint.Infrequent)
            {
                Texture = facilityTexture
            };
            marker.Set(new[] { earth.Shape.CartographicToCartesian(facilityPosition) });

            SceneManager.Primitives.Add(marker);

            PointCartographic point = new PointCartographic(earth, facilityPosition);
            Axes topographic = new AxesNorthEastDown(earth, point);
            ReferenceFrame facilityTopo = new ReferenceFrame(point, topographic);

            m_fixedToFacilityTopoEvaluator = GeometryTransformer.GetReferenceFrameTransformation(earth.FixedFrame, facilityTopo);
            Axes temeAxes = earth.TrueEquatorMeanEquinoxFrame.Axes;
            m_temeToFixedEvaluator = GeometryTransformer.GetAxesTransformation(temeAxes, earth.FixedFrame.Axes);
            m_showAccess = true;
            m_satellites = new Satellites();
            CreateSatellites("stkSatDb");

            // This Render() is needed so that the stars will show.
            scene.Render();
        }

        /// <summary>
        /// Load TLEs from a satellite database and create marker primitives for each satellite
        /// </summary>
        private void CreateSatellites(string fileName)
        {
            m_satellites.Clear();

            JulianDate? epoch = null;

            StkSatelliteDatabase db = new StkSatelliteDatabase(GetDataFilePath("SatelliteDatabase"), fileName);
            foreach (StkSatelliteDatabaseEntry entry in db.GetEntries())
            {
                if (entry.TwoLineElementSet != null)
                {
                    Sgp4Propagator propagator = new Sgp4Propagator(entry.TwoLineElementSet);

                    if (epoch == null)
                    {
                        epoch = propagator.InitialConditions.Epoch;
                    }

                    Duration epochDifference = epoch.Value - propagator.InitialConditions.Epoch;
                    if (epochDifference < Duration.FromDays(1))
                    {
                        m_satellites.Add(propagator.GetEvaluator(), entry.TwoLineElementSet.Epoch);
                    }
                }
            }

            SetText(m_satellites.Count);

            JulianDate time = epoch.Value.ToTimeStandard(TimeStandard.InternationalAtomicTime);

            // Set epoch time
            m_animation.Pause();
            m_animation.StartTime = time;
            m_animation.EndTime = time.AddDays(1.0);
            m_animation.Time = time;
            m_animation.PlayForward();
        }

        private void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            if (m_temeToFixedEvaluator == null)
                return;

            JulianDate date = e.Time;
            Matrix3By3 temeToFixed = new Matrix3By3(m_temeToFixedEvaluator.Evaluate(date));

            KinematicTransformation transformation = m_fixedToFacilityTopoEvaluator.Evaluate(date, 0);

            List<int> satellitesToRemove = null;

            m_satellites.ClearPositions();

            for (int i = 0; i < m_satellites.Count; ++i)
            {
                MotionEvaluator<Cartesian> satellite = m_satellites.GetSatellite(i);

                try
                {
                    // Update position of marker representing this satellite
                    Cartesian position = satellite.Evaluate(date).Rotate(temeToFixed);

                    // Compute access from satellite to facility
                    if (m_showAccess)
                    {
                        Cartesian positionInTopo = transformation.Transform(position);
                        AzimuthElevationRange azimuthElevationRange = new AzimuthElevationRange(positionInTopo);
                        m_satellites.AppendPosition(position, azimuthElevationRange.Elevation > 0.0);
                    }
                    else
                    {
                        m_satellites.AppendPosition(position, false);
                    }
                }
                catch (Exception)
                {
                    if (satellitesToRemove == null)
                    {
                        satellitesToRemove = new List<int>();
                    }

                    satellitesToRemove.Add(i);
                }
            }

            // Remove satellites that could not be evaluated
            if (satellitesToRemove != null)
            {
                m_satellites.RemoveUsingIndexList(satellitesToRemove);
                SetText(m_satellites.Count);
            }

            m_satellites.SetMarkerBatches();
        }

        /// <summary>
        /// Update the number of satellites on the text panel
        /// </summary>
        private void SetText(int number)
        {
            if (m_textOverlay != null)
            {
                m_textPanel.Overlays.Remove(m_textOverlay);
                m_textOverlay = null;
            }

            Font font = new Font("Arial", 10, FontStyle.Bold);
            string text = "Satellites:\n" + number;
            Size textSize = Insight3DHelper.MeasureString(text, font);
            Bitmap textBitmap = new Bitmap(textSize.Width, textSize.Height);
            Graphics gfx = Graphics.FromImage(textBitmap);
            gfx.DrawString(text, font, Brushes.Black, new PointF(0, 0));
            m_textOverlay = new TextureScreenOverlay(0, 0, textSize.Width, textSize.Height)
            {
                Origin = ScreenOverlayOrigin.Center,
                Texture = SceneManager.Textures.FromBitmap(textBitmap)
            };
            m_textPanel.Overlays.Add(m_textOverlay);
        }

        // Button actions
        public void ToggleNumberOfSatellites()
        {
            m_showAllSatellites = !m_showAllSatellites;

            if (m_showAllSatellites)
            {
                CreateSatellites("stkAllTLE");
            }
            else
            {
                CreateSatellites("stkSatDb");
            }
        }

        public void ToggleComputeAccess()
        {
            m_showAccess = !m_showAccess;

            if (!m_showAccess)
            {
                m_satellites.ClearAccesses();
            }
        }

        private readonly Insight3D m_insight3D;
        private readonly SimulationAnimation m_animation;

        private ReferenceFrameEvaluator m_fixedToFacilityTopoEvaluator;
        private AxesEvaluator m_temeToFixedEvaluator;

        private Satellites m_satellites;
        private OverlayToolbar m_overlayToolbar;
        private TextureScreenOverlay m_textPanel;
        private TextureScreenOverlay m_textOverlay;

        private bool m_showAllSatellites = false;
        private bool m_showAccess = true;
    }
}
