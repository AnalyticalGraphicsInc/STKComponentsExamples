package agi.examples;

import java.awt.Color;
import java.awt.Point;
import java.awt.event.MouseEvent;
import java.awt.event.MouseListener;
import java.awt.event.MouseMotionListener;
import java.util.ArrayList;
import java.util.List;

import agi.foundation.Constants;
import agi.foundation.celestial.CentralBodiesFacet;
import agi.foundation.celestial.EarthCentralBody;
import agi.foundation.celestial.MoonCentralBody;
import agi.foundation.compatibility.Action;
import agi.foundation.compatibility.EventArgs;
import agi.foundation.compatibility.EventHandler;
import agi.foundation.compatibility.IDisposable;
import agi.foundation.coordinates.BoundingRectangle;
import agi.foundation.graphics.SceneManager;
import agi.foundation.graphics.ScreenOverlay;
import agi.foundation.graphics.ScreenOverlayPickResult;
import agi.foundation.graphics.SimulationAnimation;
import agi.foundation.graphics.TextureScreenOverlay;
import agi.foundation.graphics.advanced.ForwardAnimation;
import agi.foundation.graphics.advanced.ScreenOverlayOrigin;
import agi.foundation.graphics.advanced.ScreenOverlaySize;
import agi.foundation.graphics.advanced.ScreenOverlayUnit;
import agi.foundation.graphics.advanced.StartedEventArgs;
import agi.foundation.graphics.awt.Insight3D;

/**
 * Renders a toolbar of buttons as a screen overlay in Insight3D.
 */
public final class OverlayToolbar implements IDisposable {
    /**
     * Initializes a new instance.
     */
    public OverlayToolbar(Insight3D insight3D) {
        m_insight3D = insight3D;
        m_buttonHolders = new ArrayList<>();

        m_insight3D.getMouseOptions().addZoomComplete(EventHandler.of(this::onZoomComplete));

        m_insight3D.addMouseListener(new MouseListener() {
            @Override
            public void mouseReleased(MouseEvent e) {
                onMouseUp(null, e);
            }

            @Override
            public void mousePressed(MouseEvent e) {
                onMouseDown(null, e);
            }

            @Override
            public void mouseClicked(MouseEvent e) {
                onMouseClick(null, e);
                if (e.getClickCount() == 2) {
                    onMouseDoubleClick(null, e);
                }
            }

            @Override
            public void mouseEntered(MouseEvent e) {}

            @Override
            public void mouseExited(MouseEvent e) {}
        });

        m_insight3D.addMouseMotionListener(new MouseMotionListener() {
            @Override
            public void mouseMoved(MouseEvent e) {
                onMouseMove(null, e);
            }

            @Override
            public void mouseDragged(MouseEvent e) {
                onMouseMove(null, e);
            }
        });

        SceneManager.getAnimation().addStopped(EventHandler.of(this::onAnimationStopped));
        SceneManager.getAnimation().addStarted(EventHandler.of(this::onAnimationStarted));
        SceneManager.getAnimation().addPaused(EventHandler.of(this::onAnimationPaused));
        SceneManager.getAnimation().addHasReset(EventHandler.of(this::onAnimationHasReset));

        m_overlay = new TextureScreenOverlay(0.0, 0.0, DefaultPanelWidth, ButtonSize);
        m_overlay.setOrigin(ScreenOverlayOrigin.BOTTOM_LEFT);
        m_overlay.setBorderSize(2);
        m_overlay.setColor(Color.GRAY);
        m_overlay.setBorderColor(Color.GRAY);
        m_overlay.setTranslucency(PanelTranslucencyRegular);
        m_overlay.setBorderTranslucency(PanelBorderTranslucencyRegular);
        SceneManager.getScreenOverlays().add(m_overlay);

        // ShowHide button
        addButton(getTexturePath("visible.png"), getTexturePath("invisible.png"), Action.of(this::showHide));

        // Reset button
        addButton(getTexturePath("reset.png"), Action.of(this::reset));

        // DecreaseDelta button
        m_decreaseDeltaButton = addButton(getTexturePath("decreasedelta.png"), Action.of(this::decreaseDelta));

        // StepBack button
        m_stepReverseButton = addButton(getTexturePath("stepreverse.png"), Action.of(this::stepReverse));

        // PlayBack button
        addButton(getTexturePath("playreverse.png"), Action.of(this::playReverse));

        // Pause button
        addButton(getTexturePath("pause.png"), Action.of(this::pause));

        // Play button
        addButton(getTexturePath("playforward.png"), Action.of(this::playForward));

        // StepForward button
        m_stepForwardButton = addButton(getTexturePath("stepforward.png"), Action.of(this::stepForward));

        // IncreaseDelta button
        m_increaseDeltaButton = addButton(getTexturePath("increasedelta.png"), Action.of(this::increaseDelta));

        // Zoom button
        m_zoomButton = addButton(getTexturePath("zoompressed.png"), getTexturePath("zoom.png"), Action.of(this::zoom));

        // Pan button
        addButton(getTexturePath("panpressed.png"), getTexturePath("pan.png"), Action.of(this::pan));

        // Home button
        addButton(getTexturePath("home.png"), Action.of(this::home));

        // Moon Button
        addButton(getTexturePath("moon.png"), Action.of(this::moon));

        // Scale button
        m_scaleButton = new OverlayButtonHolder(m_insight3D, Action.of(this::scale), getTexturePath("scale.png"), 0, m_overlay.getWidth(),
                0.5, 0.0);
        m_scaleButton.getOverlay().setOrigin(ScreenOverlayOrigin.TOP_RIGHT);
        m_overlay.getOverlays().add(m_scaleButton.getOverlay());
        m_buttonHolders.add(m_scaleButton);

        // Rotate button
        m_rotateButton = new OverlayButtonHolder(m_insight3D, Action.of(this::rotate), getTexturePath("rotate.png"), 0,
                m_overlay.getWidth(), 0.5, 0.0);
        m_rotateButton.getOverlay().setOrigin(ScreenOverlayOrigin.BOTTOM_RIGHT);
        m_overlay.getOverlays().add(m_rotateButton.getOverlay());
        m_buttonHolders.add(m_rotateButton);
    }

