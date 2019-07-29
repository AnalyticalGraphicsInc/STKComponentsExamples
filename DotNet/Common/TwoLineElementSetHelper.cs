using System;
using System.Collections.Generic;
using System.IO;
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
        /// Tries to download from AGI's server a list of TLEs describing the satellite which has the given string 
        /// NORAD identifier for the 24 hour period following the given date.  If AGI's server is unavailable 
        /// (i.e. if the machine on which the demo is running is not connected to the internet), 
        /// pulls the list of TLEs from the local Data directory.
        /// </summary>
        public static List<TwoLineElementSet> GetTles(string satelliteIdentifier, JulianDate date)
        {
            try
            {
                return TwoLineElementSet.DownloadTles(satelliteIdentifier, date, date.AddDays(1.0));
            }
            catch (DataUnavailableException)
            {
                // Read from local data if the machine does not have access to the internet.
                string dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath ?? "", "Data");
                var satelliteDatabase = new StkSatelliteDatabase(Path.Combine(dataPath, "SatelliteDatabase"), "stkSatDb");
                var query = new StkSatelliteDatabaseQuery
                {
                    SatelliteNumber = new Regex(satelliteIdentifier)
                };

                foreach (var entry in satelliteDatabase.GetEntries(query))
                {
                    return new List<TwoLineElementSet> { entry.TwoLineElementSet };
                }

                throw new DataUnavailableException("TLE data for " + satelliteIdentifier + " could not be found in local SatelliteDatabase");
            }
        }
    }
}
