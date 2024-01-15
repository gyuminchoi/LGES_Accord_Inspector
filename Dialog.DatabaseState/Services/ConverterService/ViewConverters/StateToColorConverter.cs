using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Dialog.DatabaseState.Services.ConverterService.ViewConverters
{
    public class StateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ConnectionState connectionState = (ConnectionState)value;
            switch (connectionState)
            {
                case ConnectionState.Open:
                case ConnectionState.Connecting:
                case ConnectionState.Executing:
                case ConnectionState.Fetching:
                    return "#9EF048";   // Green

                case ConnectionState.Closed:
                case ConnectionState.Broken:
                default:
                    return "#F44848";   // Red
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
