using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using EventHandling;
using FlexiWallWPF.Commands;
using NLog;
using Prism.Events;
using Prism.Mvvm;

namespace FlexiWallWPF.ViewModel
{
    public class LogViewModel : BindableBase
    {
        private static readonly string NoFilter = "None";

        private string _selectedFilter = NoFilter;

        public List<string> Filters { get; set; }

        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (_selectedFilter == value)
                    return;

                _selectedFilter = value;
                RaisePropertyChanged();
                ApplyFilter();
            }
        }

        public ObservableCollection<LogUpdatedEventArgs> LogEntries { get; }

        public ObservableCollection<LogUpdatedEventArgs> FilteredLogEntries { get; }

        public LogWindowCommand LogWindowCmd { get; }

        public ClearLogCommand ClearLogCmd { get; }

        public LogViewModel(IEventAggregator evtAggregator)
        {
            LogEntries = new ObservableCollection<LogUpdatedEventArgs>();
            FilteredLogEntries = new ObservableCollection<LogUpdatedEventArgs>();

            Filters = new List<string> { NoFilter };
            LogLevel.AllLevels.ToList().ForEach(i => Filters.Add(i.Name));
            SelectedFilter = Filters[0];

            evtAggregator?.GetEvent<BroadcastLogEvent>().Subscribe(UpdateLogEntries);

            LogWindowCmd = new LogWindowCommand();
            ClearLogCmd = new ClearLogCommand(this);
        }

        private void UpdateLogEntries(LogUpdatedEventArgs args)
        {
            Application.Current?.Dispatcher?.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
               {
                   LogEntries.Add(args);
                   if (_selectedFilter == NoFilter || Equals(args.Level.Name, _selectedFilter))
                       FilteredLogEntries.Add(args);
               }));
        }

        private void ApplyFilter()
        {
            FilteredLogEntries.Clear();
            if (_selectedFilter == NoFilter)
            {
                LogEntries.ToList().ForEach(e => FilteredLogEntries.Add(e));
            }
            else
            {
                var filterLvl = _selectedFilter;
                LogEntries.ToList().ForEach(e =>
                {
                    if (Equals(e.Level.Name, filterLvl))
                        FilteredLogEntries.Add(e);
                });
            }

            RaisePropertyChanged(nameof(FilteredLogEntries));
        }
    }
}
