// -----------------------------------------------------------------------
// <copyright file = "IGameCyclePlay.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.CoplayerLib.Interfaces
{
    using System;
    using System.Collections.Generic;
    using OutcomeList.Interfaces;
    using Platform.Interfaces;
    using GameCycleStateEnum = Platform.Interfaces.GameCycleState;

    /// <summary>
    /// This class defines the interface for the coplayers to talk to the Foundation in terms of game cycle play.
    /// </summary>
    public interface IGameCyclePlay
    {
        #region Events

        /// <summary>
        /// Occurs when an abort request has completed,
        /// and the game cycle state has transitioned to <see cref="GameCycleStateEnum.Finalized"/> state.
        /// </summary>
        event EventHandler<AbortCompleteEventArgs> AbortCompleteEvent;

        /// <summary>
        /// Occurs when the enrollment results are available,
        /// and the game cycle state has transitioned to <see cref="GameCycleStateEnum.EnrollComplete"/> state.
        /// </summary>
        event EventHandler<EnrollResponseReadyEventArgs> EnrollResponseReadyEvent;

        /// <summary>
        /// Occurs when the outcome list has been evaluated and (potentially) adjusted,
        /// and the game cycle state has transitioned to either
        /// <see cref="GameCycleStateEnum.Playing"/> state (when not last outcome)
        /// or <see cref="GameCycleStateEnum.MainPlayComplete"/> state (when last outcome).
        /// </summary>
        event EventHandler<OutcomeResponseReadyEventArgs> OutcomeResponseReadyEvent;

        /// <summary>
        /// Occurs when the Foundation has finalized (metered, posted, and logged) the outcome,
        /// and the game cycle state has transitioned to <see cref="GameCycleStateEnum.Finalized"/> state.
        /// </summary>
        event EventHandler<FinalizeOutcomeEventArgs> FinalizeOutcomeEvent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current GameCycleState.
        /// Returns the current game cycle state if game is in Play Mode,
        /// <see cref="GameCycleStateEnum.Invalid"/> otherwise.
        /// </summary>
        GameCycleStateEnum GameCycleState { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Requests the Foundation to start a game cycle and transition the game cycle to
        /// <see cref="GameCycleStateEnum.Committed"/> state.
        /// </summary>
        /// <remarks>
        /// This method is valid in <see cref="GameCycleStateEnum.Idle"/> state only.
        /// </remarks>
        /// <returns>
        /// True if the request is accepted.  The game is <see cref="GameCycleStateEnum.Committed"/> state,
        /// and may commit a bet.
        /// False if the request is rejected.  The game stays in <see cref="GameCycleStateEnum.Idle"/> state
        /// </returns>
        bool CommitGameCycle();

        /// <summary>
        /// Informs the Foundation that the current game cycle is being canceled
        /// and the game should return to <see cref="GameCycleStateEnum.Idle"/> state.
        /// </summary>
        /// <remarks>
        /// Before calling this function, the game should un-commit the bet by calling
        /// <see cref="IGameCycleBetting.UncommitBet"/> first. Otherwise an exception would be thrown.
        /// 
        /// This method is valid in <see cref="GameCycleStateEnum.Committed"/> state only.
        /// </remarks>
        void UncommitGameCycle();

        /// <summary>
        /// Informs the Foundation to enroll the game cycle, and transition the game cycle to
        /// <see cref="GameCycleStateEnum.EnrollPending"/> state.
        /// When the Foundation has finished enrollment, an <see cref="EnrollResponseReadyEvent"/> will be posted.
        /// </summary>
        /// <remarks>
        /// Before calling this function, the game should commit a bet by calling
        /// <see cref="IGameCycleBetting.CommitBet"/> first. Otherwise an exception would be thrown.
        /// 
        /// This method is valid in <see cref="GameCycleStateEnum.Committed"/> state only.
        /// </remarks>
        /// <param name="enrollmentData">Binary enrollment data.</param>
        void EnrollGameCycle(byte[] enrollmentData = null);

        /// <summary>
        /// Informs the Foundation to un-enroll the game cycle and transition the game cycle to
        /// <see cref="GameCycleStateEnum.Idle"/> state.  This indicates to Foundation that
        /// the game cycle is being canceled.
        /// </summary>
        /// <remarks>
        /// Before calling this function, the game should un-commit the bet by calling
        /// <see cref="IGameCycleBetting.UncommitBet"/> first. Otherwise an exception would be thrown.
        /// 
        /// This method is valid in <see cref="GameCycleStateEnum.EnrollComplete"/> state only.
        /// </remarks>
        void UnenrollGameCycle();

        /// <summary>
        /// Informs the Foundation that the game is starting play, and to transition the game cycle state to
        /// <see cref="GameCycleStateEnum.Playing"/> state.
        /// </summary>
        /// <remarks>
        /// Before calling this function, the game should place the committed bet first by calling
        /// <see cref="IGameCycleBetting.PlaceStartingBet"/> once. Otherwise an exception would be thrown.
        /// 
        /// This method is valid in <see cref="GameCycleStateEnum.EnrollComplete"/> state only.
        /// </remarks>
        void StartPlaying();

        /// <summary>
        /// Requests an outcome list, which is NOT the last one of the game cycle, to be evaluated (and potentially adjusted).
        /// Transitions the game cycle to <see cref="GameCycleStateEnum.EvaluatePending"/> state.
        /// When Foundation is done with the evaluation, an <see cref="OutcomeResponseReadyEvent"/> will be posted.
        /// </summary>
        /// <remarks>
        /// This is to evaluate the interim outcomes during a game cycle.
        /// To evaluate the last outcome of the game cycle, use the <see cref="AdjustLastOutcome"/> call instead.
        /// 
        /// This method is valid only in <see cref="GameCycleStateEnum.Playing"/>, <see cref="GameCycleStateEnum.AncillaryPlaying"/>
        /// and <see cref="GameCycleStateEnum.BonusPlaying"/> states.
        /// </remarks>
        /// <param name="outcomeList">
        /// The outcome list to be evaluated and adjusted.
        /// </param>
        void AdjustOutcome(IOutcomeList outcomeList);

        /// <summary>
        /// Requests the last outcome list of the game cycle to be evaluated (and potentially adjusted).
        /// Transitions the game cycle to <see cref="GameCycleStateEnum.EvaluatePending"/> state.
        /// When Foundation is done with the evaluation, an <see cref="OutcomeResponseReadyEvent"/> will be posted.
        /// </summary>
        /// <remarks>
        /// This is to evaluate the last outcome of a game cycle.
        /// To evaluate the outcome that is NOT the last, use the <see cref="AdjustOutcome"/> call instead.
        /// 
        /// This method is valid only in <see cref="GameCycleStateEnum.Playing"/>, <see cref="GameCycleStateEnum.AncillaryPlaying"/>
        /// and <see cref="GameCycleStateEnum.BonusPlaying"/> states.
        /// </remarks>
        /// <param name="outcomeList">
        /// The last outcome list of the game cycle to be evaluated and adjusted.
        /// </param>
        /// <param name="wagerCatOutcomeList">
        /// The list of <see cref="WagerCategoryOutcome"/>.  Must not be null or empty.
        /// </param>
        void AdjustLastOutcome(IOutcomeList outcomeList, IList<WagerCategoryOutcome> wagerCatOutcomeList);

        /// <summary>
        /// Informs the Foundation to finalize the outcome list, and transition the game cycle to
        /// <see cref="GameCycleStateEnum.FinalizeAwardPending"/> state.
        /// When Foundation is done with finalizing the awards, a <see cref="FinalizeOutcomeEvent"/> will be posted.
        /// </summary>
        /// <remarks>
        /// This method is valid only in <see cref="GameCycleStateEnum.MainPlayComplete"/>, <see cref="GameCycleStateEnum.AncillaryPlayComplete"/>
        /// and <see cref="GameCycleStateEnum.BonusPlayComplete"/> states.
        /// </remarks>
        void FinalizeOutcome();

        /// <summary>
        /// Requests the Foundation to abort the game cycle, and transition the game cycle to
        /// <see cref="GameCycleStateEnum.AbortPending"/> state.
        /// When Foundation is done with processing the abort,  an <see cref="AbortCompleteEvent"/> will be posted.
        /// </summary>
        /// <remarks>
        /// This method is valid only in <see cref="GameCycleStateEnum.Playing"/> and <see cref="GameCycleStateEnum.MainPlayComplete"/> states.
        /// </remarks>
        /// <returns>
        /// True if the request is accepted.  The game is in <see cref="GameCycleStateEnum.AbortPending"/> state,
        /// and must wait for the <see cref="AbortCompleteEvent"/>.
        /// False if the request is rejected.  The game stays in its current state.
        /// </returns>
        bool AbortGameCycle();

        /// <summary>
        /// Informs the Foundation to end a game-cycle, and transition the game cycle to
        /// <see cref="GameCycleStateEnum.Idle"/> state.
        /// </summary>
        /// <remarks>
        /// This function will block until the Foundation allows the game to end.
        /// 
        /// The method is valid in <see cref="GameCycleStateEnum.Finalized"/> state only.
        /// </remarks>
        /// <param name="numberOfSteps">
        /// The number of history steps that are recorded in history for the current game cycle.
        /// </param>
        void EndGameCycle(int numberOfSteps);

        /// <summary>
        /// Gets the enrollment response data.
        /// </summary>
        /// <returns>
        /// The <see cref="EnrollResponseData"/> retrieved from the Foundation.
        /// </returns>
        EnrollResponseData GetEnrollResponseData();

        /// <summary>
        /// Gets the outcome response data.
        /// </summary>
        /// <returns>
        /// The <see cref="OutcomeResponseData"/> retrieved from the Foundation.
        /// </returns>
        OutcomeResponseData GetOutcomeResponseData();

        #endregion
    }
}