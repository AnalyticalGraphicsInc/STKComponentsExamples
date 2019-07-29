using System;
using System.Windows.Forms;
using AGI.Foundation.Celestial;

namespace AGI.Examples
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // startup data configuration

            // Update LeapSecond.dat, and use it in the current calculation context.
            LeapSecondsFacetHelper.GetLeapSeconds().UseInCurrentContext();

            EarthCentralBody earth = CentralBodiesFacet.GetFromContext().Earth;

            // Load EOP Data - For fixed to inertial transformations
            earth.OrientationParameters = EarthOrientationParametersHelper.GetEarthOrientationParameters();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }
}