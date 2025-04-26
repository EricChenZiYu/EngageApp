using System.Windows.Media;
using Prism.Mvvm;

namespace EngageApp.Modules.Widget.ViewModels
{
    /// <summary>
    /// ViewModel for the WidgetView
    /// </summary>
    public class WidgetViewModel : BindableBase
    {
        private string _widgetText = "W";
        private Brush _widgetBackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4070FF"));
        private Brush _widgetTextColor = Brushes.White;
        private Color _widgetGlowColor = (Color)ColorConverter.ConvertFromString("#4070FF");
        
        /// <summary>
        /// Gets or sets the widget text
        /// </summary>
        public string WidgetText
        {
            get => _widgetText;
            set => SetProperty(ref _widgetText, value);
        }
        
        /// <summary>
        /// Gets or sets the widget background color
        /// </summary>
        public Brush WidgetBackgroundColor
        {
            get => _widgetBackgroundColor;
            set => SetProperty(ref _widgetBackgroundColor, value);
        }
        
        /// <summary>
        /// Gets or sets the widget text color
        /// </summary>
        public Brush WidgetTextColor
        {
            get => _widgetTextColor;
            set => SetProperty(ref _widgetTextColor, value);
        }
        
        /// <summary>
        /// Gets or sets the widget glow color
        /// </summary>
        public Color WidgetGlowColor
        {
            get => _widgetGlowColor;
            set => SetProperty(ref _widgetGlowColor, value);
        }
        
        /// <summary>
        /// Updates the widget appearance
        /// </summary>
        /// <param name="backgroundColor">Background color (hex format: #RRGGBB)</param>
        /// <param name="text">Text to display</param>
        /// <param name="textColor">Text color (hex format: #RRGGBB)</param>
        /// <param name="glowColor">Glow color (hex format: #RRGGBB)</param>
        public void UpdateAppearance(string backgroundColor, string text, string textColor, string glowColor)
        {
            WidgetText = text;
            
            // Use color converter to create brushes from hex strings
            WidgetBackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundColor));
            WidgetTextColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(textColor));
            WidgetGlowColor = (Color)ColorConverter.ConvertFromString(glowColor);
        }
    }
} 