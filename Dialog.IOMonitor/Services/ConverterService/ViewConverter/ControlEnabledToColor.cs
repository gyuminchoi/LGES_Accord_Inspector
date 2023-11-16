using System;
using System.Globalization;
using System.Windows.Data;

namespace Dialog.IOMonitor.Services.ConverterService.ViewConverter
{
    public class ControlEnabledToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isEnabled = (bool)value;

            if (isEnabled)
                return "#9EF048";   // Green 계열
            else
                return "#F44848";   // red 계열
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
