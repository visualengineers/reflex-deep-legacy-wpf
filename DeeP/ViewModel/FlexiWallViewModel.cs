using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using EventHandling;
using FastDrawingUtilityLibrary;
using Prism.Events;
using Prism.Mvvm;
using ReFlex.Apps.DeeP.Commands;
using ReFlex.Apps.DeeP.Model;
using ReFlex.Core.Networking.Event;
using WPFPhysicsControlsLib.ViewModel;

namespace ReFlex.Apps.DeeP.ViewModel
{
    public class FlexiWallViewModel : BindableBase, IFlexiWallApplicationActions, IDisposable
    {
        #region Fields

        private readonly LogViewModel _logVm;
        private readonly InteractionProcessor _processor;

        private bool _showPropertyPanel;
        private bool _showHelp;
        private bool _isFullScreen;
        private double _currFps;
        private const double Tolerance = 0.01;
        private ulong _currFrames = 0;
        private ulong _frameCounter;
        private double _totalFps;
        private double _elapsedTime;
        private bool _showExtrema;
        
        private readonly Stopwatch _watch = Stopwatch.StartNew();

        #endregion

        #region Properties

        public SimulationVm SimulationVm { get; }

        public SimulationPropertiesVm SimPropertiesVm { get; }

        public ObservableCollection<VectorData> Vectors { get; }

        public Visibility PropertyPanelVisibility => _showPropertyPanel ? Visibility.Visible : Visibility.Collapsed;

        public Visibility HelpPanelVisibility => _showHelp ? Visibility.Visible : Visibility.Collapsed;

        public Visibility TitleBarVisibility => _isFullScreen ? Visibility.Collapsed : Visibility.Visible;

        public string AppVersion => "Version " + Assembly.GetExecutingAssembly().GetName().Version;

        public string CurrentFps => $"{_currFps:0.00}";

        public string AverageFps
        {
            get
            {
                if (_elapsedTime <= 0)
                    return "0";

                var result = _frameCounter / _elapsedTime;
                return $"{result:0.00}";
            }
        }

        public string TotalFrames => $"{_frameCounter:0}";

        public double CurrFps
        {
            get { return _currFps; }
            set
            {
                _currFps = value;
                _totalFps += value;
                // only update every 4 Frames
                if (_frameCounter % 4 == 0)
                {
                    RaisePropertyChanged(nameof(AverageFps));
                }
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CurrentFps));
                RaisePropertyChanged(nameof(TotalFrames));
            }
        }

        public double TotalTime
        {
            get => _elapsedTime;
            set
            {
                if (Math.Abs(_elapsedTime - value) < Tolerance)
                    return;

                _elapsedTime = value;
                RaisePropertyChanged();
            }
        }

        
        public bool ShowExtrema
        {
            get => _showExtrema;
            set
            {
                if (_showExtrema == value)
                    return;
                _showExtrema = value;
                RaisePropertyChanged();
            }
        }

        public ApplicationCommand AppCmd { get; }

        
        public SaveDefaultSettingsCommand SaveSettingsCmd { get; }

        #endregion

        #region Constructor
        public FlexiWallViewModel(IEventAggregator eventAggregator, LogViewModel logVm, SimulationVm simVm, SimulationPropertiesVm simPropsVm, InteractionProcessor processor)
        {
            SimulationVm = simVm;
            SimPropertiesVm = simPropsVm;


            AppCmd = new ApplicationCommand(this);
            SaveSettingsCmd = new SaveDefaultSettingsCommand(this);
            Vectors = new ObservableCollection<VectorData>();
            
            // Apply Default Values
            SaveSettingsCmd.Execute("Apply");

            eventAggregator.GetEvent<InteractionsUpdatedEvent>().Subscribe(ManualUpdate);

            _logVm = logVm;
            _processor = processor;
            
            CompositionTarget.Rendering += OnRendering;
            _watch.Restart();
        }

        private void ManualUpdate(InteractionsReceivedEventArgs args)
        {
            _processor.ProcessInteractions(args.Interactions);
        }

        #endregion
        

        #region Implementation of IFlexiWallApplicationActions

        public void TogglePropertyPanelVisibility()
        {
            _showPropertyPanel = !_showPropertyPanel;
            RaisePropertyChanged(nameof(PropertyPanelVisibility));
        }

        public void ToggleFullScreen()
        {
            _isFullScreen = !_isFullScreen;
            var wnd = Application.Current?.MainWindow;
            if (wnd != null)
                wnd.WindowState = _isFullScreen ? WindowState.Maximized : WindowState.Normal;
            RaisePropertyChanged(nameof(TitleBarVisibility));
        }

        public void ToggleAppMinimized()
        {
            var wnd = Application.Current?.MainWindow;
            if (wnd != null)
                wnd.WindowState = WindowState.Minimized;
        }

        public void Exit()
        {
            Application.Current?.Shutdown();
        }

        public void ToggleHelp()
        {
            _showHelp = !_showHelp;
            RaisePropertyChanged(nameof(HelpPanelVisibility));
        }

        public void ToggleLogVisibility()
        {
            _logVm.LogWindowCmd.Execute("ToggleVisibility");
        }

        #endregion
        
        public void Dispose()
        {
            CompositionTarget.Rendering -= OnRendering;
        }

        
        
        private void OnRendering(object? sender, EventArgs e)
        {
            _frameCounter++;
            _currFrames++;
            
            RaisePropertyChanged(nameof(TotalFrames));
            
            if (_watch.Elapsed.TotalSeconds >= 1)
            {
                CurrFps = _currFrames / _watch.Elapsed.TotalSeconds;
                _currFrames = 0;
                TotalTime += _watch.Elapsed.TotalSeconds;
                _watch.Restart();
            }
        }
    }
}
