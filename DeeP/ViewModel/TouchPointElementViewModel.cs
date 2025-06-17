using System.Windows;
using Prism.Mvvm;
using ReFlex.Core.Common.Components;
using Math = System.Math;

namespace ReFlex.Apps.DeeP.ViewModel
{
    public class TouchPointElementViewModel : BindableBase
    {
        private double _canvasWidth = 800;
        private double _canvasHeight = 600;
        public Interaction Interaction { get; }

        public double CanvasWidth
        {
            get => _canvasWidth;
            set
            {
                if (Equals(_canvasWidth, value))
                    return;
                _canvasWidth = value;
                RaisePropertyChanged(nameof(CanvasWidth));
                RaisePropertyChanged(nameof(PosX));
                RaisePropertyChanged(nameof(PosY));
                RaisePropertyChanged(nameof(Size));
            }
        }

        public double CanvasHeight
        {
            get => _canvasHeight;
            set
            {
                if (Equals(_canvasHeight, value))
                    return;
                _canvasHeight = value;
                RaisePropertyChanged(nameof(CanvasHeight));
                RaisePropertyChanged(nameof(PosX));
                RaisePropertyChanged(nameof(PosY));
                RaisePropertyChanged(nameof(Size));
            }
        }

        public double PosX => Interaction?.Position?.X * _canvasWidth ?? 0;

        public double PosY => Interaction?.Position?.Y * _canvasHeight ?? 0;

        public double Size => Math.Abs(Interaction?.Position?.Z ?? 0) * 0.01 * _canvasHeight;

        public bool IsPush => Interaction?.Position?.Z > 0;

        public int TouchId => Interaction?.TouchId ?? -1;

        public TouchPointElementViewModel(Interaction interaction, Size canvasSize)
        {
            Interaction = interaction;
            CanvasHeight = canvasSize.Height;
            CanvasWidth = canvasSize.Width;
            RaisePropertyChanged(nameof(PosX));
            RaisePropertyChanged(nameof(PosY));
            RaisePropertyChanged(nameof(Size));
            RaisePropertyChanged(nameof(IsPush));
            RaisePropertyChanged(nameof(TouchId));
        }
    }
}
