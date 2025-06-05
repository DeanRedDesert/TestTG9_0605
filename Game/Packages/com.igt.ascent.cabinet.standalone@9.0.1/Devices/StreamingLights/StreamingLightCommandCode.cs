//-----------------------------------------------------------------------
// <copyright file = "StreamingLightCommandCode.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.StreamingLights
{
    /// <summary>
    /// The various streaming light command codes.
    /// </summary>
    internal enum StreamingLightCommandCode
    {
        /// <summary>
        /// Sets the lights to a solid color.
        /// </summary>
        SetLights = 0x00,

        /// <summary>
        /// Real time light frame control command.
        /// </summary>
        RealTimeLightFrameControl = 0x01,

        /// <summary>
        /// Sets the overall intensity for all groups.
        /// </summary>
        SetOverallIntensity = 0x02,

        /// <summary>
        /// Set symbol highlights for reel stops.
        /// </summary>
        SetSymbolHighlights = 0x03,

        /// <summary>
        /// Command to clear specified symbol highlights.
        /// </summary>
        ClearSymbolHighlights = 0x04,

        /// <summary>
        /// Command to enable or disable the specified highlight types.
        /// </summary>
        EnableHighlights = 0x05
    }
}
