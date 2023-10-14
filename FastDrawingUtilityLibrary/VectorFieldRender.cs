using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FastDrawingUtilityLibrary.Events;

namespace FastDrawingUtilityLibrary
{
    public class VectorFieldRender : FrameworkElement
    {
        #region DependencyProperties

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource",
                typeof(ObservableNotifiableCollection<VectorData>),
                typeof(VectorFieldRender),
                new PropertyMetadata(OnItemsSourceChanged));

        public static readonly DependencyProperty BrushesProperty =
            DependencyProperty.Register("Brushes",
                typeof(Brush[]),
                typeof(VectorFieldRender),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty BackgroundProperty =
            Panel.BackgroundProperty.AddOwner(typeof(VectorFieldRender));

        #endregion

        #region Properties

        public ObservableNotifiableCollection<VectorData> ItemsSource
        {
            set { SetValue(ItemsSourceProperty, value); }
            get { return (ObservableNotifiableCollection<VectorData>)GetValue(ItemsSourceProperty); }
        }

        public Brush[] Brushes
        {
            set { SetValue(BrushesProperty, value); }
            get { return (Brush[])GetValue(BrushesProperty); }
        }

        public Brush Background
        {
            set { SetValue(BackgroundProperty, value); }
            get { return (Brush)GetValue(BackgroundProperty); }
        }

        #endregion

        #region DepencyPropertyChangedHandling

        static void OnItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as VectorFieldRender).OnItemsSourceChanged(args);
        }

        void OnItemsSourceChanged(DependencyPropertyChangedEventArgs args)
        {
            if (args.OldValue != null)
            {
                ObservableNotifiableCollection<VectorData> coll = args.OldValue as ObservableNotifiableCollection<VectorData>;
                coll.CollectionChanged -= OnCollectionChanged;
                coll.ItemPropertyChanged -= OnItemPropertyChanged;
            }

            if (args.NewValue != null)
            {
                ObservableNotifiableCollection<VectorData> coll = args.NewValue as ObservableNotifiableCollection<VectorData>;
                coll.CollectionChanged += OnCollectionChanged;
                coll.ItemPropertyChanged += OnItemPropertyChanged;
            }

            InvalidateVisual();
        }

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            InvalidateVisual();
        }

        void OnItemPropertyChanged(object sender, ItemPropertyChangedEventArgs args)
        {
            InvalidateVisual();
        }

        #endregion

        #region Method override

        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawRectangle(Background, null, new Rect(RenderSize));

            if (ItemsSource == null || Brushes == null)
                return;

            foreach (VectorData vector in ItemsSource)
            {
                dc.DrawLine(new Pen(new SolidColorBrush(vector.Color), 1), vector.Start, null, vector.End, null);
            }
        }

        #endregion
    }
}