using System;
using NLog;

namespace EventHandling
{
    public class LogUpdatedEventArgs
    {
        public string FormattedMessage { get; }

        public LogLevel Level { get; }

        public string Date { get; }

        public LogUpdatedEventArgs(LogLevel level, string formattedMessage)
        {
            Level = level;
            FormattedMessage = formattedMessage;
            var t = DateTime.Now;

            Date = $"{t:G}";
        }
    }
}
