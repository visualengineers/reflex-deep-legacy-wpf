using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using EventHandling;
using FlexiWallWPF.Event;
using FlexiWallWPF.Properties;
using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using ReFlex.Core.Common.Components;
using ReFlex.Core.Networking.Components;
using ReFlex.Core.Networking.Event;
using ReFlex.Core.Networking.Util;

namespace FlexiWallWPF.ViewModel
{
    public class SensorViewModel : BindableBase
    {
        #region Properties

        private WebSocketClient _wsClient;
        private readonly IEventAggregator _eventAggregator;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private bool _reconnectPause;
        private bool _showTouchPoints = true;

        private readonly BackgroundWorker _connectionChecker;

        public bool SensorConnected { get; private set; }

        public ICommand StartSensorCommand { get; }

        public ICommand StopSensorCommand { get; }

        public ICommand ToggleShowTouchPoints { get; }

        public string Address { get; private set; }

        public ObservableCollection<Interaction> Interactions { get; } = new ObservableCollection<Interaction>();

        public int FrameCounter { get; private set; }

        public bool ShowTouchPoints
        {
            get => _showTouchPoints;
            set => ToggleTouchPointVisibility();
        }

        #endregion

        #region Constructor

        public SensorViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            Application.Current.Exit += OnApplicationShutdown;

            _connectionChecker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = false };
            _connectionChecker.DoWork += CheckConnection;
            _connectionChecker.ProgressChanged += OnConnectionStatusReport;
            _connectionChecker.RunWorkerCompleted += OnConnectionCheckFinished;

            if (Settings.Default.SensorAutostart)
                StartSensorDelayed();

            StartSensorCommand = new DelegateCommand(() => StartSensor());
            StopSensorCommand = new DelegateCommand(StopSensor);

            ToggleShowTouchPoints = new DelegateCommand(ToggleTouchPointVisibility);

            _eventAggregator.GetEvent<TouchPointVisibilityChangedEvent>().Publish(_showTouchPoints);

            Logger.Info($"{GetType().FullName}: Initialization completed.");
        }

        private void ToggleTouchPointVisibility()
        {
            _showTouchPoints = !_showTouchPoints;
            RaisePropertyChanged(nameof(ShowTouchPoints));
            _eventAggregator.GetEvent<TouchPointVisibilityChangedEvent>().Publish(_showTouchPoints);
        }

        private void OnApplicationShutdown(object sender, ExitEventArgs e)
        {
            StopSensor();
        }

        private void OnConnectionCheckFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            Logger.Info($"{GetType().FullName}: BackgroundWorker Completed.");
        }

        private void OnConnectionStatusReport(object sender, ProgressChangedEventArgs e)
        {
            SensorConnected = _wsClient?.IsConnected ?? false;

            if (!SensorConnected && Settings.Default.SensorAutostart && !_reconnectPause)
            {
                _reconnectPause = true;
                StopSensor();
                var startSensorDelayed = StartSensorDelayed();
            }
        }

        private void CheckConnection(object sender, DoWorkEventArgs e)
        {
            var i = 0;

            var worker = sender as BackgroundWorker;
            var client = e.Argument as WebSocketClient;

            if (worker == null || client == null)
                return;

            while (!worker.CancellationPending)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                worker.ReportProgress(i);
                i++;
            }
        }

        private void InteractionsUpdated(object sender, NetworkingDataMessage e)
        {
            var args = new InteractionsReceivedEventArgs(sender, e.Message);

            _eventAggregator?.GetEvent<InteractionsUpdatedEvent>().Publish(args);

            Application.Current?.Dispatcher?.BeginInvoke(DispatcherPriority.Normal, (Action) (() =>
            {
                Interactions.Clear();
                Interactions.AddRange(args.Interactions);
            }));

            FrameCounter++;

            RaisePropertyChanged(nameof(FrameCounter));

            Logger.Trace($"Received Interaction Update - num Interactions: {args.Interactions.Count}");
        }

        #endregion

        #region Auxiliary Methods

        private async Task<bool> StartSensorDelayed()
        {
            await Task.Run(() => Thread.Sleep(TimeSpan.FromSeconds(3)));

            await StartSensor();

            _reconnectPause = false;

            return SensorConnected;
        }

        private async Task<bool> StartSensor()
        {
            if (_wsClient != null)
            {
                StopSensor();
                await StartSensorDelayed();
                return false;
            }

            _wsClient = new WebSocketClient(
                $"ws://{Settings.Default.ReFlexServerAddress}",
                Settings.Default.ReFlexServerPort,
                $"/{Settings.Default.ReFlexServerEndpoint}");

            if (_wsClient == null)
            {
                Logger.Error($"{GetType().FullName}: Cannot Start sensor. {nameof(_wsClient)}[{typeof(WebSocketClient).FullName}] is null.");
                return false;
            }

            var connected = await Task.Run(() =>
            {
                _wsClient.Connect();
                return _wsClient.IsConnected;
            });

            _wsClient.NewDataReceived += InteractionsUpdated;

            SensorConnected = connected;
            Address = _wsClient.Address;

            RaisePropertyChanged(nameof(SensorConnected));
            RaisePropertyChanged(nameof(Address));

            Logger.Info($"Try to connect to ReFlex {Enum.GetName(typeof(NetworkInterface), _wsClient.Type)} server using address: {_wsClient.Address}. Connection successful: {SensorConnected}.");

            if (!_connectionChecker.IsBusy)
                _connectionChecker.RunWorkerAsync(_wsClient);

            return SensorConnected;
        }

        private void StopSensor()
        {
            if (_wsClient != null)
            {
                try
                {
                    if (_wsClient.IsConnected)
                        _wsClient.Disconnect();
                    _wsClient.NewDataReceived -= InteractionsUpdated;
                }
                catch (Exception e)
                {
                    Logger.Warn($"{e.GetType().FullName} when disconnecting from Reflex server.");
                }
            }

            SensorConnected = _wsClient?.IsConnected ?? false;
            Address = "-";

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                Interactions.Clear();
            }));

            RaisePropertyChanged(nameof(SensorConnected));
            RaisePropertyChanged(nameof(Address));

            Logger.Info($"Close Connection to ReFlex server. IsConnected: {SensorConnected}.");


            try
            {
                _wsClient = null;
            }
            catch (Exception e)
            {
                Logger.Warn($"{e.GetType().FullName} when disposing client.");
            }
        }

        #endregion
    }
}
