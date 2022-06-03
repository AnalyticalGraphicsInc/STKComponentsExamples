package agi.examples.communicationsdemo;

import java.awt.Color;
import java.awt.Dimension;
import java.awt.Font;
import java.io.PrintWriter;
import java.io.StringWriter;
import java.util.ArrayList;
import java.util.Hashtable;

import javax.swing.GroupLayout;
import javax.swing.GroupLayout.Alignment;
import javax.swing.GroupLayout.ParallelGroup;
import javax.swing.GroupLayout.SequentialGroup;
import javax.swing.JButton;
import javax.swing.JComboBox;
import javax.swing.JFrame;
import javax.swing.JLabel;
import javax.swing.JOptionPane;
import javax.swing.JScrollPane;
import javax.swing.JSlider;
import javax.swing.JTextArea;
import javax.swing.SwingConstants;
import javax.swing.SwingUtilities;

import agi.examples.LeapSecondsFacetHelper;
import agi.examples.OverlayToolbar;
import agi.examples.TwoLineElementSetHelper;
import agi.foundation.Trig;
import agi.foundation.TypeLiteral;
import agi.foundation.access.AccessConstraint;
import agi.foundation.access.AccessQuery;
import agi.foundation.access.constraints.CentralBodyObstructionConstraint;
import agi.foundation.access.constraints.ScalarConstraint;
import agi.foundation.celestial.CentralBodiesFacet;
import agi.foundation.celestial.EarthCentralBody;
import agi.foundation.communications.CommunicationAnalysis;
import agi.foundation.communications.CommunicationLinkCollection;
import agi.foundation.communications.CommunicationSystem;
import agi.foundation.communications.LinkBudgetScalars;
import agi.foundation.communications.SimpleDigitalTransmitter;
import agi.foundation.communications.SimpleReceiver;
import agi.foundation.communications.antennas.IsotropicGainPattern;
import agi.foundation.communications.signalprocessing.IntendedSignalByFrequency;
import agi.foundation.compatibility.EventHandler;
import agi.foundation.compatibility.PointF;
import agi.foundation.coordinates.Cartographic;
import agi.foundation.geometry.PointCartographic;
import agi.foundation.graphics.SceneManager;
import agi.foundation.graphics.SimulationAnimation;
import agi.foundation.graphics.TimeChangedEventArgs;
import agi.foundation.graphics.advanced.Origin;
import agi.foundation.graphics.advanced.ScreenOverlayOrigin;
import agi.foundation.graphics.advanced.SimulationAnimationCycle;
import agi.foundation.graphics.awt.Insight3D;
import agi.foundation.graphics.renderer.Texture2D;
import agi.foundation.graphics.renderer.TextureFilter2D;
import agi.foundation.graphics.renderer.TextureWrap;
import agi.foundation.infrastructure.ExtensibleObject;
import agi.foundation.infrastructure.INameService;
import agi.foundation.infrastructure.ServiceHelper;
import agi.foundation.platforms.AccessConstraintsExtension;
import agi.foundation.platforms.AccessQueryGraphicsParameter;
import agi.foundation.platforms.ConstantGraphicsParameter;
import agi.foundation.platforms.LineGraphics;
import agi.foundation.platforms.LinkGraphicsExtension;
import agi.foundation.platforms.MarkerGraphics;
import agi.foundation.platforms.MarkerGraphicsExtension;
import agi.foundation.platforms.ServiceProviderDisplay;
import agi.foundation.platforms.TextGraphics;
import agi.foundation.platforms.TextGraphicsExtension;
import agi.foundation.propagators.TwoLineElementSet;
import agi.foundation.time.Duration;
import agi.foundation.time.GregorianDate;
import agi.foundation.time.JulianDate;

/**
 * This example application simulates a call from a location in Dessie, Ehtiopia to a satellite phone in Bangalore, India.
 * This is a direct connection call with no ground based telecom switches. All frequencies, modulations, data rates and
 * power (EIRP) settings were retrieved from
 * http://www.icao.int/anb/panels/acp/wg/m/iridium_swg/ird-03/IRD-SWG03-WP02-Draft%20Iridium%20AMS(R)S%20Tech%20Manual%20110105.pdf
 */
public class Main extends JFrame {
    public static void main(String[] args) {
        SwingUtilities.invokeLater(Main::createAndShowGUI);
    }

