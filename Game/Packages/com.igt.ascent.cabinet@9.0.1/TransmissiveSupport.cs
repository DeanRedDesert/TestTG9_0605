//-----------------------------------------------------------------------
// <copyright file = "TransmissiveSupport.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Enumeration of possible transmissive display support a theme's content may have for a specific monitor.
    /// </summary>
    public enum TransmissiveSupport
    {
        /// <summary>
        /// There is no support for transmissive displays.
        /// </summary>
        None,

        /// <summary>
        /// Content is available for a widescreen transmissive display.
        /// </summary>
        Widescreen,

        /// <summary>
        /// Content is available for a portrait transmissive display.
        /// </summary>
        Portrait,

        /// <summary>
        /// Content is available for both a widescreen and portrait transmissive display.
        /// </summary>
        WidescreenAndPortrait
    }
}
