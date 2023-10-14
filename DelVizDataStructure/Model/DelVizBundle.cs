using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelVizDataStructure
{
    class DelVizBundle
    {
        public int Id { get; set; }
        public string Name_en { get; set; }
        public List<DelVizTag> tags {get; set;}
    }
}
