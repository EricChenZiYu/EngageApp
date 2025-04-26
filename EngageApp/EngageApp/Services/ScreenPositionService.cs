using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using EngageApp.Services.Interfaces;
using Point = System.Drawing.Point;

namespace EngageApp.Services
{
    public class ScreenPositionService : IScreenPositionService
    {
        public void PositionWindowTopRight(Window window)
        {
            try
            {
                var screen = GetCurrentScreen();
                const int margin = 10;
                
                // Get DPI scaling factors
                var source = PresentationSource.FromVisual(window);
                if (source != null)
                {
                    Matrix m = source.CompositionTarget.TransformToDevice;
                    double dpiX = m.M11;
                    double dpiY = m.M22;
                    
                    Console.WriteLine($"DPI scale: X={dpiX}, Y={dpiY}");
                    
                    // Calculate the correct position with DPI adjustments
                    double screenWidth = screen.WorkingArea.Width / dpiX;
                    double screenHeight = screen.WorkingArea.Height / dpiY;
                    
                    double left = screenWidth - window.Width - margin;
                    double top = margin;
                    
                    Console.WriteLine($"Positioning window at: Left={left}, Top={top} | " +
                                     $"Screen (DPI adjusted): Width={screenWidth}, Height={screenHeight}");
                    
                    // Set the position
                    window.Left = left;
                    window.Top = top;
                }
                else
                {
                    // Fallback positioning without DPI adjustment
                    window.Left = 100;
                    window.Top = 100;
                }
                
                // Ensure visibility and activation
                window.Topmost = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error positioning window: {ex.Message}");
                
                // Fallback position
                window.Left = 100;
                window.Top = 100;
            }
        }
        
        public Rect GetCurrentScreenWorkingArea(Window window)
        {
            try
            {
                var screen = GetCurrentScreen();
                
                // Get DPI scaling factors
                var source = PresentationSource.FromVisual(window);
                if (source != null)
                {
                    Matrix m = source.CompositionTarget.TransformToDevice;
                    double dpiX = m.M11;
                    double dpiY = m.M22;
                    
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
                Console.WriteLine($"Error getting screen area: {ex.Message}");
            }
            
            // Fallback to a reasonable default
            return new Rect(0, 0, 1024, 768);
        }
        
        private Screen GetCurrentScreen()
        {
            // Default to primary screen
            var screen = Screen.PrimaryScreen;
            
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
            
            return screen;
        }
    }
} 