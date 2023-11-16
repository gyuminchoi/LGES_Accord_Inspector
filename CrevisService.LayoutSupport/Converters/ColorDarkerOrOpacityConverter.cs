using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace CrevisService.LayoutSupport.Converters
{
    /// <summary>
    /// 너무 어둡다면 Opacity 조절을 통해 MouseOver, Pressed를 표시하고,
    /// 너무 어둡지 않다면 Color를 좀 더 어둡게 만들어 MouseOver, Pressed를 표시함.
    /// </summary>
    public class ColorDarkerOrOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                var factor = double.Parse((string)parameter);
                if (color.R < 50 && color.G < 50 && color.B < 50)
                {
                    // If color is dark, return the color with decreased opacity
                    return Color.FromArgb((byte)(color.A * factor), color.R, color.G, color.B);
                }
                else
                {
                    // If color is not dark, return a darker color
                    return Color.FromArgb(color.A,
                        (byte)(color.R * factor),
                        (byte)(color.G * factor),
                        (byte)(color.B * factor));
                }
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                var lightenAmount = 1 / double.Parse((string)parameter);
                return Color.FromArgb(color.A,
                    (byte)Math.Min(color.R * lightenAmount, 255),
                    (byte)Math.Min(color.G * lightenAmount, 255),
                    (byte)Math.Min(color.B * lightenAmount, 255));
            }

            return DependencyProperty.UnsetValue;
        }
    }
}
