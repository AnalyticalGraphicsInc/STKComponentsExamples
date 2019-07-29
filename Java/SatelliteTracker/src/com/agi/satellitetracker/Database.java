package com.agi.satellitetracker;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.regex.Pattern;

import agi.foundation.stk.StkSatelliteDatabase;
import agi.foundation.stk.StkSatelliteDatabaseEntry;
import agi.foundation.stk.StkSatelliteDatabaseQuery;
import agi.foundation.time.GregorianDate;

/**
 * Provides a higher-level interface to a StkSatelliteDatabase.
 */
// #region Database
public class Database {
    public Database() {
        m_stkSatelliteDatabase = new StkSatelliteDatabase(SatelliteTracker.getDataFilePath("SatelliteDatabase"), "stkSatDb");

        ArrayList<String> categories = new ArrayList<>();
        for (String mission : m_stkSatelliteDatabase.getMissions()) {
            if (categoryHasSatellites(mission)) {
                categories.add(mission);
            }
        }

        Collections.sort(categories);
        categories.add(USER_ENTERED_TLE);
        m_categories = Collections.unmodifiableList(categories);
    }

    private boolean categoryHasSatellites(String category) {
        StkSatelliteDatabaseQuery query = new StkSatelliteDatabaseQuery();
        query.setMission(Pattern.compile(category));

        for (StkSatelliteDatabaseEntry entry : m_stkSatelliteDatabase.getEntries(query)) {
            if (entry.getTwoLineElementSet() != null) {
                return true;
            }
        }
        return false;
    }

    public GregorianDate getLastUpdateDate() {
        return m_stkSatelliteDatabase.getLastUpdateDate();
    }

    /**
     * Query the database for an array of names of satellites that are in a
     * given category.
     *
     * @param category
     *            The category (mission) of the satellites.
     * @return An array of names of satellites that are in the category.
     */
    public String[] getSatellitesInCategory(String category) {
        ArrayList<String> result = new ArrayList<>();

        StkSatelliteDatabaseQuery query = new StkSatelliteDatabaseQuery();
        query.setMission(Pattern.compile(category));
        for (StkSatelliteDatabaseEntry entry : m_stkSatelliteDatabase.getEntries(query)) {
            if (entry.getTwoLineElementSet() != null) {
                result.add(entry.getCommonName());
            }
        }

        Collections.sort(result);
        return result.toArray(new String[result.size()]);
    }

    /**
     * Query the database for an array of category names.
     *
     * @return An array of names of categories (missions) for satellites.
     */
    public String[] getCategories() {
        return m_categories.toArray(new String[m_categories.size()]);
    }

    /**
     * Query the database to find the TLE for a particular satellite.
     *
     * @param category
     *            The category (mission) of the satellite.
     * @param spacecraft
     *            The name of the satellite.
     * @return The TLE for the given satellite if it exists, otherwise an empty
     *         string.
     */
    public String getTLEText(String category, String spacecraft) {
        StkSatelliteDatabaseQuery query = new StkSatelliteDatabaseQuery();
        query.setMission(Pattern.compile(category));
        query.setCommonName(Pattern.compile(spacecraft));

        for (StkSatelliteDatabaseEntry entry : m_stkSatelliteDatabase.getEntries(query)) {
            if (entry.getTwoLineElementSet() != null) {
                return entry.getTwoLineElementSet().toTleString();
            }
        }

        return "";
    }

    public static final String USER_ENTERED_TLE = "User-entered TLE";

    private final StkSatelliteDatabase m_stkSatelliteDatabase;
    private final List<String> m_categories;
}

// #endregion