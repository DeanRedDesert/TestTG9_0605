//-----------------------------------------------------------------------
// <copyright file = "TransitionType.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Streaming
{
    /// <summary>
    /// The frame transition types.
    /// </summary>
    /// <remarks>
    /// These values come from the IGT USB Feature 121 specification.
    /// </remarks>
    public enum TransitionType : byte
    {
        /// <summary>
        /// Instantly switch to new frame.
        /// </summary>
        Instant = 0,

        /// <summary>
        /// Crossfade to new frame.
        /// </summary>
        Crossfade = 1,
    }
}
