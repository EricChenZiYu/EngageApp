using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using EngageApp.Modules.Widget.Services.Interfaces;
using EngageApp.Modules.Widget.ViewModels;
using System.Runtime.InteropServices;

namespace EngageApp.Modules.Widget.Views
{
    /// <summary>
    /// Interaction logic for WidgetView.xaml
    /// </summary>
    public partial class WidgetView : Window
    {
        private readonly IWidgetScreenService _screenService;
        private readonly IWidgetLoggerService _logger;
        private bool _isDragging;
        private Point _dragStartPoint;
        private readonly DispatcherTimer _idleTimer;
        private readonly DispatcherTimer _cornerHoverTimer;
        private readonly DispatcherTimer _cornerCheckTimer;
        private const int COLLAPSE_AFTER_SECONDS = 3;
        private const int CORNER_HOVER_SECONDS = 1;
        private bool _isManuallyExpanded = false;
        private double _storedTopPosition;
        
        /// <summary>
        /// Occurs when the widget is clicked.
        /// </summary>
        public event EventHandler WidgetClicked;
        
        /// <summary>
        /// Occurs when the widget is double-clicked.
        /// </summary>
        public event EventHandler WidgetDoubleClicked;

        /// <summary>
        /// Initializes a new instance of the <see cref="WidgetView"/> class.
        /// </summary>
        /// <param name="screenService">The screen service</param>
        /// <param name="logger">The logger service</param>
        public WidgetView(IWidgetScreenService screenService, IWidgetLoggerService logger)
        {
            InitializeComponent();
            
            _screenService = screenService;
            _logger = logger;
            
            // Hide the widget initially
            Visibility = Visibility.Hidden;
            
            // Initialize idle timer for auto-collapse
            _idleTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(COLLAPSE_AFTER_SECONDS)
            };
            _idleTimer.Tick += (s, e) => CollapseWidget();
            
            // Initialize corner hover detection timer
            _cornerHoverTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(CORNER_HOVER_SECONDS)
            };
            _cornerHoverTimer.Tick += (s, e) => 
            {
                try
                {
                    if (IsMouseInCorner())
                    {
                        ExpandWidget();
                        _cornerHoverTimer.Stop();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Error in corner hover timer", ex);
                }
            };
            
            // Initialize corner check timer for continuous checking
            _cornerCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _cornerCheckTimer.Tick += (s, e) => CheckForCornerHover();
            
            // Set window style to prevent stealing focus
            this.SourceInitialized += (s, e) => SetNoActivateStyle();
        }
        
