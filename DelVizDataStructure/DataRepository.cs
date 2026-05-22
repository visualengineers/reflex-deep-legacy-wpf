using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using NLog;

namespace DelVizDataStructure
{
    public class DataRepository
    {
        public List<TagCategory> Categories { get; }
        public List<DataItem> DataItems { get; }

        /// <summary>
        /// Maps Category IDs to visual Index
        /// @todo: read from json / config file
        /// </summary>
        public static readonly Dictionary<int, int> CategoryMapping = new Dictionary<int, int>()
        {
            {1, 2}, // data is the last
            {2, 0}, // visualization is the first
            {3, 1}
        };

        
        /// <summary>
        /// maps the offset of each dimension inside the category
        /// @todo: read from json / config file
        /// </summary>
        public static readonly Dictionary<int, int> DimensionOffsetMapping = new Dictionary<int, int>()
        {
            { 156, 0 },
            { 157, 1 },
            { 158, 2 },

            { 164, 0 },
            { 166, 2 },
            { 167, 1 },

            { 159, 2 },
            { 160, 1 },
            { 161, 0 }
        };



        public bool EnforceEqualDistribution
        {
            get => _enforceEqualDistribution;
            set
            {
                if (_enforceEqualDistribution == value)
                    return;
                _enforceEqualDistribution = value;
                ReloadData();
            }
        }

        public int MaxNumItemsPerTag { get; set; }
        
        private bool _enforceEqualDistribution;
        private readonly Logger _logger;
        private List<DelVizCategory> _vishierarchie;
        private List<DelVizItem> _visitems;

        public DataRepository(bool enforceEqualDistribution, int maxItemsPerTag, Logger logger)
        {
            _enforceEqualDistribution = enforceEqualDistribution;
            _logger = logger;
            MaxNumItemsPerTag = maxItemsPerTag;
            Categories = new List<TagCategory>();
            DataItems = new List<DataItem>();

            Initialize();

            _logger.Info($"{GetType().FullName}: Initialization completed.");
        }

        public void ReloadData()
        {
            Categories.Clear();
            DataItems.Clear();
            Initialize();

            _logger.Info($"{GetType().FullName}: Reload data completed.");
        }

        private void Initialize()
        {
            LoadJsonFiles();
            InitJsonCategories();
            InitJsonData();
        }

        private void LoadJsonFiles()
        {
            var filenameHierarchie = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/Resources/delviz_hierarchy.json";
            var filenameItems = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/Resources/delviz_visitems.json";
            _vishierarchie = JsonConvert.DeserializeObject<List<DelVizCategory>>(File.ReadAllText(filenameHierarchie));
            _visitems = JsonConvert.DeserializeObject<List<DelVizItem>>(File.ReadAllText(filenameItems));
        }

        private void InitJsonCategories()
        {
            var i = 0;
            foreach (var cat in _vishierarchie)
            {
                if (i >= 3 || !EnforceEqualDistribution) 
                    continue;

                var j = 0;
                var category = new TagCategory(cat.Name_en, cat.Id);
                foreach (var bundle in cat.bundles)
                {
                    if (j < 3 && EnforceEqualDistribution)
                    {
                        var k = 0;
                        var bun = new TagDimension(bundle.Name_en, bundle.Id, category);
                        foreach (var tag in bundle.tags)
                        {
                            if (k < 3 && EnforceEqualDistribution)
                            {
                                var t = new ObjectTag(tag.Name_en, tag.Id, bun);
                                bun.ContainedItems.Add(t);
                            }
                            k++;
                        }
                        category.ContainedItems.Add(bun);
                    }
                    j++;
                }
                Categories.Add(category);
                i++;
            }

            _logger.Info($"{GetType().FullName}: Loaded Categories from JSON. Number of Categories: {Categories.Count}.");
        }

        private void InitJsonData()
        {
            var alltags = GetAllTags();
            if (EnforceEqualDistribution)
            {
                foreach (var tag in alltags)
                {
                    var visItems = _visitems.Where(x => x.Category.Contains(tag.Name));
                    foreach (var item in visItems.Take(MaxNumItemsPerTag))
                    {
                        var dataItem = new DataItem(item.Id, new[] { tag }, item.Description, item.Image, item.Url, item.Title, item.Publicationdate);
                        var dat = DataItems.FirstOrDefault(x => x.Id == item.Id);
                        if (dat == null)
                        {
                            DataItems.Add(dataItem);
                            tag.ContainedItems.Add(dataItem);
                        }
                        else
                        {
                            dat.AddTag(tag);
                            tag.ContainedItems.Add(dat);
                        }
                    }
                }
            }
            else
            {
                int i = 0;
                foreach (var item in _visitems)
                {
                    if (i < MaxNumItemsPerTag * 27)
                    {
                        var categories = new List<ObjectTag>();
                        foreach (var tag in item.Category)
                        {
                            categories.AddRange(alltags.Where(x => x.Name.Equals(tag)));
                        }

                        var dataItem = new DataItem(item.Id, categories, item.Description, item.Image, item.Url, item.Title, item.Publicationdate);

                        DataItems.Add(dataItem);
                        foreach (var tag in categories)
                        {
                            tag.ContainedItems.Add(dataItem);
                        }
                    }
                    i++;
                }
            }

            _logger.Info($"{GetType().FullName}: Loaded {DataItems.Count} Data items from JSON.");
        }
        
        public List<ObjectTag> GetAllTags()
        {
            var result = new List<ObjectTag>();

            Categories.ForEach(cat => cat.ContainedItems.ForEach(dim => result.AddRange(dim.ContainedItems)));

            return result;
        }
        
    }
}
