using PhysicsSimulation.Utilities;

namespace PhysicsSimulation.Model
{
    public class PhysicalObjectState
    {
        public Vector2D LastPosition { get; protected set; }

        public Vector2D CurrentPosition { get; protected set; }

        public Vector2D InitialPosition { get; protected set; }

        public float InfluenceFactor { get; set; }

        #region Constructor

        public PhysicalObjectState(Vector2D initialPos)
        {
            InitialPosition = initialPos;
            CurrentPosition = initialPos;
        }

        #endregion

        public void Move(Vector2D amount)
        {
            LastPosition = CurrentPosition;
            CurrentPosition += amount;
        }

        public void MoveTo(Vector2D newPosition)
        {
            Vector2D amount = newPosition - CurrentPosition;
            Move(amount);
        }

        public void ResetPosition()
        {
            CurrentPosition = InitialPosition;
            LastPosition = InitialPosition;
        }
    }
}
