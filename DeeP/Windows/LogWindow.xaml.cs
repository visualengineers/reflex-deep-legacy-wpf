using System.Windows;
using System.Windows.Input;
using NLog;
using ReFlex.Apps.DeeP.ViewModel;

namespace ReFlex.Apps.DeeP.Windows
{
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public LogWindow(LogViewModel vm)
        {
            InitializeComponent();

            WindowState = WindowState.Minimized;
            Logger.Log(LogLevel.Warn, "LogWindow created.");

            DataContext = vm;
        }

        private void MoveWindow(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