    private static void createAndShowGUI() {
        // install an exception handler that will create and display a dialog
        // containing the exception stack trace
        Thread.currentThread().setUncaughtExceptionHandler((t, e) -> {
            e.printStackTrace();

            StringWriter sw = new StringWriter();
            e.printStackTrace(new PrintWriter(sw));

            JTextArea textArea = new JTextArea(sw.toString());
            JScrollPane scrollPane = new JScrollPane(textArea);
            scrollPane.setPreferredSize(new Dimension(700, 300));
            textArea.setEditable(false);
            JOptionPane.showMessageDialog(null, scrollPane, "Unhandled Exception", JOptionPane.ERROR_MESSAGE);
        });

        // startup data configuration

        // Update LeapSecond.dat, and use it in the current calculation context.
        LeapSecondsFacetHelper.getLeapSeconds().useInCurrentContext();

        Main app = new Main();
        app.setSize(1024, 768);
        app.setVisible(true);
    }

    public Main() {
        createControls();

        // We don't have a call placed yet.
        m_hangUpButton.setEnabled(false);
        m_callPlaced = false;

        // Load a texture which we will use for our phone locations.
        m_phoneTexture = SceneManager.getTextures().fromUri("Data/Markers/facility.png");

        // Create a ServiceProviderDisplay to be used for visualization.
        m_display = new ServiceProviderDisplay();

        // Create the font to use for labeling
        m_labelFont = new Font("Microsoft Sans Serif", Font.PLAIN, 10);

        // The call will be taking place at a frequency of 1620.5e6.
        m_intendedSignal = new IntendedSignalByFrequency(1620.5e6);

        // Create the transmitting phone that we will use for our call and add it to the
        // display.
        m_transmittingPhone = createTransmittingPhone();
        m_display.getServiceProviders().add(m_transmittingPhone);

        // Do the same for the receiving phone.
        m_receivingPhone = createReceivingPhone();
        m_display.getServiceProviders().add(m_receivingPhone);

        // Create an instance of IridiumSatellite for each of the three satellites we will
        // be using and add them to the display.

        // IridiumSatellite is a Platform-based class that adds default graphics and a
        // Transceiver object for communication.

        JulianDate analysisDate = new JulianDate(new GregorianDate(2011, 8, 2, 18, 1, 0));

        ArrayList<TwoLineElementSet> iridium49Tles = TwoLineElementSetHelper.getTles("25108", analysisDate);
        IridiumSatellite iridium49 = new IridiumSatellite("Iridium 49", iridium49Tles, m_labelFont, m_intendedSignal.getTargetFrequency());
        m_display.getServiceProviders().add(iridium49);

        ArrayList<TwoLineElementSet> iridium58Tles = TwoLineElementSetHelper.getTles("25274", analysisDate);
        IridiumSatellite iridium58 = new IridiumSatellite("Iridium 58", iridium58Tles, m_labelFont, m_intendedSignal.getTargetFrequency());
        m_display.getServiceProviders().add(iridium58);

        ArrayList<TwoLineElementSet> iridium4Tles = TwoLineElementSetHelper.getTles("24796", analysisDate);
        IridiumSatellite iridium4 = new IridiumSatellite("Iridium 4", iridium4Tles, m_labelFont, m_intendedSignal.getTargetFrequency());
        m_display.getServiceProviders().add(iridium4);

        // If the TLE epoch is too far from our expected analysis date, then we are
        // offline and loading cached data. Adjust the analysis date to match.
        if (iridium49Tles.get(0).getEpoch().daysDifference(analysisDate) > 5) {
            analysisDate = iridium49Tles.get(0).getEpoch();
        }

        // Iridium 49 will be the receiving end of our caller's "uplink".
        // Modify the receiving antenna to be isotropic.
        iridium49.getTransceiver().setInputAntennaGainPattern(new IsotropicGainPattern());

        // Iridium 4 will be the transmitting end of our call receiver's "downlink".
        // Modify the transmitting antenna to be isotropic.
        iridium4.getTransceiver().setOutputAntennaGainPattern(new IsotropicGainPattern());

        // Now that we've created all of our definition objects, we need to build the
        // links between them that make up our communication system.
        m_communicationSystem = new CommunicationSystem();

        // Add a link for each hop in the chain.
        // This could have been accomplished with a single call to addChain.
        // However, since we plan on further configuring each of the individual links
        // generated for us, we add them one at a time.

        CommunicationLinkCollection linkCollection = m_communicationSystem.getLinks();
        m_callerToIridium49UpLink = linkCollection.add("Uplink to Iridium 49", m_transmittingPhone, iridium49.getTransceiver());
        m_iridium49To58Crosslink = linkCollection.add("Iridium 49 -> Iridium 58", iridium49.getTransceiver(), iridium58.getTransceiver());
        m_iridium58To4Crosslink = linkCollection.add("Iridium 58 -> Iridium 4", iridium58.getTransceiver(), iridium4.getTransceiver());
        m_iridium4ToReceiverDownLink = linkCollection.add("Downlink from Iridium 4", iridium4.getTransceiver(), m_receivingPhone);

        // Now that we have the links, we can add an AccessConstraintsExtension to them so
        // that each link is only valid if they have line of sight to each other.
        EarthCentralBody earth = CentralBodiesFacet.getFromContext().getEarth();

        AccessConstraint callerToIridium49Constraint = new CentralBodyObstructionConstraint(m_callerToIridium49UpLink, earth);
        m_callerToIridium49UpLink.getExtensions().add(new AccessConstraintsExtension(callerToIridium49Constraint));

        AccessConstraint iridium49To58Constraint = new CentralBodyObstructionConstraint(m_iridium49To58Crosslink, earth);
        m_iridium49To58Crosslink.getExtensions().add(new AccessConstraintsExtension(iridium49To58Constraint));

        AccessConstraint iridium58To4Constraint = new CentralBodyObstructionConstraint(m_iridium58To4Crosslink, earth);
        m_iridium58To4Crosslink.getExtensions().add(new AccessConstraintsExtension(iridium58To4Constraint));

        AccessConstraint iridium4ToReceiverConstraint = new CentralBodyObstructionConstraint(m_iridium4ToReceiverDownLink, earth);
        m_iridium4ToReceiverDownLink.getExtensions().add(new AccessConstraintsExtension(iridium4ToReceiverConstraint));

        m_linkComboBox.addItem(new ComboBoxItem(m_callerToIridium49UpLink));
        m_linkComboBox.addItem(new ComboBoxItem(m_iridium49To58Crosslink));
        m_linkComboBox.addItem(new ComboBoxItem(m_iridium58To4Crosslink));
        m_linkComboBox.addItem(new ComboBoxItem(m_iridium4ToReceiverDownLink));
        m_linkComboBox.setSelectedItem(m_linkComboBox.getItemAt(3));

        // Even though we haven't added any link graphics yet, we will later, so add them
        // to the display now.
        m_display.getServiceProviders().add(m_callerToIridium49UpLink);
        m_display.getServiceProviders().add(m_iridium49To58Crosslink);
        m_display.getServiceProviders().add(m_iridium58To4Crosslink);
        m_display.getServiceProviders().add(m_iridium4ToReceiverDownLink);

        // While we have set the location for our two phones and the satellite
        // transceivers, what we haven't done is assign their orientations. This can be
        // done automatically using the configureAntennaTargeting method. Note that
        // configureAntennaTargeting is a best effort method and returns status
        // information in regards to any problems it encounters. We don't check them here
        // simply because we know it will succeed.
        m_communicationSystem.configureAntennaTargeting();

        // Now that our initial configuration is complete, make sure we call applyChanges
        // on the display so that the visualization is created.
        m_display.applyChanges();

        // Update the display to the current time.
        m_display.update(SceneManager.getTime());

        // Set up the animation time for our call.
        SimulationAnimation animation = new SimulationAnimation();
        animation.setStartTime(analysisDate);
        animation.setEndTime(analysisDate.addMinutes(15.0));
        animation.setTimeStep(Duration.fromSeconds(0.5));
        animation.setStartCycle(SimulationAnimationCycle.LOOP);
        animation.setEndCycle(SimulationAnimationCycle.LOOP);
        SceneManager.setAnimation(animation);

        // Subscribe to the time changed event so we can update our display.
        SceneManager.addTimeChanged(EventHandler.of(this::sceneManagerTimeChanged));

        // Reset to the beginning.
        animation.reset();

        // Configure the animation toolbar that overlays the 3D view.
        @SuppressWarnings("resource")
        OverlayToolbar animationToolbar = new OverlayToolbar(m_insight3D);
        animationToolbar.getOverlay().setOrigin(ScreenOverlayOrigin.BOTTOM_CENTER);

        // Zoom to a location that includes both the caller and receiver.
        m_insight3D.getScene().getCamera().viewExtent(earth, Trig.degreesToRadians(39.6333), Trig.degreesToRadians(11.1333),
                Trig.degreesToRadians(77.5833), Trig.degreesToRadians(12.9833));

        // Move the camera further back so that all satellites are visible.
        m_insight3D.getScene().getCamera().setDistance(m_insight3D.getScene().getCamera().getDistance() + 5000000.0);

        // Turn off lighting since it's the middle of the night and we want to be able to
        // see everything,
        m_insight3D.getScene().getLighting().setEnabled(false);

        // Create our link budget overlay helper which will display
        // the complete link budget for a selected link on top of the display.
        m_linkBudgetOverlayHelper = new LinkBudgetOverlayHelper(m_labelFont);

        // Hide it until the call is initiated
        m_linkBudgetOverlayHelper.getOverlay().setDisplay(false);

        // Add the actual overlay to Insight3D
        SceneManager.getScreenOverlays().add(m_linkBudgetOverlayHelper.getOverlay());

        layoutFrame();
        setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        pack();
    }

