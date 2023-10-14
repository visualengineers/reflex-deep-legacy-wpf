using PhysicsSimulation.Utilities;

namespace PhysicsSimulation.Model
{
    public static class PhysicsSimulationProperties
    {
        private static Vector2D _itemsCenter;
        private static Vector2D _tagsCenter;
        private static float _tagCanvasHeight;
        private static float _tagCanvasWidth;
        private static float _itemCanvasHeight;
        private static float _itemCanvasWidth;

        public static float ObjectForceModifier { get; set; }

        public static float TagForceModifier { get; set; }

        public static float TagItemRepulsionModifier { get; set; }

        public static float BoundaryForceModifier { get; set; }

        public static float ItemsCanvasWidth
        {
            get { return _itemCanvasWidth; }
            set
            {
                _itemCanvasWidth = value;
                UpdateItemsCanvasCenter();
            }
        }

        public static float ItemsCanvasHeight
        {
            get { return _itemCanvasHeight; }
            set
            {
                _itemCanvasHeight = value;
                UpdateItemsCanvasCenter();
            }
        }

        public static Vector2D ItemsCanvasCenter {
            get
            {
                if (_itemsCenter == null)
                    UpdateItemsCanvasCenter();
                return _itemsCenter;
            }
        }


        public static float TagsCanvasWidth
        {
            get { return _tagCanvasWidth; }
            set
            {
                _tagCanvasWidth = value;
                UpdateTagsCanvasCenter();
            }
        }

        public static float TagsCanvasHeight
        {
            get { return _tagCanvasHeight; }
            set
            {
                _tagCanvasHeight = value;
                UpdateTagsCanvasCenter();
            }
        }

        public static Vector2D TagsCanvasCenter
        {
            get
            {
                if (_tagsCenter == null)
                    UpdateTagsCanvasCenter();
                return _tagsCenter;
            }
        }

        public static float ItemSize { get; set; }

        public static float ItemBaseSize { get; set; }

        public static float TagSize { get; set; }

        public static float TagBaseSize { get; set; }

        public static float MinDistSquared { get; set; }

        public static float StandardTagAttraction { get; set; }

        public static float StandardTagRepulsion { get; set; }

        public static float TagSameDimensionForceModifier { get; set; }

        public static float TagSameCategoryForceModifier { get; set; }


        public static int SimulationTimerInterval { get; set; }

        public static float SelectedTagAttributionModifier { get; set; }

        public static float UnselectedTagRepulsionModifier { get; set; }
        public static bool ComputeRepulsionUnselected { get; set; }

        public static bool DetectCollisions { get; set; }

        public static int CollisionDetectionIterations { get; set; }

        public static float CentripetalSpeed { get; set; }

        public static float MaxUserInfluenceValue { get; set; }

        public static int ProcessDepthOption { get; set; }

        public static bool EnforceEqualDataDistribution { get; set; }

        public static int MaxNumItemsPerTag { get; set; }

        private static void UpdateItemsCanvasCenter()
        {
            _itemsCenter = new Vector2D(0.5f*ItemsCanvasWidth, 0.5f*ItemsCanvasHeight);
        }

        private static void UpdateTagsCanvasCenter()
        {
            _tagsCenter = new Vector2D(0.5f * TagsCanvasWidth, 0.5f * TagsCanvasHeight);
        }
    }
}
