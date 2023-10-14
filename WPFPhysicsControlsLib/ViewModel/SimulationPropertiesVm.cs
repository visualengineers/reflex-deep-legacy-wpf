using System;
using System.Windows.Input;
using DelVizDataStructure;
using EventHandling.Utilities;
using NLog;
using PhysicsSimulation.Events;
using PhysicsSimulation.Model;
using Prism.Events;
using Prism.Mvvm;
using WPFPhysicsControlsLib.Commands;

namespace WPFPhysicsControlsLib.ViewModel
{
    public class SimulationPropertiesVm : BindableBase
    {
        #region Fields

        private readonly IEventAggregator _eventAggregator;

        private readonly DataRepository _dataRepo;
        private readonly Logger _logger;

        private static readonly float ModifierScale = 100.0f;
        private static readonly float SpeedModifierScale = 1000.0f;
        private bool _forceRatioLocked;
        private float _forceRatio;
        private int _tagCollisionRatio = 50; 
        private int _itemCollisionRatio = 35;
        
        #endregion

        #region Properties

        public float ObjectForce
        {
            get => PhysicsSimulationProperties.ObjectForceModifier / ModifierScale;
            set
            {
                var val = value * ModifierScale;
                if (Math.Abs(PhysicsSimulationProperties.ObjectForceModifier - val) < float.Epsilon)
                    return;

                PhysicsSimulationProperties.ObjectForceModifier = val;
                RaisePropertyChanged(nameof(ObjectForce));
                UpdateRatio(true);
            }
        }

        public float TagForce
        {
            get => PhysicsSimulationProperties.TagForceModifier;
            set
            {
                var val = value;
                if (Math.Abs(PhysicsSimulationProperties.TagForceModifier - val) < float.Epsilon)
                    return;

                PhysicsSimulationProperties.TagForceModifier = val;
                RaisePropertyChanged(nameof(TagForce));
            }
        }

        public float TagItemRepulsion
        {
            get => PhysicsSimulationProperties.TagItemRepulsionModifier * ModifierScale;
            set
            {
                if (Math.Abs(PhysicsSimulationProperties.TagItemRepulsionModifier - value / ModifierScale) < float.Epsilon)
                    return;

                PhysicsSimulationProperties.TagItemRepulsionModifier = value / ModifierScale;
                RaisePropertyChanged(nameof(TagItemRepulsion));
            }
        }

        public float BoundaryForce
        {
            get => PhysicsSimulationProperties.BoundaryForceModifier/ModifierScale;
            set
            {
                var val = value*ModifierScale;
                if (Math.Abs(PhysicsSimulationProperties.BoundaryForceModifier - val) < float.Epsilon)
                    return;

                PhysicsSimulationProperties.BoundaryForceModifier = val;
                RaisePropertyChanged(nameof(BoundaryForce));
                UpdateRatio(false);
            }
        }
        public String ObjectToBoundaryForceRatio => String.Format("{0: 00.000}", _forceRatio);

        public bool IsForceRatioLocked
        {
            get => _forceRatioLocked;
            set
            {
                if (_forceRatioLocked == value)
                    return;
                _forceRatioLocked = value;
                RaisePropertyChanged(nameof(IsForceRatioLocked));
            }
        }

        public float ItemCanvasWidth
        {
            get => PhysicsSimulationProperties.ItemsCanvasWidth;
            set
            {
                if (Math.Abs(PhysicsSimulationProperties.ItemsCanvasWidth - value) < float.Epsilon)
                    return;

                PhysicsSimulationProperties.ItemsCanvasWidth = value;
                RaisePropertyChanged(nameof(ItemCanvasWidth));
                _eventAggregator.GetEvent<CanvasSizeChangedEvent>().Publish(true);
            }
        }

