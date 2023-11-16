using Dialog.IOMonitor.Models.States;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Dialog.IOMonitor.Services.ConverterService.ViewConverter
{
    public class SignalCheckStateToBackground : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ESignalCheckState signalState = (ESignalCheckState)value;
            switch (signalState)
            {
                case ESignalCheckState.Stopped:
                    return "#9EF048";   // Green 계열
                case ESignalCheckState.Running:
                    return "#F44848";   // Red 계열
                default:
                    return "#000000";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
