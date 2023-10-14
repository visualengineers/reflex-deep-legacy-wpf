using System;
using System.Collections.Generic;

namespace DelVizDataStructure
{
    public interface IDataCollection<T> where T: class
    {
        String Name { get; }

        List<T> ContainedItems { get; }
    }
}
