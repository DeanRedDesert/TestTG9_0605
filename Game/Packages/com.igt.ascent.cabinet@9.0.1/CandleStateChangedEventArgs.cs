//-----------------------------------------------------------------------
// <copyright file = "CandleStateChangedArgs.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Event arguments for candle state changes.
    /// </summary>
    public class CandleStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Construct an instance of the event arguments with the given candle information.
        /// </summary>
        /// <param name="candleId">The candle identifier which the event is for.</param>
        /// <param name="illuminated">The illumination state of the candle.</param>
        public CandleStateChangedEventArgs(CandleID candleId, bool illuminated)
        {
            CandleId = candleId;
            Illuminated = illuminated;
        }

        /// <summary>
        /// The candle identifier which the event is for.
        /// </summary>
        public CandleID CandleId { get; }

        /// <summary>
        /// The illumination state of the candle.
        /// </summary>
        public bool Illuminated { get; }
    }
}