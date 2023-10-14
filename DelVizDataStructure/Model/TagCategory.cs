using System;
using System.Collections.Generic;

namespace DelVizDataStructure
{
    public class TagCategory : IDataCollection<TagDimension>
    {
        public String Name { get; private set; }
        public int Id { get; private set; }

        public List<TagDimension> ContainedItems { get; private set; }

        public TagCategory(String categoryName, int id)
        {
            Name = categoryName;
            Id = id;
            ContainedItems = new List<TagDimension>();
        }
    }
}
