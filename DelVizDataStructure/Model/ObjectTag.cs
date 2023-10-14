using System;
using System.Collections.Generic;

namespace DelVizDataStructure
{
    public class ObjectTag : IDataCollection<DataItem>
    {
        public String Name { get; private set; }

        public int Id { get; private set; }

        public List<DataItem> ContainedItems { get; private set; }

        public TagDimension AssociatedDimension { get; private set; }

        public ObjectTag(string tagName, int id, TagDimension associatedDimension)
        {
            Name = tagName;
            Id = id;
            AssociatedDimension = associatedDimension;
            ContainedItems = new List<DataItem>();
        }
    }
}