    private static final String getTexturePath(String filename) {
        return "Data/Textures/OverlayToolbar/" + filename;
    }

    /**
     * Gets the screen overlay that contains the toolbar.
     */
    public final ScreenOverlay getOverlay() {
        return m_overlay;
    }

    /**
     * Adds a simple button to the panel.
     */
    public final OverlayButtonHolder addButton(String image, Action action) {
        return addButton(image, image, action);
    }

    /**
     * Adds a toggle button to the panel, with different images for the enabled and disabled state.
     */
    public final OverlayButtonHolder addButton(String enabledImage, String disabledImage, Action action) {
        m_overlay.setWidth(m_overlay.getWidth() + ButtonSize);

        OverlayButtonHolder buttonHolder = new OverlayButtonHolder(m_insight3D, action, enabledImage, disabledImage, m_LocationOffset,
                m_overlay.getWidth());

        m_overlay.getOverlays().add(buttonHolder.getOverlay());
        m_buttonHolders.add(buttonHolder);

        m_LocationOffset += ButtonSize;

        for (OverlayButtonHolder button : m_buttonHolders) {
            button.resize(m_overlay.getWidth());
        }

        return buttonHolder;
    }

    /**
     * Removes the toolbar from the scene manager.
     */
    public final void remove() {
        SceneManager.getScreenOverlays().remove(m_overlay);
    }

    /**
     * Orients all of the buttons on the Panel so that they do not rotate with the panel,
     * but, rather, flip every 90 degrees in order to remain upright.
     */
    private void orientButtons() {
        double buttonAngle;
        if (m_overlay.getRotationAngle() <= -Math.PI / 4 || m_overlay.getRotationAngle() > 5 * Math.PI / 4) {
            buttonAngle = Constants.HalfPi;
        } else if (m_overlay.getRotationAngle() > -Math.PI / 4 && m_overlay.getRotationAngle() <= Math.PI / 4) {
            buttonAngle = 0;
        } else if (m_overlay.getRotationAngle() > Math.PI / 4 && m_overlay.getRotationAngle() <= 3 * Math.PI / 4) {
            buttonAngle = -Constants.HalfPi;
        } else if (m_overlay.getRotationAngle() > 3 * Math.PI / 4 && m_overlay.getRotationAngle() <= 5 * Math.PI / 4) {
            buttonAngle = -Math.PI;
        } else {
            return;
        }

        for (OverlayButtonHolder buttonHolder : m_buttonHolders) {
            if (buttonHolder != m_rotateButton && buttonHolder != m_scaleButton) {
                buttonHolder.getOverlay().setRotationAngle(buttonAngle);
            }
        }
    }