        /// <summary>
        /// Sets the window style to prevent it from stealing focus
        /// </summary>
        private void SetNoActivateStyle()
        {
            try
            {
                // Create WindowInteropHelper
                var helper = new WindowInteropHelper(this);
                
                // Get the handle
                var hwnd = helper.Handle;
                
                // Get current style
                var extendedStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
                
                // Add WS_EX_NOACTIVATE style to prevent stealing focus
                NativeMethods.SetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE, extendedStyle | NativeMethods.WS_EX_NOACTIVATE);
                
                _logger.Debug("Set widget window to non-activating style");
            }
            catch (Exception ex)
            {
                _logger.Error("Error setting window style", ex);
            }
        }
        
        /// <summary>
        /// Shows the widget.
        /// </summary>
        public void ShowWidget()
        {
            try
            {
                _logger.Debug("ShowWidget called - positioning widget and setting visible");
                
                // First make sure the window is created and visible (but hidden) to get a handle
                Show();
                Visibility = Visibility.Hidden;
                
                // Force layout update to ensure all measurements are available
                UpdateLayout();
                
                // Position horizontally (right side)
                _screenService.PositionWindowTopRight(this);
                
                // Now position at absolute top edge using Win32 API for accuracy
                // This single positioning should be sufficient
                PositionAtAbsoluteTopEdge();
                
                // Make visible
                Visibility = Visibility.Visible;
                Topmost = true;
                
                _logger.Debug($"Widget positioned at: Left={Left}, Top={Top}");
                
                // Force layout update
                UpdateLayout();
                
                // Show in collapsed state initially
                CollapseWidget(immediate: true);
                
                // Single delayed positioning attempt (instead of multiple) to avoid shaking
                ScheduleDelayedPositioning();
                
                // Start corner check timer
                _cornerCheckTimer.Start();
            }
            catch (Exception ex)
            {
                _logger.Error("Error showing widget", ex);
            }
        }
        
        /// <summary>
        /// Schedules a single delayed positioning attempt
        /// </summary>
        private void ScheduleDelayedPositioning()
        {
            // Store the initial position
            double initialTop = Top;
            
            // Use DispatcherTimer which naturally runs on the UI thread
            var delayedTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            
            delayedTimer.Tick += DelayedPositioning_Tick;
            delayedTimer.Start();
        }
        
        // Separate handler for the timer tick to avoid reflection issues
        private void DelayedPositioning_Tick(object sender, EventArgs e)
        {
            try
            {
                // Get the timer that called this method
                if (sender is DispatcherTimer timer)
                {
                    timer.Stop();
                    timer.Tick -= DelayedPositioning_Tick;
                }
                
                // Only reposition if needed
                double initialTop = _storedTopPosition;
                if (Math.Abs(Top - initialTop) > 1.0)
                {
                    PositionAtAbsoluteTopEdge();
                    _logger.Debug($"Reapplied positioning after delay, position updated to: {Top}");
                }
                else
                {
                    _logger.Debug($"Position stable after delay, maintaining at: {Top}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in delayed positioning", ex);
            }
        }
        
        /// <summary>
        /// Positions the widget at the absolute top edge of the screen using Win32 API
        /// </summary>
        public void PositionAtAbsoluteTopEdge()
        {
            try
            {
                // Get the window handle
                var helper = new WindowInteropHelper(this);
                var hwnd = helper.Handle;
                
                if (hwnd == IntPtr.Zero)
                {
                    _logger.Warning("Window handle is zero, cannot position at absolute top edge");
                    return;
                }
                
                // Force the window to have ToolWindow style (smaller title bar) and TOPMOST
                int exStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
                exStyle |= NativeMethods.WS_EX_TOOLWINDOW | NativeMethods.WS_EX_TOPMOST | NativeMethods.WS_EX_NOACTIVATE;
                NativeMethods.SetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE, exStyle);
                
                // Get screen information
                System.Windows.Forms.Screen currentScreen = System.Windows.Forms.Screen.FromHandle(hwnd);
                
                // Get current window position in screen coordinates
                NativeMethods.GetWindowRect(hwnd, out var windowRect);
                
                // Get DPI information
                var source = PresentationSource.FromVisual(this);
                if (source?.CompositionTarget != null)
                {
                    var m = source.CompositionTarget.TransformToDevice;
                    double dpiX = m.M11;
                    double dpiY = m.M22;
                    
                    _logger.Debug($"Setting absolute top position with DPI factors: X={dpiX}, Y={dpiY}");
                    
                    // Calculate left position in screen coordinates (maintain current horizontal position)
                    int screenLeft = windowRect.Left;
                    int screenTop = currentScreen.Bounds.Top; // Absolute top edge
                    
                    _logger.Debug($"Positioning window at absolute top: " +
                                 $"Screen coordinates: Left={screenLeft}, Top={screenTop}, " +
                                 $"Screen bounds: Left={currentScreen.Bounds.Left}, Top={currentScreen.Bounds.Top}, " +
                                 $"DPI: X={dpiX}, Y={dpiY}");
                    
                    // Direct Win32 API positioning at absolute top
                    NativeMethods.SetWindowPos(
                        hwnd,
                        NativeMethods.HWND_TOPMOST_PTR,
                        screenLeft,
                        screenTop,
                        0, 0,
                        NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW
                    );
                    
                    // Update WPF properties to reflect actual position (needed for animations to work correctly)
                    var newTop = screenTop / dpiY;
                    if (Math.Abs(Top - newTop) > 0.5) // Only update if there's a meaningful difference
                    {
                        Top = newTop;
                        _logger.Debug($"Updated WPF Top property to {Top}");
                    }
                    
                    _logger.Debug($"Set widget to absolute top with Win32 API: " +
                                 $"Left={Left}, Top={Top}, " +
                                 $"Native position: X={screenLeft}, Y={screenTop}");
                }
                else
                {
                    // Fallback for direct positioning without DPI info
                    NativeMethods.SetWindowPos(
                        hwnd,
                        NativeMethods.HWND_TOPMOST_PTR,
                        windowRect.Left,
                        currentScreen.Bounds.Top,
                        0, 0,
                        NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW
                    );
                    
                    // Update WPF property for consistency, but may not be accurate without DPI info
                    Top = 0;
                    _logger.Warning("Positioned at top edge without DPI information");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error positioning at absolute top edge", ex);
                Top = -5; // Fallback
            }
        }
        
        /// <summary>
        /// Adjusts the top position accounting for DPI scaling factors
        /// </summary>
        private void AdjustTopPositionForDpi()
        {
            try
            {
                // Use Win32 API for accurate positioning
                PositionAtAbsoluteTopEdge();
            }
            catch (Exception ex)
            {
                _logger.Error("Error adjusting top position for DPI", ex);
                // Fallback in case of error
                Top = -5;
            }
        }
        
        /// <summary>
        /// Hides the widget.
        /// </summary>
        public void HideWidget()
        {
            Visibility = Visibility.Hidden;
            _idleTimer.Stop();
            _cornerHoverTimer.Stop();
            _cornerCheckTimer.Stop();
            _logger.Debug("Widget hidden");
        }
        
        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            _idleTimer.Stop();
            ExpandWidget();
        }
        
        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!_isDragging && !_isManuallyExpanded)
            {
                _idleTimer.Start();
            }
        }
        
        private void WidgetBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                _isDragging = true;
                _dragStartPoint = e.GetPosition(this);
                CaptureMouse();
                
                // Attach mouse move and mouse up handlers
                MouseMove += Widget_MouseMove;
                MouseLeftButtonUp += Widget_MouseLeftButtonUp;
                
                // Raise clicked event
                WidgetClicked?.Invoke(this, EventArgs.Empty);
                
                // Keep expanded while dragging
                ExpandWidget();
                _idleTimer.Stop();
                _isManuallyExpanded = true;
                
                // We don't reposition the widget on click anymore to prevent shaking
                // The widget should already be correctly positioned
            }
            else if (e.ClickCount == 2)
            {
                // Double-click to restore the main window
                WidgetDoubleClicked?.Invoke(this, EventArgs.Empty);
            }
        }
        
        private void Widget_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                // Get current mouse position
                Point currentPosition = e.GetPosition(this);
                
                // Calculate drag offset
                Vector dragOffset = currentPosition - _dragStartPoint;
                
                // Move the widget with smooth direct updating (without triggering layout recalculation)
                Left += dragOffset.X;
                Top += dragOffset.Y;
                
                // Only snap to edges if the drag distance is significant
                // This prevents small movements from causing snapping and shaking
                if (dragOffset.Length > 5.0)
                {
                    // Snap to edges with a larger snap distance to prevent oscillation
                    _screenService.SnapToNearestEdge(this, snapDistance: 15);
                }
                
                // Update drag start point to current position to prepare for next move
                _dragStartPoint = currentPosition;
            }
        }
        
        private void Widget_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            ReleaseMouseCapture();
            
            // Remove handlers
            MouseMove -= Widget_MouseMove;
            MouseLeftButtonUp -= Widget_MouseLeftButtonUp;
            
            // Reset manual expansion state
            _isManuallyExpanded = false;
            
            // Start idle timer
            _idleTimer.Start();
        }
        
        /// <summary>
        /// Collapses the widget from an external call
        /// </summary>
        public void CollapseWidgetExternally()
        {
            try
            {
                _logger.Debug("CollapseWidgetExternally called");
                CollapseWidget(immediate: false);
            }
            catch (Exception ex)
            {
                _logger.Error("Error in CollapseWidgetExternally", ex);
            }
        }
        
        private void ExpandWidget()
        {
            try
            {
                // Store current position before expansion to prevent shaking
                _storedTopPosition = Top;
                
                // Make sure widget always stays at top edge but still visible
                if (WidgetBorder != null)
                {
                    // When expanding, keep the top edge flat but rounded corners at bottom
                    WidgetBorder.CornerRadius = new System.Windows.CornerRadius(0, 0, 5, 5);
                    
                    // Adjust the shadow for expanded state
                    if (WidgetBorder.Effect is System.Windows.Media.Effects.DropShadowEffect effect)
                    {
                        effect.Direction = 270; // Shadow goes down from top edge
                        effect.ShadowDepth = 2;
                    }
                }
                
                // Start the animation
                var storyboard = FindResource("ExpandStoryboard") as Storyboard;
                if (storyboard != null)
                {
                    // Important: Make sure to use the EXACT delegate signature
                    storyboard.Completed -= Storyboard_ExpandCompleted; // Remove any previous handlers
                    storyboard.Completed += Storyboard_ExpandCompleted; // Add the handler
                    storyboard.Begin(this);
                }
                
                _logger.Debug("Widget expanded");
            }
            catch (Exception ex)
            {
                _logger.Error("Error expanding widget", ex);
            }
        }
        
        // IMPORTANT: The exact signature for the Storyboard.Completed event
        private void Storyboard_ExpandCompleted(object sender, EventArgs e)
        {
            try
            {
                // Restore original top position to prevent movement
                if (Math.Abs(Top - _storedTopPosition) > 0.1)
                {
                    Top = _storedTopPosition;
                }
                _logger.Debug($"Widget expand animation completed, kept position at: {Top}");
                
                // Remove the handler to prevent memory leaks
                if (sender is Storyboard storyboard)
                {
                    storyboard.Completed -= Storyboard_ExpandCompleted;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in expand animation completed handler", ex);
            }
        }
        
        private void CollapseWidget(bool immediate = false)
        {
            try
            {
                // Store current position to maintain stability
                _storedTopPosition = Top;
                
                if (immediate)
                {
                    // Set collapsed state immediately without animation
                    if (WidgetBorder != null)
                    {
                        WidgetBorder.Width = 15;
                        WidgetBorder.Height = 15;
                        if (WidgetBorder.Effect is System.Windows.Media.Effects.DropShadowEffect effect)
                        {
                            effect.Opacity = 0.15;
                            effect.Direction = 270; // Shadow goes down from top edge
                            effect.ShadowDepth = 1.5;
                            effect.BlurRadius = 5;
                        }
                        
                        // Ensure rounded corners only at bottom for top placement
                        WidgetBorder.CornerRadius = new System.Windows.CornerRadius(0, 0, 5, 5);
                    }
                    
                    // Keep the same top position to prevent movement/shaking
                    if (Math.Abs(Top - _storedTopPosition) > 0.1)
                    {
                        Top = _storedTopPosition;
                    }
                    
                    _logger.Debug($"Widget collapsed immediately, maintained position at: {Top}");
                }
                else
                {
                    var storyboard = FindResource("CollapseStoryboard") as Storyboard;
                    
                    // Ensure position stability during animation
                    if (storyboard != null)
                    {
                        // Important: Make sure to use the EXACT delegate signature
                        storyboard.Completed -= Storyboard_CollapseCompleted; // Remove any previous handlers
                        storyboard.Completed += Storyboard_CollapseCompleted; // Add the handler
                        storyboard.Begin(this);
                    }
                    
                    _logger.Debug("Widget collapsed with animation");
                }
                
                _idleTimer.Stop();
            }
            catch (Exception ex)
            {
                _logger.Error("Error collapsing widget", ex);
            }
        }
        
        // IMPORTANT: The exact signature for the Storyboard.Completed event
        private void Storyboard_CollapseCompleted(object sender, EventArgs e)
        {
            try
            {
                // Restore original position to prevent movement
                if (Math.Abs(Top - _storedTopPosition) > 0.1)
                {
                    Top = _storedTopPosition;
                }
                _logger.Debug($"Widget collapse animation completed, maintained position at: {Top}");
                
                // Remove the handler to prevent memory leaks
                if (sender is Storyboard storyboard)
                {
                    storyboard.Completed -= Storyboard_CollapseCompleted;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in collapse animation completed handler", ex);
            }
        }
        
        private bool IsMouseInCorner()
        {
            try
            {
                // Get current screen working area
                var workingArea = _screenService.GetScreenWorkingArea(this);
                
                // Get current mouse position
                var mousePos = System.Windows.Forms.Control.MousePosition;
                
                // Get DPI information for accurate corner detection
                var source = PresentationSource.FromVisual(this);
                if (source?.CompositionTarget != null)
                {
                    var m = source.CompositionTarget.TransformToDevice;
                    double dpiX = m.M11;
                    double dpiY = m.M22;
                    
                    // Convert screen coordinates to DPI-aware WPF coordinates
                    double scaledMouseX = mousePos.X / dpiX;
                    double scaledMouseY = mousePos.Y / dpiY;
                    
                    // Increase detection area slightly with DPI awareness
                    double cornerWidth = 50 / dpiX;  // Adjust corner width by DPI factor
                    double cornerHeight = 25 / dpiY; // Adjust corner height by DPI factor
                    
                    // Check if mouse is in top-right corner region with DPI-adjusted values
                    bool inCorner = (scaledMouseX >= workingArea.Right - cornerWidth) && 
                                    (scaledMouseY <= workingArea.Top + cornerHeight);
                    
                    if (inCorner)
                    {
                        _logger.Debug($"Mouse detected in corner (DPI adjusted): " + 
                                       $"Mouse: ({scaledMouseX},{scaledMouseY}), " + 
                                       $"Corner: Right={workingArea.Right}, Top={workingArea.Top}, " + 
                                       $"DPI: ({dpiX},{dpiY})");
                    }
                    
                    return inCorner;
                }
                else
                {
                    // Fallback to non-DPI aware detection
                    bool inCorner = (mousePos.X >= workingArea.Right - 50) && (mousePos.Y <= workingArea.Top + 25);
                    
                    if (inCorner)
                    {
                        _logger.Debug($"Mouse detected in corner (non-DPI): X={mousePos.X}, Y={mousePos.Y}");
                    }
                    
                    return inCorner;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error checking if mouse is in corner", ex);
                return false;
            }
        }
        
        private void CheckForCornerHover()
        {
            try
            {
                if (Visibility == Visibility.Visible && !_isManuallyExpanded && !_isDragging)
                {
                    bool inCorner = IsMouseInCorner();
                    
                    if (inCorner)
                    {
                        if (!_cornerHoverTimer.IsEnabled)
                        {
                            _logger.Debug("Mouse in corner, starting hover timer");
                            _cornerHoverTimer.Start();
                        }
                    }
                    else
                    {
                        if (_cornerHoverTimer.IsEnabled)
                        {
                            _cornerHoverTimer.Stop();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error checking for corner hover", ex);
            }
        }
    }
    
    /// <summary>
    /// Native methods for window manipulation
    /// </summary>
    internal static class NativeMethods
    {
        // Extended window styles
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_NOACTIVATE = 0x08000000;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_TOPMOST = 0x00000008;
        
        // Window Z-order and positioning flags
        public const int HWND_TOPMOST = -1;
        public const int HWND_TOP = 0;
        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOACTIVATE = 0x0010;
        public const int SWP_FRAMECHANGED = 0x0020;
        public const int SWP_SHOWWINDOW = 0x0040;
        public const int SWP_NOOWNERZORDER = 0x0200;
        
        // Window positioning constants
        public static readonly IntPtr HWND_TOPMOST_PTR = new IntPtr(HWND_TOPMOST);
        
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);
        
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
        
        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);
        
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        
        // For screen coordinate conversion
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            
            public int Width { get { return Right - Left; } }
            public int Height { get { return Bottom - Top; } }
        }
    }
} 