using System.Windows;

namespace EngageApp.Modules.Widget.Services.Interfaces
{
    /// <summary>
    /// Interface for widget screen positioning service
    /// </summary>
    public interface IWidgetScreenService
    {
        /// <summary>
        /// Positions a window at the top-right corner of the current screen
        /// </summary>
        /// <param name="window">The window to position</param>
        void PositionWindowTopRight(Window window);
        
        /// <summary>
        /// Positions a window at the center of the current screen
        /// </summary>
        /// <param name="window">The window to position</param>
        void PositionWindowCenter(Window window);
        
        /// <summary>
        /// Gets the working area of the screen containing the window
        /// </summary>
        /// <param name="window">The window</param>
        /// <returns>A rectangle representing the working area</returns>
        Rect GetScreenWorkingArea(Window window);
        
        /// <summary>
        /// Snaps a window to the nearest screen edge if within the snap distance
        /// </summary>
        /// <param name="window">The window to snap</param>
        /// <param name="snapDistance">Distance in pixels to activate snapping</param>
        /// <returns>True if window was snapped, false otherwise</returns>
        bool SnapToNearestEdge(Window window, double snapDistance = 20);
    }
} 