using Dialog.IOMonitor.Models.IOData;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Dialog.IOMonitor.Services.ConverterService.ViewConverter
{
    public class SignalStateToBorderBrush : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            SignalData beforeSignal = (SignalData)values[0];
            bool signal = (bool)values[1];
            
            if(beforeSignal == null)
                return Brushes.Transparent;

            if (beforeSignal.Signal == signal)
                return Brushes.Transparent;
            else
                return Brushes.DarkGray;

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
