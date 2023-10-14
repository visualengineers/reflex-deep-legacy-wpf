using System.Windows.Input;
using Prism.Ioc;
using WPFPhysicsControlsLib.ViewModel;

namespace WPFPhysicsControlsLib.Windows
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow
    {
        public SimulationVm SimulationVm { get; }

        public SimulationPropertiesVm SimPropertiesVm { get; }

        public DebugWindow()
        {
            InitializeComponent();

            SimulationVm = ContainerLocator.Current.Resolve<SimulationVm>();
            SimPropertiesVm = ContainerLocator.Current.Resolve<SimulationPropertiesVm>();

            DataContext = this;
        }

        private void DragWindow(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
