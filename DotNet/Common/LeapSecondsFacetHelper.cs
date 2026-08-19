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
        /// Tries to download LeapSecond.dat, which lists all leap seconds from 1970 to the present, from an AGI server.
        /// If the download fails, which could occur if the server is unavailable,
        /// or if the machine on which the demo is running is not connected to the internet,
        /// then a local copy of LeapSecond.dat is loaded from the Data directory.
        /// </summary>
        public static LeapSecondsFacet GetLeapSeconds()
        {
            return Download() ?? LoadLocal();
        }

        private static LeapSecondsFacet Download()
        {
            try
            {
                return LeapSecondFile.DownloadLeapSeconds();
            }
            catch (DataUnavailableException)
            {
                return null;
            }
        }

        /// <summary>
        /// Read from local data if internet is not available.
        /// </summary>
        private static LeapSecondsFacet LoadLocal()
        {
            string dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath ?? "", "Data");
            string fileName = Path.Combine(dataDirectory, "LeapSecond.dat");
            return LeapSecondFile.ReadLeapSeconds(fileName);
        }
    }
}
