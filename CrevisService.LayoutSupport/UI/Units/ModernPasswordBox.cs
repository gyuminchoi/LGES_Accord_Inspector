using System.Windows;
using System.Windows.Controls;

namespace CrevisService.LayoutSupport.UI.Units
{
    public class ModernPasswordBox : TextBox
    {
        public static readonly DependencyProperty BindablePasswordProperty =
            DependencyProperty.Register(
                nameof(BindablePassword),
                typeof(string),
                typeof(ModernPasswordBox),
                new PropertyMetadata(string.Empty));

        public string BindablePassword
        {
            get => (string)GetValue(BindablePasswordProperty);
            set => SetValue(BindablePasswordProperty, value);
        }

        static ModernPasswordBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ModernPasswordBox), new FrameworkPropertyMetadata(typeof(ModernPasswordBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_PasswordBox") is PasswordBox pb)
            {
                pb.PasswordChanged += PbPasswordChanged;
            }
        }

        private void PbPasswordChanged(object sender, RoutedEventArgs e)
        {
            BindablePassword = ((PasswordBox)sender).Password;
            if (GetTemplateChild("PART_TextBlock") is TextBlock tblock)
            {
                tblock.Visibility = ( BindablePassword == string.Empty || BindablePassword == null ) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
