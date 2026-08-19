package agi.examples.trajectoryoptimizationdemo;

import java.awt.BorderLayout;
import java.awt.Color;
import java.awt.Dimension;
import java.awt.Font;
import java.awt.Graphics2D;
import java.awt.GridBagConstraints;
import java.awt.Insets;
import java.awt.image.BufferedImage;
import java.io.File;
import java.net.URI;
import java.util.ArrayList;
import java.util.List;

import javax.swing.JButton;
import javax.swing.JFrame;
import javax.swing.JPanel;
import javax.swing.JScrollPane;
import javax.swing.JSplitPane;
import javax.swing.JTable;
import javax.swing.table.AbstractTableModel;

import agi.examples.OverlayToolbar;
import agi.foundation.DateMotionCollection1;
import agi.foundation.EvaluatorGroup;
import agi.foundation.Motion1;
import agi.foundation.Trig;
import agi.foundation.celestial.CentralBodiesFacet;
import agi.foundation.celestial.EarthCentralBody;
import agi.foundation.celestial.SunCentralBody;
import agi.foundation.celestial.WorldGeodeticSystem1984;
import agi.foundation.compatibility.Action;
import agi.foundation.compatibility.DoWorkEventHandler;
import agi.foundation.compatibility.EventHandler;
import agi.foundation.compatibility.PointF;
import agi.foundation.compatibility.RunWorkerCompletedEventHandler;
import agi.foundation.coordinates.Cartesian;
import agi.foundation.coordinates.KeplerianElements;
import agi.foundation.coordinates.UnitCartesian;
import agi.foundation.geometry.PointInterpolator;
import agi.foundation.geometry.PointFixedOffset;
import agi.foundation.geometry.ReferenceFrame;
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
import agi.foundation.graphics.advanced.Origin;
import agi.foundation.graphics.advanced.ScreenOverlayOrigin;
import agi.foundation.graphics.awt.Insight3D;
import agi.foundation.graphics.renderer.Texture2D;
import agi.foundation.infrastructure.threading.BackgroundCalculation;
import agi.foundation.numericalmethods.MultivariableFunctionSolverIterationResults;
import agi.foundation.numericalmethods.MultivariableFunctionSolverResults;
import agi.foundation.numericalmethods.OptimizerMultivariableFunctionResults;
import agi.foundation.numericalmethods.ParameterOptimizerIterationResults;
import agi.foundation.numericalmethods.SolvableMultivariableFunctionResults;
import agi.foundation.numericalmethods.TargetedSegmentListDifferentialCorrectorResults;
import agi.foundation.numericalmethods.TargetedSegmentListParameterOptimizerResults;
import agi.foundation.numericalmethods.TranslationalMotionInterpolator;
import agi.foundation.numericalmethods.advanced.LagrangePolynomialApproximation;
import agi.foundation.platforms.ConstantGraphicsParameter;
import agi.foundation.platforms.DisplayParameters;
import agi.foundation.platforms.MarkerGraphics;
import agi.foundation.platforms.MarkerGraphicsExtension;
import agi.foundation.platforms.ModelGraphics;
import agi.foundation.platforms.ModelGraphicsExtension;
import agi.foundation.platforms.Platform;
import agi.foundation.platforms.PointGraphics;
import agi.foundation.platforms.PointGraphicsExtension;
import agi.foundation.platforms.ServiceProviderDisplay;
import agi.foundation.platforms.TextGraphics;
import agi.foundation.platforms.TextGraphicsExtension;
import agi.foundation.segmentpropagation.SegmentListResults;
import agi.foundation.segmentpropagation.SegmentPropagator;
import agi.foundation.segmentpropagation.TargetedSegmentList;
import agi.foundation.segmentpropagation.TargetedSegmentListResults;
import agi.foundation.time.GregorianDate;
import agi.foundation.time.JulianDate;

public class TrajectoryOptimizationDemo extends JFrame {
	/**
     * Constructs the demo application.
     */
	public TrajectoryOptimizationDemo() {
	setTitle("Trajectory Optimization Demo");
    setSize(1170, 480);
    getContentPane().setLayout(new BorderLayout());
    setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);

    m_insight3D = new Insight3D();

