using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Input;
using System.Windows.Media;
using DelVizDataStructure;
using EventHandling.Utilities;
using PhysicsSimulation;
using PhysicsSimulation.Events;
using PhysicsSimulation.Model;
using PhysicsSimulation.Utilities;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using WPFPhysicsControlsLib.Commands;
using WPFPhysicsControlsLib.Utilities;

namespace WPFPhysicsControlsLib.ViewModel
{
    public class SimulatedTagVm : SimulatedItemVm, IModifyItemInfluence
    {
        #region Fields

        private readonly IEventAggregator _eventAggregator;

        private readonly Dictionary<int, int> _tagDimensionRotation = new Dictionary<int, int>
        {
            {156, -250},
            {158, -180},
            {157, -145},
            {164, -90},
            {167, -60},
            {166, -20},
            {161, 20},
            {160, 60},
            {159, 60}
        };

        private float _influenceModifier;

        #endregion

        #region Properties

        public SimulatedTagObject Tag { get; private set; }

        public Color TagBaseColor { get; private set; }

        public Color TagColor { get; private set; }

        public Color TagColorTransparent { get; private set; }

        public String AssociatedDimensionName { get; private set; }
        public String AssociatedCategoryName { get; private set; }

        public double AssociatedDimensionRotation { get; private set; }

        public String Name
        {
            get { return Tag.Tag.Name; }
        }

        public float UserDefinedInfluence
        {
            get { return _influenceModifier; }
            set
            {
                if (Math.Abs(_influenceModifier - value) < Double.Epsilon)
                    return;
                _influenceModifier = value;
                Tag.State.InfluenceFactor = _influenceModifier;
                RaisePropertyChanged(nameof(UserDefinedInfluence));
            }
        }

        public ICommand ModifyTagAttractionCmd { get; private set; }

        #endregion

        public SimulatedTagVm(SimulatedTagObject obj)
            : base(obj)
        {
            Tag = obj;


            ModifyTagAttractionCmd = new DelegateCommand<object>(ModifyItemInfluence);

            UpdateBaseSize(ObjectType.Tag);
            SizeModifier = 1.0f;
            MinSizeModifer = 0.75f;
            MaxSizeModifier = 2.0f;

            if (!ColorDictionary.IsInitialized)
            {
                var dataRepo = ContainerLocator.Current.Resolve(typeof(DataRepository)) as DataRepository;

                if (dataRepo == null)
                    throw new NullReferenceException($"Cannot retrieve {typeof(DataRepository).FullName}. Source:{GetType().FullName}");


                ColorDictionary.Instance.InitializeDictionary(dataRepo.Categories);
            }

            _eventAggregator = ContainerLocator.Current.Resolve(typeof(IEventAggregator)) as IEventAggregator;

            if (_eventAggregator == null)
                throw new NullReferenceException($"Cannot retrieve {typeof(EventAggregator).FullName}. Source:{GetType().FullName}");

            _eventAggregator.GetEvent<ItemSizeChangedEvent>().Subscribe(UpdateBaseSize);

            AssociatedDimensionName = obj.Tag.AssociatedDimension.Name.ToUpper();
            AssociatedCategoryName = obj.Tag.AssociatedDimension.AssociatedCategory.Name.ToUpper();

            int rot = 0;
            var found = _tagDimensionRotation.TryGetValue(obj.Tag.AssociatedDimension.DimensionId, out rot);

            if (found)
                AssociatedDimensionRotation = rot;

            var col = ColorDictionary.Instance.GetColor(obj.Tag);
            TagBaseColor = col;
            TagColor = col;
            RaisePropertyChanged(nameof(Name));
            RaisePropertyChanged(nameof(TagBaseColor));
            RaisePropertyChanged(nameof(TagColor));
            RaisePropertyChanged(nameof(AssociatedDimensionName));
            RaisePropertyChanged(nameof(AssociatedDimensionRotation));
            RaisePropertyChanged(nameof(AssociatedCategoryName));

        }

        private void UpdateBaseSize(ObjectType type)
        {
            if (type == ObjectType.Tag)
                BaseSize = PhysicsSimulationProperties.TagBaseSize;
        }

        private void UpdateColor()
        {
            //TODO: parameterize --> currently optimized for _influenceModifier in range betweeen -7 and +7
            double[] colHSV = ColorConversion.RGB2HSV(TagBaseColor);
            var saturation = (50 + _influenceModifier*30)/255.0;
            saturation = saturation < 0 ? 0 : saturation > 1 ? 1 : saturation;
            var value = 0.5 + (_influenceModifier*0.05);
            value = value < 0 ? 0 : value > 1.0 ? 1.0 : value;
            colHSV[1] = saturation;
            colHSV[2] = value;

            var col = ColorConversion.HSV2RGB(colHSV);
            TagColor = col;
            TagColorTransparent = Color.FromArgb(0, TagColor.R, TagColor.G, TagColor.B);
            var factor = State.InfluenceFactor < 0 ? State.InfluenceFactor*0.10f : State.InfluenceFactor*0.05f;
            ObjectOpacity = 0.7f + factor;
            RaisePropertyChanged(nameof(TagColor));
            RaisePropertyChanged(nameof(ObjectOpacity));
        }

        public void ModifyItemInfluence(object amount)
        {
            if (!ConvertInfluenceExtension.TryGetInfluenceAmount(amount, out var influenceAmount))
                return;

            UserDefinedInfluence = (float)(_influenceModifier + influenceAmount);
            var clamp = PhysicsSimulationProperties.MaxUserInfluenceValue;
            UserDefinedInfluence = UserDefinedInfluence < -clamp
                ? -clamp
                : UserDefinedInfluence > clamp ? clamp : UserDefinedInfluence;

            _eventAggregator.GetEvent<ItemInfluenceChangedEvent>().Publish(ObjectType.Tag);
        }

        public override void UpdateVisualization()
        {
            base.UpdateVisualization();

            UpdateColor();
        }
    }
}
