using System;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EventHandling.Utilities;
using PhysicsSimulation.Events;
using PhysicsSimulation.Model;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using WPFPhysicsControlsLib.Utilities;

namespace WPFPhysicsControlsLib.ViewModel
{
    public class SimulatedContentItemVm : SimulatedItemVm, IModifyItemInfluence
    {
        private readonly IEventAggregator _eventAggregator;

        private float _itemSelection;

        public SimulatedContentObject Object { get; private set; }

        public float Transparency { get; private set; }

        public Color ItemBaseColor { get; private set; }

        public Color ItemColor { get; private set; }

        public ImageSource ImgSource { get; private set; }

        public float ImgTransparency { get; private set; }

        public float BlurRadius { get; private set; }

        public float ItemSelectionAmount
        {
            get { return _itemSelection; } 
            set { _itemSelection = value; }
        }

        public DetailsSelectionState SelectionState { get; private set; }

        public ICommand ModifyItemInfluenceCmd { get; private set; }

        public float[] Categories { get; } =
        {
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0
        };

        public SimulatedContentItemVm(SimulatedContentObject obj) : base(obj)
        {
            Object = obj;

            MinSizeModifer = 0.25f;
            MaxSizeModifier = 8.0f;
            SizeModifier = 4.0f;
            UpdateBaseSize(ObjectType.Content);
            Transparency = 0.6f; 
            
            _eventAggregator = ContainerLocator.Current.Resolve(typeof(IEventAggregator)) as IEventAggregator;

            if (_eventAggregator == null)
                throw new NullReferenceException($"Cannot retrieve {typeof(EventAggregator).FullName}. Source:{GetType().FullName}");

            _eventAggregator.GetEvent<ItemSizeChangedEvent>().Subscribe(UpdateBaseSize);
            ItemBaseColor = Color.FromArgb(255, 127, 127, 127);

            ImgSource = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}{obj.Data.ImageLocation}"));
            ImgSource.Freeze();
            RaisePropertyChanged(nameof(ImgSource));

            ModifyItemInfluenceCmd = new DelegateCommand<double?>(ModifyItemInfluence);

            if (obj.AssociatedCategories == null || obj.AssociatedCategories.Length < Categories.Length)
                return;

            for (var i = 0; i < Categories.Length; i++)
            {
                if (obj.AssociatedCategories[i] > 0)
                {
                    Categories[i] = obj.AssociatedCategories[i] / 3f;
                }
            }

            RaisePropertyChanged(nameof(Categories));
        }

        private void UpdateBaseSize(ObjectType type)
        {
            if (type == ObjectType.Content)
                BaseSize = PhysicsSimulationProperties.ItemBaseSize;
        }

        private void UpdateTagInfluences()
        {
            var colValueSum = new[] {0.0, 0.0, 0.0};
            var influenceSum = 0.0;
            var data = ObjectData as SimulatedContentObject;
            if (data == null)
                return;
            
            var influences = data.InfluencingTags;

            foreach (var influence in influences)
            {
                var col = ColorDictionary.Instance.GetColor(influence.Tag);
                var value = influence.State.InfluenceFactor;

                colValueSum[0] += col.R*value;
                colValueSum[1] += col.G*value;
                colValueSum[2] += col.B*value;
                influenceSum += value;
            }

            if (influenceSum <= 0 || influences.Count == 0)
            {
                ItemColor = ItemBaseColor;
            }
            else
            {

                colValueSum[0] /= 2.0*influenceSum;
                colValueSum[1] /= 2.0*influenceSum;
                colValueSum[2] /= 2.0*influenceSum;

                colValueSum[0] += ItemBaseColor.R;
                colValueSum[1] += ItemBaseColor.G;
                colValueSum[2] += ItemBaseColor.B;

                colValueSum[0] = colValueSum[0] < 0 ? 0 : colValueSum[0] > 255 ? 255 : colValueSum[0];
                colValueSum[1] = colValueSum[1] < 0 ? 0 : colValueSum[1] > 255 ? 255 : colValueSum[1];
                colValueSum[2] = colValueSum[2] < 0 ? 0 : colValueSum[2] > 255 ? 255 : colValueSum[2];

                ItemColor = Color.FromArgb(255,
                    Convert.ToByte((int) colValueSum[0]),
                    Convert.ToByte((int) colValueSum[1]),
                    Convert.ToByte((int) colValueSum[2]));
            }

            RaisePropertyChanged(nameof(ItemColor));
        }


        public override void UpdateVisualization()
        {
            base.UpdateVisualization();
            var factor = State.InfluenceFactor*0.1f;
            var trans = 0.3f + factor;
            var imgTrans = factor;

            trans = (trans > 1) ? 1.0f : (trans < 0.1f) ? 0.1f : trans;
            Transparency = trans;

            imgTrans = (imgTrans > 0) ? 1.0f : (imgTrans < 0) ? 0 : imgTrans;
            ImgTransparency = imgTrans;

            RaisePropertyChanged(nameof(ImgTransparency));
            RaisePropertyChanged(nameof(Transparency));

            BlurRadius = 0.2f + trans*0.6f;
            RaisePropertyChanged(nameof(BlurRadius));

            UpdateTagInfluences();
        }

        public void ModifyItemInfluence(double? amount)
        {
            if (amount == null)
                return;

            _itemSelection = (float)amount;
            _itemSelection = _itemSelection < 0
                ? 0
                : _itemSelection > PhysicsSimulationProperties.MaxUserInfluenceValue
                    ? PhysicsSimulationProperties.MaxUserInfluenceValue
                    : _itemSelection;

            SelectionState = _itemSelection <= 0f
                ? DetailsSelectionState.NotSelected
                : _itemSelection < 5f
                    ? DetailsSelectionState.ShowName
                    : _itemSelection < 6f ? DetailsSelectionState.ShowImage : DetailsSelectionState.FullDescription;

            RaisePropertyChanged(nameof(SelectionState));
            RaisePropertyChanged(nameof(ItemSelectionAmount));

            _eventAggregator.GetEvent<ItemInfluenceChangedEvent>().Publish(ObjectType.Content);
        }
    }
}
