//-----------------------------------------------------------------------
// <copyright file = "InspectionReportConverters.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;
    using F2X.Schemas.Internal.ReportGameDataInspection;
    using GameReport.Interfaces;
    using F2XProgressiveReportItem = F2X.Schemas.Internal.ReportGameDataInspection.ProgressiveReportItem;
    using ProgressiveReportItem = GameReport.Interfaces.ProgressiveReportItem;

    /// <summary>
    /// Collection of extension methods helping convert <see cref="IGameInspectionReport"/> and <see cref="IGameHtmlInspectionReport"/>
    /// implementation members to their F2X versions as needed.
    /// </summary>
    internal static class InspectionReportConverters
    {
        /// <summary>
        /// Mappings from <see cref="StandardReportLabel"/> to <see cref="ReportItemKey"/>.
        /// </summary>
        private static readonly Dictionary<StandardReportLabel, ReportItemKey> MapToReportItemKey =
            new Dictionary<StandardReportLabel, ReportItemKey>
                {
                    { StandardReportLabel.BaseHoldPercent,  ReportItemKey.BaseHoldPercent },
                    { StandardReportLabel.BaseRtpPercent,   ReportItemKey.BaseRTPPercent },
                    { StandardReportLabel.GameDescription,  ReportItemKey.GameDescription },
                    { StandardReportLabel.JackpotRtp,       ReportItemKey.JackpotRTP },
                    { StandardReportLabel.LinkSeriesModel,  ReportItemKey.LinkSeriesModel },
                    { StandardReportLabel.MaxLines,         ReportItemKey.MaxLines },
                    { StandardReportLabel.MaxWays,          ReportItemKey.MaxWays },
                    { StandardReportLabel.MaxWinAmount,     ReportItemKey.MaxWinAmount },
                    { StandardReportLabel.MaxWinCredits,    ReportItemKey.MaxWinCredits },
                    { StandardReportLabel.MinBetAmount,     ReportItemKey.MinBetAmount },
                    { StandardReportLabel.MinBetCredits,    ReportItemKey.MinBetCredits },
                    { StandardReportLabel.MinLines,         ReportItemKey.MinLines },
                    { StandardReportLabel.MinWays,          ReportItemKey.MinWays },
                    { StandardReportLabel.TotalHoldPercent, ReportItemKey.TotalHoldPercent },
                    { StandardReportLabel.TotalRtpPercent,  ReportItemKey.TotalRTPPercent },
                };

        /// <summary>
        /// Mappings from <see cref="ProgressiveReportLabel"/> to <see cref="ProgressiveReportItemKey"/>.
        /// </summary>
        private static readonly Dictionary<ProgressiveReportLabel, ProgressiveReportItemKey> MapToProgressiveReportItemKey =
            new Dictionary<ProgressiveReportLabel, ProgressiveReportItemKey>
                {
                    { ProgressiveReportLabel.GameLevelDescription,   ProgressiveReportItemKey.GameLevelDescription },
                    { ProgressiveReportLabel.ContributionPercentage, ProgressiveReportItemKey.ContributionPercentage },
                    { ProgressiveReportLabel.LevelTotalRtp,          ProgressiveReportItemKey.LevelTotalRTP },
                    { ProgressiveReportLabel.LinkStatus,             ProgressiveReportItemKey.LinkStatus },
                    { ProgressiveReportLabel.MaxAmount,              ProgressiveReportItemKey.MaxAmount },
                    { ProgressiveReportLabel.StartAmount,            ProgressiveReportItemKey.StartAmount },
                    { ProgressiveReportLabel.StartPercent,           ProgressiveReportItemKey.StartPercent },
                };

        /// <summary>
        /// Extension method to convert a <see cref="StandardReportLabel"/> to
        /// a F2X <see cref="ReportItemKey"/>.
        /// </summary>
        /// <param name="label">The label to convert.</param>
        /// <returns>The conversion result.</returns>
        public static ReportItemKey ToReportItemKey(this StandardReportLabel label)
        {
            return MapToReportItemKey[label];
        }

        /// <summary>
        /// Extension method to convert a <see cref="ProgressiveReportLabel"/> to
        /// a F2X <see cref="ProgressiveReportItemKey"/>.
        /// </summary>
        /// <param name="label">The label to convert.</param>
        /// <returns>The conversion result.</returns>
        public static ProgressiveReportItemKey ToProgressiveReportItemKey(this ProgressiveReportLabel label)
        {
            return MapToProgressiveReportItemKey[label];
        }

        /// <summary>
        /// Extension method to convert a <see cref="StandardReportItem"/> to
        /// a F2X <see cref="ReportItem"/>.
        /// </summary>
        /// <param name="reportItem">The standard report item to convert.</param>
        /// <returns>The conversion result.</returns>
        public static ReportItem ToReportItem(this StandardReportItem reportItem)
        {
            return reportItem == null
                       ? null
                       : new ReportItem
                             {
                                 Item = reportItem.Label.ToReportItemKey(),
                                 Value = reportItem.Value
                             };
        }

        /// <summary>
        /// Extension method to convert a <see cref="CustomReportItem"/> to
        /// a F2X <see cref="ReportItem"/>.
        /// </summary>
        /// <param name="reportItem">The custom report item to convert.</param>
        /// <returns>The conversion result.</returns>
        public static ReportItem ToReportItem(this CustomReportItem reportItem)
        {
            return reportItem == null
                       ? null
                       : new ReportItem
                             {
                                 Item = reportItem.Label,
                                 Value = reportItem.Value
                             };
        }

        /// <summary>
        /// Extension method to convert a <see cref="ProgressiveReportItem"/> to
        /// a F2X <see cref="F2XProgressiveReportItem"/>.
        /// </summary>
        /// <param name="reportItem">The progressive report item to convert.</param>
        /// <returns>The conversion result.</returns>
        public static F2XProgressiveReportItem ToProgressiveReportItem(this ProgressiveReportItem reportItem)
        {
            return reportItem == null
                       ? null
                       : new F2XProgressiveReportItem
                             {
                                 Item = reportItem.Label.ToProgressiveReportItemKey(),
                                 Value = reportItem.Value
                             };
        }

        /// <summary>
        /// Extension method to convert a <see cref="CustomReportItem"/> to
        /// a F2X <see cref="F2XProgressiveReportItem"/>.
        /// </summary>
        /// <param name="reportItem">The custom report item to convert.</param>
        /// <returns>The conversion result.</returns>
        public static F2XProgressiveReportItem ToProgressiveReportItem(this CustomReportItem reportItem)
        {
            return reportItem == null
                       ? null
                       : new F2XProgressiveReportItem
                             {
                                 Item = reportItem.Label,
                                 Value = reportItem.Value
                             };
        }

        /// <summary>
        /// Extension method to convert a <see cref="StandardReportPart"/> to
        /// a list of F2X <see cref="ReportItem"/>.
        /// </summary>
        /// <param name="reportPart">The standard report part to convert.</param>
        /// <returns>The conversion result.</returns>
        public static List<ReportItem> ToReportItems(this StandardReportPart reportPart)
        {
            if(reportPart == null)
            {
                return null;
            }

            var reportItems = reportPart.DefinedReportItems
                                        .Select(item => item.ToReportItem())
                                        .ToList();

            reportItems.AddRange(reportPart.CustomReportItems
                                           .Select(item => item.ToReportItem()));

            return reportItems;
        }

        /// <summary>
        /// Extension method to convert a <see cref="ProgressiveLevelReportPart"/> to
        /// a F2X <see cref="ProgressiveGameLevelData"/>.
        /// </summary>
        /// <param name="reportPart">The progressive report part to convert.</param>
        /// <returns>The conversion result.</returns>
        public static ProgressiveGameLevelData ToProgressiveGameLevelData(this ProgressiveLevelReportPart reportPart)
        {
            if(reportPart == null)
            {
                return null;
            }

            var progressiveReportItems = reportPart.DefinedReportItems
                                                   .Select(item => item.ToProgressiveReportItem())
                                                   .ToList();

            progressiveReportItems.AddRange(reportPart.CustomReportItems
                                                      .Select(item => item.ToProgressiveReportItem()));

            return new ProgressiveGameLevelData
                       {
                           GameLevel = (uint)reportPart.ProgressiveLevel,
                           ProgressiveReportItems = progressiveReportItems
                       };
        }

        /// <summary>
        /// Extension method to convert a collection of F2X <see cref="ThemeInfo"/> objects to a dictionary of theme names vs. a collection of
        /// <see cref="PaytableDenominationInfo"/>.
        /// </summary>
        /// <param name="f2XThemeInfoList">
        /// The internal F2X type to convert.
        /// </param>
        /// <returns>The public game facing conversion result.</returns>
        public static IDictionary<string, IList<PaytableDenominationInfo>> ToThemeVsPaytableDenomDictionary(this IEnumerable<ThemeInfo> f2XThemeInfoList)
        {
            var fxThemeInfoList = f2XThemeInfoList.ToList();
            if(!fxThemeInfoList.Any())
            {
                return null;
            }

            var result = new Dictionary<string, IList<PaytableDenominationInfo>>();

            foreach(var f2XThemeInfo in fxThemeInfoList)
            {
                if(f2XThemeInfo.ThemeIdentifier != null && f2XThemeInfo.ThemeIdentifier.Value != null)
                {
                    var paytableDenomList = new List<PaytableDenominationInfo>();
                    foreach(var f2XPayvarInfo in f2XThemeInfo.PayvarInfoList)
                    {
                        var payvarIdentifier = f2XPayvarInfo.PayvarIdentifier.Value ?? string.Empty;

                        foreach(var f2XDenom in f2XPayvarInfo.DenominationList)
                        {
                            paytableDenomList.Add(new PaytableDenominationInfo(payvarIdentifier, f2XDenom));
                        }
                    }

                    result[f2XThemeInfo.ThemeIdentifier.Value] = paytableDenomList;
                }
            }

            return result;
        }
    }
}