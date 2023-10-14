using EventHandling.Utilities;
using PhysicsSimulation.Utilities;

namespace PhysicsSimulation.Model
{
    public abstract class SimulatedObject
    {
        public int Id { get; private set; }
        public PhysicalObjectState State { get; private set; }

        protected SimulatedObject(int id, Vector2D initialPos, ObjectType type)
        {
            Id = id;
            State = new PhysicalObjectState(initialPos);
            Type = type;
        }

        public ObjectType Type { get; private set; }
    }
}
