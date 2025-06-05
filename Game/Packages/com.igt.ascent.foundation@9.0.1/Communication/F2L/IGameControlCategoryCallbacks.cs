//-----------------------------------------------------------------------
// <copyright file = "IGameControlCategoryCallbacks.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L
{
    using Schemas;
    using System.Collections.Generic;
    using Ascent.OutcomeList.Interfaces;
    using Schemas.Internal;

    /// <summary>
    /// Interface to accept callbacks from the game control category.
    /// </summary>
    public interface IGameControlCategoryCallbacks
    {
        /// <summary>
        /// Method called when an Activate Theme Context message is received from the foundation.
        /// </summary>
        void ProcessActivateThemeContext();

        /// <summary>
        /// Method called when an Inactivate Theme Context message is received from the foundation.
        /// </summary>
        void ProcessInactivateThemeContext();

        /// <summary>
        /// Method called when a New Theme Context message is received from the foundation.
        /// </summary>
        /// <param name="gameMode">The game mode of the context.</param>
        /// <param name="gameSubMode">The game sub-mode of the context.</param>
        /// <param name="denomination">The denomination for the context.</param>
        /// <param name="paytable">The paytable for the context.</param>
        /// <param name="paytableFile">The paytable file for the context.</param>
        /// <param name="newlySelectedForPlay">Flag indicating that the new (play mode) context is a context that 
        /// should be considered new to the player.</param>
        void ProcessNewThemeContext(GameMode gameMode, GameSubMode gameSubMode, uint denomination,
                                    string paytable, string paytableFile, bool newlySelectedForPlay);

        /// <summary>
        /// Method called when a Switch Theme Context message is received from the foundation.
        /// </summary>
        /// <param name="themeTag">Tag defined by the creator - opaque to Foundation.</param>
        /// <param name="themeTagDataFile">Tag data file - opaque to Foundation.</param>
        /// <param name="resourcePaths">Set of theme resource paths (e.g. inclusion type "Theme").</param>
        /// <param name="denom">Denomination for this context.</param>
        /// <param name="payvarTag">Tag defined by the payvar creator - opaque to the Foundation.</param>
        /// <param name="payvarTagDataFile">Data file for the payvar - opaque to the Foundation.</param>
        void ProcessSwitchThemeContext(string themeTag, string themeTagDataFile,
                                       IEnumerable<GameControlSwitchThemeContextSendResourcePaths> resourcePaths,
                                       uint denom, string payvarTag, string payvarTagDataFile);

        /// <summary>
        /// Method called when an action response is received from the foundation.
        /// </summary>
        /// <param name="payload">Byte array payload which was sent in the request.</param>
        void ProcessActionResponse(byte[] payload);

        /// <summary>
        /// Method called when an Enroll Response is received from the foundation.
        /// </summary>
        /// <param name="enrollSuccess">Flag indicating if enrollment was successful.</param>
        /// <param name="enrollmentData">Protocol specific enrollment data from the host.</param>
        void ProcessEnrollResponse(bool enrollSuccess, byte[] enrollmentData);

        /// <summary>
        /// Method called when an adjusted outcome is received from the foundation.
        /// </summary>
        /// <param name="outcomeList">Adjusted outcome list.</param>
        /// <param name="isFinal">Boolean which indicates if this is the last outcome of the game cycle.</param>
        void ProcessEvalOutcomeResponse(IOutcomeList outcomeList, bool isFinal);

        /// <summary>
        /// Method called when a finalize award message is received from the foundation.
        /// </summary>
        void ProcessFinalizeAwardResponse();

        /// <summary>
        /// Method called when a Set Display Control State message is received from the foundation.
        /// </summary>
        /// <param name="controlState">The current control state.</param>
        void ProcessSetDisplayControlState(DisplayControlState controlState);

        /// <summary>
        /// Method called when a money wagerable message is received from the foundation.
        /// </summary>
        /// <param name="amountMoved">The amount of money moved.</param>
        /// <param name="wagerableDirection">The direction og the transfer.</param>
        /// <param name="playerMeters">The player meters after the transfer.</param>
        void ProcessMoneyWagerable(long amountMoved, string wagerableDirection, PlayerMeters playerMeters);

        /// <summary>
        /// Method called when a money bet message is received from the foundation.
        /// </summary>
        /// <param name="amountBet">The amount bet.</param>
        /// <param name="playerMeters">The player meters after the bet.</param>
        void ProcessMoneyBet(long amountBet, PlayerMeters playerMeters);

        /// <summary>
        /// Method called when a money committed changed message is received from the foundation.
        /// </summary>
        /// <param name="committedBet">The current committed bet amount.</param>
        /// <param name="playerMeters">The player meters after the commit.</param>
        void ProcessMoneyCommittedChanged(long committedBet, PlayerMeters playerMeters);

        /// <summary>
        /// Method called when a money in message is received from the foundation.
        /// </summary>
        /// <param name="amountIn">The amount of money added.</param>
        /// <param name="moneyInSource">The source of the added money.</param>
        /// <param name="fundsTransferAccountSource">
        /// Funds transfer account source of the received money. This information is sent when the MoneyInSource is "FundsTransfer". In all other cases this value is not sent (null).
        /// </param>
        /// <param name="playerMeters">The player meters after the money was added.</param>
        void ProcessMoneyIn(long amountIn,
                            MoneyInSource moneyInSource,
                            FundsTransferAccountSource? fundsTransferAccountSource,
                            PlayerMeters playerMeters);

        /// <summary>
        /// Method called when a money out message is received from the foundation.
        /// </summary>
        /// <param name="amountOut">The amount of money which was removed.</param>
        /// <param name="moneyOutSource">The source of the removed money.</param>
        /// <param name="playerMeters">The player meters after the funds were removed.</param>
        void ProcessMoneyOut(long amountOut, MoneyOutSource moneyOutSource, PlayerMeters playerMeters);

        /// <summary>
        /// Method called when a money won message is received from the foundation.
        /// </summary>
        /// <param name="amountWon">The amount of money won.</param>
        /// <param name="playerMeters">The player meters after the win.</param>
        void ProcessMoneyWon(long amountWon, PlayerMeters playerMeters);

        /// <summary>
        /// Method called when a money set message is received from the foundation.
        /// </summary>
        /// <param name="playerMeters">The current player meters.</param>
        void ProcessMoneySet(PlayerMeters playerMeters);

        /// <summary>
        /// Method called when a bank status changed message is received from the foundation.
        /// </summary>
        /// <param name="bankStatus">The new bank status.</param>
        void ProcessBankStatusChanged(BankStatus bankStatus);

        /// <summary>
        /// Method called when there is a change in the availability of the theme selection menu.
        /// </summary>
        /// <param name="offerable">True if the menu can be offered.</param>
        void ProcessThemeSelectionMenuOfferableStatusChanged(bool offerable);

        /// <summary>
        /// Method called when the foundation disables the ancillary game offer.
        /// </summary>
        void ProcessDisableAncillaryGameOffer();

        /// <summary>
        /// Method called when the culture changes.
        /// </summary>
        /// <param name="culture">The current culture.</param>
        void ProcessCultureChanged(string culture);

        /// <summary>
        /// Method called when a denomination change has been cancelled.
        /// </summary>
        void ProcessDenominationChangeCancelled();
    }
}
