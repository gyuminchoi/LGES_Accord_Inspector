﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace CrevisService.LayoutSupport.Converters
{
    public class BooleanToRedGreenColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "Green" : "Red";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString() == "Green";
        }
    }
}
