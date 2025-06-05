//-----------------------------------------------------------------------
// <copyright file = "AttractMode.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization
{
    /// <summary>
    /// The various types of game attract modes that the EGM can run.
    /// </summary>
    public enum AttractMode
    {
        /// <summary>
        /// Non-synchronized attract sequences.
        /// </summary>
        Standalone,

        /// <summary>
        /// Attract sequences synchronized over multiple EGMs with a high degree of precision.
        /// </summary>
        HighPrecisionSynchronized,

        /// <summary>
        /// Attract sequences synchronized over multiple EGMs with a low degree of precision.
        /// </summary>
        LowPrecisionSynchronized
    }
}
