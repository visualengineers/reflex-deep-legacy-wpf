using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FastDrawingUtilityLibrary
{
    public class VectorFieldVisuals : FrameworkElement
    {
        private readonly VisualCollection _visualChildren;

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource",
                typeof(List<VectorData>),
                typeof(VectorFieldVisuals),
                new PropertyMetadata(OnItemsSourceChanged));

        public static readonly DependencyProperty BrushProperty =
            DependencyProperty.Register("Brush",
                typeof(Brush),
                typeof(VectorFieldVisuals),
                new FrameworkPropertyMetadata(null,
                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty BackgroundProperty =
            Panel.BackgroundProperty.AddOwner(typeof(VectorFieldRender));

        public VectorFieldVisuals()
        {
            _visualChildren = new VisualCollection(this);
            ToolTip = "";
        }

        public List<VectorData> ItemsSource
        {
            set { SetValue(ItemsSourceProperty, value); }
            get { return (List<VectorData>)GetValue(ItemsSourceProperty); }
        }

        public Brush Brush
        {
            set { SetValue(BrushProperty, value); }
            get { return (Brush)GetValue(BrushProperty); }
        }

        public Brush Background
        {
            set { SetValue(BackgroundProperty, value); }
            get { return (Brush)GetValue(BackgroundProperty); }
        }

        static void OnItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as VectorFieldVisuals).OnItemsSourceChanged(args);
        }

        void OnItemsSourceChanged(DependencyPropertyChangedEventArgs args)
        {
            _visualChildren.Clear();

            if (args.NewValue != null)
            {
                List<VectorData> coll = args.NewValue as List<VectorData>;
                CreateVisualChildren(coll);
            }
        }

        void CreateVisualChildren(IList<VectorData> coll)
        {
            foreach (var vectorData in coll)
            {
                DrawingVisualDescription drawingVisual = new DrawingVisualDescription();
                drawingVisual.VectorData = vectorData;
                DrawingContext dc = drawingVisual.RenderOpen();

                dc.DrawLine(new Pen(new SolidColorBrush(vectorData.Color), 1), vectorData.Start, null,
                    vectorData.End, null);

                dc.Close();
                _visualChildren.Add(drawingVisual);
            }
        }


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
            dc.DrawRectangle(Background, null, new Rect(RenderSize));
        }

        protected override void OnToolTipOpening(ToolTipEventArgs e)
        {
            HitTestResult result = VisualTreeHelper.HitTest(this, Mouse.GetPosition(this));

            if (result.VisualHit is DrawingVisualDescription)
            {
                DrawingVisualDescription drawingVisual = result.VisualHit as DrawingVisualDescription;
                VectorData vectorData = drawingVisual.VectorData;
                ToolTip = String.Format("{0}, X={1}, Y={2}", vectorData.Id, vectorData.Start, vectorData.End);
            }
            base.OnToolTipOpening(e);
        }

        protected override void OnToolTipClosing(ToolTipEventArgs e)
        {
            ToolTip = "";
            base.OnToolTipClosing(e);
        }

        public class DrawingVisualDescription : DrawingVisual
        {
            public VectorData VectorData { get; set; }
        }
    }
}
