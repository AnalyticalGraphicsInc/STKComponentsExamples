package agi.examples;

import java.util.ArrayList;
import java.util.regex.Pattern;

import agi.foundation.DataUnavailableException;
import agi.foundation.propagators.TwoLineElementSet;
import agi.foundation.stk.StkSatelliteDatabase;
import agi.foundation.stk.StkSatelliteDatabaseEntry;
import agi.foundation.stk.StkSatelliteDatabaseQuery;
import agi.foundation.time.JulianDate;

/**
 * Contains helper methods used in multiple demos to load TLE data.
 */
public final class TwoLineElementSetHelper {
    private TwoLineElementSetHelper() {}

    /**
     * Tries to download from AGI's server a list of TLEs describing the satellite which has the given string
     * NORAD identifier for the 24 hour period following the given date.  If AGI's server is unavailable
     * (i.e. if the machine on which the demo is running is not connected to the internet),
     * pulls the list of TLEs from the local Data directory.
     */
    public static ArrayList<TwoLineElementSet> getTles(String satelliteIdentifier, JulianDate date) {
        try {
            return TwoLineElementSet.downloadTles(satelliteIdentifier, date, date.addDays(1.0));
        } catch (DataUnavailableException e) {
            // Read from local data if the machine does not have access to the internet.
            StkSatelliteDatabase satelliteDatabase = new StkSatelliteDatabase("Data/SatelliteDatabase", "stkSatDb");
            StkSatelliteDatabaseQuery query = new StkSatelliteDatabaseQuery();
            query.setSatelliteNumber(Pattern.compile(satelliteIdentifier));

            for (StkSatelliteDatabaseEntry entry : satelliteDatabase.getEntries(query)) {
                ArrayList<TwoLineElementSet> result = new ArrayList<>();
                result.add(entry.getTwoLineElementSet());
                return result;
            }

            throw new DataUnavailableException("TLE data for " + satelliteIdentifier + " could not be found in local SatelliteDatabase");
        }
    }

}