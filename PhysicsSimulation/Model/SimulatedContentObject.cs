using System;
using System.Linq;
using System.Collections.Generic;
using DelVizDataStructure;
using EventHandling.Utilities;
using PhysicsSimulation.Utilities;

namespace PhysicsSimulation.Model
{
    public class SimulatedContentObject : SimulatedObject
    {
        public DataItem Data { get; private set; }

        public List<SimulatedTagObject> InfluencingTags { get; private set; }

        public int[] AssociatedCategories = {0, 0, 0, 0, 0, 0, 0, 0, 0};

        public List<Tuple<List<SimulatedTagObject>, List<SimulatedContentObject>>> SharedTags { get; private set; }  

        // public List<SimulatedContentObject> 

        public SimulatedContentObject(int id, Vector2D initialPos, DataItem data) : base(id, initialPos, ObjectType.Content)
        {
            Data = data;
            InfluencingTags = new List<SimulatedTagObject>();
            SharedTags = new List<Tuple<List<SimulatedTagObject>, List<SimulatedContentObject>>>();
        }

        public void UpdateAssociatedCategories(List<SimulatedTagObject> tags)
        {
            tags.ForEach(tag =>
            {
                var cat = tag.Tag.AssociatedDimension.AssociatedCategory.Id;
                var dim = tag.Tag.AssociatedDimension.DimensionId;

                var foundCatMapping = DataRepository.CategoryMapping.TryGetValue(cat, out var catMap);
                var foundDimMapping = DataRepository.DimensionOffsetMapping.TryGetValue(dim, out var dimMap);

                if (!foundCatMapping || !foundDimMapping || catMap > 2 || dimMap > 2 || catMap < 0 || dimMap < 0)
                    throw new DataMisalignedException();

                var idx = catMap * 3 + dimMap;

                AssociatedCategories[idx]++;
            });
            
        }

        public int GetNumberOfInfluencingTags(double minInfluence)
        {
            int result = 0;
            
            lock (InfluencingTags)
            {
                result = InfluencingTags.Count(tag => Math.Abs(tag.State.InfluenceFactor) > minInfluence);
            }

            return result;
        }

        public void AddInfluencingTag(SimulatedTagObject tag)
        {
            lock (InfluencingTags)
            {
                InfluencingTags.Add(tag);
            }
        }

        public void ClearInfluencingTags()
        {
            lock (InfluencingTags)
            {
                InfluencingTags.Clear();
            }
        }
    }
}
