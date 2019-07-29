package com.agi.satellitetracker;

import agi.foundation.coordinates.Cartographic;

/**
 * Data object storing a particular ground position on the earth's surface and
 * whether the satellite has access to the facility at that point.
 */
public class GroundTrackPosition {
    public GroundTrackPosition(Cartographic position, boolean hasAccess) {
        m_position = position;
        m_hasAccess = hasAccess;
    }

    public Cartographic getPosition() {
        return m_position;
    }

    public boolean hasAccess() {
        return m_hasAccess;
    }

    public double getLongitude() {
        return m_position.getLongitude();
    }

    public double getLatitude() {
        return m_position.getLatitude();
    }

    private final Cartographic m_position;
    private final boolean m_hasAccess;
}
