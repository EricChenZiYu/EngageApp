using System.Windows;

namespace EngageApp.Services.Interfaces
{
    public interface IScreenPositionService
    {
        /// <summary>
        /// Positions a window at the top-right corner of the current screen
        /// </summary>
        /// <param name="window">The window to position</param>
        void PositionWindowTopRight(Window window);
        
        /// <summary>
        /// Gets the working area of the screen containing the window
        /// </summary>
        /// <param name="window">The window</param>
        /// <returns>A rectangle representing the working area</returns>
        Rect GetCurrentScreenWorkingArea(Window window);
    }
} 