    private void createControls() {
        m_transmitPowerTrackBar = new JSlider(SwingConstants.VERTICAL, 0, 10, 5);
        m_transmitPowerTrackBar.setMajorTickSpacing(1);
        m_transmitPowerTrackBar.setPaintTicks(true);
        m_transmitPowerTrackBar.setSnapToTicks(true);

        // Create the label table
        Hashtable<Integer, JLabel> labelTable = new Hashtable<>();
        labelTable.put(0, new JLabel("0 dB"));
        labelTable.put(5, new JLabel("5 dB"));
        labelTable.put(10, new JLabel("10 dB"));
        m_transmitPowerTrackBar.setLabelTable(labelTable);
        m_transmitPowerTrackBar.setPaintLabels(true);

        m_transmitPowerTrackBar.addChangeListener(e -> onTransmitPowerValueChanged());

        m_linkComboBox = new JComboBox<>();
        m_linkComboBox.addItemListener(e -> onLinkComboBoxSelectedIndexChanged());

        m_placeCallButton = new JButton("Place Call");
        m_placeCallButton.addActionListener(e -> onPlaceCallClick());

        m_hangUpButton = new JButton("Hang Up");
        m_hangUpButton.addActionListener(e -> onHangUpClick());

        m_insight3D = new Insight3D();
    }

