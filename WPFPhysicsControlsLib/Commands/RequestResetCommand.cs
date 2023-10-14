using System;
using PhysicsSimulation;
using PhysicsSimulation.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;

namespace WPFPhysicsControlsLib.Commands
{
    public class RequestResetCommand : DelegateCommand
    {
        public RequestResetCommand() : base(RequestReset)
        {
            // CanExecute(true);
        }

        private static void RequestReset()
        {
            var eventAggregator = ContainerLocator.Current.Resolve(typeof(IEventAggregator)) as IEventAggregator;

            if (eventAggregator == null)
                throw new NullReferenceException($"Cannot retrieve {typeof(EventAggregator).FullName}. Source:{typeof(RequestResetCommand).FullName}");

            eventAggregator.GetEvent<ResetRequestedEvent>().Publish(true);
        }
    }
}