    /**
     * Finds a button using a pick result.
     */
    private OverlayButtonHolder findButton(List<ScreenOverlayPickResult> picked) {
        for (ScreenOverlayPickResult pickResult : picked) {
            OverlayButtonHolder button = findButton(pickResult.getOverlay());
            if (button != null) {
                return button;
            }
        }
        return null;
    }

    /**
     * Finds a button using an overlay.
     */
    private OverlayButtonHolder findButton(ScreenOverlay overlay) {
        for (OverlayButtonHolder button : m_buttonHolders) {
            if (button.getOverlay() == overlay) {
                return button;
            }
        }
        return null;
    }

    /**
     * Finds an overlay panel using a pick result.
     */
    private boolean overlayPanelPicked(List<ScreenOverlayPickResult> picked) {
        for (ScreenOverlayPickResult pickResult : picked) {
            if (pickResult.getOverlay() == m_overlay) {
                return true;
            }
        }
        return false;
    }

    /**
     * Enables/disables all buttons except for one.
     */
    private void enableButtons(OverlayButtonHolder excludeButton, boolean enabled) {
        for (OverlayButtonHolder button : m_buttonHolders) {
            if (button != excludeButton) {
                button.getOverlay().setPickingEnabled(enabled);
            }
        }
    }

    /**
     * Called when the mouse is moved.
     */
    public final void onMouseMove(Object sender, MouseEvent e) {
        List<ScreenOverlayPickResult> picked = m_insight3D.getScene().pickScreenOverlays(e.getX(), e.getY());
        OverlayButtonHolder button = findButton(picked);

        if (!m_transforming) {
            if (overlayPanelPicked(picked) && !m_panelCurrentlyPicked) {
                m_overlay.setBorderTranslucency(PanelBorderTranslucencyPicked);
                m_overlay.setTranslucency(PanelTranslucencyPicked);
                m_panelCurrentlyPicked = true;
                m_panelTranslucencyChanged = true;
            } else if (!overlayPanelPicked(picked) && m_panelCurrentlyPicked) {
                m_overlay.setBorderTranslucency(PanelBorderTranslucencyRegular);
                m_overlay.setTranslucency(PanelTranslucencyRegular);
                m_panelCurrentlyPicked = false;
                m_panelTranslucencyChanged = true;
            }
            if (m_panelTranslucencyChanged) {
                m_panelTranslucencyChanged = false;
                if (SceneManager.getAnimation().getIsAnimating() == false) {
                    m_insight3D.getScene().render();
                }
            }
        }

        if (button != null) {
            if (m_mouseOverButton != null && m_mouseOverButton != button) {
                m_mouseOverButton.onMouseLeave();
            }
            m_mouseOverButton = button;
            m_mouseOverButton.onMouseEnter();
        } else {
            if (m_anchorPoint.x != 0 || m_anchorPoint.y != 0) {
                Point current = new Point(e.getX(), e.getY());
                current.translate(m_anchorPoint.x, m_anchorPoint.y);
                int offsetX = e.getX() - m_anchorPoint.x;
                int offsetY = m_anchorPoint.y - e.getY();

                // This fixes the bug with the ScreenOverlayOrigin being different.
                // Before, if you dragged left with +x to the left, the panel would
                // have gone right.
                if (getOverlay().getOrigin() == ScreenOverlayOrigin.BOTTOM_RIGHT
                        || getOverlay().getOrigin() == ScreenOverlayOrigin.CENTER_RIGHT
                        || getOverlay().getOrigin() == ScreenOverlayOrigin.TOP_RIGHT) {
                    m_overlay.setTranslationX(m_baseAnchorPoint.x - offsetX);
                } else {
                    m_overlay.setTranslationX(m_baseAnchorPoint.x + offsetX);
                }

                if (getOverlay().getOrigin() == ScreenOverlayOrigin.TOP_RIGHT || getOverlay().getOrigin() == ScreenOverlayOrigin.TOP_CENTER
                        || getOverlay().getOrigin() == ScreenOverlayOrigin.TOP_LEFT) {
                    m_overlay.setTranslationY(m_baseAnchorPoint.y - offsetY);
                } else {
                    m_overlay.setTranslationY(m_baseAnchorPoint.y + offsetY);
                }

                if (SceneManager.getAnimation().getIsAnimating() == false) {
                    m_insight3D.getScene().render();
                }
            } else if (m_rotatePoint.x != 0 || m_rotatePoint.y != 0) {
                Point current = new Point(e.getX(), e.getY());
                current.translate(m_rotatePoint.x, m_rotatePoint.y);
                double centerX = m_overlay.getControlPosition().getX() + m_overlay.getControlBounds().getWidth() / 2.0;
                double centerY = m_overlay.getControlPosition().getY() + m_overlay.getControlBounds().getHeight() / 2.0;
                double adjacent = e.getX() - centerX;
                double opposite = m_insight3D.getBounds().height - e.getY() - centerY;

                if (adjacent >= 0) {
                    m_overlay.setRotationAngle(Math.atan(opposite / adjacent));
                } else {
                    m_overlay.setRotationAngle(Math.PI + Math.atan(opposite / adjacent));
                }

                orientButtons();

                if (SceneManager.getAnimation().getIsAnimating() == false) {
                    m_insight3D.getScene().render();
                }
            } else if (m_scalePoint.x != 0 || m_scalePoint.y != 0) {
                // Get the cos, sin and tan to make this easier to understand.
                double cos = Math.cos(m_overlay.getRotationAngle());
                double sin = Math.sin(m_overlay.getRotationAngle());
                double tan = Math.tan(m_overlay.getRotationAngle());

                double xVector = e.getX() - m_scalePoint.x;
                double yVector = m_scalePoint.y - e.getY();

                // Get the projection of e.X and e.Y in the direction
                // of the toolbar's horizontal.
                double x = (xVector * cos + yVector * sin) * cos;
                double y = (xVector * cos + yVector * sin) * sin;

                // Figure out if we are shrinking or growing the toolbar
                // (This is dependent on the quadrant we are in)
                double magnitude = Math.sqrt(Math.pow(x, 2) + Math.pow(y, 2));
                if (sin >= 0 && cos >= 0 && tan >= 0) {
                    magnitude = x < 0 || y < 0 ? -magnitude : magnitude;
                } else if (sin >= 0) {
                    magnitude = x > 0 || y < 0 ? -magnitude : magnitude;
                } else if (tan >= 0) {
                    magnitude = x > 0 || y > 0 ? -magnitude : magnitude;
                } else if (cos >= 0) {
                    magnitude = x < 0 || y > 0 ? -magnitude : magnitude;
                }

                double scale = (magnitude + m_scaleBounds.getWidth()) / m_scaleBounds.getWidth();

                if (scale < 0) {
                    scale = 0;
                }

                m_overlay.setScale(Math.min(Math.max(m_startScale * scale, 0.5), 10));
                double width = m_overlay.getWidth() * m_overlay.getScale();
                double startWidth = m_overlay.getWidth() * m_startScale;

                // Translate the toolbar in order to account for the
                // fact that rotation does not affect the location
                // of the toolbar, but just rotates the texture.
                // (This causes the toolbar, if +/-90 degrees to scale
                // off the screen if not fixed).
                m_overlay.setTranslationX(
                        m_scaleOffset - (width / 2.0 - startWidth / 2.0) * Math.abs(Math.sin(m_overlay.getRotationAngle())));

                if (SceneManager.getAnimation().getIsAnimating() == false) {
                    m_insight3D.getScene().render();
                }
            } else if (m_mouseOverButton != null) {
                m_mouseOverButton.onMouseLeave();
                m_mouseOverButton = null;
            }
        }
    }

