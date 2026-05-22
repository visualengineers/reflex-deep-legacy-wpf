using System;
using System.Windows;
using NLog;
using Prism.Commands;
using Prism.Ioc;
using ReFlex.Apps.DeeP.Windows;

namespace ReFlex.Apps.DeeP.Commands
{
    public class LogWindowCommand : DelegateCommand<string>
    {
        private static Window _wnd;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public LogWindowCommand() : base(ExecuteLogWindowCommand)
        {
            // _wnd = window;
            // CanExecute(true);
        }

        public static void ExecuteLogWindowCommand(string s)
        {
            if (_wnd == null)
                _wnd = ContainerLocator.Current.Resolve<LogWindow>();

            if (string.IsNullOrWhiteSpace(s))
            {
                Logger.Error($"no argument provided for {nameof(SaveDefaultSettingsCommand)}.{nameof(ExecuteLogWindowCommand)}.");

                return;
            }

            if (s.Equals("ToggleVisibility"))
            {


                if (_wnd.WindowState != WindowState.Minimized)
                {
                    _wnd.Hide();
                    _wnd.WindowState = WindowState.Minimized;
                }
                else
                {
                    _wnd.WindowState = WindowState.Normal;
                    _wnd.Show();
                }

                Logger.Info($"{nameof(SaveDefaultSettingsCommand)}.{nameof(ExecuteLogWindowCommand)} sucessfully Executed with param {s}.");

                return;
            }

            Logger.Warn($"Tried to execute {nameof(SaveDefaultSettingsCommand)}.{nameof(ExecuteLogWindowCommand)} with invalid parameter [{s}]. Parameter not found in Dictionary.");

            throw new NotImplementedException("Value " + s + " is not connected with an implemented action."); 
        }
    }
}
