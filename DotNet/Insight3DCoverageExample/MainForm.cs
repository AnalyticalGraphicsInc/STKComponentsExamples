using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AGI.Foundation.Celestial;
using AGI.Foundation.Coordinates;
using AGI.Foundation.Coverage;
using System.IO;
using AGI.Foundation.Geometry.Shapes;
using AGI.Foundation.Platforms;
using AGI.Foundation.Geometry;
using AGI.Foundation;
using AGI.Foundation.Geometry.Discrete;
using AGI.Foundation.Access.Constraints;
using AGI.Foundation.Access;
using AGI.Foundation.Time;
using AGI.Foundation.Propagators;
using AGI.Foundation.Graphics;
using AGI.Examples;
using AGI.Foundation.Graphics.Advanced;
using AGI.Foundation.Graphics.Renderer;
//using AGI.Foundation.Cesium;

namespace Spatial_Library_Exercise
{
    public partial class MainForm : Form
    {
        EarthCentralBody m_earth;

        string WorldView1tle = "1 32060U 07041A   15266.66666667  .00003437  00000-0  14005-3 0 00005\n2 32060 097.8627 006.4229 0002401 023.1506 015.4780 15.24528736445897";

        string WorldView3tle = "1 40115U 14048A   15266.66666667  .00000348  00000-0  40864-4 0 00001\n2 40115 097.9289 341.4329 0001032 089.0864 044.3865 14.84989268060245";

        string WorldView2tle = "1 35946U 09055A   15266.66666667  .00000101  00000-0  33772-4 0 00006\n2 35946 098.3819 339.5989 0001882 107.7329 190.7987 14.37674387312626";

        string m_dataPath = Path.Combine(Application.StartupPath, "Data");

        public MainForm()
        {
            JulianDate startTime = new JulianDate(new DateTime(2015, 9, 23, 6, 0, 0));
            InitializeComponent();
            cmbRegion.SelectedIndex = 0;

            m_insight3D = new Insight3D();
            m_insight3D.Dock = DockStyle.Fill;
            m_insight3DPanel.Controls.Add(m_insight3D);

            m_display = new ServiceProviderDisplay();

            m_display.ApplyChanges();

            m_display.Update(SceneManager.Time);

            var animation = new SimulationAnimation
            {
                StartTime = startTime,
                EndTime = startTime.AddDays(1.0),
                TimeStep = Duration.FromSeconds(0.5),
                StartCycle = SimulationAnimationCycle.Loop,
                EndCycle = SimulationAnimationCycle.Loop
            };

            SceneManager.Animation = animation;

            // Subscribe to the time changed event so we can update our display.
            SceneManager.TimeChanged += SceneManagerTimeChanged;

            // Reset to the beginning.
            animation.Reset();

        }

        #region Code

