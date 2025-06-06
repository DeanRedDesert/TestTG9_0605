//-----------------------------------------------------------------------
// <copyright file = "ISportsBettingGamingCycleCategory.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// <auto-generated>
//     This code was generated by C3G.
//
//     Changes to this file may cause incorrect behavior
//     and will be lost if the code is regenerated.
// </auto-generated>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2X
{
    using System;
    using Schemas.Internal.SportsBettingGamingCycle;
    using Schemas.Internal.Types;

    /// <summary>
    /// The SportsBettingGamingCycle category of messages allows the client to enter a sports betting "gaming-cycle",
    /// and place a bet on the player's behalf.
    /// Category: 144; Major Version: 1
    /// </summary>
    /// <remarks>
    /// All documentation is generated from the XSD schema files.
    /// </remarks>
    public interface ISportsBettingGamingCycleCategory
    {
        /// <summary>
        /// A request to commit the entire reserved amount.  A reserve that was adjusted to a zero amount must still be
        /// committed.  This message is always successful (assuming it is issued in a valid state).  If invalid an
        /// exeception is returned.  Retruns an exception if there is no open/uncommitted reserve.  Returns an
        /// exception if not in the Committed state.
        /// </summary>
        void CommitSportsBetReserve();

        /// <summary>
        /// Commit the sports betting gaming-cycle.  Transitions from Idle state to the Committed state.  Returns an
        /// exception if not in the Idle state.
        /// </summary>
        /// <returns>
        /// The content of the CommitSportsBettingGamingCycleReply message.
        /// </returns>
        bool CommitSportsBettingGamingCycle();

        /// <summary>
        /// End the sports betting gaming-cycle.  Transitions from the Committed state to the Idle state.  Returns an
        /// exception if not in the Committed state or if money operations are pending.
        /// </summary>
        void EndSportsBettingGamingCycle();

        /// <summary>
        /// Get the sports betting gaming-cycle state.
        /// </summary>
        /// <returns>
        /// The content of the GetSportsBettingGamingCycleStateReply message.
        /// </returns>
        SportsBettingGamingCycleState GetSportsBettingGamingCycleState();

        /// <summary>
        /// Retrieves the gaming-cycle state-machine related properties. All SportsBettingProperties will be returned in
        /// this message. This message may **ONLY** be sent during ActivateContext.
        /// </summary>
        /// <returns>
        /// The content of the GetSportsBettingPropertiesReply message.
        /// </returns>
        SportsBettingProperties GetSportsBettingProperties();

        /// <summary>
        /// Get the reserved amount, if any.  The reservation (amount) element will be omitted on the reply if a
        /// reservation has not been made during the gaming-cycle, or if the reservation has been committed.  This
        /// method may be called in any gaming-cycle state.
        /// </summary>
        /// <returns>
        /// The content of the GetSportsBettingReserveReply message.
        /// </returns>
        Reserve GetSportsBettingReserve();

        /// <summary>
        /// A request to reserve an amount from the player meters.  This call changes the credit balance/player meters.
        /// This message may be used to adjust the reserve amount.  A reduction in the reserve amount is guaranteed to
        /// succeed.  Returns an exception if not in the Committed state.
        /// </summary>
        /// <param name="amount">
        /// The amount requested.
        /// </param>
        /// <returns>
        /// The content of the ReserveSportsBetReply message.
        /// </returns>
        bool ReserveSportsBet(Amount amount);

    }

}

