package com.agi.satellitetracker;

import java.awt.BasicStroke;
import java.awt.BorderLayout;
import java.awt.Color;
import java.awt.Container;
import java.awt.Dimension;
import java.awt.Graphics2D;
import java.awt.Rectangle;
import java.awt.Stroke;
import java.awt.geom.Path2D;
import java.awt.geom.Point2D;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashSet;
import java.util.List;
import java.util.Set;

import javax.swing.JFrame;
import javax.swing.JScrollPane;
import javax.swing.JTable;
import javax.swing.table.AbstractTableModel;

import org.jxmapviewer.JXMapKit;
import org.jxmapviewer.JXMapViewer;
import org.jxmapviewer.painter.CompoundPainter;
import org.jxmapviewer.painter.Painter;
import org.jxmapviewer.viewer.DefaultWaypoint;
import org.jxmapviewer.viewer.GeoPosition;
import org.jxmapviewer.viewer.Waypoint;
import org.jxmapviewer.viewer.WaypointPainter;

import agi.foundation.Trig;
import agi.foundation.coordinates.AzimuthElevationRange;
import agi.foundation.coordinates.Cartographic;
import agi.foundation.time.TimeInterval;
import agi.foundation.time.TimeIntervalCollection;

/**
 * Window displaying the results of a computation, both in graphical and tabular
 * form.
 */
public class ResultsWindow extends JFrame {
    /**
     * Construct the window. After construction this object will have all of the
     * controls built and added to the Swing hierarchy.
     */
    public ResultsWindow(UserInput userInput, Computation computation) {
        super("Satellite Tracker Results");
        setDefaultCloseOperation(JFrame.DISPOSE_ON_CLOSE);

        Container contentPane = getContentPane();
        contentPane.setLayout(new BorderLayout());

        // create a new map viewer, using OpenStreetMap, centered on the
        // facility location
        JXMapKit jxMapKit = new JXMapKit();
        jxMapKit.setMiniMapVisible(false);

        jxMapKit.setZoom(15);

        GeoPosition geoPosition = new GeoPosition(userInput.getLatitude(), userInput.getLongitude());
        jxMapKit.setAddressLocation(geoPosition);

        // create a waypoint marker for the facility position
        Set<Waypoint> waypoints = new HashSet<>();
        waypoints.add(new DefaultWaypoint(geoPosition));

        // construct a waypoint painter that will draw the facility waypoint
        WaypointPainter<Waypoint> wayPointPainter = new WaypointPainter<>();
        wayPointPainter.setWaypoints(waypoints);

        // construct our custom map painter class
        Painter<JXMapViewer> groundTrackPainter = new GroundTrackPainter(computation);

        // construct a compound painter that will use both painters, and use it
        // as the painter for the map
        CompoundPainter<JXMapViewer> painter = new CompoundPainter<>(Arrays.asList(wayPointPainter, groundTrackPainter));
        painter.setCacheable(false);

        jxMapKit.getMainMap().setOverlayPainter(painter);

        contentPane.add(jxMapKit, BorderLayout.CENTER);

        // construct a table containing the results using our custom table model
        AccessResultsTableModel model = new AccessResultsTableModel(computation);
        JTable table = new JTable(model);
        model.setColumnWidths(table);

        JScrollPane scrollPane = new JScrollPane(table);
        table.setFillsViewportHeight(true);

        scrollPane.setPreferredSize(new Dimension(800, 200));

        contentPane.add(scrollPane, BorderLayout.PAGE_END);

        setSize(800, 800);
    }

    /**
     * Painter class that can draw a list of ground track positions onto a map,
     * using green for access and red for no access.
     */
    private static final class GroundTrackPainter implements Painter<JXMapViewer> {
        public GroundTrackPainter(Computation computation) {
            m_groundTracks = computation.getGroundTracks();
        }

