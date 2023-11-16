using Dialog.IOMonitor.Models.States;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Dialog.IOMonitor.Services.ConverterService.ViewConverter
{
    public class SignalCheckStateToContent : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ESignalCheckState signalState = (ESignalCheckState)value;

            switch (signalState)
            {
                case ESignalCheckState.Stopped:
                    return "Start Signal Check";
                case ESignalCheckState.Running:;
                    return "Stop Signal Check";
                default:
                    return "Error";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
