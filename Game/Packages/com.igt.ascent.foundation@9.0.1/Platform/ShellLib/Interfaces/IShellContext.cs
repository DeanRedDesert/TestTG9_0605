// -----------------------------------------------------------------------
// <copyright file = "IShellContext.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using Platform.Interfaces;

    /// <summary>
    /// This interface defines data pertaining to a shell's context.
    /// </summary>
    public interface IShellContext
    {
        /// <summary>
        /// Gets the mount point of the shell package.
        /// </summary>
        string MountPoint { get; }

        /// <summary>
        /// Gets the game mode in which the shell application is running.
        /// Shell and coplayers always run in the same game mode.
        /// </summary>
        GameMode GameMode { get; }

        /// <summary>
        /// Gets the TagData defined in the shell registry.
        /// </summary>
        string ShellTag { get; }

        /// <summary>
        /// Gets the TagDataFile defined in the shell registry.
        /// </summary>
        /// <remarks>
        /// This is usually the path to the sub folder of the theme
        /// where EGM resources etc. are located.
        /// </remarks>
        string ShellTagDataFile { get; }
    }
}