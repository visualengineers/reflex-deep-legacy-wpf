using PhysicsSimulation.Utilities;

namespace PhysicsSimulation.Model
{
    public class SimulatedAssociationObject
    {
        public SimulatedContentObject AssociatedContentItem { get; private set; }

        public SimulatedTagObject AssociatedTagObject { get; private set; }

        public PhysicalObjectState CenterState { get; private set; }

        public PhysicalObjectState DirectionState { get; private set; }

        public float ActivityMeasure { get; set; }

        public float InfluenceMeasure { get; set; }

        public Vector2D NormalizedDirection { get; private set; }

        public SimulatedAssociationObject(SimulatedContentObject entity, SimulatedTagObject tag, Vector2D normalizedDirection)
        {
            AssociatedContentItem = entity;
            AssociatedTagObject = tag;
            NormalizedDirection = normalizedDirection;

            CenterState = new PhysicalObjectState(ComputeCenter());
            DirectionState = new PhysicalObjectState(ComputeDirection());
        }

        public void Update()
        {
            CenterState.MoveTo(ComputeCenter());
            DirectionState.MoveTo(ComputeDirection());
            InfluenceMeasure = AssociatedContentItem.State.InfluenceFactor;
            var influence = ComputeInfluence();
            CenterState.InfluenceFactor = influence;
            DirectionState.InfluenceFactor = influence;
            ActivityMeasure = influence;
        }

        private Vector2D ComputeCenter()
        {
            var result = new Vector2D(0, 0);

            if (AssociatedContentItem == null || AssociatedContentItem.InfluencingTags.Count == 0)
                return result;

            var i = 0;

            AssociatedContentItem.InfluencingTags.ForEach(tag =>
            {
                result += tag.State.CurrentPosition;
                i++;
            });

            //result += AssociatedTagObject.State.CurrentPosition;
            //i++;

            result /= i;

            return result;
        }

        private Vector2D ComputeDirection()
        {
            var result = new Vector2D(0, 0);

            if (CenterState == null)
                return result;

            var start = AssociatedTagObject.State.CurrentPosition;
            var end = AssociatedContentItem.State.CurrentPosition;

            var associationCenter = 0.5f*(start - end);

            
            var dist = associationCenter.Length;
            // TODO: Configurable min length via App.Config
            dist = dist < 100 ? 0 : dist;

            var target = end + dist*NormalizedDirection;

            return target;
        }

        private float ComputeInfluence()
        {
            return AssociatedContentItem.InfluencingTags.Count;
        }

    }
}
