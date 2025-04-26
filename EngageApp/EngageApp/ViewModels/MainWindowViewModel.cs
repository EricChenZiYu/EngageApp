using System;
using System.Windows;
using EngageApp.Core.Events;
using EngageApp.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace EngageApp.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private string _title = "Engage App";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public DelegateCommand MinimizeCommand { get; private set; }
        public DelegateCommand CloseCommand { get; private set; }
        public DelegateCommand TestWidgetCommand { get; private set; }

        public MainWindowViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            
            MinimizeCommand = new DelegateCommand(ExecuteMinimizeCommand);
            CloseCommand = new DelegateCommand(ExecuteCloseCommand);
            TestWidgetCommand = new DelegateCommand(ExecuteTestWidgetCommand);
            
            _eventAggregator.GetEvent<WindowRestoreRequestedEvent>().Subscribe(HandleWindowRestoreRequested);
        }

        private void ExecuteMinimizeCommand()
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.MinimizeWithAnimation();
            }
        }

        private void ExecuteCloseCommand()
        {
            Application.Current.Shutdown();
        }
        
        private void HandleWindowRestoreRequested()
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.RestoreWithAnimation();
            }
        }

        private void ExecuteTestWidgetCommand()
        {
            Console.WriteLine("Test widget command executed");
            // Use the application instance to show the widget for testing
            if (Application.Current is App app)
            {
                app.ShowWidgetForTesting();
            }
        }
    }
}