        public float ItemCanvasHeight
        {
            get => PhysicsSimulationProperties.ItemsCanvasHeight;
            set
            {
                if (Math.Abs(PhysicsSimulationProperties.ItemsCanvasHeight - value) < float.Epsilon)
                    return;

                PhysicsSimulationProperties.ItemsCanvasHeight = value;
                RaisePropertyChanged(nameof(ItemCanvasHeight));
                _eventAggregator.GetEvent<CanvasSizeChangedEvent>().Publish(true);
            }
        }

        public float TagCanvasWidth
        {
            get => PhysicsSimulationProperties.TagsCanvasWidth;
            set
            {
                if (Math.Abs(PhysicsSimulationProperties.TagsCanvasWidth - value) < float.Epsilon)
                    return;

                PhysicsSimulationProperties.TagsCanvasWidth = value;
                RaisePropertyChanged(nameof(TagCanvasWidth));
                _eventAggregator.GetEvent<CanvasSizeChangedEvent>().Publish(true);
            }
        }

        public float TagCanvasHeight
        {
            get => PhysicsSimulationProperties.TagsCanvasHeight;
            set
            {
                if (Math.Abs(PhysicsSimulationProperties.TagsCanvasHeight - value) < float.Epsilon)
                    return;

                PhysicsSimulationProperties.TagsCanvasHeight = value;
                RaisePropertyChanged(nameof(TagCanvasHeight));
                _eventAggregator.GetEvent<CanvasSizeChangedEvent>().Publish(true);
            }
        }

        public float TagForceFieldDiameter
        {
            get => PhysicsSimulationProperties.TagSize;
            set
            {
                if (Math.Abs(PhysicsSimulationProperties.TagSize - value) < float.Epsilon)
                    return;

                PhysicsSimulationProperties.TagSize = value;
                RaisePropertyChanged(nameof(TagForceFieldDiameter));
                _eventAggregator.GetEvent<CanvasSizeChangedEvent>().Publish(true);
            }
        }

        public float TagBaseSize
        {
            get => PhysicsSimulationProperties.TagBaseSize;
            set
            {
                if (Math.Abs(PhysicsSimulationProperties.TagBaseSize - value) < float.Epsilon)
                    return;

                PhysicsSimulationProperties.TagBaseSize = value;
                RaisePropertyChanged(nameof(TagBaseSize));
                _eventAggregator.GetEvent<ItemSizeChangedEvent>().Publish(ObjectType.Tag);
            }
        }

        public float ItemForceFieldDiameter
        {
            get => PhysicsSimulationProperties.ItemSize;
            set
            {
                if (Math.Abs(PhysicsSimulationProperties.ItemSize - value) < float.Epsilon)
                    return;

                PhysicsSimulationProperties.ItemSize = value;
                RaisePropertyChanged(nameof(ItemForceFieldDiameter));
                _eventAggregator.GetEvent<CanvasSizeChangedEvent>().Publish(true);
            }
        }

        public float ItemBaseSize
        {
            get => PhysicsSimulationProperties.ItemBaseSize;
            set
            {
                if (Math.Abs(PhysicsSimulationProperties.ItemBaseSize - value) < float.Epsilon)
                    return;

                PhysicsSimulationProperties.ItemBaseSize = value;
                RaisePropertyChanged(nameof(ItemBaseSize));
                _eventAggregator.GetEvent<ItemSizeChangedEvent>().Publish(ObjectType.Content);
            }
        }

        public float StandardTagAttraction
        {
            get => PhysicsSimulationProperties.StandardTagAttraction * ModifierScale;
            set
            {
                if (Math.Abs(PhysicsSimulationProperties.StandardTagAttraction - value / ModifierScale) < float.Epsilon)
                    return;

                PhysicsSimulationProperties.StandardTagAttraction = value / ModifierScale;
                RaisePropertyChanged(nameof(StandardTagAttraction));
                _eventAggregator.GetEvent<InterTagForcesChangedEvent>().Publish(true);
            }
        }

