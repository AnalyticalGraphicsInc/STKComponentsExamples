using System;
using System.Windows.Forms;
using AGI.Foundation;
using AGI.Foundation.NumericalMethods;

namespace AGI.Examples
{
    /// <summary>
    /// A class for displaying the properties for a NumericalIntegrator.
    /// </summary>
    public partial class IntegratorSettings : Form
    {
        /// <summary>
        /// Constructs a default instance.
        /// </summary>
        public IntegratorSettings()
        {
            InitializeComponent();

            m_fixedOrRelative.Items.Add(RELATIVE);
            m_fixedOrRelative.Items.Add(FIXED);
            m_fixedOrRelative.SelectedIndex = 0;
            m_fixedOrRelative.DropDownStyle = ComboBoxStyle.DropDownList;

            m_integrator.Items.Add(RKF78);
            m_integrator.Items.Add(RK4);
            m_integrator.SelectedIndex = 0;
            m_integrator.DropDownStyle = ComboBoxStyle.DropDownList;

            m_stepSize.Text = 60.ToString();
            m_minStep.Text = 1.ToString();
            m_maxStep.Text = 86400.ToString();
            m_maxError.Text = Constants.Epsilon13.ToString();
        }

        /// <summary>
        /// Gets the integrator as configured.
        /// </summary>
        public NumericalIntegrator GetIntegrator()
        {
            if (m_integrator.SelectedItem.ToString() == RKF78)
            {
                RungeKuttaFehlberg78Integrator integrator = new RungeKuttaFehlberg78Integrator
                {
                    Direction = IntegrationSense.Increasing,
                    InitialStepSize = double.Parse(m_stepSize.Text),
                    MaximumStepSize = double.Parse(m_maxStep.Text),
                    MinimumStepSize = double.Parse(m_minStep.Text),
                    AbsoluteTolerance = double.Parse(m_maxError.Text)
                };
                if (m_fixedOrRelative.SelectedItem.ToString() == RELATIVE)
                {
                    integrator.StepSizeBehavior = KindOfStepSize.Relative;
                }
                else if (m_fixedOrRelative.SelectedItem.ToString() == FIXED)
                {
                    integrator.StepSizeBehavior = KindOfStepSize.Fixed;
                }
                return integrator;
            }
            if (m_integrator.SelectedItem.ToString() == RK4)
            {
                return new RungeKutta4Integrator
                {
                    InitialStepSize = double.Parse(m_stepSize.Text)
                };
            }
            throw new InvalidOperationException("Unknown integrator");
        }

        /// <summary>
        /// Closes the window
        /// </summary>
        /// <param name="sender">What fired this event.</param>
        /// <param name="e">Additional information about this event.</param>
        private void OnFinishClick(object sender, EventArgs e)
        {
            Hide();
        }

        /// <summary>
        /// This turns on/off various properties in the GUI that do/don't apply when the 
        /// integrator type is changed.
        /// </summary>
        /// <param name="sender">What fired this event.</param>
        /// <param name="e">Additional information about this event.</param>
        private void OnIntegratorSelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_integrator.SelectedItem.ToString() == RKF78)
            {
                m_maxError.Enabled = true;
                m_maxStep.Enabled = true;
                m_minStep.Enabled = true;
                m_fixedOrRelative.Enabled = true;
            }
            else if (m_integrator.SelectedItem.ToString() == RK4)
            {
                m_maxError.Enabled = false;
                m_maxStep.Enabled = false;
                m_minStep.Enabled = false;
                m_fixedOrRelative.Enabled = false;
            }
        }

        private const string RKF78 = "Runge-Kutta-Fehlberg 7/8";
        private const string RK4 = "Runge-Kutta 4";
        private const string FIXED = "Fixed";
        private const string RELATIVE = "Relative";
    }
}