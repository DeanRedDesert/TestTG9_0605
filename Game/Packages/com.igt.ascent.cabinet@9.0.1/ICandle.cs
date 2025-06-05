//-----------------------------------------------------------------------
// <copyright file = "ICandle.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Interface for notification of the Candle state(s).
    /// </summary>
    public interface ICandle
    {
        /// <summary>
        /// Event indicating that a candle state has changed.
        /// </summary>
        event EventHandler<CandleStateChangedEventArgs> CandleStateChangedEvent;

        /// <summary>
        /// Returns true if a candle is currently illuminated.
        /// </summary>
        /// <param name="candleId">
        /// The candle's identifier.
        /// </param>
        /// <returns>
        /// The candle illumination state, or false if the API is not supported.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when requesting the candle state for any invalid Candles (CandleID.Invalid or CandleID.All).
        /// </exception>
        bool CandleIlluminated(CandleID candleId);

        /// <summary>
        /// Request to register for candle state change events.
        /// </summary>
        /// <param name="candleId">
        /// The candle's identifier.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when requesting to register from the Invalid candle ID.
        /// </exception>
        void RegisterForCandleStateChangeEvents(CandleID candleId);

        /// <summary>
        /// Request to unregister for candle state change events.
        /// </summary>
        /// <param name="candleId">
        /// The candle's identifier.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when requesting to unregister from the Invalid candle ID.
        /// </exception>
        void UnregisterForCandleStateChangeEvents(CandleID candleId);
    }
}
