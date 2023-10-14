using System;
using System.Globalization;
using System.Windows.Media.Imaging;
using WPFPhysicsControlsLib.Utilities;

namespace WPFPhysicsControlsLib.ViewModel.DesignTimeData
{
    public class DesignTimeDetailsVisualizationViewModel : DetailsVisualizationViewModel
    {
        public DesignTimeDetailsVisualizationViewModel()
        {
            PositionX = 100;
            PositionY = 150;
            DetailsState = DetailsSelectionState.FullDescription;
            CreationDate = DateTime.Now.ToString("D", CultureInfo.CreateSpecificCulture("en-US"));
            Description = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, " +
                          "sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum " +
                          "dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna " +
                          "aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est " +
                          "Lorem ipsum dolor sit amet.";
            Address = "http://www.example-visualization.net/~testuser/visualization.htm";
            Title = "visualization title";
            ImgSrc = new BitmapImage(new Uri("pack://application:,,,/WPFPhysicsControlsLib;component/Resources/DelViz_Grafiken/ExampleResource.PNG"));
            ImgSrc.Freeze();


            UpdateVisibility();

            RaisePropertyChanged(nameof(Description));
            RaisePropertyChanged(nameof(Address));
            RaisePropertyChanged(nameof(Title));
            RaisePropertyChanged(nameof(CreationDate));

            RaisePropertyChanged(nameof(PositionX));
            RaisePropertyChanged(nameof(PositionY));

            RaisePropertyChanged(nameof(DetailsState));
            RaisePropertyChanged(nameof(BorderThicknessImg));

            RaisePropertyChanged(nameof(ImgSrc));

        }
    }
}
