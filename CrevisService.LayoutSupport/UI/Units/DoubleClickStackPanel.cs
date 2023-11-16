using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CrevisService.LayoutSupport.UI.Units
{
    public class DoubleClickStackPanel : StackPanel
    {
        public static readonly RoutedEvent MouseDoubleClickEvnet =
            EventManager.RegisterRoutedEvent("StackPanelDoubleClick", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(DoubleClickStackPanel));

        public event RoutedEventHandler MouseDoubleClick
        {
            add { AddHandler(MouseDoubleClickEvnet, value); }
            remove { RemoveHandler(MouseDoubleClickEvnet, value); }
        }


        static DoubleClickStackPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DoubleClickStackPanel), new FrameworkPropertyMetadata(typeof(DoubleClickStackPanel)));
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (e.ClickCount == 2)
            {
                RaiseEvent(new RoutedEventArgs(MouseDoubleClickEvnet));
            }
        }
    }
}
