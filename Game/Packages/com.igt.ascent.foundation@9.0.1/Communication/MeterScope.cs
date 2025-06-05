//-----------------------------------------------------------------------
// <copyright file = "MeterScope.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using System;

    /// <summary>
    /// The MeterScope enumeration is used to represent
    /// the different meter scopes available to game as
    /// defined by the F2L.
    /// </summary>
    [Serializable]
    public enum MeterScope
    {
        /// <summary>
        /// Paytable Variation.  A configured and metered instance of a paytable.
        /// </summary>
        PayVar,

        /// <summary>
        /// A collection of payvars with associated attributes and configuration.
        /// </summary>
        Theme,

        /// <summary>
        /// A collection of themes with associated attributes.
        /// </summary>
        Bin,

        /// <summary>
        /// The gaming machine on which the games run.
        /// </summary>
        Machine,
    }
}
