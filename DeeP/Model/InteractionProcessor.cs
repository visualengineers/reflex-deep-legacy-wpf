using System;
using System.Collections.Generic;
using System.Linq;
using PhysicsSimulation.Utilities;
using Prism.Events;
using ReFlex.Core.Common.Components;
using WPFPhysicsControlsLib.ViewModel;
using Math = System.Math;

namespace DeeP.Model
{
    public class InteractionProcessor
    {
        private readonly SimulationVm _simVm;

        private readonly double _canvasWidth;
        private readonly double _canvasHeight;
        private readonly SimulationPropertiesVm _simPropVm;

        public InteractionProcessor(IEventAggregator eventAggregator, SimulationPropertiesVm simPropVm, SimulationVm simVm)
        {
            _simPropVm = simPropVm;
            _simVm = simVm;

            _canvasWidth = simPropVm.TagCanvasWidth;
            _canvasHeight = simPropVm.TagCanvasHeight;
        }

        public void ProcessInteractions(List<Interaction> interactions)
        {
            var preprocessed = PreprocessInteractions(interactions);
            Update(preprocessed);
        }

        private List<Interaction> PreprocessInteractions(List<Interaction> rawInteractions)
        {
            return rawInteractions.Select(raw =>
            {
                var processed = new Interaction(raw);

                var posConverted = Map2dPosition(raw.Position.X, raw.Position.Y);
                var zConverted = MapDepthValue(raw.Position.Z);

                processed.Position = new Point3(posConverted.X, posConverted.Y, zConverted);

                return processed;
            }).ToList();
        }

        private void Update(List<Interaction> processedInteractions)
        {
            var matches = new List<Vector2D>();

            var tags = _simVm.AcquireCopyOfTagList();

            foreach (var tag in tags)
            {
                var diameter = _simPropVm.TagCollisionRatio;

                var match = processedInteractions.FindAll(interaction =>
                {
                    var tagPos = tag.Tag.State.CurrentPosition;
                    return Math.Abs(interaction.Position.X - tagPos.X) < diameter && Math.Abs(interaction.Position.Y - tagPos.Y) < diameter;
                });


                var influence = match.Count == 0 ? 0.0f : match[0].Position.Z;
                // TODO: picking the first sample doesn't seem to be perfect --> compute median value ?

                tag.Tag.State.InfluenceFactor = influence;
                tag.UserDefinedInfluence = influence;

                match.ForEach(interaction =>
                {
                    processedInteractions.Remove(interaction);
                    matches.Add(new Vector2D(interaction.Position.X, interaction.Position.Y));

                });
            }

            var objects = _simVm.AcquireCopyOfEntitiesList();

            foreach (var obj in objects)
            {
                var diameter = _simPropVm.ItemCollisionRatio;

                var match = processedInteractions.FindAll(interaction =>
                {
                    var objPos = obj.Object.State.CurrentPosition;
                    bool isAssociatedTagActive = obj.Object.GetNumberOfInfluencingTags(0.5) > 0;
                    // obj.Object.InfluencingTags.Count(tag => Math.Abs(tag.State.InfluenceFactor) > 0.5) > 0;
                    return isAssociatedTagActive && Math.Abs(interaction.Position.X - objPos.X) < diameter &&
                           Math.Abs(interaction.Position.Y - objPos.Y) < diameter;
                });

                var influence = match.Count == 0 ? 0.0f : match[0].Position.Z;
                // TODO: picking the first sample doesn't seem to be perfect --> compute median value ?

                // obj.Object.State.InfluenceFactor = influence;
                obj.ModifyItemInfluence(influence * 2f);

                match.ForEach(interaction =>
                {
                    processedInteractions.Remove(interaction);
                    matches.Add(new Vector2D(interaction.Position.X, interaction.Position.Y));
                });
            }

            _simVm.UpdateCurrentlyDetectedTouchPoints(processedInteractions);
        }

        private Vector2D Map2dPosition(float xNormalized, float yNormalized)
        {
            return new Vector2D(xNormalized * (float) _canvasWidth, yNormalized* (float)_canvasHeight);
        }

        private float MapDepthValue(float zNormalized)
        {

            // DeeP interaction depth range is [-14 , 14 ]
            return zNormalized * -20f;
        }



    }
}