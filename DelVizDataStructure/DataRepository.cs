using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DelVizDataStructure.DelVizWebService;
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

        private Service1Client _client;
        private bool _enforceEqualDistribution;
        private readonly Logger _logger;
        private readonly bool _loadFromWebservice;
        private List<DelVizCategory> _vishierarchie;
        private List<DelVizItem> _visitems;

        public DataRepository(bool enforceEqualDistribution, int maxItemsPerTag, Logger logger, bool loadFromWebservice = false)
        {
            _enforceEqualDistribution = enforceEqualDistribution;
            _logger = logger;
            _loadFromWebservice = loadFromWebservice;
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

        [Obsolete("Webservice doesn't exist anymore...")]
        public bool CheckWebServiceConnectivity()
        {
            _logger.Warn($"{GetType().FullName}: Calling deprecated Method {nameof(CheckWebServiceConnectivity)}.");
            return false;

            /*
            var url = "http://141.76.66.11:8080/DelVizWebService.svc";

            try
            {
                var myRequest = (HttpWebRequest)WebRequest.Create(url);

                var response = (HttpWebResponse)myRequest.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    //  it's at least in some way responsive
                    //  but may be internally broken
                    //  as you could find out if you called one of the methods for real
                    Debug.Write(string.Format("{0} Available", url));

                    return true;
                }
                else
                {
                    //  well, at least it returned...
                    Debug.Write(string.Format("{0} Returned, but with status: {1}", url, response.StatusDescription));
                    return false;
                }
            }
            catch (Exception ex)
            {
                //  not available at all, for some reason
                Debug.Write(string.Format("{0} unavailable: {1}", url, ex.Message));
                return false;
            }
            */
        }

        [Obsolete]
        private void OpenClientConnection()
        {
            try
            {
                _client = new Service1Client();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Obsolete]
        private void CloseClientConnection()
        {
            try
            {
                _client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        private void Initialize()
        {
            if (_loadFromWebservice && CheckWebServiceConnectivity())
            {
                OpenClientConnection();
                InitCategories();
                InitData();
                CloseClientConnection();
                _client = null;
            }
            else
            {
                LoadJsonFiles();
                InitJsonCategories();
                InitJsonData();
            }
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


        [Obsolete]
        private void InitCategories()
        {
            if (_client == null)
                return;

            var categories = new List<Category>();

            //TODO: Set Count for Items, if too many
            categories.AddRange(_client.GetAllCategories());

            categories.ForEach(i => Categories.Add(new TagCategory(i.Name_en, i.ID)));

            Categories.ForEach(i => _client.GetAllBundlesInCategory(i.Id));

            foreach (var cat in Categories)
            {
                var bundle = new List<Bundle>(_client.GetAllBundlesInCategory(cat.Id));
                var tagDimensions = new List<TagDimension>();
                bundle.ForEach(i => tagDimensions.Add(new TagDimension(i.Name_en, i.ID, cat)));
                var k = 0;
                for (int index = 0; index < tagDimensions.Count; index++)
                {
                    var tagDim = tagDimensions[index];
                    if (k > 2)
                        break;
                    k++;
                    var tags = new List<Tag>(_client.GetAllTagsInBundle(tagDim.DimensionId));
                    var objectTags = new List<ObjectTag>();
                    var j = 0;
                    foreach (var tag in tags.TakeWhile(tag => j <= 2))
                    {
                        j++;
                        objectTags.Add(new ObjectTag(tag.Name_en, tag.ID, tagDim));
                    }
                    tagDim.ContainedItems.AddRange(objectTags);
                }
                cat.ContainedItems.AddRange(tagDimensions);
            }
        }

        [Obsolete]
        private void InitData()
        {
            if (_client == null)
                return;


            var listOfTags = GetAllTags();
            var listofVisualisations = new List<Visualization>(_client.GetAllVisualizations());

            foreach (var tag in listOfTags)
            {
                var exp = new TagFilterExpression { TagID = tag.Id };
                var visIds = _client.GetAllVisIDsByTags(new[] { exp });
                //TODO: Decide which method is more suitable
                if (EnforceEqualDistribution)
                {
                    foreach (var id in visIds.Take(MaxNumItemsPerTag))
                    {
                        var vis = listofVisualisations.FirstOrDefault(i => i.ID == id);
                        if (vis == null)
                            continue;
                        var dataItem = new DataItem(vis.ID, new[] { tag }, vis.Description_en, vis.Image, vis.URL,
                            vis.Title, vis.Time);
                        var dat = DataItems.FirstOrDefault(item => item.Id == vis.ID);
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
                else
                {
                    int numVis = 0;

                    foreach (var id in visIds)
                    {
                        var vis = listofVisualisations.FirstOrDefault(i => i.ID == id);
                        if (vis == null)
                            continue;
                        var dataItem = new DataItem(vis.ID, new[] { tag }, vis.Description_en, vis.Image, vis.URL,
                            vis.Title, vis.Time);
                        var dat = DataItems.FirstOrDefault(item => item.Id == vis.ID);
                        if (dat == null)
                        {
                            if (numVis >= MaxNumItemsPerTag)
                                continue;
                            DataItems.Add(dataItem);
                            tag.ContainedItems.Add(dataItem);

                            numVis++;
                        }
                        else
                        {
                            dat.AddTag(tag);
                            tag.ContainedItems.Add(dat);
                        }
                    }
                }
            }
        }

        public List<ObjectTag> GetAllTags()
        {
            var result = new List<ObjectTag>();

            Categories.ForEach(cat => cat.ContainedItems.ForEach(dim => result.AddRange(dim.ContainedItems)));

            return result;
        }

        [Obsolete]
        private void ItemsToJson()
        {
            var listofVisualisations = new List<Visualization>(_client.GetAllVisualizations());
            foreach (var item in listofVisualisations)
            {
                var listofTagsOfItem = new List<Tag>(_client.GetAllTagsOfVisualization(item.ID));
                if (listofTagsOfItem.Count != 0)
                {
                    var desc = item.Description_en;
                    if (desc.IndexOf("\"", StringComparison.Ordinal) >= 0)
                    {
                        desc = desc.Replace("\"", "'");
                    }
                    var text1 = String.Format("{{\"id\": {0}, \"title\": \"{1}\", \"image\": \"{2}\", \"description\": \"{3}\", \"url\": \"{4}\", \"category\": [", item.ID, item.Title, item.Image, desc, item.URL);
                    foreach (var tag in listofTagsOfItem)
                    {
                        text1 += "\"" + tag.Name_en + "\"";
                        text1 += ",";
                    }
                    var time = JsonConvert.SerializeObject(item.Time);
                    text1 = text1.Substring(0, text1.Length - 1) + "], \"publicationdate\": " + time + "},";
                    Console.WriteLine(text1);
                }
            }
        }

        [Obsolete]
        private void HierarchyToJson()
        {
            var listOfCategories = new List<Category>(_client.GetAllCategories());
            foreach (var item in listOfCategories)
            {
                var text = String.Format("{{\"id\": {0}, \"Name_en\": \"{1}\", \"bundles\": [", item.ID, item.Name_en);

                var listofBundlesOfCategory = new List<Bundle>(_client.GetAllBundlesInCategory(item.ID));
                foreach (var bundle in listofBundlesOfCategory)
                {
                    text += "{\"id\": " + bundle.ID + ", \"Name_en\": \"" + bundle.Name_en + "\", \"tags\": [";
                    var listofTags = new List<Tag>(_client.GetAllTagsInBundle(bundle.ID));
                    foreach (var tag in listofTags)
                    {
                        text += "{\"id\": " + tag.ID + ", \"Name_en\": \"" + tag.Name_en + "\"},";
                    }
                    text = text.Substring(0, text.Length - 1) + "]},";
                }
                text = text.Substring(0, text.Length - 1) + "]},";
                Console.WriteLine(text.Substring(0, text.Length - 1));
            }
        }
    }
}
