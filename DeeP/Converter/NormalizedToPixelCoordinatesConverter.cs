using System;
using System.Windows;
using System.Windows.Data;

namespace ReFlex.Apps.DeeP.Converter
{
    public class NormalizedToPixelCoordinatesConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Checking for value[0] == DependencProperty.UnsetValue prevents an InvalidCastException being thrown on when window is created /resized which resulted (even when caught directly) in a huge performance penalty
            if (!(value.Length == 2 || value.Length == 3) || value[0] == null || value[1] == null || value[0] == DependencyProperty.UnsetValue || value[1] == DependencyProperty.UnsetValue)
                return 0.0;
            var normValue = (double)value[0];
            var actualDimension = (double)value[1];

            // no third param set: no offset
            var controlDimension = value.Length == 3 ? (double)value[2] : 0;

            return (normValue * actualDimension) - (0.5 * controlDimension);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
