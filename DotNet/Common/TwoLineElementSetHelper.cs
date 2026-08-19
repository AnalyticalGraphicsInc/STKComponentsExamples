using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AGI.Foundation;
using AGI.Foundation.Propagators;
using AGI.Foundation.Stk;
using AGI.Foundation.Time;

namespace AGI.Examples
{
    /// <summary>
    /// Contains helper methods used in multiple demos to load TLE data.
    /// </summary>
    public static class TwoLineElementSetHelper
    {
        /// <summary>
        /// Tries to download from an AGI server a list of TLEs for the given satellite
        /// identifier for the 24-hour period following the given date.
        /// If the download fails, which could occur if the server is unavailable,
        /// or if the machine on which the demo is running is not connected to the internet,
        /// then the TLEs are loaded from local files in the Data directory.
        /// </summary>
        public static List<TwoLineElementSet> GetTles(string satelliteIdentifier, JulianDate date)
        {
            return Download(satelliteIdentifier, date) ?? GetFromDatabase(satelliteIdentifier, LoadLocalDatabase());
        }

        private static List<TwoLineElementSet> Download(string satelliteIdentifier, JulianDate date)
        {
            try
            {
                return TwoLineElementSet.DownloadTles(satelliteIdentifier, date, date.AddDays(1.0));
            }
            catch (DataUnavailableException)
            {
                return null;
            }
        }

        /// <summary>
        /// Read from local data if internet is not available.
        /// </summary>
        private static StkSatelliteDatabase LoadLocalDatabase()
        {
            string dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath ?? "", "Data");
            string satDbDirectory = Path.Combine(dataPath, "SatelliteDatabase");
            return new StkSatelliteDatabase(satDbDirectory, "stkSatDb");
        }

        private static List<TwoLineElementSet> GetFromDatabase(string satelliteIdentifier, StkSatelliteDatabase database)
        {
            var query = new StkSatelliteDatabaseQuery
            {
                SatelliteNumber = new Regex(satelliteIdentifier),
            };

            var result = database.GetEntries(query)
                                 .Select(entry => entry.TwoLineElementSet)
                                 .ToList();
            if (result.Count == 0)
            {
                throw new DataUnavailableException("TLE data for " + satelliteIdentifier + " could not be found in local SatelliteDatabase.");
            }

            return result;
        }
    }
}
