//-----------------------------------------------------------------------
// <copyright file = "SuzoHappSequence.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    /// <summary>
    /// The different light sequences supported by the Suzo Happ topper
    /// </summary>
    public enum SuzoHappSequence
    {
        /// <summary>
        /// Turns the device off.
        /// </summary>
        Off = 0,
        /// <summary>
        /// The color palette forms segments that flow from the top-center to the bottom-center of the device.
        /// </summary>
        Rain,
        /// <summary>
        /// Ten segments fill the ring with the first color flashing three times. The second and third colors follow identically.
        /// </summary>
        DashFlash,
        /// <summary>
        /// Fills the ring with colors that drop from the left and right to "build" a new color.
        /// </summary>
        Build,
        /// <summary>
        /// Similar to 'Dash Flash' with ten segments that rotate counter-clockwise; the second and third colors follow identically.
        /// </summary>
        DashCircle,
        /// <summary>
        /// Each of the three colors fills one third of the ring. It can rotate clockwise, counter-clockwise, or clockwise then counter-clockwise.
        /// </summary>
        ThreeThirds,
        /// <summary>
        /// A small segment starts at the center-bottom and moves counter-clockwise around until it reaches the center-bottom again.
        /// Once there, it is filled by another color from the palette. The process then repeats.
        /// </summary>
        HeadToTail,
        /// <summary>
        /// Similar to 'Three Thirds' except the segments are smaller and don't consume the entire ring.
        /// It can rotate clockwise, counter-clockwise, or clockwise then counter-clockwise.
        /// </summary>
        ThreeMice,
        /// <summary>
        /// Identical to 'Three Mice', except with five segments instead of three. 
        /// </summary>
        FiveMice,
        /// <summary>
        /// One color fills the ring from top to bottom and then it "bounces" back with a new color from bottom to top. 
        /// </summary>
        BounceFill,
        /// <summary>
        /// Paints the ring with a new color.
        /// </summary>
        Paint
    }
}
