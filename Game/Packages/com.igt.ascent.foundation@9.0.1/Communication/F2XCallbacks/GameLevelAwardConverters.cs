// -----------------------------------------------------------------------
// <copyright file = "GameLevelAwardConverters.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.Communication.Platform.ReportLib.Interfaces;
    using F2X.Schemas.Internal.GameLevelAward;
    using F2X.Schemas.Internal.Types;
    using F2XGameLevelLinkedValue = F2X.Schemas.Internal.GameLevelAward.GameLevelLinkedValue;
    using F2XGameLevelLinkedData = F2X.Schemas.Internal.GameLevelAward.GameLevelLinkedData;
    using GameLevelLinkedData = Ascent.Communication.Platform.ReportLib.Interfaces.GameLevelLinkedData;

    /// <summary>
    /// Collection of extension methods helping convert between interface types and F2X schema types.
    /// </summary>
    internal static class GameLevelAwardConverters
    {
        #region Public Methods

        /// <summary>
        /// Extension method to convert a F2X <see cref="ProgressiveLevelLinkedData"/> to a
        /// <see cref="GameLevelLinkedData"/>.
        /// </summary>
        /// <param name="progressiveLevelLinkedData">
        /// The progressive level linked data to convert.
        /// </param>
        /// <returns>The conversion result.</returns>
        public static GameLevelLinkedData ToGameLevelLinkedData(
            this ProgressiveLevelLinkedData progressiveLevelLinkedData)
        {
            if(progressiveLevelLinkedData == null)
            {
                return null;
            }

            if(progressiveLevelLinkedData.ProgressiveLevelLinkedValue == null ||
               progressiveLevelLinkedData.ProgressiveLevelLinkedValue.Item == null)
            {
                return new GameLevelLinkedData(progressiveLevelLinkedData.GameLevel);
            }

            var item = progressiveLevelLinkedData.ProgressiveLevelLinkedValue.Item as ProgressiveLevelValue;
            if(item != null)
            {
                return new GameLevelLinkedData(progressiveLevelLinkedData.GameLevel, item.ToGameLevelLinkUpValue());
            }

            return new GameLevelLinkedData(progressiveLevelLinkedData.GameLevel,
                (progressiveLevelLinkedData.ProgressiveLevelLinkedValue.Item as LinkDownTextLocalizations)
                    .ToGameLevelLinkDownValue());
        }

        /// <summary>
        /// Extension method to convert a F2X <see cref="PayvarDenominations"/> to
        /// a collection of <see cref="PaytableDenominationInfo"/>s.
        /// </summary>
        /// <param name="payvarDenominations">The payvar-denominations to convert.</param>
        /// <returns>The conversion result.</returns>
        public static IEnumerable<PaytableDenominationInfo> ToPayvarDenominationInfos(
            this PayvarDenominations payvarDenominations)
        {
            if(payvarDenominations == null)
            {
                return null;
            }

            return payvarDenominations.Denomination.Select(
                denomination => new PaytableDenominationInfo(payvarDenominations.Payvar.Value, denomination));
        }

        /// <summary>
        /// Extension method to convert the type <see cref="GameLevelLinkedData"/> to 
        /// <see cref="F2XGameLevelLinkedData"/>.
        /// </summary>
        /// <remarks>For internal use.</remarks>
        /// <param name="gameLevelLinkedData">The data to convert.</param>
        /// <returns>The conversion result.</returns>
        public static F2XGameLevelLinkedData ToInternalGameLevelLinkedData(
            this GameLevelLinkedData gameLevelLinkedData)
        {
            if(gameLevelLinkedData == null)
            {
                return null;
            }

            var dataResult = new F2XGameLevelLinkedData
                                 {
                                     GameLevel = gameLevelLinkedData.GameLevelIndex
                                 };

            if(gameLevelLinkedData.IsLinked)
            {
                if(gameLevelLinkedData.IsLinkUp)
                {
                    dataResult.GameLevelLinkedValue
                        = new F2XGameLevelLinkedValue
                              {
                                  Item = gameLevelLinkedData.GameLevelLinkUpValue.ToGameLevelValue()
                              };
                }
                else
                {
                    dataResult.GameLevelLinkedValue
                        = new F2XGameLevelLinkedValue
                              {
                                  Item = gameLevelLinkedData.GameLevelLinkDownValue.ToLinkDownTextLocalizations()
                              };
                }
            }
            else
            {
                dataResult.GameLevelLinkedValue = null;
            }

            return dataResult;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Extension method to convert a F2X <see cref="LinkDownTextLocalizations"/>
        /// to a <see cref="GameLevelLinkDownValue"/>.
        /// </summary>
        /// <param name="linkDownTextLocalizations">The link-down text to convert.</param>
        /// <returns>The conversion result.</returns>
        private static GameLevelLinkDownValue ToGameLevelLinkDownValue(
            this LinkDownTextLocalizations linkDownTextLocalizations)
        {
            var textLocalizations = linkDownTextLocalizations.LinkDownTextLocalization
                .Select(textLocalization => new TextLocalization(textLocalization.Culture,
                                                                 textLocalization.Value))
                .ToList();
            return new GameLevelLinkDownValue(textLocalizations);
        }

        /// <summary>
        /// Extension method to convert a F2X <see cref="ProgressiveLevelValue"/> to a
        /// <see cref="GameLevelLinkUpValue"/>.
        /// </summary>
        /// <param name="progressiveLevelValue">
        /// The progressive level value to convert.
        /// </param>
        /// <returns>The conversion result.</returns>
        private static GameLevelLinkUpValue ToGameLevelLinkUpValue(this ProgressiveLevelValue progressiveLevelValue)
        {
            var amount = progressiveLevelValue.Amount == null ? (long?)null : progressiveLevelValue.Amount.Value;

            if(progressiveLevelValue.Prize == null || !progressiveLevelValue.Prize.Any())
            {
                return new GameLevelLinkUpValue(amount);
            }

            var textLocalizations = progressiveLevelValue.Prize
                .Select(textLocalization => new TextLocalization(textLocalization.Culture,
                                                                 textLocalization.Value))
                .ToList();
            return new GameLevelLinkUpValue(amount, textLocalizations);
        }

        /// <summary>
        /// Extension method to convert a <see cref="TextLocalization"/> to
        /// a F2X <see cref="GameLevelValuePrizeLocalization"/>.
        /// </summary>
        /// <remarks>For internal use.</remarks>
        /// <param name="textLocalization">The text localization to convert.</param>
        /// <returns>The conversion result.</returns>
        private static GameLevelValuePrizeLocalization ToGameLevelValuePrizeLocalization(
            this TextLocalization textLocalization)
        {
            return new GameLevelValuePrizeLocalization
                       {
                           Culture = textLocalization.Culture,
                           Value = textLocalization.Text
                       };
        }

        /// <summary>
        /// Extension method to convert a <see cref="GameLevelLinkUpValue"/> to
        /// a F2X <see cref="GameLevelValue"/>.
        /// </summary>
        /// <remarks>For internal use.</remarks>
        /// <param name="gameLevelLinkUpValue">The linked value text localization to convert.</param>
        /// <returns>The conversion result.</returns>
        private static GameLevelValue ToGameLevelValue(this GameLevelLinkUpValue gameLevelLinkUpValue)
        {
            var amount = !gameLevelLinkUpValue.Amount.HasValue ? null : new Amount(gameLevelLinkUpValue.Amount.Value);

            var prize = gameLevelLinkUpValue.PrizeLocalizations == null || !gameLevelLinkUpValue.PrizeLocalizations.Any()
                ? null
                : gameLevelLinkUpValue.PrizeLocalizations.Select(
                    localization => localization.ToGameLevelValuePrizeLocalization()).ToList();

            return new GameLevelValue
                       {
                           Amount = amount,
                           Prize = prize
                       };
        }

        /// <summary>
        /// Extension method to convert a <see cref="GameLevelLinkDownValue"/> to
        /// a F2X <see cref="LinkDownTextLocalizations"/>.
        /// </summary>
        /// <remarks>For internal use.</remarks>
        /// <param name="gameLevelLinkDownValue">The link-down text value localization to convert.</param>
        /// <returns>The conversion result.</returns>
        private static LinkDownTextLocalizations ToLinkDownTextLocalizations(
            this GameLevelLinkDownValue gameLevelLinkDownValue)
        {
            var linkDownTextLocalizations =
                gameLevelLinkDownValue.LinkDownTextLocalizations.Select(
                    valueLocalization => new LinkDownTextLocalizationsLinkDownTextLocalization
                                         {
                                             Culture = valueLocalization.Culture,
                                             Value = valueLocalization.Text
                                         }).ToList();

            return new LinkDownTextLocalizations
                       {
                           LinkDownTextLocalization = linkDownTextLocalizations
                       };
        }

        #endregion
    }
}