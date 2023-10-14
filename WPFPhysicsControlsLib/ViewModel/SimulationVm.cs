using EventHandling.Utilities;
using PhysicsSimulation.Events;
using PhysicsSimulation.Model;
using PhysicsSimulation.Utilities;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using NLog;
using ReFlex.Core.Common.Components;
using WPFPhysicsControlsLib.Commands;
using WPFPhysicsControlsLib.Utilities;
using Math = System.Math;

namespace WPFPhysicsControlsLib.ViewModel
{
    public class SimulationVm : BindableBase
    {
        private readonly Logger _logger;
        private readonly PhysicsSimulator _sim;
        private bool _showTPs;

        private readonly IEventAggregator _eventAggregator;

        public List<SimulatedContentItemVm> Objects { get; protected set; }

        public List<SimulatedTagVm> Tags { get; protected set; }

        public List<SimulatedTagVm> ActiveTags { get; protected set; }

        public List<SimulatedContentItemVm> SelectedObjects { get; protected set; }

        public ObservableCollection<Vector2D> CurrentlyDetectedTouchPoints { get; } = new ObservableCollection<Vector2D>();

        public bool ShowTouchPoints
        {
            get => _showTPs;
            set
            {
                if (_showTPs == value)
                    return;
                _showTPs = value;
                RaisePropertyChanged();
            }
        }

        public PhysicsSimulator PhysicsSim => _sim;

        public ICommand RequestResetCommand { get; }

        public ResetItemInfluencesCommand ResetTagInfluencesCmd { get; }

        public ResetItemInfluencesCommand ResetContentItemInfluencesCmd { get; }

        public DetailsVisualizationViewModel DetailsVm { get; }

        public BenchmarkViewModel BenchmarkVm { get; }

        public int SimulationTimeStamp { get; protected set; }

        public SimulationVm(Logger logger, IEventAggregator eventAggregator)
        {
            _logger = logger;
            _eventAggregator = eventAggregator;

            _eventAggregator.GetEvent<SimulationUpdatedEvent>().Subscribe(HandleSimulationUpdate);
            _eventAggregator.GetEvent<ObjectsInitializedEvent>().Subscribe(ObjectsInitialized);
            _eventAggregator.GetEvent<ItemInfluenceChangedEvent>()
                .Subscribe(UpdateResetCommandState);
            _eventAggregator.GetEvent<ItemInfluenceResetRequestedEvent>()
                .Subscribe(ResetItemInfluences);

            Objects = new List<SimulatedContentItemVm>();
            Tags = new List<SimulatedTagVm>();
            SelectedObjects = new List<SimulatedContentItemVm>();
            ActiveTags = new List<SimulatedTagVm>();
            _sim = new PhysicsSimulator();
            _sim.Reset();

            RequestResetCommand = new RequestResetCommand();
            ResetTagInfluencesCmd = new ResetItemInfluencesCommand(ObjectType.Tag);
            ResetContentItemInfluencesCmd = new ResetItemInfluencesCommand(ObjectType.Content);

            DetailsVm = new DetailsVisualizationViewModel();
            BenchmarkVm = new BenchmarkViewModel(this);

            _logger.Info($"{GetType().FullName}: Initialization completed.");

        }

        public void UpdateCurrentlyDetectedTouchPoints(List<Interaction> touchPoints)
        {
            if (!ShowTouchPoints)
                return;

            Application.Current?.Dispatcher?.BeginInvoke(DispatcherPriority.Normal, (Action) (() =>
            {
                CurrentlyDetectedTouchPoints.Clear();
                touchPoints.ForEach(tp =>
                    CurrentlyDetectedTouchPoints.Add(new Vector2D(tp.Position.X, tp.Position.Y)));
            }));
        }

