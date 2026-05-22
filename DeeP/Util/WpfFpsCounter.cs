namespace ReFlex.Apps.DeeP.Util;

using System;
using System.Diagnostics;
using System.Windows.Media;

public sealed class WpfFpsCounter
{
    private readonly Stopwatch _watch = Stopwatch.StartNew();
    private int _frames;

    public double CurrentFps { get; private set; }
    
    public double TotalSeconds { get; private set; }
    
    public double TotalFrames { get; private set; }

    public void Start()
    {
        CompositionTarget.Rendering += OnRendering;
    }

    public void Stop()
    {
        CompositionTarget.Rendering -= OnRendering;
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        _frames++;

        if (_watch.Elapsed.TotalSeconds >= 1)
        {
            CurrentFps = _frames / _watch.Elapsed.TotalSeconds;
            _frames = 0;
            _watch.Restart();

            Debug.WriteLine($"WPF FPS: {CurrentFps:F1}");
        }
    }
}