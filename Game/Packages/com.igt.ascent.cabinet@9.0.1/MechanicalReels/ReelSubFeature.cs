//-----------------------------------------------------------------------
// <copyright file = "ReelSubFeature.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    /// <summary>
    /// Enumeration containing the different sub features of a reel device.
    /// </summary>
    public enum ReelSubFeature
    {
        /// <summary>
        /// Device used for main stepper game play. 
        /// </summary>
        GamePlayReels,

        /// <summary>
        /// Generic bonus reel device.
        /// </summary>
        BonusReels,

        /// <summary>
        /// Dice device. Contains several physical dice which can be positioned.
        /// </summary>
        Dice,

        /// <summary>
        /// Wheel device. In a wheel device the players sees a wheel face generally sliced into wedges.
        /// </summary>
        Wheel,

        /// <summary>
        /// Prism device.
        /// </summary>
        Prism,

        /// <summary>
        /// Slider device. 
        /// </summary>
        LinearSlider,

        /// <summary>
        /// Sphere device.
        /// </summary>
        Sphere,

        /// <summary>
        /// Pointer device. Pointer with movable position.
        /// </summary>
        Pointer,

        /// <summary>
        /// The sub feature type is not one understood by this interface version.
        /// </summary>
        Unknown
    }
}
