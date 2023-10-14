using System;
using PhysicsSimulation;
using PhysicsSimulation.Events;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using WPFPhysicsControlsLib.Utilities;

namespace WPFPhysicsControlsLib.ViewModel
{
    public class BenchmarkViewModel : BindableBase
    {
        private readonly SimulationVm _simVm;

        private long _frameTime;
        private long _totalFrametime;
        private float _fps;
        private float _totalFps;
        private string _createTags = " - ";
        private long _createEntities;
        private string _createLines = " - ";
        private string _layoutUpdate = " - ";
        private int _numUpdates;
        
        public int NumTags { get; private set; }
        public int NumEntities { get; private set; }
        public int NumLines { get; private set; }

        public long FrameTime
        {
            get => _frameTime;
            set
            {
                if (Equals(_frameTime, value))
                    return;

                _frameTime = value;
                if (_numUpdates > 0)
                {
                    _totalFrametime += _frameTime;
                    AverageFrameTime = (float) _totalFrametime/_numUpdates;
                    RaisePropertyChanged(nameof(AverageFrameTime));
                }

                RaisePropertyChanged();
            }
        }

        public float AverageFrameTime { get; private set; }

        public float Fps
        {
            get => _fps;
            set
            {
                if (Equals(_fps, value))
                    return;

                _fps = value;
                _numUpdates++;
                _totalFps += _fps;
                AverageFps = _totalFps/_numUpdates;

                RaisePropertyChanged();
                RaisePropertyChanged(nameof(AverageFps));
            }
        }

        public float AverageFps { get; private set; }


        public string CreateTags
        {
            get => _createTags;
            set
            {
                if (Equals(_createTags, value))
                    return;

                _createTags = value;
                RaisePropertyChanged();
            }
        }

        public long CreateEntities
        {
            get => _createEntities;
            set
            {
                if (Equals(_createEntities, value))
                    return;

                _createEntities = value;
                RaisePropertyChanged();
            }
        }

        public string CreateLines
        {
            get => _createLines;
            set
            {
                if (Equals(_createLines, value))
                    return;

                _createLines = value;
                RaisePropertyChanged();
            }
        }

        public string LayoutUpdate
        {
            get => _layoutUpdate;
            set
            {
                if (Equals(_layoutUpdate, value))
                    return;

                _layoutUpdate = value;
                RaisePropertyChanged();
            }
        }

        public string PhysicsNumIterations { get; private set; }

        public string PhysicsTotalTime { get; private set; }

        public string PhysicsAverageTime { get; private set; }

        public BenchmarkViewModel(SimulationVm simVm)
        {
            _simVm = simVm;

            var eventAggregator = ContainerLocator.Current.Resolve<IEventAggregator>();

            if (eventAggregator == null)
                throw new NullReferenceException($"Cannot retrieve {typeof(EventAggregator).FullName}. Source:{GetType().FullName}");

            eventAggregator.GetEvent<ObjectsInitializedEvent>().Subscribe((args) =>
            {
                NumTags = _simVm.Tags.Count;
                NumEntities = _simVm.Objects.Count;
                RaisePropertyChanged(nameof(NumTags));
                RaisePropertyChanged(nameof(NumEntities));
            });
            

            eventAggregator.GetEvent<PhysicsSimBenchmarkUpdated>()
                .Subscribe(OnPhysicsSimBenchmarkUpdated);
        }

        private void OnPhysicsSimBenchmarkUpdated(BenchmarkData data)
        {
            PhysicsNumIterations = $"{data.NumSamples}";
            PhysicsAverageTime = $"{data.AverageTime:##.###}";
            PhysicsTotalTime = $"{data.TotalTime}";
            
            NumLines = _simVm.PhysicsSim.RelevantAssociations.Count;

            RaisePropertyChanged(nameof(PhysicsNumIterations));
            RaisePropertyChanged(nameof(PhysicsAverageTime));
            RaisePropertyChanged(nameof(PhysicsTotalTime));
            RaisePropertyChanged(nameof(NumLines));
        }
    }
}
