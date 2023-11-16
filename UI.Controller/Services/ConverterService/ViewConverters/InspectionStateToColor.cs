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
    public class InspectionStateToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EInspectionState state = (EInspectionState)value;
            switch (state) 
            {
                case EInspectionState.Stopped:
                    return "#9EF048";   // Green 계열
                case EInspectionState.Running:
                    return "#F44848";   // Red 계열
                case EInspectionState.Error:
                default:
                    return "#FF9900";   // 주황 계열
            }  
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
