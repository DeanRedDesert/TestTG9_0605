//-----------------------------------------------------------------------
// <copyright file = "IUgpGameMeter.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.GameMeter
{
    using System;

    /// <summary>
    /// Defines an interface that allows a package to send the ASP game meters to the foundation.
    /// </summary>
    public interface IUgpGameMeter
    {
        /// <summary>
        /// Sends the message of current bet per line and selected lines to foundation.
        /// </summary>
        /// <param name='betPerLine'>The bet per line.</param>
        /// <param name='selectedLines'>The selected lines.</param>
        /// <exception cref='NotImplementedException'>
        /// Thrown when a requested method or operation is not implemented.
        /// </exception>
        void SendCurrentBetPerLineAndSelectedLines(long betPerLine, int selectedLines);

        /// <summary>
        /// Sends the message to update game bet meter upon bet.
        /// </summary>
        /// <param name="horizontalKey">The game bet horizontal key.</param>
        /// <param name="verticalKey">The game bet vertical key.</param>
        void SendUpdateGameBetMeterOnBet(string horizontalKey, string verticalKey);

        /// <summary>
        /// Sends the message to update the credit denomination selected by the game upon bet.
        /// </summary>
        /// <param name="creditDenom">The credit denomination to update.</param>
        void SendUpdateGameCreditDenom(long creditDenom);
    }
}