    /**
     * Called when the mouse is pressed.
     */
    private void onMouseDown(Object sender, MouseEvent e) {
        List<ScreenOverlayPickResult> picked = m_insight3D.getScene().pickScreenOverlays(e.getX(), e.getY());
        OverlayButtonHolder button = findButton(picked);

        if (button == null && overlayPanelPicked(picked)) {
            m_mouseDown = true;
            m_insight3D.getMouseOptions().setAutoHandleMouse(false);
            m_anchorPoint = e.getPoint();

            m_overlay.setTranslucency(PanelTranslucencyClicked);
            m_overlay.setBorderTranslucency(PanelBorderTranslucencyClicked);
            m_insight3D.getScene().render();
        }

        if (button != null) {
            m_mouseDown = true;
            m_mouseOverButton = button;
            m_mouseOverButton.onMouseDown();

            if (button == m_rotateButton) {
                m_transforming = true;
                m_insight3D.getMouseOptions().setAutoHandleMouse(false);
                m_rotatePoint = e.getPoint();
                enableButtons(m_rotateButton, false);
            } else if (button == m_scaleButton) {
                m_transforming = true;
                m_insight3D.getMouseOptions().setAutoHandleMouse(false);
                m_scalePoint = e.getPoint();
                m_startScale = m_overlay.getScale();
                m_scaleOffset = m_overlay.getTranslationX();
                m_scaleBounds = m_overlay.getControlBounds();
                enableButtons(m_scaleButton, false);
            }
        }
    }

