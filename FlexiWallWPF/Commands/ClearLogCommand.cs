using FlexiWallWPF.ViewModel;
using Prism.Commands;

namespace FlexiWallWPF.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class ClearLogCommand : DelegateCommand
    {
        private static LogViewModel _vm;

        public ClearLogCommand(LogViewModel vm) : base(ClearLog)
        {
            _vm = vm;
            // CanExecute(true);
        }

        public static void ClearLog()
        {
            _vm.LogEntries.Clear();
        }
    }
}
