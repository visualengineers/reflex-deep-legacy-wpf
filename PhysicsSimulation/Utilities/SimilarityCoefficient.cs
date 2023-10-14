using System;
using System.Collections.Generic;
using System.Linq;
using DelVizDataStructure;
using PhysicsSimulation.Model;
using Prism.Ioc;

namespace PhysicsSimulation.Utilities
{
    public class SimilarityCoefficient
    {
        private readonly SimulatedTagObject _object;

        public Dictionary<SimulatedTagObject, Tuple<float, float>> CoefficientDictionary;

        public SimilarityCoefficient(SimulatedTagObject ownerObject, IEnumerable<SimulatedTagObject> otherObjects )
        {
            _object = ownerObject;
            CoefficientDictionary = new Dictionary<SimulatedTagObject, Tuple<float, float>>();
            Init(otherObjects);
        }

        private void Init(IEnumerable<SimulatedTagObject> otherObjects)
        {

            var repository = ContainerLocator.Current.Resolve(typeof(DataRepository)) as DataRepository;

            if (repository == null)
                throw new NullReferenceException($"Cannot retrieve {typeof(DataRepository).FullName}. Source:{GetType().FullName}");

            var otherTags = repository.GetAllTags().Where(t => !t.Equals(_object.Tag)).ToList();
            otherTags.ForEach(other =>
            {
                var numShared = other.ContainedItems.Where(i => i.Tags.FirstOrDefault(t => t.Id == _object.Tag.Id) != null).ToList().Count;
                var numDifferent = other.ContainedItems.Where(i => i.Tags.FirstOrDefault(t => t.Id == _object.Tag.Id) == null).ToList().Count;
                var total = other.ContainedItems.Count;

                if (numShared + numDifferent != total)
                    throw new Exception("Berechnung der Übereinstimmungen fehlgeschlagen !");

                var forceModifier = 1.0f;
                forceModifier *= other.AssociatedDimension.Equals(_object.Tag.AssociatedDimension)
                    ? PhysicsSimulationProperties.TagSameDimensionForceModifier
                    : 1.0f;

                forceModifier *= other.AssociatedDimension.AssociatedCategory.Equals(_object.Tag.AssociatedDimension.AssociatedCategory)
                    ? PhysicsSimulationProperties.TagSameCategoryForceModifier
                    : 1.0f;

                var attraction = 1.0f*forceModifier*PhysicsSimulationProperties.StandardTagAttraction*numShared;
                var repulsion = forceModifier*PhysicsSimulationProperties.StandardTagRepulsion*numDifferent;

               var coefficient = Tuple.Create(-FastSquareRoot.Sqrt(attraction), FastSquareRoot.Sqrt(repulsion));
                var associatedObject = otherObjects.FirstOrDefault(obj => obj.Tag.Equals(other));

                if (associatedObject == null)
                    return;

                CoefficientDictionary.Add(associatedObject, coefficient);
            });

        }

    }
}
