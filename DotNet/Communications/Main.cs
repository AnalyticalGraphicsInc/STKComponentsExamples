using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AGI.Examples;
using AGI.Foundation;
using AGI.Foundation.Access;
using AGI.Foundation.Access.Constraints;
using AGI.Foundation.Celestial;
using AGI.Foundation.Communications;
using AGI.Foundation.Communications.Antennas;
using AGI.Foundation.Communications.SignalProcessing;
using AGI.Foundation.Coordinates;
using AGI.Foundation.Geometry;
using AGI.Foundation.Graphics;
using AGI.Foundation.Graphics.Advanced;
using AGI.Foundation.Graphics.Renderer;
using AGI.Foundation.Infrastructure;
using AGI.Foundation.Platforms;
using AGI.Foundation.Time;

namespace Communications
{
    /// <summary>
    /// This example application simulates a call from a location in Dessie, Ethiopia to a satellite phone in Bangalore, India.
    /// This is a direct connection call with no ground based telecom switches. All frequencies, modulations, data rates and 
    /// power (EIRP) settings were retrieved from
    /// http://www.icao.int/anb/panels/acp/wg/m/iridium_swg/ird-03/IRD-SWG03-WP02-Draft%20Iridium%20AMS(R)S%20Tech%20Manual%20110105.pdf                                    
    /// </summary>
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();

            m_insight3D = new Insight3D();
            m_insight3D.Dock = DockStyle.Fill;
            m_insight3DPanel.Controls.Add(m_insight3D);

            // We don't have a call placed yet.
            m_hangUpButton.Enabled = false;
            m_callPlaced = false;

            // Load a texture which we will use for our phone locations.
            m_phoneTexture = SceneManager.Textures.FromUri(Path.Combine(Application.StartupPath, "Data/Markers/facility.png"));

            // Create a ServiceProviderDisplay to be used for visualization.
            m_display = new ServiceProviderDisplay();

            // Create the font to use for labeling. We are using the same font as the control, only smaller.
            m_labelFont = new Font(Font.FontFamily, 10, Font.Style, Font.Unit, Font.GdiCharSet, Font.GdiVerticalFont);

            // The call will be taking place at a frequency of 1620.5e6.
            m_intendedSignal = new IntendedSignalByFrequency(1620.5e6);

            // Create the transmitting phone that we will use for our call and add it to the display.
            m_transmittingPhone = CreateTransmittingPhone();
            m_display.ServiceProviders.Add(m_transmittingPhone);

            // Do the same for the receiving phone.
            m_receivingPhone = CreateReceivingPhone();
            m_display.ServiceProviders.Add(m_receivingPhone);

            // Create an instance of IridiumSatellite for each of the three satellites we will 
            // be using and add them to the display.

            // IridiumSatellite is a Platform-based class that adds default graphics and a 
            // Transceiver object for communication.

            var analysisDate = new JulianDate(new GregorianDate(2011, 8, 2, 18, 1, 0));

            var iridium49Tles = TwoLineElementSetHelper.GetTles("25108", analysisDate);
            var iridium49 = new IridiumSatellite("Iridium 49", iridium49Tles, m_labelFont, m_intendedSignal.TargetFrequency);
            m_display.ServiceProviders.Add(iridium49);

            var iridium58Tles = TwoLineElementSetHelper.GetTles("25274", analysisDate);
            var iridium58 = new IridiumSatellite("Iridium 58", iridium58Tles, m_labelFont, m_intendedSignal.TargetFrequency);
            m_display.ServiceProviders.Add(iridium58);

            var iridium4Tles = TwoLineElementSetHelper.GetTles("24796", analysisDate);
            var iridium4 = new IridiumSatellite("Iridium 4", iridium4Tles, m_labelFont, m_intendedSignal.TargetFrequency);
            m_display.ServiceProviders.Add(iridium4);

            // If the TLE epoch is too far from our expected analysis date, then we are 
            // offline and loading cached data. Adjust the analysis date to match.
            if (iridium49Tles[0].Epoch.DaysDifference(analysisDate) > 5)
            {
                analysisDate = iridium49Tles[0].Epoch;
            }

            // Iridium 49 will be the receiving end of our caller's "uplink".
            // Modify the receiving antenna to be isotropic.
            iridium49.Transceiver.InputAntennaGainPattern = new IsotropicGainPattern();

