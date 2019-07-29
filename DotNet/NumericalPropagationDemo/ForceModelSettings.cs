using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using AGI.Foundation.Celestial;
using AGI.Foundation.Geometry;
using AGI.Foundation.Propagators;

namespace AGI.Examples
{
    /// <summary>
    /// Displays the properties of all of the possible ForceModel's that can be configured
    /// for a satellite.
    /// </summary>
    public partial class ForceModelSettings : Form
    {
        /// <summary>
        /// Constructs an instance with the entered properties.
        /// </summary>
        /// <param name="jplInfo">The JplDe info.</param>
        /// <param name="gravityFile">A file specifying the gravitational field for the primary
        /// central body.</param>
        public ForceModelSettings(JplDE jplInfo, string gravityFile)
        {
            InitializeComponent();

            // adding all of the central body gravitational parameters.
            m_gravConstants.Add(JplDECentralBody.Earth.ToString(), jplInfo.GetGravitationalParameter(JplDECentralBody.Earth));
            m_gravConstants.Add(JplDECentralBody.Sun.ToString(), jplInfo.GetGravitationalParameter(JplDECentralBody.Sun));
            m_gravConstants.Add(JplDECentralBody.Moon.ToString(), jplInfo.GetGravitationalParameter(JplDECentralBody.Moon));
            m_gravConstants.Add(JplDECentralBody.Jupiter.ToString(), jplInfo.GetGravitationalParameter(JplDECentralBody.Jupiter));
            m_gravConstants.Add(JplDECentralBody.Mars.ToString(), jplInfo.GetGravitationalParameter(JplDECentralBody.Mars));
            m_gravConstants.Add(JplDECentralBody.Mercury.ToString(), jplInfo.GetGravitationalParameter(JplDECentralBody.Mercury));

            m_gravConstants.Add(JplDECentralBody.Neptune.ToString(), jplInfo.GetGravitationalParameter(JplDECentralBody.Neptune));
            m_gravConstants.Add(JplDECentralBody.Pluto.ToString(), jplInfo.GetGravitationalParameter(JplDECentralBody.Pluto));
            m_gravConstants.Add(JplDECentralBody.Saturn.ToString(), jplInfo.GetGravitationalParameter(JplDECentralBody.Saturn));

            m_gravConstants.Add(JplDECentralBody.Uranus.ToString(), jplInfo.GetGravitationalParameter(JplDECentralBody.Uranus));
            m_gravConstants.Add(JplDECentralBody.Venus.ToString(), jplInfo.GetGravitationalParameter(JplDECentralBody.Venus));

            m_nameToJplMapping.Add(EARTH, JplDECentralBody.Earth);
            m_nameToJplMapping.Add(SUN, JplDECentralBody.Sun);
            m_nameToJplMapping.Add(MOON, JplDECentralBody.Moon);
            m_nameToJplMapping.Add("Jupiter", JplDECentralBody.Jupiter);
            m_nameToJplMapping.Add("Mars", JplDECentralBody.Mars);
            m_nameToJplMapping.Add("Mercury", JplDECentralBody.Mercury);
            m_nameToJplMapping.Add("Neptune", JplDECentralBody.Neptune);
            m_nameToJplMapping.Add("Pluto", JplDECentralBody.Pluto);
            m_nameToJplMapping.Add("Saturn", JplDECentralBody.Saturn);
            m_nameToJplMapping.Add("Uranus", JplDECentralBody.Uranus);
            m_nameToJplMapping.Add("Venus", JplDECentralBody.Venus);
            m_jplInfo = jplInfo;

            // setting default non-spherical gravity parameters.
            m_degree.Text = 21.ToString();
            m_order.Text = 21.ToString();
            m_gravFile.Text = gravityFile;
            m_tides.Items.Add(PERMANENTTIDES);
            m_tides.Items.Add(NOTIDES);
            m_tides.SelectedIndex = 0;
            m_tides.DropDownStyle = ComboBoxStyle.DropDownList;

            // configuring primary central body list.
            m_primaryCB.Items.Add(CentralBodiesFacet.GetFromContext().Earth.Name);
            m_primaryCB.Items.Add(CentralBodiesFacet.GetFromContext().Moon.Name);
            m_primaryCB.DropDownStyle = ComboBoxStyle.DropDownList;
            m_lastCB = EARTH;
            CurrentCentralBody = CentralBodiesFacet.GetFromContext().Earth;
            m_primaryCB.SelectedItem = EARTH;

            // adding all of the possible third bodies
            foreach (KeyValuePair<string, double> pair in m_gravConstants)
            {
                string name = pair.Key;
                if (!m_thirdBodies.Items.Contains(name) && name != CurrentCentralBody.Name)
                {
                    m_thirdBodies.Items.Add(name);
                }
            }
            m_thirdBodies.Items.Remove(EARTH);

            // setting the default Solar Radiation Pressure properties
            m_useSRP.Checked = true;
            foreach (CentralBody body in CentralBodiesFacet.GetFromContext())
            {
                if (body.Name == EARTH || body.Name == SUN || body.Name == MOON)
                {
                    m_eclipsingBodies.Items.Add(body.Name, true);
                }
            }
            m_eclipsingBodies.Items.Remove(SUN);

            m_shadowModel.Items.Add(DUALCONE);
            m_shadowModel.Items.Add(CYLINDRICAL);
            m_shadowModel.SelectedIndex = 0;
            m_shadowModel.DropDownStyle = ComboBoxStyle.DropDownList;
            m_reflectivity.Text = (1.0).ToString();

            // setting the default drag properties
            m_useDrag.Checked = true;

            m_densityModel.Items.Add(JR);
            m_densityModel.Items.Add(MSIS86);
            m_densityModel.Items.Add(MSIS90);
            m_densityModel.Items.Add(MSIS2000);
            m_densityModel.SelectedIndex = 0;
            m_densityModel.DropDownStyle = ComboBoxStyle.DropDownList;

            m_dragCoeff.Text = (2.2).ToString();
            m_kp.Text = (3.0).ToString();
            m_solarFlux.Text = (150).ToString();
            m_averageSolarFlux.Text = (150).ToString();
        }

