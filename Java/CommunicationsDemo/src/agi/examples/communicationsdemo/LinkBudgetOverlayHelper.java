package agi.examples.communicationsdemo;

import java.awt.Color;
import java.awt.Dimension;
import java.awt.Font;
import java.awt.Graphics2D;
import java.awt.image.BufferedImage;
import java.util.List;

import agi.examples.Insight3DHelper;
import agi.foundation.Evaluator;
import agi.foundation.EvaluatorGroup;
import agi.foundation.communications.CommunicationAnalysis;
import agi.foundation.communications.LinkBudgetScalars;
import agi.foundation.compatibility.Converter;
import agi.foundation.compatibility.IDisposable;
import agi.foundation.geometry.Scalar;
import agi.foundation.graphics.SceneManager;
import agi.foundation.graphics.ScreenOverlay;
import agi.foundation.graphics.TextureScreenOverlay;
import agi.foundation.graphics.advanced.ScreenOverlayOrigin;
import agi.foundation.graphics.advanced.ScreenOverlaySize;
import agi.foundation.graphics.renderer.TextureFilter2D;
import agi.foundation.graphics.renderer.TextureWrap;
import agi.foundation.platforms.ConstantGraphicsParameter;
import agi.foundation.platforms.GraphicsParameterFormatter;
import agi.foundation.platforms.GraphicsParameterTransform;
import agi.foundation.platforms.ScalarGraphicsParameter;
import agi.foundation.platforms.advanced.IGraphicsParameter;
import agi.foundation.time.JulianDate;
import agi.foundation.time.TimeStandard;

/**
 * A simple helper class that creates and updates
 * a ScreenOverlay with the formatted text of
 * a generated link budget.
 */
public class LinkBudgetOverlayHelper implements IDisposable {
    /**
     * Creates a new instance. Font, Name and LinkScalars properties must be specified.
     */
    public LinkBudgetOverlayHelper(Font font) {
        m_font = font;

        // GraphicsParameterFormatter is part of Platform.Graphics and optimizes dynamic
        // text generation by only recalculating values as they are needed. For example,
        // if a field is not, time varying, it will never evaluate it more than once.
        m_formatter = new GraphicsParameterFormatter();

        // Specify a format string that provides the output we want.
        m_formatter.setFormatString(
                "Name: {0}\nEffective Isotropic Radiated Power: {1:0.000} dBW\nReceived Isotropic Power: {2:0.000} dBW\nPower At Receiver Output: {3:0.000} dBW\nReceived Power Flux Density: {4:0.000} dBW/m^2\nPropagation Loss: {5:0.000} dB\nCarrier To Noise Density: {6:0.000} dB*Hz\nCarrier To Noise: {7:0.000} dB\nEnergy Per Bit To Noise Density: {8:0.000} dB\nBit Error Rate: {9:0.###E+000}");

        // Create a screen overlay with a translucent background to make the text easier
        // to read.
        m_overlay = new ScreenOverlay(0.0, 0.0, 1.0, 1.0);
        m_overlay.setColor(Color.BLACK);
        m_overlay.setTranslucency(0.5f);
        m_overlay.setOrigin(ScreenOverlayOrigin.TOP_LEFT);

        // Create a child overlay for our background to write text on.
        m_textOverlay = new TextureScreenOverlay(0.0, 0.0, 1.0, 1.0);
        m_textOverlay.setOrigin(ScreenOverlayOrigin.CENTER);
        m_textOverlay.setColor(Color.YELLOW);
        m_overlay.getOverlays().add(m_textOverlay);

        if (TextureFilter2D.supported(TextureWrap.CLAMP_TO_EDGE)) {
            m_textOverlay.setTextureFilter(TextureFilter2D.getNearestClampToEdge());
        }
    }

    @Override
    public void dispose() {
        SceneManager.getScreenOverlays().remove(m_overlay);
        m_overlay.dispose();
    }

    /**
     * Gets the name to display
     */
    public String getName() {
        return m_name;
    }

    /**
     * Sets the name to display
     */
    public void setName(final String name) {
        m_name = name;
    }

    /**
     * Gets the scalars that define the link budget.
     */
    public LinkBudgetScalars getScalars() {
        return m_scalars;
    }

