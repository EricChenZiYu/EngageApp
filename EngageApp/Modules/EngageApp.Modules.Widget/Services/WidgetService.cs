using System;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;
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
        private System.Threading.Timer _topMostTimer;
        
        // Win32 constants and methods for window z-order
        private const int HWND_TOPMOST = -1;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOACTIVATE = 0x0010;
        
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        
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
                
                // First, use immediate positioning
                Application.Current.Dispatcher.Invoke(() => {
                    _logger.Debug("ShowWidget called - displaying widget");
                    
                    // Ensure window is shown but initially hidden
                    _widgetView.Show();
                    _widgetView.Visibility = Visibility.Hidden;
                    
                    // Force immediate position calculation 
                    _widgetView.ShowWidget();
                    
                    // Ensure it's at the very top of z-order using Win32 API
                    EnsureTopMost();
                    
                    // Double check the window is topmost
                    _widgetView.Topmost = true;
                    
                    _logger.Debug($"Initial widget position: Left={_widgetView.Left}, Top={_widgetView.Top}");
                    
                    // Start periodic topmost enforcement to keep the widget visible
                    StartTopMostTimer();
                });
                
                // Then schedule a series of positioning attempts with delays
                SchedulePositioningAttempts();
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
                    StopTopMostTimer();
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
        
        /// <summary>
        /// Collapses the widget without hiding it
        /// </summary>
        public void CollapseWidget()
        {
            try
            {
                if (_widgetView != null)
                {
                    _logger.Debug("CollapseWidget called");
                    _widgetView.CollapseWidgetExternally();
                    
                    // Ensure it's at the very top of z-order even when collapsed
                    EnsureTopMost();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error collapsing widget", ex);
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
        
        /// <summary>
        /// Ensures the widget stays at the very top of the z-order using Win32 API
        /// and positioned correctly at the top edge of the screen
        /// </summary>
        private void EnsureTopMost()
        {
            try
            {
                if (_widgetView != null && _widgetView.Visibility == Visibility.Visible)
                {
                    // Get the window handle
                    var windowInteropHelper = new WindowInteropHelper(_widgetView);
                    var handle = windowInteropHelper.Handle;
                    
                    if (handle != IntPtr.Zero)
                    {
                        // Directly position widget at absolute top edge
                        _widgetView.PositionAtAbsoluteTopEdge();
                        _logger.Debug("Reapplied absolute top edge positioning");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error setting widget as topmost", ex);
            }
        }
        
        /// <summary>
        /// Starts a timer to periodically ensure the widget stays on top
        /// </summary>
        private void StartTopMostTimer()
        {
            try
            {
                // Stop existing timer if any
                StopTopMostTimer();
                
                // Create new timer that ensures topmost every 1 second
                _topMostTimer = new System.Threading.Timer(
                    _ => Application.Current.Dispatcher.BeginInvoke(new Action(EnsureTopMost)),
                    null,
                    0,
                    1000
                );
                
                _logger.Debug("Topmost timer started");
            }
            catch (Exception ex)
            {
                _logger.Error("Error starting topmost timer", ex);
            }
        }
        
        /// <summary>
        /// Stops the topmost timer
        /// </summary>
        private void StopTopMostTimer()
        {
            try
            {
                if (_topMostTimer != null)
                {
                    _topMostTimer.Dispose();
                    _topMostTimer = null;
                    _logger.Debug("Topmost timer stopped");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error stopping topmost timer", ex);
            }
        }
        
        /// <summary>
        /// Schedules multiple positioning attempts with increasing delays
        /// </summary>
        private void SchedulePositioningAttempts()
        {
            // Use DispatcherTimer for safer, reflection-free approach
            SchedulePositioningAttempt(1, 50);
            SchedulePositioningAttempt(2, 200);
            SchedulePositioningAttempt(3, 500);
            SchedulePositioningAttempt(4, 1000);
        }
        
        private void SchedulePositioningAttempt(int attemptNumber, int delayMs)
        {
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(delayMs)
            };
            
            timer.Tick += (s, e) =>
            {
                try
                {
                    // Stop the timer
                    timer.Stop();
                    
                    // Position the widget
                    EnsureTopMost();
                    
                    // Log the attempt
                    if (attemptNumber == 4)
                    {
                        _logger.Debug($"Final positioning attempt: Left={_widgetView?.Left}, Top={_widgetView?.Top}");
                    }
                    else
                    {
                        _logger.Debug($"Positioning attempt {attemptNumber}: Left={_widgetView?.Left}, Top={_widgetView?.Top}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error in positioning attempt {attemptNumber}", ex);
                }
            };
            
            timer.Start();
        }
        
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
} 