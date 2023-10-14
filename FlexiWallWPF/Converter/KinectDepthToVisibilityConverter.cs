using System;
using System.Windows;
using System.Windows.Data;

namespace FlexiWallWPF.Converter
{
    public class KinectDepthToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Member

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //var properties = (KinectFrameProperties)value;

            //if (properties == null)
            //    return Application.Current.Resources["InvisibleBorderBrush"];

            //float parameterFloat = System.Convert.ToSingle(parameter);

            //if (properties.Maximum.DepthValue >= parameterFloat && properties.Minimum.DepthValue <= parameterFloat)
            //{
            //    return Application.Current.Resources["VisibleBorderBrush"];
            //}

                
            return Application.Current.Resources["InvisibleBorderBrush"];
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
