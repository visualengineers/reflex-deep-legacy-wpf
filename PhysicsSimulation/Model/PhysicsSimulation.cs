using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DelVizDataStructure;
using EventHandling.Utilities;
using PhysicsSimulation.Events;
using PhysicsSimulation.Utilities;
using Prism.Events;
using Prism.Ioc;

namespace PhysicsSimulation.Model
{
    /// <summary>
    /// Core class to initializing and updating positions of Objects.
    /// On Initialized by Reset() Method and Updated by UpdateSimulation().
    /// Includes computation oif forces between Objects (to prevent collisions to a certain degree), Tags (to create a natural distribution 
    /// of Tags according to their importance (based on SimilarityCoefficients) and Tags and Objects (prevent collisions and simulate 
    /// user weightings on each Tag). Explicit collision handling can additionally be turned on or off.
    /// </summary>
    public class PhysicsSimulator
    {
        #region Fields

        /// <summary>
        /// Current frame number of the simulation.
        /// </summary>
        private int _timestamp;

        /// <summary>
        /// Bool to store whether the canvas properties (accessible via <see cref="PhysicsSimulationProperties">PhysicsSimaluationProperties</see>) have changed. 
        /// In case the Properties have changed; positions need to be recalculated upon reset.
        /// </summary>
        private bool _canvasPropertiesChanged = true;

        private readonly IEventAggregator _eventAggregator;

        private readonly DataRepository _dataRepo;

        #endregion

        #region Properties

        public bool IsPaused { get; private set; }
        
        public bool IsCalibrating { get; private set; }

        /// <summary>
        /// List of Objects which are simulated. Need to be initialzed after creating the class creating the class 
        /// and recreated when the simulation is reset (<see cref="Reset()">Reset()</see>). 
        /// Objects interact with each other by different attraction and repulsion forces and forces origginating from Tags.
        /// </summary>
        public List<SimulatedContentObject> Objects { get; private set; }

        /// <summary>
        /// List of Tags which are influencing the objects with physical attraction and repulsion forces. 
        /// Need to be initialzed after creating the class and recreated when the simulation is reset (<see cref="Reset()">Reset()</see>). 
        /// </summary>
        public List<SimulatedTagObject> Tags { get; private set; }

        /// <summary>
        /// List of Associations between Tags and Entities. 
        /// Need to be initialzed after creating the class and recreated when the simulation is reset (<see cref="Reset()">Reset()</see>). 
        /// </summary>
        public List<SimulatedAssociationObject> Associations { get; private set; }

