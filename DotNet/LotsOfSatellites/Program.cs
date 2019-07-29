using System;
using System.IO;
using System.Windows.Forms;
using AGI.Foundation.Celestial;

namespace AGI.Examples.LotsOfSatellites
{
    public static class Program
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

            // Load JPL data
            // Optional - Without this an analytic model is used to position central bodies
            string dataPath = Path.Combine(Application.StartupPath, "Data");
            JplDE430 jpl = new JplDE430(Path.Combine(dataPath, "plneph.430"));
            jpl.UseForCentralBodyPositions(CentralBodiesFacet.GetFromContext());

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LotsOfSatellites());
        }
    }
}