    /**
     * Called when the mouse is released.
     */
    private void onMouseUp(Object sender, MouseEvent e) {
        if (m_mouseDown) {
            List<ScreenOverlayPickResult> picked = m_insight3D.getScene().pickScreenOverlays(e.getX(), e.getY());
            OverlayButtonHolder button = findButton(picked);

            if (button == null && overlayPanelPicked(picked)) {
                m_overlay.setTranslucency(PanelTranslucencyPicked);
                m_overlay.setBorderTranslucency(PanelBorderTranslucencyPicked);
                if (SceneManager.getAnimation().getIsAnimating() == false) {
                    m_insight3D.getScene().render();
                }
            }

            if (button != null) {
                m_mouseOverButton = button;
                m_mouseOverButton.onMouseUp();
            }

            m_anchorPoint = new Point();
            m_rotatePoint = new Point();
            m_scalePoint = new Point();

            enableButtons(null, true);
            m_baseAnchorPoint = new Point((int) m_overlay.getTranslationX(), (int) m_overlay.getTranslationY());

            m_transforming = false;
            m_insight3D.getMouseOptions().setAutoHandleMouse(true);
            m_mouseDown = false;
        }
    }

    /**
     * Called when the mouse is clicked.
     */
    private void onMouseClick(Object sender, MouseEvent e) {
        List<ScreenOverlayPickResult> picked = m_insight3D.getScene().pickScreenOverlays(e.getX(), e.getY());
        OverlayButtonHolder button = findButton(picked);

        if (button != null) {
            m_mouseOverButton = button;
            m_mouseOverButton.onMouseClick();
        }
    }

    /**
     * Called when the mouse is double clicked.
     */
    private void onMouseDoubleClick(Object sender, MouseEvent e) {
        List<ScreenOverlayPickResult> picked = m_insight3D.getScene().pickScreenOverlays(e.getX(), e.getY());
        OverlayButtonHolder button = findButton(picked);

        if (button == null && overlayPanelPicked(picked)) {
            m_overlay.setTranslationX(0.0);
            m_overlay.setTranslationY(0.0);

            m_overlay.setRotationAngle(0.0);
            m_overlay.setScale(1.0);
            orientButtons();
        }
    }

    /**
     * Called when animation is stopped.
     */
    private void onAnimationStopped(Object sender, EventArgs e) {
        enableStepButtons();
    }

    /**
     * Called when animation is started.
     */
    private void onAnimationStarted(Object sender, StartedEventArgs e) {
        disableStepButtons();
    }

    /**
     * Called when animation is paused.
     */
    private void onAnimationPaused(Object sender, EventArgs e) {
        enableStepButtons();
    }

    /**
     * Called when animation is reset.
     */
    private void onAnimationHasReset(Object sender, EventArgs e) {
        enableStepButtons();
    }

