using System;
using System.Windows;
using EngageApp.Core.Events;
using EngageApp.Modules.Widget.Services.Interfaces;
using EngageApp.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace EngageApp.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IWidgetService _widgetService;
        private string _title = "Engage App";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public DelegateCommand MinimizeCommand { get; private set; }
        public DelegateCommand CloseCommand { get; private set; }
        public DelegateCommand TestWidgetCommand { get; private set; }

        public MainWindowViewModel(IEventAggregator eventAggregator, IWidgetService widgetService)
        {
            _eventAggregator = eventAggregator;
            _widgetService = widgetService;
            
            MinimizeCommand = new DelegateCommand(ExecuteMinimizeCommand);
            CloseCommand = new DelegateCommand(ExecuteCloseCommand);
            TestWidgetCommand = new DelegateCommand(ExecuteTestWidgetCommand);
            
            // Subscribe to widget events
            _widgetService.WidgetDoubleClicked += WidgetService_WidgetDoubleClicked;
        }

        private void WidgetService_WidgetDoubleClicked(object sender, EventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                // Restore main window
                mainWindow.RestoreWithAnimation();
                
                // Hide widget
                _widgetService.HideWidget();
            }
        }

        private void ExecuteMinimizeCommand()
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.MinimizeWithAnimation();
                
                // Show widget when minimized
                _widgetService.ShowWidget();
            }
        }

        private void ExecuteCloseCommand()
        {
            Application.Current.Shutdown();
        }
        
        private void ExecuteTestWidgetCommand()
        {
            Console.WriteLine("Test widget command executed");
            _widgetService.ShowWidget();
        }
    }
}
