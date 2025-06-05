//-----------------------------------------------------------------------
// <copyright file = "IEgmConfigData.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    /// <summary>
    /// This interface defines methods to request EGM-wide data
    /// from the Foundation.
    /// </summary>
    public interface IEgmConfigData
    {
        /// <summary>
        /// Gets the EGM-wide minimum bet amount.
        /// </summary>
        /// <returns>Minimum bet amount in base units.</returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        long GetMinimumBet();

        /// <summary>
        /// Gets the EGM-wide monetary limit on ancillary games.
        /// </summary>
        /// <returns>Monetary limit in base units on ancillary games.</returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        long GetAncillaryMonetaryLimit();

        /// <summary>
        /// Gets the EGM-wide win cap behavior.
        /// </summary>
        /// <returns>The information of Win Cap Behavior.</returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <remarks>
        /// There is no assumption on the relationship between game/progressive/total win capping value.
        /// It would be up to the game to decide which win (progressive or game) would need to be scaled
        /// to make sure none of these three win capping values is exceeded.
        /// </remarks>
        WinCapBehaviorInfo GetWinCapBehaviorInfo();

        /// <summary>
        /// Gets the EGM wide marketing behavior.
        /// </summary>
        /// <returns>EGM wide setting for the marketing behavior.</returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        MarketingBehavior GetMarketingBehavior();

        /// <summary>
        /// Gets the default bet selection style.
        /// </summary>
        /// <returns>The default bet selection style.</returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        BetSelectionStyleInfo GetDefaultBetSelectionStyle();

        /// <summary>
        /// Gets the standalone or external progressive win cap limit for this game.
        /// </summary>
        /// <returns>
        /// Returns progressive win cap amount in base units, and 0 indicates the jurisdiction does not 
        /// support it.
        /// </returns>
        /// <remarks>
        /// There is no assumption on the relationship between game/progressive/total win capping value.
        /// It would be up to the game to decide which win (progressive or game) would need to be scaled
        /// to make sure none of these three win capping values is exceeded.
        /// </remarks>
        long GetProgressiveWinCap();

        /// <summary>
        /// Gets the total win cap amount for this game, including paytable and progressive win amounts.
        /// </summary>
        /// <returns>
        /// Returns total win cap amount in base units, and 0 indicates the jurisdiction does not
        /// support it.
        /// </returns>
        /// <remarks>
        /// There is no assumption on the relationship between game/progressive/total win capping value.
        /// It would be up to the game to decide which win (progressive or game) would need to be scaled
        /// to make sure none of these three win capping values is exceeded.
        /// </remarks>
        long GetTotalWinCap();

        /// <summary>
        /// Gets the requirement of whether a video reels presentation should be displayed for a stepper game.
        /// </summary>
        /// <returns>
        /// Returns true if the video reels presentation should be displayed. Otherwise, false.
        /// </returns>
        bool GetDisplayVideoReelsForStepper();

        /// <summary>
        /// Gets the settings for the Single Option Auto Advance (SOAA) feature in a Bonus.
        /// </summary>
        /// <returns>
        /// The settings for the Single Option Auto Advance (SOAA) feature in a Bonus.
        /// Null if no settings is provided by the Foundation.
        /// </returns>
        BonusSoaaSettings GetBonusSoaaSettings();

        /// <summary>
        /// Gets the requirement of whether higher total bets must return a higher RTP than a lesser bet.
        /// </summary>
        /// <returns>
        /// True if higher total bets must return a higher RTP than a lesser bet. False otherwise.
        /// </returns>
        bool GetRtpOrderedByBetRequired();
    }
}
