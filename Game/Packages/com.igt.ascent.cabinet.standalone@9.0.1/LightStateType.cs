//-----------------------------------------------------------------------
// <copyright file = "LightStateType.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    /// <summary>
    /// Enumeration which represents the different possible type of light state.
    /// </summary>
    public enum LightStateType
    {
        /// <summary>
        /// Enumeration for two bit light intensity.
        /// </summary>
        TwoBitIntensity,

        /// <summary>
        /// Enumeration for eight bit light intensity.
        /// </summary>
        EightBitIntensity,

        /// <summary>
        /// Enumeration for four bit color.
        /// </summary>
        FourBitColor,

        /// <summary>
        /// Enumeration for six bit color.
        /// </summary>
        SixBitColor,

        /// <summary>
        /// Enumeration for sixteen bit color.
        /// </summary>
        SixteenBitColor
    }
}
