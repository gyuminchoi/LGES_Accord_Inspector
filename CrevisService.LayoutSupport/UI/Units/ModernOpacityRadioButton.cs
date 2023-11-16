using System.Windows;
using System.Windows.Controls;

namespace CrevisService.LayoutSupport.UI.Units
{
    public class ModernOpacityRadioButton : RadioButton
    {
        static ModernOpacityRadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ModernOpacityRadioButton), new FrameworkPropertyMetadata(typeof(ModernOpacityRadioButton)));
        }
    }
}
