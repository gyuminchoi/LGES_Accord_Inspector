using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using UI.Controller.Models;

namespace UI.Controller.Services.ConverterService.ViewConverters
{
    public class InspectionStateToContent : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EInspectionState state = (EInspectionState)value;

            switch (state)
            {
                case EInspectionState.Stopped:
                    return "Start";

                case EInspectionState.Running:
                    return "Stop";

                case EInspectionState.Error:
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
