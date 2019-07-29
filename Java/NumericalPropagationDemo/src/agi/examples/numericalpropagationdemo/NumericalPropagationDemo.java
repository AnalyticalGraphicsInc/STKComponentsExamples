package agi.examples.numericalpropagationdemo;

import java.awt.BorderLayout;
import java.awt.Color;
import java.awt.Dimension;
import java.awt.FlowLayout;
import java.awt.Font;
import java.awt.Graphics2D;
import java.awt.GridBagConstraints;
import java.awt.GridBagLayout;
import java.awt.Insets;
import java.awt.image.BufferedImage;
import java.io.File;
import java.util.ArrayList;

import javax.swing.JButton;
import javax.swing.JFormattedTextField;
import javax.swing.JFrame;
import javax.swing.JLabel;
import javax.swing.JOptionPane;
import javax.swing.JPanel;
import javax.swing.JProgressBar;
import javax.swing.JSplitPane;
import javax.swing.JTextField;

import agi.examples.OverlayToolbar;
import agi.foundation.DateMotionCollection1;
import agi.foundation.Motion1;
import agi.foundation.celestial.CentralBodiesFacet;
import agi.foundation.celestial.CentralBody;
import agi.foundation.celestial.EarthCentralBody;
import agi.foundation.celestial.JplDE430;
import agi.foundation.celestial.SunCentralBody;
import agi.foundation.celestial.WorldGeodeticSystem1984;
import agi.foundation.compatibility.DoWorkEventHandler;
import agi.foundation.compatibility.EventHandler;
import agi.foundation.compatibility.ProgressChangedEventHandler;
import agi.foundation.compatibility.RunWorkerCompletedEventHandler;
import agi.foundation.coordinates.Cartesian;
import agi.foundation.coordinates.KeplerianElements;
import agi.foundation.coordinates.UnitCartesian;
import agi.foundation.geometry.PointInterpolator;
import agi.foundation.geometry.ReferenceFrame;
import agi.foundation.geometry.ScalarFixed;
import agi.foundation.geometry.VectorEvaluator;
import agi.foundation.geometry.VectorTrueDisplacement;
import agi.foundation.graphics.PathPoint;
import agi.foundation.graphics.PathPointBuilder;
import agi.foundation.graphics.PathPrimitive;
import agi.foundation.graphics.Primitive;
import agi.foundation.graphics.SceneManager;
import agi.foundation.graphics.SimulationAnimation;
import agi.foundation.graphics.TextureScreenOverlay;
import agi.foundation.graphics.TimeChangedEventArgs;
import agi.foundation.graphics.advanced.ScreenOverlayOrigin;
import agi.foundation.graphics.awt.Insight3D;
import agi.foundation.graphics.renderer.Texture2D;
import agi.foundation.infrastructure.threading.BackgroundCalculation;
import agi.foundation.numericalmethods.TranslationalMotionInterpolator;
import agi.foundation.numericalmethods.advanced.LagrangePolynomialApproximation;
import agi.foundation.platforms.ConstantGraphicsParameter;
import agi.foundation.platforms.MarkerGraphics;
import agi.foundation.platforms.MarkerGraphicsExtension;
import agi.foundation.platforms.Platform;
import agi.foundation.platforms.ServiceProviderDisplay;
import agi.foundation.propagators.NumericalPropagationStateHistory;
import agi.foundation.propagators.NumericalPropagator;
import agi.foundation.propagators.NumericalPropagatorDefinition;
import agi.foundation.propagators.PropagationEventIndication;
import agi.foundation.propagators.PropagationNewtonianPoint;
import agi.foundation.time.GregorianDate;
import agi.foundation.time.JulianDate;

public class NumericalPropagationDemo extends JFrame {
    /**
     * Constructs the demo application
     */
    public NumericalPropagationDemo() {
        setTitle("Numerical Propagation Demo");
        setSize(890, 460);
        getContentPane().setLayout(new BorderLayout());
        setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);

