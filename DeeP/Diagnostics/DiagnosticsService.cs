using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using EventHandling.Utilities;
using NLog;
using PhysicsSimulation.Events;
using Prism.Events;
using ReFlex.Apps.DeeP.Event;
using ReFlex.Apps.DeeP.Event.EventData;

namespace ReFlex.Apps.DeeP.Diagnostics;

public class DiagnosticsService
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly DiagnosticsClient _client;
    private readonly IEventAggregator _eventAggregator;

    public DiagnosticsService(DiagnosticsClient client, IEventAggregator eventAggregator)
    {
        _client = client;
        _eventAggregator = eventAggregator;
        Initialize();
    }

    public async Task<HttpResponseMessage> SendAppDataAsync(DiagnosticsData data)
    {
        try
        {
            return await _client.PostAppDataAsync(data.ToJson());
        }
        catch (Exception exc)
        {
            _logger.Error(exc, "Error sending diagnostics data.");
        }

        return new HttpResponseMessage(HttpStatusCode.NotFound);
    }

    private void Initialize()
    {
        if (!global::DeeP.Properties.Settings.Default.SendDiagnosticData)
            return;
        
        Application.Current.Exit += OnExit;

        Application.Current.Activated += OnActivated;
        Application.Current.Deactivated += OnDeactivated;

        Application.Current.DispatcherUnhandledException += OnUnhandledException;

        _eventAggregator.GetEvent<ConnectionStateChangedEvent>().Subscribe(OnConnectionStateChanged);
        _eventAggregator.GetEvent<ResetRequestedEvent>().Subscribe(OnResetRequested);
        _eventAggregator.GetEvent<SimulationPauseRequestedEvent>().Subscribe(OnSimulationPauseRequested);

        _eventAggregator.GetEvent<ObjectsInitializedEvent>().Subscribe(OnObjectsInitialized);
        _eventAggregator.GetEvent<ItemInfluenceChangedEvent>().Subscribe(OnItemInfluenceChanged);
        _eventAggregator.GetEvent<ItemSizeChangedEvent>().Subscribe(OnItemSizeChanged);
        _eventAggregator.GetEvent<InterTagForcesChangedEvent>().Subscribe(OnInterTagForcesChanged);
        
        _ = SendAppDataAsync(new("Application started."));
    }

    private void OnObjectsInitialized(bool obj)
    {
        _ = SendAppDataAsync(new("PhysicsSim: Objects Initialized", 
            obj.ToString()));
    }

    private void OnInterTagForcesChanged(bool obj)
    {
        _ = SendAppDataAsync(new("PhysicsSim: Inter Tag Forces Changed", 
            obj.ToString()));
    }

    private void OnItemSizeChanged(ObjectType obj)
    {
        _ = SendAppDataAsync(new("PhysicsSim: Item Size Changed", 
            obj.ToString()));
    }

    private void OnItemInfluenceChanged(ObjectType obj)
    {
        _ = SendAppDataAsync(new("PhysicsSim: Item Influence Changed", 
            obj.ToString()));
    }

    private void OnSimulationPauseRequested(Tuple<bool, bool> obj)
    {
        _ = SendAppDataAsync(new("PhysicsSim: Simulation Pause Requested", 
            obj.Item1.ToString(), obj.Item2.ToString()));
    }

    private void OnResetRequested(bool obj)
    {
        _ = SendAppDataAsync(new("PhysicsSim: Reset Requested", 
            obj.ToString()));
    }

    private void OnConnectionStateChanged(ConnectionStateEventData obj)
    {
        var state = obj.IsConnected ? "Connected" : "Disconnected";
        
        _ = SendAppDataAsync(new("ConnectionState Changed", 
            state, $"{obj.Id}", $"{obj.StateMsg}|{obj.Address}"));
    }
    
    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        _ = SendAppDataAsync(new("Unhandled Exception", 
            $"{e.Exception.Source}", $"{e.Exception.GetType()}", $"{e.Exception.Message}"));
    }

    private void OnDeactivated(object sender, EventArgs e)
    {
        _ = SendAppDataAsync(new("Application Deactivated."));
    }

    private void OnActivated(object sender, EventArgs e)
    {
        _ = SendAppDataAsync(new("Application Activated."));
    }

    private void OnExit(object sender, ExitEventArgs e)
    {
        _ = SendAppDataAsync(new("Application exited."));
        
        Application.Current.Exit -= OnExit;

        Application.Current.Activated -= OnActivated;
        Application.Current.Deactivated -= OnDeactivated;

        Application.Current.DispatcherUnhandledException -= OnUnhandledException;
        
        _eventAggregator.GetEvent<ConnectionStateChangedEvent>().Unsubscribe(OnConnectionStateChanged);
        _eventAggregator.GetEvent<ResetRequestedEvent>().Unsubscribe(OnResetRequested);
        _eventAggregator.GetEvent<SimulationPauseRequestedEvent>().Unsubscribe(OnSimulationPauseRequested);

        _eventAggregator.GetEvent<ObjectsInitializedEvent>().Unsubscribe(OnObjectsInitialized);
        _eventAggregator.GetEvent<ItemInfluenceChangedEvent>().Unsubscribe(OnItemInfluenceChanged);
        _eventAggregator.GetEvent<ItemSizeChangedEvent>().Unsubscribe(OnItemSizeChanged);
        _eventAggregator.GetEvent<InterTagForcesChangedEvent>().Unsubscribe(OnInterTagForcesChanged);
    }
}