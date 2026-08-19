package agi.examples;

import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.List;
import java.util.regex.Pattern;
import java.util.stream.Collectors;

import agi.foundation.DataUnavailableException;
import agi.foundation.propagators.TwoLineElementSet;
import agi.foundation.stk.StkSatelliteDatabase;
import agi.foundation.stk.StkSatelliteDatabaseQuery;
import agi.foundation.time.JulianDate;

/**
 * Contains helper methods used in multiple demos to load TLE data.
 */
public final class TwoLineElementSetHelper {
    private TwoLineElementSetHelper() {}

    /**
     * Tries to download from an AGI server a list of TLEs for the given satellite
     * identifier for the 24 hour period following the given date. 
     * If the download fails, which could occur if the server is unavailable,
     * or if the machine on which the demo is running is not connected to the internet,
     * then the TLEs are loaded from local files in the Data directory.
     */
    public static List<TwoLineElementSet> getTles(String satelliteIdentifier, JulianDate date) {
        List<TwoLineElementSet> result = download(satelliteIdentifier, date);
        if (result == null) {
            result = getFromDatabase(satelliteIdentifier, loadLocalDatabase());
        }

        return result;
    }

    private static List<TwoLineElementSet> download(String satelliteIdentifier, JulianDate date) {
        try {
            return TwoLineElementSet.downloadTles(satelliteIdentifier, date, date.addDays(1.0));
        } catch (DataUnavailableException e) {
            return null;
        }
    }

    private static StkSatelliteDatabase loadLocalDatabase() {
        Path dataPath = Paths.get("Data");
        Path satDbDirectory = dataPath.resolve("SatelliteDatabase");
        return new StkSatelliteDatabase(satDbDirectory.toString(), "stkSatDb");
    }

    private static List<TwoLineElementSet> getFromDatabase(String satelliteIdentifier, StkSatelliteDatabase database) {
        StkSatelliteDatabaseQuery query = new StkSatelliteDatabaseQuery();
        query.setSatelliteNumber(Pattern.compile(satelliteIdentifier));

        List<TwoLineElementSet> result = database.getEntries(query).stream().map(entry -> entry.getTwoLineElementSet()).collect(Collectors.toList());
        if (result.size() == 0) {
            throw new DataUnavailableException("TLE data for " + satelliteIdentifier + " could not be found in local SatelliteDatabase.");
        }

        return result;
    }
}