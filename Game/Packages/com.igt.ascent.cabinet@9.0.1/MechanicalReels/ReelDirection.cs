//-----------------------------------------------------------------------
// <copyright file = "ReelDirection.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    using System;

    /// <summary>
    /// Enumeration used to specify the direction of a reel spin.
    /// </summary>
    [Serializable]
    public enum ReelDirection : byte
    {
        /// <summary>
        /// Spin will take the shortest direction.
        /// </summary>
        Shortest,

        /// <summary>
        /// Stops will pass a fixed point in ascending order.
        /// </summary>
        Ascending,

        /// <summary>
        /// Stops will pass a fixed point in descending order.
        /// </summary>
        Descending
    }
}
