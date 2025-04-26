using System;
using System.Windows;
using EngageApp.Modules.ModuleName;
using EngageApp.Modules.Widget;
using EngageApp.Modules.Widget.Services;
using EngageApp.Modules.Widget.Services.Interfaces;
using EngageApp.Services;
using EngageApp.Services.Interfaces;
using EngageApp.ViewModels;
using EngageApp.Views;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;

namespace EngageApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Register application services
            containerRegistry.RegisterSingleton<IMessageService, MessageService>();
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterSingleton<IScreenPositionService, ScreenPositionService>();
            
            // Register widget services directly (normally done by the module)
            containerRegistry.RegisterSingleton<IWidgetLoggerService, WidgetLoggerService>();
            containerRegistry.RegisterSingleton<IDpiAwarenessService, DpiAwarenessService>();
            containerRegistry.RegisterSingleton<IWidgetScreenService, WidgetScreenService>();
            containerRegistry.RegisterSingleton<IWidgetService, WidgetService>();
            
            // Register ViewModels
            containerRegistry.RegisterSingleton<MainWindowViewModel>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            // Load modules
            moduleCatalog.AddModule<WidgetModule>();
            moduleCatalog.AddModule<ModuleNameModule>();
        }
        
        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            // Initialize widget services
            Container.Resolve<IWidgetLoggerService>().Initialize();
            Container.Resolve<IDpiAwarenessService>().EnableHighDpiSupport();
            Container.Resolve<IWidgetLoggerService>().Info("Application started");
        }
    }
}
