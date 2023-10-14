using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using PhysicsSimulation.Model;
using PhysicsSimulation.Utilities;
using Prism.Mvvm;
using WPFPhysicsControlsLib.Utilities;

namespace WPFPhysicsControlsLib.ViewModel
{
    public class DetailsVisualizationViewModel : BindableBase
    {
        protected SimulatedContentItemVm ItemVm;
        private static int BezierHandleOffset = 50;
        protected readonly Thickness BdTicknessShowImage = new Thickness(1.0);
        protected readonly Thickness BdTicknessFullDesc = new Thickness(1.0, 0.0, 1.0, 1.0);
        protected readonly Vector2D PanelOffset = new Vector2D(25, 0);
        protected readonly Vector2D PanelSize = new Vector2D(300, 450);
        protected readonly Vector2D MinPanelBorderDist = new Vector2D(25, 25);
        protected readonly float AnchorHeightOffset = 415;

        public ObservableCollection<Tuple<Point, Point, Point, Point>> ConnectionLines { get; private set; }

        public SimulatedContentItemVm CurrentlyActiveItem
        {
            get => ItemVm;
            set
            {
                //if (ItemVm == value)
                //    return;

                ItemVm = value;
                RaisePropertyChanged(nameof(CurrentlyActiveItem));
                SwitchActiveItem();
            }
        }

        public float PositionX { get; protected set; }

        

        public float PositionY { get; protected set; }

        public float AnchorPositionX => DrawOnLeftSide ? PositionX + PanelSize.X : PositionX;

        public float AnchorPositionY => PositionY + AnchorHeightOffset;
        public int BorderSpan { get; protected set; }

        public int BorderRow { get; protected set; }

        public DetailsSelectionState DetailsState { get; protected set; }

        public Visibility PanelVisibility { get; protected set; }

        public Visibility LabelVisibility { get; protected set; }

        public Visibility ImageVisibility { get; protected set; }

        public Visibility DetailsVisibility { get; protected set; }

