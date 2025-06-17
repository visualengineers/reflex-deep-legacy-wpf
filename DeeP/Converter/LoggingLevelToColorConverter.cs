using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using NLog;

namespace ReFlex.Apps.DeeP.Converter
{
    [ValueConversion(typeof(LogLevel), typeof(Color))]
    public class LoggingLevelToColorConverter : IValueConverter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                Logger.Log(LogLevel.Error, new StackTrace().GetFrame(1).GetMethod().Name, new NullReferenceException("No value for conversion from LoggingLevel to Color provided."));
                return null;
            }
            var lvl = value as LogLevel;

            //TODO: error prone in case of refactoring: find cleaner way to implement
            if (lvl == LogLevel.Debug)
                return Application.Current.Resources["LoggingDebugColor"];
            else if (lvl == LogLevel.Trace)
                return Application.Current.Resources["LoggingTraceColor"];
            else if (lvl == LogLevel.Info)
                return Application.Current.Resources["LoggingNotificationColor"];
            else if (lvl == LogLevel.Warn)
                return Application.Current.Resources["LoggingWarningColor"];
            else if (lvl == LogLevel.Error)
                return Application.Current.Resources["LoggingErrorColor"];
            else if (lvl == LogLevel.Fatal) 
                return Application.Current.Resources["LoggingExceptionColor"];

            Logger.Log(LogLevel.Warn, "Provided LoggingLevel is not implemented. At " + GetType().FullName + "." + new StackTrace().GetFrame(1).GetMethod().Name + ".");
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                Logger.Log(LogLevel.Error, new StackTrace().GetFrame(1).GetMethod().Name, new NullReferenceException("No value for conversion from Color to LoggingLevel provided."));
                return null;
            }

            var c = (Color)value;

            //TODO: error prone in case of refactoring: find cleaner way to implement
            if (c.Equals(Application.Current.Resources["LoggingDebugColor"]))
                return LogLevel.Debug;

            if (c.Equals(Application.Current.Resources["LoggingTraceColor"]))
                return LogLevel.Trace;

            if (c.Equals(Application.Current.Resources["LoggingNotificationColor"]))
                return LogLevel.Info;

            if (c.Equals(Application.Current.Resources["LoggingWarningColor"]))
                return LogLevel.Warn;

            if (c.Equals(Application.Current.Resources["LoggingErrorColor"]))
                return LogLevel.Error;

            if (c.Equals(Application.Current.Resources["LoggingExceptionColor"]))
                return LogLevel.Fatal;

            Logger.Log(LogLevel.Warn, "Provided Color is not associated to LoggingLevel. At " + GetType().FullName + "." + new StackTrace().GetFrame(1).GetMethod().Name + ".");
            return null;

        }
    }
}