    private void layoutFrame() {
        JLabel transmitPowerLabel = new JLabel("Transmit Power");
        transmitPowerLabel.setLabelFor(m_transmitPowerTrackBar);

        JLabel displayedLinkBudgetLabel = new JLabel("Displayed Link Budget");
        displayedLinkBudgetLabel.setLabelFor(m_linkComboBox);

        GroupLayout layout = new GroupLayout(getContentPane());
        layout.setAutoCreateGaps(true);
        layout.setAutoCreateContainerGaps(true);

        ParallelGroup hLeft = layout.createParallelGroup(Alignment.LEADING, false);
        hLeft.addComponent(transmitPowerLabel);
        hLeft.addComponent(m_transmitPowerTrackBar);
        hLeft.addComponent(m_placeCallButton);
        hLeft.addComponent(m_hangUpButton);
        hLeft.addComponent(displayedLinkBudgetLabel);
        hLeft.addComponent(m_linkComboBox);

        SequentialGroup vLeft = layout.createSequentialGroup();
        vLeft.addComponent(transmitPowerLabel, GroupLayout.PREFERRED_SIZE, GroupLayout.PREFERRED_SIZE, GroupLayout.PREFERRED_SIZE);
        vLeft.addComponent(m_transmitPowerTrackBar, GroupLayout.PREFERRED_SIZE, GroupLayout.PREFERRED_SIZE, GroupLayout.PREFERRED_SIZE);
        vLeft.addComponent(m_placeCallButton, GroupLayout.PREFERRED_SIZE, GroupLayout.PREFERRED_SIZE, GroupLayout.PREFERRED_SIZE);
        vLeft.addComponent(m_hangUpButton, GroupLayout.PREFERRED_SIZE, GroupLayout.PREFERRED_SIZE, GroupLayout.PREFERRED_SIZE);
        vLeft.addComponent(displayedLinkBudgetLabel, GroupLayout.PREFERRED_SIZE, GroupLayout.PREFERRED_SIZE, GroupLayout.PREFERRED_SIZE);
        vLeft.addComponent(m_linkComboBox, GroupLayout.PREFERRED_SIZE, GroupLayout.PREFERRED_SIZE, GroupLayout.PREFERRED_SIZE);

        SequentialGroup hGroup = layout.createSequentialGroup();
        ParallelGroup vGroup = layout.createParallelGroup();
        hGroup.addGroup(hLeft);
        hGroup.addComponent(m_insight3D);

        vGroup.addGroup(vLeft);
        vGroup.addComponent(m_insight3D);

        layout.linkSize(m_placeCallButton, m_hangUpButton);
        layout.setHorizontalGroup(hGroup);
        layout.setVerticalGroup(vGroup);
        getContentPane().setLayout(layout);
    }