            // Iridium 4 will be the transmitting end of our call receiver's "downlink".
            // Modify the transmitting antenna to be isotropic.
            iridium4.Transceiver.OutputAntennaGainPattern = new IsotropicGainPattern();

            // Now that we've created all of our definition objects, we need to build
            // the links between them that make up our communication system.
            m_communicationSystem = new CommunicationSystem();

            // Add a link for each hop in the chain.
            // This could have been accomplished with a single call to AddChain.
            // However, since we plan on further configuring each of the individual links 
            // generated for us, we add them one at a time.

            var linkCollection = m_communicationSystem.Links;
            m_callerToIridium49UpLink = linkCollection.Add("Uplink to Iridium 49", m_transmittingPhone, iridium49.Transceiver);
            m_iridium49To58Crosslink = linkCollection.Add("Iridium 49 -> Iridium 58", iridium49.Transceiver, iridium58.Transceiver);
            m_iridium58To4Crosslink = linkCollection.Add("Iridium 58 -> Iridium 4", iridium58.Transceiver, iridium4.Transceiver);
            m_iridium4ToReceiverDownLink = linkCollection.Add("Downlink from Iridium 4", iridium4.Transceiver, m_receivingPhone);

            // Now that we have the links, we can add an AccessConstraintsExtension to them so 
            // that each link is only valid if they have line of sight to each other.
            var earth = CentralBodiesFacet.GetFromContext().Earth;

            var callerToIridium49Constraint = new CentralBodyObstructionConstraint(m_callerToIridium49UpLink, earth);
            m_callerToIridium49UpLink.Extensions.Add(new AccessConstraintsExtension(callerToIridium49Constraint));

            var iridium49To58Constraint = new CentralBodyObstructionConstraint(m_iridium49To58Crosslink, earth);
            m_iridium49To58Crosslink.Extensions.Add(new AccessConstraintsExtension(iridium49To58Constraint));

            var iridium58To4Constraint = new CentralBodyObstructionConstraint(m_iridium58To4Crosslink, earth);
            m_iridium58To4Crosslink.Extensions.Add(new AccessConstraintsExtension(iridium58To4Constraint));

            var iridium4ToReceiverConstraint = new CentralBodyObstructionConstraint(m_iridium4ToReceiverDownLink, earth);
            m_iridium4ToReceiverDownLink.Extensions.Add(new AccessConstraintsExtension(iridium4ToReceiverConstraint));

            m_linkComboBox.DisplayMember = "Name";
            m_linkComboBox.Items.Add(m_callerToIridium49UpLink);
            m_linkComboBox.Items.Add(m_iridium49To58Crosslink);
            m_linkComboBox.Items.Add(m_iridium58To4Crosslink);
            m_linkComboBox.Items.Add(m_iridium4ToReceiverDownLink);
            m_linkComboBox.SelectedItem = m_iridium4ToReceiverDownLink;

            // Even though we haven't added any link graphics yet, we will later, so add them 
            // to the display now.
            m_display.ServiceProviders.Add(m_callerToIridium49UpLink);
            m_display.ServiceProviders.Add(m_iridium49To58Crosslink);
            m_display.ServiceProviders.Add(m_iridium58To4Crosslink);
            m_display.ServiceProviders.Add(m_iridium4ToReceiverDownLink);

            // While we have set the location for our two phones and the satellite 
            // transceivers, what we haven't done is assign their orientations. This can be 
            // done automatically using the ConfigureAntennaTargeting method. Note that 
            // ConfigureAntennaTargeting is a best effort method and returns status information 
            // in regards to any problems it encounters. We don't check them here 
            // simply because we know it will succeed.
            m_communicationSystem.ConfigureAntennaTargeting();

            // Now that our initial configuration is complete, make sure we call ApplyChanges 
            // on the display so that the visualization is created.
            m_display.ApplyChanges();

            // Update the display to the current time.
            m_display.Update(SceneManager.Time);

            // Set up the animation time for our call.
            var animation = new SimulationAnimation
            {
                StartTime = analysisDate,
                EndTime = analysisDate.AddMinutes(15.0),
                TimeStep = Duration.FromSeconds(0.5),
                StartCycle = SimulationAnimationCycle.Loop,
                EndCycle = SimulationAnimationCycle.Loop
            };
            SceneManager.Animation = animation;