        public float StandardTagRepulsion
        {
            get => PhysicsSimulationProperties.StandardTagRepulsion * ModifierScale;
            set
            {
                if (Math.Abs(PhysicsSimulationProperties.StandardTagRepulsion - value / ModifierScale) < float.Epsilon)
                    return;

                PhysicsSimulationProperties.StandardTagRepulsion = value / ModifierScale;
                RaisePropertyChanged(nameof(StandardTagRepulsion));
                _eventAggregator.GetEvent<InterTagForcesChangedEvent>().Publish(true);
            }
        }

        public float TagSameCategoryForceModifier
        {
            get => PhysicsSimulationProperties.TagSameCategoryForceModifier * ModifierScale;
            set
            {
                if (Math.Abs(PhysicsSimulationProperties.TagSameCategoryForceModifier - value / ModifierScale) < float.Epsilon)
                    return;

                PhysicsSimulationProperties.TagSameCategoryForceModifier = value / ModifierScale;
                RaisePropertyChanged(nameof(TagSameCategoryForceModifier));
                _eventAggregator.GetEvent<InterTagForcesChangedEvent>().Publish(true);
            }
        }

        public float TagSameDimensionForceModifier
        {
            get => PhysicsSimulationProperties.TagSameDimensionForceModifier * ModifierScale;
            set
            {
                if (Math.Abs(PhysicsSimulationProperties.TagSameDimensionForceModifier - value / ModifierScale) < float.Epsilon)
                    return;

                PhysicsSimulationProperties.TagSameDimensionForceModifier = value / ModifierScale;
                RaisePropertyChanged(nameof(TagSameDimensionForceModifier));
            }
        }

        public float SelectedTagAttractionModifier
        {
            get => PhysicsSimulationProperties.SelectedTagAttributionModifier;
            set
            {
                if (Math.Abs(PhysicsSimulationProperties.SelectedTagAttributionModifier - value) < float.Epsilon)
                    return;

                PhysicsSimulationProperties.SelectedTagAttributionModifier = value;
                RaisePropertyChanged(nameof(SelectedTagAttractionModifier));
            }
        }

        public float UnselectedTagRepulsionModifier
        {
            get => PhysicsSimulationProperties.UnselectedTagRepulsionModifier;
            set
            {
                if (Math.Abs(PhysicsSimulationProperties.UnselectedTagRepulsionModifier - value) < float.Epsilon)
                    return;

                PhysicsSimulationProperties.UnselectedTagRepulsionModifier = value;
                RaisePropertyChanged(nameof(UnselectedTagRepulsionModifier));
            }
        }

        public bool ComputeRepulsionModifierUnselectedTag
        {
            get => PhysicsSimulationProperties.ComputeRepulsionUnselected;
            set
            {
                if (PhysicsSimulationProperties.ComputeRepulsionUnselected == value)
                    return;

                PhysicsSimulationProperties.ComputeRepulsionUnselected = value;
                RaisePropertyChanged(nameof(ComputeRepulsionModifierUnselectedTag));
            }
        }

        public int SimulationInterval
        {
            get => PhysicsSimulationProperties.SimulationTimerInterval;
            set
            {
                if (PhysicsSimulationProperties.SimulationTimerInterval == value)
                    return;

                PhysicsSimulationProperties.SimulationTimerInterval = value;
                _eventAggregator.GetEvent<TimerIntervalChanged>().Publish(value);
                RaisePropertyChanged(nameof(SimulationInterval));
            }
        }

        public bool DetectCollisions
        {
            get => PhysicsSimulationProperties.DetectCollisions;
            set
            {
                if (PhysicsSimulationProperties.DetectCollisions == value)
                    return;

                PhysicsSimulationProperties.DetectCollisions = value;
                RaisePropertyChanged(nameof(DetectCollisions));
            }
        }

        public int CollisionDetectionIterations
        {
            get => PhysicsSimulationProperties.CollisionDetectionIterations;
            set
            {
                if (PhysicsSimulationProperties.CollisionDetectionIterations == value)
                    return;

                PhysicsSimulationProperties.CollisionDetectionIterations = value;
                RaisePropertyChanged(nameof(CollisionDetectionIterations));
            }
        }

