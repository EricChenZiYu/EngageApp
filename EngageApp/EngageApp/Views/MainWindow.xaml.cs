using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using EngageApp.Core.Events;
using Prism.Events;

namespace EngageApp.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                DragMove();
            }
        }

        internal void MinimizeWithAnimation()
        {
            Console.WriteLine("MinimizeWithAnimation called");
            
            // Create fade and shrink animation
            var storyboard = new Storyboard();
            
            var fadeAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(System.TimeSpan.FromMilliseconds(500)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            
            var scaleXAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.1,
                Duration = new Duration(System.TimeSpan.FromMilliseconds(500)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            
            var scaleYAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.1,
                Duration = new Duration(System.TimeSpan.FromMilliseconds(500)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(fadeAnimation, this);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(OpacityProperty));
            
            storyboard.Children.Add(fadeAnimation);
            storyboard.Completed += (s, e) => 
            {
                // Hide window
                Visibility = Visibility.Hidden;
            };
            
            storyboard.Begin();
        }

        internal void RestoreWithAnimation()
        {
            // Make window visible again
            Visibility = Visibility.Visible;
            Opacity = 0;

            // Create fade-in and slide-down animation
            var storyboard = new Storyboard();
            
            var fadeAnimation = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = new Duration(System.TimeSpan.FromMilliseconds(500)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            
            // Slide in from top effect
            var topAnimation = new DoubleAnimation
            {
                From = -Height,
                To = Top,
                Duration = new Duration(System.TimeSpan.FromMilliseconds(500)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(fadeAnimation, this);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(OpacityProperty));
            
            Storyboard.SetTarget(topAnimation, this);
            Storyboard.SetTargetProperty(topAnimation, new PropertyPath(TopProperty));
            
            storyboard.Children.Add(fadeAnimation);
            storyboard.Children.Add(topAnimation);
            
            storyboard.Begin();
        }
    }
}
