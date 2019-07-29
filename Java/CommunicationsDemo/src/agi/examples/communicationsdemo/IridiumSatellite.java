package agi.examples.communicationsdemo;

import java.awt.Color;
import java.awt.Font;
import java.io.File;
import java.net.URI;

import agi.foundation.celestial.CentralBodiesFacet;
import agi.foundation.celestial.EarthCentralBody;
import agi.foundation.communications.Transceiver;
import agi.foundation.communications.antennas.GaussianGainPattern;
import agi.foundation.communications.signalprocessing.ModulationQpsk;
import agi.foundation.communications.signalprocessing.RectangularFilter;
import agi.foundation.compatibility.PointF;
import agi.foundation.coordinates.Cartesian;
import agi.foundation.geometry.AxesVehicleVelocityLocalHorizontal;
import agi.foundation.geometry.PointFixedOffset;
import agi.foundation.graphics.SceneManager;
import agi.foundation.graphics.advanced.Origin;
import agi.foundation.graphics.renderer.Texture2D;
import agi.foundation.graphics.renderer.TextureFilter2D;
import agi.foundation.graphics.renderer.TextureWrap;
import agi.foundation.platforms.ConstantGraphicsParameter;
import agi.foundation.platforms.MarkerGraphics;
import agi.foundation.platforms.MarkerGraphicsExtension;
import agi.foundation.platforms.ModelGraphics;
import agi.foundation.platforms.ModelGraphicsExtension;
import agi.foundation.platforms.Platform;
import agi.foundation.platforms.TextGraphics;
import agi.foundation.platforms.TextGraphicsExtension;
import agi.foundation.propagators.Sgp4Propagator;
import agi.foundation.propagators.TwoLineElementSet;

/**
 * A simple Platform based class which models an Iridium satellite.
 * It also adds a Transceiver property as well as default extensions for graphics.
 */
public class IridiumSatellite extends Platform {
    /**
     * Creates a new instance with the provided properties.
     */
    public IridiumSatellite(String name, Iterable<TwoLineElementSet> tles, Font font, double targetFrequency) {
        EarthCentralBody earth = CentralBodiesFacet.getFromContext().getEarth();

        // Set the name and create the point from the TLE.
        setName(name);
        setLocationPoint(new Sgp4Propagator(tles).createPoint());
        setOrientationAxes(new AxesVehicleVelocityLocalHorizontal(earth.getInertialFrame(), getLocationPoint()));

        // Create a transceiver modeled after Iridium specs.
        m_transceiver = new Transceiver();
        m_transceiver.setName(getName() + " Transceiver");
        m_transceiver.setModulation(new ModulationQpsk());
        m_transceiver.setOutputGain(100.0);
        m_transceiver.setOutputNoiseFactor(1.2);
        m_transceiver.setCarrierFrequency(targetFrequency);
        RectangularFilter filter = new RectangularFilter();
        filter.setFrequency(targetFrequency);
        filter.setUpperBandwidthLimit(20.0e6);
        filter.setLowerBandwidthLimit(-20.0e6);
        m_transceiver.setFilter(filter);
        m_transceiver.setInputAntennaGainPattern(new GaussianGainPattern(1.0, 0.55, 0.001));
        m_transceiver.setOutputAntennaGainPattern(new GaussianGainPattern(1.0, 0.55, 0.001));
        m_transceiver.getInputAntenna().setLocationPoint(new PointFixedOffset(getReferenceFrame(), new Cartesian(0, -1.0, 0)));
        m_transceiver.getOutputAntenna().setLocationPoint(new PointFixedOffset(getReferenceFrame(), new Cartesian(0, +1.0, 0)));

        // setup Marker graphics for the satellites
        Texture2D satelliteTexture = SceneManager.getTextures().fromUri("Data/Markers/smallsatellite.png");

        MarkerGraphics markerGraphics = new MarkerGraphics();
        markerGraphics.setTexture(new ConstantGraphicsParameter<>(satelliteTexture));
        // hide marker when camera is closer than the below distance
        markerGraphics.getDisplayParameters().setMinimumDistance(new ConstantGraphicsParameter<>(17500000.0));

        m_markerGraphicsExtension = new MarkerGraphicsExtension(markerGraphics);
        getExtensions().add(m_markerGraphicsExtension);

        // setup Model graphics for the satellites
        URI modelUri = new File("Data/Models/iridium.mdl").toURI();

        ModelGraphics modelGraphics = new ModelGraphics();
        modelGraphics.setUri(new ConstantGraphicsParameter<>(modelUri));
        modelGraphics.setScale(new ConstantGraphicsParameter<>(50000.0));
        // hide model when camera is further than the marker minimum distance above
        modelGraphics.getDisplayParameters().setMaximumDistance(markerGraphics.getDisplayParameters().getMinimumDistance());

        m_modelGraphicsExtension = new ModelGraphicsExtension(modelGraphics);
        getExtensions().add(m_modelGraphicsExtension);

        // setup text graphics for the satellites
        TextGraphics textGraphics = new TextGraphics();
        textGraphics.setColor(new ConstantGraphicsParameter<>(Color.RED));
        textGraphics.setFont(new ConstantGraphicsParameter<>(font));
        textGraphics.setOutline(new ConstantGraphicsParameter<>(true));
        textGraphics.setOutlineColor(new ConstantGraphicsParameter<>(Color.BLACK));
        textGraphics.setText(new ConstantGraphicsParameter<>(getName()));
        textGraphics.setOrigin(new ConstantGraphicsParameter<>(Origin.TOP_CENTER));
        textGraphics.setPixelOffset(new ConstantGraphicsParameter<>(new PointF(0, -satelliteTexture.getTemplate().getHeight() / 2)));
        if (TextureFilter2D.supported(TextureWrap.CLAMP_TO_EDGE)) {
            textGraphics.setTextureFilter(new ConstantGraphicsParameter<>(TextureFilter2D.getNearestClampToEdge()));
        }

        m_textGraphicsExtension = new TextGraphicsExtension(textGraphics);
        getExtensions().add(m_textGraphicsExtension);
    }

    // Gets the Transceiver on this satellite.
    public Transceiver getTransceiver() {
        return m_transceiver;
    }

    // Gets the marker graphics.
    public MarkerGraphics getMarkerGraphics() {
        return m_markerGraphicsExtension.getMarkerGraphics();
    }

    // Gets the model graphics.
    public ModelGraphics getModelGraphics() {
        return m_modelGraphicsExtension.getModelGraphics();
    }

    // Gets the text graphics.
    public TextGraphics getTextGraphics() {
        return m_textGraphicsExtension.getTextGraphics();
    }

    private final TextGraphicsExtension m_textGraphicsExtension;
    private final MarkerGraphicsExtension m_markerGraphicsExtension;
    private final ModelGraphicsExtension m_modelGraphicsExtension;
    private final Transceiver m_transceiver;
}