        public float CentripetalSpeed
        {
            get => PhysicsSimulationProperties.CentripetalSpeed * SpeedModifierScale;
            set
            {
                if (Math.Abs(PhysicsSimulationProperties.CentripetalSpeed - value / SpeedModifierScale) < float.Epsilon)
                    return;
                PhysicsSimulationProperties.CentripetalSpeed = value / SpeedModifierScale;
                RaisePropertyChanged(nameof(CentripetalSpeed));
            }
        }

        public float MaxUserInfluenceValue
        {
            get => PhysicsSimulationProperties.MaxUserInfluenceValue;
            set
            {
                if (Math.Abs(PhysicsSimulationProperties.MaxUserInfluenceValue - value) < float.Epsilon)
                    return;
                PhysicsSimulationProperties.MaxUserInfluenceValue = value;
                RaisePropertyChanged(nameof(MaxUserInfluenceValue));
            }
        }

        public int ProcessDepthOption { get => PhysicsSimulationProperties.ProcessDepthOption;
            set
            {
                if (PhysicsSimulationProperties.ProcessDepthOption == value)
                    return;
                PhysicsSimulationProperties.ProcessDepthOption = value;
                RaisePropertyChanged(nameof(ProcessDepthOption));
            } }

        public int TagCollisionRatio
        {
            get => _tagCollisionRatio;
            set
            {
                if (_tagCollisionRatio == value)
                    return;
                _tagCollisionRatio = value;
                RaisePropertyChanged(nameof(TagCollisionRatio));
            }
        }

        public int ItemCollisionRatio
        {
            get => _itemCollisionRatio;
            set
            {
                if (_itemCollisionRatio == value)
                    return;
                _itemCollisionRatio = value;
                RaisePropertyChanged(nameof(ItemCollisionRatio));
            }
        }

        public bool EnforceEqualDataDistribution
        {
            get => PhysicsSimulationProperties.EnforceEqualDataDistribution;
            set
            {
                if (PhysicsSimulationProperties.EnforceEqualDataDistribution == value)
                    return;
                PhysicsSimulationProperties.EnforceEqualDataDistribution = value;
                RaisePropertyChanged(nameof(EnforceEqualDataDistribution));
            }
        }

        public String MaxNumItemsPerTag
        {
            get => $"{PhysicsSimulationProperties.MaxNumItemsPerTag:###}";
            set
            {
                int val = PhysicsSimulationProperties.MaxNumItemsPerTag;
                var success = Int32.TryParse(value, out val);
                if (!success || PhysicsSimulationProperties.MaxNumItemsPerTag == val)
                    return;
                PhysicsSimulationProperties.MaxNumItemsPerTag = val;
                _dataRepo.MaxNumItemsPerTag = val;
                RaisePropertyChanged(nameof(MaxNumItemsPerTag));
            }
        }

        public static ICommand ReloadCommand { get; private set; }

        #endregion

        #region Constructor

        public SimulationPropertiesVm(DataRepository dataRepo, Logger logger, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _dataRepo = dataRepo;
            _logger = logger;

            UpdateRatio(false);
            ReloadCommand = new ReloadDataCommand();

            _logger.Info($"{GetType().FullName}: Initialization completed.");
        }

        #endregion

        #region Auxiliary Methods

        private void UpdateRatio(bool objectForceChanged)
        {
            if (!_forceRatioLocked)
            {
                _forceRatio = BoundaryForce/ObjectForce;
                RaisePropertyChanged(nameof(ObjectToBoundaryForceRatio));
                return;
            }


            if (objectForceChanged)
            {
                PhysicsSimulationProperties.BoundaryForceModifier = PhysicsSimulationProperties.ObjectForceModifier * _forceRatio;
                RaisePropertyChanged(nameof(BoundaryForce));
            }
            else
            {
                PhysicsSimulationProperties.ObjectForceModifier = PhysicsSimulationProperties.BoundaryForceModifier/_forceRatio;
                RaisePropertyChanged(nameof(ObjectForce));
            }
        }

        #endregion
    }
}
