//-----------------------------------------------------------------------
// <copyright file = "IColorProfileSetting.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.Monitor
{
    /// <summary>
    /// An interface for storing the color profile setting for a single monitor device type.
    /// </summary>
    public interface IColorProfileSetting
    {
        /// <summary>
        /// Get the monitor color profile ID.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Get the Custom color profile file.
        /// </summary>
        string CustomFile { get; }

        /// <summary>
        /// Get the general color profile strategy to use.
        /// </summary>
        ColorProfileStrategy Strategy { get; }
    }
}
