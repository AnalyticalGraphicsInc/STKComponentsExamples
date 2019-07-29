package com.agi;

import java.awt.Color;
import java.awt.Font;
import java.awt.FontMetrics;
import java.awt.Graphics;
import java.awt.image.BufferedImage;
import java.io.IOException;

import javax.imageio.ImageIO;
import javax.swing.JApplet;
import javax.swing.SwingUtilities;

import agi.foundation.Trig;
import agi.foundation.celestial.CentralBodiesFacet;
import agi.foundation.celestial.EarthCentralBody;
import agi.foundation.compatibility.EventHandler;
import agi.foundation.coordinates.CartographicExtent;
import agi.foundation.graphics.RealTimeSimulationAnimation;
import agi.foundation.graphics.SceneManager;
import agi.foundation.graphics.TextureScreenOverlay;
import agi.foundation.graphics.advanced.ScreenOverlayOrigin;
import agi.foundation.graphics.advanced.ScreenOverlaySize;
import agi.foundation.graphics.awt.Insight3D;
import agi.foundation.graphics.renderer.Texture2D;
import agi.foundation.graphics.renderer.TextureFilter2D;
import agi.foundation.platforms.ConstantGraphicsParameter;
import agi.foundation.platforms.MarkerGraphicsExtension;
import agi.foundation.platforms.Platform;
import agi.foundation.platforms.ServiceProviderDisplay;
import agi.foundation.platforms.TextGraphicsExtension;
import agi.foundation.propagators.Sgp4Propagator;
import agi.foundation.propagators.TwoLineElementSet;

public class InsightApplet extends JApplet {
    private static final long serialVersionUID = 1L;
    private static final Font labelFont = new Font("Arial", Font.PLAIN, 10);
    private static final Font overlayFont = new Font("Arial", Font.PLAIN, 12);
    private Insight3D insight;

    public void viewExtent(final String westDeg, final String southDeg, final String eastDeg, final String northDeg) {
        try {
            SwingUtilities.invokeAndWait(() -> {
                double west = Trig.degreesToRadians(Double.parseDouble(westDeg));
                double south = Trig.degreesToRadians(Double.parseDouble(southDeg));
                double east = Trig.degreesToRadians(Double.parseDouble(eastDeg));
                double north = Trig.degreesToRadians(Double.parseDouble(northDeg));

                CartographicExtent extent = new CartographicExtent(west, south, east, north);
                EarthCentralBody earth = CentralBodiesFacet.getFromContext().getEarth();
                insight.getScene().getCamera().viewExtent(earth, extent);

                insight.getScene().render();
                insight.repaint();
            });
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    @Override
    public void init() {
        try {
            SwingUtilities.invokeAndWait(() -> {
                try {
                    insight = new Insight3D();
                    add(insight);
                    createSatellite();
                } catch (Exception e) {
                    e.printStackTrace();
                }
            });
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    @Override
    public void destroy() {
        try {
            SwingUtilities.invokeAndWait(() -> {
                SceneManager.getAnimation().pause();
                remove(insight);
                insight.dispose();
            });
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    private void createSatellite() throws IOException {
        TwoLineElementSet issTle = new TwoLineElementSet("1 25544U 98067A   16278.37779601  .00004775  00000-0  79598-4 0  9999\r\n"
                + "2 25544  51.6441 233.3757 0006609  37.6498  69.1223 15.54033846 21972");

        RealTimeSimulationAnimation animation = new RealTimeSimulationAnimation();
        animation.setTime(issTle.getEpoch());
        animation.setTimeMultiplier(1000);

        SceneManager.setAnimation(animation);

        Platform iss = new Platform("ISS");
        Sgp4Propagator propagator = new Sgp4Propagator(issTle);
        iss.setLocationPoint(propagator.createPoint());

        TextGraphicsExtension issText = new TextGraphicsExtension();
        issText.getTextGraphics().setFont(new ConstantGraphicsParameter<>(labelFont));
        issText.getTextGraphics().setText(new ConstantGraphicsParameter<>(iss.getName()));
        issText.getTextGraphics().setColor(new ConstantGraphicsParameter<>(Color.RED));
        iss.getExtensions().add(issText);

        MarkerGraphicsExtension issMarker = new MarkerGraphicsExtension();

        BufferedImage satelliteImage = ImageIO.read(InsightApplet.class.getResourceAsStream("Satellite.png"));
        Texture2D satelliteTexture = SceneManager.getTextures().fromBitmap(satelliteImage);
        satelliteImage.flush();

        issMarker.getMarkerGraphics().setTexture(new ConstantGraphicsParameter<>(satelliteTexture));
        iss.getExtensions().add(issMarker);

        @SuppressWarnings("resource")
        final ServiceProviderDisplay display = new ServiceProviderDisplay();
        display.getServiceProviders().add(iss);
        display.applyChanges();

        final TextureScreenOverlay timeOverlay = new TextureScreenOverlay(10, 10, 0, 0);
        timeOverlay.setOrigin(ScreenOverlayOrigin.BOTTOM_LEFT);
        timeOverlay.setTextureFilter(TextureFilter2D.getNearestClampToEdge());

        updateTimeOverlay(timeOverlay);

        SceneManager.getScreenOverlays().add(timeOverlay);

        SceneManager.addTimeChanged(EventHandler.of((sender, e) -> {
            display.update(e.getTime());
            updateTimeOverlay(timeOverlay);
        }));

        SceneManager.getAnimation().playForward();
    }

    private void updateTimeOverlay(TextureScreenOverlay timeOverlay) {
        String time = SceneManager.getTime().toGregorianDate().toString("MM/dd/yyyy HH:mm:ss");

        int textWidth;
        int textHeight;

        Graphics g = insight.getGraphics();
        g.setFont(overlayFont);

        FontMetrics fontMetrics = g.getFontMetrics();

        textWidth = fontMetrics.stringWidth(time);
        textHeight = fontMetrics.getHeight();

        g.dispose();

        timeOverlay.setSize(new ScreenOverlaySize(textWidth, textHeight));

        BufferedImage textBitmap = new BufferedImage(textWidth, textHeight, BufferedImage.TYPE_INT_ARGB);

        g = textBitmap.createGraphics();
        g.setColor(Color.WHITE);
        g.setFont(overlayFont);
        g.drawString(time, 0, textHeight);
        g.dispose();

        Texture2D oldTexture = timeOverlay.getTexture();

        timeOverlay.setTexture(SceneManager.getTextures().fromBitmap(textBitmap));

        textBitmap.flush();

        if (oldTexture != null)
            oldTexture.dispose();
    }
}
