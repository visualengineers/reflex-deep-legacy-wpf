using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using PhysicsSimulation.Events;
using PhysicsSimulation.Model;
using Prism.Events;
using Prism.Ioc;

namespace PhysicsSimulation.Utilities
{
    public class BackgroundSimulationTask
    {
        private readonly PhysicsSimulator _sim;
        private bool _simulationThreadIsBusy;
        private readonly IEventAggregator _eventAggregator;

        private Task _simulationTask;

#if DEBUG
        private int _numIterations;
        private readonly Stopwatch _stopwatch;
        private float _averageTime;
        private int _totalTime;
#endif


         public BackgroundSimulationTask(PhysicsSimulator sim)
        {
            _sim = sim;

            _eventAggregator = ContainerLocator.Current.Resolve(typeof(IEventAggregator)) as IEventAggregator;

            if (_eventAggregator == null)
                throw new NullReferenceException($"Cannot retrieve {typeof(EventAggregator).FullName}. Source:{GetType().FullName}");

 
            //_simulationTask = new Task(() => UpdateSimulationBackground(this, null));
            _eventAggregator.GetEvent<ResetRequestedEvent>()
                .Subscribe(delegate { RequestReset(); });


#if DEBUG
             _stopwatch = new Stopwatch();
#endif
        }

        public void RequestReset()
        {
            //if (_simulationThreadIsBusy)
            //    _simulationThread.RunWorkerCompleted += ExecuteReset;
            //else
                ExecuteReset(this, null);

        }

        private void ExecuteReset(object sender, RunWorkerCompletedEventArgs e)
        {
            _sim.Reset();
            // _simulationThread.RunWorkerCompleted -= ExecuteReset;
        }


        public async void RequestUpdate(long frameTime)
        {
            var t = frameTime;
            
            await Task.Factory.StartNew(() =>
            {
                while (_simulationThreadIsBusy)
                {
                    Thread.Sleep(5);
                    t += 5;
                }
            });
                
            
            // Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => UpdateSimulationBackground(this, null)), DispatcherPriority.Background);

            _simulationThreadIsBusy = true;
            var result = await Task.Run(() =>
            {
                Thread.Sleep(10);
                UpdateSimulationBackground(this, new DoWorkEventArgs(t+10));
                return true;
            }); //Task.Factory.StartNew(() => UpdateSimulationBackground(this, null));
            _simulationThreadIsBusy = false;
            
        }

        private void UpdateSimulationBackground(object sender, DoWorkEventArgs e)
        {
#if DEBUG
            _numIterations++;
            _stopwatch.Start();
#endif
            _simulationThreadIsBusy = true;
            _sim.UpdateSimulation((long) e.Argument);
            _simulationThreadIsBusy = false;
#if DEBUG
            _stopwatch.Stop();
            var t = _stopwatch.Elapsed;
            _totalTime += t.Milliseconds;
            _averageTime = _totalTime/(float)_numIterations;
            _eventAggregator.GetEvent<PhysicsSimBenchmarkUpdated>()
                .Publish(new BenchmarkData
                {
                    AverageTime = _averageTime,
                    NumSamples = _numIterations,
                    TotalTime = _totalTime
                });
            _stopwatch.Reset();
#endif
        }
    }
}