        private void CalculateCoverage()
        {
            var satelliteTexture = SceneManager.Textures.FromUri(Path.Combine(Application.StartupPath, @"..\..\..\Data\Markers\smallsatellite.png"));
            m_earth = CentralBodiesFacet.GetFromContext().Earth;

            string areaTargetName = cmbRegion.Items[cmbRegion.SelectedIndex].ToString() + ".at";            
            //define grid points

            //set up area
            string areaTargetFilePath = Path.Combine(m_dataPath, areaTargetName);
            IList<Cartographic> areaTargetPoints = STKUtil.ReadAreaTargetCartographic(areaTargetFilePath);

            ParameterizedSpatiallyPartitionedCoverageDefinition coverageDefinition = new ParameterizedSpatiallyPartitionedCoverageDefinition();

            //Define border
            var regionBuilder = new SpecifiedNodesEllipsoidSurfaceRegionBuilder(m_earth.Shape, areaTargetPoints);
            EllipsoidSurfaceRegion region = regionBuilder.GetEllipsoidSurfaceRegion();

            // Create the platform template that will be used for each grid point in the coverage computation
            Platform templateGridPoint = new Platform { LocationPoint = coverageDefinition.GridPoint };
            templateGridPoint.OrientationAxes = new AxesEastNorthUp(m_earth, templateGridPoint.LocationPoint);

            // set the platform template
            coverageDefinition.GridPointPlaceholder = templateGridPoint;

            //set up assets
            List<Platform> satellites = GetSatellites();
            foreach (Platform satellite in satellites)
            {
                // use a 5 degree mask angle for the receiver constraint
                ElevationAngleConstraint constraint = new ElevationAngleConstraint(Trig.DegreesToRadians(5.0));
                constraint.ConstrainedLink = new LinkInstantaneous(satellite, coverageDefinition.GridPointPlaceholder);
                constraint.ConstrainedLinkEnd = LinkRole.Receiver;
                // up the max step size to decrease time to calculate access
                constraint.Sampling.MaximumStep = Duration.FromSeconds(3600.0);
                // finally add the assets to the coverage definition
                coverageDefinition.AddAsset(new AssetDefinition(satellite, constraint));


                satellite.Extensions.Add(new MarkerGraphicsExtension
                {
                    MarkerGraphics =
                    {
                        Texture = new ConstantGraphicsParameter<Texture2D>(satelliteTexture)
                    }
                });

                m_display.ServiceProviders.Add(satellite);

            }

            // ensure this calculation uses all threads possible
            coverageDefinition.MultithreadCoverage = true;

            //compute coverage
            double gridResolution = Trig.DegreesToRadians((double)nudResolution.Value);
            coverageDefinition.Grid = new SurfaceRegionsCoverageGrid(gridResolution, m_earth, region);

            // set the start and stop times for the calculation
            JulianDate start = new JulianDate(new DateTime(2015, 9, 23, 6, 0, 0));
            JulianDate stop = new JulianDate(new DateTime(2015, 9, 23, 18, 0, 0));

            // calculate access over the grid - this will take a little time
            try
            {
                CoverageResults gridResult = coverageDefinition.ComputeCoverageOverTheGrid(start, stop);

                //get results
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var gridPoint in gridResult.GridPoints)
                {
                    MotionEvaluator<Cartographic> cartographicEvaluator = m_earth.ObserveCartographicPoint(gridPoint.CoverageGridPoint.Position);
                    Cartographic gridPointLocation = cartographicEvaluator.Evaluate(start);

                    stringBuilder.AppendLine(string.Format("{0:f2} {1:f2} - {2}", Trig.RadiansToDegrees(gridPointLocation.Longitude),
                                                                                  Trig.RadiansToDegrees(gridPointLocation.Latitude),
                                                                                  gridPoint.AssetCoverage.SatisfactionIntervals.Count));
                }
                CreateGridDictionary(gridResult);
                txtResult.Text = stringBuilder.ToString();

                OverlayToolbar animationToolbar = new OverlayToolbar(m_insight3D);
                animationToolbar.Overlay.Origin = ScreenOverlayOrigin.BottomCenter;

                m_display.ApplyChanges();
                m_insight3D.Scene.Render();
            }
            catch {
                Console.WriteLine("No Coverage Assets Selected");
           }
        }

