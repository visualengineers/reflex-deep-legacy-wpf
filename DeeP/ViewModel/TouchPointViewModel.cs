using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using DeeP.Event;
using EventHandling;
using NLog;
using Prism.Events;
using Prism.Mvvm;
using ReFlex.Core.Networking.Event;
namespace DeeP.ViewModel
{
    public class TouchPointViewModel : BindableBase, IDisposable
    {
        private readonly IEventAggregator _eventAggregator;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private Size _canvasSize;

        public ObservableCollection<TouchPointElementViewModel> Interactions { get; } = new ObservableCollection<TouchPointElementViewModel>();

        public bool ShowTouchPoints { get; private set; }

        public TouchPointViewModel(IEventAggregator eventAggregator)
        {
            this._eventAggregator = eventAggregator;
            _eventAggregator?.GetEvent<InteractionsUpdatedEvent>().Subscribe(InteractionsUpdated);
            _eventAggregator?.GetEvent<TouchPointVisibilityChangedEvent>().Subscribe(UpdateVisibility);
            _eventAggregator?.GetEvent<MainCanvasSizeChangedEvent>().Subscribe(UpdateSize);
        }

        private void UpdateSize(Size newSize)
        {
            _canvasSize = newSize;
        }

        private void UpdateVisibility(bool visible)
        {
            ShowTouchPoints = visible;
            RaisePropertyChanged(nameof(ShowTouchPoints));
        }

        private void InteractionsUpdated(InteractionsReceivedEventArgs args)
        {
            Application.Current?.Dispatcher?.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                Interactions.Clear();
                args.Interactions.ForEach(item => Interactions.Add(new TouchPointElementViewModel(item, _canvasSize)));
            }));
        }

        public void Dispose()
        {
            _eventAggregator?.GetEvent<InteractionsUpdatedEvent>().Unsubscribe(InteractionsUpdated);
            _eventAggregator?.GetEvent<TouchPointVisibilityChangedEvent>().Unsubscribe(UpdateVisibility);
            _eventAggregator?.GetEvent<MainCanvasSizeChangedEvent>().Unsubscribe(UpdateSize);
        }
    }
}