        m_insight3D = new Insight3D();

        // All of the auto-generated GUI code
        setUpGui();

        m_defaultStart = GregorianDate.getNow();
        m_defaultEnd = m_defaultStart.addDays(1);

        m_animation = new SimulationAnimation();
        SceneManager.setAnimation(m_animation);

        m_display = new ServiceProviderDisplay();

        m_forceModelSettings = new ForceModelSettings(s_jplData, getDataFilePath("EarthGravityField_EGM2008.grv"));
        m_integratorSettings = new IntegratorSettings();
        m_mass.setText("500");
        m_crossSectionalArea.setText("20");

        // Create overlay toolbar and panels
        m_overlayToolbar = new OverlayToolbar(m_insight3D);
        m_overlayToolbar.getOverlay().setOrigin(ScreenOverlayOrigin.BOTTOM_CENTER);

        // Initalize the text panel
        TextureScreenOverlay textPanel = new TextureScreenOverlay(0D, 0D, 200D, 35D);
        textPanel.setOrigin(ScreenOverlayOrigin.TOP_RIGHT);
        textPanel.setBorderSize(2);
        textPanel.setBorderColor(Color.GRAY);
        textPanel.setBorderTranslucency(0.6f);
        textPanel.setColor(Color.GRAY);
        textPanel.setTranslucency(0.4f);
        SceneManager.getScreenOverlays().add(textPanel);

        m_dateTimeFont = new Font("Courier New", Font.BOLD, 12);
        m_textOverlay = new TextureScreenOverlay(0, 0, 200, 30);
        m_textOverlay.setOrigin(ScreenOverlayOrigin.CENTER);
        m_textOverlay.setBorderSize(0);
        textPanel.getOverlays().add(m_textOverlay);

        // Show label for the moon
        m_insight3D.getScene().getCentralBodies().get(CentralBodiesFacet.getFromContext().getMoon()).setShowLabel(true);

        // Sets the element ID to be used in the NumericalPropagator
        m_elementID = "Satellite";

        // Subscribe to the time changed event
        SceneManager.addTimeChanged(EventHandler.of(this::onTimeChanged));

        // Set the start and stop times
        m_start.setText(m_defaultStart.toString(DateFormat));
        m_end.setText(m_defaultEnd.toString(DateFormat));

        m_animation.setTime(m_defaultStart.toJulianDate());
        m_animation.setStartTime(m_defaultStart.toJulianDate());
        m_animation.setEndTime(m_defaultEnd.toJulianDate());

        // Populate default values
        m_semiMajorAxis.setText(Double.toString(m_defaultElements.getSemimajorAxis()));
        m_eccentricity.setText(Double.toString(m_defaultElements.getEccentricity()));
        m_inclination.setText(Double.toString(m_defaultElements.getInclination()));
        m_argOfPeriapsis.setText(Double.toString(m_defaultElements.getArgumentOfPeriapsis()));
        m_raan.setText(Double.toString(m_defaultElements.getRightAscensionOfAscendingNode()));
        m_trueAnomaly.setText(Double.toString(m_defaultElements.getTrueAnomaly()));

        // Dynamically set the camera's position and direction so that the camera will always be pointed at the daylit portion of the earth.
        EarthCentralBody earth = CentralBodiesFacet.getFromContext().getEarth();
        SunCentralBody sun = CentralBodiesFacet.getFromContext().getSun();
        VectorTrueDisplacement earthToSunVector = new VectorTrueDisplacement(earth.getCenterOfMassPoint(), sun.getCenterOfMassPoint());
        VectorEvaluator earthToSunEvaluator = earthToSunVector.getEvaluator();
        Cartesian earthToSunCartesian = earthToSunEvaluator.evaluate(new JulianDate(m_defaultStart));
        UnitCartesian earthToSunUnitCartesian = new UnitCartesian(earthToSunCartesian);
        UnitCartesian cameraDirection = earthToSunUnitCartesian.invert();
        Cartesian cameraPosition = new Cartesian(earthToSunUnitCartesian.getX() * 50000000, earthToSunUnitCartesian.getY() * 50000000,
                earthToSunUnitCartesian.getZ() * 50000000);
        m_insight3D.getScene().getCamera().setPosition(cameraPosition);
        m_insight3D.getScene().getCamera().setDirection(cameraDirection);

