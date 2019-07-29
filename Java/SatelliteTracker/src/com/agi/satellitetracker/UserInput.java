package com.agi.satellitetracker;

import agi.foundation.time.GregorianDate;

/**
 * Class to store user-entered information.
 */
public class UserInput {
    public UserInput(GregorianDate startDate) {
        m_startDate = startDate;
        m_endDate = new GregorianDate(startDate.toDateTime().plusDays(1));
    }

    public String getAddress() {
        return m_address;
    }

    public void setAddress(String address) {
        m_address = address;
    }

    public double getLatitude() {
        return m_latitude;
    }

    public void setLatitude(double latitude) {
        m_latitude = latitude;
    }

    public double getLongitude() {
        return m_longitude;
    }

    public void setLongitude(double longitude) {
        m_longitude = longitude;
    }

    public double getElevation() {
        return m_elevation;
    }

    public void setElevation(double elevation) {
        m_elevation = elevation;
    }

    public GregorianDate getStartDate() {
        return m_startDate;
    }

    public void setStartDate(GregorianDate startDate) {
        m_startDate = startDate;
    }

    public GregorianDate getEndDate() {
        return m_endDate;
    }

    public void setEndDate(GregorianDate endDate) {
        m_endDate = endDate;
    }

    public String getSpacecraft() {
        return m_spacecraft;
    }

    public void setSpacecraft(String spacecraft) {
        m_spacecraft = spacecraft;
    }

    public String getSpacecraftCategory() {
        return m_spacecraftCategory;
    }

    public void setSpacecraftCategory(String spacecraftCategory) {
        m_spacecraftCategory = spacecraftCategory;
    }

    public String getTle() {
        return m_tle;
    }

    public void setTle(String tle) {
        m_tle = tle;
    }

    public boolean getUseAddress() {
        return m_useAddress;
    }

    public void setUseAddress(boolean useAddress) {
        m_useAddress = useAddress;
    }

    private static final String DEFAULT_ADDRESS = "220 Valley Creek Blvd, Exton, PA";
    private static final double DEFAULT_LATITUDE = 40.0385776;
    private static final double DEFAULT_LONGITUDE = -75.5966244;
    private static final double DEFAULT_ELEVATION = 10.0;
    private static final String DEFAULT_SPACECRAFT_CATEGORY = "Human Crew";
    private static final String DEFAULT_SPACECRAFT = "ISS";

    private boolean m_useAddress = false;
    private String m_address = DEFAULT_ADDRESS;
    private double m_latitude = DEFAULT_LATITUDE;
    private double m_longitude = DEFAULT_LONGITUDE;
    private double m_elevation = DEFAULT_ELEVATION;
    private String m_spacecraftCategory = DEFAULT_SPACECRAFT_CATEGORY;
    private String m_spacecraft = DEFAULT_SPACECRAFT;
    private String m_tle;
    private GregorianDate m_startDate;
    private GregorianDate m_endDate;
}
