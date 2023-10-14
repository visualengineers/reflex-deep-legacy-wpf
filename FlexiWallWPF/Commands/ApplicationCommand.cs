using System;
using System.Collections.Generic;
using FlexiWallWPF.ViewModel;
using NLog;
using NLog.Fluent;
using Prism.Commands;

namespace FlexiWallWPF.Commands
{
    /// <summary>
    /// Command handling actions regarding different window activities:
    /// - switch visibility of property panel
    /// - switch visibility of help overlay
    /// - toggle fullscreen-mode
    /// - minimize window
    /// - switch visibility of log-window
    /// - close application 
    /// The action to be executed is specified by the appropriate straing value as command parameter.
    /// Possible values and associated actions are stored in a dictionary. In case of an invalid command parameter, 
    /// a <see cref="System.NotImplementedException">NotImplementedException</see> is thrown during execution.
    /// </summary>
    public class ApplicationCommand : DelegateCommand<string>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// <see cref="IFlexiWallApplicationActions"/> which executes the actions associated with the given command parameter.
        /// </summary>
        private readonly IFlexiWallApplicationActions _vm;

        /// <summary>
        /// Dictionary associating the appropriate actions (implemented in given <see cref="FlexiWallWPF.ViewModel.IFlexiWallApplicationActions">IFlexiWallApplicationActions</see> to possible command parameters as key-values.
        /// </summary>
        private static readonly Dictionary<string, Action> _actions = new Dictionary<string, Action>(); 

        /// <summary>
        /// Initialization. Command is enabled and Dictionary is initialized.
        /// </summary>
        /// <param name="vm">object responsible to execute the associated actions for each command parameter.</param>
        public ApplicationCommand(IFlexiWallApplicationActions vm) : base(param => ExecuteAction(param))
        {
            _vm = vm;
            InitActions();
        }

        /// <summary>
        /// Adds the 6 possible parametrs and associated actions from <see cref="FlexiWallWPF.ViewModel.IFlexiWallApplicationActions">IFlexiWallApplicationActions</see>-interface.
        /// </summary>
        private void InitActions()
        {
            _actions.Add("PropertyPanelVisibility", () => _vm.TogglePropertyPanelVisibility());
            _actions.Add("Help", () => _vm.ToggleHelp());
            _actions.Add("FullScreen", () => _vm.ToggleFullScreen());
            _actions.Add("Minimize", () => _vm.ToggleAppMinimized());
            _actions.Add("ToggleLog", () => _vm.ToggleLogVisibility());
            _actions.Add("Exit", () => _vm.Exit());
        }

        /// <summary>
        /// Exectues the desired Action by matching the paramewter with the keys in the action dictionary. If the aparm is null or not a string 
        /// or there is no appropriate key, an appropriate LogMessage is submitted. Additionally, when the  provided parameter is not contained 
        /// in the dictionary an exception is thrown.
        /// </summary>
        /// <param name="s">object parameter containing the string representation of th action to be executed.</param> 
        private static void ExecuteAction(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                Logger.Error($"no argument provided for {nameof(ApplicationCommand)}.{nameof(ExecuteAction)}.");
                return;
            }

            if (_actions.ContainsKey(s))
            {
                _actions[s].Invoke();
                Logger.Info($"{nameof(ApplicationCommand)}.{nameof(ExecuteAction)} sucessfully Executed with param {s}.");
                return;
            }

            Logger.Warn($"Tried to execute {nameof(ApplicationCommand)}.{nameof(ExecuteAction)} with invalid parameter [{s}]. Parameter not found in Dictionary.");
            throw new NotImplementedException($"Value {s} is not connected with an implemented action."); 
        }
    }
}
