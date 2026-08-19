using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AGI.Foundation;
using AGI.Foundation.Celestial;
using AGI.Foundation.Coordinates;
using AGI.Foundation.Graphics;
using AGI.Foundation.Graphics.Advanced;

namespace AGI.Examples
{
    /// <summary>
    /// Renders a toolbar of buttons as a screen overlay in Insight3D.
    /// </summary>
    public class OverlayToolbar : IDisposable
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public OverlayToolbar(Insight3D insight3D)
        {
            m_insight3D = insight3D;
            m_buttonHolders = new List<OverlayButtonHolder>();

            insight3D.MouseOptions.ZoomComplete += OnZoomComplete;
            insight3D.MouseDown += OnMouseDown;
            insight3D.MouseMove += OnMouseMove;
            insight3D.MouseUp += OnMouseUp;
            insight3D.MouseClick += OnMouseClick;
            insight3D.MouseDoubleClick += OnMouseDoubleClick;
            SceneManager.Animation.Stopped += OnAnimationStopped;
            SceneManager.Animation.Started += OnAnimationStarted;
            SceneManager.Animation.Paused += OnAnimationPaused;
            SceneManager.Animation.HasReset += OnAnimationHasReset;

            m_overlay = new TextureScreenOverlay(0.0, 0.0, DefaultPanelWidth, ButtonSize)
            {
                Origin = ScreenOverlayOrigin.BottomLeft,
                BorderSize = 2,
                Color = Color.Transparent,
                BorderColor = Color.Transparent,
                Translucency = PanelTranslucencyRegular,
                BorderTranslucency = PanelBorderTranslucencyRegular
            };
            SceneManager.ScreenOverlays.Add(m_overlay);

            // ShowHide button
            AddButton(GetTexturePath("visible.png"), GetTexturePath("invisible.png"), ShowHide);

            // Reset button
            AddButton(GetTexturePath("reset.png"), Reset);

            // DecreaseDelta button
            m_decreaseDeltaButton = AddButton(GetTexturePath("decreasedelta.png"), DecreaseDelta);

            // StepBack button
            m_stepReverseButton = AddButton(GetTexturePath("stepreverse.png"), StepReverse);

            // PlayBack button
            AddButton(GetTexturePath("playreverse.png"), PlayReverse);

            // Pause button
            AddButton(GetTexturePath("pause.png"), Pause);

            // Play button
            AddButton(GetTexturePath("playforward.png"), PlayForward);

            // StepForward button
            m_stepForwardButton = AddButton(GetTexturePath("stepforward.png"), StepForward);

            // IncreaseDelta button
            m_increaseDeltaButton = AddButton(GetTexturePath("increasedelta.png"), IncreaseDelta);

            // Zoom button
            AddButton(GetTexturePath("zoompressed.png"), GetTexturePath("zoom.png"), Zoom);
            m_zoomButton = m_buttonHolders[m_buttonHolders.Count - 1];

            // Pan button
            AddButton(GetTexturePath("panpressed.png"), GetTexturePath("pan.png"), Pan);

            // Home button
            AddButton(GetTexturePath("home.png"), Home);

            // Moon Button
            AddButton(GetTexturePath("moon.png"), Moon);

            // Scale button
            m_scaleButton = new OverlayButtonHolder(m_insight3D, Scale, GetTexturePath("scale.png"), 0, m_overlay.Width, 0.5, 0.0);
            m_scaleButton.Overlay.Origin = ScreenOverlayOrigin.TopRight;
            m_overlay.Overlays.Add(m_scaleButton.Overlay);
            m_buttonHolders.Add(m_scaleButton);

            // Rotate button
            m_rotateButton = new OverlayButtonHolder(m_insight3D, Rotate, GetTexturePath("rotate.png"), 0, m_overlay.Width, 0.5, 0.0);
            m_rotateButton.Overlay.Origin = ScreenOverlayOrigin.BottomRight;
            m_overlay.Overlays.Add(m_rotateButton.Overlay);
            m_buttonHolders.Add(m_rotateButton);

            DockBottom();
        }

        private static string GetTexturePath(string filename)
        {
            string dataPath = Path.Combine(Application.StartupPath, "../../../Data");
            string texturesPath = Path.Combine(dataPath, "Textures");
            string overlayToolbarPath = Path.Combine(texturesPath, "OverlayToolbar");
            return Path.Combine(overlayToolbarPath, filename);
        }