    /**
     * Creates a new instance of our transmitting phone
     * with default configuration and graphics.
     * @return A new SimpleDigitalTransmitter.
     */
    private SimpleDigitalTransmitter createTransmittingPhone() {
        EarthCentralBody earth = CentralBodiesFacet.getFromContext().getEarth();

        // Create a new SimpleDigitalTransmitter and assign it's basic property.
        // Even though we are using a static location for our transmitter,
        // it can easily be changed to a moving one by simply modifying the
        // LocationPoint to something else, for example a route generated
        // with the Route Design Library.
        SimpleDigitalTransmitter phone = new SimpleDigitalTransmitter();
        phone.setName("Dessie, Ethiopia");
        double longitude = Trig.degreesToRadians(39.6333);
        double latitude = Trig.degreesToRadians(11.1333);
        phone.setLocationPoint(new PointCartographic(earth, new Cartographic(longitude, latitude, 0.0)));
        phone.setCarrierFrequency(m_intendedSignal.getTargetFrequency());
        phone.setEffectiveIsotropicRadiatedPower(CommunicationAnalysis.fromDecibels(m_transmitPowerTrackBar.getValue()));
        phone.setDataRate(50000.0);

        // Add a default marker
        MarkerGraphics markerGraphics = new MarkerGraphics();
        markerGraphics.setTexture(new ConstantGraphicsParameter<>(m_phoneTexture));
        phone.getExtensions().add(new MarkerGraphicsExtension(markerGraphics));

        // Add a label based on the name and show just below the marker.
        TextGraphics textGraphics = new TextGraphics();
        textGraphics.setColor(new ConstantGraphicsParameter<>(Color.YELLOW));
        textGraphics.setFont(new ConstantGraphicsParameter<>(m_labelFont));
        textGraphics.setOutline(new ConstantGraphicsParameter<>(true));
        textGraphics.setOutlineColor(new ConstantGraphicsParameter<>(Color.BLACK));
        textGraphics.setText(new ConstantGraphicsParameter<>(phone.getName()));
        textGraphics.setOrigin(new ConstantGraphicsParameter<>(Origin.TOP_CENTER));
        textGraphics.setPixelOffset(new ConstantGraphicsParameter<>(new PointF(0, -m_phoneTexture.getTemplate().getHeight() / 2)));
        textGraphics.getDisplayParameters().setMaximumDistance(new ConstantGraphicsParameter<>(75000000.0));
        if (TextureFilter2D.supported(TextureWrap.CLAMP_TO_EDGE)) {
            textGraphics.setTextureFilter(new ConstantGraphicsParameter<>(TextureFilter2D.getNearestClampToEdge()));
        }
        phone.getExtensions().add(new TextGraphicsExtension(textGraphics));

        return phone;
    }

