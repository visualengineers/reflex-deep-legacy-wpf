using System;
using System.Collections.Generic;

namespace DelVizDataStructure
{
    public class TagDimension : IDataCollection<ObjectTag>
    {
        public String Name { get; private set; }

        public int DimensionId { get; private set; }

        public List<ObjectTag> ContainedItems { get; private set; }

        public TagCategory AssociatedCategory { get; private set; }

        public TagDimension(string dimensionName, int dimensionId, TagCategory associatedCategory)
        {
            Name = dimensionName;
            DimensionId = dimensionId;
            ContainedItems = new List<ObjectTag>();
            AssociatedCategory = associatedCategory;
        }
    }
}