        /// <summary>
        /// Gets the screen overlay that contains the toolbar.
        /// </summary>
        public ScreenOverlay Overlay
        {
            get { return m_overlay; }
        }

        /// <summary>
        /// Adds a simple button to the panel.
        /// </summary>
        public OverlayButtonHolder AddButton(string image, Action action)
        {
            return AddButton(image, image, action);
        }

        /// <summary>
        /// Adds a toggle button to the panel, with different images for the enabled and disabled state.
        /// </summary>
        public OverlayButtonHolder AddButton(string enabledImage, string disabledImage, Action action)
        {
            m_overlay.Width += ButtonSize;

            var buttonHolder = new OverlayButtonHolder(m_insight3D, action, enabledImage, disabledImage, m_locationOffset, m_overlay.Width);

            m_overlay.Overlays.Add(buttonHolder.Overlay);
            m_buttonHolders.Add(buttonHolder);

            m_locationOffset += ButtonSize;

            foreach (var button in m_buttonHolders)
            {
                button.Resize(m_overlay.Width);
            }

            return buttonHolder;
        }

        /// <summary>
        /// Docks the toolbar to the right of the screen.
        /// </summary>
        public void DockRight()
        {
            m_overlay.Origin = ScreenOverlayOrigin.CenterRight;
            m_overlay.RotationAngle = Constants.HalfPi;
            OrientButtons();
            m_overlay.TranslationX = -(m_overlay.Width / 2.0 - ButtonSize / 2.0) * m_overlay.Scale;
            m_baseAnchorPoint = new Point((int)m_overlay.TranslationX, (int)m_overlay.TranslationY);
        }

        /// <summary>
        /// Docks the toolbar to the bottom of the screen.
        /// </summary>
        public void DockBottom()
        {
            m_overlay.Origin = ScreenOverlayOrigin.BottomCenter;
            m_overlay.RotationAngle = 0;
            OrientButtons();
            m_overlay.TranslationY = 0;
            m_baseAnchorPoint = new Point((int)m_overlay.TranslationX, (int)m_overlay.TranslationY);
        }

        /// <summary>
        /// Docks the toolbar to the left of the screen.
        /// </summary>
        public void DockLeft()
        {
            m_overlay.Origin = ScreenOverlayOrigin.CenterLeft;
            m_overlay.RotationAngle = Constants.HalfPi;
            OrientButtons();
            m_overlay.TranslationX = -(m_overlay.Width / 2.0 - ButtonSize / 2.0) * m_overlay.Scale;
            m_baseAnchorPoint = new Point((int)m_overlay.TranslationX, (int)m_overlay.TranslationY);
        }

        /// <summary>
        /// Docks the toolbar to the top of the screen.
        /// </summary>
        public void DockTop()
        {
            m_overlay.Origin = ScreenOverlayOrigin.TopCenter;
            m_overlay.RotationAngle = 0;
            OrientButtons();
            m_overlay.TranslationY = 0;
            m_baseAnchorPoint = new Point((int)m_overlay.TranslationX, (int)m_overlay.TranslationY);
        }

        /// <summary>
        /// Removes the toolbar from the scene manager.
        /// </summary>
        public void Remove()
        {
            SceneManager.ScreenOverlays.Remove(m_overlay);
        }

        /// <summary>
        /// Orients all of the buttons on the toolbar so that they do not rotate with the panel, 
        /// but, rather, flip every 90 degrees in order to remain upright.
        /// </summary>
        private void OrientButtons()
        {
            double buttonAngle;
            if (m_overlay.RotationAngle <= -Math.PI / 4 || m_overlay.RotationAngle > 5 * Math.PI / 4)
            {
                buttonAngle = Constants.HalfPi;
            }
            else if (m_overlay.RotationAngle > -Math.PI / 4 && m_overlay.RotationAngle <= Math.PI / 4)
            {
                buttonAngle = 0;
            }
            else if (m_overlay.RotationAngle > Math.PI / 4 && m_overlay.RotationAngle <= 3 * Math.PI / 4)
            {
                buttonAngle = -Constants.HalfPi;
            }
            else if (m_overlay.RotationAngle > 3 * Math.PI / 4 && m_overlay.RotationAngle <= 5 * Math.PI / 4)
            {
                buttonAngle = -Math.PI;
            }
            else
            {
                return;
            }

            foreach (var buttonHolder in m_buttonHolders.Where(buttonHolder => buttonHolder != m_rotateButton && buttonHolder != m_scaleButton))
            {
                buttonHolder.Overlay.RotationAngle = buttonAngle;
            }
        }

