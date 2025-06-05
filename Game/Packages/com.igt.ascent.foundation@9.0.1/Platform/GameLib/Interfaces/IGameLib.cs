//-----------------------------------------------------------------------
// <copyright file = "IGameLib.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.OutcomeList.Interfaces;

    /// <summary>
    /// GameLib interface which contains the most used functionality
    /// used by all client code.
    /// </summary>
    public interface IGameLib : IGameContextEvents, IGameCriticalData, IGameConfiguration
    {
        #region Meter, RNG and Service Request Functions

        /// <summary>
        /// Get a set of random numbers with a single request.
        /// </summary>
        /// <param name="request">
        /// A request specifying the counts and ranges of the random numbers requested.
        /// </param>
        /// <returns>The list of random numbers as requested.</returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        ICollection<int> GetRandomNumbers(RandomValueRequest request);

        /// <summary>
        /// Get a set of random numbers with a list of requests.
        /// </summary>
        /// <param name="requestList">
        /// A list of requests specifying the counts and ranges of the random
        /// numbers requested.  For instance, the client may request 3 values
        /// with ranges of 1-30 and 2 values with ranges of 5-10 in the same call.
        /// </param>
        /// <returns>The list of random numbers as requested.</returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        ICollection<int> GetRandomNumbers(ICollection<RandomValueRequest> requestList);

        /// <summary>
        /// Request that the Foundation change the denomination within the current theme.
        /// The function will not block until the request is satisfied. The response
        /// will be sent back in the NewThemeContextEvent.
        /// </summary>
        /// <praram name = "newDenomination">The new denomination to be set.</praram>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="newDenomination"/> is less than 1.
        /// </exception>
        /// <exception cref="FunctionCallNotAllowedInModeOrStateException">
        /// Thrown when the function is called in a wrong game mode or a wrong game cycle state.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <paramref name="newDenomination"/> is not available for the player to pick.
        /// </exception>
        /// <returns>True if the request was accepted.</returns>
        bool RequestDenominationChange(long newDenomination);

        /// <summary>
        /// Request that the system show the theme selection menu.
        /// </summary>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <returns>True if the request was sent to foundation.</returns>
        /// <remarks>This request will return false if made in an inappropriate state. For example, it will fail if the game is in the Utility or History context, or if a game cycle is in progress.</remarks>
        bool RequestThemeSelectionMenu();

        /// <summary>
        /// Request that the system show the service window.
        /// </summary>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        void RequestServiceWindow();

        /// <summary>
        /// Get the denominations available for the player to pick.
        /// </summary>
        /// <returns>List of denominations.</returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="FunctionCallNotAllowedInModeOrStateException">
        /// Thrown when the function is called in a wrong game mode or a wrong game cycle state.
        /// </exception>
        ICollection<long> GetAvailableDenominations();

        /// <summary>
        /// Get the denominations that are associated with progressives and available for the player to pick.
        /// </summary>
        /// <returns>List of denominations contain both system progressive and GCP linked.</returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="FunctionCallNotAllowedInModeOrStateException">
        /// Thrown when the function is called in a wrong game mode or a wrong game cycle state.
        /// </exception>
        ICollection<long> GetAvailableProgressiveDenominations();

        /// <summary>
        /// Get the denominations that have progressives connected to them.
        /// </summary>
        /// <returns>
        /// A dictionary of dictionaries of progressive data keyed by the denomination.
        /// The progressive data dictionary is keyed by the game level.
        /// The dictionary would not contain data for GCP.
        /// </returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        IDictionary<long, IDictionary<int, ProgressiveBroadcastData>> GetAvailableDenominationsWithProgressives();

        /// <summary>
        /// Get the foundation progressives for a specific denom.
        /// </summary>
        /// <returns>
        /// A dictionary of progressive data.
        /// The progressive data dictionary is keyed by the game level.
        /// </returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        IDictionary<int, ProgressiveBroadcastData> GetAvailableProgressiveBroadcastData(long denom);

        /// <summary>
        /// Notify the Foundation that game language has been changed.
        /// </summary>
        /// <praram name = "newLanguage">The new language to be set.</praram>
        /// <exception cref="FunctionCallNotAllowedInModeOrStateException">
        /// Thrown when the function is called in a wrong game mode or a wrong game cycle state.
        /// </exception>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the language passed in is not an enabled language.
        /// </exception>
        void SetLanguage(string newLanguage);

        /// <summary>
        /// Sets the game language to be the default game language.
        /// </summary>
        /// <exception cref="FunctionCallNotAllowedInModeOrStateException">
        /// Thrown when the function is called in a wrong game mode or a wrong game cycle state.
        /// </exception>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <returns>
        /// The language that had been set. Or, null means that the method is not supported.
        /// </returns>
        string SetDefaultLanguage();

        /// <summary>
        /// Get the current date and time.
        /// </summary>
        /// <returns>Current date and time.</returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        DateTime GetDateTime();

        /// <summary>
        /// Gets the current credit meter behavior.
        /// </summary>
        /// <remarks>
        /// Can also be obtained from <see cref="PresentationBehaviorConfigs"/>.
        /// </remarks>
        /// <returns>The credit meter display behavior.</returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        CreditMeterDisplayBehaviorMode GetCreditMeterBehavior();

        #endregion

        #region Game Cycle Functions

        /// <summary>
        /// Get the current game cycle state maintained by the Foundation.
        /// </summary>
        /// <returns>
        /// The current game cycle if game is in Play Mode.
        /// GameCycleState.Invalid otherwise.
        /// </returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        GameCycleState QueryGameCycleState();

        /// <summary>
        /// Check with the Foundation to see if it is okay to transition
        /// from Idle to Committed state.
        /// </summary>
        /// <returns>True if the transition is allowed.  False otherwise.</returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="FunctionCallNotAllowedInModeOrStateException">
        /// Thrown when the function is called in a wrong game mode or a wrong game cycle state.
        /// </exception>
        bool CanCommitGameCycle();

        /// <summary>
        /// Request that the Foundation move into the Committed state and that a game cycle start.
        /// </summary>
        /// <remarks>
        /// If the function returns true, then the game may commit a bet.
        /// If the Foundation rejected the request, then the game should remain in Idle.
        /// A game can only be committed from Idle.
        /// </remarks>
        /// <returns>True if the transition is accepted.  False otherwise.</returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        bool CommitGameCycle();

        /// <summary>
        /// Inform the Foundation that the current game cycle is being canceled
        /// and the Foundation should return to Idle.
        /// </summary>
        /// <remarks>
        /// Before calling this function, the game should un-commit the bet by
        /// calling <see cref="UncommitBet"/> first.  Otherwise an exception would
        /// be thrown.
        /// The game can only be uncommitted from the Committed state.
        /// </remarks>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="FunctionCallNotAllowedInModeOrStateException">
        /// Thrown when the function is called in a wrong game mode or a wrong game cycle state.
        /// </exception>
        void UncommitGameCycle();

        /// <summary>
        /// Enroll the game cycle, cause the Foundation to enter the Enroll Pending state.
        /// When the foundation has finished enrollment an EnrollResponseEvent will be posted.
        /// </summary>
        /// <remarks>
        /// Before calling this function, the game should commit a bet by calling
        /// <see cref="CommitBet"/> first.  Otherwise an exception would be thrown.
        /// </remarks>
        /// <param name="enrollmentData">Enrollment system specific data.</param>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="FunctionCallNotAllowedInModeOrStateException">
        /// Thrown when the function is called in a wrong game mode or a wrong game cycle state.
        /// </exception>
        void EnrollGameCycle(byte[] enrollmentData);

        /// <summary>
        /// Indicate to the Foundation that the game is canceling the current game cycle,
        /// and the Foundation should return to Idle.
        /// </summary>
        /// <remarks>
        /// This function should only be called while the Foundation is in the Enroll Complete state.
        /// Before calling this function, the game should un-commit the bet by
        /// calling <see cref="UncommitBet"/> first.  Otherwise an exception would
        /// be thrown.
        /// </remarks>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="FunctionCallNotAllowedInModeOrStateException">
        /// Thrown when the function is called in a wrong game mode or a wrong game cycle state.
        /// </exception>
        void UnenrollGameCycle();

        /// <summary>
        /// Check with the Foundation to see if it is okay to start playing
        /// and transition from the Enroll Complete state to the Playing state. 
        /// </summary>
        /// <returns>True if the transition is allowed.  False otherwise.</returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="FunctionCallNotAllowedInModeOrStateException">
        /// Thrown when the function is called in a wrong game mode or a wrong game cycle state.
        /// </exception>
        bool CanStartPlaying();

        /// <summary>
        /// Inform the Foundation that the game is starting play, and the Foundation
        /// to move from the Enroll Complete state to the Playing state.
        /// </summary>
        /// <remarks>
        /// Before calling this function, the game should place the committed bet first
        /// by calling <see cref="PlaceStartingBet"/> once.  Otherwise an exception
        /// would be thrown.
        /// </remarks>
        /// <returns>True if the transition is accepted.  False otherwise.</returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        bool StartPlaying();

        /// <summary>
        /// Inform the Foundation that the game has completed, and the Foundation
        /// should transition from Finalized to Idle state.
        /// </summary>
        /// <remarks>
        /// This function will block until the Foundation allows the game to end.
        /// and is valid only when it is called from within Finalized state.
        /// </remarks>
        /// <param name="historySteps">The number of history steps that are recorded in
        ///                            history for the current game cycle .</param>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="FunctionCallNotAllowedInModeOrStateException">
        /// Thrown when the function is called in a wrong game mode or a wrong game cycle state.
        /// </exception>
        void EndGameCycle(uint historySteps);

        /// <summary>
        /// Notify the Foundation that the game would like to offer the player
        /// an ancillary game cycle.
        /// </summary>
        /// <remarks>
        /// This function can only be called from within Main Play Complete state.
        /// The Foundation may deny the request by returning false.
        /// </remarks>
        /// <returns>True if an ancillary game may be offered.  False otherwise.</returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="FunctionCallNotAllowedInModeOrStateException">
        /// Thrown when the function is called in a wrong game mode or a wrong game cycle state.
        /// </exception>
        bool OfferAncillaryGame();

        /// <summary>
        /// Request that the Foundation start an ancillary game cycle, and
        /// move into the Ancillary Playing state.
        /// </summary>
        /// <remarks>
        /// The game should track whether the ancillary game is offerable by calling <see cref="OfferAncillaryGame"/>
        /// and handling <see cref="DisableAncillaryGameOfferEvent"/> diligently.
        /// It is an error to call <see cref="StartAncillaryPlaying"/> when ancillary game is not offerable.
        /// </remarks>
        /// <returns>
        /// True if the request is accepted and ancillary game cycle has started.
        /// False if ancillary game may not be started due to some last minute change that has not been
        /// communicated to the game via <see cref="DisableAncillaryGameOfferEvent"/>.
        /// </returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        bool StartAncillaryPlaying();

        /// <summary>
        /// Request the foundation to start the bonus extension play.
        /// Transitions the GameCycle state from MainPlayComplete/AncillaryPlayComplete to BonusPlaying.
        /// </summary>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="FunctionCallNotAllowedInModeOrStateException">
        /// Thrown when the function is called in a wrong game mode or a wrong game cycle state.
        /// </exception>
        /// <returns>
        /// True if the bonus extension game was successfully started.
        /// </returns>
        bool StartBonusPlaying();

        /// <summary>
        /// Allow the foundation to adjust the outcome, causing the Foundation
        /// to transition from the Playing state to the Evaluate Pending state;
        /// or from Ancillary Playing to Ancillary Evaluate Pending if the game
        /// is currently in Ancillary Play;
        /// or from Bonus Playing to Bonus Evaluate Pending if the game
        /// is currently in Bonus Extension Play.
        /// When the foundation is finished adjusting the outcome,
        /// a <see cref="OutcomeResponseEvent"/> will be posted.
        /// </summary>
        /// <param name="outcome">The outcome to adjust.</param>
        /// <param name="isFinalOutcome">Flag indicating if this is the last outcome.</param>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the outcome list passed in is null.
        /// </exception>
        /// <exception cref="FunctionCallNotAllowedInModeOrStateException">
        /// Thrown when the function is called in a wrong game mode or a wrong game cycle state.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the final outcome is adjusted without at least 1 wager category present.
        /// </exception>
        void AdjustOutcome(IOutcomeList outcome, bool isFinalOutcome);

        /// <summary>
        /// Finalize the current outcome, causing the Foundation to transition
        /// to the Finalize Award Pending state.
        /// It allows the outcome which was sent in the previous outcome to be finalized.
        /// When the Foundation is finished finalizing the outcome,
        /// a FinalizeOutcomeResponseEvent will be posted.
        /// </summary>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="FunctionCallNotAllowedInModeOrStateException">
        /// Thrown when the function is called in a wrong game mode or a wrong game cycle state.
        /// </exception>
        void FinalizeOutcome();

        #endregion

        #region Betting Functions

        /// <summary>
        /// Check to see if the given bet amount may be committed.
        /// </summary>
        /// <param name="bet">Total requested bet amount in units of the denomination passed in.</param>
        /// <param name="denomination">The denomination of the bet.</param>
        /// <returns>True if requested bet amount may be committed.  False otherwise.</returns>
        /// <remarks>
        /// This function can only be called while the Foundation is in Idle or
        /// Committed state.
        /// </remarks>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="bet"/> is less than 0, or <paramref name="denomination"/> is less than 1.
        /// </exception>
        /// <exception cref="FunctionCallNotAllowedInModeOrStateException">
        /// Thrown when the function is called in a wrong game mode or a wrong game cycle state.
        /// </exception>
        bool CanCommitBet(long bet, long denomination);

        /// <summary>
        /// Check which of the given list of bets can be committed.
        /// </summary>
        /// <param name="bets">The list of bet amounts in units of the denomination passed in.</param>
        /// <param name="denomination">The denomination of the bet.</param>
        /// <returns>A enumerable of Boolean flags in which each Boolean value correlates to a passed bet.</returns>
        /// <remarks>
        /// This function can only be called while the Foundation is in Idle or
        /// Committed state.
        /// </remarks>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="bets"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="bets"/> is empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="bets"/> has element which is less than 0, or <paramref name="denomination"/> is less than 1.
        /// </exception>
        /// <exception cref="FunctionCallNotAllowedInModeOrStateException">
        /// Thrown when the function is called in a wrong game mode or a wrong game cycle state.
        /// </exception>
        IEnumerable<bool> CanCommitBets(IEnumerable<long> bets, long denomination);

        /// <summary>
        /// Commit a bet to the Foundation.
        /// </summary>
        /// <param name="bet">The be amount in units of the denomination passed in.</param>
        /// <param name="denomination">The denomination of the bet.</param>
        /// <returns></returns>
        /// <remarks>
        /// If the bet has not been previously checked with CanCommitBet, the request can fail.
        /// A bet can only be committed while the Foundation is in the Committed state.
        /// A committed bet is escrow-ed by the Foundation so that it cannot be affected by
        /// funds transfer off the machine.
        ///
        /// CommitBet can be called more than once before enrolling.  It is the last call that matters.
        /// The bet amount in latter calls can be more or less than the bet amount in the previous calls.
        /// For example, it is okay to call CommitBet(5, 1) followed by CommitBet(20, 1), then CommitBet(10, 1).
        /// The final committed bet will be 10.
        /// 
        /// This function must be called prior to enrolling the game by
        /// <see cref="EnrollGameCycle"/>.
        /// </remarks>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when bet is less than 0, or denomination is less than 1.
        /// </exception>
        /// <exception cref="FunctionCallNotAllowedInModeOrStateException">
        /// Thrown when the function is called in a wrong game mode or a wrong game cycle state.
        /// </exception>
        bool CommitBet(long bet, long denomination);

        /// <summary>
        /// Uncommit the entire committed amount from the Foundation.
        /// </summary>
        /// <remarks>
        /// If a game wishes to cancel a game cycle either from Committed or from Enroll Complete,
        /// then it should call this function to release the escrow-ed bet from the Foundation.
        /// When canceling a game the entire committed amount must be uncommitted.
        /// This function must be called prior to un-enrolling the game by
        /// <see cref="UnenrollGameCycle"/>.
        /// </remarks>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="FunctionCallNotAllowedInModeOrStateException">
        /// Thrown when the function is called in a wrong game mode or a wrong game cycle state.
        /// </exception>
        void UncommitBet();

        /// <summary>
        /// Query the current committed bet amount.
        /// </summary>
        /// <param name="bet">Return the bet amount in units of the denomination returned.</param>
        /// <param name="denomination">Return the denomination of the bet.</param>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        void GetCommittedBet(out long bet, out long denomination);

        /// <summary>
        /// Place the currently committed bet amount with the Foundation. 
        /// </summary>
        /// <remarks>
        /// Once a bet has been placed it can no longer be canceled.
        /// This function must be called prior to starting the game play by
        /// <see cref="StartPlaying"/>, and be called only once.  Otherwise
        /// an exception would be thrown.
        /// Once the Foundation is in Playing state, mid-game bet can be placed by
        /// <see cref="PlaceBet"/> or <see cref="PlaceBetAgainstPendingWins"/>.
        /// </remarks>
        /// <seealso cref="PlaceBet"/>
        /// <seealso cref="PlaceBetAgainstPendingWins"/>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="FunctionCallNotAllowedInModeOrStateException">
        /// Thrown when the function is called in a wrong game mode or a wrong game cycle state.
        /// </exception>
        void PlaceStartingBet();

        /// <summary>
        /// Check to see if the given bet amount may be placed during a game
        /// play (mid-game bet).
        /// </summary>
        /// <param name="bet">The bet amount in units of the denomination passed in.</param>
        /// <param name="denomination">The denomination of the bet.</param>
        /// <returns>True if the requested bet amount may be placed.  False otherwise.</returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when bet is less than 0, or denomination is less than 1.
        /// </exception>
        /// <exception cref="FunctionCallNotAllowedInModeOrStateException">
        /// Thrown when the function is called in a wrong game mode or a wrong game cycle state.
        /// </exception>
        bool CanPlaceBet(long bet, long denomination);

        /// <summary>
        /// Check to see if a mid-game bet can be placed against the pending wins.
        /// </summary>
        /// <param name="bet">Total requested bet amount in units of the denomination passed in.</param>
        /// <param name="betFromCredits">The amount to bet against the current balance,
        ///                              in units of the denomination passed in.</param>
        /// <param name="betFromPendingWins">The amount to bet against the pending wins,
        ///                                  in units of the denomination passed in.</param>
        /// <param name="availablePendingWins">The win amount the game has available to bet against,
        ///                                    in units of the denomination passed in.
        ///                                    This amount should not include amounts
        ///                                    previously bet against.</param>
        /// <param name="denomination">The denomination for the bet and win amounts.</param>
        /// <returns>True if the requested bet amount may be placed.  False otherwise.</returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when any bet or win amount is less than 0, or denomination is less than 1.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the bet amounts passed in don't add up, or <paramref name="betFromPendingWins"/>
        /// is greater than <paramref name="availablePendingWins"/>.
        /// </exception>
        /// <exception cref="FunctionCallNotAllowedInModeOrStateException">
        /// Thrown when the function is called in a wrong game mode or a wrong game cycle state.
        /// </exception>
        bool CanPlaceBetAgainstPendingWins(long bet, long betFromCredits, long betFromPendingWins,
                                           long availablePendingWins, long denomination);

        /// <summary>
        /// Place a mid-game bet with the Foundation against the credit balance.
        /// Multiple calls to this function may be made, and are cumulative in
        /// respect to the amount placed by the Foundation.
        /// </summary>
        /// <param name="bet">The bet amount in units of the denomination passed in.</param>
        /// <param name="denomination">The denomination of the bet.</param>
        /// <returns>True if the requested be has been placed.  False otherwise.</returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when bet is less than 0, or denomination is less than 1.
        /// </exception>
        bool PlaceBet(long bet, long denomination);

        /// <summary>
        /// Place a mid-game bet with the Foundation against wins which are pending.
        /// Multiple calls to this function may be made, and are cumulative in
        /// respect to the amount placed by the Foundation.
        /// </summary>
        /// <param name="bet">Total requested bet amount in units of the denomination passed in.</param>
        /// <param name="betFromCredits">The amount to bet against the current balance,
        ///                              in units of the denomination passed in.</param>
        /// <param name="betFromPendingWins">The amount to bet against the pending wins,
        ///                                  in units of the denomination passed in.</param>
        /// <param name="availablePendingWins">The win amount the game has available to bet against,
        ///                                    in units of the denomination passed in.
        ///                                    This amount should not include amounts
        ///                                    previously bet against.</param>
        /// <param name="denomination">The denomination for the bet and win amounts.</param>
        /// <returns>True if the requested be has been placed.  False otherwise.</returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when any bet or win amount is less than 0, or denomination is less than 1.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the bet amounts passed in don't add up, or <paramref name="betFromPendingWins"/>
        /// is greater than <paramref name="availablePendingWins"/>.
        /// </exception>
        bool PlaceBetAgainstPendingWins(long bet, long betFromCredits, long betFromPendingWins,
                                        long availablePendingWins, long denomination);

        /// <summary>
        /// Check which of the given list of bets can be committed in next game cycle.
        /// </summary>
        /// <param name="bets">The list of bet amounts in units of the denomination passed in.</param>
        /// <param name="denomination">The denomination of the bet.</param>
        /// <returns>A enumerable of Boolean flags in which each Boolean value correlates to a passed bet.</returns>
        /// <remarks>
        /// This function is designed for the ancillary/Double Up game 
        /// and can only be called while the Foundation is in Main Play Complete state, 
        /// where the player choose to start a new game instead of ancillary/Double Up game,
        /// this function is used to check the given bet amounts are committal against the money
        /// in the bank, together with the win amount not committed to the bank yet.
        /// </remarks>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="bets"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="bets"/> is empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="bets"/> has element which is less than 0, or <paramref name="denomination"/> is less than 1.
        /// </exception>
        /// <exception cref="FunctionCallNotAllowedInModeOrStateException">
        /// Thrown when the function is called in a wrong game mode or a wrong game cycle state.
        /// </exception>
        IEnumerable<bool> CanBetNextGameCycle(IEnumerable<long> bets, long denomination);

        /// <summary>
        /// Check to see if the given bet amount may be committed in next game cycle.
        /// </summary>
        /// <param name="bet">The bet amount in units of the denomination passed in.</param>
        /// <param name="denomination">The denomination of the bet.</param>
        /// <returns>True if requested bet amount may be committed in next game cycle. False otherwise.</returns>
        /// <remarks>
        /// This function can only be called while the Foundation is in Main Play Complete state. 
        /// </remarks>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="bet"/> is less than 0, or <paramref name="denomination"/> is less than 1.
        /// </exception>
        /// <exception cref="FunctionCallNotAllowedInModeOrStateException">
        /// Thrown when the function is called in a wrong game mode or a wrong game cycle state.
        /// </exception>
        bool CanBetNextGameCycle(long bet, long denomination);

        #endregion

        #region Money Functions

        /// <summary>
        /// Query the values of the set of player meters.
        /// </summary>
        /// <returns>
        /// PlayerMeters struct that holds the current meter values, in base units.
        /// </returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        PlayerMeters GetPlayerMeters();

        /// <summary>
        /// Query the bank status.
        /// </summary>
        /// <returns>The bank status at the time.</returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        BankStatus QueryBankStatus();

        /// <summary>
        /// Request that the system initiate a cash out.  The function will not block
        /// until the cash out request is satisfied. The response will be sent back
        /// in the CashOutEvent.
        /// </summary>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        void RequestCashOut();

        /// <summary>
        /// Request a money movement of the default amount between meters.
        /// The default amount is only known to the Foundation.
        /// The function will not block until the money move request is satisfied.
        /// The response will be sent back in the MoneyEvent.
        /// </summary>
        /// <param name="from">The origination of the money movement.</param>
        /// <param name="to">The destination of the money movement.</param>
        /// <returns>True if the request is valid and accepted for processing.  False otherwise.</returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        bool RequestMoneyMove(MoneyLocation from, MoneyLocation to);

        #endregion

        #region Environment Attribute Functions

        /// <summary>
        /// Check if the gaming environment has the attribute described
        /// by a given Environment Attribute.
        /// </summary>
        /// <param name="environmentAttribute">The environment attribute to check.</param>
        /// <returns>True if the gaming environment has the attribute.  False otherwise.</returns>
        bool IsEnvironmentTrue(EnvironmentAttribute environmentAttribute);

        #endregion

        #region Auto Play

        /// <summary>
        /// Query if the auto play is currently on. 
        /// </summary>
        /// <returns>True if the auto play is currently on.</returns>
        bool IsAutoPlayOn();

        /// <summary>
        /// Request the foundation to turn on the auto play.
        /// </summary>
        /// <returns>True if the auto play has been successfully turned on.</returns>
        bool SetAutoPlayOn();

        /// <summary>
        /// Request the foundation to turn off the auto play.
        /// </summary>
        void SetAutoPlayOff();

        #endregion

        #region Game Stop Reporting

        /// <summary>
        /// Report the final reel stops of a game. Stops should be reported for base games and free spin games.
        /// </summary>
        /// <param name="physicalReelStops">List of the physical stops.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="physicalReelStops"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="physicalReelStops"/> is empty.</exception>
        void ReportReelStops(ICollection<uint> physicalReelStops);

        #endregion

        #region Extended Interface Handling

        /// <summary>
        /// Get an extended interface. This allows for extension of platform features beyond <see cref="IGameLib"/>.
        /// </summary>
        /// <typeparam name="TExtendedInterface">
        /// Interface to get an implementation of.
        /// </typeparam>
        /// <returns>
        /// An implementation of the interface. If no implementation can be accessed, then <see langword="null"/> will 
        /// be returned.
        /// </returns>
        TExtendedInterface GetInterface<TExtendedInterface>() where TExtendedInterface : class;

        #endregion

        #region Events

        /// <summary>
        /// Event handler of AutoPlayOffEvent.
        /// </summary>
        event EventHandler<AutoPlayOffEventArgs> AutoPlayOffEvent;

        /// <summary>
        /// Event handler of AutoPlayOnRequestEvent.
        /// </summary>
        event EventHandler<AutoPlayOnRequestEventArgs> AutoPlayOnRequestEvent;

        /// <summary>
        /// Event handler of BankStatusChangedEvent.
        /// </summary>
        event EventHandler<BankStatusChangedEventArgs> BankStatusChangedEvent;

        /// <summary>
        /// Event which indicates that a request to change denomination has been canceled.
        /// </summary>
        event EventHandler<DenominationChangeCancelledEventArgs> DenominationChangeCancelledEvent;

        /// <summary>
        /// Event handler of DisableAncillaryGameOfferEvent.
        /// </summary>
        event EventHandler<DisableAncillaryGameOfferEventArgs> DisableAncillaryGameOfferEvent;

        /// <summary>
        /// Event handler for the DisplayControlEvent.
        /// </summary>
        event EventHandler<DisplayControlEventArgs> DisplayControlEvent;

        /// <summary>
        /// Event handler of EnrollResponseEvent.
        /// </summary>
        event EventHandler<EnrollResponseEventArgs> EnrollResponseEvent;

        /// <summary>
        /// Event handler of FinalizeOutcomeEvent.
        /// </summary>
        event EventHandler<FinalizeOutcomeEventArgs> FinalizeOutcomeEvent;

        /// <summary>
        /// Event handler of LanguageChangedEvent.
        /// </summary>
        event EventHandler<LanguageChangedEventArgs> LanguageChangedEvent;

        /// <summary>
        /// Event handler of MoneyEvent.
        /// </summary>
        event EventHandler<MoneyEventArgs> MoneyEvent;

        /// <summary>
        /// Event handler of NewThemeContextEvent.
        /// </summary>
        event EventHandler<NewThemeContextEventArgs> NewThemeContextEvent;

        /// <summary>
        /// Event handler of NewThemeSelectionEvent.
        /// </summary>
        /// <remarks>
        /// This event is not a transactional event, therefore,
        /// the handler of this event should not perform
        /// any operations that require an open transaction.
        /// </remarks>>
        event EventHandler<NewThemeSelectionEventArgs> NewThemeSelectionEvent;

        /// <summary>
        /// Event handler of OutcomeResponseEvent.
        /// </summary>
        event EventHandler<OutcomeResponseEventArgs> OutcomeResponseEvent;

        /// <summary>
        /// Event handler of ParkEvent.
        /// </summary>
        /// <remarks>
        /// This event is not a transactional event, therefore,
        /// the handler of this event should not perform
        /// any operations that require an open transaction.
        /// </remarks>>
        event EventHandler<ParkEventArgs> ParkEvent;

        /// <summary>
        /// Event handler of ShutDownEvent.
        /// </summary>
        event EventHandler<ShutDownEventArgs> ShutDownEvent;

        /// <summary>
        /// Event which indicates that the offer-able status of the theme selection menu has changed.
        /// </summary>
        event EventHandler<ThemeSelectionMenuOfferableStatusChangedEventArgs>
            ThemeSelectionMenuOfferableStatusChangedEvent;

        /// <summary>
        /// Event handler of VoucherPrintNotificationEvent.
        /// </summary>
        event EventHandler<VoucherPrintEventArgs> VoucherPrintNotificationEvent;

        /// <summary>
        /// Event handler of ProgressiveBroadcastEvent.
        /// </summary>
        /// <remarks>
        /// This event is not a transactional event, therefore,
        /// the handler of this event should not perform
        /// any operations that require an open transaction.
        /// </remarks>>
        event EventHandler<ProgressiveBroadcastEventArgs> ProgressiveBroadcastEvent;

        /// <summary>
        /// Event to broadcast the progressive data for available denominations
        /// that have progressvies configured.
        /// </summary>
        /// <remarks>
        /// This event is not a transactional event, therefore,  the handler of this event should not
        /// perform any operations that require an open transaction.
        /// </remarks>>
        event EventHandler<DenominationsWithProgressivesBroadcastEventArgs> AvailableDenominationsWithProgressivesBroadcastEvent;

        #endregion

        #region Properties

        /// <summary>
        /// The mount point of the game package.
        /// </summary>
        string GameMountPoint { get; }

        /// <summary>
        /// The mode of the game maintained by the Foundation.
        /// </summary>
        GameMode GameContextMode { get; }

        /// <summary>
        /// The game sub-mode of the current game.
        /// </summary>
        GameSubMode GameSubMode { get; }

        /// <summary>
        /// The name of the paytable.
        /// </summary>
        string PaytableName { get; }

        /// <summary>
        /// The full file path of the paytable.
        /// </summary>
        string PaytableFileName { get; }

        /// <summary>
        /// The current game denomination.
        /// </summary>
        long GameDenomination { get; }

        /// <summary>
        /// Gets the default game denomination.
        /// 0 means that the default game denomination is not supported, in which case, game should NOT
        /// request with default game denomination, otherwise it would crash.
        /// </summary>
        long DefaultGameDenomination { get; }

        /// <summary>
        /// The minimum base game time;
        /// </summary>
        /// <remarks>
        /// Can also be obtained from <see cref="PresentationBehaviorConfigs"/>.
        /// </remarks>
        int MinimumBaseGameTime { get; }

        /// <summary>
        /// The minimum time for a single slot free spin cycle in milliseconds.
        /// </summary>
        /// <remarks>
        /// Can also be obtained from <see cref="PresentationBehaviorConfigs"/>.
        /// </remarks>
        int MinimumFreeSpinTime { get; }

        /// <summary>
        /// This flag indicates whether the player wager is offerable, i.e betting permitted
        /// </summary>
        bool IsPlayerWagerOfferable { get; }

        /// <summary>
        /// This flag indicates whether or not player requested cash outs are currently
        /// being accepted by the Foundation.
        /// </summary>
        bool IsCashOutOfferable { get; }

        /// <summary>
        /// The flag indicates whether or not the player requested transfers of funds
        /// from the Player Bank Meter to the Player Wagerable Meter are currently
        /// accepted by the Foundation.
        /// </summary>
        bool IsBankToWagerableOfferable { get; }

        /// <summary>
        /// Get the language currently configured for the game.
        /// </summary>
        string GameLanguage { get; }

        /// <summary>
        /// Get the jurisdiction string value.
        /// </summary>
        /// <remarks>
        /// IMPORTANT:
        /// 
        /// DO NOT rely on a specific jurisdiction string value to implement a feature,
        /// as the jurisdiction string value is not enumerated, and could change over time.
        /// 
        /// For example, Nevada used to be reported as USDM, but later as 00NV.
        /// 
        /// This API is kept only for the purpose of temporary work-around, when the time-line
        /// of the official support for a feature in Foundation and/or SDK could not meet a game's
        /// specific timetable requirement.  The game should use this jurisdiction string at
        /// its own risks of breaking compatibility with future Foundation and/or SDK.
        /// </remarks>
        string Jurisdiction { get; }

        /// <summary>
        /// This flag indicates whether or not the theme selection menu may be offered. This is 
        /// determined solely on whether multiple games are enabled. The game is responsible for also 
        /// checking the <see cref="GameContextMode"/> and <see cref="GameCycleState"/>.
        /// </summary>
        bool IsThemeSelectionMenuOfferable { get; }

        /// <summary>
        /// Flag indicating if the player initiated auto play is enabled or not for this game.
        /// </summary>
        bool IsPlayerAutoPlayEnabled { get; }

        /// <summary>
        /// Flag indicating if the auto play confirmation is required.
        /// </summary>
        bool IsAutoPlayConfirmationRequired { get; }

        /// <summary>
        /// Gets the flag indicating whether it's allowed to increase the autoplay speed.
        /// If the flag is null the Foundation can not provide the information.
        /// </summary>
        bool? IsAutoPlaySpeedIncreaseAllowed { get; }

        /// <summary>
        /// Gets or sets the wager category information for the current game cycle.
        /// Valid wager category outcome info must be set before the last outcome of main play is adjusted.
        /// </summary>
        /// <remarks>
        /// This information is cleared at the end of each game cycle.
        /// </remarks>
        ICollection<WagerCategoryOutcome> GameCycleWagerCategoryInfo { get; set; }

        /// <summary>
        /// Gets the information on imported extensions linked to the application.
        /// </summary>
        IExtensionImportCollection ExtensionImportCollection { get; }

        /// <summary>
        /// Gets the configuration read object.
        /// </summary>
        IGameConfigurationRead ConfigurationRead { get; }

        /// <summary>
        /// Gets the interface for requesting localization information.
        /// </summary>
        ILocalizationInformation LocalizationInformation { get; }

        #region Foundation Owned Configuration Items

        /// <summary>
        /// Gets the minimum bet amount per game, in base units.
        /// This is the cache of Foundation side configuration "MACHINE WIDE MIN BET PER GAME".
        /// </summary>
        long GameMinBetAmount { get; }

        /// <summary>
        /// The maximum bet for the current game.
        /// Always in units of credit.
        /// </summary>
        long GameMaxBet { get; }

        /// <summary>
        /// The minimum bet for the current game, which is converted from GameMinBetAmount with reference of
        /// current game denomination. Always in units of credit.
        /// </summary>
        long GameMinBet { get; }

        /// <summary>
        /// The minimum bet for configuring the button panel.
        /// Always in units of credit.
        /// 0 if not defined.
        /// </summary>
        long ButtonPanelMinBet { get; }

        /// <summary>
        /// The maximum number of play-steps within each game-cycle in history.
        /// </summary>
        /// <devdoc>
        /// Limitation of the history steps with the MaxHistorySteps is not used any more in the SDK state machine.
        /// Removal of any history step could lead to the situation that history becomes unusable.
        /// There is also no guarantee that any value of MaxHistorySteps would preserve enough space on safe storage
        /// to keep necessary history data. Therefore, the game must take care not to override the available
        /// safe storage size with the created history data.
        /// </devdoc>
        uint MaxHistorySteps { get; }

        /// <summary>
        /// The flag indicating if the ancillary game is enabled for the current game context.
        /// </summary>
        bool AncillaryEnabled { get; }

        /// <summary>
        /// The maximum ancillary game cycles permitted in a single ancillary play.
        /// </summary>
        long AncillaryCycleLimit { get; }

        /// <summary>
        /// The maximum ancillary game win amount allowed in a single ancillary play.
        /// </summary>
        long AncillaryMonetaryLimit { get; }

        /// <summary>
        /// The behavior of the max bet button.
        /// </summary>
        MaxBetButtonBehavior MaxBetButtonBehavior { get; }

        /// <summary>
        /// The configured line selection mode.
        /// </summary>
        LineSelectionMode ConfiguredLineSelectionMode { get; }

        /// <summary>
        /// The flag indicating if the RoundWagerUpPlayoff is enabled for the current game context.
        /// </summary>
        bool RoundWagerUpPlayoffEnabled { get; }

        /// <summary>
        /// The flag indicating if the bonus extension game is enabled for current game.
        /// </summary>
        /// <returns>
        /// True if the bonus extension game is enabled for the current game.
        /// A true value does NOT imply that a bonus play/award is pending,
        /// only that the game-cycle state transitions for extended bonus play
        /// are supported by the current configuration.
        /// It is up to game and/or other mechanisms to decide when bonus play
        /// is warranted for a particular game-cycle.
        /// </returns>
        bool BonusPlayEnabled { get; }

        /// <summary>
        /// The EGM wide top screen advertisement type.
        /// </summary>
        /// <remarks>
        /// Can also be obtained from <see cref="PresentationBehaviorConfigs"/>.
        /// </remarks>
        TopScreenGameAdvertisementType TopScreenGameAdvertisement { get; }

        /// <summary>
        /// Gets the default bet selection style.
        /// </summary>
        /// <remarks>
        /// Can also be obtained from <see cref="GamePlayBehaviorConfigs"/>.
        /// </remarks>
        BetSelectionStyleInfo DefaultBetSelectionStyle { get; }

        /// <summary>
        /// Gets the win cap information.
        /// </summary>
        WinCapInformation WinCapInformation { get; }

        // Items in EgmConfigData (except min bet, win cap info and ancillary limit) are split into two
        // game lib specific interfaces:
        //     * GamePlayBehaviorConfigs
        //     * PresentationBehaviorConfigs.
        // APIs in IGameLib that provide the same information remain as they are for the sake
        // of backwards compatibility.

        /// <summary>
        /// Gets the config data related to game play logic behaviors.
        /// </summary>
        IGamePlayBehaviorConfigs GamePlayBehaviorConfigs { get; }

        /// <summary>
        /// Gets the config data related to game presentation behaviors.
        /// </summary>
        IPresentationBehaviorConfigs PresentationBehaviorConfigs { get; }

        #endregion

        #endregion
    }
}
