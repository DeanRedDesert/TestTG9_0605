// -----------------------------------------------------------------------
// <copyright file = "SideBetSelectionBehavior.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// This enumeration is used to define the side bet selection behaviors.
    /// </summary>
    [Serializable]
    public enum SideBetSelectionBehavior {

        /// <summary>
        /// The side bet selection behavior is undefined. The user is free to use any selection behavior as it sees fit.
        /// </summary>
        Undefined,

        /// <summary>
        /// The side bet selection behavior is include.
        /// </summary>
        Include,

        /// <summary>
        /// The side bet selection behavior is exclude.
        /// </summary>
        Exclude
    }
}