    /**
     * Creates a new instance of our receiving phone
     * with default configuration and graphics.
     * @return A new SimpleReceiver.
     */
    private SimpleReceiver createReceivingPhone() {
        EarthCentralBody earth = CentralBodiesFacet.getFromContext().getEarth();

        // Create a receiving phone with a gain of 100 and a noisefactor of 2 - adding 290 Kelvin worth of noise to the call.
        // Even though we are using a static location for our receiver,
        // it can easily be changed to a moving one by simply modifying the
        // LocationPoint to something else, for example a route generated
        // with the Route Design Library.
        SimpleReceiver phone = new SimpleReceiver();
        phone.setName("Bangalore, India");
        double longitude = Trig.degreesToRadians(77.5833);
        double latitude = Trig.degreesToRadians(12.9833);
        phone.setLocationPoint(new PointCartographic(earth, new Cartographic(longitude, latitude, 0.0)));
        phone.setGain(100.0);
        phone.setNoiseFactor(2.0);
        phone.setTargetFrequency(m_intendedSignal.getTargetFrequency());

        // Add a default marker
        MarkerGraphics markerGraphics = new MarkerGraphics();
        markerGraphics.setTexture(new ConstantGraphicsParameter<>(m_phoneTexture));
        phone.getExtensions().add(new MarkerGraphicsExtension(markerGraphics));

        // Add a label based on the name and show just below the marker.
        TextGraphics textGraphics = new TextGraphics();
        textGraphics.setColor(new ConstantGraphicsParameter<>(Color.YELLOW));
        textGraphics.setFont(new ConstantGraphicsParameter<>(m_labelFont));
        textGraphics.setOutline(new ConstantGraphicsParameter<>(true));
        textGraphics.setOutlineColor(new ConstantGraphicsParameter<>(Color.BLACK));
        textGraphics.setText(new ConstantGraphicsParameter<>(phone.getName()));
        textGraphics.setOrigin(new ConstantGraphicsParameter<>(Origin.TOP_CENTER));
        textGraphics.setPixelOffset(new ConstantGraphicsParameter<>(new PointF(0, -m_phoneTexture.getTemplate().getHeight() / 2)));
        textGraphics.getDisplayParameters().setMaximumDistance(new ConstantGraphicsParameter<>(75000000.0));
        if (TextureFilter2D.supported(TextureWrap.CLAMP_TO_EDGE)) {
            textGraphics.setTextureFilter(new ConstantGraphicsParameter<>(TextureFilter2D.getNearestClampToEdge()));
        }
        phone.getExtensions().add(new TextGraphicsExtension(textGraphics));

        return phone;
    }

    /**
     * We allow the user to modify the transmission power via a track bar.
     * Whenever it is changed, assign the new power. This does not affect
     * anything until until we create new evaluators.
     */
    private void onTransmitPowerValueChanged() {
        m_transmittingPhone.setEffectiveIsotropicRadiatedPower(CommunicationAnalysis.fromDecibels(m_transmitPowerTrackBar.getValue()));
    }

    /**
     * "Places" the call by enabling the display of
     * the link budget results as well as showing a line
     * indicating if the link is currently valid. If links
     * signal to noise ratio gets to high, it becomes invalid
     * and the line disappears.
     */
    private void onPlaceCallClick() {
        m_callPlaced = true;
        m_placeCallButton.setEnabled(false);
        m_hangUpButton.setEnabled(true);
        m_transmitPowerTrackBar.setEnabled(false);
        m_linkBudgetOverlayHelper.getOverlay().setDisplay(true);

        // We want to draw link lines representing the link, but we don't want to draw the
        // lines unless the signal quality is acceptable. We already configured the
        // link itself to take the curvature of the earth into account to ensure line
        // of sight for our assets. The next step is to choice an acceptable signal
        // quality. Below we create an AccessQuery that is only valid when the
        // carrier to noise ratio of the final downlink is over -16 dB.
        // We then pass this query to updateLinkGraphics which will use it to
        // configure the Display property of the link graphics. Finally, we call
        // applyChanges to update the display.
        LinkBudgetScalars linkBudgetScalars = m_communicationSystem.getLinkBudgetScalars(m_iridium4ToReceiverDownLink, m_intendedSignal);
        AccessQuery query = new ScalarConstraint(linkBudgetScalars.getCarrierToNoise(), CommunicationAnalysis.fromDecibels(-16.0));
        updateLinkGraphics(m_callerToIridium49UpLink, query);
        updateLinkGraphics(m_iridium4ToReceiverDownLink, query);
        updateLinkGraphics(m_iridium49To58Crosslink, query);
        updateLinkGraphics(m_iridium58To4Crosslink, query);
        m_display.applyChanges();

        // Manual fire the combo box change event in order to
        // properly initialize the link budget overlay.
        onLinkComboBoxSelectedIndexChanged();

        // Start animation if it's not going.
        if (!SceneManager.getAnimation().getIsAnimating()) {
            SceneManager.getAnimation().playForward();
        }
    }

