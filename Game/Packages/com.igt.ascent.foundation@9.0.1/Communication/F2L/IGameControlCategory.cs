//-----------------------------------------------------------------------
// <copyright file = "IGameControlCategory.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.OutcomeList.Interfaces;
    using F2XTransport;
    using Schemas.Internal;
    using CriticalDataScope = Schemas.Internal.CriticalDataScope;
    using GameCycleState = Schemas.Internal.GameCycleState;
    using MaxBetButtonBehavior = Schemas.Internal.MaxBetButtonBehavior;
    using PlayerMeters = Schemas.Internal.PlayerMeters;

    /// <summary>
    /// Interface which provides the functions available in the game control category of the F2L.
    /// </summary>
    public interface IGameControlCategory
    {
        /// <summary>
        /// Send an action request to the foundation.
        /// </summary>
        /// <param name="payload">Game specific byte array payload.</param>
        /// <returns>True if the request succeeded.</returns>
        bool ActionRequest(byte[] payload);

        #region Betting Functions

        /// <summary>
        /// Check if the specified bet can be committed.
        /// </summary>
        /// <param name="betAmount">Bet amount to test.</param>
        /// <returns>True if the bet can be committed.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        bool CanCommitBet(long betAmount);

        /// <summary>
        /// Check which of the given list of bets can be committed.
        /// </summary>
        /// <param name="bets">The list of bets to check.</param>
        /// <returns>A enumerable of Boolean values in which each Boolean value correlates to a passed bet.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="bets"/> is null.
        /// </exception>
        IEnumerable<bool> CanCommitBets(IEnumerable<long> bets);

        /// <summary>
        /// Commit the given bet.
        /// </summary>
        /// <param name="betAmount">Bet amount to commit.</param>
        /// <returns>True if the bet was committed.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        bool CommitBet(long betAmount);

        /// <summary>
        /// Get the currently committed bet.
        /// </summary>
        /// <returns>The committed bet.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        long GetCommittedBet();

        /// <summary>
        /// Uncommit the committed bet.
        /// </summary>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        void UncommitBet();

        /// <summary>
        /// Place the game starting bet. The amount placed will be the amount already committed.
        /// </summary>
        /// <param name="maxBet">Flag which indicates if the bet being placed is max.</param>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        void PlaceStartingBet(bool maxBet);

        /// <summary>
        /// Check if the given bet can be placed, including betting
        /// from credits and pending wins.
        /// </summary>
        /// <param name="betAmount">The bet to check.</param>
        /// <param name="betFromCredits">The amount to bet from credits.</param>
        /// <param name="betFromPendingWins">The amount to bet from pending wins.</param>
        /// <param name="pendingWinsAvailableForBet">Pending wins which are available to bet from.</param>
        /// <returns>True if the bet could be placed.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        bool CanPlaceBet(long betAmount, long betFromCredits, long betFromPendingWins,
                         long pendingWinsAvailableForBet);

        /// <summary>
        /// Place the specified bet against pending wins.
        /// </summary>
        /// <param name="betAmount">The bet amount to place.</param>
        /// <param name="betFromCredits">The bet to place from credits.</param>
        /// <param name="betFromPendingWins">The bet to place from pending wins.</param>
        /// <param name="pendingWinsAvailableForBet">The pending wins which are available to bet.</param>
        /// <returns>True if the bet was placed.</returns>
        bool PlaceBet(long betAmount, long betFromCredits, long betFromPendingWins,
                      long pendingWinsAvailableForBet);

        /// <summary>
        /// Check which of the given list of bets can be committed in next game cycle. 
        /// </summary>
        /// <param name="bets">The list of bets to check.</param>
        /// <returns>A enumerable of Boolean values in which each Boolean value correlates to a passed bet.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="bets"/> is null.
        /// </exception>
        IEnumerable<bool> CanBetNextGameCycle(IEnumerable<long> bets);

        #endregion

        #region Money Functions

        /// <summary>
        /// Get the current player meters.
        /// </summary>
        /// <returns>The current player meters.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        PlayerMeters GetPlayerMeters();

        /// <summary>
        /// Query the current bank status.
        /// </summary>
        /// <returns>The current bank status.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        BankStatus QueryBankStatus();

        /// <summary>
        /// Query the current player wager offer status.
        /// </summary>
        /// <returns>The current player wager offer status.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        bool IsPlayerWagerOfferable();

        /// <summary>
        /// Query if player cashout is currently offer-able.
        /// </summary>
        /// <returns>The current player cashout offer-able status.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        bool IsPlayerCashoutOfferable();

        /// <summary>
        /// Query if transfer from player bank meter to player wagerable meter is currently offer-able.
        /// </summary>
        /// <returns>The current bank to wagerable meter transfer offer-able status.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        bool IsPlayerBankToWagerableOfferable();

        /// <summary>
        /// Request the player bank meter credits be cashed out.
        /// </summary>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        void PlayerCashoutRequest();

        /// <summary>
        /// Request the entire player wagerable meter amount be transferred to player bank meter.
        /// </summary>
        /// <returns>True if the operation is successful, false otherwise.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        bool TransferWagerableToBankRequest();

        /// <summary>
        /// Request default amount (known to the Foundation) be transferred from the player bank meter to the player wagerable meter.
        /// </summary>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        void TransferBankToWagerableRequest();

        #endregion

        #region Game Cycle Functions

        /// <summary>
        /// Get the current game cycle state.
        /// </summary>
        /// <returns>The current game cycle state.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        GameCycleState QueryGameCycleState();

        /// <summary>
        /// Check if the game cycle can be committed.
        /// </summary>
        /// <returns>True if the game can be committed.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        bool CanCommitGameCycle();

        /// <summary>
        /// Commit the game cycle.
        /// </summary>
        /// <returns>True if the game cycle was committed.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        bool CommitGameCycle();

        /// <summary>
        /// Uncommit the game cycle.
        /// </summary>
        /// <returns>True if the game cycle could be committed.</returns>
        bool UncommitGameCycle();

        /// <summary>
        /// Enroll the game cycle.
        /// </summary>
        /// <param name="enrollmentData">Game/Protocol specific enrollment data.</param>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        void EnrollGameCycle(byte[] enrollmentData);

        /// <summary>
        /// Unenroll the game cycle.
        /// </summary>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        void UnenrollGameCycle();

        /// <summary>
        /// Determine if the game can start playing.
        /// </summary>
        /// <returns>True if the game can be started.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        bool CanStartPlaying();

        /// <summary>
        /// Start playing the game.
        /// </summary>
        /// <returns>True if the game was started.</returns>
        bool StartPlaying();

        /// <summary>
        /// Request the foundation perform any outcome adjustments. When adjusting the final outcome the
        /// <see cref="LastEvalOutcomeRequest"/> or <see cref="LastEvalAncillaryOutcomeRequest"/> method should be
        /// used instead.
        /// </summary>
        /// <param name="outcomeList">The outcome list to adjust.</param>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        void EvalOutcomeRequest(IOutcomeList outcomeList);

        /// <summary>
        /// Request the foundation perform any outcome adjustments for the final outcome.
        /// This method is not used for ancillary play.
        /// </summary>
        /// <param name="outcomeList">The outcome list to adjust.</param>
        /// <param name="wagerCategoryOutcomes">Wager category information for the game.</param>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        void LastEvalOutcomeRequest(IOutcomeList outcomeList,
                                    ReadOnlyCollection<WagerCatOutcome> wagerCategoryOutcomes);

        /// <summary>
        /// Request the foundation perform any outcome adjustments for the final outcome of ancillary play.
        /// </summary>
        /// <param name="outcomeList">The outcome list to adjust.</param>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        void LastEvalAncillaryOutcomeRequest(IOutcomeList outcomeList);

        /// <summary>
        /// Request the foundation to offer an ancillary game.
        /// </summary>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        /// <returns>True if the ancillary game is offered.</returns>
        bool OfferAncillaryGame();

        /// <summary>
        /// Request the foundation to start the ancillary playing.
        /// </summary>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        /// <returns>True if the ancillary game was successfully started.</returns>
        bool StartAncillaryPlaying();

        #region Bonus Play

        /// <summary>
        /// Get flag indicating if the bonus extension game is enabled for current game.
        /// </summary>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        /// <returns>
        /// True if the bonus extension game is enabled for the current game.
        /// A true value does NOT imply that a bonus play/award is pending, only that the game-cycle state transitions
        /// for extended bonus play are supported by the current configuration.  It is up to game and/or other mechanisms
        /// to decide when bonus play is warranted for a particular game-cycle.
        /// </returns>
        bool GetBonusPlayEnabled();

        /// <summary>
        /// Request the foundation to start the bonus extension play.
        /// Transitions the GameCycle state from MainPlayComplete/AncillaryPlayComplete to BonusPlaying.
        /// </summary>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        /// <returns>
        /// True if the bonus extension game was successfully started.
        /// </returns>
        bool StartBonusPlaying();

        /// <summary>
        /// Request the foundation perform any outcome adjustments for the final outcome of bonus extension play.
        /// Must only be used for Bonus Extension play. Transitions the GameCycle state from BonusPlaying to BonusEvaluatePending.
        /// </summary>
        /// <param name="outcomeList">The outcome list to adjust.</param>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        void LastEvalBonusOutcomeRequest(IOutcomeList outcomeList);

        #endregion

        /// <summary>
        /// Finalize the game outcome.
        /// </summary>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        void FinalizeAwardRequest();

        /// <summary>
        /// End the game cycle.
        /// </summary>
        /// <param name="historySteps">Number of history steps stored.</param>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        void EndGameCycle(uint historySteps);

        #endregion

        #region Random Number Functions

        /// <summary>
        /// Get a random number.
        /// </summary>
        /// <param name="low">The inclusive low range of the number.</param>
        /// <param name="high">The inclusive high range of the number.</param>
        /// <param name="rngName">Random number generator name associated with the request.</param>
        /// <returns>Random number within the specified range.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        int GetRandomNumber(int low, int high, string rngName);

        /// <summary>
        /// Get a list of random numbers each with the same range.
        /// </summary>
        /// <param name="count">The number of random numbers to pull.</param>
        /// <param name="low">The inclusive low range of each number.</param>
        /// <param name="high">The inclusive high range of each number.</param>
        /// <param name="prePicked">A list of numbers which have already been picked. Used for limiting duplicates.</param>
        /// <param name="allowedDuplicates">
        ///   Number of duplicates which are allowed. An allowed duplicate count of 0 or 1 indicates that there can only
        ///   be a single instance of any given value.
        /// </param>
        /// <param name="rngName">Random number generator name associated with the request.</param>
        /// <returns>A list of random numbers with the specified ranges.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        IList<int> GetRandomNumbers(uint count, int low, int high, ReadOnlyCollection<int> prePicked, uint allowedDuplicates, string rngName);


        /// <summary>
        /// Get a list of random numbers each with a different range.
        /// </summary>
        /// <param name="count">The number of random numbers to pull.</param>
        /// <param name="lowRanges">A list containing the low range for each number.</param>
        /// <param name="highRanges">A list containing the high range for each number.</param>
        /// <param name="rngName">Random number generator name associated with the request.</param>
        /// <returns>A list of random numbers with the specified ranges.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        IList<int> GetRandomNumbers(uint count, ReadOnlyCollection<int> lowRanges, ReadOnlyCollection<int> highRanges,
                                    string rngName);

        #endregion

        #region Service Request Functions

        /// <summary>
        /// Get a Boolean indicating if the theme selection menu if offerable.
        /// </summary>
        /// <returns>True if the menu if offerable</returns>
        bool GetThemeSelectionMenuOfferable();

        /// <summary>
        /// Request the foundation show the theme selection menu.
        /// </summary>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        void RequestThemeSelectionMenu();

        /// <summary>
        /// Request that the Foundation change the denomination within the current theme.
        /// </summary>
        /// <praram name = "newDenomination">The new denomination to be set.</praram>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        /// <returns>True if the request was accepted.</returns>
        bool RequestDenominationChange(long newDenomination);

        /// <summary>
        /// Gets the denominations available for the player to pick.
        /// </summary>
        /// <returns>List of denominations.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        ICollection<long> GetAvailableDenominations();

        /// <summary>
        /// Get the denominations that are associated with progressives and available for the player to pick.
        /// </summary>
        /// <returns>List of denominations.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        ICollection<long> GetAvailableProgressiveDenominations();

        /// <summary>
        /// Gets the game denomination info which includes the available denomination, available denominations
        /// associated with the progressives.
        /// </summary>
        /// <returns>The game denomination info.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        GameDenominationInfo GetGameDenominationInfo();

        #endregion

        #region Critical Data Functions

        /// <summary>
        /// Write critical data.
        /// </summary>
        /// <param name="scope">The critical data scope to write the data.</param>
        /// <param name="itemName">The name of the data to write.</param>
        /// <param name="data">The data to write.</param>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        void WriteCriticalData(CriticalDataScope scope, string itemName, byte[] data);

        /// <summary>
        /// Read critical data.
        /// </summary>
        /// <param name="scope">The critical data scope to read from.</param>
        /// <param name="itemName">The name of the critical data.</param>
        /// <returns>The read critical data. Empty array if data could not be read.</returns>
        byte[] ReadCriticalData(CriticalDataScope scope, string itemName);

        /// <summary>
        /// Remove an item of critical data.
        /// </summary>
        /// <param name="scope">The scope of the critical data to remove.</param>
        /// <param name="itemName"></param>
        /// <returns>True if the data was removed.</returns>
        bool RemoveCriticalData(CriticalDataScope scope, string itemName);

        #endregion

        #region Custom Configuration Item Functions

        /// <summary>
        /// Get a configuration item containing an enumeration reference.
        /// </summary>
        /// <param name="name">The name of the configuration item.</param>
        /// <param name="scope">The scope of the configuration item.</param>
        /// <param name="values">A list of the valid values of the enumeration.</param>
        /// <returns>True if the item could be read.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        bool GetCustomConfigItemReferencedEnumeration(string name, CustomConfigItemScope scope, IList<string> values);

        /// <summary>
        /// Get a custom configuration item containing an amount.
        /// </summary>
        /// <param name="name">The name of the configuration item.</param>
        /// <param name="scope">The scope of the configuration item.</param>
        /// <param name="value">The value of the configuration item.</param>
        /// <param name="lowRange">The allowed low range of the item.</param>
        /// <param name="highRange">The allowed high range of the item.</param>
        /// <returns>True if the item could be read.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        bool GetCustomConfigItemAmount(string name, CustomConfigItemScope scope, out long value, out long lowRange,
                                       out long highRange);

        /// <summary>
        /// Read a custom configuration item containing a Boolean.
        /// </summary>
        /// <param name="name">The name of the configuration item.</param>
        /// <param name="scope">The scope of the configuration item.</param>
        /// <param name="value">The value of the configuration item.</param>
        /// <returns>True if the item could be read.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        bool GetCustomConfigItemBoolean(string name, CustomConfigItemScope scope, out bool value);

        /// <summary>
        /// Get a configuration item containing an enumeration.
        /// </summary>
        /// <param name="name">The name of the configuration item.</param>
        /// <param name="scope">The scope of the configuration item.</param>
        /// <param name="values">A list of the valid values of the enumeration.</param>
        /// <returns>True if the item could be read.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        bool GetCustomConfigItemEnumeration(string name, CustomConfigItemScope scope, IList<string> values);

        /// <summary>
        /// Get a custom configuration item containing a flag list.
        /// </summary>
        /// <param name="name">The name of the configuration item.</param>
        /// <param name="scope">The scope of the configuration item.</param>
        /// <param name="values">A dictionary of flag names to Boolean values.</param>
        /// <returns>True if the item could be read.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        bool GetCustomConfigItemFlagList(string name, CustomConfigItemScope scope, IDictionary<string, bool> values);

        /// <summary>
        /// Get a custom configuration item containing a float.
        /// </summary>
        /// <param name="name">The name of the configuration item.</param>
        /// <param name="scope">The scope of the configuration item.</param>
        /// <param name="value">The value of the configuration item.</param>
        /// <returns>True if the item could be read.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        bool GetCustomConfigItemFloat(string name, CustomConfigItemScope scope, out float value);

        /// <summary>
        /// Get a custom configuration item containing an long.
        /// </summary>
        /// <param name="name">The name of the configuration item.</param>
        /// <param name="scope">The scope of the configuration item.</param>
        /// <param name="value">The value of the configuration item.</param>
        /// <returns>True if the item could be read.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        bool GetCustomConfigItemInt64(string name, CustomConfigItemScope scope, out long value);

        /// <summary>
        /// Get a custom configuration an enumeration value.
        /// </summary>
        /// <param name="name">The name of the configuration item.</param>
        /// <param name="scope">The scope of the configuration item.</param>
        /// <param name="value">The string value of the selected value.</param>
        /// <returns>True if the item could be read.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        bool GetCustomConfigItem(string name, CustomConfigItemScope scope, out string value);

        /// <summary>
        /// Get a custom configuration item containing an string.
        /// </summary>
        /// <param name="name">The name of the configuration item.</param>
        /// <param name="scope">The scope of the configuration item.</param>
        /// <param name="value">The value of the configuration item.</param>
        /// <returns>True if the item could be read.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        bool GetCustomConfigItemString(string name, CustomConfigItemScope scope, out string value);

        /// <summary>
        /// Get the type of a custom configuration item.
        /// </summary>
        /// <param name="name">The name of the configuration item.</param>
        /// <param name="scope">The scope of the configuration item.</param>
        /// <param name="type">The type of the configuration item.</param>
        /// <returns>True if the item could be read.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        bool GetCustomConfigItemType(string name, CustomConfigItemScope scope, out CustomConfigItemType type);

        /// <summary>
        /// Get the types of all the custom configuration items within a scope.
        /// </summary>
        /// <param name="scope">The scope of the configuration item.</param>
        /// <returns>Dictionary of configuration item names to the type of the item.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        IDictionary<string, CustomConfigItemType> GetCustomConfigItemTypes(CustomConfigItemScope scope);

        #endregion

        #region Foundation Owned Configurations
        // These are the F2L APIs that start with "GetConfigData"

        /// <summary>
        /// Get the currently configured max bet amount.
        /// </summary>
        /// <returns>The maximum bet amount.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        long GetMaxBetAmount();

        /// <summary>
        /// Get the currently configured min bet amount.
        /// </summary>
        /// <returns>The minimum bet amount.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        long GetMinBetAmount();

        /// <summary>
        /// Get the currently configured button panel min bet amount.
        /// </summary>
        /// <returns>The button panel minimum bet amount.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        long GetButtonPanelMinBetAmount();

        /// <summary>
        /// Get the configured win cap limit.
        /// </summary>
        /// <returns>The win cap limit.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        long GetWinCapLimitAmount();

        /// <summary>
        /// Get the minimum base game time.
        /// </summary>
        /// <returns>The minimum base spin time.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        int GetMinimumBaseGameTimeInMs();

        /// <summary>
        /// Get the minimum free spin game time.
        /// </summary>
        /// <returns>The minimum free spin time.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        int GetMinimumFreeSpinTimeInMs();

        /// <summary>
        /// Get the maximum number of history steps.
        /// </summary>
        /// <returns>The maximum number of history steps.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        uint GetHistoryPlaySteps();

        /// <summary>
        /// Get the credit meter behavior.
        /// </summary>
        /// <returns>The credit meter behavior.</returns>
        CreditMeterDisplayBehaviorType GetCreditMeterBehavior();

        /// <summary>
        /// Get the max bet button behavior.
        /// </summary>
        /// <returns>The max bet button behavior.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        MaxBetButtonBehavior GetMaxBetButtonBehavior();

        /// <summary>
        /// Gets the configured line selection.
        /// </summary>
        /// <returns>The configured line selection.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        LineSelectionType GetConfiguredLineSelection();

        #region Ancillary Play

        /// <summary>
        /// Get the maximum cycles within an ancillary game.
        /// </summary>
        /// <returns>The maximum ancillary cycles.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        uint GetAncillaryCycleLimit();

        /// <summary>
        /// Get flag indicating if the ancillary game is enabled for current game.
        /// </summary>
        /// <returns>True if the ancillary game is enabled for current game. Otherwise false.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        bool GetAncillaryGameEnabled();

        /// <summary>
        /// Get the maximum money that an ancillary game could win.
        /// </summary>
        /// <returns>The monetary limit for an ancillary game.</returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        AmountType GetAncillaryMonetaryLimit();

        #endregion

        #endregion

        #region Localization

        /// <summary>
        /// Get the currently configured culture.
        /// </summary>
        /// <returns>
        /// The currently configured culture as a culture code. This code consist of the languageId-RegionId_[Dialect]
        /// an example would be "en-US".
        /// </returns>
        string GetCulture();

        /// <summary>
        /// Get a list of the available cultures.
        /// </summary>
        /// <returns>
        /// A list of cultures which are available. Each code consist of the languageId-RegionId_[Dialect]
        /// an example would be "en-US".
        ///  </returns>
        ICollection<string> GetAvailableCultures();

        /// <summary>
        /// Set the current culture.
        /// </summary>
        /// <param name="culture">
        /// The desired culture. This code consist of the languageId-RegionId_[Dialect] an example would be "en-US".
        /// </param>
        void SetCulture(string culture);

        /// <summary>
        /// Sets the culture to the default. Similar to SetCulture, but does not take a parameter.
        /// Simultaneously applies the default Foundation culture and returns the new (default or fallback) culture.
        /// </summary>
        /// <returns>
        /// The current culture that had been set. null means this API is not supported by the foundation.
        /// </returns>
        string SetDefaultCulture();

        /// <summary>
        /// Get the currently configured credit formatting information.
        /// </summary>
        /// <returns>
        /// A <see cref="CreditFormatterInfoType"/> object containing the credit formatting information.
        /// </returns>
        CreditFormatterInfoType GetCreditFormatting();

        #endregion

        #region Progressive Functions

        /// <summary>
        /// Get progressive information for the specified denominations and the currently active theme.
        /// </summary>
        /// <param name="denominations">List of denominations to get progressive information for.</param>
        /// <returns>
        /// A dictionary of information about the progressives for the specified denominations.
        /// The dictionary is keyed by denomination. If there are no entries for the denomination it will still be
        /// included in the dictionary, but an empty enumerable of progressive levels will be returned.
        /// </returns>
        IDictionary<long, IEnumerable<SystemProgressiveData>> GetProgressiveLevels(IEnumerable<long> denominations);

        /// <summary>
        /// Get progressive information for the specified denomination and the currently active theme.
        /// </summary>
        /// <param name="denomination">Denomination to get progressive information for.</param>
        /// <returns>
        /// An enumerable of information about the progressives for the specified denomination.
        /// If there are no entries for the denomination, an empty enumerable of progressive levels will be returned.
        /// </returns>
        IEnumerable<SystemProgressiveData> GetProgressiveLevels(long denomination);

        #endregion

        #region Axxis Functions

        /// <summary>
        /// Get flag indicating if the RoundWagerUpPlayoff is enabled.
        /// </summary>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        /// <returns>
        /// True if the RoundWagerUpPlayoff is enabled.
        /// </returns>
        bool GetRoundWagerUpPlayoffEnabled();

        /// <summary>
        /// Get WinCapBehavior info from foundation.
        /// </summary>
        /// <returns>
        /// Win cap limit behavior and limit value.
        /// </returns>
        /// <exception cref="FoundationReplyException">
        /// Thrown when an error code is received from the foundation.
        /// </exception>
        WinCapBehaviorInfo GetWinCapBehavior();

        #endregion
    }
}