        private void HandleSimulationUpdate(int idx)
        {
            SimulatedContentItemVm detailsSelection = null;
            var maxDetails = 3.0;

            var selObj = new List<SimulatedContentItemVm>();
            var numObj = SelectedObjects.Count(vm => vm.SelectionState != DetailsSelectionState.NotSelected);

            if (numObj != 0 && !PhysicsSim.IsPaused)
                _eventAggregator.GetEvent<SimulationPauseRequestedEvent>().Publish(new Tuple<bool, bool>(true, false));

            if (numObj == 0 && PhysicsSim.IsPaused)
                _eventAggregator.GetEvent<SimulationPauseRequestedEvent>().Publish(new Tuple<bool, bool>(false, false));


            //TODO: not needed anymore, grab positions directly from Simulation ???
            Objects.ForEach(vm =>
            {
                vm.UpdateTranslation();
                vm.UpdateVisualization();

                if (vm.ItemSelectionAmount > 0)
                {
                    selObj.Add(vm);

                    if (vm.ItemSelectionAmount > maxDetails)
                    {
                        maxDetails = vm.ItemSelectionAmount;
                        detailsSelection = vm;
                    }
                }
            });

            var remObj = SelectedObjects.Where(vm => !selObj.Contains(vm)).ToList();
            var addObj = selObj.Where(vm => !SelectedObjects.Contains(vm)).ToList();

            if (remObj.Count > 0 || addObj.Count > 0)
            {
                Application.Current?.Dispatcher?.BeginInvoke(DispatcherPriority.Send, new Action(() =>
                {
                    remObj.ForEach(vm => SelectedObjects.Remove(vm));
                    addObj.ForEach(vm => SelectedObjects.Add(vm));
                }));
            }

            var selTags = new List<SimulatedTagVm>();

            Tags.ForEach(vm =>
            {
                vm.UpdateTranslation();
                vm.UpdateVisualization();
                if (vm.UserDefinedInfluence > 0)
                {
                    selTags.Add(vm);
                }
            });

            var remTags = ActiveTags.Where(vm => !selTags.Contains(vm)).ToList();
            var addTags = selTags.Where(vm => !ActiveTags.Contains(vm)).ToList();

            if (remTags.Count > 0 || addTags.Count > 0)
            {
                Application.Current?.Dispatcher?.BeginInvoke(DispatcherPriority.Send, new Action(() =>
                {
                    remTags.ForEach(vm => ActiveTags.Remove(vm));
                    addTags.ForEach(vm => ActiveTags.Add(vm));
                }));
            }

            DetailsVm.CurrentlyActiveItem = detailsSelection;

            SimulationTimeStamp = idx;
            RaisePropertyChanged(nameof(SimulationTimeStamp));
        }

        private void InitCollections()
        {
            Objects.Clear();
            Tags.Clear();
            _sim.Objects.ForEach(obj => Objects.Add(new SimulatedContentItemVm(obj)));

            foreach (var tag in _sim.Tags)
            {
                Tags.Add(new SimulatedTagVm(tag));
            }
        }

        private void ObjectsInitialized(bool completeRefresh)
        {
            if (completeRefresh)
            {
                InitCollections();
            }
        }

        private void UpdateResetCommandState(ObjectType type)
        {
            Application.Current?.Dispatcher?.BeginInvoke(DispatcherPriority.Background, new Action(() => {
                var noInfluences = true;

                if (type == ObjectType.Tag)
                {
                    foreach (var tagVm in Tags)
                    {
                        if (Math.Abs(tagVm.UserDefinedInfluence) > Double.Epsilon)
                            noInfluences = false;
                    }

                    ResetTagInfluencesCmd.TagInfluenceResetEnabled = !noInfluences;
                }
                else if (type == ObjectType.Content)
                {
                    foreach (var objVm in Objects)
                    {
                        if (Math.Abs(objVm.ItemSelectionAmount) > Double.Epsilon)
                            noInfluences = false;
                    }


                    ResetContentItemInfluencesCmd.TagInfluenceResetEnabled = !noInfluences;
                }
            }));
        }

        private void ResetItemInfluences(ObjectType itemType)
        {
            Application.Current?.Dispatcher?.BeginInvoke(DispatcherPriority.Background, new Action(() => {

                if (itemType == ObjectType.Tag)
                {
                    foreach (var tagVm in Tags)
                    {
                        tagVm.UserDefinedInfluence = 0;
                    }
                }

                if (itemType == ObjectType.Content)
                {
                    foreach (var itemVm in Objects)
                    {
                        itemVm.ModifyItemInfluence(-itemVm.ItemSelectionAmount);
                    }
                }
            }));
        }

        public void RequestUpdate()
        {
            _eventAggregator.GetEvent<RequestSimulationUpdateEvent>().Publish(0);
        }

        public SimulatedTagVm[] AcquireCopyOfTagList()
        {
            lock (Tags)
                return Tags.ToArray();
        }

        public SimulatedContentItemVm[] AcquireCopyOfEntitiesList()
        {
            lock (Objects)
                return Objects.ToArray();
        }
    }
}
