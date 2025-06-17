using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using NLog;
using Prism.Events;

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
        
        // _eventAggregator.GetEvent<ConnectionStateChangedEvent>().Subscribe(OnConnectionStateChanged);
        // _eventAggregator.GetEvent<DataSetChangedEvent>().Subscribe(OnDataSetChanged);
        // _eventAggregator.GetEvent<ImageComponentInitializationStateChangedEvent>().Subscribe(OnImageComponentInitializationChanged);
        // _eventAggregator.GetEvent<ImageSourceLoadingFinishedEvent>().Subscribe(OnImageSourceLoadingFinished);
        // _eventAggregator.GetEvent<PreviewImageLoadedEvent>().Subscribe(OnPreviewImageLoaded);
        // _eventAggregator.GetEvent<RequestImageChangeEvent>().Subscribe(OnRequestImageChange);
        // _eventAggregator.GetEvent<SettingsChangedEvent>().Subscribe(OnSettingsChanged);
        // _eventAggregator.GetEvent<ViewChangedEvent>().Subscribe(OnViewChanged);
        
        _ = SendAppDataAsync(new("Application started."));
    }

    private void OnViewChanged(string obj)
    {
        _ = SendAppDataAsync(new("View Changed", obj));
    }

    private void OnPreviewImageLoaded(ImageSource obj)
    {
        _ = SendAppDataAsync(new("Preview Image Loaded", "", "",$"{obj.Width} x {obj.Height}"));
    }

    private void OnImageSourceLoadingFinished(string obj)
    {
        _ = SendAppDataAsync(new("Image Loading Finished", obj));
    }

    private void OnImageComponentInitializationChanged(bool obj)
    {
        _ = SendAppDataAsync(new("Image Component Initialization Changed", $"{obj}"));
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
    }
}