            // Subscribe to the time changed event so we can update our display.
            SceneManager.TimeChanged += SceneManagerTimeChanged;

            // Reset to the beginning.
            animation.Reset();

            // Configure the animation toolbar that overlays the 3D view.
            OverlayToolbar animationToolbar = new OverlayToolbar(m_insight3D);
            animationToolbar.Overlay.Origin = ScreenOverlayOrigin.BottomCenter;

            // Zoom to a location that includes both the caller and receiver.
            m_insight3D.Scene.Camera.ViewExtent(earth,
                                                Trig.DegreesToRadians(39.6333), Trig.DegreesToRadians(11.1333),
                                                Trig.DegreesToRadians(77.5833), Trig.DegreesToRadians(12.9833));

            // Move the camera further back so that all satellites are visible.
            m_insight3D.Scene.Camera.Distance += 5000000.0;

            // Turn off lighting since it's the middle of the night and we want to be able to see everything,
            m_insight3D.Scene.Lighting.Enabled = false;

            // Create our link budget overlay helper which will display
            // the complete link budget for a selected link on top of the display.
            m_linkBudgetOverlayHelper = new LinkBudgetOverlayHelper(m_labelFont);

            //Hide it until the call is initiated
            m_linkBudgetOverlayHelper.Overlay.Display = false;

