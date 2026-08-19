package agi.examples;

import java.nio.file.Path;
import java.nio.file.Paths;

import agi.foundation.DataUnavailableException;
import agi.foundation.time.LeapSecondFile;
import agi.foundation.time.LeapSecondsFacet;

/**
 * Contains helper methods used in multiple demos to load leap second data.
 */
public final class LeapSecondsFacetHelper {
    private LeapSecondsFacetHelper() {}

    /**
     * Tries to download LeapSecond.dat, which lists all leap seconds from 1970 to the present, from an AGI server.
     * If the download fails, which could occur if the server is unavailable,
     * or if the machine on which the demo is running is not connected to the internet,
     * then a local copy of LeapSecond.dat is loaded from the Data directory.
     */
    public static LeapSecondsFacet getLeapSeconds() {
        LeapSecondsFacet result = download();
        if (result == null) {
            result = loadLocal();
        }

        return result;
    }

    private static LeapSecondsFacet download() {
        try {
            return LeapSecondFile.downloadLeapSeconds();
        } catch (DataUnavailableException e) {
            return null;
        }
    }

    private static LeapSecondsFacet loadLocal() {
        Path dataDirectory = Paths.get("Data");
        String fileName = dataDirectory.resolve("LeapSecond.dat").toString();
        return LeapSecondFile.readLeapSeconds(fileName);
    }
}
