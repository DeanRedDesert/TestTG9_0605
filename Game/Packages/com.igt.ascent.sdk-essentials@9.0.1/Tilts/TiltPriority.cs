//-----------------------------------------------------------------------
// <copyright file = "TiltPriority.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Tilts
{
    using System;

    /// <summary>
    /// An enumeration defining priority for a tilt.
    /// </summary>
    [Serializable]
    public enum TiltPriority
    {
        /// <summary>
        /// The value for low priority.
        /// </summary>
        Low,

        /// <summary>
        /// The value for medium priority.
        /// </summary>
        Medium,

        /// <summary>
        /// The value for high priority.
        /// </summary>
        High,
    }
}