        public List<SimulatedAssociationObject> RelevantAssociations { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor. Creates the empty list of Objects and Tags and subscribes to relevant events.
        /// </summary>
        public PhysicsSimulator()
        {
            Objects = new List<SimulatedContentObject>();
            Tags = new List<SimulatedTagObject>();
            Associations = new List<SimulatedAssociationObject>();
            RelevantAssociations = new List<SimulatedAssociationObject>();

            _eventAggregator = ContainerLocator.Current.Resolve(typeof(IEventAggregator)) as IEventAggregator;

            if (_eventAggregator == null)
                throw new NullReferenceException($"Cannot retrieve {typeof(EventAggregator).FullName}. Source:{GetType().FullName}");

            _eventAggregator.GetEvent<CanvasSizeChangedEvent>().Subscribe(delegate { _canvasPropertiesChanged = true; });
            _eventAggregator.GetEvent<InterTagForcesChangedEvent>().Subscribe(delegate { InitTagsSimilarityCoefficients(); });
            _eventAggregator.GetEvent<SimulationPauseRequestedEvent>().Subscribe(HandlePauseRequested);

            _dataRepo = ContainerLocator.Current.Resolve(typeof(DataRepository)) as DataRepository;

            if (_dataRepo == null)
                throw new NullReferenceException($"Cannot retrieve {typeof(DataRepository).FullName}. Source:{GetType().FullName}");


            _dataRepo.EnforceEqualDistribution = PhysicsSimulationProperties.EnforceEqualDataDistribution;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Core method to trigger a positional update of Tags and Objects. New positions are computed based on the forces between the items. 
        /// Tag positions are updated first, Object positions are computed afterwards.
        /// Increments current Timestamp and fires <see cref="SimulationUpdatedEvent">SimulationUpdatedEvent</see> when finished.
        /// </summary>
        public void UpdateSimulation(long frameTime)
        {
            if (!IsPaused)
            {
                var t = frameTime > 0 ? frameTime / 40.0f : 1.0f;

                UpdateTagPositions(t);
                UpdateItemPositions(t);
                UpdateAssociations();
            }

            _eventAggregator?.GetEvent<SimulationUpdatedEvent>().Publish(_timestamp);
            _timestamp++;
        }

        /// <summary>
        /// Restes the simulation in its original state. If Canvas properties didn't change, all Objects and Tags are set back to their inital position only.
        /// However, if Canvas properties have changed, all positions are recreated by reinitializing all Object lists. First initialization can also be done by calling
        /// this method. Computation of positions is done by calculating random numbers accroding to given canvas dimensions 
        /// (<see cref="InitEntities">InitEntities()</see> and <see cref="InitTags">InitTagsWithDummyDelVizData()</see>)
        /// </summary>
        public void Reset(bool dataSetHasChanged = false)
        {
            var completeRefresh = _canvasPropertiesChanged || dataSetHasChanged;

            if (completeRefresh)
            {
                Tags.Clear();
                Objects.Clear();
                InitEntities();
                InitTags();
                InitAssociations();
                _canvasPropertiesChanged = false;
            }
            else
            {
                Tags.ForEach(tag => tag.State.ResetPosition());
                Objects.ForEach(item => item.State.ResetPosition());
                Associations.ForEach(item =>
                {
                    item.CenterState.ResetPosition();
                    item.DirectionState.ResetPosition();
                });
            }

            _eventAggregator?.GetEvent<ObjectsInitializedEvent>().Publish(completeRefresh);
        }

        #endregion

        #region Auxiliary Methods

        #region Update Methods

        private void UpdateTagPositions(float frameTimeFactor)
        {
            var newPositions = new Dictionary<SimulatedObject, Vector2D>();
            Parallel.ForEach(Tags, tag =>
            {
                var resultingForce = new Vector2D(0, 0);

                foreach (var coeff in tag.Coefficients.CoefficientDictionary)
                {

                    var tagForce = ComputeForceBetweenTags(tag.State.CurrentPosition, coeff.Key.State.CurrentPosition, coeff.Value, PhysicsSimulationProperties.TagForceModifier, tag.State.InfluenceFactor, coeff.Key.State.InfluenceFactor);
                    var repulsionForce = ComputeForceByDistanceToOtherObject(tag.State.CurrentPosition, coeff.Key.State.CurrentPosition, PhysicsSimulationProperties.TagForceModifier);

                    // still need standard repulsion to prevent collisions
                    resultingForce += repulsionForce;

                    // Add force to total force
                    resultingForce += tagForce;
                }

                var centripetalForce = ComputeCentripetalForce(tag.State.CurrentPosition,
                    PhysicsSimulationProperties.TagsCanvasCenter, tag.Id) * frameTimeFactor;

                resultingForce *= frameTimeFactor;

                var absInfluence = Math.Abs(tag.State.InfluenceFactor);

                
                // only compute new tag position when no user-interaction takes place on it
                var newPos = absInfluence < 0.1 ? (HandleBorderCollision(tag, resultingForce) + centripetalForce) : tag.State.CurrentPosition;

                lock (newPositions)
                    newPositions.Add(tag, newPos);
            });

            foreach (var pos in newPositions)
            {
                pos.Key.State.MoveTo(pos.Value);
            }
        }

        private void UpdateItemPositions(float frameTimeFactor)
        {
            var newPositions = new Dictionary<SimulatedObject, Vector2D>();
            Parallel.ForEach(Objects, obj =>
            {
                obj.ClearInfluencingTags();

                var otherObjects = Objects.Where(o => !o.Equals(obj)).ToList();
                var resultingForce = new Vector2D(0, 0);

                // compute repulsion forces for all other objects
                otherObjects.ForEach(other =>
                {
                    var repulsionForce = ComputeForceByDistanceToOtherObject(obj.State.CurrentPosition, other.State.CurrentPosition, PhysicsSimulationProperties.ObjectForceModifier);

                    repulsionForce *= 1 + Math.Abs(obj.State.InfluenceFactor);

                    // Add force to total force
                    resultingForce += repulsionForce;

                });

                //reset influenceFactor;
                obj.State.InfluenceFactor = 0;

                var tagPos = new List<Vector2D>();

                // Parallel.ForEach(Tags, tag =>
                Tags.ForEach(tag =>
                {
                    //lock (tagPos)
                    //{
                    tagPos.Add(tag.State.CurrentPosition);
                    //}

                    // var filterForce = new Vector2D(0, 0);

                    var repulsionForce = ComputeForceByDistanceToOtherObject(obj.State.CurrentPosition,
                        GetTagCoordinatesInObjectSpace(tag.State.CurrentPosition), PhysicsSimulationProperties.ObjectForceModifier * PhysicsSimulationProperties.TagItemRepulsionModifier);

                    if (Math.Abs(tag.State.InfluenceFactor) > Double.Epsilon)
                    {
                        //add influence factor if appropriate
                        // if (tag.Tag.ContainedItems.Contains(obj.Data))
                        if (tag.Tag.ContainedItems.FirstOrDefault(it => it.Id.Equals(obj.Data.Id)) != null)
                        {
                            obj.State.InfluenceFactor += tag.State.InfluenceFactor;
                            obj.AddInfluencingTag(tag);
                        }


                        var filterForce = ComputeForceForCurrentFilter(obj.State.CurrentPosition,
                            tag.State.CurrentPosition, tag.Tag, obj.Data.Tags, tag.State.InfluenceFactor);

                        resultingForce += filterForce;
                    }

                    //lock (resultingForce)
                    //{
                    resultingForce += repulsionForce;
                    //}
                });

                var centripetalForce = ComputeCentripetalForce(obj.State.CurrentPosition,
                    PhysicsSimulationProperties.ItemsCanvasCenter, obj.Id) * frameTimeFactor;

                resultingForce *= frameTimeFactor;

                var newPos = HandleBorderCollision(obj, resultingForce) + centripetalForce;

                var posCollision = HandleCollisionWithTags(obj.State.CurrentPosition, newPos, tagPos);

                var diff = (posCollision - obj.State.CurrentPosition);
                var l = diff.Length;


                //clamp maximum speed
                if (diff.Length > 5)
                    posCollision = obj.State.CurrentPosition + 5 * (diff / l);

                lock (newPositions)
                    newPositions.Add(obj, posCollision);
            });

            // Parallel.ForEach(newPositions, pos =>
            foreach (var pos in newPositions)
            {
                pos.Key.State.MoveTo(pos.Value);
            }
        }

        public void UpdateAssociations()
        {
            Parallel.ForEach(Associations, assoc =>
            {
                assoc.Update();
            });

            RelevantAssociations = Associations.Where(assoc => assoc.InfluenceMeasure > 1f).ToList();
        }

        #endregion

        #region Initialization Methods

        [Obsolete]
        private void InitWithDummyData()
        {
            // var rand = new Random();
            //var max = Convert.ToInt32(PhysicsSimulationProperties.ForceFieldDiameter / 2);
            //var min = -max;
            //for (int i = 0; i < 50; i++)
            //{
            //    var x = rand.Next(min, max);
            //    var y = rand.Next(min, max);
            //    var obj = new SimulatedObject(i, new Vector2D(x,y), ObjectType.Tag);
            //    Objects.Add(obj);
            //}
        }

        private void InitEntities()
        {
            var rand = new Random();
            var maxX = Convert.ToInt32(PhysicsSimulationProperties.ItemsCanvasWidth - PhysicsSimulationProperties.ItemSize);
            var minX = Convert.ToInt32(PhysicsSimulationProperties.ItemSize);
            var maxY = Convert.ToInt32(PhysicsSimulationProperties.ItemsCanvasHeight - PhysicsSimulationProperties.ItemSize);
            var minY = Convert.ToInt32(PhysicsSimulationProperties.ItemSize);
            var dataObjects = _dataRepo.DataItems;
            var i = 0;
            dataObjects.ForEach(data =>
            {
                var x = rand.Next(minX, maxX);
                var y = rand.Next(minY, maxY);
                var obj = new SimulatedContentObject(i, new Vector2D(x, y), data);
                Objects.Add(obj);
                i++;
            });
        }

        private void InitTags()
        {
            var allTags = _dataRepo.GetAllTags();
            int idx = 0;
            var rand = new Random();
            var maxX = Convert.ToInt32(PhysicsSimulationProperties.TagsCanvasWidth - PhysicsSimulationProperties.TagSize);
            var minX = Convert.ToInt32(PhysicsSimulationProperties.TagSize);
            var maxY = Convert.ToInt32(PhysicsSimulationProperties.TagsCanvasHeight - PhysicsSimulationProperties.TagSize);
            var minY = Convert.ToInt32(PhysicsSimulationProperties.TagSize);

            allTags.ForEach(tag =>
            {
                var x = rand.Next(minX, maxX);
                var y = rand.Next(minY, maxY);
                var obj = new SimulatedTagObject(idx, new Vector2D(x, y), tag);
                Tags.Add(obj);
                idx++;
            });

            InitTagsSimilarityCoefficients();

        }

        private void InitAssociations()
        {
            var baseAngle = (360.0f / Tags.Count) * (Math.PI / 180.0f);

            Objects.ForEach(entity =>
            {
                entity.Data.Tags.ForEach(tag =>
                {
                    var tagObj = Tags.FirstOrDefault(t => Equals(t.Tag, tag));
                    if (tagObj != null)
                    {
                        int idx = entity.Data.GetAssociatedDimensionIdx(tag);

                        if (idx >= 0)
                        {
                            var angle = baseAngle * idx;
                            var x = Convert.ToSingle(Math.Sin(angle));
                            var y = Convert.ToSingle(Math.Cos(angle));
                            var direction = new Vector2D(x, -y);

                            Associations.Add(new SimulatedAssociationObject(entity, tagObj, direction));
                        }
                    }
                });
            });

            Parallel.ForEach(Objects, obj =>
            {
                Objects.ForEach(anotherObj =>
                {
                    var sharedTags = anotherObj.Data.Tags.Where(t => obj.Data.Tags.Contains(t)).ToList();
                    var num = sharedTags.Count;
                    if (num > 2)
                    {
                        var sameSharedTags =
                            obj.SharedTags.FirstOrDefault(
                                tuple =>
                                    tuple.Item1.Count == num &&
                                    tuple.Item1.TrueForAll(tag => sharedTags.Contains(tag.Tag)));
                        if (sameSharedTags == null)
                        {
                            var sharedsimTags = Tags.Where(t => sharedTags.Contains(t.Tag)).ToList();
                            obj.SharedTags.Add(
                                new Tuple<List<SimulatedTagObject>, List<SimulatedContentObject>>(sharedsimTags,
                                    new List<SimulatedContentObject>() { obj, anotherObj }));
                            obj.UpdateAssociatedCategories(sharedsimTags);
                        }
                        else
                        {
                            sameSharedTags.Item2.Add(anotherObj);
                        }
                    }
                });
            });
        }

        private void InitTagsSimilarityCoefficients()
        {
            Tags.ForEach(tag => tag.InitSimilarityCoefficients(Tags.Where(t => !t.Equals(tag))));
        }

        #endregion

        #region Auxiliary Methods to Compute Forces

        private Vector2D HandleBorderCollision(SimulatedObject obj, Vector2D resultingForce)
        {
            var resultingForceClamped = Vector2D.ClampToMaxLength(resultingForce, 100);

            // compute repulsion force for Border
            var newPos = obj.State.CurrentPosition + resultingForceClamped;
            var size = obj.Type == ObjectType.Content
                ? PhysicsSimulationProperties.ItemSize
                : PhysicsSimulationProperties.TagSize;

            var width = obj.Type == ObjectType.Content
                ? PhysicsSimulationProperties.ItemsCanvasWidth
                : PhysicsSimulationProperties.TagsCanvasWidth;

            var height = obj.Type == ObjectType.Content
                ? PhysicsSimulationProperties.ItemsCanvasHeight
                : PhysicsSimulationProperties.TagsCanvasHeight;

            var clampedPos = newPos.ClampToBounds(size, size, width - size, height - size);

            // project position on Border to get perpendicular forces
            var xMinPos = new Vector2D(0, clampedPos.Y);
            var xMaxPos = new Vector2D(width, clampedPos.Y);
            var yMinPos = new Vector2D(clampedPos.X, 0);
            var yMaxPos = new Vector2D(clampedPos.X, height);

            // compute perpendicular forces
            var repulsionForceMinX = ComputeForceByDistanceToOtherObject(clampedPos, xMinPos, PhysicsSimulationProperties.BoundaryForceModifier, false);
            var repulsionForceMaxX = ComputeForceByDistanceToOtherObject(clampedPos, xMaxPos, PhysicsSimulationProperties.BoundaryForceModifier, false);
            var repulsionForceMinY = ComputeForceByDistanceToOtherObject(clampedPos, yMinPos, PhysicsSimulationProperties.BoundaryForceModifier, false);
            var repulsionForceMaxY = ComputeForceByDistanceToOtherObject(clampedPos, yMaxPos, PhysicsSimulationProperties.BoundaryForceModifier, false);

            var boundaryForce = repulsionForceMinX + repulsionForceMaxX + repulsionForceMinY + repulsionForceMaxY;

            return clampedPos + boundaryForce;
        }

        private Vector2D ComputeForceByDistanceToOtherObject(Vector2D objectPosition, Vector2D otherPosition, float factor, bool squaredFFallOff = true)
        {
            // compute vector between points -> direction of repulsion
            var direction = (objectPosition - otherPosition);

            // compute distance squared -> repulsion decreases over distance (squared decay)
            var fallOffExp = squaredFFallOff ? direction.LengthSquared : direction.Length;
            if (Math.Abs(fallOffExp) < float.Epsilon)
                fallOffExp = PhysicsSimulationProperties.MinDistSquared;

            // repulsion between 2 Objects: 1.0 / dist² * Factor * dir_normalized
            var repulsionForce = 1.0f / fallOffExp * factor * direction.NormalizedCopy();

            return repulsionForce;
        }

        private Vector2D ComputeForceBetweenTags(Vector2D tagPosition, Vector2D otherTagPosition, Tuple<float, float> interTagForces, float factor, float userInfluenceFactor, float otherUserInfluenceFactor)
        {
            // compute vector between points -> direction of repulsion
            var direction = (tagPosition - otherTagPosition);

            // compute distance -> repulsion decreases only slightly over distance (squared decay)
            //var dist = direction.Length;
            //if (Math.Abs(dist) < Double.Epsilon)
            //    dist = PhysicsSimulationProperties.MinDistSquared;

            // modify forces according to differences in weight: tags with differnt weight are pulled away.
            var sameInfluenceFactor = otherUserInfluenceFactor - userInfluenceFactor;

            // attraction between 2 Tags: 1.0 / dist * attractionFactor * dir_normalized
            var attractionForce = factor * interTagForces.Item1 * direction.NormalizedCopy();


            // clamp attraction to prevent too strong effects (max 4*original attraction force for equally weightes tags)
            sameInfluenceFactor = Math.Abs(sameInfluenceFactor) < 0.25f ? sameInfluenceFactor < 0 ? -0.25f : 0.25f : sameInfluenceFactor;

            // clamp other influence to prevent this factor from eliminating attration completely
            var otherInfluenceClamped = Math.Abs(otherUserInfluenceFactor) > 1.0
                ? otherUserInfluenceFactor
                : otherUserInfluenceFactor < 0 ? -1.0f : 1.0f;

            // modify attraction by strenghtening attraction for equally weighted tags. Tags with large differnces will pe pushed way, Tags which have negative values will generally negate attraction
            attractionForce *= (otherInfluenceClamped / sameInfluenceFactor);

            // repulsion between 2 Tags: 1.0 / dist * repulsionFactor * dir_normalized
            var repulsionForce = interTagForces.Item2 * direction.NormalizedCopy();

            // repulsionForce *= 1.0 + sameInfluenceFactor;

            // prevent collisions: multiply repulsion with distance (difference to min distance == 2 * tagdiameter) between tags
            var distRepulsion = 2.0f * PhysicsSimulationProperties.TagBaseSize - direction.Length;

            // clamping: if dist > 2 * tagsize then only apply modification based on tag weights, otherwise don't apply tag weight modification, but modify repulsion according to distance between tags
            distRepulsion = distRepulsion < 1 ? 1 + sameInfluenceFactor : distRepulsion;
            repulsionForce *= distRepulsion;

            return (repulsionForce + attractionForce);
        }

        private Vector2D ComputeForceForCurrentFilter(Vector2D objectPosition, Vector2D tagPosition, ObjectTag tag,
            List<ObjectTag> tags, float selectionWeight)
        {
            var resultingForce = new Vector2D(0, 0);

            if (Math.Abs(selectionWeight) < float.Epsilon)
                return resultingForce;

            // compute vector between points -> direction of repulsion
            var direction = (GetTagCoordinatesInObjectSpace(tagPosition) - objectPosition);
            var isContained = tags.Contains(tag);

            //three cases: 
            //      1. item is tagged with selected tag --> strong attraction over whole canvas
            //      2. item is not tagged with selected tag --> repulsion in the area around the tag
            //      3. item is tagged with explicitly unselected (negatively weighted) tag --> repulsion, but again only in th area around the tag (2. and 3. same foces applied; only need to flip direction regarding the context)
            if (isContained && selectionWeight > 0)
            {
                var dist = direction.Length;

                // compute distance squared -> repulsion decreases over distance (squared decay);
                if (Math.Abs(dist) < float.Epsilon)
                    dist = PhysicsSimulationProperties.MinDistSquared;

                var distFactor = (dist - 0.5f * PhysicsSimulationProperties.TagSize);
                distFactor *= 0.01f;
                distFactor = distFactor > 1 ? 1 : distFactor < 0 ? 0 : distFactor;


                var force = distFactor * selectionWeight * PhysicsSimulationProperties.SelectedTagAttributionModifier * direction.NormalizedCopy();
                resultingForce += force;

                //if (direction.Length <= PhysicsSimulationProperties.TagSize)
                //    resultingForce *= PhysicsSimulationProperties.MinDistSquared;
            }
            else if ((!isContained && selectionWeight < 0) || PhysicsSimulationProperties.ComputeRepulsionUnselected)
            {
                var distSq = direction.LengthSquared;

                // compute distance squared -> repulsion decreases over distance (squared decay);
                if (Math.Abs(distSq) < Double.Epsilon)
                    distSq = PhysicsSimulationProperties.MinDistSquared;

                var distFactor = (distSq - 0.5f * PhysicsSimulationProperties.TagSize);
                distFactor *= 0.01f;
                distFactor = distFactor > 1 ? 1 : distFactor < 0 ? 0 : distFactor;

                var force = distFactor * selectionWeight * PhysicsSimulationProperties.UnselectedTagRepulsionModifier * direction.NormalizedCopy();
                resultingForce += selectionWeight > 0 ? -force : force;
                // PhysicsSimulationProperties.TagSize/distSq* selectionWeight * PhysicsSimulationProperties.UnselectedTagRepulsionModifier * direction.NormalizedCopy();
            }

            return resultingForce;
        }

        private Vector2D ComputeCentripetalForce(Vector2D position, Vector2D center, int objId)
        {
            var speedFactor = PhysicsSimulationProperties.CentripetalSpeed;

            var dir = position - center;
            var dist = dir.Length;
            var forceDir = new Vector2D(-dir.Y, dir.X);

            forceDir = forceDir / FastSquareRoot.Sqrt(dist) * speedFactor;

            // invert the direction of the movement for every 3rd object
            var dirChange = objId % 3 == 0 ? -1.0f : 1.0f;

            // modify speed according to id of the object (more randomized movements)
            var forceMod = (1.0f + objId / 200.0f) * dirChange;

            return forceDir * forceMod;
        }

        #endregion

        private void HandlePauseRequested(Tuple<bool, bool> doPause)
        {
            if (!(IsCalibrating && !doPause.Item2 && !doPause.Item1))
            {
                IsPaused = doPause.Item1;
                IsCalibrating = doPause.Item2;
            }
        }

        private Vector2D HandleCollisionWithTags(Vector2D oldObjectPosition, Vector2D newObjectPosition, List<Vector2D> tagPositions)
        {
            if (!PhysicsSimulationProperties.DetectCollisions)
                return newObjectPosition;

            var positionAfterCollision = newObjectPosition;

            var minDistSquared = PhysicsSimulationProperties.TagSize * PhysicsSimulationProperties.TagSize;

            for (int i = 0; i < PhysicsSimulationProperties.CollisionDetectionIterations; i++)
            {
                var minDist = minDistSquared + 1.0;

                tagPositions.ForEach(pos =>
                {
                    var position = GetTagCoordinatesInObjectSpace(pos);
                    var distSq = (newObjectPosition - position).LengthSquared;

                    if (minDist > distSq)
                    {
                        minDist = distSq;

                        int numSolutions;
                        int nearestSolution;
                        var intersections = Vector2D.ComputeIntersection(oldObjectPosition, newObjectPosition, position,
                            0.5f * PhysicsSimulationProperties.TagSize, out numSolutions, out nearestSolution);

                        if (numSolutions > 0)
                            positionAfterCollision = intersections[nearestSolution];
                    }

                });

                if (minDist > minDistSquared)
                    break;
            }

            return positionAfterCollision;
        }

        private Vector2D GetTagCoordinatesInObjectSpace(Vector2D tagCoords)
        {
            var offsetX = PhysicsSimulationProperties.ItemsCanvasWidth - PhysicsSimulationProperties.TagsCanvasWidth;
            var offsetY = PhysicsSimulationProperties.ItemsCanvasHeight - PhysicsSimulationProperties.TagsCanvasHeight;

            return tagCoords + new Vector2D(0.5f * offsetX, 0.5f * offsetY);
        }

        #endregion
    }
}
