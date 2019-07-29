package agi.examples.lotsofsatellites;

import java.awt.BorderLayout;
import java.awt.Color;
import java.awt.Dimension;
import java.awt.Font;
import java.awt.Graphics2D;
import java.awt.image.BufferedImage;
import java.io.File;
import java.util.ArrayList;
import java.util.Arrays;

import javax.swing.JFrame;

import agi.examples.Insight3DHelper;
import agi.examples.OverlayToolbar;
import agi.foundation.MotionEvaluator1;
import agi.foundation.Trig;
import agi.foundation.celestial.CentralBodiesFacet;
import agi.foundation.celestial.EarthCentralBody;
import agi.foundation.compatibility.Action;
import agi.foundation.compatibility.EventHandler;
import agi.foundation.coordinates.AzimuthElevationRange;
import agi.foundation.coordinates.Cartesian;
import agi.foundation.coordinates.Cartographic;
import agi.foundation.coordinates.KinematicTransformation;
import agi.foundation.coordinates.Matrix3By3;
import agi.foundation.coordinates.UnitQuaternion;
import agi.foundation.geometry.Axes;
import agi.foundation.geometry.AxesEvaluator;
import agi.foundation.geometry.AxesNorthEastDown;
import agi.foundation.geometry.GeometryTransformer;
import agi.foundation.geometry.PointCartographic;
import agi.foundation.geometry.ReferenceFrame;
import agi.foundation.geometry.ReferenceFrameEvaluator;
import agi.foundation.graphics.MarkerBatchPrimitive;
import agi.foundation.graphics.Scene;
import agi.foundation.graphics.SceneManager;
import agi.foundation.graphics.SetHint;
import agi.foundation.graphics.SimulationAnimation;
import agi.foundation.graphics.TextureScreenOverlay;
import agi.foundation.graphics.TimeChangedEventArgs;
import agi.foundation.graphics.advanced.ScreenOverlayOrigin;
import agi.foundation.graphics.awt.Insight3D;
import agi.foundation.graphics.renderer.Texture2D;
import agi.foundation.propagators.Sgp4Propagator;
import agi.foundation.stk.StkSatelliteDatabase;
import agi.foundation.stk.StkSatelliteDatabaseEntry;
import agi.foundation.time.Duration;
import agi.foundation.time.JulianDate;
import agi.foundation.time.TimeStandard;

public class LotsOfSatellites extends JFrame {
    public LotsOfSatellites() {
        setTitle("Lots of Satellites");
        setSize(792, 566);

        m_insight3D = new Insight3D();
        m_insight3D.setSize(792, 566);

        setLayout(new BorderLayout());
        add(m_insight3D, BorderLayout.CENTER);

        setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);

        SceneManager.addTimeChanged(EventHandler.of(this::onTimeChanged));

        m_animation = new SimulationAnimation();
        SceneManager.setAnimation(m_animation);

        // Create overlay toolbar and panels
        m_overlayToolbar = new OverlayToolbar(m_insight3D);
        m_overlayToolbar.getOverlay().setOrigin(ScreenOverlayOrigin.BOTTOM_CENTER);

        // Add additional toolbar buttons

        // Number of Satellites Button
        m_overlayToolbar.addButton(//
                getDataFilePath("Textures/OverlayToolbar/manysatellites.png"), //
                getDataFilePath("Textures/OverlayToolbar/fewsatellites.png"), //
                Action.of(this::toggleNumberOfSatellites));

        // Show/Hide Access Button
        m_overlayToolbar.addButton(//
                getDataFilePath("Textures/OverlayToolbar/noshowaccess.png"), //
                getDataFilePath("Textures/OverlayToolbar/showaccess.png"), //
                Action.of(this::toggleComputeAccess));

