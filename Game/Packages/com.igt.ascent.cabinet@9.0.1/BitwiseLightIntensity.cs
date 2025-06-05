//-----------------------------------------------------------------------
// <copyright file = "BitwiseLightIntensity.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{

    /// <summary>
    /// Enumeration used for bitwise light intensity.
    /// </summary>
    /// <remarks>
    /// EnumStorageShouldBeInt32 is suppressed because this enumeration is associated with a hardware device where the
    /// storage type is a byte.
    /// </remarks>
    public enum BitwiseLightIntensity : byte
    {
        /// <summary>
        /// Turn the light off.
        /// </summary>
        Off = 0,

        /// <summary>
        /// Set the light to have low intensity.
        /// </summary>
        Low,

        /// <summary>
        /// Set the light to have medium intensity.
        /// </summary>
        Medium,

        /// <summary>
        /// Set the light to full intensity.
        /// </summary>
        Full
    }
}
