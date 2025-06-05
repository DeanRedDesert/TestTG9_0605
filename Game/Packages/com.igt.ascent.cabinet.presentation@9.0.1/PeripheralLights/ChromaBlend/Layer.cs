//-----------------------------------------------------------------------
// <copyright file = "Layer.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("IGT.Game.Core.Presentation.PeripheralLights.ChromaBlend.Tests")]
namespace IGT.Game.Core.Presentation.PeripheralLights.ChromaBlend
{
    using System;
    using Streaming;

    /// <summary>
    /// Represents a light layer that contains a light sequence
    /// and a blend effect.
    /// </summary>
    internal class Layer
    {
        public Layer(IBlendEffect blendEffect, LightSequence lightSequence)
        {
            BlendEffect = blendEffect;
            LightSequence = lightSequence ?? throw new ArgumentNullException(nameof(lightSequence));
        }

        /// <summary>
        /// Gets the blend effect for the layer.
        /// </summary>
        public IBlendEffect BlendEffect
        {
            get;
        }

        /// <summary>
        /// Gets the light sequence playing the the layer.
        /// </summary>
        public LightSequence LightSequence
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"Light Sequence: {LightSequence.Name} Blend Effect: {(BlendEffect != null ? BlendEffect.ToString() : "(None)")}";
        }
    }
}
