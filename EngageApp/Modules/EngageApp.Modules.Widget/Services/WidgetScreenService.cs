using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using EngageApp.Modules.Widget.Services.Interfaces;

namespace EngageApp.Modules.Widget.Services
{
    /// <summary>
    /// Implementation of the widget screen positioning service
    /// </summary>
    public class WidgetScreenService : IWidgetScreenService
    {
        private readonly IWidgetLoggerService _logger;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WidgetScreenService"/> class.
        /// </summary>
        /// <param name="logger">The widget logger service</param>
        public WidgetScreenService(IWidgetLoggerService logger)
        {
            _logger = logger;
        }
        
        /// <inheritdoc/>
        public void PositionWindowTopRight(Window window)
        {
            try
            {
                var screen = GetCurrentScreen();
                const int margin = 20; // increased margin to ensure visibility
                
                // Get DPI scaling factors
                var dpiInfo = GetDpiFactors(window);
                
                if (dpiInfo.HasValue)
                {
                    var (dpiX, dpiY) = dpiInfo.Value;
                    _logger.Debug($"DPI scale: X={dpiX}, Y={dpiY}");
                    
                    // Better calculation for screen working area
                    double screenLeft = screen.WorkingArea.Left / dpiX;
                    double screenTop = screen.WorkingArea.Top / dpiY;
                    double screenWidth = screen.WorkingArea.Width / dpiX;
                    double screenHeight = screen.WorkingArea.Height / dpiY;
                    
                    double left = screenLeft + screenWidth - window.ActualWidth - margin;
                    double top = screenTop + margin;
                    
                    _logger.Debug($"Positioning window at: Left={left}, Top={top} | " +
                                 $"Screen (DPI adjusted): Left={screenLeft}, Top={screenTop}, Width={screenWidth}, Height={screenHeight}");
                    
                    if (left < 0) left = 0;
                    if (top < 0) top = 0;
                    
                    // Set the position
                    window.Left = left;
                    window.Top = top;
                    
                    // Ensure we're still on screen after setting position
                    if (window.Left + window.ActualWidth > screenLeft + screenWidth)
                    {
                        window.Left = screenLeft + screenWidth - window.ActualWidth - 5;
                    }
                    
                    if (window.Top + window.ActualHeight > screenTop + screenHeight)
                    {
                        window.Top = screenTop + screenHeight - window.ActualHeight - 5;
                    }
                }
                else
                {
                    // Fallback positioning without DPI adjustment
                    window.Left = 100;
                    window.Top = 100;
                    _logger.Warning("Failed to get DPI information, using fallback position (100,100)");
                }
                
                // Ensure visibility and activation
                window.Topmost = true;
            }
            catch (Exception ex)
            {
                _logger.Error("Error positioning window at top-right", ex);
                
                // Fallback position
                window.Left = 100;
                window.Top = 100;
            }
        }
        
        /// <inheritdoc/>
        public void PositionWindowCenter(Window window)
        {
            try
            {
                var screen = GetCurrentScreen();
                
                // Get DPI scaling factors
                var dpiInfo = GetDpiFactors(window);
                
                if (dpiInfo.HasValue)
                {
                    var (dpiX, dpiY) = dpiInfo.Value;
                    
                    // Calculate the correct position with DPI adjustments
                    double screenWidth = screen.WorkingArea.Width / dpiX;
                    double screenHeight = screen.WorkingArea.Height / dpiY;
                    
                    double left = (screenWidth - window.Width) / 2;
                    double top = (screenHeight - window.Height) / 2;
                    
                    _logger.Debug($"Positioning window at center: Left={left}, Top={top}");
                    
                    // Set the position
                    window.Left = left;
                    window.Top = top;
                }
                else
                {
                    // Fallback positioning without DPI adjustment
                    window.Left = 100;
                    window.Top = 100;
                    _logger.Warning("Failed to get DPI information, using fallback position (100,100)");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error positioning window at center", ex);
                
                // Fallback position
                window.Left = 100;
                window.Top = 100;
            }
        }
        
        /// <inheritdoc/>
        public Rect GetScreenWorkingArea(Window window)
        {
            try
            {
                var screen = GetCurrentScreen();
                
                // Get DPI scaling factors
                var dpiInfo = GetDpiFactors(window);
                
                if (dpiInfo.HasValue)
                {
                    var (dpiX, dpiY) = dpiInfo.Value;
                    
                    // Return DPI-adjusted screen working area
                    return new Rect(
                        screen.WorkingArea.Left / dpiX,
                        screen.WorkingArea.Top / dpiY,
                        screen.WorkingArea.Width / dpiX,
                        screen.WorkingArea.Height / dpiY);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting screen area", ex);
            }
            
            // Fallback to a reasonable default
            return new Rect(0, 0, 1024, 768);
        }
        
        /// <inheritdoc/>
        public bool SnapToNearestEdge(Window window, double snapDistance = 20)
        {
            try
            {
                var workingArea = GetScreenWorkingArea(window);
                bool snapped = false;
                
                // Snap to right edge
                if (Math.Abs(window.Left + window.Width - workingArea.Right) < snapDistance)
                {
                    window.Left = workingArea.Right - window.Width;
                    snapped = true;
                }
                
                // Snap to left edge
                if (Math.Abs(window.Left - workingArea.Left) < snapDistance)
                {
                    window.Left = workingArea.Left;
                    snapped = true;
                }
                
                // Snap to top edge
                if (Math.Abs(window.Top - workingArea.Top) < snapDistance)
                {
                    window.Top = workingArea.Top;
                    snapped = true;
                }
                
                // Snap to bottom edge
                if (Math.Abs(window.Top + window.Height - workingArea.Bottom) < snapDistance)
                {
                    window.Top = workingArea.Bottom - window.Height;
                    snapped = true;
                }
                
                return snapped;
            }
            catch (Exception ex)
            {
                _logger.Error("Error snapping window to edge", ex);
                return false;
            }
        }
        
        private Screen GetCurrentScreen()
        {
            // Default to primary screen
            var screen = Screen.PrimaryScreen;
            
            try
            {
                // Look for the screen that contains mouse cursor
                var mousePosition = Control.MousePosition;
                foreach (var currentScreen in Screen.AllScreens)
                {
                    if (currentScreen.Bounds.Contains(mousePosition))
                    {
                        screen = currentScreen;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting current screen", ex);
            }
            
            return screen;
        }
        
        private (double, double)? GetDpiFactors(Visual visual)
        {
            try
            {
                var source = PresentationSource.FromVisual(visual);
                if (source?.CompositionTarget != null)
                {
                    Matrix m = source.CompositionTarget.TransformToDevice;
                    return (m.M11, m.M22);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting DPI factors", ex);
            }
            
            return null;
        }
    }
} 