        @Override
        public void paint(Graphics2D g, JXMapViewer map, int w, int h) {
            if (m_groundTracks.isEmpty())
                return;

            g = (Graphics2D) g.create();

            // build two lists of paths so we can draw each in a different color
            List<Path2D.Double> accessPaths = new ArrayList<>();
            List<Path2D.Double> noAccessPaths = new ArrayList<>();

            Path2D.Double path = new Path2D.Double();
            GroundTrackPosition first = m_groundTracks.get(0);
            addMoveTo(map, path, first.getPosition());

            GroundTrackPosition previousPosition = first;
            boolean previousPositionAccess = first.hasAccess();
            for (int i = 1; i < m_groundTracks.size(); ++i) {
                GroundTrackPosition position = m_groundTracks.get(i);

                if (previousPositionAccess != position.hasAccess()) {
                    // if we are switching from access to no access (or vice-versa) 
                    // then put the path we build into the appropriate list and start a new path
                    if (previousPositionAccess)
                        accessPaths.add(path);
                    else
                        noAccessPaths.add(path);

                    path = new Path2D.Double();
                    addMoveTo(map, path, position.getPosition());
                }

                if (Math.abs(previousPosition.getLongitude() - position.getLongitude()) > Math.PI) {
                    // handle case where ground track crosses the international
                    // date line and longitude wraps from -180 deg to 180 deg (or vice-versa).

                    // Use simple linear interpolation to find the position on the date line.
                    GroundTrackPosition interpolated = findPointOnEdge(previousPosition, position);

                    // draw line to interpolated position
                    addLineTo(map, path, interpolated);

                    // move to the opposite side of the date line
                    addMoveTo(map, path, new Cartographic(-interpolated.getLongitude(), interpolated.getLatitude(), 0));
                }

                // draw line to position
                addLineTo(map, path, position);
                previousPosition = position;
                previousPositionAccess = position.hasAccess();
            }

            // add final path to appropriate list
            if (previousPositionAccess)
                accessPaths.add(path);
            else
                noAccessPaths.add(path);

            Rectangle viewportBounds = map.getViewportBounds();
            Dimension mapSizeInTiles = map.getTileFactory().getMapSize(map.getZoom());
            int tileSize = map.getTileFactory().getTileSize(map.getZoom());
            Dimension mapSizeInPixels = new Dimension(mapSizeInTiles.width * tileSize, mapSizeInTiles.height * tileSize);

            // vpx will be the x location of the edge of the map
            double vpx = viewportBounds.getX();
            // normalize the left edge of the viewport to be positive
            while (vpx < 0) {
                vpx += mapSizeInPixels.getWidth();
            }
            // normalize the left edge of the viewport to not wrap around the world
            while (vpx > mapSizeInPixels.getWidth()) {
                vpx -= mapSizeInPixels.getWidth();
            }

            // create two sub-graphics objects...
            Graphics2D g1 = (Graphics2D) g.create();
            Graphics2D g2 = (Graphics2D) g.create();

            // ...one translated to the copy of the map to the left of the on-screen edge
            g1.translate(viewportBounds.getX() - vpx, 0);

            // ...one translated to the copy of the map to the right of the on-screen edge
            g2.translate(viewportBounds.getX() - vpx + mapSizeInPixels.getWidth(), 0);

            Stroke stroke = new BasicStroke(3.0f);
            g1.setStroke(stroke);
            g2.setStroke(stroke);

            // draw no access paths in red
            g1.setColor(Color.RED);
            g2.setColor(Color.RED);
            for (Path2D.Double p : noAccessPaths) {
                g1.draw(p);
                g2.draw(p);
            }

            // draw access paths in green
            g1.setColor(Color.GREEN);
            g2.setColor(Color.GREEN);
            for (Path2D.Double p : accessPaths) {
                g1.draw(p);
                g2.draw(p);
            }

            g1.dispose();
            g2.dispose();
            g.dispose();
        }

        private void addLineTo(JXMapViewer map, Path2D.Double path, GroundTrackPosition position) {
            Point2D point = cartographicToPoint(map, position.getPosition());
            path.lineTo(point.getX(), point.getY());
        }

        private void addMoveTo(JXMapViewer map, Path2D.Double path, Cartographic position) {
            Point2D point = cartographicToPoint(map, position);
            path.moveTo(point.getX(), point.getY());
        }

        /**
         * Uses simple linear interpolation to find and approximation of the
         * ground track point that lands exactly on the international date line.
         */
        private GroundTrackPosition findPointOnEdge(GroundTrackPosition previousPosition, GroundTrackPosition nextPosition) {
            double previousLongitude = previousPosition.getLongitude();
            double previousLatitude = previousPosition.getLatitude();

            double interpolatedLongitude = previousLongitude < 0 ? -Math.PI : Math.PI;
            double interpolatedLatitude;

            double longitude = nextPosition.getLongitude();
            double latitude = nextPosition.getLatitude();

            longitude += interpolatedLongitude * 2;

            double ratio = (interpolatedLongitude - previousLongitude) / (longitude - previousLongitude);
            interpolatedLatitude = previousLatitude + (latitude - previousLatitude) * ratio;

            Cartographic cartographic = new Cartographic(interpolatedLongitude, interpolatedLatitude, 0.0);
            return new GroundTrackPosition(cartographic, previousPosition.hasAccess());
        }

