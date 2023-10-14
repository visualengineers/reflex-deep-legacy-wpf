using System.Windows;
using PhysicsSimulation.Model;
using PhysicsSimulation.Utilities;
using Prism.Mvvm;

namespace WPFPhysicsControlsLib.ViewModel
{
    public abstract class SimulatedItemVm : BindableBase
    {
        protected float SizeModifier = 2.0f;
        protected float MinSizeModifer = 0.25f;
        protected float MaxSizeModifier = 4.0f;
        protected float BaseSize = 10.0f;
        
        protected readonly PhysicalObjectState State;
        protected readonly SimulatedObject ObjectData;
        private float _transX, _transY;
        //private Thickness _margin = new Thickness(0);

        //public Thickness Margin { get { return _margin; } }

        public float TranslationX => _transX;
        public float TranslationY => _transY;

        public float ItemSize { get; protected set; }

        public float ObjectOpacity { get; protected set; }

        protected SimulatedItemVm(SimulatedObject obj)
        {
            ObjectData = obj;

            State = obj.State;
            UpdateTranslation();
        }

        private Vector2D ComputeTranslation()
        {
            if (State == null || State.CurrentPosition == null)
                return new Vector2D(0, 0);

            return State.CurrentPosition;
        }

        public void UpdateTranslation()
        {
            var updatedPosition = ComputeTranslation();
            _transX = updatedPosition.X - 0.5f*ItemSize;
            _transY = updatedPosition.Y - 0.5f*ItemSize;
            // _margin = new Thickness(_transX, _transY, 0, 0);

            RaisePropertyChanged(nameof(TranslationX));
            RaisePropertyChanged(nameof(TranslationY));
            // OnPropertyChanged(ReflectionUtility.GetMemberName(() => Margin));
        }

        public virtual void UpdateVisualization()
        {
            var factor = State.InfluenceFactor;

            var resultingSize = BaseSize + SizeModifier * factor;

            var minSize = MinSizeModifer * BaseSize;
            var maxSize = MaxSizeModifier * BaseSize;

            //clamp size to [minSize, maxSize]
            resultingSize = (resultingSize > maxSize)
                ? maxSize
                : (resultingSize < 0.25 * BaseSize) ? minSize : resultingSize;

            ObjectOpacity = factor;
            ItemSize = resultingSize;
            RaisePropertyChanged(nameof(ObjectOpacity));
            RaisePropertyChanged(nameof(ItemSize));
        }
    }
}
