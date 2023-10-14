using System;
using DelVizDataStructure;
using PhysicsSimulation;
using PhysicsSimulation.Model;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using WPFPhysicsControlsLib.Utilities;
using WPFPhysicsControlsLib.ViewModel;

namespace WPFPhysicsControlsLib.Commands
{
    public class ReloadDataCommand : DelegateCommand
    {
        private static DataRepository _dataRepo;
        private static PhysicsSimulator _physicsSim;

        public ReloadDataCommand() : base(ReloadData)
        {
            _dataRepo = ContainerLocator.Current.Resolve<DataRepository>();
            _physicsSim = ContainerLocator.Current.Resolve<SimulationVm>().PhysicsSim;

            if (_dataRepo == null)
                throw new NullReferenceException($"Cannot retrieve {typeof(DataRepository).FullName}. Source:{GetType().FullName}");
        }

        private static void ReloadData()
        {
            _dataRepo.ReloadData();
            _physicsSim.Reset(true);
        }
    }
}
