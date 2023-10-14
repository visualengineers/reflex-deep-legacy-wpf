using System;
using System.Collections.Generic;
using System.Linq;

namespace DelVizDataStructure
{
    public class DataItem
    {
        public int Id { get; private set; }

        public List<ObjectTag> Tags { get; private set; }

        public int[] AssociatedDimensions = {0, 0, 0, 0, 0, 0, 0, 0, 0};

        //TODO: could be more generic; maybe configurable via app.config ?
        public static readonly string[] AssociatedDimensionNames = { "update", "elements", "dimension", "functions",  "modality", "control element", "data structure",  "domain", "data type" };

        public String Description { get; private set; }

        public string ImageLocation { get; private set; }

        public DateTime CreationDate { get; private set; }

        public String Address { get; private set; }

        public String Title { get; private set; }

        public DataItem(int id, IEnumerable<ObjectTag> tags, String desc, String imgLoc, String address, String title, DateTime creationDate)
        {
            Id = id;
            Tags = new List<ObjectTag>();
            foreach (var objectTag in tags)
            {
                AddTag(objectTag);
            }
            Description = desc;
            ImageLocation = "/resources"+imgLoc;
            Address = address;
            Title = title;
            CreationDate = creationDate;
        }

        public void AddTag(ObjectTag tag)
        {
            var existingTag = Tags.FirstOrDefault(t => Equals(t.Id, tag.Id));
            if (existingTag != null)
                return;
            Tags.Add(tag);
            var idx = GetAssociatedDimensionIdx(tag);
            if (idx > 0 && idx < AssociatedDimensions.Length)
                AssociatedDimensions[idx]++;
        }

        public int GetAssociatedDimensionIdx(ObjectTag tag)
        {
            return AssociatedDimensionNames.ToList().IndexOf(tag.AssociatedDimension.Name.Trim().ToLower());
        }
    }

}
