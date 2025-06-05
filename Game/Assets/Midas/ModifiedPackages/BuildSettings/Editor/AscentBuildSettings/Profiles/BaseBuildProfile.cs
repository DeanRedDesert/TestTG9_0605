//-----------------------------------------------------------------------
// <copyright file = "BaseBuildProfile.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.AscentBuildSettings.Editor.Profiles
{
    using Core.Communication;
    using System;
    using System.Linq;
    using UnityEditor;

    /// <summary>
    /// Base class for build profiles that provide GUI elements and default values for the Ascent Build Settings inspector.
    /// </summary>
    public abstract class BaseBuildProfile
    {
        /// <summary>
        /// List of foundation target names based on the foundation target enum.
        /// </summary>
        private readonly string[] ascentFoundationTargetNames =
            Enum.GetNames(typeof(FoundationTarget)).Where(foundationTargetName =>
                foundationTargetName != Enum.GetName(typeof(FoundationTarget), FoundationTarget.All) &&
                foundationTargetName != Enum.GetName(typeof(FoundationTarget), FoundationTarget.AllAscent) &&
                foundationTargetName != Enum.GetName(typeof(FoundationTarget), FoundationTarget.UniversalController) &&
                foundationTargetName != Enum.GetName(typeof(FoundationTarget), FoundationTarget.UniversalController2)).ToArray();

        /// <summary>
        /// Display a GUI for the Build Type setting.
        /// </summary>
        /// <param name="current">The current build type.</param>
        /// <returns>The new build type.</returns>
        public virtual IgtGameParameters.GameType BuildTypeGui(IgtGameParameters.GameType current)
        {
            return (IgtGameParameters.GameType)EditorGUILayout.EnumPopup("Build Type", current);
        }

        /// <summary>
        /// Display a GUI for the Build Type setting.
        /// </summary>
        /// <param name="current">The current build type.</param>
        /// <returns>The new build type.</returns>
        public virtual FoundationTarget FoundationTargetGui(FoundationTarget current)
        {
            return DoFoundationTargetGui(ascentFoundationTargetNames, current);
        }

        /// <summary>
        /// Display a GUI for the Build Type setting.
        /// </summary>
        /// <param name="foundationTargetNames">List of selectable foundation targets to be displayed in a dropdown menu</param>
        /// <param name="current">The current build type.</param>
        /// <returns>The new build type.</returns>
        protected FoundationTarget DoFoundationTargetGui(string[] foundationTargetNames, FoundationTarget current)
        {
            var currentName = Enum.GetName(typeof(FoundationTarget), current);
            var currentIndexInNameArray = Array.IndexOf(foundationTargetNames, currentName);

            if(currentIndexInNameArray == -1)
            {
                currentIndexInNameArray = 0;
            }

            currentIndexInNameArray = EditorGUILayout.Popup("Target Foundation",
                currentIndexInNameArray, foundationTargetNames.ToArray());

            return (FoundationTarget)Enum.Parse(typeof(FoundationTarget),
                foundationTargetNames[currentIndexInNameArray]);
        }

        /// <summary>
        /// Display a GUI for the Tool Connections setting.
        /// </summary>
        /// <param name="current">The current tool connection setting.</param>
        /// <returns>The new tool connection setting.</returns>
        public virtual IgtGameParameters.ConnectionType ToolConnectionsGui(IgtGameParameters.ConnectionType current)
        {
            return (IgtGameParameters.ConnectionType)EditorGUILayout.EnumPopup("Tool Connections", current);
        }

        /// <summary>
        /// Display a GUI for the Mouse Cursor setting.
        /// </summary>
        /// <param name="current">The current  mouse cursor setting.</param>
        /// <returns>The new mouse cursor setting.</returns>
        public virtual bool MouseCursorGui(bool current)
        {
            return EditorGUILayout.Toggle("Show Mouse Cursor", current);
        }

        /// <summary>
        /// Display a GUI for the Mono Aot Compile setting.
        /// </summary>
        /// <param name="current">The current mono aot compile setting.</param>
        /// <returns>The new mono aot compile setting.</returns>
        public virtual bool MonoAotCompileGui(bool current)
        {
            return EditorGUILayout.Toggle("Mono AOT Compile", current);
        }
    }
}