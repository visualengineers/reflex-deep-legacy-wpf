using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Threading;
using PhysicsSimulation;
using PhysicsSimulation.Events;
using PhysicsSimulation.Model;
using PhysicsSimulation.Utilities;
using Prism.Events;
using Prism.Ioc;
using WPFPhysicsControlsLib.ViewModel;

namespace WPFPhysicsControlsLib.Utilities
{
    public class PhysicsSimulationTimer
    {
        private static DispatcherTimer _timer;
        private static Stopwatch _stopwatch;
        private static bool useTimer = false;
        private static BackgroundSimulationTask _backgroundSim;
        private static long _totalEllapsedMiliseconds;

        public static void Start()
        {
            _backgroundSim = new BackgroundSimulationTask(ContainerLocator.Current.Resolve<SimulationVm>().PhysicsSim);

            var eventAggregator = ContainerLocator.Current.Resolve<IEventAggregator>();

            if (eventAggregator == null)
                throw new NullReferenceException($"Cannot retrieve {typeof(EventAggregator).FullName}. Source:{typeof(PhysicsSimulationTimer).FullName}");

            if (useTimer)
            {
                _timer = new DispatcherTimer(DispatcherPriority.DataBind)
                {
                    Interval = TimeSpan.FromMilliseconds(PhysicsSimulationProperties.SimulationTimerInterval)
                };
                _timer.Tick += (o, args) => _backgroundSim.RequestUpdate(_timer.Interval.Milliseconds);
                _timer.Start();

                eventAggregator.GetEvent<TimerIntervalChanged>()
                    .Subscribe(ChangeTimerInterval);
            }
            else
            {
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
                _totalEllapsedMiliseconds = _stopwatch.ElapsedMilliseconds;
                eventAggregator.GetEvent<RequestSimulationUpdateEvent>().Subscribe((i =>
                {
                    OnUpdateRequested(_backgroundSim);
                }), ThreadOption.BackgroundThread);

                OnUpdateRequested(_backgroundSim);
            }
        }

        private static void OnUpdateRequested(BackgroundSimulationTask backgroundSim)
        {
            _stopwatch.Stop();
            long time = _stopwatch.ElapsedMilliseconds - _totalEllapsedMiliseconds;
            backgroundSim.RequestUpdate(time);
            _stopwatch.Start();
            _totalEllapsedMiliseconds = _stopwatch.ElapsedMilliseconds;
        }

        private static void ChangeTimerInterval(int numMilliseconds)
        {
            _timer.Stop();
            _timer.Interval = TimeSpan.FromMilliseconds(numMilliseconds);
            _timer.Start();
        }
    }
}