        /// <summary>
        /// This will configure the entered point with all of the force models as entered in the GUI.
        /// </summary>
        /// <param name="point">The point to add the force models too.</param>
        /// <param name="area">The Scalar representing the area that the SRP and Drag model will see.</param>
        public void SetForceModelsOnPoint(PropagationNewtonianPoint point, Scalar area)
        {
            point.AppliedForces.Clear();
            if (m_useSRP.Checked)
            {
                SimpleSolarRadiationForce srp = new SimpleSolarRadiationForce(point.IntegrationPoint, double.Parse(m_reflectivity.Text), area);
                if (m_shadowModel.SelectedItem.ToString() == DUALCONE)
                {
                    srp.OccultationFactor = new ScalarOccultationDualCone(CentralBodiesFacet.GetFromContext().Sun,
                                                                          point.IntegrationPoint);
                }
                else if (m_shadowModel.SelectedItem.ToString() == CYLINDRICAL)
                {
                    srp.OccultationFactor = new ScalarOccultationCylindrical(CentralBodiesFacet.GetFromContext().Sun,
                                                                             point.IntegrationPoint);
                }
                foreach (string id in m_eclipsingBodies.CheckedItems)
                {
                    srp.OccultationFactor.OccludingBodies.Add(CentralBodiesFacet.GetFromContext().GetByName(id));
                }

                point.AppliedForces.Add(srp);
            }

            if (m_useDrag.Checked)
            {
                ScalarAtmosphericDensity density = null;
                SolarGeophysicalData geoData = new ConstantSolarGeophysicalData(double.Parse(m_averageSolarFlux.Text),
                                                                                double.Parse(m_solarFlux.Text),
                                                                                double.Parse(m_kp.Text));
                if (m_densityModel.SelectedItem.ToString() == JR)
                {
                    density = new ScalarDensityJacchiaRoberts(point.IntegrationPoint, geoData);
                }
                else if (m_densityModel.SelectedItem.ToString() == MSIS86)
                {
                    density = new ScalarDensityMsis86(point.IntegrationPoint, geoData);
                }
                else if (m_densityModel.SelectedItem.ToString() == MSIS90)
                {
                    density = new ScalarDensityMsis90(point.IntegrationPoint, geoData);
                }
                else if (m_densityModel.SelectedItem.ToString() == MSIS2000)
                {
                    density = new ScalarDensityMsis2000(point.IntegrationPoint, geoData);
                }
                AtmosphericDragForce atmo = new AtmosphericDragForce(density, new ScalarFixed(double.Parse(m_dragCoeff.Text)), area);
                point.AppliedForces.Add(atmo);
            }

            ThirdBodyGravity thirdGravity = new ThirdBodyGravity(point.IntegrationPoint);
            foreach (string item in m_thirdBodies.CheckedItems)
            {
                if (item == EARTH || item == SUN || item == MOON)
                {
                    thirdGravity.AddThirdBody(item, CentralBodiesFacet.GetFromContext().GetByName(item).CenterOfMassPoint, m_gravConstants[item]);
                }
                else
                {
                    thirdGravity.AddThirdBody(item, m_jplInfo.GetCenterOfMassPoint(m_nameToJplMapping[item]), m_gravConstants[item]);
                }
            }
            thirdGravity.CentralBody = CentralBodiesFacet.GetFromContext().GetByName(m_primaryCB.SelectedItem.ToString());
            if (thirdGravity.ThirdBodies.Count > 0)
            {
                point.AppliedForces.Add(thirdGravity);
            }

            // Primary gravity
            string primaryCB = m_primaryCB.SelectedItem.ToString();
            if (m_twoBody.Checked)
            {
                TwoBodyGravity gravity = new TwoBodyGravity(point.IntegrationPoint, CentralBodiesFacet.GetFromContext().GetByName(primaryCB), m_gravConstants[primaryCB]);
                point.AppliedForces.Add(gravity);
            }
            else
            {
                SphericalHarmonicsTideType tideType = SphericalHarmonicsTideType.None;
                if (m_tides.SelectedItem.ToString() == NOTIDES)
                    tideType = SphericalHarmonicsTideType.None;
                else if (m_tides.SelectedItem.ToString() == PERMANENTTIDES)
                    tideType = SphericalHarmonicsTideType.PermanentTideOnly;

                int order = int.Parse(m_order.Text);
                int degree = int.Parse(m_degree.Text);
                SphericalHarmonicGravityModel model = SphericalHarmonicGravityModel.ReadFrom(m_gravFile.Text);
                SphericalHarmonicGravity gravity = new SphericalHarmonicGravity(point.IntegrationPoint,
                                                                                new SphericalHarmonicGravityField(model, degree, order, true, tideType));
                point.AppliedForces.Add(gravity);

                if (gravity.GravityField.CentralBody.Name != primaryCB)
                {
                    throw new InvalidDataException("The central body you have selected does not match the central body in the gravity file.");
                }
            }
            if (primaryCB == EARTH)
            {
                point.IntegrationFrame = CentralBodiesFacet.GetFromContext().Earth.InternationalCelestialReferenceFrame;
            }
            else if (primaryCB == MOON)
            {
                point.IntegrationFrame = CentralBodiesFacet.GetFromContext().Moon.InertialFrame;
            }
        }

