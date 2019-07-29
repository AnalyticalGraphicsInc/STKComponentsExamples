using System;
using System.Windows.Forms;
using AGI.Examples;

namespace Communications
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

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }
}
