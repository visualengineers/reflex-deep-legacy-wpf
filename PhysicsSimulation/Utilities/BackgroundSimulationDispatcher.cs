using System;
using System.ComponentModel;
using PhysicsSimulation.Events;
using PhysicsSimulation.Model;
using Prism.Events;
using Prism.Ioc;

namespace PhysicsSimulation.Utilities
{
    public class BackgroundSimulationDispatcher
    {
        private readonly PhysicsSimulator _sim;
        private readonly BackgroundWorker _simulationThread;

        public BackgroundSimulationDispatcher(PhysicsSimulator sim)
        {
            _sim = sim;
            _simulationThread = new BackgroundWorker();
            _simulationThread.DoWork += UpdateSimulationBackground;

            var eventAggregator = ContainerLocator.Current.Resolve(typeof(IEventAggregator)) as IEventAggregator;

            if (eventAggregator == null)
                throw new NullReferenceException($"Cannot retrieve {typeof(EventAggregator).FullName}. Source:{GetType().FullName}");

            eventAggregator.GetEvent<ResetRequestedEvent>()
                .Subscribe(delegate { RequestReset(); });
        }

        public void RequestReset()
        {
            if (_simulationThread.IsBusy)
                _simulationThread.RunWorkerCompleted += ExecuteReset;
            else
                ExecuteReset(this, null);

        }

        private void ExecuteReset(object sender, RunWorkerCompletedEventArgs e)
        {
            _sim.Reset();
            _simulationThread.RunWorkerCompleted -= ExecuteReset;
        }


        public void RequestUpdate()
        {
            if (_simulationThread.IsBusy)
                return;

            _simulationThread.RunWorkerAsync();
        }

        private void UpdateSimulationBackground(object sender, DoWorkEventArgs e)
        {
            _sim.UpdateSimulation(0);
        }
    }
}