        /// <summary>
        /// Finds a button using a pick result.
        /// </summary>
        private OverlayButtonHolder FindButton(IEnumerable<ScreenOverlayPickResult> picked)
        {
            return picked.Select(pickResult => FindButton(pickResult.Overlay))
                         .FirstOrDefault(button => button != null);
        }

        /// <summary>
        /// Finds a button using an overlay.
        /// </summary>
        private OverlayButtonHolder FindButton(ScreenOverlay overlay)
        {
            return m_buttonHolders.FirstOrDefault(button => button.Overlay == overlay);
        }

        /// <summary>
        /// Finds an overlay panel using a pick result.
        /// </summary>
        private bool OverlayPanelPicked(IEnumerable<ScreenOverlayPickResult> picked)
        {
            return picked.Any(pickResult => pickResult.Overlay == m_overlay);
        }

        /// <summary>
        /// Enables/disables all buttons except for one.
        /// </summary>
        private void EnableButtons(OverlayButtonHolder excludeButton, bool enabled)
        {
            foreach (var button in m_buttonHolders.Where(button => button != excludeButton))
            {
                button.Overlay.PickingEnabled = enabled;
            }
        }

        /// <summary>
        /// Called when the mouse is moved.
        /// </summary>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var picked = m_insight3D.Scene.PickScreenOverlays(e.X, e.Y);
            var button = FindButton(picked);

            if (!m_transforming)
            {
                if (OverlayPanelPicked(picked) && !m_panelCurrentlyPicked)
                {
                    m_overlay.BorderTranslucency = PanelBorderTranslucencyPicked;
                    m_overlay.Translucency = PanelTranslucencyPicked;
                    m_panelCurrentlyPicked = true;
                    m_panelTranslucencyChanged = true;
                }
                else if (!OverlayPanelPicked(picked) && m_panelCurrentlyPicked)
                {
                    m_overlay.BorderTranslucency = PanelBorderTranslucencyRegular;
                    m_overlay.Translucency = PanelTranslucencyRegular;
                    m_panelCurrentlyPicked = false;
                    m_panelTranslucencyChanged = true;
                }
                if (m_panelTranslucencyChanged)
                {
                    m_panelTranslucencyChanged = false;
                    if (SceneManager.Animation.IsAnimating == false)
                    {
                        m_insight3D.Scene.Render();
                    }
                }
            }

