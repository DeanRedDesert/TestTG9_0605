//-----------------------------------------------------------------------
// <copyright file = "ILightState.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    /// <summary>
    /// Provides a read-only view of a lights state.
    /// </summary>
    public interface ILightState
    {
        /// <summary>
        /// Flag which indicates if the light is enabled.
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// Get the type of the light state. This is based on the last modification to
        /// the light state; different modes of the light may also be populated with data.
        /// </summary>
        LightStateType LastSetType { get; }

        /// <summary>
        /// Get the two bit intensity of the light.
        /// </summary>
        BitwiseLightIntensity TwoBitIntensity { get; }

        /// <summary>
        /// Get the eight bit intensity.
        /// </summary>
        byte EightBitIntensity { get; }

        /// <summary>
        /// Get the four bit color of the light.
        /// </summary>
        BitwiseLightColor FourBitColor { get; }

        /// <summary>
        /// Get the six bit color of the light.
        /// </summary>
        Rgb6 SixBitColor { get; }

        /// <summary>
        /// Get the sixteen bit color of the light.
        /// </summary>
        Rgb16 SixteenBitColor { get; }
    }
}
