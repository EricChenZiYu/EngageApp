using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using EngageApp.Modules.Widget.Services.Interfaces;

namespace EngageApp.Modules.Widget.Services
{
    /// <summary>
    /// Service for handling DPI awareness in the application.
    /// </summary>
    public class DpiAwarenessService : IDpiAwarenessService
    {
        private readonly IWidgetLoggerService _logger;
        
        // P/Invoke definitions for DPI awareness
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [DllImport("shcore.dll")]
        private static extern int SetProcessDpiAwareness(ProcessDpiAwareness value);

        [DllImport("user32.dll")]
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
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DpiAwarenessService"/> class.
        /// </summary>
        /// <param name="logger">The logger service</param>
        public DpiAwarenessService(IWidgetLoggerService logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public void EnableHighDpiSupport()
        {
            try
            {
                // Set DPI awareness for high resolution displays
                RenderOptions.ProcessRenderMode = RenderMode.Default;
                
                // Enable per-monitor DPI awareness
                if (Environment.OSVersion.Version >= new Version(6, 3, 0))
                {
                    // Windows 8.1 and above, enable per monitor DPI awareness
                    if (Environment.OSVersion.Version >= new Version(10, 0, 15063))
                    {
                        SetProcessDpiAwarenessContext((int)DpiAwarenessContext.PerMonitorAwareV2);
                        _logger.Info("Enabled PerMonitorAwareV2 DPI awareness");
                    }
                    else
                    {
                        SetProcessDpiAwareness(ProcessDpiAwareness.PerMonitorDpiAware);
                        _logger.Info("Enabled PerMonitorDpiAware DPI awareness");
                    }
                }
                else
                {
                    // Windows 7 and 8, use system DPI awareness
                    SetProcessDPIAware();
                    _logger.Info("Enabled System DPI awareness");
                }
                
                _logger.Info($"Current DPI scale: {GetSystemDpiScale()}");
            }
            catch (Exception ex)
            {
                _logger.Error("Error enabling high DPI support", ex);
            }
        }

        /// <inheritdoc/>
        public double GetSystemDpiScale()
        {
            try
            {
                // Get DPI of primary screen
                var source = PresentationSource.FromVisual(Application.Current.MainWindow);
                if (source?.CompositionTarget != null)
                {
                    Matrix m = source.CompositionTarget.TransformToDevice;
                    return m.M11; // Use horizontal scale (usually same as vertical scale)
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting system DPI scale", ex);
            }
            
            // Default to 100% scaling
            return 1.0;
        }
    }
} 