        // Initialize the text panel
        m_textPanel = new TextureScreenOverlay(0, 0, 80, 35);
        m_textPanel.setOrigin(ScreenOverlayOrigin.TOP_RIGHT);
        m_textPanel.setBorderSize(2);
        m_textPanel.setBorderColor(Color.GRAY);
        m_textPanel.setBorderTranslucency(0.6f);
        m_textPanel.setColor(Color.GRAY);
        m_textPanel.setTranslucency(0.4f);
        SceneManager.getScreenOverlays().add(m_textPanel);

        // Show label for the moon
        Scene scene = m_insight3D.getScene();
        scene.getCentralBodies().get(CentralBodiesFacet.getFromContext().getMoon()).setShowLabel(true);

        // Create a marker primitive for the facility at Bells Beach Australia
        EarthCentralBody earth = CentralBodiesFacet.getFromContext().getEarth();

        Cartographic facilityPosition = new Cartographic(Trig.degreesToRadians(144.2829), Trig.degreesToRadians(-38.3697), 0.0);

        Texture2D facilityTexture = SceneManager.getTextures().fromUri(getDataFilePath("Markers/Facility.png"));

        MarkerBatchPrimitive marker = new MarkerBatchPrimitive(SetHint.INFREQUENT);
        marker.setTexture(facilityTexture);
        marker.set(Arrays.asList(earth.getShape().cartographicToCartesian(facilityPosition)));

        SceneManager.getPrimitives().add(marker);

        PointCartographic point = new PointCartographic(earth, facilityPosition);
        Axes topographic = new AxesNorthEastDown(earth, point);
        ReferenceFrame facilityTopo = new ReferenceFrame(point, topographic);

        m_fixedToFacilityTopoEvaluator = GeometryTransformer.getReferenceFrameTransformation(earth.getFixedFrame(), facilityTopo);
        Axes temeAxes = earth.getTrueEquatorMeanEquinoxFrame().getAxes();
        m_temeToFixedEvaluator = GeometryTransformer.getAxesTransformation(temeAxes, earth.getFixedFrame().getAxes());
        m_showAccess = true;
        m_satellites = new Satellites();
        createSatellites("stkSatDb");

        setVisible(true);

