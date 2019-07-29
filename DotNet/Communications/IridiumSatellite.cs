using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AGI.Foundation.Celestial;
using AGI.Foundation.Communications;
using AGI.Foundation.Communications.Antennas;
using AGI.Foundation.Communications.SignalProcessing;
using AGI.Foundation.Coordinates;
using AGI.Foundation.Geometry;
using AGI.Foundation.Graphics;
using AGI.Foundation.Graphics.Advanced;
using AGI.Foundation.Graphics.Renderer;
using AGI.Foundation.Platforms;
using AGI.Foundation.Propagators;

namespace Communications
{
    /// <summary>
    /// A simple Platform based class which models an Iridium satellite.
    /// It also adds a Transceiver property as well as default extensions for graphics.
    /// </summary>
    public class IridiumSatellite : Platform
    {
        /// <summary>
        /// Creates a new instance with the provided properties.
        /// </summary>
        public IridiumSatellite(string name, IEnumerable<TwoLineElementSet> tles, Font font, double targetFrequency)
        {
            var earth = CentralBodiesFacet.GetFromContext().Earth;

            //Set the name and create the point from the TLE.
            Name = name;
            LocationPoint = new Sgp4Propagator(tles).CreatePoint();
            OrientationAxes = new AxesVehicleVelocityLocalHorizontal(earth.InertialFrame, LocationPoint);

            //Create a transceiver modeled after Iridium specs.
            m_transceiver = new Transceiver
            {
                Name = Name + " Transceiver",
                Modulation = new ModulationQpsk(),
                OutputGain = 100.0,
                OutputNoiseFactor = 1.2,
                CarrierFrequency = targetFrequency,
                Filter = new RectangularFilter
                {
                    Frequency = targetFrequency,
                    UpperBandwidthLimit = 20.0e6,
                    LowerBandwidthLimit = -20.0e6
                },
                InputAntennaGainPattern = new GaussianGainPattern(1.0, 0.55, 0.001),
                OutputAntennaGainPattern = new GaussianGainPattern(1.0, 0.55, 0.001)
            };
            m_transceiver.InputAntenna.LocationPoint = new PointFixedOffset(ReferenceFrame, new Cartesian(0, -1.0, 0));
            m_transceiver.OutputAntenna.LocationPoint = new PointFixedOffset(ReferenceFrame, new Cartesian(0, +1.0, 0));

            // setup Marker graphics for the satellites
            var satelliteTexture = SceneManager.Textures.FromUri(Path.Combine(Application.StartupPath, "Data/Markers/smallsatellite.png"));

            m_markerGraphicsExtension = new MarkerGraphicsExtension(new MarkerGraphics
            {
                Texture = new ConstantGraphicsParameter<Texture2D>(satelliteTexture),
                DisplayParameters = new DisplayParameters
                {
                    // hide marker when camera is closer than the below distance
                    MinimumDistance = new ConstantGraphicsParameter<double>(17500000.0)
                }
            });
            Extensions.Add(m_markerGraphicsExtension);

            // setup Model graphics for the satellites
            var modelUri = new Uri(Path.Combine(Application.StartupPath, "Data/Models/iridium.mdl"));

            m_modelGraphicsExtension = new ModelGraphicsExtension(new ModelGraphics
            {
                Uri = new ConstantGraphicsParameter<Uri>(modelUri),
                Scale = new ConstantGraphicsParameter<double>(50000.0),
                DisplayParameters = new DisplayParameters
                {
                    // hide model when camera is further than the marker minimum distance above
                    MaximumDistance = m_markerGraphicsExtension.MarkerGraphics.DisplayParameters.MinimumDistance
                }
            });
            Extensions.Add(m_modelGraphicsExtension);

            // setup text graphics for the satellites
            m_textGraphicsExtension = new TextGraphicsExtension(new TextGraphics
            {
                Color = new ConstantGraphicsParameter<Color>(Color.Red),
                Font = new ConstantGraphicsParameter<Font>(font),
                Outline = new ConstantGraphicsParameter<bool>(true),
                OutlineColor = new ConstantGraphicsParameter<Color>(Color.Black),
                Text = new ConstantGraphicsParameter<string>(Name),
                Origin = new ConstantGraphicsParameter<Origin>(Origin.TopCenter),
                PixelOffset = new ConstantGraphicsParameter<PointF>(new PointF(0, -satelliteTexture.Template.Height / 2))
            });
            if (TextureFilter2D.Supported(TextureWrap.ClampToEdge))
            {
                m_textGraphicsExtension.TextGraphics.TextureFilter = new ConstantGraphicsParameter<TextureFilter2D>(TextureFilter2D.NearestClampToEdge);
            }

            Extensions.Add(m_textGraphicsExtension);
        }

        /// <summary>
        /// Gets the Transceiver on this satellite.
        /// </summary>
        public Transceiver Transceiver
        {
            get { return m_transceiver; }
        }

        /// <summary>
        /// Gets the marker graphics.
        /// </summary>
        public MarkerGraphics MarkerGraphics
        {
            get { return m_markerGraphicsExtension.MarkerGraphics; }
        }

        /// <summary>
        /// Gets the model graphics.
        /// </summary>
        public ModelGraphics ModelGraphics
        {
            get { return m_modelGraphicsExtension.ModelGraphics; }
        }

        /// <summary>
        /// Gets the text graphics.
        /// </summary>
        public TextGraphics TextGraphics
        {
            get { return m_textGraphicsExtension.TextGraphics; }
        }

        private readonly TextGraphicsExtension m_textGraphicsExtension;
        private readonly MarkerGraphicsExtension m_markerGraphicsExtension;
        private readonly ModelGraphicsExtension m_modelGraphicsExtension;
        private readonly Transceiver m_transceiver;
    }
}
