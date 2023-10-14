using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Prism.Ioc;
using WPFPhysicsControlsLib.Utilities;
using WPFPhysicsControlsLib.ViewModel;

namespace WPFPhysicsControlsLib.DrawingVisuals
{
    public abstract class VisualHostBase : FrameworkElement
    {
        protected readonly VisualCollection _visualChildren;
        protected readonly SimulationVm _data;
        protected bool _isInvalidated;
        protected bool _visualChildrenCreated = false;

        protected VisualHostBase()
        {
            _visualChildren = new VisualCollection(this);
            _data = ContainerLocator.Current.Resolve<SimulationVm>();
        }

        protected abstract void CreateVisualChildren();

        protected abstract void UpdateVisualChildren(DrawingContext dc);

        public abstract void InitStaticResources();

        protected override int VisualChildrenCount
        {
            get
            {
                return _visualChildren.Count;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _visualChildren.Count)
                throw new ArgumentOutOfRangeException("index");

            return _visualChildren[index];
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (!_visualChildrenCreated)
            {
                CreateVisualChildren();
                _visualChildrenCreated = true;
                return;
            }

            UpdateVisualChildren(dc);
        }

        protected override void OnToolTipOpening(ToolTipEventArgs e)
        {
            //HitTestResult result = VisualTreeHelper.HitTest(this, Mouse.GetPosition(this));

            //if (result.VisualHit is DrawingVisualDescription)
            //{
            //    DrawingVisualDescription drawingVisual = result.VisualHit as DrawingVisualDescription;
            //    VectorData vectorData = drawingVisual.VectorData;
            //    ToolTip = String.Format("{0}, X={1}, Y={2}", vectorData.Id, vectorData.Start, vectorData.End);
            //}
            base.OnToolTipOpening(e);
        }

        protected override void OnToolTipClosing(ToolTipEventArgs e)
        {
            ToolTip = "";
            base.OnToolTipClosing(e);
        }
    }
}