        /// <summary>
        /// Gets the gravitational parameter of the CurrentCentralBody.
        /// </summary>
        public double CurrentCentralBodysGravitationalParameter
        {
            get { return m_gravitationalParameter; }
            private set { m_gravitationalParameter = value; }
        }

        /// <summary>
        /// Gets the primary central body that the satellite will be propagated around.
        /// </summary>
        public CentralBody CurrentCentralBody
        {
            get { return m_centralBody; }
            private set { m_centralBody = value; }
        }

        /// <summary>
        /// Closes the form.
        /// </summary>
        /// <param name="sender">What fired this event.</param>
        /// <param name="e">Additional information about this event.</param>
        private void OnDoneClick(object sender, EventArgs e)
        {
            Hide();
        }

        /// <summary>
        /// Handles updating the various central body lists if the primary central body changes.
        /// </summary>
        /// <param name="sender">What fired this event.</param>
        /// <param name="e">Additional information about this event.</param>
        private void OnPrimaryCBSelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_primaryCB.SelectedItem.ToString() != m_lastCB)
            {
                m_thirdBodies.Items.Remove(m_primaryCB.SelectedItem.ToString());
                m_thirdBodies.Items.Add(m_lastCB);
                m_lastCB = m_primaryCB.SelectedItem.ToString();
                if (m_primaryCB.SelectedItem.ToString() != EARTH)
                {
                    // we only have drag models for the earth
                    m_lastDragChecked = m_useDrag.Checked;
                    m_useDrag.Checked = false;
                    m_useDrag.Enabled = false;
                }
                else
                {
                    m_useDrag.Checked = m_lastDragChecked;
                    m_useDrag.Enabled = true;
                }
                CurrentCentralBodysGravitationalParameter = m_gravConstants[m_primaryCB.SelectedItem.ToString()];
                CurrentCentralBody = CentralBodiesFacet.GetFromContext().GetByName(m_primaryCB.SelectedItem.ToString());
            }
        }

        /// <summary>
        /// Opens a file find dialog for locating the spherical harmonic gravity file.
        /// </summary>
        /// <param name="sender">What fired this event.</param>
        /// <param name="e">Additional information about this event.</param>
        private void OnFindGravityFileClick(object sender, EventArgs e)
        {
            OpenFileDialog findGravity = new OpenFileDialog();
            findGravity.Multiselect = false;
            findGravity.InitialDirectory = Path.GetFullPath(Main.DataPath);
            string currentWorkingDir = Environment.CurrentDirectory;
            DialogResult result = findGravity.ShowDialog();

            if (result == DialogResult.OK)
            {
                Environment.CurrentDirectory = currentWorkingDir;
                m_gravFile.Text = findGravity.FileName;
            }
        }

        /// <summary>
        /// Handles disabling/enabling widgets based on the type of primary gravity force model selection.
        /// </summary>
        /// <param name="sender">What called this method.</param>
        /// <param name="e">Relevent arguments for this event.</param>
        private void OnTwoBodyCheckedChanged(object sender, EventArgs e)
        {
            m_gravFile.Enabled = !m_twoBody.Checked;
            m_degree.Enabled = !m_twoBody.Checked;
            m_order.Enabled = !m_twoBody.Checked;
            m_tides.Enabled = !m_twoBody.Checked;
            m_findGravityFile.Enabled = !m_twoBody.Checked;
        }

        /// <summary>
        /// Handles disabling/enabling drag widgets for when drag is turned on/off.
        /// </summary>
        /// <param name="sender">What called this method.</param>
        /// <param name="e">Relevent arguments for this event.</param>
        private void OnUseDragCheckedChanged(object sender, EventArgs e)
        {
            m_averageSolarFlux.Enabled = m_useDrag.Checked;
            m_densityModel.Enabled = m_useDrag.Checked;
            m_dragCoeff.Enabled = m_useDrag.Checked;
            m_kp.Enabled = m_useDrag.Checked;
            m_solarFlux.Enabled = m_useDrag.Checked;
        }

        private void OnUseSRPCheckedChanged(object sender, EventArgs e)
        {
            m_shadowModel.Enabled = m_useSRP.Checked;
            m_reflectivity.Enabled = m_useSRP.Checked;
            m_eclipsingBodies.Enabled = m_useSRP.Checked;
        }

        private readonly JplDE m_jplInfo;
        private CentralBody m_centralBody;
        private double m_gravitationalParameter;
        private readonly Dictionary<string, double> m_gravConstants = new Dictionary<string, double>();
        private readonly Dictionary<string, JplDECentralBody> m_nameToJplMapping = new Dictionary<string, JplDECentralBody>();
        private string m_lastCB;
        private bool m_lastDragChecked = true;

        private static readonly string EARTH = CentralBodiesFacet.GetFromContext().Earth.Name;
        private static readonly string SUN = CentralBodiesFacet.GetFromContext().Sun.Name;
        private static readonly string MOON = CentralBodiesFacet.GetFromContext().Moon.Name;

        private const string JR = "Jacchia-Roberts";
        private const string MSIS86 = "MSIS86";
        private const string MSIS90 = "MSIS90";
        private const string MSIS2000 = "MSIS2000";

        private const string DUALCONE = "Dual Cone";
        private const string CYLINDRICAL = "Cylindrical";

        private const string PERMANENTTIDES = "Permanent";
        private const string NOTIDES = "None";
    }
}