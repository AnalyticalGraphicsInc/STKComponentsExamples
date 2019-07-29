package agi.examples;

import agi.foundation.DataUnavailableException;
import agi.foundation.celestial.EarthOrientationParameters;
import agi.foundation.celestial.EarthOrientationParametersFile;

public final class EarthOrientationParametersHelper {
    private EarthOrientationParametersHelper() {}

    /**
     * Tries to download EOP-v1.1.txt, which contains Earth Orientation Parameters data, from AGI's server.
     * If AGI's server is unavailable (i.e. if the machine on which the demo is running is not connected
     * to the internet), pulls EOP-v1.1.txt from the local Data directory.
     */
    public static EarthOrientationParameters getEarthOrientationParameters() {
        try {
            return EarthOrientationParametersFile.downloadData();
        } catch (DataUnavailableException e) {
            // Read from local file if the machine does not have access to the internet.
            return EarthOrientationParametersFile.readData("Data/EOP-v1.1.txt");
        }
    }
}