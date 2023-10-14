using WPFPhysicsControlsLib.Utilities;
using WPFPhysicsControlsLib.ViewModel;

namespace WPFPhysicsControlsLib.Controls
{
    /// <summary>
    /// Interaction logic for ContentItemControl.xaml
    /// </summary>
    public partial class ContentItemControl
    {
        private MousePressureEmulator _mouseEmulator;

        public ContentItemControl()
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