    /**
     * Show or hide the toolbar.
     */
    public final void showHide() {
        if (m_visible) {
            double x = m_overlay.getScale() * (m_overlay.getSize().getWidth() / 2.0 - ButtonSize / 2.0);
            double y = m_overlay.getScale() * (m_overlay.getSize().getHeight() / 2.0 - ButtonSize / 2.0);
            double z = Math.sqrt(x * x + y * y);
            double panelWidth = m_overlay.getWidth();
            double panelHeight = m_overlay.getSize().getHeight();

            m_overlay.setWidth(ButtonSize);
            m_buttonHolders.get(0).resize(m_overlay.getWidth());

            for (OverlayButtonHolder button : m_buttonHolders) {
                if (button != m_buttonHolders.get(0)) {
                    button.getOverlay().setTranslucency(1.0f);
                }
            }
            int xDelta = (int) (z * Math.cos(m_overlay.getRotationAngle()) - m_overlay.getScale() * panelWidth / 2.0
                    + m_overlay.getScale() * ButtonSize / 2.0);
            m_overlay.setTranslationX(m_overlay.getTranslationX() - xDelta);
            int yDelta = (int) (z * Math.sin(m_overlay.getRotationAngle()) - m_overlay.getScale() * panelHeight / 2.0
                    + m_overlay.getScale() * ButtonSize / 2.0);
            m_overlay.setTranslationY(m_overlay.getTranslationY() - yDelta);
        } else {
            m_overlay.setWidth((int) ((m_buttonHolders.size() - 1.5) * ButtonSize));
            m_buttonHolders.get(0).resize(m_overlay.getWidth());

            double x = m_overlay.getScale() * (m_overlay.getSize().getWidth() / 2.0 - ButtonSize / 2.0);
            double y = m_overlay.getScale() * (m_overlay.getSize().getHeight() / 2.0 - ButtonSize / 2.0);
            double z = Math.sqrt(x * x + y * y);

            for (OverlayButtonHolder button : m_buttonHolders) {
                if (button != m_buttonHolders.get(0)) {
                    button.getOverlay().setTranslucency(0.0f);
                }
            }

            int xDelta = (int) (z * Math.cos(m_overlay.getRotationAngle()) - m_overlay.getScale() * m_overlay.getSize().getWidth() / 2.0
                    + m_overlay.getScale() * ButtonSize / 2.0);
            m_overlay.setTranslationX(m_overlay.getTranslationX() + xDelta);
            int yDelta = (int) (z * Math.sin(m_overlay.getRotationAngle()) - m_overlay.getScale() * m_overlay.getSize().getHeight() / 2.0
                    + m_overlay.getScale() * ButtonSize / 2.0);
            m_overlay.setTranslationY(m_overlay.getTranslationY() + yDelta);
        }
        m_visible = !m_visible;
    }

    /**
     * Reset animation.
     */
    public final void reset() {
        SceneManager.getAnimation().reset();
    }

    private SimulationAnimation getSimulationAnimation() {
        ForwardAnimation animation = SceneManager.getAnimation();
        return animation instanceof SimulationAnimation ? (SimulationAnimation) animation : null;
    }

    /**
     * Take one step in reverse.
     */
    public final void stepReverse() {
        SimulationAnimation simulationAnimation = getSimulationAnimation();
        if (simulationAnimation != null) {
            simulationAnimation.stepBackward();
        }
    }

    /**
     * Animate in reverse.
     */
    public final void playReverse() {
        SimulationAnimation simulationAnimation = getSimulationAnimation();
        if (simulationAnimation != null) {
            simulationAnimation.playBackward();
        }
    }

    /**
     * Pause animation.
     */
    public final void pause() {
        SceneManager.getAnimation().pause();
    }

    /**
     * Animate forward.
     */
    public final void playForward() {
        SceneManager.getAnimation().playForward();
    }

    /**
     * Take one step forward.
     */
    public final void stepForward() {
        SceneManager.getAnimation().stepForward();
    }

    /**
     * Decrease the animation timestep by halfing the value.
     */
    public final void decreaseDelta() {
        SimulationAnimation simulationAnimation = getSimulationAnimation();
        if (simulationAnimation != null) {
            simulationAnimation.setTimeStep(simulationAnimation.getTimeStep().multiply(0.5));
            enableIncreaseDeltaButton();
            if (simulationAnimation.getTimeStep().getTotalSeconds() < 1e-2) {
                disableDecreaseDeltaButton();
            }
        }
    }

    /**
     * Increase the animation timestep by doubling the value.
     */
    public final void increaseDelta() {
        SimulationAnimation simulationAnimation = getSimulationAnimation();
        if (simulationAnimation != null) {
            simulationAnimation.setTimeStep(simulationAnimation.getTimeStep().multiply(2.0));
            enableDecreaseDeltaButton();
            if (simulationAnimation.getTimeStep().getTotalSeconds() > 100000) {
                disableIncreaseDeltaButton();
            }
        }
    }

    /**
     * Switch the mouse to zoom mode.
     */
    public final void zoom() {
        m_insight3D.getMouseOptions().setZooming(true);
    }

    /**
     * Called when zooming is complete.
     */
    private void onZoomComplete(Object sender, EventArgs e) {
        m_insight3D.getMouseOptions().setZooming(false);
        m_zoomButton.setState(m_insight3D.getMouseOptions().getZooming());
    }

    /**
     * Toggle mouse panning mode.
     */
    public final void pan() {
        m_insight3D.getMouseOptions().setPanning(!m_insight3D.getMouseOptions().getPanning());
    }

