using System;
using System.Windows;
using EngageApp.Core.Events;
using EngageApp.Modules.ModuleName;
using EngageApp.Services;
using EngageApp.Services.Interfaces;
using EngageApp.Views;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows.Media;

namespace EngageApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private WidgetView _widgetView;

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        // P/Invoke definitions for DPI awareness
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [System.Runtime.InteropServices.DllImport("shcore.dll")]
        private static extern int SetProcessDpiAwareness(ProcessDpiAwareness value);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetProcessDpiAwarenessContext(int value);

        private enum ProcessDpiAwareness
        {
            Unaware = 0,
            SystemAware = 1,
            PerMonitorDpiAware = 2
        }

        private enum DpiAwarenessContext
        {
            Unaware = -1,
            SystemAware = -2,
            PerMonitorAware = -3,
            PerMonitorAwareV2 = -4,
            UnawareGdiScaled = -5
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try 
            {
                // Set DPI awareness for high resolution displays
                System.Windows.Media.RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;
                
                // Enable per-monitor DPI awareness
                if (Environment.OSVersion.Version >= new Version(6, 3, 0))
                {
                    // Windows 8.1 and above, enable per monitor DPI awareness
                    if (Environment.OSVersion.Version >= new Version(10, 0, 15063))
                    {
                        SetProcessDpiAwarenessContext((int)DpiAwarenessContext.PerMonitorAwareV2);
                    }
                    else
                    {
                        SetProcessDpiAwareness(ProcessDpiAwareness.PerMonitorDpiAware);
                    }
                }
                else
                {
                    // Windows 7 and 8, use system DPI awareness
                    SetProcessDPIAware();
                }
                
                // Create console window for debugging
                AllocConsole();
                Console.WriteLine("EngageApp starting up...");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing console: {ex.Message}");
            }
            
            base.OnStartup(e);
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IMessageService, MessageService>();
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterSingleton<WidgetView>();
            containerRegistry.RegisterSingleton<IScreenPositionService, ScreenPositionService>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<ModuleNameModule>();
        }
        
        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            try
            {
                Console.WriteLine("Initializing widget...");
                
                // Initialize the widget - make sure it's properly instantiated
                _widgetView = Container.Resolve<WidgetView>();
                
                // Ensure the widget is created but not shown
                if (_widgetView != null)
                {
                    Console.WriteLine("Widget resolved successfully");
                    
                    // Create window but keep it hidden
                    _widgetView.WindowState = WindowState.Normal;
                    
                    // To avoid visual glitches, we position the widget off-screen initially
                    _widgetView.Left = -1000;
                    _widgetView.Top = -1000;
                    
                    _widgetView.Show();
                    _widgetView.Hide();
                    
                    Console.WriteLine("Widget initialized and hidden");
                }
                else
                {
                    Console.WriteLine("ERROR: Widget could not be resolved");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR initializing widget: {ex.Message}");
            }
        }
        
        protected override void OnExit(ExitEventArgs e)
        {
            // Clean up resources
            _widgetView?.Close();
            
            base.OnExit(e);
        }

        // Add public method to access widget for testing
        public void ShowWidgetForTesting()
        {
            try 
            {
                Console.WriteLine("ShowWidgetForTesting called");
                
                if (_widgetView == null)
                {
                    Console.WriteLine("WidgetView is null - resolving");
                    _widgetView = Container.Resolve<WidgetView>();
                    
                    if (_widgetView == null)
                    {
                        Console.WriteLine("ERROR: Could not resolve widget");
                        return;
                    }
                }
                
                Console.WriteLine("Showing widget for testing");
                
                // First, make sure the window is created
                _widgetView.WindowState = WindowState.Normal;
                
                // Get primary screen size in device-independent units
                System.Windows.Forms.Screen primaryScreen = System.Windows.Forms.Screen.PrimaryScreen;
                
                // Convert to device-independent units in WPF
                var source = PresentationSource.FromVisual(Application.Current.MainWindow);
                if (source != null)
                {
                    // Get DPI scaling factors
                    Matrix m = source.CompositionTarget.TransformToDevice;
                    double dpiX = m.M11;
                    double dpiY = m.M22;
                    
                    Console.WriteLine($"DPI scale: X={dpiX}, Y={dpiY}");
                    
                    // Position in center of screen with DPI adjustments
                    double screenWidth = primaryScreen.WorkingArea.Width / dpiX;
                    double screenHeight = primaryScreen.WorkingArea.Height / dpiY;
                    
                    // Position in center of screen
                    _widgetView.Left = screenWidth / 2 - _widgetView.Width / 2;
                    _widgetView.Top = screenHeight / 2 - _widgetView.Height / 2;
                    
                    Console.WriteLine($"Screen dimensions (adjusted for DPI): Width={screenWidth}, Height={screenHeight}");
                }
                else
                {
                    // Fallback if we can't get DPI information
                    _widgetView.Left = 100;
                    _widgetView.Top = 100;
                }
                
                // Make sure it's visible and on top
                _widgetView.Visibility = Visibility.Visible;
                _widgetView.Topmost = true;
                _widgetView.Show();
                _widgetView.Activate();
                
                Console.WriteLine($"Widget positioned at: Left={_widgetView.Left}, Top={_widgetView.Top}, " +
                                $"Width={_widgetView.Width}, Height={_widgetView.Height}, " +
                                $"Visibility={_widgetView.Visibility}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR showing widget: {ex.Message}");
            }
        }
    }
}
