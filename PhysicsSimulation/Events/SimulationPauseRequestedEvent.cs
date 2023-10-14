using System;
using Prism.Events;

namespace PhysicsSimulation.Events
{
    public class SimulationPauseRequestedEvent : PubSubEvent<Tuple<bool, bool>>
    {
    }
}
