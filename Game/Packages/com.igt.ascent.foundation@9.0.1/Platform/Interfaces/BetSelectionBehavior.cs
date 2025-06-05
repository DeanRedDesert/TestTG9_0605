// -----------------------------------------------------------------------
// <copyright file = "BetSelectionBehavior.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// This enumeration is used to define the bet selection behaviors.
    /// </summary>
    [Serializable]
    public enum BetSelectionBehavior {

        /// <summary>
        /// The bet selection behavior is undefined. The user is free to use any selection behavior as it sees fit.
        /// </summary>
        Undefined,

        /// <summary>
        /// The bet selection behavior is minimum.
        /// </summary>
        Minimum,

        /// <summary>
        /// The bet selection behavior is maximum.
        /// </summary>
        Maximum
    }
}
