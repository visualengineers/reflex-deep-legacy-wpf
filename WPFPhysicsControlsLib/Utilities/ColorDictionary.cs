using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using DelVizDataStructure;

namespace WPFPhysicsControlsLib.Utilities
{
    public class ColorDictionary
    {
        private static ColorDictionary _instance;

        private static int _numCategories = 3;
        //private static int _numDimensPerCategory = 3;


        public static bool IsInitialized { get; private set; }

        public static ColorDictionary Instance
        {
            get { return _instance ?? (_instance = new ColorDictionary()); }
        }

        //TODO: Outsourcing: read from configuration xml
        private static readonly List<string> ResourceNames = new List<string>
        {
            "01DataStructureColor",
            "02DomainColor",
            "03DataTypeColor",
            "04UpdateBehaviorColor",
            "05ElementsColor",
            "06CompositionTypeColor",
            "07FunctionalityColor",
            "08InteractionTypeColor",
            "09InterfaceElementColor"
        };

        public static readonly Dictionary<int, int> TagDimensionIdResourceIdxMap = new Dictionary<int, int>
        {
            {156, 0},
            {158, 1},
            {157, 2},
            {164, 3},
            {167, 4},
            {166, 5},
            {161, 6},
            {160, 7},
            {159, 8}
        };

        public static readonly Dictionary<TagDimension, string> ColorResourceDictionary =
            new Dictionary<TagDimension, string>();

        public void InitializeDictionary(List<TagCategory> categories)
        {
            if (IsInitialized)
                return;

            IsInitialized = true;

            if (categories.Count != _numCategories)
            {
                IsInitialized = false;
                throw new ArgumentException();
            }



            categories.ForEach(cat =>
            {
                int i = 0;

                //if (cat.ContainedItems.Count != _numDimensPerCategory)
                //{
                //    IsInitialized = false;
                //    ColorResourceDictionary.Clear();
                //    throw new ArgumentException();
                //}

                cat.ContainedItems.ForEach(dim =>
                {
                    var newDimension =
                        ColorResourceDictionary.Keys.FirstOrDefault(k => dim.DimensionId.Equals(k.DimensionId)) == null;

                    int resIdx = -1;
                    bool idxFound = TagDimensionIdResourceIdxMap.TryGetValue(dim.DimensionId, out resIdx);

                    if (newDimension && idxFound)
                    {
                        ColorResourceDictionary.Add(dim, ResourceNames[resIdx]);
                        i++;
                    }
                });
            });
        }

        public Color GetColor(ObjectTag key)
        {
            var result = ColorResourceDictionary.FirstOrDefault(elem =>
                Equals(elem.Key.DimensionId, key.AssociatedDimension.DimensionId));

            if (string.IsNullOrWhiteSpace(result.Value))
                return Colors.Transparent;

            var col = (Color)Application.Current.Resources[result.Value];
            return col;
        }
    }
}
