using System;
using System.Windows;
using System.Windows.Data;

namespace DeeP.Converter
{
    [ValueConversion(typeof(bool), typeof(Visibility), ParameterType = typeof(string))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is bool))
                return Visibility.Visible;
            
            var param = parameter as String;

            var val = (bool) value;

            if (!String.IsNullOrWhiteSpace(param) && param.Equals("invert"))
            {
                val = ! val;
            }

            return val ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is Visibility))
                return true;

            var param = parameter as String;

            var val = (Visibility)value;

            var result = val == Visibility.Visible;

            if (!String.IsNullOrWhiteSpace(param) && param.Equals("invert"))
            {
                result = !result;
            }

            return result;
        }
    }
}
