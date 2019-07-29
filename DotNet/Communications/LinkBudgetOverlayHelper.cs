using System;
using System.Drawing;
using AGI.Examples;
using AGI.Foundation;
using AGI.Foundation.Communications;
using AGI.Foundation.Geometry;
using AGI.Foundation.Graphics;
using AGI.Foundation.Graphics.Advanced;
using AGI.Foundation.Graphics.Renderer;
using AGI.Foundation.Platforms;
using AGI.Foundation.Time;

namespace Communications
{
    /// <summary>
    /// A simple helper class that creates and updates
    /// a ScreenOverlay with the formatted text of
    /// a generated link budget.
    /// </summary>
    internal class LinkBudgetOverlayHelper : IDisposable
    {
        /// <summary>
        /// Creates a new instance. Font, Name and LinkScalars properties must be specified.
        /// </summary>
        public LinkBudgetOverlayHelper(Font font)
        {
            m_font = font;

            // GraphicsParameterFormatter is part of Platform.Graphics and
            // optimizes dynamic text generation by only recalculating
            // values as they are needed. For example, if a field is not,
            // time varying, it will never evaluate it more than once.
            m_formatter = new GraphicsParameterFormatter
            {
                // Specify a format string that provides the output we want.
                FormatString = @"Name: {0}
Effective Isotropic Radiated Power: {1:0.000} dBW
Received Isotropic Power: {2:0.000} dBW
Power At Receiver Output: {3:0.000} dBW
Received Power Flux Density: {4:0.000} dBW/m^2
Propagation Loss: {5:0.000} dB
Carrier To Noise Density: {6:0.000} dB*Hz
Carrier To Noise: {7:0.000} dB
Energy Per Bit To Noise Density: {8:0.000} dB
Bit Error Rate: {9:0.###E+000}"
            };

            // Create a screen overlay with a translucent background to make the
            // text easier to read.
            m_overlay = new ScreenOverlay(0.0, 0.0, 1.0, 1.0)
            {
                Color = Color.Black,
                Translucency = 0.5f,
                Origin = ScreenOverlayOrigin.TopLeft
            };

            // Create a child overlay for our background to write text on.
            m_textOverlay = new TextureScreenOverlay(0.0, 0.0, 1.0, 1.0)
            {
                Origin = ScreenOverlayOrigin.Center,
                Color = Color.Yellow
            };
            m_overlay.Overlays.Add(m_textOverlay);

            if (TextureFilter2D.Supported(TextureWrap.ClampToEdge))
            {
                m_textOverlay.TextureFilter = TextureFilter2D.NearestClampToEdge;
            }
        }

        public void Dispose()
        {
            SceneManager.ScreenOverlays.Remove(m_overlay);
            m_overlay.Dispose();
        }

        /// <summary>
        /// Gets or sets the name to display on the overlay.
        /// </summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>
        /// Gets or sets the scalars that define the link budget.
        /// </summary>
        public LinkBudgetScalars Scalars
        {
            get { return m_scalars; }
            set { m_scalars = value; }
        }

        /// <summary>
        /// Gets the font to use.
        /// </summary>
        public Font Font
        {
            get { return m_font; }
        }

        /// <summary>
        /// Gets the resulting overlay.
        /// </summary>
        public ScreenOverlay Overlay
        {
            get { return m_overlay; }
        }

        /// <summary>
        /// Must be called to apply any changes to the definitional properties.
        /// </summary>
        public void ApplyChanges()
        {
            // Order is important here as it will reflect the order in the FormatString property.
            var parameters = m_formatter.Parameters;
            parameters.Clear();
            parameters.Add(new ConstantGraphicsParameter<string>(Name));
            parameters.Add(GetScalarInDecibels(Scalars.EffectiveIsotropicRadiatedPower));
            parameters.Add(GetScalarInDecibels(Scalars.ReceivedIsotropicPower));
            parameters.Add(GetScalarInDecibels(Scalars.PowerAtReceiverOutput));
            parameters.Add(GetScalarInDecibels(Scalars.ReceivedPowerFluxDensity));
            parameters.Add(GetScalarInDecibels(Scalars.PropagationLoss));
            parameters.Add(GetScalarInDecibels(Scalars.CarrierToNoiseDensity));
            parameters.Add(GetScalarInDecibels(Scalars.CarrierToNoise));
            parameters.Add(GetScalarInDecibels(Scalars.EnergyPerBitToNoiseDensity));
            parameters.Add(new ScalarGraphicsParameter(Scalars.BitErrorRate));

            // These are available, but we are not using them.
            //parameters.Add(new ScalarGraphicsParameter(Scalars.CarrierToInterference));
            //parameters.Add(GetScalarInDecibels(Scalars.CarrierToNoisePlusInterference));
            //parameters.Add(new ScalarGraphicsParameter(Scalars.ReceiverAntennaGainInLinkDirection));
            //parameters.Add(new ScalarGraphicsParameter(Scalars.TransmitterAntennaGainInLinkDirection));

            m_evaluator = m_formatter.GetEvaluator(new EvaluatorGroup());
        }

        /// <summary>
        /// Updates the overlay to the provided time.
        /// </summary>
        public void Update(JulianDate time)
        {
            if (m_textOverlay.Texture != null)
            {
                m_textOverlay.Texture.Dispose();
            }

            string text = string.Concat("Time: ",
                                        time.ToTimeStandard(TimeStandard.CoordinatedUniversalTime).ToGregorianDate().ToString("MMM d, yyyy hh:mm:ss"),
                                        " UTCG\n",
                                        m_evaluator.Evaluate(time));

            // measure the actual text and resize the overlays to match
            Size textSize = Insight3DHelper.MeasureString(text, m_font);
            m_textOverlay.Size = new ScreenOverlaySize(textSize.Width, textSize.Height);
            m_overlay.Size = m_textOverlay.Size;

            // Draw the text to a bitmap, create a texture and use that texture as the overlay.
            using (var textBitmap = new Bitmap(textSize.Width, textSize.Height))
            using (var graphics = Graphics.FromImage(textBitmap))
            {
                graphics.DrawString(text, m_font, Brushes.White, new PointF(0, 0));
                m_textOverlay.Texture = SceneManager.Textures.FromBitmap(textBitmap);
            }
        }

        /// <summary>
        /// Creates a GraphicsParameter that takes a scalar and turns it into its value in decibels.
        /// </summary>
        private static GraphicsParameterTransform<double, double> GetScalarInDecibels(Scalar scalar)
        {
            return new GraphicsParameterTransform<double, double>(new ScalarGraphicsParameter(scalar), CommunicationAnalysis.ToDecibels);
        }

        private readonly Font m_font;
        private readonly ScreenOverlay m_overlay;
        private readonly TextureScreenOverlay m_textOverlay;
        private readonly GraphicsParameterFormatter m_formatter;
        private string m_name;
        private LinkBudgetScalars m_scalars;
        private Evaluator<string> m_evaluator;
    }
}