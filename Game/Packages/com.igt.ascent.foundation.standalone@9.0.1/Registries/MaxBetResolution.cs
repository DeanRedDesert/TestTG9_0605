//-----------------------------------------------------------------------
// <copyright file = "MaxBetResolution.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System;

    /// <summary>
    /// This enumeration enumerates the values of the max bet resolution.
    /// </summary>
    [Serializable]
    public enum MaxBetResolution
    {
        /// <summary>
        /// Max bet is defined per theme.
        /// </summary>
        PerTheme,

        /// <summary>
        /// Max bet is defined per payvar.
        /// </summary>
        PerPayvar,

        /// <summary>
        /// Max bet is defined per payvar per denomination.
        /// </summary>
        PerPayvarDenomination,

        /// <summary>
        /// Unknown max bet resolution definition. 100 should be good enough even more bet
        /// resolutions may be defined in the future.
        /// </summary>
        Unknown = 100,
    }
}