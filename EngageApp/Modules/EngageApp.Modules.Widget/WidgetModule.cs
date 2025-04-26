using EngageApp.Modules.Widget.Services;
using EngageApp.Modules.Widget.Services.Interfaces;
using Prism.Ioc;
using Prism.Modularity;

namespace EngageApp.Modules.Widget
{
    /// <summary>
    /// Widget module for Prism.
    /// </summary>
    public class WidgetModule : IModule
    {
        /// <summary>
        /// Registers the types with the container.
        /// </summary>
        /// <param name="containerRegistry">The container registry.</param>
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Register services
            containerRegistry.RegisterSingleton<IWidgetLoggerService, WidgetLoggerService>();
            containerRegistry.RegisterSingleton<IDpiAwarenessService, DpiAwarenessService>();
            containerRegistry.RegisterSingleton<IWidgetScreenService, WidgetScreenService>();
            containerRegistry.RegisterSingleton<IWidgetService, WidgetService>();
        }

        /// <summary>
        /// Initializes the module.
        /// </summary>
        /// <param name="containerProvider">The container provider.</param>
        public void OnInitialized(IContainerProvider containerProvider)
        {
            // Initialize the logger service
            var logger = containerProvider.Resolve<IWidgetLoggerService>();
            logger.Initialize();
            logger.Info("Widget module initializing");
            
            // Enable high DPI support
            var dpiService = containerProvider.Resolve<IDpiAwarenessService>();
            dpiService.EnableHighDpiSupport();
            
            logger.Info("Widget module initialized");
        }
    }
} 