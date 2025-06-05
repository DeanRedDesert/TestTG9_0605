//-----------------------------------------------------------------------
// <copyright file = "ReelRecoveryOrder.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    /// <summary>
    /// Enumeration used to set the reel recovery order.
    /// </summary>
    public enum RecoveryOrder
    {
        /// <summary>
        /// Recover from the reel with the lowest number to
        /// the one with the highest.
        /// </summary>
        Ascending,

        /// <summary>
        /// Recover from the reel with the highest number
        /// to the one with the lowest.
        /// </summary>
        Descending
    }
}