            if (button != null)
            {
                if (m_mouseOverButton != null && m_mouseOverButton != button)
                {
                    m_mouseOverButton.OnMouseLeave();
                }
                m_mouseOverButton = button;
                m_mouseOverButton.OnMouseEnter();
            }
            else
            {
                if (m_anchorPoint != Point.Empty)
                {
                    Point current = new Point(e.X, e.Y);
                    current.Offset(m_anchorPoint);
                    int offsetX = e.X - m_anchorPoint.X;
                    int offsetY = m_anchorPoint.Y - e.Y;

                    // This fixes the bug with the ScreenOverlayOrigin being different.
                    // Before, if you dragged left with +x to the left, the panel would
                    // have gone right.
                    if (Overlay.Origin == ScreenOverlayOrigin.BottomRight || Overlay.Origin == ScreenOverlayOrigin.CenterRight || Overlay.Origin == ScreenOverlayOrigin.TopRight)
                    {
                        m_overlay.TranslationX = m_baseAnchorPoint.X - offsetX;
                    }
                    else
                    {
                        m_overlay.TranslationX = m_baseAnchorPoint.X + offsetX;
                    }

                    if (Overlay.Origin == ScreenOverlayOrigin.TopRight || Overlay.Origin == ScreenOverlayOrigin.TopCenter || Overlay.Origin == ScreenOverlayOrigin.TopLeft)
                    {
                        m_overlay.TranslationY = m_baseAnchorPoint.Y - offsetY;
                    }
                    else
                    {
                        m_overlay.TranslationY = m_baseAnchorPoint.Y + offsetY;
                    }

                    if (SceneManager.Animation.IsAnimating == false)
                    {
                        m_insight3D.Scene.Render();
                    }
                }
                else if (m_rotatePoint != Point.Empty)
                {
                    Point current = new Point(e.X, e.Y);
                    current.Offset(m_rotatePoint);
                    double centerX = m_overlay.ControlPosition.X + m_overlay.ControlBounds.Width / 2.0;
                    double centerY = m_overlay.ControlPosition.Y + m_overlay.ControlBounds.Height / 2.0;
                    double adjacent = e.X - centerX;
                    double opposite = m_insight3D.Bounds.Height - e.Y - centerY;

                    if (adjacent >= 0)
                    {
                        m_overlay.RotationAngle = Math.Atan(opposite / adjacent);
                    }
                    else
                    {
                        m_overlay.RotationAngle = Math.PI + Math.Atan(opposite / adjacent);
                    }

                    OrientButtons();

                    if (SceneManager.Animation.IsAnimating == false)
                    {
                        m_insight3D.Scene.Render();
                    }
                }
                else if (m_scalePoint != Point.Empty)
                {
                    // Get the cos, sin and tan to make this easier to understand.
                    double cos = Math.Cos(m_overlay.RotationAngle);
                    double sin = Math.Sin(m_overlay.RotationAngle);
                    double tan = Math.Tan(m_overlay.RotationAngle);

                    double xVector = e.X - m_scalePoint.X;
                    double yVector = m_scalePoint.Y - e.Y;

                    // Get the projection of e.X and e.Y in the direction
                    // of the toolbar's horizontal.
                    double x = (xVector * cos + yVector * sin) * cos;
                    double y = (xVector * cos + yVector * sin) * sin;

                    // Figure out if we are shrinking or growing the toolbar
                    // (This is dependant on the quadrant we are in)
                    double magnitude = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
                    if (sin >= 0 && cos >= 0 && tan >= 0)
                    {
                        magnitude = x < 0 || y < 0 ? -magnitude : magnitude;
                    }
                    else if (sin >= 0)
                    {
                        magnitude = x > 0 || y < 0 ? -magnitude : magnitude;
                    }
                    else if (tan >= 0)
                    {
                        magnitude = x > 0 || y > 0 ? -magnitude : magnitude;
                    }
                    else if (cos >= 0)
                    {
                        magnitude = x < 0 || y > 0 ? -magnitude : magnitude;
                    }

                    double scale = (magnitude + m_scaleBounds.Width) / m_scaleBounds.Width;

                    if (scale < 0)
                    {
                        scale = 0;
                    }

                    m_overlay.Scale = Math.Min(Math.Max(m_startScale * scale, 0.5), 10);
                    double width = m_overlay.Width * m_overlay.Scale;
                    double startWidth = m_overlay.Width * m_startScale;

                    // Translate the toolbar in order to account for the
                    // fact that rotation does not affect the location
                    // of the toolbar, but just rotates the texture.
                    // (This causes the toolbar, if +/-90 degrees to scale
                    // off the screen if not fixed).
                    m_overlay.TranslationX = m_scaleOffset - (width / 2.0 - startWidth / 2.0) * Math.Abs(Math.Sin(m_overlay.RotationAngle));

                    if (SceneManager.Animation.IsAnimating == false)
                    {
                        m_insight3D.Scene.Render();
                    }
                }
                else if (m_mouseOverButton != null)
                {
                    m_mouseOverButton.OnMouseLeave();
                    m_mouseOverButton = null;
                }
            }
        }

        /// <summary>
        /// Called when the mouse is pressed.
        /// </summary>
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            var picked = m_insight3D.Scene.PickScreenOverlays(e.X, e.Y);
            var button = FindButton(picked);

            if (button == null && OverlayPanelPicked(picked))
            {
                m_mouseDown = true;
                m_insight3D.MouseOptions.AutoHandleMouse = false;
                m_anchorPoint = e.Location;

                m_overlay.Translucency = PanelTranslucencyClicked;
                m_overlay.BorderTranslucency = PanelBorderTranslucencyClicked;
                m_insight3D.Scene.Render();
            }

            if (button != null)
            {
                m_mouseDown = true;
                m_mouseOverButton = button;
                m_mouseOverButton.OnMouseDown();

                if (button == m_rotateButton)
                {
                    m_transforming = true;
                    m_insight3D.MouseOptions.AutoHandleMouse = false;
                    m_rotatePoint = e.Location;
                    EnableButtons(m_rotateButton, false);
                }
                else if (button == m_scaleButton)
                {
                    m_transforming = true;
                    m_insight3D.MouseOptions.AutoHandleMouse = false;
                    m_scalePoint = e.Location;
                    m_startScale = m_overlay.Scale;
                    m_scaleOffset = m_overlay.TranslationX;
                    m_scaleBounds = m_overlay.ControlBounds;
                    EnableButtons(m_scaleButton, false);
                }
            }
        }

        /// <summary>
        /// Called when the mouse is released.
        /// </summary>
        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (m_mouseDown)
            {
                var picked = m_insight3D.Scene.PickScreenOverlays(e.X, e.Y);
                var button = FindButton(picked);

                if (button == null && OverlayPanelPicked(picked))
                {
                    m_overlay.Translucency = PanelTranslucencyPicked;
                    m_overlay.BorderTranslucency = PanelBorderTranslucencyPicked;
                    if (SceneManager.Animation.IsAnimating == false)
                    {
                        m_insight3D.Scene.Render();
                    }
                }

                if (button != null)
                {
                    m_mouseOverButton = button;
                    m_mouseOverButton.OnMouseUp();
                }

                m_anchorPoint = Point.Empty;
                m_rotatePoint = Point.Empty;
                m_scalePoint = Point.Empty;

                EnableButtons(null, true);
                m_baseAnchorPoint = new Point((int)m_overlay.TranslationX, (int)m_overlay.TranslationY);

                m_transforming = false;
                m_insight3D.MouseOptions.AutoHandleMouse = true;
                m_mouseDown = false;
            }
        }

        /// <summary>
        /// Called when the mouse is clicked.
        /// </summary>
        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (m_mouseDown)
            {
                var picked = m_insight3D.Scene.PickScreenOverlays(e.X, e.Y);
                var button = FindButton(picked);

                if (button != null)
                {
                    m_mouseOverButton = button;
                    m_mouseOverButton.OnMouseClick();
                }
            }
        }

        /// <summary>
        /// Called when the mouse is double clicked.
        /// </summary>
        private void OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
            var picked = m_insight3D.Scene.PickScreenOverlays(e.X, e.Y);
            var button = FindButton(picked);

            if (button == null && OverlayPanelPicked(picked))
            {
                m_overlay.TranslationX = 0.0;
                m_overlay.TranslationY = 0.0;

                m_overlay.RotationAngle = 0.0;
                m_overlay.Scale = 1.0;
                OrientButtons();
            }
        }

        /// <summary>
        /// Called when animation is stopped.
        /// </summary>
        private void OnAnimationStopped(object sender, EventArgs e)
        {
            EnableStepButtons();
        }

        /// <summary>
        /// Called when animation is started.
        /// </summary>
        private void OnAnimationStarted(object sender, StartedEventArgs e)
        {
            DisableStepButtons();
        }

        /// <summary>
        /// Called when animation is paused.
        /// </summary>
        private void OnAnimationPaused(object sender, EventArgs e)
        {
            EnableStepButtons();
        }

        /// <summary>
        /// Called when animation is reset.
        /// </summary>
        private void OnAnimationHasReset(object sender, EventArgs e)
        {
            EnableStepButtons();
        }

        /// <summary>
        /// Show or hide the toolbar.
        /// </summary>
        public void ShowHide()
        {
            if (m_visible)
            {
                double x = m_overlay.Scale * (m_overlay.Size.Width / 2.0 - ButtonSize / 2.0);
                double y = m_overlay.Scale * (m_overlay.Size.Height / 2.0 - ButtonSize / 2.0);
                double z = Math.Sqrt(x * x + y * y);
                double panelWidth = m_overlay.Width;
                double panelHeight = m_overlay.Size.Height;

                m_overlay.Width = ButtonSize;
                m_buttonHolders[0].Resize(m_overlay.Width);

                foreach (OverlayButtonHolder button in m_buttonHolders)
                {
                    if (button != m_buttonHolders[0])
                    {
                        button.Overlay.Translucency = 1.0f;
                    }
                }

                int xDelta = (int)(z * Math.Cos(m_overlay.RotationAngle) - m_overlay.Scale * panelWidth / 2.0 + m_overlay.Scale * ButtonSize / 2.0);
                m_overlay.TranslationX -= xDelta;
                int yDelta = (int)(z * Math.Sin(m_overlay.RotationAngle) - m_overlay.Scale * panelHeight / 2.0 + m_overlay.Scale * ButtonSize / 2.0);
                m_overlay.TranslationY -= yDelta;
            }
            else
            {
                m_overlay.Width = (int)((m_buttonHolders.Count - 1.5) * ButtonSize);
                m_buttonHolders[0].Resize(m_overlay.Width);

                double x = m_overlay.Scale * (m_overlay.Size.Width / 2.0 - ButtonSize / 2.0);
                double y = m_overlay.Scale * (m_overlay.Size.Height / 2.0 - ButtonSize / 2.0);
                double z = Math.Sqrt(x * x + y * y);

                foreach (OverlayButtonHolder button in m_buttonHolders)
                {
                    if (button != m_buttonHolders[0])
                    {
                        button.Overlay.Translucency = 0.0f;
                    }
                }

                int xDelta = (int)(z * Math.Cos(m_overlay.RotationAngle) - m_overlay.Scale * m_overlay.Size.Width / 2.0 + m_overlay.Scale * ButtonSize / 2.0);
                m_overlay.TranslationX += xDelta;
                int yDelta = (int)(z * Math.Sin(m_overlay.RotationAngle) - m_overlay.Scale * m_overlay.Size.Height / 2.0 + m_overlay.Scale * ButtonSize / 2.0);
                m_overlay.TranslationY += yDelta;
            }

            m_visible = !m_visible;
        }

        /// <summary>
        /// Reset animation.
        /// </summary>
        public void Reset()
        {
            SceneManager.Animation.Reset();
        }

        /// <summary>
        /// Take one step in reverse.
        /// </summary>
        public void StepReverse()
        {
            var simulationAnimation = SceneManager.Animation as SimulationAnimation;
            if (simulationAnimation != null)
                simulationAnimation.StepBackward();
        }

        /// <summary>
        /// Animate in reverse.
        /// </summary>
        public void PlayReverse()
        {
            var simulationAnimation = SceneManager.Animation as SimulationAnimation;
            if (simulationAnimation != null)
                simulationAnimation.PlayBackward();
        }

        /// <summary>
        /// Pause animation.
        /// </summary>
        public void Pause()
        {
            SceneManager.Animation.Pause();
        }

        /// <summary>
        /// Animate forward.
        /// </summary>
        public void PlayForward()
        {
            SceneManager.Animation.PlayForward();
        }

        /// <summary>
        /// Take one step forward.
        /// </summary>
        public void StepForward()
        {
            SceneManager.Animation.StepForward();
        }

        /// <summary>
        /// Decrease the animation timestep by halfing the value.
        /// </summary>
        public void DecreaseDelta()
        {
            var simulationAnimation = SceneManager.Animation as SimulationAnimation;
            if (simulationAnimation != null)
            {
                simulationAnimation.TimeStep *= 0.5;
                EnableIncreaseDeltaButton();
                if (simulationAnimation.TimeStep.TotalSeconds < 1e-2)
                    DisableDecreaseDeltaButton();
            }
        }

        /// <summary>
        /// Increase the animation timestep by doubling the value.
        /// </summary>
        public void IncreaseDelta()
        {
            var simulationAnimation = SceneManager.Animation as SimulationAnimation;
            if (simulationAnimation != null)
            {
                simulationAnimation.TimeStep *= 2.0;
                EnableDecreaseDeltaButton();
                if (simulationAnimation.TimeStep.TotalSeconds > 100000)
                    DisableIncreaseDeltaButton();
            }
        }

        /// <summary>
        /// Switch the mouse to zoom mode.
        /// </summary>
        public void Zoom()
        {
            m_insight3D.MouseOptions.Zooming = true;
        }

        /// <summary>
        /// Called when zooming is complete.
        /// </summary>
        private void OnZoomComplete(object sender, EventArgs e)
        {
            m_insight3D.MouseOptions.Zooming = false;
            m_zoomButton.SetState(m_insight3D.MouseOptions.Zooming);
        }

        /// <summary>
        /// Toggle mouse panning mode.
        /// </summary>
        public void Pan()
        {
            m_insight3D.MouseOptions.Panning = !m_insight3D.MouseOptions.Panning;
        }

        /// <summary>
        /// Reset the camera to the home view.
        /// </summary>
        public void Home()
        {
            var earth = CentralBodiesFacet.GetFromContext().Earth;
            m_insight3D.Scene.Camera.ViewCentralBody(earth, earth.InertialFrame.Axes);
        }

        /// <summary>
        /// Switch the camera to view the Moon.
        /// </summary>
        public void Moon()
        {
            var moon = CentralBodiesFacet.GetFromContext().Moon;
            m_insight3D.Scene.Camera.ViewCentralBody(moon, moon.InertialFrame.Axes);
        }

        private void Scale()
        {
        }

        private void Rotate()
        {
        }

        private void EnableStepButtons()
        {
            m_stepForwardButton.SetEnabled(true);
            m_stepReverseButton.SetEnabled(true);
        }

        private void DisableStepButtons()
        {
            m_stepForwardButton.SetEnabled(false);
            m_stepReverseButton.SetEnabled(false);
        }

        private void EnableIncreaseDeltaButton()
        {
            m_increaseDeltaButton.SetEnabled(true);
        }

        private void DisableIncreaseDeltaButton()
        {
            m_increaseDeltaButton.SetEnabled(false);
        }

        private void EnableDecreaseDeltaButton()
        {
            m_decreaseDeltaButton.SetEnabled(true);
        }

        private void DisableDecreaseDeltaButton()
        {
            m_decreaseDeltaButton.SetEnabled(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~OverlayToolbar()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_overlay != null)
                {
                    m_overlay.Dispose();
                }
                if (m_rotateButton != null)
                {
                    m_rotateButton.Dispose();
                }
                if (m_scaleButton != null)
                {
                    m_scaleButton.Dispose();
                }
                if (m_zoomButton != null)
                {
                    m_zoomButton.Dispose();
                }
                if (m_stepForwardButton != null)
                {
                    m_stepForwardButton.Dispose();
                }
                if (m_stepReverseButton != null)
                {
                    m_stepReverseButton.Dispose();
                }
            }
        }

        /// <summary>
        /// Holds an individual button in the toolbar. Each button is a separate screen overlay, 
        /// rendered within the overlay containing the toolbar.
        /// </summary>
        public class OverlayButtonHolder : IDisposable
        {
            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            public OverlayButtonHolder(Insight3D insight3D, Action action, string image, int xOffset, double panelWidth)
                : this(insight3D, action, image, image, xOffset, panelWidth)
            {
            }

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            public OverlayButtonHolder(Insight3D insight3D, Action action, string enabledImage, string disabledImage, int xOffset, double panelWidth)
            {
                m_insight3D = insight3D;
                m_action = action;
                m_enabled = true;
                m_enabledImage = enabledImage;
                m_disabledImage = disabledImage;
                m_xOffset = xOffset;

                m_overlay = new TextureScreenOverlay
                {
                    X = xOffset / panelWidth,
                    XUnit = ScreenOverlayUnit.Fraction,
                    Width = ButtonSize / panelWidth,
                    WidthUnit = ScreenOverlayUnit.Fraction,
                    Height = 1.0,
                    HeightUnit = ScreenOverlayUnit.Fraction,
                    Translucency = MouseExitTranslucency,
                    Texture = SceneManager.Textures.FromUri(disabledImage)
                };
            }

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            public OverlayButtonHolder(Insight3D insight3D, Action action, string image, int xOffset, double panelWidth, double scale, double rotate)
                : this(insight3D, action, image, xOffset, panelWidth)
            {
                m_overlay.Scale = scale;
                m_overlay.RotationAngle = rotate;
            }

            /// <summary>
            /// Gets the screen overlay for this button.
            /// </summary>
            public ScreenOverlay Overlay
            {
                get { return m_overlay; }
            }

            /// <summary>
            /// Sets the on/off state of this button.
            /// </summary>
            public void SetState(bool state)
            {
                m_state = state;
                m_overlay.Texture = SceneManager.Textures.FromUri(state ? m_enabledImage : m_disabledImage);
            }

            /// <summary>
            /// Enables or disables this button.
            /// </summary>
            public void SetEnabled(bool enabled)
            {
                m_enabled = enabled;
                Overlay.Color = enabled ? Color.White : Color.Gray;
            }

            /// <summary>
            /// Resize this button given the overall toolbar width.
            /// </summary>
            public void Resize(double toolbarWidth)
            {
                m_overlay.Size = new ScreenOverlaySize(ButtonSize / toolbarWidth, 1.0, ScreenOverlayUnit.Fraction, ScreenOverlayUnit.Fraction);
                m_overlay.X = m_xOffset / toolbarWidth;
                m_overlay.XUnit = ScreenOverlayUnit.Fraction;
            }

            /// <summary>
            /// Called when the mouse enters this button.
            /// </summary>
            public void OnMouseEnter()
            {
                if (m_enabled)
                {
                    Overlay.Translucency = MouseEnterTranslucency;
                    if (SceneManager.Animation.IsAnimating == false)
                    {
                        m_insight3D.Scene.Render();
                    }
                }
            }

            /// <summary>
            /// Called when the mouse leaves this button.
            /// </summary>
            public void OnMouseLeave()
            {
                if (m_enabled)
                {
                    Overlay.Translucency = MouseExitTranslucency;
                    Overlay.Color = Color.White;
                    if (SceneManager.Animation.IsAnimating == false)
                    {
                        m_insight3D.Scene.Render();
                    }
                }
            }

            /// <summary>
            /// Called when the mouse is pressed on this button.
            /// </summary>
            public void OnMouseDown()
            {
                if (m_enabled)
                {
                    Overlay.Color = Color.DarkGray;
                    if (SceneManager.Animation.IsAnimating == false)
                    {
                        m_insight3D.Scene.Render();
                    }
                }
            }

            /// <summary>
            /// Called when the mouse is released on this button.
            /// </summary>
            public void OnMouseUp()
            {
                if (m_enabled)
                {
                    Overlay.Color = Color.White;
                    if (SceneManager.Animation.IsAnimating == false)
                    {
                        m_insight3D.Scene.Render();
                    }
                }
            }

            /// <summary>
            /// Called when the mouse is clicked on this button.
            /// </summary>
            public void OnMouseClick()
            {
                if (m_enabled)
                {
                    SetState(!m_state);

                    if (m_action != null)
                    {
                        m_action();
                    }
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            ~OverlayButtonHolder()
            {
                Dispose(false);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (m_overlay != null)
                    {
                        m_overlay.Dispose();
                    }
                }
            }

            private const float MouseEnterTranslucency = 0.01f;
            private const float MouseExitTranslucency = 0.25f;

            private readonly Insight3D m_insight3D;
            private readonly Action m_action;
            private readonly string m_enabledImage;
            private readonly string m_disabledImage;
            private readonly int m_xOffset;
            private readonly TextureScreenOverlay m_overlay;

            private bool m_state;
            private bool m_enabled;
        }

        private const float PanelTranslucencyRegular = 0.95f;
        private const float PanelTranslucencyPicked = 0.85f;
        private const float PanelTranslucencyClicked = 0.8f;
        private const float PanelBorderTranslucencyRegular = 0.6f;
        private const float PanelBorderTranslucencyPicked = 0.5f;
        private const float PanelBorderTranslucencyClicked = 0.4f;
        private const int ButtonSize = 35;
        private const int DefaultPanelWidth = (int)(ButtonSize * 0.5);

        private readonly Insight3D m_insight3D;
        private readonly List<OverlayButtonHolder> m_buttonHolders;

        private int m_locationOffset;

        private readonly TextureScreenOverlay m_overlay;

        private readonly OverlayButtonHolder m_rotateButton;
        private readonly OverlayButtonHolder m_scaleButton;
        private readonly OverlayButtonHolder m_zoomButton;
        private readonly OverlayButtonHolder m_stepForwardButton;
        private readonly OverlayButtonHolder m_stepReverseButton;
        private readonly OverlayButtonHolder m_increaseDeltaButton;
        private readonly OverlayButtonHolder m_decreaseDeltaButton;

        private Point m_anchorPoint = Point.Empty;
        private Point m_rotatePoint = Point.Empty;
        private Point m_scalePoint = Point.Empty;
        private Point m_baseAnchorPoint = Point.Empty;

        private BoundingRectangle m_scaleBounds;
        private double m_startScale;
        private double m_scaleOffset;

        private bool m_panelTranslucencyChanged;
        private bool m_panelCurrentlyPicked;
        private bool m_visible = true;
        private bool m_transforming;

        private OverlayButtonHolder m_mouseOverButton;
        private bool m_mouseDown;
    }
}