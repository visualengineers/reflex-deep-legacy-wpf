using System;
using System.Windows.Input;
using System.Windows.Threading;
using WPFPhysicsControlsLib.ViewModel;

namespace WPFPhysicsControlsLib.Utilities
{
    public class MousePressureEmulator
    {
        private readonly DispatcherTimer _timer;
        private float _amount;
        private IModifyItemInfluence _item;

        public IModifyItemInfluence Item
        {
            get { return _item; }
            set
            {
                if (_item == value)
                    return;
                OnLeave(this, null);
                _item = value;
            }
        }

        public MousePressureEmulator()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _timer.Tick += OnTimerTick;
        }

        public MousePressureEmulator(IModifyItemInfluence viewModel)
            : this()
        {
            Item = viewModel;
        }

        public void OnButtonDown(object sender, MouseButtonEventArgs e)
        {
            bool leftButton = e.LeftButton == MouseButtonState.Pressed;
            bool rightButton = e.RightButton == MouseButtonState.Pressed;

            StartTimer(leftButton, rightButton);
        }

        public void OnButtonUp(object sender, MouseButtonEventArgs e)
        {
            bool leftButton = e.LeftButton == MouseButtonState.Pressed;
            bool rightButton = e.RightButton == MouseButtonState.Pressed;

            StartTimer(leftButton, rightButton);
        }

        public void OnLeave(object sender, MouseEventArgs e)
        {
            StartTimer(false, false);
        }

        private void OnTimerTick(object sender, EventArgs args)
        {
            if (Item == null)
                return;

            Item.ModifyItemInfluence(_amount);
        }

        private void StartTimer(bool leftButton, bool rightButton)
        {
            if (!(leftButton || rightButton))
            {
                _timer.Stop();
                return;
            }

            _timer.Start();

            if (leftButton)
                _amount = 0.3f;
            else
                _amount = -0.3f;

            _timer.Start();

        }
    }
}
