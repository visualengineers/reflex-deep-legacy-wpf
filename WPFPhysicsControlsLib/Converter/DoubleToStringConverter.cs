using System;
using System.Windows.Data;

namespace WPFPhysicsControlsLib.Converter
{
    [ValueConversion(typeof(Double), typeof(String), ParameterType = typeof(int))]
    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var dbl = value as Double?;
            if (dbl == null)
                return "0";

            //TODO: parameter: number of fractional digits
            
            return String.Format("{0: 0.00}", dbl);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            Double dbl;

            Double.TryParse(value.ToString(), out dbl);

            return dbl;
        }
    }
}