        // This Render() is needed so that the stars will show.
        scene.render();
    }

    public static String getDataPath() {
        return "Data";
    }

    public static String getDataFilePath(String path) {
        return new File(getDataPath(), path).getAbsolutePath();
    }

    /**
     * Load TLEs from a satellite database and creates marker primitives for
     * each satellite
     */
    private void createSatellites(String fileName) {
        m_satellites.clear();

        JulianDate epoch = null;

        StkSatelliteDatabase db = new StkSatelliteDatabase(getDataFilePath("SatelliteDatabase"), fileName);
        for (StkSatelliteDatabaseEntry entry : db.getEntries()) {
            if (entry.getTwoLineElementSet() != null) {
                Sgp4Propagator propagator = new Sgp4Propagator(entry.getTwoLineElementSet());

                if (epoch == null) {
                    epoch = propagator.getInitialConditions().getEpoch();
                }

                Duration epochDifference = epoch.subtract(propagator.getInitialConditions().getEpoch());

                if (Duration.lessThan(epochDifference, Duration.fromDays(1))) {
                    m_satellites.add(propagator.getEvaluator(), entry.getTwoLineElementSet().getEpoch());
                }
            }
        }

        setText(m_satellites.getCount());
        JulianDate time = epoch.toTimeStandard(TimeStandard.getInternationalAtomicTime());

        // Set epoch time
        m_animation.pause();
        m_animation.setStartTime(time);
        m_animation.setTime(time);
        m_animation.setEndTime(time.addDays(1.0));
        m_animation.playForward();
    }

    private void onTimeChanged(Object sender, TimeChangedEventArgs e) {
        if (m_temeToFixedEvaluator == null) {
            return;
        }

        JulianDate date = e.getTime();
        UnitQuaternion temeToFixedQuat = m_temeToFixedEvaluator.evaluate(date);
        Matrix3By3 temeToFixed = new Matrix3By3(temeToFixedQuat);

        KinematicTransformation transformation = m_fixedToFacilityTopoEvaluator.evaluate(date, 0);

        ArrayList<Integer> satellitesToRemove = null;

        m_satellites.clearPositions();

        for (int i = 0; i < m_satellites.getCount(); ++i) {
            MotionEvaluator1<Cartesian> satellite = m_satellites.getSatellite(i);

            try {
                // Update position of marker representing this satellite
                Cartesian position = satellite.evaluate(date).rotate(temeToFixed);

                // Compute access from satellite to facility -
                if (m_showAccess) {
                    Cartesian positionInTopo = transformation.transform(position);
                    AzimuthElevationRange azimuthElevationRange = new AzimuthElevationRange(positionInTopo);
                    m_satellites.appendPosition(position, azimuthElevationRange.getElevation() > 0.0);
                } else {
                    m_satellites.appendPosition(position, false);
                }
            } catch (Exception ex) {
                if (satellitesToRemove == null) {
                    satellitesToRemove = new ArrayList<>();
                }
                satellitesToRemove.add(i);
            }
        }

        // Remove satellites that could not be evaluated
        if (satellitesToRemove != null) {
            m_satellites.removeUsingIndexList(satellitesToRemove);
            setText(m_satellites.getCount());
        }

        m_satellites.setMarkerBatches();
    }

    /**
     * Updates the number of satellites on the text panel
     */
    private void setText(int number) {
        if (m_textOverlay != null) {
            m_textPanel.getOverlays().remove(m_textOverlay);
            m_textOverlay = null;
        }

        Font font = new Font("Arial", Font.BOLD, 11);
        String text = "Satellites:\n" + number;
        Dimension textSize = Insight3DHelper.measureString(text, font);
        BufferedImage textBitmap = new BufferedImage(textSize.width, textSize.height, BufferedImage.TYPE_INT_ARGB);

        Graphics2D gfx = textBitmap.createGraphics();
        gfx.setColor(Color.BLACK);
        gfx.setFont(font);

        String[] splitText = text.split("\n");
        int lineHeight = gfx.getFontMetrics().getAscent() + gfx.getFontMetrics().getDescent();
        int maxAdvance = gfx.getFontMetrics().getMaxAdvance();
        int currentLineY = lineHeight;
        for (String textLine : splitText) {
            gfx.drawString(textLine, maxAdvance, currentLineY);
            currentLineY += lineHeight;
        }

        m_textOverlay = new TextureScreenOverlay(0, 0, textSize.width, textSize.height);
        m_textOverlay.setOrigin(ScreenOverlayOrigin.CENTER);
        m_textOverlay.setTexture(SceneManager.getTextures().fromBitmap(textBitmap));
        m_textPanel.getOverlays().add(m_textOverlay);
    }

    // Button actions
    private void toggleNumberOfSatellites() {
        m_showAllSatellites = !m_showAllSatellites;

        if (m_showAllSatellites) {
            createSatellites("stkAllTLE");
        } else {
            createSatellites("stkSatDb");
        }
    }

    private void toggleComputeAccess() {
        m_showAccess = !m_showAccess;

        if (!m_showAccess) {
            m_satellites.clearAccesses();
        }
    }

    private static final long serialVersionUID = 1L;
    private final Insight3D m_insight3D;
    private final SimulationAnimation m_animation;

    private final ReferenceFrameEvaluator m_fixedToFacilityTopoEvaluator;
    private final AxesEvaluator m_temeToFixedEvaluator;

    private final Satellites m_satellites;
    private final OverlayToolbar m_overlayToolbar;
    private final TextureScreenOverlay m_textPanel;
    private TextureScreenOverlay m_textOverlay;

    private boolean m_showAllSatellites = false;
    private boolean m_showAccess = true;
}