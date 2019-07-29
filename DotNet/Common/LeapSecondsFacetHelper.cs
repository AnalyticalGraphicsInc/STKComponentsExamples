using System;
using System.IO;
using AGI.Foundation;
using AGI.Foundation.Time;

namespace AGI.Examples
{
    /// <summary>
    /// Contains helper methods used in multiple demos to load leap second data.
    /// </summary>
    public static class LeapSecondsFacetHelper
    {
        /// <summary>
        /// Tries to download LeapSecond.dat, which lists all leap seconds from 1970 to the present, from AGI's server.
        /// If AGI's server is unavailable (i.e. if the machine on which the demo is running is not connected 
        /// to the internet), pulls LeapSecond.dat from the local Data directory.
        /// </summary>
        public static LeapSecondsFacet GetLeapSeconds()
        {
            try
            {
                return LeapSecondFile.DownloadLeapSeconds();
            }
            catch (DataUnavailableException)
            {
                // Read from local file if the machine does not have access to the internet.
                string dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath ?? "", "Data");
                return LeapSecondFile.ReadLeapSeconds(Path.Combine(dataPath, "LeapSecond.dat"));
            }
        }
    }
}
