using System;
using System.Globalization;
using System.Windows.Data;

namespace Dialog.IOMonitor.Services.ConverterService.ViewConverter
{
    public class SignalStateToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool signal = (bool)value;
            int height = 0;
            if (signal)
            {
                height = 30;
                return height;
            }
            else
            {
                height = 2;
                return height;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