    /**
     * Reset the camera to the home view.
     */
    public final void home() {
        EarthCentralBody earth = CentralBodiesFacet.getFromContext().getEarth();
        m_insight3D.getScene().getCamera().viewCentralBody(earth, earth.getInertialFrame().getAxes());
    }

    /**
     * Switch the camera to view the Moon.
     */
    public final void moon() {
        MoonCentralBody moon = CentralBodiesFacet.getFromContext().getMoon();
        m_insight3D.getScene().getCamera().viewCentralBody(moon, moon.getInertialFrame().getAxes());
    }

    private void scale() {}

    private void rotate() {}

    private void enableStepButtons() {
        m_stepForwardButton.setEnabled(true);
        m_stepReverseButton.setEnabled(true);
    }

    private void disableStepButtons() {
        m_stepForwardButton.setEnabled(false);
        m_stepReverseButton.setEnabled(false);
    }

    private void enableIncreaseDeltaButton() {
        m_increaseDeltaButton.setEnabled(true);
    }

    private void disableIncreaseDeltaButton() {
        m_increaseDeltaButton.setEnabled(false);
    }

    private void enableDecreaseDeltaButton() {
        m_decreaseDeltaButton.setEnabled(true);
    }

    private void disableDecreaseDeltaButton() {
        m_decreaseDeltaButton.setEnabled(false);
    }

    @Override
    public final void dispose() {
        dispose(true);
    }

    @Override
    protected void finalize() throws Throwable {
        try {
            dispose(false);
        } finally {
            super.finalize();
        }
    }

    protected void dispose(boolean disposing) {
        if (disposing) {
            if (m_overlay != null) {
                m_overlay.dispose();
            }
            if (m_rotateButton != null) {
                m_rotateButton.dispose();
            }
            if (m_scaleButton != null) {
                m_scaleButton.dispose();
            }
            if (m_zoomButton != null) {
                m_zoomButton.dispose();
            }
            if (m_stepForwardButton != null) {
                m_stepForwardButton.dispose();
            }
            if (m_stepReverseButton != null) {
                m_stepReverseButton.dispose();
            }
        }
    }

    /**
     * Holds an individual button in the toolbar. Each button is a separate screen overlay,
     * rendered within the overlay containing the toolbar.
     */
    public static final class OverlayButtonHolder implements IDisposable {
        /**
         * Initializes a new instance.
         */
        public OverlayButtonHolder(Insight3D insight3D, Action action, String image, int xOffset, double panelWidth) {
            this(insight3D, action, image, image, xOffset, panelWidth);
        }

        /**
         * Initializes a new instance.
         */
        public OverlayButtonHolder(Insight3D insight3D, Action action, String enabledImage, String disabledImage, int xOffset,
                double panelWidth) {
            m_insight3D = insight3D;
            m_action = action;
            m_enabled = true;
            m_enabledImage = enabledImage;
            m_disabledImage = disabledImage;
            m_xOffset = xOffset;

            m_overlay = new TextureScreenOverlay();
            m_overlay.setX(xOffset / panelWidth);
            m_overlay.setXUnit(ScreenOverlayUnit.FRACTION);
            m_overlay.setWidth(ButtonSize / panelWidth);
            m_overlay.setWidthUnit(ScreenOverlayUnit.FRACTION);
            m_overlay.setHeight(1.0);
            m_overlay.setHeightUnit(ScreenOverlayUnit.FRACTION);
            m_overlay.setTranslucency(MouseExitTranslucency);
            m_overlay.setTexture(SceneManager.getTextures().fromUri(disabledImage));
        }

        /**
         * Initializes a new instance.
         */
        public OverlayButtonHolder(Insight3D insight3D, Action action, String image, int xOffset, double panelWidth, double scale,
                double rotate) {
            this(insight3D, action, image, xOffset, panelWidth);
            m_overlay.setScale(scale);
            m_overlay.setRotationAngle(rotate);
        }

        /**
         * Gets the screen overlay for this button.
         */
        public final ScreenOverlay getOverlay() {
            return m_overlay;
        }

        /**
         * Sets the on/off state of this button.
         */
        public final void setState(boolean state) {
            m_state = state;
            m_overlay.setTexture(SceneManager.getTextures().fromUri(state ? m_enabledImage : m_disabledImage));
        }

        /**
         * Enables or disables this button.
         */
        public final void setEnabled(boolean enabled) {
            m_enabled = enabled;
            getOverlay().setColor(enabled ? Color.WHITE : Color.GRAY);
        }

