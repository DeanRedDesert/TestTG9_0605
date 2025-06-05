//-----------------------------------------------------------------------
// <copyright file = "IGTEGMBuild.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.AscentBuildSettings.Editor
{
    using UnityEngine;

    /// <summary>
    /// The Build Type modes
    /// </summary>
    public enum BuildType
    {
        /// <summary>
        ///  Game build type.
        /// </summary>
        Game = 0,

        /// <summary>
        /// Theme menu build type.
        /// </summary>
        ThemeMenu = 1,

        /// <summary>
        /// A generic build type that allows spanning multiple monitors.
        /// </summary>
        Other
    };

    /// <summary>
    /// This class provides ready only flags and properties set by the user when building the game.
    /// </summary>
    public class IGTEGMBuild : ScriptableObject
    {
        /// <summary>
        /// The build type of the client.
        /// </summary>
        public BuildType BuildType;

        /// <summary>
        /// Parameters for "Game" type builds.
        /// </summary>
        public IgtGameParameters GameParameters;

        /// <summary>
        /// Flag indicating if the build should be release.
        /// </summary>
        public bool Release;

        /// <summary>
        /// Flag indicating if Mono AOT build is enabled.
        /// </summary>
        public bool MonoAoTCompile;
    }
}
