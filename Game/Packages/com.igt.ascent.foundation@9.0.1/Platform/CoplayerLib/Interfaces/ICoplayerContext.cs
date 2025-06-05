// -----------------------------------------------------------------------
// <copyright file = "ICoplayerContext.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.CoplayerLib.Interfaces
{
    using Platform.Interfaces;

    /// <summary>
    /// This interface defines data pertaining to a coplayer's context.
    /// </summary>
    public interface ICoplayerContext
    {
        /// <summary>
        /// Gets the unique id of the coplayer.
        /// </summary>
        int CoplayerId { get; }

        /// <summary>
        /// Gets the mount point of the shell application package.
        /// </summary>
        string MountPoint { get; }

        /// <summary>
        /// Gets the game mode in which the shell application is running.
        /// Shell and coplayers always run in the same game mode.
        /// </summary>
        GameMode GameMode { get; }

        /// <summary>
        /// Gets the G2SThemeId defined in the theme registry.
        /// </summary>
        /// <remarks>
        /// This is used by SDK to obtain the corresponding
        /// Game Configurator from the Shell Configurator.
        /// </remarks>
        string G2SThemeId { get; }

        /// <summary>
        /// Gets the TagData defined in the theme registry.
        /// </summary>
        string ThemeTag { get; }

        /// <summary>
        /// Gets the TagDataFile defined in the theme registry.
        /// </summary>
        /// <remarks>
        /// This is usually the path to the sub folder of the theme
        /// where EGM resources etc. are located.
        /// </remarks>
        string ThemeTagDataFile { get; }

        /// <summary>
        /// Gets the denomination for the coplayer context, in base units.
        /// </summary>
        long Denomination { get; }

        /// <summary>
        /// Gets the TagData defined in the payvar registry.
        /// </summary>
        string PayvarTag { get; }

        /// <summary>
        /// Gets the TagDataFile defined in the payvar registry.
        /// </summary>
        /// <remarks>
        /// This is usually the path to paytable file.
        /// </remarks>
        string PayvarTagDataFile { get; }
    }
}