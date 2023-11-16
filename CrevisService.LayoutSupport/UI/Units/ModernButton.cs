using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CrevisService.LayoutSupport.UI.Units
{
    public class ModernButton : Button
    {
        public static readonly DependencyProperty MouseOverBackgroundProperty =
            DependencyProperty.Register(
            nameof(MouseOverBackground),
            typeof(Brush),
            typeof(ModernButton),
            new PropertyMetadata((SolidColorBrush)new BrushConverter().ConvertFrom("#005F8B")));
        public static readonly DependencyProperty PressedBackgroundProperty =
            DependencyProperty.Register(
            nameof(PressedBackground),
            typeof(Brush),
            typeof(ModernButton),
            new PropertyMetadata((SolidColorBrush)new BrushConverter().ConvertFrom("#003B54")));
        public static readonly DependencyProperty DisableBackgroundProperty =
            DependencyProperty.Register(
            nameof(DisableBackground),
            typeof(Brush),
            typeof(ModernButton),
            new PropertyMetadata((SolidColorBrush)new BrushConverter().ConvertFrom("#CCCCCC")));
        public static readonly DependencyProperty DisableForegroundProperty =
            DependencyProperty.Register(
            nameof(DisableForeground),
            typeof(Brush),
            typeof(ModernButton),
            new PropertyMetadata((SolidColorBrush)new BrushConverter().ConvertFrom("#999999")));

        public Brush MouseOverBackground
        {
            get => (Brush)GetValue(MouseOverBackgroundProperty);
            set => SetValue(MouseOverBackgroundProperty, value);
        }
        public Brush PressedBackground
        {
            get => (Brush)GetValue(PressedBackgroundProperty);
            set => SetValue(PressedBackgroundProperty, value);
        }
        public Brush DisableBackground
        {
            get => (Brush)GetValue(DisableBackgroundProperty);
            set => SetValue(DisableBackgroundProperty, value);
        }
        public Brush DisableForeground
        {
            get => (Brush)GetValue(DisableForegroundProperty);
            set => SetValue(DisableForegroundProperty, value);
        }

        static ModernButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ModernButton), new FrameworkPropertyMetadata(typeof(ModernButton)));
        }
    }
}
