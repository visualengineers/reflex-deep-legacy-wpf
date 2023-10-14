using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPFPhysicsControlsLib.Converter
{
    [ValueConversion(typeof(String), typeof(Visibility), ParameterType = typeof(String))]
    public class CategorySelectionVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var paramString = parameter as String;
            var valueString = value as String;

            if (String.IsNullOrWhiteSpace(paramString) || String.IsNullOrWhiteSpace(valueString))
                return Visibility.Collapsed;

            paramString = paramString.Trim().ToLower();
            valueString = valueString.Trim().ToLower();

            return Equals(paramString, valueString)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