    /**
     * Sets the scalars that define the link budget.
     */
    public void setScalars(final LinkBudgetScalars scalars) {
        m_scalars = scalars;
    }

    /**
     * Gets the font to use.
     */
    public Font getFont() {
        return m_font;
    }

    /**
     * Gets the created overlay.
     */
    public ScreenOverlay getOverlay() {
        return m_overlay;
    }

    /**
     * Must be called to apply any changes to the definitional properties.
     */
    public void applyChanges() {
        // Order is important here as it will reflect the order in the FormatString
        // property.
        List<IGraphicsParameter> parameters = m_formatter.getParameters();
        parameters.clear();
        parameters.add(new ConstantGraphicsParameter<>(m_name));
        parameters.add(getScalarInDecibels(m_scalars.getEffectiveIsotropicRadiatedPower()));
        parameters.add(getScalarInDecibels(m_scalars.getReceivedIsotropicPower()));
        parameters.add(getScalarInDecibels(m_scalars.getPowerAtReceiverOutput()));
        parameters.add(getScalarInDecibels(m_scalars.getReceivedPowerFluxDensity()));
        parameters.add(getScalarInDecibels(m_scalars.getPropagationLoss()));
        parameters.add(getScalarInDecibels(m_scalars.getCarrierToNoiseDensity()));
        parameters.add(getScalarInDecibels(m_scalars.getCarrierToNoise()));
        parameters.add(getScalarInDecibels(m_scalars.getEnergyPerBitToNoiseDensity()));
        parameters.add(new ScalarGraphicsParameter(m_scalars.getBitErrorRate()));

        // These are available, but we are not using them.
        // parameters.add(new ScalarGraphicsParameter(m_scalars.getCarrierToInterference()));
        // parameters.add(getScalarInDecibels(m_scalars.getCarrierToNoisePlusInterference()));
        // parameters.add(new ScalarGraphicsParameter(m_scalars.getReceiverAntennaGainInLinkDirection()));
        // parameters.add(new ScalarGraphicsParameter(m_scalars.getTransmitterAntennaGainInLinkDirection()));

        m_evaluator = m_formatter.getEvaluator(new EvaluatorGroup());
    }

    /**
     * Updates the overlay to the provided time.
     */
    public void update(JulianDate time) {
        if (m_textOverlay.getTexture() != null) {
            m_textOverlay.getTexture().dispose();
        }

        JulianDate utcTime = time.toTimeStandard(TimeStandard.getCoordinatedUniversalTime());
        String text = String.format("Time: %tb %<td, %<tY %<tT UTCG\r\n%s", utcTime.toDateTime(), m_evaluator.evaluate(time));

        Dimension textSize = Insight3DHelper.measureString(text, m_font);
        m_textOverlay.setSize(new ScreenOverlaySize(textSize.getWidth(), textSize.getHeight()));
        m_overlay.setSize(m_textOverlay.getSize());

        BufferedImage textBitmap = new BufferedImage(textSize.width, textSize.height, BufferedImage.TYPE_INT_ARGB);
        Graphics2D graphics = textBitmap.createGraphics();
        graphics.setColor(Color.WHITE);
        graphics.setFont(m_font);
        String[] splitText = text.split("\n");
        int lineHeight = graphics.getFontMetrics().getAscent() + graphics.getFontMetrics().getDescent();
        int maxAdvance = graphics.getFontMetrics().getMaxAdvance();
        int currentLineY = lineHeight;
        for (String textLine : splitText) {
            graphics.drawString(textLine, maxAdvance, currentLineY);
            currentLineY += lineHeight;
        }
        m_textOverlay.setTexture(SceneManager.getTextures().fromBitmap(textBitmap));
    }

    /**
     * Creates a GraphicsParameter that takes a scalar and turns it into its value in decibels.
     */
    private GraphicsParameterTransform<Double, Double> getScalarInDecibels(final Scalar scalar) {
        return new GraphicsParameterTransform<>(new ScalarGraphicsParameter(scalar),
                Converter.of(CommunicationAnalysis::toDecibels));
    }

    private final Font m_font;
    private final ScreenOverlay m_overlay;
    private final TextureScreenOverlay m_textOverlay;
    private final GraphicsParameterFormatter m_formatter;
    private String m_name;
    private LinkBudgetScalars m_scalars;
    private Evaluator<String> m_evaluator;
}
