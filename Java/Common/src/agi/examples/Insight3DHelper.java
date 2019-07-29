package agi.examples;

import java.awt.Dimension;
import java.awt.Font;
import java.awt.Graphics;
import java.awt.image.BufferedImage;

import agi.foundation.Trig;
import agi.foundation.celestial.CentralBody;
import agi.foundation.coordinates.AzimuthElevationRange;
import agi.foundation.coordinates.Cartesian;
import agi.foundation.coordinates.CartographicExtent;
import agi.foundation.geometry.Axes;
import agi.foundation.geometry.AxesEastNorthUp;
import agi.foundation.geometry.PointFixedOffset;
import agi.foundation.graphics.Camera;
import agi.foundation.graphics.advanced.BoundingSphere;
import agi.foundation.graphics.awt.Insight3D;

/**
 * Contains various helper methods used in multiple demos that use Insight3D.
 */
public final class Insight3DHelper {
    private Insight3DHelper() {}

    /**
     * Measure the size of a string rendered in a given font.
     */
    public static Dimension measureString(String text, Font font) {
        BufferedImage tempBitmap = new BufferedImage(1, 1, BufferedImage.TYPE_INT_ARGB);
        Graphics graphics = tempBitmap.getGraphics();
        // Split string to take into account line breaks
        String[] splitText = text.split("\n");
        int maxWidth = 0;
        try {
            graphics.setFont(font);
            for (String textLine : splitText) {
                int lineWidth = graphics.getFontMetrics().stringWidth(textLine);
                if (lineWidth > maxWidth)
                    maxWidth = lineWidth;
            }
            int lineHeight = graphics.getFontMetrics().getAscent() + graphics.getFontMetrics().getDescent();
            int height = lineHeight * splitText.length;
            height += graphics.getFontMetrics().getDescent() * 2;
            return new Dimension(maxWidth + graphics.getFontMetrics().getMaxAdvance() * 2, height);
        } finally {
            graphics.dispose();
        }
    }

    /**
     * Draw a given string to a graphics object, splitting on newlines.
     */
    public static void drawString(String text, int x, int y, Font font, Graphics gfx) {
        gfx.setFont(font);

        String[] splitText = text.split("\n");
        int lineHeight = gfx.getFontMetrics().getAscent() + gfx.getFontMetrics().getDescent();
        int currentLineY = y + lineHeight;
        for (String textLine : splitText) {
            gfx.drawString(textLine, x, currentLineY);
            currentLineY += lineHeight;
        }
    }

    /**
     * Positions the camera to view a given bounding sphere.
     */
    public static void viewBoundingSphere(Insight3D insight3D, CentralBody centralBody, BoundingSphere sphere) {
        viewBoundingSphere(insight3D, centralBody, sphere, Trig.degreesToRadians(-90.0), Trig.degreesToRadians(-30.0));
    }

    /**
     * Positions the camera to view a given bounding sphere from a given azimuth and elevation angle.
     */
    public static void viewBoundingSphere(Insight3D insight3D, CentralBody centralBody, BoundingSphere sphere, double azimuthAngle,
            double elevationAngle) {
        PointFixedOffset boundingSphereCenter = new PointFixedOffset(centralBody.getFixedFrame(), sphere.getCenter());
        Axes boundingSphereAxes = new AxesEastNorthUp(centralBody, boundingSphereCenter);

        Camera camera = insight3D.getScene().getCamera();
        Cartesian offset = new Cartesian(
                new AzimuthElevationRange(azimuthAngle, elevationAngle, camera.getDistancePerRadius() * sphere.getRadius()));
        camera.viewOffset(boundingSphereAxes, boundingSphereCenter, offset);
    }

    /**
     * Positions the camera to view a given extent on the surface from a given azimuth and elevation angle.
     */
    public static void viewExtent(Insight3D insight3D, CentralBody centralBody, double west, double south, double east, double north,
            double azimuthAngle, double elevationAngle) {
        Camera camera = insight3D.getScene().getCamera();
        camera.viewExtent(centralBody, west, south, east, north);
        Cartesian offset = new Cartesian(new AzimuthElevationRange(azimuthAngle, elevationAngle, camera.getDistance()));
        camera.setPosition(camera.getReferencePoint().add(offset));
    }

    /**
     * Positions the camera to view a given extent on the surface from a given azimuth and elevation angle.
     */
    public static void viewExtent(Insight3D insight3D, CentralBody centralBody, CartographicExtent extent, double azimuthAngle,
            double elevationAngle) {
        viewExtent(insight3D, centralBody, extent.getWestLongitude(), extent.getSouthLatitude(), extent.getEastLongitude(),
                extent.getNorthLatitude(), azimuthAngle, elevationAngle);
    }

}