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
            CloseCommand = new DelegateCommand(ExecuteMinimizeCommand);
            TestWidgetCommand = new DelegateCommand(ExecuteTestWidgetCommand);
            
            // Subscribe to widget events
            _widgetService.WidgetDoubleClicked += WidgetService_WidgetDoubleClicked;
        }

        private void WidgetService_WidgetDoubleClicked(object sender, EventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                // Keep track of main window's current position before restoring
                double left = mainWindow.Left;
                double top = mainWindow.Top;
                
                // Restore main window but keep its position
                mainWindow.RestoreAtPositionWithAnimation(left, top);
                
                // Hide widget
                _widgetService.HideWidget();
            }
        }

        private void ExecuteMinimizeCommand()
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                // First show widget to ensure it's positioned at top edge
                _widgetService.ShowWidget();
                
                // Now minimize the main window with animation
                mainWindow.MinimizeWithAnimation();
                
                // Add a short delay and collapse the widget after window is minimizing
                Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                    try {
                        // Very short delay to ensure widget is shown and positioned correctly
                        System.Threading.Thread.Sleep(200);
                        
                        // Try to call the method dynamically to collapse widget
                        var methodInfo = _widgetService.GetType().GetMethod("CollapseWidget");
                        if (methodInfo != null)
                        {
                            methodInfo.Invoke(_widgetService, null);
                            Console.WriteLine("Successfully collapsed widget");
                        }
                        else
                        {
                            // Try the external collapse method as fallback
                            var widgetViewType = Type.GetType("EngageApp.Modules.Widget.Views.WidgetView, EngageApp.Modules.Widget");
                            if (widgetViewType != null)
                            {
                                // Get all windows of this type
                                foreach (Window window in Application.Current.Windows)
                                {
                                    if (window.GetType() == widgetViewType)
                                    {
                                        // Try to call the CollapseWidgetExternally method
                                        var externalMethod = widgetViewType.GetMethod("CollapseWidgetExternally");
                                        if (externalMethod != null)
                                        {
                                            externalMethod.Invoke(window, null);
                                            Console.WriteLine("Collapsed widget externally");
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"Error collapsing widget: {ex.Message}");
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void ExecuteCloseCommand()
        {
            Application.Current.Shutdown();
        }
        
        private void ExecuteTestWidgetCommand()
        {
            Console.WriteLine("Test widget command executed");
            
            if (_widgetService.IsWidgetVisible)
            {
                Console.WriteLine("Widget is currently visible, hiding it");
                _widgetService.HideWidget();
            }
            else
            {
                Console.WriteLine("Widget is currently hidden, showing it");
                _widgetService.ShowWidget();
            }
        }
    }
}
