using BarcodeLabel.Core.Models;
using System.Windows;
using System.Windows.Controls;

namespace CrevisService.LayoutSupport.UI.Units
{
    public class IsConnectedButton : Button
    {
        public static readonly DependencyProperty ConnectionMonitorProperty =
            DependencyProperty.Register(
            nameof(ConnectionMonitor),
            typeof(IConnectionMonitor),
            typeof(IsConnectedButton),
            new PropertyMetadata(null));

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(IsConnectedButton),
            new PropertyMetadata(null));

        public static readonly DependencyProperty CircleSizeProperty =
            DependencyProperty.Register(
            nameof(CircleSize),
            typeof(double),
            typeof(IsConnectedButton),
            new PropertyMetadata(null));

        public IConnectionMonitor ConnectionMonitor
        {
            get => (IConnectionMonitor)GetValue(ConnectionMonitorProperty);
            set => SetValue(ConnectionMonitorProperty, value);
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public double CircleSize
        {
            get => (double)GetValue(CircleSizeProperty);
            set => SetValue(CircleSizeProperty, value);
        }

        static IsConnectedButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IsConnectedButton), new FrameworkPropertyMetadata(typeof(IsConnectedButton)));
        }
    }
}
