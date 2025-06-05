//-----------------------------------------------------------------------
// <copyright file = "ReelEventArgs.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.MechanicalReels
{
    using System;

    /// <summary>
    /// Event arguments for single-reel events.
    /// </summary>
    public class ReelEventArgs : EventArgs
    {
        /// <summary>
        /// Index of the reel in its shelf.
        /// </summary>
        public byte ReelNumber;
    }
}
