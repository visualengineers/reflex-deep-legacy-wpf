using System.Collections.Generic;
using DelVizDataStructure;
using EventHandling.Utilities;
using PhysicsSimulation.Utilities;

namespace PhysicsSimulation.Model
{
    public class SimulatedTagObject : SimulatedObject
    {
        public ObjectTag Tag { get; private set; }

        public SimilarityCoefficient Coefficients { get; private set; }
        
        public SimulatedTagObject(int id, Vector2D initialPos, ObjectTag tag) : base(id, initialPos, ObjectType.Tag)
        {
            Tag = tag;
        }

        public void InitSimilarityCoefficients(IEnumerable<SimulatedTagObject> otherTags)
        {
            Coefficients = new SimilarityCoefficient(this, otherTags);
        }
    }
}