            // Add the actual overlay to Insight3D
            SceneManager.ScreenOverlays.Add(m_linkBudgetOverlayHelper.Overlay);
        }

        /// <summary>
        /// Creates a new instance of our transmitting phone
        /// with default configuration and graphics.
        /// </summary>
        /// <returns>A new SimpleDigitalTransmitter.</returns>
        private SimpleDigitalTransmitter CreateTransmittingPhone()
        {
            var earth = CentralBodiesFacet.GetFromContext().Earth;

            // Create a new SimpleDigitalTransmitter and assign its basic properties.
            // Even though we are using a static location for our transmitter,
            // it can easily be changed to a moving one by simply modifying the 
            // LocationPoint to something else, for example a route generated
            // with the Route Design Library.
            double longitude = Trig.DegreesToRadians(39.6333);
            double latitude = Trig.DegreesToRadians(11.1333);
            var phone = new SimpleDigitalTransmitter
            {
                Name = "Dessie, Ethiopia",
                LocationPoint = new PointCartographic(earth, new Cartographic(longitude, latitude, 0.0)),
                CarrierFrequency = m_intendedSignal.TargetFrequency,
                EffectiveIsotropicRadiatedPower = CommunicationAnalysis.FromDecibels(m_transmitPowerTrackBar.Value),
                DataRate = 50000.0
            };

            //Add a default marker
            phone.Extensions.Add(new MarkerGraphicsExtension(new MarkerGraphics
            {
                Texture = new ConstantGraphicsParameter<Texture2D>(m_phoneTexture)
            }));

            //Add a label based on the name and show just below the marker.
            var textGraphics = new TextGraphics
            {
                Color = new ConstantGraphicsParameter<Color>(Color.Yellow),
                Font = new ConstantGraphicsParameter<Font>(m_labelFont),
                Outline = new ConstantGraphicsParameter<bool>(true),
                OutlineColor = new ConstantGraphicsParameter<Color>(Color.Black),
                Text = new ConstantGraphicsParameter<string>(phone.Name),
                Origin = new ConstantGraphicsParameter<Origin>(Origin.TopCenter),
                PixelOffset = new ConstantGraphicsParameter<PointF>(new PointF(0, -m_phoneTexture.Template.Height / 2)),
                DisplayParameters =
                {
                    MaximumDistance = new ConstantGraphicsParameter<double>(75000000.0)
                }
            };
            if (TextureFilter2D.Supported(TextureWrap.ClampToEdge))
            {
                textGraphics.TextureFilter = new ConstantGraphicsParameter<TextureFilter2D>(TextureFilter2D.NearestClampToEdge);
            }

            phone.Extensions.Add(new TextGraphicsExtension(textGraphics));

            return phone;
        }

        /// <summary>
        /// Creates a new instance of our receiving phone
        /// with default configuration and graphics.
        /// </summary>
        /// <returns>A new SimpleReceiver.</returns>
        private SimpleReceiver CreateReceivingPhone()
        {
            var earth = CentralBodiesFacet.GetFromContext().Earth;

            // Create a receiving phone with a gain of 100 and a noisefactor of 2 - adding 290 Kelvin worth of noise to the call.
            // Even though we are using a static location for our receiver,
            // it can easily be changed to a moving one by simply modifying the 
            // LocationPoint to something else, for example a route generated
            // with the Route Design Library.
            double longitude = Trig.DegreesToRadians(77.5833);
            double latitude = Trig.DegreesToRadians(12.9833);
            var phone = new SimpleReceiver
            {
                Name = "Bangalore, India",
                LocationPoint = new PointCartographic(earth, new Cartographic(longitude, latitude, 0)),
                Gain = 100.0,
                NoiseFactor = 2.0,
                TargetFrequency = m_intendedSignal.TargetFrequency
            };

            //Add a default marker
            phone.Extensions.Add(new MarkerGraphicsExtension(new MarkerGraphics
            {
                Texture = new ConstantGraphicsParameter<Texture2D>(m_phoneTexture)
            }));

            //Add a label based on the name and show just below the marker.
            var textGraphics = new TextGraphics
            {
                Color = new ConstantGraphicsParameter<Color>(Color.Yellow),
                Font = new ConstantGraphicsParameter<Font>(m_labelFont),
                Outline = new ConstantGraphicsParameter<bool>(true),
                OutlineColor = new ConstantGraphicsParameter<Color>(Color.Black),
                Text = new ConstantGraphicsParameter<string>(phone.Name),
                Origin = new ConstantGraphicsParameter<Origin>(Origin.TopCenter),
                PixelOffset = new ConstantGraphicsParameter<PointF>(new PointF(0, -m_phoneTexture.Template.Height / 2)),
                DisplayParameters =
                {
                    MaximumDistance = new ConstantGraphicsParameter<double>(75000000.0)
                }
            };
            if (TextureFilter2D.Supported(TextureWrap.ClampToEdge))
            {
                textGraphics.TextureFilter = new ConstantGraphicsParameter<TextureFilter2D>(TextureFilter2D.NearestClampToEdge);
            }

            phone.Extensions.Add(new TextGraphicsExtension(textGraphics));

            return phone;
        }

        /// <summary>
        /// We allow the user to modify the transmission power via a track bar. Whenever it is changed,
        /// assign the new power. This does not affect anything until until we create new evaluators.
        /// </summary>
        private void OnTransmitPowerValueChanged(object sender, EventArgs e)
        {
            m_transmittingPhone.EffectiveIsotropicRadiatedPower = CommunicationAnalysis.FromDecibels(m_transmitPowerTrackBar.Value);
        }

        /// <summary>
        /// "Places" the call by enabling the display of
        /// the link budget results as well as showing a line
        /// indicating if the link is currently valid. If links
        /// signal to noise ratio gets to high, it becomes invalid
        /// and the line disappears.
        /// </summary>
        private void OnPlaceCallClick(object sender, EventArgs e)
        {
            m_callPlaced = true;
            m_placeCallButton.Enabled = false;
            m_hangUpButton.Enabled = true;
            m_transmitPowerTrackBar.Enabled = false;
            m_linkBudgetOverlayHelper.Overlay.Display = true;

            // We want to draw link lines representing the link, but we don't want to draw the
            // lines unless the signal quality is acceptable. We already configured the
            // link itself to take the curvature of the earth into account to ensure line
            // of sight for our assets. The next step is to choice an acceptable signal
            // quality. Below we create an AccessQuery that is only valid when the
            // carrier to noise ratio of the final downlink is over -16 dB.
            // We then pass this query to UpdateLinkGraphics which will use it to 
            // configure the Display property of the link graphics. Finally, we call
            // ApplyChanges to update the display.
            var linkBudgetScalars = m_communicationSystem.GetLinkBudgetScalars(m_iridium4ToReceiverDownLink, m_intendedSignal);
            AccessQuery query = new ScalarConstraint(linkBudgetScalars.CarrierToNoise, CommunicationAnalysis.FromDecibels(-16.0));
            UpdateLinkGraphics(m_callerToIridium49UpLink, query);
            UpdateLinkGraphics(m_iridium4ToReceiverDownLink, query);
            UpdateLinkGraphics(m_iridium49To58Crosslink, query);
            UpdateLinkGraphics(m_iridium58To4Crosslink, query);
            m_display.ApplyChanges();

            // Manually fire the combo box change event in order to
            // properly initialize the link budget overlay.
            OnLinkComboBoxSelectedIndexChanged(this, EventArgs.Empty);

            // Start animation if it's not going.
            if (!SceneManager.Animation.IsAnimating)
            {
                SceneManager.Animation.PlayForward();
            }
        }

        /// <summary>
        /// Given a link and a AccessQuery, assigns link graphics which will
        /// only be displayed when the AccessQuery is satisfied.
        /// </summary>
        private static void UpdateLinkGraphics(ExtensibleObject link, AccessQuery query)
        {
            // First check if the link already has a link graphics extension.
            var linkGraphicsExtension = link.Extensions.GetByType<LinkGraphicsExtension>();
            if (linkGraphicsExtension == null)
            {
                // If it does not, add one.
                linkGraphicsExtension = new LinkGraphicsExtension(new LineGraphics
                {
                    Color = new ConstantGraphicsParameter<Color>(Color.Yellow)
                });
                link.Extensions.Add(linkGraphicsExtension);
            }

            // Configure the link graphics to show the line only when access exists,
            // using a AccessQueryGraphicsParameter for the provided query. 
            // In our case, this will be when carrier to noise for the downlink is greater than -16db.
            linkGraphicsExtension.LinkGraphics.DisplayParameters.Display = new AccessQueryGraphicsParameter<bool>(query, true, false, false);
        }

        /// <summary>
        /// Turns off the link so the user can make changes.
        /// </summary>
        private void OnHangUpClick(object sender, EventArgs e)
        {
            m_callPlaced = false;
            m_placeCallButton.Enabled = true;
            m_hangUpButton.Enabled = false;
            m_transmitPowerTrackBar.Enabled = true;
            m_linkBudgetOverlayHelper.Overlay.Display = false;
            if (SceneManager.Animation.IsAnimating)
            {
                SceneManager.Animation.Pause();
            }
        }

        /// <summary>
        /// If the user changes the combo box to a different link, update
        /// the overlay helper to use the new link.
        /// </summary>
        private void OnLinkComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!m_callPlaced)
                return;

            // Get the selected link from the combo box.
            var selectedLink = (ExtensibleObject)m_linkComboBox.SelectedItem;

            // Get the link budget scalars from the communication system 
            var linkBudgetScalars = m_communicationSystem.GetLinkBudgetScalars(selectedLink, m_intendedSignal);
            m_linkBudgetOverlayHelper.Scalars = linkBudgetScalars;

            // Get the name of the link from the INameService it implements
            m_linkBudgetOverlayHelper.Name = ServiceHelper.GetService<INameService>(selectedLink).Name;

            // Apply the changes.
            m_linkBudgetOverlayHelper.ApplyChanges();
            m_linkBudgetOverlayHelper.Update(SceneManager.Time);
        }

        /// <summary>
        /// Whenever the animation time changes, we need to update the display
        /// and associated data output.
        /// </summary>
        private void SceneManagerTimeChanged(object sender, TimeChangedEventArgs e)
        {
            // Update graphics
            m_display.Update(e.Time);

            // Update the overlay
            if (m_callPlaced)
            {
                m_linkBudgetOverlayHelper.Update(e.Time);
            }
        }

        private readonly Insight3D m_insight3D;
        private readonly Font m_labelFont;
        private readonly IntendedSignalByFrequency m_intendedSignal;
        private readonly SimpleDigitalTransmitter m_transmittingPhone;
        private readonly SimpleReceiver m_receivingPhone;
        private readonly ExtensibleObject m_callerToIridium49UpLink;
        private readonly ExtensibleObject m_iridium4ToReceiverDownLink;
        private readonly ExtensibleObject m_iridium49To58Crosslink;
        private readonly ExtensibleObject m_iridium58To4Crosslink;
        private readonly CommunicationSystem m_communicationSystem;
        private readonly Texture2D m_phoneTexture;
        private readonly ServiceProviderDisplay m_display;
        private readonly LinkBudgetOverlayHelper m_linkBudgetOverlayHelper;
        private bool m_callPlaced;
    }
}
