//-----------------------------------------------------------------------
// <copyright file = "Hardware.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.MechanicalReels
{
    /// <summary>
    /// Enumerates the different mechanical reel hardware types.
    /// </summary>
    public enum Hardware
    {
        /// <summary>
        /// The default reel shelf (An Ascent or compatible AVP reel shelf.)
        /// </summary>
        ReelShelf,
        
        /// <summary>
        /// The Diamond RS EGM reel shelf, introduced in 2021 as replacement hardware and firmware for the prior Ascent S3000 reels.
        /// This hardware descriptor has been added to enable checks for these reels versus prior version reels, but the
        /// SDK mechanical reels API to use these reels is identical as of 2021. 
        /// </summary>
        DrsReelShelf,

        /// <summary>
        /// A mechanical wheel device with multiple wheels nested together.
        /// (Such as the WOF triple spin wheel.)
        /// </summary>
        NestedWheel,

        /// <summary>
        /// A mechanical wheel device with only one wheel.
        /// (Such as the wheel on the Crystal Core 23 Wheel cabinet.)
        /// </summary>
        SingleWheel,
    }
}
