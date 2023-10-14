using System;
using System.Windows.Data;

namespace WPFPhysicsControlsLib.Converter
{
    [ValueConversion(typeof(Single), typeof(String), ParameterType = typeof(int))]
    public class FloatToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var floatValue = value as Single?;
            if (floatValue == null)
                return "0";

            //TODO: parameter: number of fractional digits

            return String.Format("{0: 0.00}", floatValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            Single floatValue;

            Single.TryParse(value.ToString(), out floatValue);

            return floatValue;
        }
    }
}
