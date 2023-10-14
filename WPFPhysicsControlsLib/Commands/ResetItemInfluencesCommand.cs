using System;
using EventHandling.Utilities;
using PhysicsSimulation;
using PhysicsSimulation.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;

namespace WPFPhysicsControlsLib.Commands
{
    public class ResetItemInfluencesCommand : DelegateCommand
    {
        public bool TagInfluenceResetEnabled
        {
            get { return CanExecute(); }
            set
            {
                if (CanExecute() == value)
                    return;

                CanExecute(value);
            }
        }

        public ResetItemInfluencesCommand(ObjectType objType) : base(() => ResetItemInfluences(objType))
        {
            TagInfluenceResetEnabled = false;
        }

        protected override void Execute(object parameter)
        {
            base.Execute(parameter);
            TagInfluenceResetEnabled = false;
        }

        private static void ResetItemInfluences(ObjectType objType)
        {
            var eventAggregator = ContainerLocator.Current.Resolve(typeof(IEventAggregator)) as IEventAggregator;

            if (eventAggregator == null)
                throw new NullReferenceException($"Cannot retrieve {typeof(EventAggregator).FullName}. Source:{typeof(ResetItemInfluencesCommand).FullName}");

            eventAggregator.GetEvent<ItemInfluenceResetRequestedEvent>().Publish(objType);
            
        }
    }
}
