using System;
using System.Windows;
using System.Windows.Data;

namespace FlexiWallWPF.Converter
{
    public class MultiVisibilityConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool result = true;
                int num = 0;

            foreach (object obj in values)
            {
                if (obj is Visibility)
                {
                    num++;
                    result = result && ((Visibility) obj) == Visibility.Visible;
                }
            }
            
            return (result && num > 1) ? Visibility.Visible : Visibility.Hidden;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
