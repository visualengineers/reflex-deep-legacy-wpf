using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using PhysicsSimulation;
using PhysicsSimulation.Events;
using Prism.Events;
using Prism.Ioc;
using WPFPhysicsControlsLib.Utilities;
using WPFPhysicsControlsLib.ViewModel;

namespace WPFPhysicsControlsLib.DrawingVisuals
{
    public class VisualLinesHost : VisualHostBase
    {
        private static Pen _linePenWeak;
        private static Pen _linePenNormal;
        private static Pen _linePenStrong;

        private const bool UseDynamicLineThickness = false;

        private static bool _isInitialized;

        private readonly Stopwatch _stopwatch;

        private readonly SimulationVm _simVm;

        public VisualLinesHost()
        {
            _simVm = ContainerLocator.Current.Resolve<SimulationVm>();
            
            if (!_isInitialized)
                InitStaticResources();

            var eventAggregator = ContainerLocator.Current.Resolve<IEventAggregator>();
            
            if (eventAggregator == null)
                throw new NullReferenceException($"Cannot retrieve {typeof(EventAggregator).FullName}. Source:{GetType().FullName}");

            eventAggregator.GetEvent<SimulationUpdatedEvent>()
                .Subscribe(args => InvalidateVisual(), ThreadOption.UIThread);

#if DEBUG
            _stopwatch = new Stopwatch();
#endif
        }

        protected override void UpdateVisualChildren(DrawingContext dc)
        {
            _visualChildren.Clear();
            CreateVisualChildren();

            //@todo: not really a good place for requesting a simulation update...
            _simVm.RequestUpdate();
        }

        public override void InitStaticResources()
        {
            _linePenWeak = Application.Current.Resources["WeakPen"] as Pen;
            _linePenNormal = Application.Current.Resources["NormalPen"] as Pen;
            _linePenStrong = Application.Current.Resources["StrongPen"] as Pen;

            _isInitialized = true;
        }


        protected override void CreateVisualChildren()
        {
#if DEBUG
            _stopwatch.Start();
#endif

            _data.PhysicsSim.RelevantAssociations.ToList().ForEach(association =>
            {
                var start = association.AssociatedTagObject.State.CurrentPosition;
                var end = association.AssociatedContentItem.State.CurrentPosition;

                var pControl1 = association.CenterState.CurrentPosition;

                var pControl2 = association.DirectionState.CurrentPosition;

                // dc.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(25, 255, 255, 255)), 1), start, end);

                var bSeg = new BezierSegment(new Point(pControl1.X, pControl1.Y), new Point(pControl2.X, pControl2.Y),
                    new Point(end.X + 10, end.Y + 10), true);
                bSeg.Freeze();
                var segmentColl = new List<PathSegment> {bSeg};
                var pFig = new PathFigure(new Point(start.X, start.Y), segmentColl, false);
                pFig.Freeze();

                var pGeo = new PathGeometry(new List<PathFigure> {pFig});
                pGeo.Freeze();

                DrawingVisual line = new DrawingVisual();
                var dc = line.RenderOpen();

                if (UseDynamicLineThickness)
                {
                    var brushOpacity = System.Convert.ToByte(40 + association.InfluenceMeasure/80 * 255);
                    var brush = new SolidColorBrush(Color.FromArgb(brushOpacity, 255, 255, 255));
                    
                    var pen = new Pen(brush, association.InfluenceMeasure/7.0f);
                    dc.DrawGeometry(null, pen, pGeo);
                }
                else
                {
                    var pen = association.InfluenceMeasure < 8
                    ? _linePenWeak
                    : association.InfluenceMeasure < 16 ? _linePenNormal : _linePenStrong;
                    dc.DrawGeometry(null, pen, pGeo);
                }
                
                _visualChildren.Add(line);

                dc.Close();
                

            });

#if DEBUG
            _stopwatch.Stop();
            _simVm.BenchmarkVm.CreateLines = _stopwatch.ElapsedMilliseconds.ToString();
            _stopwatch.Reset();

#endif
        }
    }
}

