package agi.examples;

import agi.foundation.DataUnavailableException;
import agi.foundation.time.LeapSecondFile;
import agi.foundation.time.LeapSecondsFacet;

/**
 * Contains helper methods used in multiple demos to load leap second data.
 */
public final class LeapSecondsFacetHelper {
    private LeapSecondsFacetHelper() {}

    /**
     * Tries to download LeapSecond.dat, which lists all leap seconds from 1970 to the present, from AGI's server.
     * If AGI's server is unavailable (i.e. if the machine on which the demo is running is not connected
     * to the internet), pulls LeapSecond.dat from the local Data directory.
     */
    public static LeapSecondsFacet getLeapSeconds() {
        try {
            return LeapSecondFile.downloadLeapSeconds();
        } catch (DataUnavailableException e) {
            return LeapSecondFile.readLeapSeconds("Data/LeapSecond.dat");
        }
    }
}