        /**
         * Resize this button given the overall toolbar width.
         */
        public final void resize(double toolbarWidth) {
            double width = ButtonSize / toolbarWidth;
            m_overlay.setSize(new ScreenOverlaySize(width, 1.0, ScreenOverlayUnit.FRACTION, ScreenOverlayUnit.FRACTION));
            m_overlay.setX(m_xOffset / toolbarWidth);
            m_overlay.setXUnit(ScreenOverlayUnit.FRACTION);
        }

        /**
         * Called when the mouse enters this button.
         */
        public void onMouseEnter() {
            if (m_enabled) {
                getOverlay().setTranslucency(MouseEnterTranslucency);
                if (SceneManager.getAnimation().getIsAnimating() == false) {
                    m_insight3D.getScene().render();
                }
            }
        }

        /**
         * Called when the mouse leaves this button.
         */
        public void onMouseLeave() {
            if (m_enabled) {
                getOverlay().setTranslucency(MouseExitTranslucency);
                getOverlay().setColor(Color.WHITE);
                if (SceneManager.getAnimation().getIsAnimating() == false) {
                    m_insight3D.getScene().render();
                }
            }
        }

        /**
         * Called when the mouse is pressed on this button.
         */
        public void onMouseDown() {
            if (m_enabled) {
                Color darkGray = new Color(0xA9A9A9);
                getOverlay().setColor(darkGray);
                if (SceneManager.getAnimation().getIsAnimating() == false) {
                    m_insight3D.getScene().render();
                }
            }
        }

        /**
         * Called when the mouse is released on this button.
         */
        public void onMouseUp() {
            if (m_enabled) {
                getOverlay().setColor(Color.WHITE);
                if (SceneManager.getAnimation().getIsAnimating() == false) {
                    m_insight3D.getScene().render();
                }
            }
        }

        /**
         * Called when the mouse is clicked on this button.
         */
        public void onMouseClick() {
            if (m_enabled) {
                setState(!m_state);

                if (m_action != null) {
                    m_action.invoke();
                }
            }
        }

        @Override
        public final void dispose() {
            dispose(true);
        }

        @Override
        protected void finalize() throws Throwable {
            try {
                dispose(false);
            } finally {
                super.finalize();
            }
        }

        protected void dispose(boolean disposing) {
            if (disposing) {
                if (m_overlay != null) {
                    m_overlay.dispose();
                }
            }
        }

        private static final float MouseEnterTranslucency = 0.01f;
        private static final float MouseExitTranslucency = 0.25f;

        private final Insight3D m_insight3D;
        private final Action m_action;
        private final String m_enabledImage;
        private final String m_disabledImage;
        private final int m_xOffset;
        private final TextureScreenOverlay m_overlay;

        private boolean m_state;
        private boolean m_enabled;
    }

    private static final float PanelTranslucencyRegular = 0.95f;
    private static final float PanelTranslucencyPicked = 0.85f;
    private static final float PanelTranslucencyClicked = 0.8f;
    private static final float PanelBorderTranslucencyRegular = 0.6f;
    private static final float PanelBorderTranslucencyPicked = 0.5f;
    private static final float PanelBorderTranslucencyClicked = 0.4f;
    private static final int ButtonSize = 35;
    private static final int DefaultPanelWidth = (int) (ButtonSize * 0.5);

    private final Insight3D m_insight3D;
    private final ArrayList<OverlayButtonHolder> m_buttonHolders;

    private int m_LocationOffset;

    private final TextureScreenOverlay m_overlay;

    private final OverlayButtonHolder m_rotateButton;
    private final OverlayButtonHolder m_scaleButton;
    private final OverlayButtonHolder m_zoomButton;
    private final OverlayButtonHolder m_stepForwardButton;
    private final OverlayButtonHolder m_stepReverseButton;
    private final OverlayButtonHolder m_increaseDeltaButton;
    private final OverlayButtonHolder m_decreaseDeltaButton;

    private Point m_anchorPoint = new Point();
    private Point m_rotatePoint = new Point();
    private Point m_scalePoint = new Point();
    private Point m_baseAnchorPoint = new Point();

    private BoundingRectangle m_scaleBounds;
    private double m_startScale;
    private double m_scaleOffset;

    private boolean m_panelTranslucencyChanged;
    private boolean m_panelCurrentlyPicked;
    private boolean m_visible = true;
    private boolean m_transforming;

    private OverlayButtonHolder m_mouseOverButton;
    private boolean m_mouseDown;
}