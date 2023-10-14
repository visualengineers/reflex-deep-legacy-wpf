using System.Windows;
using DelVizDataStructure;
using FlexiWallWPF.Properties;
using FlexiWallWPF.Util;
using FlexiWallWPF.ViewModel;
using FlexiWallWPF.Views;
using FlexiWallWPF.Windows;
using NLog;
using PhysicsSimulation.Model;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;
using ReFlex.Core.Networking.Components;
using WPFPhysicsControlsLib.ViewModel;
using WPFPhysicsControlsLib.Windows;

namespace FlexiWallWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App: PrismApplication
    {
        private DebugWindow _debug;
        private LogWindow _logWnd;
        private Window _mainAppWindow;

        public static IContainerProvider AppContainer { get; private set; }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var eventAggregator = Container.Resolve<IEventAggregator>();

            LogManager.Setup().SetupExtensions(s => s.RegisterTarget<InAppLoggingTarget>("InAppLogging"));
            
            var logger = LogManager.GetCurrentClassLogger();
            containerRegistry.RegisterInstance(logger);

            containerRegistry.RegisterInstance(new LogViewModel(eventAggregator));

            logger.Info($"{GetType().FullName}: Registered LoggingTarget {typeof(InAppLoggingTarget).FullName}.");

            PhysicsSimulationProperties.BoundaryForceModifier = Settings.Default.BoundaryForceModifier;
            PhysicsSimulationProperties.CentripetalSpeed = Settings.Default.CentripetalSpeed;
            PhysicsSimulationProperties.CollisionDetectionIterations = Settings.Default.CollisionDetectionIterations;
            PhysicsSimulationProperties.ComputeRepulsionUnselected = Settings.Default.ComputeRepulsionForUnselectedTags;
            PhysicsSimulationProperties.DetectCollisions = Settings.Default.DetectCollisions;
            PhysicsSimulationProperties.ItemBaseSize = Settings.Default.ItemBaseSize;
            PhysicsSimulationProperties.ItemSize = Settings.Default.ItemForceFieldDiameter;
            PhysicsSimulationProperties.ItemsCanvasHeight = Settings.Default.ItemsCanvasHeight;
            PhysicsSimulationProperties.ItemsCanvasWidth = Settings.Default.ItemsCanvasWidth;
            PhysicsSimulationProperties.MaxUserInfluenceValue = Settings.Default.MaxIntensityValue;
            PhysicsSimulationProperties.MinDistSquared = 0.00000001f;
            PhysicsSimulationProperties.ObjectForceModifier = Settings.Default.ObjectForceModifier;
            PhysicsSimulationProperties.SelectedTagAttributionModifier = Settings.Default.SelectedTagAttractionModifier;
            PhysicsSimulationProperties.SimulationTimerInterval = Settings.Default.SimulationTimerIntervalMs;
            PhysicsSimulationProperties.StandardTagAttraction = Settings.Default.StandardTagAttraction;
            PhysicsSimulationProperties.StandardTagRepulsion = Settings.Default.StandardTagRepulsion;
            PhysicsSimulationProperties.TagBaseSize = Settings.Default.TagBaseSize;
            PhysicsSimulationProperties.TagForceModifier = Settings.Default.TagForceModifier;
            PhysicsSimulationProperties.TagItemRepulsionModifier = Settings.Default.TagItemRepulsionModifier;
            PhysicsSimulationProperties.TagSameCategoryForceModifier = Settings.Default.TagSameCategoryForceModifier;
            PhysicsSimulationProperties.TagSameDimensionForceModifier = Settings.Default.TagSameDimensionForceModifier;
            PhysicsSimulationProperties.TagSize = Settings.Default.TagForceFieldDiameter;
            PhysicsSimulationProperties.TagsCanvasHeight = Settings.Default.TagCanvasHeight;
            PhysicsSimulationProperties.TagsCanvasWidth = Settings.Default.TagCanvasWidth;
            PhysicsSimulationProperties.UnselectedTagRepulsionModifier = Settings.Default.UnselectedTagRepulsionModifier;
            PhysicsSimulationProperties.ProcessDepthOption = Settings.Default.ProcessDepthOption;
            PhysicsSimulationProperties.EnforceEqualDataDistribution = Settings.Default.EnforceEqualDataDistribution;
            PhysicsSimulationProperties.MaxNumItemsPerTag = Settings.Default.MaxNumItemsPerTag;

            logger.Info($"{GetType().FullName}: initialized values for {typeof(PhysicsSimulationProperties).FullName} from {typeof(Settings).FullName}.");

            
            
            var repository = new DataRepository(PhysicsSimulationProperties.EnforceEqualDataDistribution, PhysicsSimulationProperties.MaxNumItemsPerTag, logger);
            containerRegistry.RegisterInstance(repository);

            containerRegistry.RegisterInstance(new SimulationVm(logger, eventAggregator));

            containerRegistry.RegisterInstance(new SimulationPropertiesVm(repository, logger, eventAggregator));

            logger.Info($"{GetType().FullName}: Registered model types.");

            ViewModelLocationProvider.Register<LogWindow, LogViewModel>();

            ViewModelLocationProvider.Register<SensorView, SensorViewModel>();

            ViewModelLocationProvider.Register<TouchPointView, TouchPointViewModel>();

            ViewModelLocationProvider.Register<MainWindowPhysicsSim, FlexiWallViewModel>();

            logger.Info($"{GetType().FullName}: Registered viewmodels.");

            logger.Info($"{GetType().FullName}: Finished Type Registrations.");
        }

        protected override Window CreateShell()
        {
            AppContainer = Container;

            _mainAppWindow = Container.Resolve<MainWindowPhysicsSim>();

            _debug = new DebugWindow();
            _debug.Show();

            

            return _mainAppWindow;
        }
    }
}