        public Visibility LeftAnchorVisibility
        {
            get
            {
                return LabelVisibility == Visibility.Visible && !DrawOnLeftSide
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public Visibility RightAnchorVisibility
        {
            get
            {
                return LabelVisibility == Visibility.Visible && DrawOnLeftSide ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public ImageSource ImgSrc { get; protected set; }

        public string Title { get; protected set; }

        public string Address { get; protected set; }

        public string Description { get; protected set; }

        public string CreationDate { get; protected set; }

        public bool DrawOnLeftSide { get; private set; }


        public Thickness BorderThicknessImg => DetailsState == DetailsSelectionState.FullDescription ? BdTicknessFullDesc : BdTicknessShowImage;

        public DetailsVisualizationViewModel()
        {
            ConnectionLines = new ObservableCollection<Tuple<Point, Point, Point, Point>>();
            SwitchActiveItem();
        }

        protected void SwitchActiveItem()
        {
            UpdateVisibility();

            if (ItemVm == null)
            {
                if (ConnectionLines.Count > 0)
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                        new Action(() => ConnectionLines.Clear()));
                return;
            }

            var data = ItemVm.Object.Data;

            CreationDate = data.CreationDate.ToString("D", CultureInfo.CreateSpecificCulture("en-US"));
            Description = data.Description;
            Address = data.Address;
            Title = data.Title;

            var nextTag = ItemVm.Object.InfluencingTags.OrderBy(
                t => Math.Abs((t.State.CurrentPosition - ItemVm.Object.State.CurrentPosition).LengthSquared))
                .FirstOrDefault();

            // default position right beneath item (X++) and above (Y--) 
            var offset = new Vector2D(PanelOffset.X, -(PanelSize.Y + PanelOffset.Y));

            DrawOnLeftSide = nextTag != null &&
                                nextTag.State.CurrentPosition.X > ItemVm.Object.State.CurrentPosition.X;

            if (DrawOnLeftSide)
                // if left of item: move whole panel to the left
                offset = new Vector2D(-(PanelSize.X + PanelOffset.X), offset.Y);

            // compute resulting position (Upper left corner of panel) and clamp position so that the panel always stays in visible area
            var pX = ItemVm.TranslationX + offset.X;
            var pY = ItemVm.TranslationY + offset.Y;
            if (pX < MinPanelBorderDist.X)
                pX = MinPanelBorderDist.X;

            if (pY < MinPanelBorderDist.Y)
                pY = MinPanelBorderDist.Y;

            var distXMax = pX + (PanelSize.X + PanelOffset.X) - PhysicsSimulationProperties.TagsCanvasWidth;
            if (distXMax > MinPanelBorderDist.X)
                pX -= distXMax;


            var distYMax = pY + (PanelSize.Y + PanelOffset.Y) - PhysicsSimulationProperties.TagsCanvasHeight;

            if (distYMax > MinPanelBorderDist.Y)
                pY -= (distYMax + MinPanelBorderDist.Y);

            PositionX = pX;
            PositionY = pY;

            var p1 = new Point(AnchorPositionX, AnchorPositionY + 2);
            var p2 = DrawOnLeftSide
                ? new Point(AnchorPositionX + BezierHandleOffset, AnchorPositionY + 2)
                : new Point(AnchorPositionX - BezierHandleOffset, AnchorPositionY);
            var p4 = Vector2D.CreatePointFromVector(ItemVm.Object.State.CurrentPosition);
            var p3 = DrawOnLeftSide
                ? new Point(p4.X - BezierHandleOffset, p4.Y)
                : new Point(p4.X + BezierHandleOffset, p4.Y);


            var lines = new List<Tuple<Point, Point, Point, Point>>();


            lines.Add(Tuple.Create(p1, p2, p3, p4));

            //ItemVm.Object.Data.Tags.ForEach(t =>
            //{
            //    var tagVm = ViewModelLocator.Instance.SimulationVm.Tags.FirstOrDefault(tag => tag.Tag.Tag.Equals(t));
            //    if (tagVm != null)
            //    {
            //        lines.Add(Tuple.Create(new Vector2D(AnchorPositionX, AnchorPositionY), tagVm.Tag.State.CurrentPosition));
            //    }
            //});

            // only set connection lines once (and clear them when ItemVm == null)
            if (ConnectionLines.Count == 0)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                    new Action(() => lines.ForEach(li => ConnectionLines.Add(li))));
            }

            RaisePropertyChanged(nameof(Description));
            RaisePropertyChanged(nameof(Address));
            RaisePropertyChanged(nameof(Title));
            RaisePropertyChanged(nameof(CreationDate));

            RaisePropertyChanged(nameof(PositionX));
            RaisePropertyChanged(nameof(PositionY));

            RaisePropertyChanged(nameof(AnchorPositionX));
            RaisePropertyChanged(nameof(AnchorPositionY));

            RaisePropertyChanged(nameof(DrawOnLeftSide));

            DetailsState = ItemVm.SelectionState;
            RaisePropertyChanged(nameof(DetailsState));
            RaisePropertyChanged(nameof(BorderThicknessImg));

            ImgSrc = ItemVm.ImgSource;
            RaisePropertyChanged(nameof(ImgSrc));

        }

        protected void UpdateVisibility()
        {

            if (ItemVm == null)
            {
                PanelVisibility = Visibility.Collapsed;
                BorderSpan = 1;
                BorderRow = 3;
            }
            else
            {
                PanelVisibility = DetailsState != DetailsSelectionState.NotSelected ? Visibility.Visible : Visibility.Collapsed;
                LabelVisibility = DetailsState != DetailsSelectionState.NotSelected ? Visibility.Visible : Visibility.Collapsed;
                ImageVisibility = DetailsState == DetailsSelectionState.ShowImage || DetailsState == DetailsSelectionState.FullDescription ? Visibility.Visible : Visibility.Collapsed;
                DetailsVisibility = DetailsState == DetailsSelectionState.FullDescription ? Visibility.Visible : Visibility.Collapsed;

                BorderSpan = DetailsState == DetailsSelectionState.FullDescription
                    ? 3
                    : DetailsState == DetailsSelectionState.ShowImage
                        ? 2
                        : 1;

                BorderRow = DetailsState == DetailsSelectionState.FullDescription
                    ? 1
                    : DetailsState == DetailsSelectionState.ShowImage
                        ? 2
                        : DetailsState == DetailsSelectionState.ShowName ? 3 : 0;

            }


            RaisePropertyChanged(nameof(BorderSpan));
            RaisePropertyChanged(nameof(BorderRow));
            RaisePropertyChanged(nameof(PanelVisibility));
            RaisePropertyChanged(nameof(LabelVisibility));
            RaisePropertyChanged(nameof(ImageVisibility));
            RaisePropertyChanged(nameof(DetailsVisibility));
            RaisePropertyChanged(nameof(LeftAnchorVisibility));
            RaisePropertyChanged(nameof(RightAnchorVisibility));

        }
    }
}
