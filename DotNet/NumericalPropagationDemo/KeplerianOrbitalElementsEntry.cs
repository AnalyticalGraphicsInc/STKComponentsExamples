using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ComponentModel;
using AGI.Foundation.Coordinates;
using AGI.Foundation.Celestial;
using AGI.Foundation;

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
            m_keplerianElements = m_defaultElements;
            DisplayOrbitalElements();
        }

        /// <summary>
        /// A constructor that takes in and displays a set of orbital elements.
        /// </summary>
        /// <param name="elements">The original orbital elements you want to display.</param>
        public KeplerianOrbitalElementsEntry(KeplerianElements elements)
        {
            InitializeComponent();
            m_keplerianElements = elements;
            DisplayOrbitalElements();
        }

        /// <summary>
        /// This will return the orbital elements currently displayed in the GUI.
        /// </summary>
        public KeplerianElements KeplerianElementValues
        {
            get
            {
                CancelEventArgs answer = new CancelEventArgs();
                KeplerianOrbit_Validating(this, answer);
                if (answer.Cancel)
                    return null;
                else
                    return m_keplerianElements;
            }
            set
            {
                m_keplerianElements = value;
                this.DisplayOrbitalElements();
            }
        }

        /// <summary>
        /// The gravitational parameter associated with this set of elements.
        /// </summary>
        public double GravitationalConstant
        {
            get { return m_keplerianElements.GravitationalParameter; }
            set
            {
                m_keplerianElements = new KeplerianElements(
                    m_keplerianElements.SemimajorAxis,
                    m_keplerianElements.Eccentricity,
                    m_keplerianElements.Inclination,
                    m_keplerianElements.ArgumentOfPeriapsis,
                    m_keplerianElements.RightAscensionOfAscendingNode,
                    m_keplerianElements.TrueAnomaly,
                    value);
            }
        }

        /// <summary>
        /// This will update the Control with the currently entered set KeplerianElements.
        /// </summary>
        private void DisplayOrbitalElements()
        {
            m_argumentOfPeriapsis.Text = m_keplerianElements.ArgumentOfPeriapsis.ToString();
            m_eccentricity.Text = m_keplerianElements.Eccentricity.ToString();
            m_inclination.Text = m_keplerianElements.Inclination.ToString();
            m_RAAN.Text = m_keplerianElements.RightAscensionOfAscendingNode.ToString();
            m_semiMajorAxis.Text = m_keplerianElements.SemimajorAxis.ToString();
            m_trueAnomaly.Text = m_keplerianElements.TrueAnomaly.ToString();
        }

        /// <summary>
        /// This will validate that the values entered in the GUI can be represented as 
        /// a set of KeplerianElements.
        /// </summary>
        /// <param name="sender">What fired this event.</param>
        /// <param name="e">Additional information about this event.</param>
        private void KeplerianOrbit_Validating(object sender, CancelEventArgs e)
        {
            double tempSemimajorAxis;
            double tempEccentricity;
            double tempInclination;
            double tempArgumentOfPeriapsis;
            double tempRightAscensionOfAscendingNode;
            double tempTrueAnomaly;

            try
            {
                tempSemimajorAxis = double.Parse(m_semiMajorAxis.Text);
                tempEccentricity = double.Parse(m_eccentricity.Text);
                tempInclination = double.Parse(m_inclination.Text);
                tempArgumentOfPeriapsis = double.Parse(m_argumentOfPeriapsis.Text);
                tempRightAscensionOfAscendingNode = double.Parse(m_RAAN.Text);
                tempTrueAnomaly = double.Parse(m_trueAnomaly.Text);

                m_keplerianElements = new KeplerianElements(tempSemimajorAxis, tempEccentricity, tempInclination, tempArgumentOfPeriapsis, tempRightAscensionOfAscendingNode, tempTrueAnomaly, GravitationalConstant);
            }
            catch (FormatException)
            {
                MessageBox.Show("One of your values is not a double", "Invalid orbital element");
                ((Control)sender).Focus();
                if (e != null)
                {
                    e.Cancel = true;
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "Bad orbital elements");
                ((Control)sender).Focus();
                if (e != null)
                {
                    e.Cancel = true;
                }
            }
        }

        //
        // Members
        //

        private KeplerianElements m_keplerianElements;
        private readonly KeplerianElements m_defaultElements = new KeplerianElements(7800000, 0.1, 0.5, 0, 0, 0, WorldGeodeticSystem1984.GravitationalParameter);
    }
}
