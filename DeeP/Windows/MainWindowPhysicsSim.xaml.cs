using System.Windows;
using System.Windows.Input;
using DeeP.Event;
using DeeP.ViewModel;
using NLog;
using Prism.Events;
using Prism.Ioc;
using WPFPhysicsControlsLib.Utilities;

namespace DeeP.Windows
{
    /// <summary>
    /// Interaction logic for MainWindowPhysicsSim.xaml
    /// </summary>
    public partial class MainWindowPhysicsSim
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IEventAggregator _evtAgg;

        public MainWindowPhysicsSim()
        {
            InitializeComponent();
            
            _evtAgg = App.AppContainer.Resolve<IEventAggregator>();

            Logger.Log(LogLevel.Info, "Application Main Window Initialized.");

            PhysicsSimulationTimer.Start();

            Logger.Log(LogLevel.Info, "Physics Simulation started");

            Closing += delegate { ((FlexiWallViewModel) DataContext).SaveSettingsCmd.Execute("Save"); };

            Closed += delegate { Application.Current.Shutdown(); };
        }

        private void MoveWindow(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _evtAgg.GetEvent<MainCanvasSizeChangedEvent>().Publish(e.NewSize);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _evtAgg.GetEvent<MainCanvasSizeChangedEvent>().Publish(new Size(ActualWidth, ActualHeight));
        }
}
}
