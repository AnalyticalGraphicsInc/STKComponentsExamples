using System.IO;
using System.Windows.Forms;
using AGI.Foundation;
using AGI.Foundation.Celestial;

namespace AGI.Examples
{
    public static class EarthOrientationParametersHelper
    {
        /// <summary>
        /// Tries to download EOP-v1.1.txt, which contains Earth Orientation Parameters data, from AGI's server.
        /// If AGI's server is unavailable (i.e. if the machine on which the demo is running is not connected 
        /// to the internet), pulls EOP-v1.1.txt from the local Data directory.
        /// </summary>
        public static EarthOrientationParameters GetEarthOrientationParameters()
        {
            try
            {
                return EarthOrientationParametersFile.DownloadData();
            }
            catch (DataUnavailableException)
            {
                // Read from local file if the machine does not have access to the internet.
                string dataPath = Path.Combine(Application.StartupPath, "Data");
                return EarthOrientationParametersFile.ReadData(Path.Combine(dataPath, "EOP-v1.1.txt"));
            }
        }
    }
}