        setVisible(true);
    }

    public static String getDataPath() {
        return "Data";
    }

    public static String getDataFilePath(String path) {
        return new File(getDataPath(), path).getAbsolutePath();
    }

    /**
     * The auto-generated GUI code, along with button actions.
     */
    private void setUpGui() {
        m_splitPane = new JSplitPane();
        m_splitPane.setResizeWeight(0.11);
        getContentPane().add(m_splitPane, BorderLayout.CENTER);

        m_splitPane.setRightComponent(m_insight3D);
        m_insight3D.setSize(792, 566);

        m_panel = new JPanel();
        m_splitPane.setLeftComponent(m_panel);
        GridBagLayout gbl_panel = new GridBagLayout();
        gbl_panel.columnWidths = new int[] {
                78,
                76,
                -9,
                0
        };
        gbl_panel.rowHeights = new int[] {
                0,
                23,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                31,
                0
        };
        gbl_panel.columnWeights = new double[] {
                0.0,
                1.0,
                0.0,
                Double.MIN_VALUE
        };
        gbl_panel.rowWeights = new double[] {
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                Double.MIN_VALUE
        };
        m_panel.setLayout(gbl_panel);

        JLabel lblNewLabel = new JLabel("Start/Epoch:");
        GridBagConstraints gbc_lblNewLabel = new GridBagConstraints();
        gbc_lblNewLabel.insets = new Insets(0, 0, 5, 5);
        gbc_lblNewLabel.anchor = GridBagConstraints.EAST;
        gbc_lblNewLabel.gridx = 0;
        gbc_lblNewLabel.gridy = 0;
        m_panel.add(lblNewLabel, gbc_lblNewLabel);

        m_start = new JFormattedTextField();
        GridBagConstraints gbc_m_startDate = new GridBagConstraints();
        gbc_m_startDate.insets = new Insets(0, 0, 5, 5);
        gbc_m_startDate.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_startDate.gridx = 1;
        gbc_m_startDate.gridy = 0;
        m_panel.add(m_start, gbc_m_startDate);

        JLabel lblNewLabel_1 = new JLabel("End:");
        GridBagConstraints gbc_lblNewLabel_1 = new GridBagConstraints();
        gbc_lblNewLabel_1.anchor = GridBagConstraints.EAST;
        gbc_lblNewLabel_1.insets = new Insets(0, 0, 5, 5);
        gbc_lblNewLabel_1.gridx = 0;
        gbc_lblNewLabel_1.gridy = 1;
        m_panel.add(lblNewLabel_1, gbc_lblNewLabel_1);

        m_end = new JFormattedTextField();
        GridBagConstraints gbc_m_endDate = new GridBagConstraints();
        gbc_m_endDate.insets = new Insets(0, 0, 5, 5);
        gbc_m_endDate.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_endDate.gridx = 1;
        gbc_m_endDate.gridy = 1;
        m_panel.add(m_end, gbc_m_endDate);

        JPanel panel_1 = new JPanel();
        FlowLayout flowLayout = (FlowLayout) panel_1.getLayout();
        flowLayout.setHgap(0);
        flowLayout.setAlignment(FlowLayout.LEADING);
        flowLayout.setVgap(0);
        GridBagConstraints gbc_panel_1 = new GridBagConstraints();
        gbc_panel_1.insets = new Insets(0, 0, 5, 0);
        gbc_panel_1.anchor = GridBagConstraints.WEST;
        gbc_panel_1.gridx = 2;
        gbc_panel_1.gridy = 1;
        m_panel.add(panel_1, gbc_panel_1);

        JLabel lblNewLabel_2 = new JLabel("Semi-Major Axis (m):");
        GridBagConstraints gbc_lblNewLabel_2 = new GridBagConstraints();
        gbc_lblNewLabel_2.anchor = GridBagConstraints.EAST;
        gbc_lblNewLabel_2.insets = new Insets(0, 0, 5, 5);
        gbc_lblNewLabel_2.gridx = 0;
        gbc_lblNewLabel_2.gridy = 2;
        m_panel.add(lblNewLabel_2, gbc_lblNewLabel_2);

        m_semiMajorAxis = new JTextField();
        GridBagConstraints gbc_m_semiMajorAxis = new GridBagConstraints();
        gbc_m_semiMajorAxis.insets = new Insets(0, 0, 5, 5);
        gbc_m_semiMajorAxis.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_semiMajorAxis.gridx = 1;
        gbc_m_semiMajorAxis.gridy = 2;
        m_panel.add(m_semiMajorAxis, gbc_m_semiMajorAxis);
        m_semiMajorAxis.setColumns(10);

        JLabel lblNewLabel_3 = new JLabel("Eccentricity:");
        GridBagConstraints gbc_lblNewLabel_3 = new GridBagConstraints();
        gbc_lblNewLabel_3.anchor = GridBagConstraints.EAST;
        gbc_lblNewLabel_3.insets = new Insets(0, 0, 5, 5);
        gbc_lblNewLabel_3.gridx = 0;
        gbc_lblNewLabel_3.gridy = 3;
        m_panel.add(lblNewLabel_3, gbc_lblNewLabel_3);

        m_eccentricity = new JTextField();
        GridBagConstraints gbc_m_eccentricity = new GridBagConstraints();
        gbc_m_eccentricity.insets = new Insets(0, 0, 5, 5);
        gbc_m_eccentricity.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_eccentricity.gridx = 1;
        gbc_m_eccentricity.gridy = 3;
        m_panel.add(m_eccentricity, gbc_m_eccentricity);
        m_eccentricity.setColumns(10);

        JLabel lblNewLabel_4 = new JLabel("Inclination (rad):");
        GridBagConstraints gbc_lblNewLabel_4 = new GridBagConstraints();
        gbc_lblNewLabel_4.anchor = GridBagConstraints.EAST;
        gbc_lblNewLabel_4.insets = new Insets(0, 0, 5, 5);
        gbc_lblNewLabel_4.gridx = 0;
        gbc_lblNewLabel_4.gridy = 4;
        m_panel.add(lblNewLabel_4, gbc_lblNewLabel_4);

        m_inclination = new JTextField();
        GridBagConstraints gbc_m_inclination = new GridBagConstraints();
        gbc_m_inclination.insets = new Insets(0, 0, 5, 5);
        gbc_m_inclination.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_inclination.gridx = 1;
        gbc_m_inclination.gridy = 4;
        m_panel.add(m_inclination, gbc_m_inclination);
        m_inclination.setColumns(10);

        JLabel lblNewLabel_5 = new JLabel("Arg. of Periapsis (rad):");
        GridBagConstraints gbc_lblNewLabel_5 = new GridBagConstraints();
        gbc_lblNewLabel_5.anchor = GridBagConstraints.EAST;
        gbc_lblNewLabel_5.insets = new Insets(0, 0, 5, 5);
        gbc_lblNewLabel_5.gridx = 0;
        gbc_lblNewLabel_5.gridy = 5;
        m_panel.add(lblNewLabel_5, gbc_lblNewLabel_5);

        m_argOfPeriapsis = new JTextField();
        GridBagConstraints gbc_m_argOfPeriapsis = new GridBagConstraints();
        gbc_m_argOfPeriapsis.insets = new Insets(0, 0, 5, 5);
        gbc_m_argOfPeriapsis.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_argOfPeriapsis.gridx = 1;
        gbc_m_argOfPeriapsis.gridy = 5;
        m_panel.add(m_argOfPeriapsis, gbc_m_argOfPeriapsis);
        m_argOfPeriapsis.setColumns(10);

        JLabel lblNewLabel_6 = new JLabel("RAAN (rad):");
        GridBagConstraints gbc_lblNewLabel_6 = new GridBagConstraints();
        gbc_lblNewLabel_6.anchor = GridBagConstraints.EAST;
        gbc_lblNewLabel_6.insets = new Insets(0, 0, 5, 5);
        gbc_lblNewLabel_6.gridx = 0;
        gbc_lblNewLabel_6.gridy = 6;
        m_panel.add(lblNewLabel_6, gbc_lblNewLabel_6);

        m_raan = new JTextField();
        GridBagConstraints gbc_m_raan = new GridBagConstraints();
        gbc_m_raan.insets = new Insets(0, 0, 5, 5);
        gbc_m_raan.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_raan.gridx = 1;
        gbc_m_raan.gridy = 6;
        m_panel.add(m_raan, gbc_m_raan);
        m_raan.setColumns(10);

        JLabel lblNewLabel_7 = new JLabel("True Anomaly (rad):");
        GridBagConstraints gbc_lblNewLabel_7 = new GridBagConstraints();
        gbc_lblNewLabel_7.anchor = GridBagConstraints.EAST;
        gbc_lblNewLabel_7.insets = new Insets(0, 0, 5, 5);
        gbc_lblNewLabel_7.gridx = 0;
        gbc_lblNewLabel_7.gridy = 7;
        m_panel.add(lblNewLabel_7, gbc_lblNewLabel_7);

        m_trueAnomaly = new JTextField();
        GridBagConstraints gbc_m_trueAnomaly = new GridBagConstraints();
        gbc_m_trueAnomaly.insets = new Insets(0, 0, 5, 5);
        gbc_m_trueAnomaly.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_trueAnomaly.gridx = 1;
        gbc_m_trueAnomaly.gridy = 7;
        m_panel.add(m_trueAnomaly, gbc_m_trueAnomaly);
        m_trueAnomaly.setColumns(10);

        JLabel lblNewLabel_8 = new JLabel("Cross Sectional Area (m*m):");
        GridBagConstraints gbc_lblNewLabel_8 = new GridBagConstraints();
        gbc_lblNewLabel_8.anchor = GridBagConstraints.EAST;
        gbc_lblNewLabel_8.insets = new Insets(0, 0, 5, 5);
        gbc_lblNewLabel_8.gridx = 0;
        gbc_lblNewLabel_8.gridy = 8;
        m_panel.add(lblNewLabel_8, gbc_lblNewLabel_8);

        m_crossSectionalArea = new JTextField();
        GridBagConstraints gbc_m_crossSectionalArea = new GridBagConstraints();
        gbc_m_crossSectionalArea.insets = new Insets(0, 0, 5, 5);
        gbc_m_crossSectionalArea.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_crossSectionalArea.gridx = 1;
        gbc_m_crossSectionalArea.gridy = 8;
        m_panel.add(m_crossSectionalArea, gbc_m_crossSectionalArea);
        m_crossSectionalArea.setColumns(10);

        JLabel lblNewLabel_9 = new JLabel("Mass (kg):");
        GridBagConstraints gbc_lblNewLabel_9 = new GridBagConstraints();
        gbc_lblNewLabel_9.anchor = GridBagConstraints.EAST;
        gbc_lblNewLabel_9.insets = new Insets(0, 0, 5, 5);
        gbc_lblNewLabel_9.gridx = 0;
        gbc_lblNewLabel_9.gridy = 9;
        m_panel.add(lblNewLabel_9, gbc_lblNewLabel_9);

        m_mass = new JTextField();
        GridBagConstraints gbc_m_mass = new GridBagConstraints();
        gbc_m_mass.insets = new Insets(0, 0, 5, 5);
        gbc_m_mass.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_mass.gridx = 1;
        gbc_m_mass.gridy = 9;
        m_panel.add(m_mass, gbc_m_mass);
        m_mass.setColumns(10);

        JButton m_integratorButton = new JButton("Integrator...     ");
        m_integratorButton.addActionListener(arg0 -> {
            m_integratorSettings.setDefaultCloseOperation(JFrame.DISPOSE_ON_CLOSE);
            m_integratorSettings.setVisible(true);
        });
        GridBagConstraints gbc_m_integratorButton = new GridBagConstraints();
        gbc_m_integratorButton.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_integratorButton.insets = new Insets(0, 0, 5, 5);
        gbc_m_integratorButton.gridx = 0;
        gbc_m_integratorButton.gridy = 10;
        m_panel.add(m_integratorButton, gbc_m_integratorButton);

        JButton m_forceModelButton = new JButton("Force Models...");
        m_forceModelButton.addActionListener(e -> {
            m_forceModelSettings.setDefaultCloseOperation(JFrame.DISPOSE_ON_CLOSE);
            m_forceModelSettings.setVisible(true);
        });
        GridBagConstraints gbc_m_forceModelButton = new GridBagConstraints();
        gbc_m_forceModelButton.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_forceModelButton.insets = new Insets(0, 0, 5, 5);
        gbc_m_forceModelButton.gridx = 0;
        gbc_m_forceModelButton.gridy = 11;
        m_panel.add(m_forceModelButton, gbc_m_forceModelButton);

        m_propagate = new JButton("Propagate");
        m_propagate.addActionListener(e -> {
            try {
                propagateSatellite();
            } catch (Exception ex) {
                JOptionPane.showMessageDialog(null, "There was an error propagating the entered elements. Make sure your satellite "
                        + "does not enter the earth and your inputs are all valid. \n" + "Exception: " + ex.getMessage());
            }
        });
        GridBagConstraints gbc_m_propagate = new GridBagConstraints();
        gbc_m_propagate.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_propagate.insets = new Insets(0, 0, 5, 5);
        gbc_m_propagate.gridx = 0;
        gbc_m_propagate.gridy = 12;
        m_panel.add(m_propagate, gbc_m_propagate);

        m_progressBar = new JProgressBar();
        GridBagConstraints gbc_m_progressBar = new GridBagConstraints();
        gbc_m_progressBar.fill = GridBagConstraints.BOTH;
        gbc_m_progressBar.insets = new Insets(0, 0, 0, 5);
        gbc_m_progressBar.gridx = 0;
        gbc_m_progressBar.gridy = 13;
        m_panel.add(m_progressBar, gbc_m_progressBar);
    }

    /**
     * This gets called every time the time is changed in Insight3D.
     */
    private void onTimeChanged(Object sender, TimeChangedEventArgs e) {
        m_display.update(e.getTime());
        setText(e.getTime().toGregorianDate().toString(DateFormat));
    }

    /**
     * Propagate a Platform using a NumericalPropagator configured with the entered KeplerianElements,
     * ForceModels, NumericalIntegrator, and start and stop dates.
     */
    private void propagateSatellite() {
        m_animation.setStartTime(new JulianDate(GregorianDate.parseExact(m_start.getText(), DateFormat, null)));
        m_animation.setEndTime(new JulianDate(GregorianDate.parseExact(m_end.getText(), DateFormat, null)));

        KeplerianElements orbitalElements = getKeplerianElementValues();
        if (orbitalElements == null)
            return;

        Motion1<Cartesian> initialMotion = orbitalElements.toCartesian();
        PropagationNewtonianPoint point = new PropagationNewtonianPoint(m_elementID,
                m_forceModelSettings.getCurrentCentralBody().getInertialFrame(), initialMotion.getValue(),
                initialMotion.getFirstDerivative());
        point.setMass(new ScalarFixed(Double.parseDouble(m_mass.getText())));
        m_forceModelSettings.setForceModelsOnPoint(point, new ScalarFixed(Double.parseDouble(m_crossSectionalArea.getText())));
        final CentralBody primaryCentralBody = m_forceModelSettings.getCurrentCentralBody();

        NumericalPropagatorDefinition state = new NumericalPropagatorDefinition();
        state.getIntegrationElements().add(point);
        state.setIntegrator(m_integratorSettings.getIntegrator());

        final JulianDate start = new JulianDate(GregorianDate.parseExact(m_start.getText(), DateFormat, null));
        final JulianDate end = new JulianDate(GregorianDate.parseExact(m_end.getText(), DateFormat, null));
        state.setEpoch(start);
        final NumericalPropagator propagator = state.createPropagator();
        propagator.addStepTaken(EventHandler.of((e, args) -> {
            // Telling the propagator to stop if we get too close to the central body
            Cartesian position = propagator.getConverter().<Cartesian> convertState(m_elementID, args.getCurrentState()).getValue();
            if (position.getMagnitude() <= primaryCentralBody.getShape().getSemimajorAxisLength() + 10000) {
                args.setIndication(PropagationEventIndication.STOP_PROPAGATION_AFTER_STEP);
            }
        }));

        final PropagateResultStorage resultStorage = new PropagateResultStorage();

        final BackgroundCalculation backgroundCalculation = new BackgroundCalculation();
        backgroundCalculation.addDoWork(DoWorkEventHandler.of((sender, e) -> {
            // actually propagate
            NumericalPropagationStateHistory result = propagator.propagate(end.subtract(start), 1, backgroundCalculation);
            resultStorage.result = result.getDateMotionCollection(m_elementID);
        }));
        backgroundCalculation.addProgressChanged(ProgressChangedEventHandler.of((sender, e) -> {
            m_progressBar.setValue(e.getProgressPercentage());
        }));
        backgroundCalculation.addRunWorkerCompleted(RunWorkerCompletedEventHandler.of((sender, e) -> {
            // when finished, draw the satellite
            drawSatellite(resultStorage.result, primaryCentralBody.getInertialFrame());
            m_propagate.setEnabled(true);
        }));

        m_propagate.setEnabled(false);
        backgroundCalculation.runWorkerAsync();
    }

    private final static class PropagateResultStorage {
        public DateMotionCollection1<Cartesian> result;
    }

    /**
     * Draws a satellite's orbit and a marker for its position in the Insight3D
     * window.
     *
     * @param ephemeris
     *            The ephemeris to display.
     * @param inertialFrame
     *            The intertial frame of the ephemeris.
     */
    private void drawSatellite(DateMotionCollection1<Cartesian> ephemeris, ReferenceFrame inertialFrame) {
        // Clean up the previous run's graphics
        for (Primitive primitive : m_primitivesAddedToScene) {
            SceneManager.getPrimitives().remove(primitive);
        }
        m_primitivesAddedToScene.clear();
        if (m_platform != null) {
            m_display.getServiceProviders().remove(m_platform);
        }

        // Draw the orbit
        ArrayList<PathPoint> points = new ArrayList<>();
        for (int i = 0; i < ephemeris.getCount(); i++) {
            points.add(new PathPointBuilder(ephemeris.getValues().get(i), ephemeris.getDates().get(i)).toPathPoint());
        }
        PathPrimitive path = new PathPrimitive();
        path.setReferenceFrame(inertialFrame);
        path.addRangeToFront(points);
        SceneManager.getPrimitives().add(path);
        m_primitivesAddedToScene.add(path);

        // Put a marker where the satellite is at a given time
        LagrangePolynomialApproximation interpolationAlgorithm = new LagrangePolynomialApproximation();
        TranslationalMotionInterpolator interpolator = new TranslationalMotionInterpolator(interpolationAlgorithm, 2, ephemeris);
        m_platform = new Platform();
        m_platform.setLocationPoint(new PointInterpolator(inertialFrame, interpolator));

        MarkerGraphics markerGraphics = new MarkerGraphics();
        Texture2D texture = SceneManager.getTextures().fromUri(getDataFilePath("Markers/Satellite.png"));
        markerGraphics.setTexture(new ConstantGraphicsParameter<>(texture));
        m_platform.getExtensions().add(new MarkerGraphicsExtension(markerGraphics));
        m_display.getServiceProviders().add(m_platform);
        m_display.applyChanges();

        // Set the date to the start of the ephemeris
        SceneManager.getAnimation().setTime(ephemeris.getDates().get(0));
    }

    /**
     * Generates a KeplerianElements from the values currently in the GUI.
     *
     * @return The KeplerianElements as entered in the GUI.
     */
    private KeplerianElements getKeplerianElementValues() {
        double sma = Double.parseDouble(m_semiMajorAxis.getText());
        double ecc = Double.parseDouble(m_eccentricity.getText());
        double inc = Double.parseDouble(m_inclination.getText());
        double aop = Double.parseDouble(m_argOfPeriapsis.getText());
        double raan = Double.parseDouble(m_raan.getText());
        double ta = Double.parseDouble(m_trueAnomaly.getText());
        double grav = m_forceModelSettings.getCurrentGravitationalParameter();
        return new KeplerianElements(sma, ecc, inc, aop, raan, ta, grav);
    }

    /**
     * Updates the displayed date
     *
     * @param text
     *            The new text to display in the upper-right corner.
     */
    private void setText(String text) {
        Dimension textSize = new Dimension(200, 30);
        BufferedImage textBitmap = new BufferedImage(textSize.width, textSize.height, BufferedImage.TYPE_INT_ARGB);

        Graphics2D gfx = textBitmap.createGraphics();
        gfx.setColor(Color.GREEN);
        gfx.setFont(m_dateTimeFont);
        gfx.drawString(text, 20, 20);
        m_textOverlay.setTexture(SceneManager.getTextures().fromBitmap(textBitmap));
    }

    private static JplDE430 s_jplData;

    static {
        // Load data for central bodies besides the earth, moon, and sun.
        s_jplData = new JplDE430(getDataFilePath("plneph.430"));
        s_jplData.useForCentralBodyPositions(CentralBodiesFacet.getFromContext());
    }

    private static final long serialVersionUID = 1L;

    private final OverlayToolbar m_overlayToolbar;
    private final Insight3D m_insight3D;
    private final ServiceProviderDisplay m_display;
    private final ForceModelSettings m_forceModelSettings;
    private final IntegratorSettings m_integratorSettings;
    private final Font m_dateTimeFont;

    private final String m_elementID;
    private final ArrayList<Primitive> m_primitivesAddedToScene = new ArrayList<>();
    private Platform m_platform;
    private final GregorianDate m_defaultStart;
    private final GregorianDate m_defaultEnd;
    private static final String DateFormat = "MMM dd, yyyy HH:mm:ss";
    private final SimulationAnimation m_animation;

    private JPanel m_panel;
    private JSplitPane m_splitPane;
    private JTextField m_semiMajorAxis;
    private JTextField m_eccentricity;
    private JTextField m_inclination;
    private JTextField m_argOfPeriapsis;
    private JTextField m_raan;
    private JTextField m_trueAnomaly;
    private JTextField m_crossSectionalArea;
    private JTextField m_mass;
    private JButton m_propagate;
    private JFormattedTextField m_end;
    private JFormattedTextField m_start;
    private JProgressBar m_progressBar;

    private final TextureScreenOverlay m_textOverlay;

    private final KeplerianElements m_defaultElements = new KeplerianElements(7800000, 0.1, 0.5, 0, 0, 0,
            WorldGeodeticSystem1984.GravitationalParameter);
}