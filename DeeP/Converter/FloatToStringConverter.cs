using System;
using System.Windows.Data;

namespace ReFlex.Apps.DeeP.Converter
{
    [ValueConversion(typeof(float), typeof(string), ParameterType = typeof(int))]
    public class FloatToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null || value == null)
                return value;

            int p = System.Convert.ToInt32(parameter);

            if (p < 1 || p > 7)
                return value;

            String formatStr = "{0:0.";
            for (int i = 0; i < p; i++)
                formatStr += "0";

            formatStr += "}";

            return String.Format(formatStr, value);
        }
    

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value == null ? 0 : System.Convert.ToSingle(value);
        }
    }
}
