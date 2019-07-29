using System;
using System.Windows.Forms;
using AGI.Foundation.Celestial;
using AGI.Foundation.Coordinates;

namespace AGI.Examples
{
    /// <summary>
    /// A class for displaying a set of KeplerianElements in a WinForms GUI.
    /// </summary>
    public partial class KeplerianOrbitalElementsEntry : UserControl
    {
        /// <summary>
        /// Constructs an instance.
        /// </summary>
        public KeplerianOrbitalElementsEntry()
        {
            InitializeComponent();

            m_argumentOfPeriapsis.Text = "0";
            m_eccentricity.Text = "0.1";
            m_inclination.Text = "0.5";
            m_RAAN.Text = "0";
            m_semiMajorAxis.Text = "7800000";
            m_trueAnomaly.Text = "0";
        }

        /// <summary>
        /// This will return the orbital elements currently displayed in the GUI.
        /// </summary>
        public KeplerianElements KeplerianElementValues
        {
            get
            {
                try
                {
                    double semimajorAxis = double.Parse(m_semiMajorAxis.Text);
                    double eccentricity = double.Parse(m_eccentricity.Text);
                    double inclination = double.Parse(m_inclination.Text);
                    double argumentOfPeriapsis = double.Parse(m_argumentOfPeriapsis.Text);
                    double rightAscensionOfAscendingNode = double.Parse(m_RAAN.Text);
                    double trueAnomaly = double.Parse(m_trueAnomaly.Text);

                    return new KeplerianElements(semimajorAxis, eccentricity, inclination, argumentOfPeriapsis, rightAscensionOfAscendingNode, trueAnomaly, WorldGeodeticSystem1984.GravitationalParameter);
                }
                catch (FormatException)
                {
                    MessageBox.Show("One of your values is not a double", "Invalid orbital element");
                    return null;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Bad orbital elements");
                    return null;
                }
            }
        }
    }
}