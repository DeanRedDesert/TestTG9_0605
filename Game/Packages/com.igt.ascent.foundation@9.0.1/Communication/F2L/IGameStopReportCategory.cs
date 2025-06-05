//-----------------------------------------------------------------------
// <copyright file = "IGameStopReportCategory.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L
{
    using System.Collections.Generic;
    using Schemas.Internal;

    /// <summary>
    /// Interface for the game stop report category of the F2L.
    /// </summary>
    public interface IGameStopReportCategory
    {
        /// <summary>
        /// Report the final reel stops of a game. Stops should be reported for base games and free spin games.
        /// </summary>
        /// <param name="physicalReelStops">List of the physical stops.</param>
        void ReportReelStops(ICollection<uint> physicalReelStops);

        /// <summary>
        /// Report a poker hand.
        /// </summary>
        /// <param name="finalHand">Flag indicating if this is the final hand of the game.</param>
        /// <param name="cards">
        /// List of the cards which are part of the hand, each card has the card number and a boolean indicating if it
        /// was held or not.
        /// </param>
        void ReportPokerHand(bool finalHand, ICollection<PokerCard> cards);
    }
}
