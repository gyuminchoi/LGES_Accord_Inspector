using Dialog.IOMonitor.Models.States;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Dialog.IOMonitor.Services.ConverterService.ViewConverter
{
    public class SignalCheckStateToEnabled : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ESignalCheckState signalState = (ESignalCheckState)value;

            if (signalState == ESignalCheckState.Stopped)
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