    setUpGui();
    
    m_defaultStart = GregorianDate.getNow();
    m_defaultEnd = m_defaultStart.addDays(3.0);
    
    m_animation = new SimulationAnimation();
    SceneManager.setAnimation(m_animation);

    m_display = new ServiceProviderDisplay();
    
    // Create overlay toolbar and panels.
    m_overlayToolbar = new OverlayToolbar(m_insight3D);
    m_overlayToolbar.getOverlay().setOrigin(ScreenOverlayOrigin.BOTTOM_CENTER);
    m_overlayToolbar.addButton(getDataFilePath("Textures/OverlayToolbar/fewsatellites.png"), Action.of(this::satellite));
    
    // Initialize the text panel.
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

    // Show label for the moon.
    m_insight3D.getScene().getCentralBodies().get(CentralBodiesFacet.getFromContext().getMoon()).setShowLabel(true);

    // Sets the element ID to be used in the Trajectory Optimizer.
    m_elementID = "Satellite";

    // Subscribe to the time changed event.
    SceneManager.addTimeChanged(EventHandler.of(this::onTimeChanged));
    
    // Set the start and stop times.
    m_animation.setTime(m_defaultStart.toJulianDate());
    m_animation.setStartTime(m_defaultStart.toJulianDate());
    m_animation.setEndTime(m_defaultEnd.toJulianDate());
    
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
    
