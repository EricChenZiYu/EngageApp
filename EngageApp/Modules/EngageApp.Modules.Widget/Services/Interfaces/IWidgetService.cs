using System;

namespace EngageApp.Modules.Widget.Services.Interfaces
{
    /// <summary>
    /// Interface for widget service
    /// </summary>
    public interface IWidgetService
    {
        /// <summary>
        /// Show the widget
        /// </summary>
        void ShowWidget();
        
        /// <summary>
        /// Hide the widget
        /// </summary>
        void HideWidget();
        
        /// <summary>
        /// Collapses the widget without hiding it
        /// </summary>
        void CollapseWidget();
        
        /// <summary>
        /// Gets a value indicating whether the widget is visible
        /// </summary>
        bool IsWidgetVisible { get; }
        
        /// <summary>
        /// Event raised when the widget is clicked
        /// </summary>
        event EventHandler WidgetClicked;
        
        /// <summary>
        /// Event raised when the widget is double-clicked
        /// </summary>
        event EventHandler WidgetDoubleClicked;
        
        /// <summary>
        /// Customize the widget appearance
        /// </summary>
        /// <param name="backgroundColor">Background color (hex format: #RRGGBB)</param>
        /// <param name="text">Text to display</param>
        /// <param name="textColor">Text color (hex format: #RRGGBB)</param>
        /// <param name="glowColor">Glow color (hex format: #RRGGBB)</param>
        void CustomizeWidget(string backgroundColor = "#4070FF", string text = "W", string textColor = "#FFFFFF", string glowColor = "#4070FF");
    }
} 