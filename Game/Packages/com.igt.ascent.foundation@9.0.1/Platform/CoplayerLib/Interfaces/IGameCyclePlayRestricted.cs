// -----------------------------------------------------------------------
// <copyright file = "IGameCyclePlayRestricted.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.CoplayerLib.Interfaces
{
    using System;

    /// <summary>
    /// An interface containing restricted functionality from <see cref="IGameCyclePlay"/>.
    /// </summary>
    public interface IGameCyclePlayRestricted
    {
        /// <summary>
        /// An event which is raised after a transition of the game cycle state.
        /// </summary>
        /// <remarks>
        /// Warning! Not all game cycle state transitioned event occurs in a transaction.
        /// The transition could be caused by non-transactional events such as
        /// EnrollResponseReadyEvent, OutcomeResponseReadyEvent and FinalizeOutcomeEvent.
        /// </remarks>
        event EventHandler<GameCycleStateTransitionedEventArgs> GameCycleStateTransitioned;
    }
}