    /**
     * Given a link and a AccessQuery, assigns link graphics which will
     * only be displayed when the AccessQuery is satisfied.
     */
    private void updateLinkGraphics(ExtensibleObject link, AccessQuery query) {
        // First check if the link already has the graphics extension.
        LinkGraphicsExtension linkGraphicsExtension = link.getExtensions().getByType(new TypeLiteral<LinkGraphicsExtension>() {});
        if (linkGraphicsExtension == null) {
            // If it does not, add one.
            LineGraphics lineGraphics = new LineGraphics();
            lineGraphics.setColor(new ConstantGraphicsParameter<>(Color.YELLOW));
            linkGraphicsExtension = new LinkGraphicsExtension(lineGraphics);
            link.getExtensions().add(linkGraphicsExtension);
        }

        // Configure the link graphics to show the line only when access exists,
        // using a AccessQueryGraphicsParameter for the provided query.
        // In our case, this will be when carrier to noise for the downlink is greater than -16db.
        linkGraphicsExtension.getLinkGraphics().getDisplayParameters()
                .setDisplay(new AccessQueryGraphicsParameter<>(query, true, false, false));
    }

    /**
     * Turns off the link so the user can make changes.
     */
    private void onHangUpClick() {
        m_callPlaced = false;
        m_placeCallButton.setEnabled(true);
        m_hangUpButton.setEnabled(false);
        m_transmitPowerTrackBar.setEnabled(true);
        m_linkBudgetOverlayHelper.getOverlay().setDisplay(false);
        if (SceneManager.getAnimation().getIsAnimating()) {
            SceneManager.getAnimation().pause();
        }
    }

    /**
     * If the user changes the combo box to a different link, update
     * the overlay helper to use the new link.
     */
    private void onLinkComboBoxSelectedIndexChanged() {
        if (!m_callPlaced)
            return;

        // Get the selected link from the combo box.
        ExtensibleObject selectedLink = ((ComboBoxItem) m_linkComboBox.getSelectedItem()).getItem();

        // Get the link budget scalars from the communication system
        LinkBudgetScalars scalars = m_communicationSystem.getLinkBudgetScalars(selectedLink, m_intendedSignal);
        m_linkBudgetOverlayHelper.setScalars(scalars);

        // Get the name of the link from the INameService it implements
        m_linkBudgetOverlayHelper.setName(ServiceHelper.getService(new TypeLiteral<INameService>() {}, selectedLink).getName());

        // Apply the changes.
        m_linkBudgetOverlayHelper.applyChanges();
        m_linkBudgetOverlayHelper.update(SceneManager.getTime());
    }

    /**
     * Whenever the animation time changes, we need to update the display
     * and associated data output.
     */
    private void sceneManagerTimeChanged(Object sender, TimeChangedEventArgs e) {
        // Update graphics
        m_display.update(e.getTime());

        // Update the overlay
        if (m_callPlaced) {
            m_linkBudgetOverlayHelper.update(e.getTime());
        }
    }

    private class ComboBoxItem {
        public ComboBoxItem(ExtensibleObject item) {
            m_item = item;
        }

        @Override
        public String toString() {
            return ServiceHelper.getService(new TypeLiteral<INameService>() {}, m_item).getName();
        }

        public ExtensibleObject getItem() {
            return m_item;
        }

        private final ExtensibleObject m_item;
    }

    private static final long serialVersionUID = 1723165082099873815L;
    private final Font m_labelFont;
    private final IntendedSignalByFrequency m_intendedSignal;
    private final SimpleDigitalTransmitter m_transmittingPhone;
    private final SimpleReceiver m_receivingPhone;
    private final ExtensibleObject m_callerToIridium49UpLink;
    private final ExtensibleObject m_iridium4ToReceiverDownLink;
    private final ExtensibleObject m_iridium49To58Crosslink;
    private final ExtensibleObject m_iridium58To4Crosslink;
    private final CommunicationSystem m_communicationSystem;
    private final Texture2D m_phoneTexture;
    private final ServiceProviderDisplay m_display;
    private final LinkBudgetOverlayHelper m_linkBudgetOverlayHelper;
    private boolean m_callPlaced;
    private JButton m_hangUpButton;
    private JComboBox<ComboBoxItem> m_linkComboBox;
    private Insight3D m_insight3D;
    private JSlider m_transmitPowerTrackBar;
    private JButton m_placeCallButton;
}
