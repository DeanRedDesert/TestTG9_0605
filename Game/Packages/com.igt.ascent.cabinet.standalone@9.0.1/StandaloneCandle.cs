// -----------------------------------------------------------------------
// <copyright file = "StandaloneCandle.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System;

    /// <summary>
    /// Standalone implementation for candles.
    /// </summary>
    internal class StandaloneCandle : ICandle
    {
        #region Private Members

        /// <summary>
        /// Flag used to determine if the client has registered for candle events.
        /// </summary>
        private bool clientRegistered;

        #endregion


        #region Constructors

        /// <summary>
        /// Create an instance of the StandaloneCandle.
        /// </summary>
        public StandaloneCandle()
        {
            CandleStateController.Candle1StateChanged += OnCandleStateChangedEvent;
        }

        #endregion

        #region ICandle

        /// <inheritdoc/>
        public event EventHandler<CandleStateChangedEventArgs> CandleStateChangedEvent;

        /// <inheritdoc/>
        public bool CandleIlluminated(CandleID candleId)
        {
            if(candleId == CandleID.Invalid || candleId == CandleID.All)
            {
                throw new ArgumentException($"Candle ID \'{candleId}\' is an invalid Candle to make a CandleStateRequest for.", nameof(candleId));
            }

            return candleId == CandleID.Candle1 && CandleStateController.Candle1State;
        }

        /// <inheritdoc/>
        public void RegisterForCandleStateChangeEvents(CandleID candleId)
        {
            if(candleId == CandleID.Invalid)
            {
                throw new ArgumentException($"Candle ID \'{candleId}\' is an invalid Candle to register events for.", nameof(candleId));
            }

            if(candleId == CandleID.Candle1)
            {
                clientRegistered = true;
            }
        }

        /// <inheritdoc/>
        public void UnregisterForCandleStateChangeEvents(CandleID candleId)
        {
            if(candleId == CandleID.Invalid)
            {
                throw new ArgumentException($"Candle ID \'{candleId}\' is an invalid Candle to unregister events for.", nameof(candleId));
            }

            if(candleId == CandleID.Candle1)
            {
                clientRegistered = false;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Triggered when a candle state changes.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="args">Event arguments containing the candle's state.</param>
        private void OnCandleStateChangedEvent(object sender, CandleStateChangedEventArgs args)
        {
            if(clientRegistered)
            {
                CandleStateChangedEvent?.Invoke(this, args);
            }
        }

        #endregion
    }
}