    // Call method that creates and sets up targeted segment list with optimizer.
    m_targetedSegmentList = TrajectoryHelper.createTrajectoryOptimizer(m_animation.getStartTime());
    m_segmentPropagator = m_targetedSegmentList.getSegmentPropagator(new EvaluatorGroup());
	}
	
	public static String getDataPath() {
        return "Data";
    }

    public static String getDataFilePath(String path) {
        return new File(getDataPath(), path).getAbsolutePath();
    }
    
    public static URI getDataFileUri(String path) {
        return new File(getDataFilePath(path)).toURI();
    }
    
    public void satellite() {
        if (m_platform == null) {
            return;
        }
        
        PointFixedOffset fixedOffset = new PointFixedOffset(m_platform.getReferenceFrame(), new Cartesian(10000000.0, 10000000.0, 0.0));
        m_insight3D.getScene().getCamera().view(m_platform.getOrientationAxes(), fixedOffset, m_platform.getLocationPoint());
    }
    
    
    public void setUpGui()
    {
        Insets defaultInsets = new Insets(5, 5, 5, 5);
        
        m_leftRightSplitPane = new JSplitPane();
    	m_leftRightSplitPane.setResizeWeight(0.03);
        getContentPane().add(m_leftRightSplitPane, BorderLayout.CENTER);

        m_leftRightSplitPane.setRightComponent(m_insight3D);
        m_insight3D.setSize(850, 480);

        m_topBottomSplitPane = new JSplitPane(JSplitPane.VERTICAL_SPLIT);
        m_leftRightSplitPane.setLeftComponent(m_topBottomSplitPane);
        
        m_buttonPanel = new JPanel();
        m_topBottomSplitPane.setTopComponent(m_buttonPanel);
        
        GridBagConstraints buttonConstraints = new GridBagConstraints();
        buttonConstraints.anchor = GridBagConstraints.LINE_END;
        buttonConstraints.insets = defaultInsets;
        buttonConstraints.gridx = 0;
        buttonConstraints.gridwidth = GridBagConstraints.REMAINDER;
        
        m_runIterationButton = new JButton("Run One Iteration");
        m_runIterationButton.addActionListener(e -> {
            runOneIteration();
            m_iterationNumber++;
        });
        m_buttonPanel.add(m_runIterationButton, buttonConstraints);
        
        m_resetOptimizerButton = new JButton("Reset Optimizer");
        m_resetOptimizerButton.addActionListener(e -> {
            // Clean up the previous run's graphics
            for (Primitive primitive : m_primitivesAddedToScene) {
                SceneManager.getPrimitives().remove(primitive);
            }
            m_primitivesAddedToScene.clear();
            if (m_platform != null) {
                m_display.getServiceProviders().remove(m_platform);
            }

            // Get new segment propagator from targeted segment list to reset.
            m_segmentPropagator = m_targetedSegmentList.getSegmentPropagator(new EvaluatorGroup());
            m_runIterationButton.setEnabled(true);
            m_iterationNumber = 0;
        });
        m_buttonPanel.add(m_resetOptimizerButton, buttonConstraints);
        
        m_screenshotButton = new JButton("Take Screenshot");
        m_screenshotButton.addActionListener(e -> {
            m_insight3D.getScene().getCamera().getSnapshot().saveToClipboard();
        });
        m_buttonPanel.add(m_screenshotButton, buttonConstraints);
        
        m_outputTableModel = new OptimizerResultsTable();
        m_outputTable = new JTable(m_outputTableModel);
        
        JScrollPane scrollPane = new JScrollPane(m_outputTable);;
        m_topBottomSplitPane.setBottomComponent(scrollPane);
    }

    /**
     * This gets called every time the time is changed in Insight3D.
     */
    private void onTimeChanged(Object sender, TimeChangedEventArgs e) {
        m_display.update(e.getTime());
        setText(e.getTime().toGregorianDate().toString(DateFormat));
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
    
    private static class OptimizerResultsTable extends AbstractTableModel {
        private static final long serialVersionUID = 1L;

        private List<String> m_outputResults = new ArrayList<String>();
        
        private static final String[] columnLabels = new String[] {
                "Parameters",
                "Output Results",
        };
        
        private static final String[] rowLabels = new String[] {
                "Iteration Number:",
                "Optimizer Converged?",
                "Total propellant usage:",
                "Total delta-v:",
                "",
                "Initial Rp:",
                "Initial i:",
                "",
                "Maneuver 1 delta-v:",
                "Rp after Maneuver 1:",
                "i after Maneuver 1:",
                "",
                "Maneuver 2 delta-v:",
                "Rp after Maneuver 2:",
                "i after Maneuver 2:",
                "",
                "Maneuver 3 delta-v:",
                "Rp after Maneuver 3:",
                "i after Maneuver 3:",
                "",
        };
        

        @Override
        public int getColumnCount() {
            return columnLabels.length;
        }

        @Override
        public String getColumnName(int column) {
            return columnLabels[column];
        }

        @Override
        public int getRowCount() {
            return m_outputResults.size();
        }

        @Override
        public Object getValueAt(int rowIndex, int columnIndex) {
            switch (columnIndex) {
            case 0:
                return rowLabels[rowIndex];
            case 1:
                return m_outputResults.get(rowIndex);
            default:
                return null;
            }
        }

        public void setOutputResults(List<String> outputResults) {
            m_outputResults = outputResults;
            fireTableDataChanged();
        }
    }
    
    private void runOneIteration() {
        EarthCentralBody earth = CentralBodiesFacet.getFromContext().getEarth();
        final BackgroundCalculation backgroundCalculation = new BackgroundCalculation();
        
        // Need to use list as a wrapper in Java.
        List<TargetedSegmentListResults> propagationResultsList = new ArrayList<TargetedSegmentListResults>();
        backgroundCalculation.addDoWork(DoWorkEventHandler.of((sender, e) -> {
            // #region Propagate Optimizer Iteration
            // Actually propagate.
            TargetedSegmentListResults propagationResults = (TargetedSegmentListResults)m_segmentPropagator.propagate((SegmentListResults)null, m_segmentPropagator.getOriginalConfiguration(), backgroundCalculation);
            // #endregion
            propagationResultsList.add(propagationResults);
        }));
        
        backgroundCalculation.addRunWorkerCompleted(RunWorkerCompletedEventHandler.of((sender, e) -> {
         // When finished, draw the satellite and output the iteration results to the table.
            Boolean converged = drawSatelliteAndOutputResults(propagationResultsList.get(0), earth.getInertialFrame());

            // If not converged, allow the user to run another iteration.
            if (!converged) {
                m_runIterationButton.setEnabled(true);
            }
        }));

        m_runIterationButton.setEnabled(false);
        backgroundCalculation.runWorkerAsync();
    }
    
    private Boolean drawSatelliteAndOutputResults(TargetedSegmentListResults propagationResults, ReferenceFrame frame) {
        // Clean up the previous run's graphics
        for (Primitive primitive : m_primitivesAddedToScene) {
            SceneManager.getPrimitives().remove(primitive);
        }
        m_primitivesAddedToScene.clear();
        if (m_platform != null) {
            m_display.getServiceProviders().remove(m_platform);
        }
        
        // Clean up output table from previous run.
        m_outputResults.clear();
    
        // #region Extract Optimizer Results
        // Extract information from propagation results.
        // Outer optimizer results are extracted first.
        TargetedSegmentListParameterOptimizerResults outerOptimizerOperatorResults = (TargetedSegmentListParameterOptimizerResults)propagationResults.getOperatorResults().get(0);
        Boolean converged = outerOptimizerOperatorResults.getConverged();
        
        MultivariableFunctionSolverResults<ParameterOptimizerIterationResults> outerOptimizerResults = outerOptimizerOperatorResults.getOptimizerResults();
        ParameterOptimizerIterationResults outerFinalIterationResults = outerOptimizerResults.getFinalIteration();

        OptimizerMultivariableFunctionResults outerFunctionResults = outerFinalIterationResults.getFunctionResult();
        Double outerCostFunctionResults = outerFunctionResults.getCostFunctionValue();
        
        double[] outerInequalityResults = outerFunctionResults.getInequalityConstraintValues();
        double radiusPeriapsisAfterManeuver1 = outerInequalityResults[0];
        double targetedInclinationAfterManeuver1 = Trig.radiansToDegrees(outerInequalityResults[1]);
        double radiusPeriapsisAfterManeuver2 = outerInequalityResults[2];
        double targetedInclinationAfterManeuver2 = Trig.radiansToDegrees(outerInequalityResults[4]);
        
        // Extract results for nested maneuver 1 targeted segment list.
        TargetedSegmentListResults maneuver1TargetedSegmentResults = (TargetedSegmentListResults)propagationResults.getAdditionalSegmentResults().get(2);
        TargetedSegmentListDifferentialCorrectorResults maneuver1OperatorResults = (TargetedSegmentListDifferentialCorrectorResults)maneuver1TargetedSegmentResults.getOperatorResults().get(0);
        //Boolean maneuver1Converged = maneuver1OperatorResults.getConverged();

        MultivariableFunctionSolverResults<MultivariableFunctionSolverIterationResults> maneuver1FunctionSolverResults = maneuver1OperatorResults.getFunctionSolverResults();
        MultivariableFunctionSolverIterationResults maneuver1FinalIterationResults = maneuver1FunctionSolverResults.getFinalIteration();
        SolvableMultivariableFunctionResults maneuver1FunctionResult = maneuver1FinalIterationResults.getFunctionResult();
        double[] deltaV1Results = maneuver1FunctionResult.getVariablesUsed();
        double deltaV1Magnitude = Math.sqrt(deltaV1Results[0] * deltaV1Results[0] + deltaV1Results[1] * deltaV1Results[1]);
        
        // Extract results for nested maneuver 2 targeted segment list.
        TargetedSegmentListResults maneuver2TargetedSegmentResults = (TargetedSegmentListResults)propagationResults.getAdditionalSegmentResults().get(5);
        TargetedSegmentListDifferentialCorrectorResults maneuver2OperatorResults = (TargetedSegmentListDifferentialCorrectorResults)maneuver2TargetedSegmentResults.getOperatorResults().get(0);
        //Boolean maneuver2Converged = maneuver2OperatorResults.getConverged();

        MultivariableFunctionSolverResults<MultivariableFunctionSolverIterationResults> maneuver2FunctionSolverResults = maneuver2OperatorResults.getFunctionSolverResults();
        MultivariableFunctionSolverIterationResults maneuver2FinalIterationResults = maneuver2FunctionSolverResults.getFinalIteration();
        SolvableMultivariableFunctionResults maneuver2FunctionResult = maneuver2FinalIterationResults.getFunctionResult();
        double[] deltaV2Results = maneuver2FunctionResult.getVariablesUsed();
        double deltaV2Magnitude = Math.sqrt(deltaV2Results[0] * deltaV2Results[0] + deltaV2Results[1] * deltaV2Results[1]);
        
        // Extract results for nested maneuver 3 targeted segment list.
        TargetedSegmentListResults maneuver3TargetedSegmentResults = (TargetedSegmentListResults)propagationResults.getAdditionalSegmentResults().get(8);
        TargetedSegmentListDifferentialCorrectorResults maneuver3OperatorResults = (TargetedSegmentListDifferentialCorrectorResults)maneuver3TargetedSegmentResults.getOperatorResults().get(0);
        //Boolean maneuver3Converged = maneuver3OperatorResults.getConverged();

        MultivariableFunctionSolverResults<MultivariableFunctionSolverIterationResults> maneuver3FunctionSolverResults = maneuver3OperatorResults.getFunctionSolverResults();
        MultivariableFunctionSolverIterationResults maneuver3FinalIterationResults = maneuver3FunctionSolverResults.getFinalIteration();
        SolvableMultivariableFunctionResults maneuver3FunctionResult = maneuver3FinalIterationResults.getFunctionResult();
        double[] deltaV3Results = maneuver3FunctionResult.getVariablesUsed();
        double deltaV3Magnitude = Math.sqrt(deltaV3Results[0] * deltaV3Results[0] + deltaV3Results[1] * deltaV3Results[1]);

        double totalDeltaV = deltaV1Magnitude + deltaV2Magnitude + deltaV3Magnitude;
        
        // Extract ephemeris results for each propagate segment.
        DateMotionCollection1<Cartesian> overallTrajectoryResults = propagationResults.getDateMotionCollectionOfOverallTrajectory(m_elementID);
        List<DateMotionCollection1<Cartesian>> individualSegmentResults = new ArrayList<DateMotionCollection1<Cartesian>>();
        individualSegmentResults.add(propagationResults.getAdditionalSegmentResults().get(1).getDateMotionCollectionOfOverallTrajectory(m_elementID));
        individualSegmentResults.add(propagationResults.getAdditionalSegmentResults().get(3).getDateMotionCollectionOfOverallTrajectory(m_elementID));
        individualSegmentResults.add(propagationResults.getAdditionalSegmentResults().get(4).getDateMotionCollectionOfOverallTrajectory(m_elementID));
        individualSegmentResults.add(propagationResults.getAdditionalSegmentResults().get(6).getDateMotionCollectionOfOverallTrajectory(m_elementID));
        individualSegmentResults.add(propagationResults.getAdditionalSegmentResults().get(7).getDateMotionCollectionOfOverallTrajectory(m_elementID));
        individualSegmentResults.add(propagationResults.getAdditionalSegmentResults().get(9).getDateMotionCollectionOfOverallTrajectory(m_elementID));

        int ephemerisCount = overallTrajectoryResults.getCount();
        Motion1<Cartesian> finalState = overallTrajectoryResults.getMotions().get(ephemerisCount - 1);
        double earthGravitationalParameter = WorldGeodeticSystem1984.GravitationalParameter;
        KeplerianElements finalKeplerianElements = new KeplerianElements(finalState, earthGravitationalParameter);
        // #endregion
        
        // Draw the orbit using different colors for each propagated segment.
        List<Color> colorList = new ArrayList<Color>();
        colorList.add(Color.ORANGE);
        colorList.add(Color.RED);
        colorList.add(Color.CYAN);
        colorList.add(Color.YELLOW);
        colorList.add(Color.MAGENTA);
        colorList.add(Color.BLUE);
        for (int i = 0; i < colorList.size(); i++)
        {
            plotEphemeris(individualSegmentResults.get(i), colorList.get(i));
        }
        
        // Set up platform and marker for satellite's dynamic positions.
        plotSatellite(overallTrajectoryResults, individualSegmentResults);
        
        m_outputResults.add(Integer.toString(m_iterationNumber));
        m_outputResults.add(Boolean.toString(converged));
        m_outputResults.add(Double.toString(outerCostFunctionResults) + " kg");
        m_outputResults.add(Double.toString(totalDeltaV) + " m/s");
        
        m_outputResults.add("");
        m_outputResults.add(Double.toString(6678000.0 / 1000.0) + " km");
        m_outputResults.add(Double.toString(57.0) + " deg.");
        
        m_outputResults.add("");
        m_outputResults.add(Double.toString(deltaV1Magnitude) + " m/s");
        m_outputResults.add(Double.toString(radiusPeriapsisAfterManeuver1 / 1000.0) + " km");
        m_outputResults.add(Double.toString(targetedInclinationAfterManeuver1) + " deg.");
        
        m_outputResults.add("");
        m_outputResults.add(Double.toString(deltaV2Magnitude) + " m/s");
        m_outputResults.add(Double.toString(radiusPeriapsisAfterManeuver2 / 1000.0) + " km");
        m_outputResults.add(Double.toString(targetedInclinationAfterManeuver2) + " deg.");

        m_outputResults.add("");
        m_outputResults.add(Double.toString(deltaV3Magnitude) + " m/s");
        m_outputResults.add(Double.toString(finalKeplerianElements.getSemimajorAxis() / 1000.0) + " km");
        m_outputResults.add(Double.toString(Trig.radiansToDegrees(finalKeplerianElements.getInclination())) + " deg.");
        
        m_outputTableModel.setOutputResults(m_outputResults);
        
        return converged;
    }
    
    private void plotEphemeris(DateMotionCollection1<Cartesian> ephemeris, Color color)
    {
        EarthCentralBody earth = CentralBodiesFacet.getFromContext().getEarth();
        List<PathPoint> points = new ArrayList<PathPoint>();
        for (int i = 0; i < ephemeris.getCount(); i++)
        {
            points.add(new PathPointBuilder(ephemeris.getValues().get(i), ephemeris.getDates().get(i), color).toPathPoint());
        }
        PathPrimitive path = new PathPrimitive();
        path.setReferenceFrame(earth.getInertialFrame());
        path.addRangeToFront(points);
        SceneManager.getPrimitives().add(path);
        m_primitivesAddedToScene.add(path);
    }
    
    private void plotSatellite(DateMotionCollection1<Cartesian> ephemeris, List<DateMotionCollection1<Cartesian>> individualSegmentEphemeris)
    {
        // Put a marker where the satellite is at a given time.
        EarthCentralBody earth = CentralBodiesFacet.getFromContext().getEarth();
        LagrangePolynomialApproximation interpolationAlgorithm = new LagrangePolynomialApproximation();

        // Need to give interpolator segment boundary times to avoid issues with interpolating ephemeris from segment list.
        List<JulianDate> segmentBoundaryTimes = new ArrayList<JulianDate>();
        segmentBoundaryTimes.add(individualSegmentEphemeris.get(0).getDates().get(0));
        segmentBoundaryTimes.add(individualSegmentEphemeris.get(1).getDates().get(0));
        segmentBoundaryTimes.add(individualSegmentEphemeris.get(2).getDates().get(0));
        segmentBoundaryTimes.add(individualSegmentEphemeris.get(3).getDates().get(0));
        segmentBoundaryTimes.add(individualSegmentEphemeris.get(4).getDates().get(0));
        segmentBoundaryTimes.add(individualSegmentEphemeris.get(5).getDates().get(0));
        
        TranslationalMotionInterpolator interpolator = new TranslationalMotionInterpolator(interpolationAlgorithm, 12, ephemeris, segmentBoundaryTimes);
        m_platform = new Platform();
        m_platform.setLocationPoint(new PointInterpolator(earth.getInertialFrame(), interpolator));
        m_platform.setName("GEO satellite");
        m_platform.setOrientationAxes(earth.getInertialFrame().getAxes());
        
        Texture2D satelliteTexture = SceneManager.getTextures().fromUri(getDataFilePath("/Markers/smallsatellite.png"));

        URI modelUri = getDataFileUri("/Models/satellite.mdl");
        
        // Create a point for each satellite.
        PointGraphics pointGraphics = new PointGraphics();
        pointGraphics.setColor(new ConstantGraphicsParameter<Color>(Color.RED));
        pointGraphics.setDisplayOutline(new ConstantGraphicsParameter<>(true));
        pointGraphics.setPixelSize(new ConstantGraphicsParameter<>(4.0f));
        pointGraphics.setOutlineWidth(new ConstantGraphicsParameter<>(2.0f));
        
        DisplayParameters pointDisplayParameters = new DisplayParameters();
        // Show the point when the camera is very far away.
        pointDisplayParameters.setMinimumDistance(new ConstantGraphicsParameter<>(750000000.0));
        pointGraphics.setDisplayParameters(pointDisplayParameters);
        
        m_platform.getExtensions().add(new PointGraphicsExtension(pointGraphics));
        
        // Create a marker for each satellite.
        MarkerGraphics markerGraphics = new MarkerGraphics();
        markerGraphics.setTexture(new ConstantGraphicsParameter<Texture2D>(satelliteTexture));
        
        DisplayParameters markerDisplayParameters = new DisplayParameters();
        markerDisplayParameters.setMinimumDistance(new ConstantGraphicsParameter<>(17500000.0));
        markerDisplayParameters.setMaximumDistance(pointGraphics.getDisplayParameters().getMinimumDistance());
        
        markerGraphics.setDisplayParameters(markerDisplayParameters);
        m_platform.getExtensions().add(new MarkerGraphicsExtension(markerGraphics));
        
        // Create a model for the satellite.
        ModelGraphics modelGraphics = new ModelGraphics();
        modelGraphics.setScale(new ConstantGraphicsParameter<>(50000.0));
        modelGraphics.setUri(new ConstantGraphicsParameter<URI>(modelUri));
        
        DisplayParameters modelDisplayParameters = new DisplayParameters();
        // Show the model when the camera is close.
        modelDisplayParameters.setMaximumDistance(markerGraphics.getDisplayParameters().getMinimumDistance());
        modelGraphics.setDisplayParameters(modelDisplayParameters);
        
        m_platform.getExtensions().add(new ModelGraphicsExtension(modelGraphics));
        
        // Create a label for the satellite.
        TextGraphics textGraphics = new TextGraphics();
        textGraphics.setColor(pointGraphics.getColor());
        textGraphics.setFont(new ConstantGraphicsParameter<Font>(new Font("Times New Roman", Font.PLAIN, 8)));
        textGraphics.setOutline(new ConstantGraphicsParameter<>(true));
        textGraphics.setOutlineColor(new ConstantGraphicsParameter<>(Color.BLACK));
        textGraphics.setText(new ConstantGraphicsParameter<>(m_platform.getName()));
        textGraphics.setOrigin(new ConstantGraphicsParameter<>(Origin.TOP_CENTER));
        textGraphics.setPixelOffset(new ConstantGraphicsParameter<>(new PointF(0F, -8F)));
        
        // Show the label when at a medium distance.
        textGraphics.getDisplayParameters().setMaximumDistance(pointGraphics.getDisplayParameters().getMinimumDistance());
        m_platform.getExtensions().add(new TextGraphicsExtension(textGraphics));
        
        m_display.getServiceProviders().add(m_platform);
        m_display.applyChanges();

        // Set the date to the start of the ephemeris.
        SceneManager.getAnimation().setTime(ephemeris.getDates().get(0).addSeconds(1.0));
    }
    
	private static final long serialVersionUID = 1L;
	
	private final Insight3D m_insight3D;
	private final SimulationAnimation m_animation;
	private final ServiceProviderDisplay m_display;
	private final OverlayToolbar m_overlayToolbar;
	
	private final String m_elementID;
	private final GregorianDate m_defaultStart;
    private final GregorianDate m_defaultEnd;
    private static final String DateFormat = "MMM dd, yyyy HH:mm:ss";
    
    private final TextureScreenOverlay m_textOverlay;
    private final Font m_dateTimeFont;
    
    private int m_iterationNumber;
    private final ArrayList<Primitive> m_primitivesAddedToScene = new ArrayList<>();
    private Platform m_platform;
    
    private TargetedSegmentList m_targetedSegmentList;
    private SegmentPropagator m_segmentPropagator;
    
    private JSplitPane m_topBottomSplitPane;
    private JSplitPane m_leftRightSplitPane;
    
    private JPanel m_buttonPanel;
    private JButton m_runIterationButton;
    private JButton m_resetOptimizerButton;
    private JButton m_screenshotButton;
    
    private JTable m_outputTable;
    private OptimizerResultsTable m_outputTableModel;
    private List<String> m_outputResults = new ArrayList<String>();
}