        private void CreateGridDictionary(CoverageResults gridResult)
        {
            JulianDate start = new JulianDate(new DateTime(2015, 9, 23, 6, 0, 0));
            JulianDate stop = new JulianDate(new DateTime(2015, 9, 23, 18, 0, 0));

            EarthCentralBody earth = CentralBodiesFacet.GetFromContext().Earth;
            earth.OrientationParameters = EarthOrientationParametersFile.ReadData(@"C:\ProgramData\AGI\STK 12\DynamicEarthData\EOP-v1.1.txt");

            //create data structures for grid point storage
            Dictionary<(CoverageGridPointWithResults, int), JulianDate> end_time_dicts = new Dictionary<(CoverageGridPointWithResults, int), JulianDate>();
            foreach (var gridPoint in gridResult.GridPoints)
            {
                for (int i = 0; i < gridPoint.AssetCoverage.SatisfactionIntervals.Count - 1; i++)
                {
                    dictionary.Add(gridPoint.AssetCoverage.SatisfactionIntervals[i].Start, (gridPoint.AssetCoverage.SatisfactionIntervals[i].Stop, (gridPoint, i)));
                    end_time_dicts.Add((gridPoint, i), gridPoint.AssetCoverage.SatisfactionIntervals[i].Start);
                }
            }

            key_list = dictionary.Keys.ToList();
            key_list.Sort();

            foreach(var gp in key_list)
            {
                EllipsoidSurfaceRegionCoverageGridCell surfaceCurve = (EllipsoidSurfaceRegionCoverageGridCell)dictionary[gp].Item2.Item1.CoverageGridPoint.CoverageGridCell;

                var surface_curves = surfaceCurve.GridCellBoundary.GetDiscretePoints(1.0);

                CartographicExtent extent =
                new CartographicExtent(surface_curves[0][0], // West
                           surface_curves[2][1], // South
                           surface_curves[1][0], // East
                            surface_curves[0][1]); // North
                SurfaceTriangulatorResult triangles = SurfaceExtentTriangulator.Compute(earth, extent);

                TriangleMeshPrimitive mesh = new TriangleMeshPrimitive(SetHint.Infrequent);
                mesh.Set(triangles);
                mesh.Color = GetColor(dictionary[gp].Item2.Item2);

                if(end_time_dicts.ContainsKey((dictionary[gp].Item2.Item1, dictionary[gp].Item2.Item2 + 1)))
                {
                    mesh.DisplayCondition = new TimeIntervalDisplayCondition(gp, end_time_dicts[(dictionary[gp].Item2.Item1, dictionary[gp].Item2.Item2 + 1)]);
                }
                else
                {
                    mesh.DisplayCondition = new TimeIntervalDisplayCondition(gp, stop);
                }

                SceneManager.Primitives.Add(mesh);
                SceneManager.Render();
            }
            m_display.ApplyChanges();
            m_insight3D.Scene.Render();
        }

        private Color GetColor(int index)
        {
            switch (index)
            {
                case 0: return Color.Red;
                case 1: return Color.Orange;
                case 2: return Color.Yellow;
                case 3: return Color.LightGreen;
                case 4: return Color.Green;
                case 5: return Color.LightBlue;
                case 6: return Color.Blue;
                default:
                    break;
            }
            return Color.White;
        }

        private void SceneManagerTimeChanged(object sender, TimeChangedEventArgs e)
        {
            m_display.Update(e.Time);
        }

        private Platform CreateSatellite(string name, string tle)
        {
            Sgp4Propagator propagator = new Sgp4Propagator(new TwoLineElementSet(tle));

            Platform satellite = new Platform(name);
            satellite.LocationPoint = propagator.CreatePoint();
            satellite.OrientationAxes = new AxesVehicleVelocityLocalHorizontal(propagator.ReferenceFrame, satellite.LocationPoint);

            ComplexConic sensor = new ComplexConic(0, Trig.DegreesToRadians(45), 0, Constants.TwoPi);
            sensor.Radius = double.PositiveInfinity;

            satellite.Extensions.Add(new FieldOfViewExtension(sensor));
            return satellite;
        }

        private List<Platform> GetSatellites()
        {
            List<Platform> satellites = new List<Platform>();

            if (cbWorldView1.Checked)
            {
                satellites.Add(CreateSatellite("WorldView1",WorldView1tle));
            }

            if (cbWorldView2.Checked)
            {
                satellites.Add(CreateSatellite("WorldView2", WorldView2tle));
            }

            if (cbWorldView3.Checked)
            {
                satellites.Add(CreateSatellite("WorldView3", WorldView3tle));
            }

            return satellites;
        }
        #endregion

        private readonly Insight3D m_insight3D;
        private readonly ServiceProviderDisplay m_display;
        private Dictionary<JulianDate, (JulianDate, (CoverageGridPointWithResults, int))> dictionary = new Dictionary<JulianDate, (JulianDate, (CoverageGridPointWithResults, int))>();
        private List<JulianDate> key_list;

        private void btnRun_Click(object sender, EventArgs e)
        {
            CalculateCoverage();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void m_insight3DPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void gbInput_Enter(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void m_insight3DPanel_Paint_1(object sender, PaintEventArgs e)
        {

        }
    }
}
