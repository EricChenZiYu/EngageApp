using System;
using System.Windows;
using EngageApp.Modules.Widget.Services.Interfaces;
using EngageApp.Modules.Widget.ViewModels;
using EngageApp.Modules.Widget.Views;

namespace EngageApp.Modules.Widget.Services
{
    /// <summary>
    /// Implementation of the widget service
    /// </summary>
    public class WidgetService : IWidgetService
    {
        private readonly IWidgetLoggerService _logger;
        private readonly IWidgetScreenService _screenService;
        private WidgetView _widgetView;
        private WidgetViewModel _widgetViewModel;
        
        /// <inheritdoc/>
        public event EventHandler WidgetClicked;
        
        /// <inheritdoc/>
        public event EventHandler WidgetDoubleClicked;
        
        /// <inheritdoc/>
        public bool IsWidgetVisible => _widgetView?.Visibility == Visibility.Visible;

        /// <summary>
        /// Initializes a new instance of the <see cref="WidgetService"/> class.
        /// </summary>
        /// <param name="logger">The logger service</param>
        /// <param name="screenService">The screen service</param>
        public WidgetService(IWidgetLoggerService logger, IWidgetScreenService screenService)
        {
            _logger = logger;
            _screenService = screenService;
            
            InitializeWidget();
        }
        
        /// <inheritdoc/>
        public void ShowWidget()
        {
            try
            {
                if (_widgetView == null)
                {
                    InitializeWidget();
                }
                
                // Make sure widget is created on UI thread
                Application.Current.Dispatcher.Invoke(() => {
                    _logger.Debug("ShowWidget called - displaying widget");
                    
                    // Ensure window is shown first
                    _widgetView.Show();
                    
                    // Force immediate position calculation
                    _screenService.PositionWindowTopRight(_widgetView);
                    
                    // Ensure widget is visible
                    _widgetView.Visibility = Visibility.Visible;
                    _widgetView.Topmost = true;
                    _widgetView.Activate();
                    
                    // Move to foreground explicitly
                    _widgetView.Focus();
                    
                    _logger.Debug($"Widget should now be visible at: Left={_widgetView.Left}, Top={_widgetView.Top}");
                });
            }
            catch (Exception ex)
            {
                _logger.Error("Error showing widget", ex);
            }
        }
        
        /// <inheritdoc/>
        public void HideWidget()
        {
            try
            {
                if (_widgetView != null)
                {
                    _widgetView.HideWidget();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error hiding widget", ex);
            }
        }
        
        /// <inheritdoc/>
        public void CustomizeWidget(string backgroundColor = "#4070FF", string text = "W", string textColor = "#FFFFFF", string glowColor = "#4070FF")
        {
            try
            {
                if (_widgetViewModel != null)
                {
                    _widgetViewModel.UpdateAppearance(backgroundColor, text, textColor, glowColor);
                    _logger.Debug($"Widget customized: BG={backgroundColor}, Text={text}, TextColor={textColor}, Glow={glowColor}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error customizing widget", ex);
            }
        }
        
        private void InitializeWidget()
        {
            try
            {
                _logger.Debug("Initializing widget");
                
                // Create view model
                _widgetViewModel = new WidgetViewModel();
                
                // Create widget view
                _widgetView = new WidgetView(_screenService, _logger);
                _widgetView.DataContext = _widgetViewModel;
                
                // Subscribe to widget events
                _widgetView.WidgetClicked += (s, e) => 
                {
                    _logger.Debug("Widget clicked");
                    WidgetClicked?.Invoke(this, e);
                };
                
                _widgetView.WidgetDoubleClicked += (s, e) => 
                {
                    _logger.Debug("Widget double clicked");
                    WidgetDoubleClicked?.Invoke(this, e);
                    HideWidget();
                };
                
                // Position widget off-screen initially
                _widgetView.Left = -1000;
                _widgetView.Top = -1000;
                _widgetView.Show();
                _widgetView.Hide();
                
                _logger.Debug("Widget initialized and hidden");
            }
            catch (Exception ex)
            {
                _logger.Error("Error initializing widget", ex);
            }
        }
    }
} 