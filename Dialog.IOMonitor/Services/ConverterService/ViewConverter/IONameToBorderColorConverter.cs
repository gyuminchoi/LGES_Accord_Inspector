using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Dialog.IOMonitor.Services.ConverterService.ViewConverter
{
    public class IONameToBorderColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string ioName = (string)value;
            SolidColorBrush color;

            switch (ioName.Split('-')[1].ToCharArray()[0]) 
            {
                case '1':
                    return "#00A5FF";
                    //color = new SolidColorBrush(Colors.Blue);
                    //break;
                case '2':
                    return "#FF6600";
                    //color = new SolidColorBrush(Colors.Orange);
                    //break;
                case '3':
                    color = new SolidColorBrush(Colors.Green);
                    break;
                case '4':
                    color = new SolidColorBrush(Colors.Brown);
                    break;
                default:
                    color = new SolidColorBrush(Colors.Transparent);
                    break;
            }
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
