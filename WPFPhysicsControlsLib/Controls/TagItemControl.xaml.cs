using WPFPhysicsControlsLib.Utilities;
using WPFPhysicsControlsLib.ViewModel;

namespace WPFPhysicsControlsLib.Controls
{
    /// <summary>
    /// Interaction logic for TagItemControl.xaml
    /// </summary>
    public partial class TagItemControl
    {
        private MousePressureEmulator _mouseEmulator;

        public TagItemControl()
        {
            InitializeComponent();

            DataContextChanged += delegate
            {
                if (_mouseEmulator != null)
                {
                    MouseLeftButtonDown -= _mouseEmulator.OnButtonDown;
                    MouseRightButtonDown -= _mouseEmulator.OnButtonDown;
                    MouseLeftButtonUp -= _mouseEmulator.OnButtonUp;
                    MouseRightButtonUp -= _mouseEmulator.OnButtonUp;
                    MouseLeave -= _mouseEmulator.OnLeave;
                }

                _mouseEmulator = new MousePressureEmulator(DataContext as IModifyItemInfluence);

                MouseLeftButtonDown += _mouseEmulator.OnButtonDown;
                MouseRightButtonDown += _mouseEmulator.OnButtonDown;
                MouseLeftButtonUp += _mouseEmulator.OnButtonUp;
                MouseRightButtonUp += _mouseEmulator.OnButtonUp;
                MouseLeave += _mouseEmulator.OnLeave;
            };
        }
    }
}
