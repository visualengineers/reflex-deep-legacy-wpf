using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using NLog;
using PhysicsSimulation;
using PhysicsSimulation.Events;
using PhysicsSimulation.Utilities;
using Prism.Events;
using Prism.Ioc;
using ReFlex.Core.Common.Components;
using ReFlex.Core.Common.Util;
using WPFPhysicsControlsLib.Utilities;
using WPFPhysicsControlsLib.ViewModel;
using Math = System.Math;

namespace WPFPhysicsControlsLib.Controls
{
    /// <summary>
    /// Interaction logic for PhysicsSimCanvas.xaml
    /// </summary>
    public partial class PhysicsSimCanvas
    {

        private readonly List<Vector3D> _filteredList = new List<Vector3D>();
        private readonly MousePressureEmulator _mouseEmulator;
        private readonly Logger _logger;

        public SimulationVm SimulationVm { get; }

        public SimulationPropertiesVm SimPropertiesVm { get; }

        public PhysicsSimCanvas()
        {
            _logger = ContainerLocator.Current.Resolve<Logger>();
            SimulationVm = ContainerLocator.Current.Resolve<SimulationVm>();
            SimPropertiesVm = ContainerLocator.Current.Resolve<SimulationPropertiesVm>();

            InitializeComponent();
            
            _mouseEmulator = new MousePressureEmulator();

            var eventAggregator = ContainerLocator.Current.Resolve<IEventAggregator>();

            if (eventAggregator == null)
                throw new NullReferenceException($"Cannot retrieve {typeof(EventAggregator).FullName}. Source:{GetType().FullName}");

            eventAggregator.GetEvent<SimulationUpdatedEvent>().Subscribe((args) =>
            {
                TagsCanvas.InvalidateVisual();
            }, ThreadOption.UIThread);

            _logger.Info($"{GetType().FullName}: Initialization completed.");
        }
        

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _filteredList.Add(new Vector3D((float)e.GetPosition(this).X, (float)e.GetPosition(this).Y, 50));
            var matches = new List<Vector2D>();

            foreach (var obj in SimulationVm.Objects)
            {
                var diameter = SimPropertiesVm.ItemCollisionRatio;

                var match = _filteredList.FindAll(extr =>
                {
                    var objPos = obj.Object.State.CurrentPosition;
                    bool isAssociatedTagActive = obj.Object.InfluencingTags.Count(tag => Math.Abs(tag.State.InfluenceFactor) > 0.5) > 0;
                    return isAssociatedTagActive && Math.Abs(extr.X - objPos.X) < diameter && Math.Abs(extr.Y - objPos.Y) < diameter;
                });

                //float influence = match.Count == 0 ? 0.0f : (float)match[0].Z; // TODO: picking the first sample doesn't seem to be perfect --> compute median value ?

                //obj.Object.State.InfluenceFactor = influence;
                //obj.ModifyItemInfluence(influence - obj.ItemSelectionAmount);

                match.ForEach(v3 =>
                {
                    _filteredList.Remove(v3);
                    matches.Add(new Vector2D((float)v3.X, (float)v3.Y));
                });
            }

            matches.ForEach(extr =>
            {
                // var activeTags = ViewModelLocator.Instance.SimulationVm.ActiveTags;
                var objects = SimulationVm.Objects;
                var minDist = 40f;
                SimulatedContentItemVm nearestObj = null;
                // var minDist = objects.Select(objVm => (objVm.Object.State.CurrentPosition - pos).Length).Concat(new[] { tagRadius }).Min();
                foreach (var objVm in objects)
                {
                    var d = (objVm.Object.State.CurrentPosition - extr).Length;
                    if (minDist > d)
                    {
                        minDist = d;
                        nearestObj = objVm;
                    }
                }

                _mouseEmulator.Item =nearestObj;
            });

            _mouseEmulator.OnButtonDown(sender, e);

            SimulationVm.UpdateCurrentlyDetectedTouchPoints(matches.Select(v2 => new Interaction(
                new Point3(v2.X, v2.Y, 1), InteractionType.None, 30f)).ToList());
        }


        private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _mouseEmulator.OnButtonUp(sender, e);
        }
    }
}
