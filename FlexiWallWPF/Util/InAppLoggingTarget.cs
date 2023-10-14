
using EventHandling;
using NLog;
using NLog.Targets;
using Prism.Events;
using Prism.Ioc;

namespace FlexiWallWPF.Util
{

    [Target("InAppLogging")]
    public sealed class InAppLoggingTarget : TargetWithLayout
    {
        private IEventAggregator _aggregator;

        public InAppLoggingTarget()
        {
        }

        protected override void Write(LogEventInfo logEvent)
        {
            var logMessage = RenderLogEvent(Layout, logEvent);
            BroadcastLogMessage(logMessage, logEvent.Level);
        }

        private void BroadcastLogMessage(string message, LogLevel level)
        {
            if (_aggregator == null)
            {
                var success = RetrieveEventAggregatorInstance();
                if (!success)
                    return;
            }
            _aggregator.GetEvent<BroadcastLogEvent>().Publish(
                new LogUpdatedEventArgs(level, message));
        }

        private bool RetrieveEventAggregatorInstance()
        {
            var result = ContainerLocator.Current.Resolve(typeof(IEventAggregator)) as IEventAggregator;

            var success = result != null;

            if (success)
                _aggregator = result;

            return success;
        }
    }

}