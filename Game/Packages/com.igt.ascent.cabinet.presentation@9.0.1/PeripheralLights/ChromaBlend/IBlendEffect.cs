//-----------------------------------------------------------------------
// <copyright file = "IBlendEffect.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.ChromaBlend
{
    using System.Collections.Generic;
    using Streaming;

    /// <summary>
    /// The interface for a light blending effect.
    /// </summary>
    public interface IBlendEffect
    {
        /// <summary>
        /// Blend two frames together to create a new frame.
        /// </summary>
        /// <param name="background">The background frame.</param>
        /// <param name="foreground">The foreground frame.</param>
        /// <param name="backgroundLedState">The current color of the LEDs in the background layer.</param>
        /// <param name="foregroundLedState">The current color of the LEDs in the foreground layer.</param>
        /// <param name="foregroundLooped">
        /// Indicates if this is the first frame after the foreground frame has looped.
        /// </param>
        /// <returns>The new blended frame.</returns>
        Frame Blend(Frame background, Frame foreground,
            List<Color> backgroundLedState, List<Color> foregroundLedState, bool foregroundLooped);
    }
}
