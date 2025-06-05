//-----------------------------------------------------------------------
// <copyright file = "IPlayerMeters.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    /// <summary>
    /// Interface that represents the values of the set of player meters
    /// maintained by the Foundation.
    /// </summary>
    public interface IPlayerMeters
    {
        /// <summary>
        /// Represents the amount of money available for player betting
        /// that is suitable for display to the player, in base units.
        /// This meter is typically labeled �Credit� in banked-credits environments.
        /// In non banked-credit environments this meter is always zero and is
        /// typically not shown.
        /// </summary>
        long Wagerable { get; }

        /// <summary>
        /// In non banked-credit environments this meter represents the whole of the machine's credit
        /// balance(including the part which is not eligible for cash out) that is suitable for display 
        /// to the player, in base units. This meter is typically labeled "Credit" when
        /// presented to the player.
        /// In a banked-credit environment, this meter represents the player funds that are not immediately
        /// available for wagering, but are part of the machine's balance. These funds are generally available
        /// for(player requested) transfer to the player wagerable meter where they may be used for wagering.
        /// This meter is typically labeled �Bank� when presented to the player.
        /// </summary>
        long Bank { get; }

        /// <summary>
        /// Represents the amount paid to the player during the last/current cashout
        /// and is suitable for display to the player, in base units.
        /// This meter will be reset to zero at the appropriate times
        /// (e.g. at game start or cashout).
        /// </summary>
        long Paid { get; }
    }
}