//-----------------------------------------------------------------------
// <copyright file = "CandleStateController.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System;

    /// <summary>
    /// Standalone controller to simulate the Foundation control of the Candle States.
    /// </summary>
    internal static class CandleStateController
    {
        /// <summary>
        /// Event to be triggered when the state of Candle1 changes.
        /// </summary>
        public static EventHandler<CandleStateChangedEventArgs> Candle1StateChanged;

        /// <summary>
        /// The illumination state for Candle1.
        /// </summary>
        public static bool Candle1State { get; private set; }

        /// <summary>
        /// Toggle the illumination state for Candle1 and invoke the Candle1StateChanged event.
        /// </summary>
        public static void ToggleCandleState()
        {
            Candle1State = !Candle1State;
            Candle1StateChanged?.Invoke(null, new CandleStateChangedEventArgs(CandleID.Candle1, Candle1State));
        }
    }
}
