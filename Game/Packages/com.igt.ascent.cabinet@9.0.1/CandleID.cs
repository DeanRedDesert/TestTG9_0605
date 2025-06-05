//-----------------------------------------------------------------------
// <copyright file = "CandleID.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Lamp identifiers supported by the foundation.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public enum CandleID
    {
        /// <summary>
        /// Value mapping to an invalid lamp identifier.
        /// </summary>
        Invalid,

        /// <summary>
        /// Value mapping to Candle 1's lamp identifier.
        /// </summary>
        Candle1,

        /// <summary>
        /// Value mapping to Candle 2's lamp identifier.
        /// </summary>
        Candle2,

        /// <summary>
        /// Value mapping to Candle 3's lamp identifier.
        /// </summary>
        Candle3,

        /// <summary>
        /// Value mapping to Candle 4's lamp identifier.
        /// </summary>
        Candle4,

        /// <summary>
        /// Value mapping to all valid lamp identifiers.
        /// </summary>
        All
    }
}
