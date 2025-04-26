using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using EngageApp.Modules.Widget.Services.Interfaces;
using EngageApp.Modules.Widget.ViewModels;

namespace EngageApp.Modules.Widget.Views
{
    /// <summary>
    /// Interaction logic for WidgetView.xaml
    /// </summary>
    public partial class WidgetView : Window
    {
        private readonly IWidgetScreenService _screenService;
        private readonly IWidgetLoggerService _logger;
        private bool _isDragging;
        private Point _dragStartPoint;
        
        /// <summary>
        /// Occurs when the widget is clicked.
        /// </summary>
        public event EventHandler WidgetClicked;
        
        /// <summary>
        /// Occurs when the widget is double-clicked.
        /// </summary>
        public event EventHandler WidgetDoubleClicked;

        /// <summary>
        /// Initializes a new instance of the <see cref="WidgetView"/> class.
        /// </summary>
        /// <param name="screenService">The screen service</param>
        /// <param name="logger">The logger service</param>
        public WidgetView(IWidgetScreenService screenService, IWidgetLoggerService logger)
        {
            InitializeComponent();
            
            _screenService = screenService;
            _logger = logger;
            
            // Hide the widget initially
            Visibility = Visibility.Hidden;
        }
        
        /// <summary>
        /// Shows the widget.
        /// </summary>
        public void ShowWidget()
        {
            try
            {
                _logger.Debug("ShowWidget called - positioning widget and setting visible");
                
                // Ensure the window is created and shown first
                Show();
                
                // Position at top-right
                _screenService.PositionWindowTopRight(this);
                
                // Ensure visibility settings
                Visibility = Visibility.Visible;
                Topmost = true;
                Activate();
                
                // Force layout update and bring to foreground
                UpdateLayout();
                Focus();
                
                // Add this animation to draw attention to the widget
                var storyboard = FindResource("ExpandStoryboard") as Storyboard;
                storyboard?.Begin();
                
                _logger.Debug($"Widget position: Left={Left}, Top={Top}, Width={Width}, Height={Height}, Visibility={Visibility}");
            }
            catch (Exception ex)
            {
                _logger.Error("Error showing widget", ex);
            }
        }
        
        /// <summary>
        /// Hides the widget.
        /// </summary>
        public void HideWidget()
        {
            Visibility = Visibility.Hidden;
            _logger.Debug("Widget hidden");
        }
        
        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            var storyboard = FindResource("ExpandStoryboard") as Storyboard;
            storyboard?.Begin();
        }
        
        private void Window_MouseLeave(object sender, MouseEventArgs e)
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
                
                // Raise clicked event
                WidgetClicked?.Invoke(this, EventArgs.Empty);
            }
            else if (e.ClickCount == 2)
            {
                // Double-click to restore the main window
                WidgetDoubleClicked?.Invoke(this, EventArgs.Empty);
            }
        }
        
        private void Widget_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point currentPosition = e.GetPosition(this);
                Vector dragOffset = currentPosition - _dragStartPoint;
                
                // Move the widget
                Left += dragOffset.X;
                Top += dragOffset.Y;
                
                // Snap to edges
                _screenService.SnapToNearestEdge(this);
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