using System.Windows;
using System.Windows.Controls;

namespace CrevisService.LayoutSupport.UI.Units
{
    public class WrappingListBox : ListBox
    {
        public static readonly DependencyProperty WrapPanelOrientationProperty =
            DependencyProperty.Register(nameof(WrapPanelOrientation), typeof(Orientation), typeof(WrappingListBox), new PropertyMetadata(Orientation.Vertical));

        public static readonly DependencyProperty ItemWidthSizeProperty =
            DependencyProperty.Register(nameof(ItemWidthSize), typeof(double), typeof(WrappingListBox), new PropertyMetadata(50.0));

        public static readonly DependencyProperty ItemHeightSizeProperty =
            DependencyProperty.Register(nameof(ItemHeightSize), typeof(double), typeof(WrappingListBox), new PropertyMetadata(50.0));

        public static readonly DependencyProperty WrapCountProperty =
            DependencyProperty.Register(nameof(WrapCount), typeof(int), typeof(WrappingListBox), new PropertyMetadata(5));

        public static readonly DependencyProperty IsEffectProperty =
            DependencyProperty.Register(nameof(IsEffect), typeof(bool), typeof(WrappingListBox), new PropertyMetadata(true));


        public Orientation WrapPanelOrientation
        {
            get => (Orientation)GetValue(WrapPanelOrientationProperty);
            set => SetValue(WrapPanelOrientationProperty, value);
        }
        public double ItemWidthSize
        {
            get => (double)GetValue(ItemWidthSizeProperty);
            set => SetValue(ItemWidthSizeProperty, value);
        }
        public double ItemHeightSize
        {
            get => (double)GetValue(ItemHeightSizeProperty);
            set => SetValue(ItemHeightSizeProperty, value);
        }
        public int WrapCount
        {
            get => (int)GetValue(WrapCountProperty);
            set => SetValue(WrapCountProperty, value);
        }
        public bool IsEffect
        {
            get => (bool)GetValue(IsEffectProperty);
            set => SetValue(IsEffectProperty, value);
        }

        static WrappingListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WrappingListBox), new FrameworkPropertyMetadata(typeof(WrappingListBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (WrapPanelOrientation == Orientation.Horizontal)
            {
                Width = (ItemWidthSize * WrapCount) + ((Padding.Left + Padding.Right) * WrapCount) + Margin.Left + Margin.Right;
            }
            else
            {
                Height = (ItemHeightSize * WrapCount) + ((Padding.Top + Padding.Bottom) * WrapCount) + Margin.Top + Margin.Bottom;
            }
        }
    }
}