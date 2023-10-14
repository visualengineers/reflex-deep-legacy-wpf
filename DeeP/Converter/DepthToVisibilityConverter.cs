using System;
using System.Windows.Data;
using System.Windows;

namespace DeeP.Converter
{
    public class DepthToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Member

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            float valueFloat = (float)value;
            float parameterFloat = System.Convert.ToSingle(parameter);

            if (valueFloat < parameterFloat)
            {
                return Application.Current.Resources["VisibleBorderBrush"];
            }
            else
            {
                return Application.Current.Resources["InvisibleBorderBrush"];
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