        private Point2D cartographicToPoint(JXMapViewer map, Cartographic position) {
            double latitude = Trig.radiansToDegrees(position.getLatitude());
            double longitude = Trig.radiansToDegrees(position.getLongitude());
            GeoPosition geoPosition = new GeoPosition(latitude, longitude);
            return map.convertGeoPositionToPoint(geoPosition);
        }

        private final List<GroundTrackPosition> m_groundTracks;
    }

    /**
     * A custom table model that presents the results of the access computation
     * in tabular form.
     */
    public static class AccessResultsTableModel extends AbstractTableModel {
        public AccessResultsTableModel(Computation computation) {
            m_intervals = computation.getIntervals();
            m_approachAers = computation.getApproachAers();
            m_departureAers = computation.getDepartureAers();
        }

        @Override
        public int getColumnCount() {
            return COLUMN_NAMES.length;
        }

        @Override
        public int getRowCount() {
            return m_intervals.size();
        }

        @Override
        public String getColumnName(int column) {
            return COLUMN_NAMES[column];
        }

        @Override
        public Class<?> getColumnClass(int columnIndex) {
            return getValueAt(0, columnIndex).getClass();
        }

        @Override
        public Object getValueAt(int rowIndex, int columnIndex) {
            switch (columnIndex) {
            case START_DATE:
                return m_intervals.get(rowIndex).getStart().toGregorianDate().toString();
            case END_DATE:
                return m_intervals.get(rowIndex).getStop().toGregorianDate().toString();
            case DURATION:
                TimeInterval interval = m_intervals.get(rowIndex);
                return interval.getStart().minutesDifference(interval.getStop());
            case APPROACH_AZIMUTH:
                return Trig.radiansToDegrees(m_approachAers.get(rowIndex).getAzimuth());
            case APPROACH_ELEVATION:
                return Trig.radiansToDegrees(m_approachAers.get(rowIndex).getElevation());
            case DEPARTURE_AZIMUTH:
                return Trig.radiansToDegrees(m_departureAers.get(rowIndex).getAzimuth());
            case DEPARTURE_ELEVATION:
                return Trig.radiansToDegrees(m_departureAers.get(rowIndex).getElevation());
            default:
                throw new IllegalArgumentException("Unknown column index" + columnIndex);
            }
        }

        public void setColumnWidths(JTable table) {
            table.getColumnModel().getColumn(START_DATE).setPreferredWidth(120);
            table.getColumnModel().getColumn(END_DATE).setPreferredWidth(120);
        }

        private static final long serialVersionUID = 1L;
        private final static int START_DATE = 0;
        private final static int END_DATE = 1;
        private final static int DURATION = 2;
        private final static int APPROACH_AZIMUTH = 3;
        private final static int APPROACH_ELEVATION = 4;
        private final static int DEPARTURE_AZIMUTH = 5;
        private final static int DEPARTURE_ELEVATION = 6;
        private final static int NUM_COLUMNS = 7;
        private final static String[] COLUMN_NAMES;

        static {
            COLUMN_NAMES = new String[NUM_COLUMNS];
            COLUMN_NAMES[START_DATE] = "Start Date (UTC)";
            COLUMN_NAMES[END_DATE] = "End Date (UTC)";
            COLUMN_NAMES[DURATION] = "Duration (sec)";
            COLUMN_NAMES[APPROACH_AZIMUTH] = "Approach Az";
            COLUMN_NAMES[APPROACH_ELEVATION] = "Approach El";
            COLUMN_NAMES[DEPARTURE_AZIMUTH] = "Departure Az";
            COLUMN_NAMES[DEPARTURE_ELEVATION] = "Departure El";
        }

        private final TimeIntervalCollection m_intervals;
        private final List<AzimuthElevationRange> m_approachAers;
        private final List<AzimuthElevationRange> m_departureAers;
    }

    private static final long serialVersionUID = 1L;
}
