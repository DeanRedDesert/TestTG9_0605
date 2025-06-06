//-----------------------------------------------------------------------
// <copyright file = "IEgmConfigDataCategory.cs" company = "IGT">
//     Copyright (c) 2022 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// <auto-generated>
//     This code was generated by C3G.
//
//     Changes to this file may cause incorrect behavior
//     and will be lost if the code is regenerated.
// </auto-generated>
//-----------------------------------------------------------------------
//-----------------------------------------------------------------------
// This file requires manual editing when merging.
// All changes are marked with "MANUAL EDIT:"
//-----------------------------------------------------------------------
// MANUAL_EDIT:
// 1. Remove GetWinCap() method.
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2X
{
    using System;
    using Schemas.Internal.EGMConfigData;
    using Schemas.Internal.Types;

    /// <summary>
    /// EGM config data category of messages. This category is used to request information regarding the EGM
    /// configuration.
    /// Category: 110; Major Version: 1
    /// </summary>
    /// <remarks>
    /// All documentation is generated from the XSD schema files.
    /// </remarks>
    public interface IEgmConfigDataCategory
    {
        /// <summary>
        /// A request for the EGM wide monetary limit on ancillary games.
        /// </summary>
        /// <returns>
        /// The content of the GetAncillaryMonetaryLimitReply message.
        /// </returns>
        Amount GetAncillaryMonetaryLimit();

        /// <summary>
        /// A request to get the default bet selection style.
        /// </summary>
        /// <returns>
        /// The content of the GetConfigDataDefaultBetSelectionStyleReply message.
        /// </returns>
        DefaultBetSelectionStyle GetConfigDataDefaultBetSelectionStyle();

        /// <summary>
        /// Get the requirement of whether a video reels presentation should be displayed for a stepper game.
        /// </summary>
        /// <returns>
        /// The content of the GetConfigDataDisplayVideoReelsForStepperReply message.
        /// </returns>
        bool GetConfigDataDisplayVideoReelsForStepper();

        /// <summary>
        /// A request for the EGM wide external bonus win cap.
        /// </summary>
        /// <returns>
        /// The content of the GetConfigDataExternalBonusWinCapReply message.
        /// </returns>
        Amount GetConfigDataExternalBonusWinCap();

        /// <summary>
        /// A request for The Game to get the GameFeatureSingleOptionAutoAdvance Settings .
        /// </summary>
        /// <returns>
        /// The content of GetConfigDataGameFeatureSingleOptionAutoAdvanceSettingsReply.
        /// It has two elements that says whether GameFeatureSingleOptionAutoAdvance is allowed or not and the time
        /// period that the Game has to wait before it gets triggered.
        /// </returns>
        GetConfigDataGameFeatureSingleOptionAutoAdvanceSettingsReplyContent GetConfigDataGameFeatureSingleOptionAutoAdvanceSettings();

        /// <summary>
        /// A request for the EGM wide progressive win cap.
        /// </summary>
        /// <returns>
        /// The content of the GetConfigDataProgressiveWinCapReply message.
        /// </returns>
        Amount GetConfigDataProgressiveWinCap();

        /// <summary>
        /// A request to get the requirement of whether higher total bets must return a higher RTP than a lesser bet.
        /// </summary>
        /// <returns>
        /// The content of the GetConfigDataRtpOrderedByBetRequiredReply message.
        /// </returns>
        bool GetConfigDataRtpOrderedByBetRequired();

        /// <summary>
        /// A request for the EGM wide total win cap.
        /// </summary>
        /// <returns>
        /// The content of the GetConfigDataTotalWinCapReply message.
        /// </returns>
        Amount GetConfigDataTotalWinCap();

        /// <summary>
        /// A request for the EGM wide marketing behavior.
        /// </summary>
        /// <returns>
        /// The content of the GetMarketingBehaviorReply message.
        /// </returns>
        TopScreenGameAdvertisement GetMarketingBehavior();

        /// <summary>
        /// A request for the EGM wide minimum bet amount.
        /// </summary>
        /// <returns>
        /// The content of the GetMinimumBetReply message.
        /// </returns>
        Amount GetMinimumBet();

        /// <summary>
        /// A request for the EGM wide win cap behavior.
        /// </summary>
        /// <returns>
        /// The content of the response.  This element should be omitted when an exception is present.
        /// </returns>
        GetWinCapBehaviorReplyContent GetWinCapBehavior();

    }

}

