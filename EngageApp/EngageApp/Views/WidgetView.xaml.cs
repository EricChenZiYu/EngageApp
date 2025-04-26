using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using EngageApp.Core.Events;
using EngageApp.Services.Interfaces;
using Prism.Events;

namespace EngageApp.Views
{
    public partial class WidgetView : Window
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IScreenPositionService _screenPositionService;
        private bool _isDragging;
        private Point _dragStartPoint;

        public WidgetView(IEventAggregator eventAggregator, IScreenPositionService screenPositionService)
        {
            InitializeComponent();
            _eventAggregator = eventAggregator;
            _screenPositionService = screenPositionService;
            
            // Subscribe to window minimized event
            _eventAggregator.GetEvent<WindowMinimizedEvent>().Subscribe(ShowWidget);
            
            // Hide the widget initially
            Visibility = Visibility.Hidden;
        }
        
        public void ShowWidget()
        {
            try
            {
                Console.WriteLine("ShowWidget called - positioning widget and setting visible");
                
                // Ensure the window is created and shown first
                Show();
                
                // First, position at a known good location
                Left = 100;
                Top = 100;
                
                // Now use the service to position at top-right
                _screenPositionService.PositionWindowTopRight(this);
                
                // Ensure visibility settings
                Visibility = Visibility.Visible;
                Topmost = true;
                Activate();
                
                // Force layout update
                UpdateLayout();
                
                Console.WriteLine($"Widget position: Left={Left}, Top={Top}, Width={Width}, Height={Height}, Visibility={Visibility}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing widget: {ex.Message}");
            }
        }
        
        private void HideWidget()
        {
            Visibility = Visibility.Hidden;
        }
        
        private void Window_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var storyboard = FindResource("ExpandStoryboard") as Storyboard;
            storyboard?.Begin();
        }
        
        private void Window_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_isDragging)
            {
                var storyboard = FindResource("CollapseStoryboard") as Storyboard;
                storyboard?.Begin();
            }
        }
        
        private void WidgetBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                _isDragging = true;
                _dragStartPoint = e.GetPosition(this);
                CaptureMouse();
                
                // Attach mouse move and mouse up handlers
                MouseMove += Widget_MouseMove;
                MouseLeftButtonUp += Widget_MouseLeftButtonUp;
            }
            else if (e.ClickCount == 2)
            {
                // Double-click to restore the main window
                _eventAggregator.GetEvent<WindowRestoreRequestedEvent>().Publish();
                HideWidget();
            }
        }
        
        private void Widget_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point currentPosition = e.GetPosition(this);
                Vector dragOffset = currentPosition - _dragStartPoint;
                
                // Move the widget
                Left += dragOffset.X;
                Top += dragOffset.Y;
                
                // Snap to edges if close enough
                const double snapDistance = 20;
                var workingArea = _screenPositionService.GetCurrentScreenWorkingArea(this);
                
                // Snap to right edge
                if (Math.Abs(Left + Width - workingArea.Right) < snapDistance)
                {
                    Left = workingArea.Right - Width;
                }
                
                // Snap to left edge
                if (Math.Abs(Left - workingArea.Left) < snapDistance)
                {
                    Left = workingArea.Left;
                }
                
                // Snap to top edge
                if (Math.Abs(Top - workingArea.Top) < snapDistance)
                {
                    Top = workingArea.Top;
                }
                
                // Snap to bottom edge
                if (Math.Abs(Top + Height - workingArea.Bottom) < snapDistance)
                {
                    Top = workingArea.Bottom - Height;
                }
            }
        }
        
        private void Widget_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            ReleaseMouseCapture();
            
            // Remove handlers
            MouseMove -= Widget_MouseMove;
            MouseLeftButtonUp -= Widget_MouseLeftButtonUp;
            
            // If the mouse is no longer over the widget, collapse it
            if (!IsMouseOver)
            {
                var storyboard = FindResource("CollapseStoryboard") as Storyboard;
                storyboard?.Begin();
            }
        }
    }
} 