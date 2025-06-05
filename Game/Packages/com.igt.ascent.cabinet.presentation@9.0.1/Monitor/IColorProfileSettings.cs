//-----------------------------------------------------------------------
// <copyright file = "IColorProfileSettings.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.Monitor
{
    /// <summary>
    /// An interface for storing profiles for several different monitor devices.
    /// </summary>
    public interface IColorProfileSettings
    {
        /// <summary>
        /// Get the profile strategy.
        /// </summary>
        ColorProfileStrategy ProfileStrategy { get; }

        /// <summary>
        /// Get the series of color profile strategies to use if the overall strategy is custom.
        /// </summary>
        IColorProfileSetting[] CustomSettings { get; }
    }
}

