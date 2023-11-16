using Service.Camera.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Dialog.CameraState.Services.ConverterService.ViewConverters
{
    public class StateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ECameraState state = (ECameraState)value;

            switch (state)
            {
                case ECameraState.Opened:
                case ECameraState.AcqStart:
                case ECameraState.GrabStart:
                case ECameraState.Setting:
                    return "#9EF048";   // Green 계열
                case ECameraState.Reconnecting:
                    return "#FF9900";   // 주황 계열
                case ECameraState.Error:
                    return "#F44848";   // Red 계열

                default: 
                    return System.Windows.Media.Brushes.Transparent;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
