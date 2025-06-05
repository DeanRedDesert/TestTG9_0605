//-----------------------------------------------------------------------
// <copyright file = "IGTEGMBuildCustomInspector.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.AscentBuildSettings.Editor
{
    using Core.Communication;
    using Profiles;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Implements a custom inspector for <see cref="IGTEGMBuild"/>.
    /// </summary>
    [CustomEditor(typeof(IGTEGMBuild))]
    // ReSharper disable once CheckNamespace
    // ReSharper disable once InconsistentNaming
    public class IGTEGMBuildCustomInspector : Editor
    {
        private const string ExperimentalFoundationMessage =
            "The foundation target {0} is experimental in this SDK release and should not be used for all production games at this time. Only use if you are developing a game that requires a specific feature in that version of the Ascent Foundation.";

        private const string CustomProfileMessage =
            "Custom Build Profile may allow for sub-optimal configurations. Use a pre-defined profile for best results.";

        private IGTEGMBuild ascentBuildSettings;
        private ProfileType? currentProfile;

        private enum ProfileType
        {
            DevelopmentEgmOrAow,
            ReleaseEgm,
            UniversalController,
            FastPlay,
            Standalone,
            Custom
        }

        private readonly Dictionary<ProfileType, BaseBuildProfile> profiles = new Dictionary
            <ProfileType, BaseBuildProfile>
        {
            {ProfileType.DevelopmentEgmOrAow, new DevEgmProfile()},
            {ProfileType.ReleaseEgm, new ReleaseEgmProfile()},
            {ProfileType.UniversalController, new UniversalControllerBuildProfile()},
            {ProfileType.FastPlay, new FastPlayProfile()},
            {ProfileType.Standalone, new StandaloneProfile()},
            {ProfileType.Custom, new CustomProfile()}
        };

        /// <summary>
        /// GUI implementation.
        /// </summary>
        public override void OnInspectorGUI()
        {
            ascentBuildSettings = target as IGTEGMBuild;

            if(ascentBuildSettings == null)
            {
                return;
            }

            EditorGUILayout.BeginVertical(GUI.skin.box);
            Undo.RecordObject(ascentBuildSettings, "Ascent Build Settings");
            EditorGUI.BeginChangeCheck();

            var gameParameters = ascentBuildSettings.GameParameters;

            ascentBuildSettings.BuildType =
                (BuildType)EditorGUILayout.EnumPopup("Build Type", ascentBuildSettings.BuildType);

            if(ascentBuildSettings.BuildType == BuildType.Game)
            {
                if(currentProfile == null)
                {
                    // Reinitialize the current profile based on the loaded parameters.
                    switch(ascentBuildSettings.GameParameters.Type)
                    {
                        case IgtGameParameters.GameType.Standard:
                            currentProfile = ascentBuildSettings.Release
                                ? ProfileType.ReleaseEgm
                                : ProfileType.DevelopmentEgmOrAow;
                            break;
                        case IgtGameParameters.GameType.StandaloneFileBackedSafeStorage:
                        case IgtGameParameters.GameType.StandaloneBinaryFileBackedSafeStorage:
                        case IgtGameParameters.GameType.StandaloneNoSafeStorage:
                            currentProfile = ProfileType.Standalone;
                            break;
                        case IgtGameParameters.GameType.UniversalController:
                            currentProfile = ProfileType.UniversalController;
                            break;
                        case IgtGameParameters.GameType.FastPlay:
                            currentProfile = ProfileType.FastPlay;
                            break;
                        default:
                            currentProfile = ProfileType.Custom;
                            break;
                    }
                }

                currentProfile = (ProfileType)EditorGUILayout.EnumPopup("Build Profile", currentProfile);
                ascentBuildSettings.Release = currentProfile == ProfileType.ReleaseEgm;
                if(currentProfile == ProfileType.Custom)
                {
                    EditorGUILayout.HelpBox(CustomProfileMessage, MessageType.Warning);
                }
            }
            else
            {
                ascentBuildSettings.Release = EditorGUILayout.Toggle("Release", ascentBuildSettings.Release);
            }

            EditorGUILayout.EndVertical();

            GameSettings();

            if(currentProfile == ProfileType.UniversalController)
            {
                gameParameters = IgtGameParameters.CreateUcParameters(gameParameters.TargetedFoundation);
                ascentBuildSettings.GameParameters = gameParameters;
            }
            else
            {
                if(ascentBuildSettings.Release)
                {
                    gameParameters = IgtGameParameters.CreateReleaseParameters(gameParameters.TargetedFoundation);
                    ascentBuildSettings.GameParameters = gameParameters;
                }
            }

            MonitorBuildSettings();

            var changed = EditorGUI.EndChangeCheck();

            if(changed || IGTEGMSettings.IgtEgmBuildSettings == null)
            {
                // Using serialized properties or the Undo class functionality is more appropriate, the way that
                // the game parameters class is designed (not inheriting from scriptable object) makes it difficult
                // to reassign all properties on that object at once. Using SetDirty in combination with Undo.RecordObject
                // gets all the functionality without hugely overhauling the scriptable object to make better use of
                // serialization.
                EditorUtility.SetDirty(ascentBuildSettings);
                IGTEGMSettings.IgtEgmBuildSettings = ascentBuildSettings;
            }
        }

        /// <summary>
        /// Draw game-build specific settings.
        /// </summary>
        private void GameSettings()
        {
            if(ascentBuildSettings.BuildType != BuildType.Game)
            {
                return;
            }

            EditorGUILayout.BeginVertical(GUI.skin.box);
            ascentBuildSettings.GameParameters.Type =
                profiles[currentProfile.Value].BuildTypeGui(ascentBuildSettings.GameParameters.Type);

            if(ascentBuildSettings.GameParameters.Type == IgtGameParameters.GameType.FastPlay)
            {
                ascentBuildSettings.GameParameters.ConfigureFastPlay();
            }

            ascentBuildSettings.GameParameters.TargetedFoundation =
                profiles[currentProfile.Value].FoundationTargetGui(ascentBuildSettings.GameParameters.TargetedFoundation);

            ascentBuildSettings.GameParameters.ToolConnections =
                profiles[currentProfile.Value].ToolConnectionsGui(ascentBuildSettings.GameParameters.ToolConnections);

            ascentBuildSettings.GameParameters.ShowMouseCursor =
                profiles[currentProfile.Value].MouseCursorGui(ascentBuildSettings.GameParameters.ShowMouseCursor);

            // Temporarily disable AOT due to the bug https://cspjira.igt.com/jira/browse/AS-9556
            // Hide this option from IGT Ascent build settings menu.
            // Same measure applied to MonoAot.cs
            //ascentBuildSettings.MonoAoTCompile =
            //    profiles[currentProfile.Value].MonoAotCompileGui(ascentBuildSettings.MonoAoTCompile);

            if (FoundationTargetExtensions.GetExperimentalFoundationTargets.Contains(ascentBuildSettings.GameParameters.TargetedFoundation))
            {
                EditorGUILayout.HelpBox(string.Format(ExperimentalFoundationMessage,
                    ascentBuildSettings.GameParameters.TargetedFoundation),
                    MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw settings related to the monitor build settings.
        /// </summary>
        private void MonitorBuildSettings()
        {
            // Start box to contain all monitor settings.
            EditorGUILayout.BeginVertical(GUI.skin.box);

            GUILayout.Label("Monitor Settings", EditorStyles.boldLabel);

            // Start box to contain "Fit windows to primary display".
            EditorGUILayout.BeginVertical(GUI.skin.box);

            ascentBuildSettings.GameParameters.FitToScreen = EditorGUILayout.Toggle(new GUIContent("Fit windows to primary display",
                "Size and position all monitors to fit on the primary display."), ascentBuildSettings.GameParameters.FitToScreen);

            // End box to contain "Fit windows to primary display".
            EditorGUILayout.EndVertical();

            // Start box to contain "Force enable all monitors".
            EditorGUILayout.BeginVertical(GUI.skin.box);

            GUILayout.Label("Force enabling all monitors will only effect game build runtime. " +
                            "Using this flag will not effect the editor monitor configuration. " +
                            "\nCommand line development and release game builds will automatically apply this option.",
                            EditorStyles.wordWrappedLabel);

            ascentBuildSettings.GameParameters.ForceEnableAllMonitors = EditorGUILayout.Toggle(new GUIContent("Force enable all monitors",
                "Force all monitors to be enabled for the game build."), ascentBuildSettings.GameParameters.ForceEnableAllMonitors);

            // End box to contain "Force enable all monitors".
            EditorGUILayout.EndVertical();

            // End box to contain all monitor settings.
            EditorGUILayout.EndVertical();
        }
    }
}
