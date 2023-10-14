using System;
using DeeP.Properties;
using DeeP.ViewModel;
using DeeP.Util;
using NLog;
using PhysicsSimulation.Model;
using Prism.Commands;

namespace DeeP.Commands
{
    public class SaveDefaultSettingsCommand : DelegateCommand<string>
    {
        private static FlexiWallViewModel _vm;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        public SaveDefaultSettingsCommand(FlexiWallViewModel vm) : base(SaveSettings)
        {
            _vm = vm;
            // CanExecute(true);
        }

        public static void SaveSettings(string parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter))
            {
                Logger.Error($"no argument provided for {nameof(SaveDefaultSettingsCommand)}.{nameof(SaveSettings)}.");
                return;
            }

            if (parameter.Equals("Save"))
            {
                SaveSettings();
                Logger.Info($"{nameof(SaveDefaultSettingsCommand)}.{nameof(SaveSettings)} sucessfully Executed with param {parameter}.");
                return;
            }

            if (parameter.Equals("Apply"))
            {
               
                Logger.Warn($"{nameof(SaveDefaultSettingsCommand)}.{nameof(SaveSettings)} {parameter} is deprecated. No action done.");
                return;
            }

            throw new NotImplementedException("Value " + parameter + " is not connected with an implemented action.");
        }

        private static void SaveSettings()
        {
            Settings.Default.BoundaryForceModifier = PhysicsSimulationProperties.BoundaryForceModifier;
            Settings.Default.CentripetalSpeed = PhysicsSimulationProperties.CentripetalSpeed;
            Settings.Default.CollisionDetectionIterations = PhysicsSimulationProperties.CollisionDetectionIterations;
            Settings.Default.ComputeRepulsionForUnselectedTags = PhysicsSimulationProperties.ComputeRepulsionUnselected;
            Settings.Default.DetectCollisions = PhysicsSimulationProperties.DetectCollisions;
            Settings.Default.ItemBaseSize = PhysicsSimulationProperties.ItemBaseSize;
            Settings.Default.ItemForceFieldDiameter = PhysicsSimulationProperties.ItemSize;
            Settings.Default.ItemsCanvasHeight = PhysicsSimulationProperties.ItemsCanvasHeight;
            Settings.Default.ItemsCanvasWidth = PhysicsSimulationProperties.ItemsCanvasWidth;
            Settings.Default.MaxIntensityValue = PhysicsSimulationProperties.MaxUserInfluenceValue;
            Settings.Default.ObjectForceModifier = PhysicsSimulationProperties.ObjectForceModifier;
            Settings.Default.SelectedTagAttractionModifier = PhysicsSimulationProperties.SelectedTagAttributionModifier;
            Settings.Default.SimulationTimerIntervalMs = PhysicsSimulationProperties.SimulationTimerInterval;
            Settings.Default.StandardTagAttraction = PhysicsSimulationProperties.StandardTagAttraction;
            Settings.Default.StandardTagRepulsion = PhysicsSimulationProperties.StandardTagRepulsion;
            Settings.Default.TagBaseSize = PhysicsSimulationProperties.TagBaseSize;
            Settings.Default.TagForceModifier = PhysicsSimulationProperties.TagForceModifier;
            Settings.Default.TagItemRepulsionModifier = PhysicsSimulationProperties.TagItemRepulsionModifier;
            Settings.Default.TagSameCategoryForceModifier = PhysicsSimulationProperties.TagSameCategoryForceModifier;
            Settings.Default.TagSameDimensionForceModifier = PhysicsSimulationProperties.TagSameDimensionForceModifier;
            Settings.Default.TagForceFieldDiameter = PhysicsSimulationProperties.TagSize;
            Settings.Default.TagCanvasHeight = PhysicsSimulationProperties.TagsCanvasHeight;
            Settings.Default.TagCanvasWidth = PhysicsSimulationProperties.TagsCanvasWidth;
            Settings.Default.UnselectedTagRepulsionModifier = PhysicsSimulationProperties.UnselectedTagRepulsionModifier;
            Settings.Default.ProcessDepthOption = PhysicsSimulationProperties.ProcessDepthOption;
            Settings.Default.EnforceEqualDataDistribution = PhysicsSimulationProperties.EnforceEqualDataDistribution;
            Settings.Default.MaxNumItemsPerTag = PhysicsSimulationProperties.MaxNumItemsPerTag;

            Settings.Default.Save();
        }
    }
}
