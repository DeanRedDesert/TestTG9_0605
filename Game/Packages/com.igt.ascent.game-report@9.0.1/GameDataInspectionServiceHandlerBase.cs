// -----------------------------------------------------------------------
// <copyright file = "GameDataInspectionServiceHandlerBase.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.ReportLib.Interfaces;
    using Communication.Foundation;
    using Interfaces;
    using Logic.PaytableLoader.Interfaces;
    using Money;
    using ProgressiveReportItem = Interfaces.ProgressiveReportItem;

    /// <summary>
    /// The implementation of <see cref="IGameDataInspectionServiceHandler"/>.
    /// </summary>
    /// <remarks>
    /// Custom game report needs to implement this abstract class if it will
    /// support the <see cref="ReportingServiceType.GameDataInspection"/>.
    /// </remarks>
    public abstract class GameDataInspectionServiceHandlerBase : IGameDataInspectionServiceHandler
    {
        /// <summary>
        /// The default precision used for standard report percentage items.
        /// </summary>
        private const int DefaultStandardPercentPrecision = 2;

        /// <summary>
        /// The precision used for progressive report percentage items.
        /// </summary>
        private const int ProgressivePercentPrecision = 2;

        /// <summary>
        /// The precision used for progressive contribution percentage items.
        /// </summary>
        private const int ContributionPercentPrecision = 3;

        /// <summary>
        /// Identifies that a report item should state to check the link controller instead of a specific value.
        /// </summary>
        public const int CheckLinkController = -1;

        /// <summary>
        /// Identifies the string to use for report items stating to check the link controller.
        /// </summary>
        private const string CheckLinkControllerText = "Check Link Controller";

        /// <summary>
        /// Gets and sets the interface of report lib.
        /// </summary>
        protected IReportLib ReportLib { get; }

        /// <summary>
        /// Gets/sets the paytable loader used to load format specific paytable data.
        /// </summary>
        protected IPaytableLoader PaytableLoader { get; }

        #region Abstract/Virtual Members

        /// <summary>
        /// Gets the number of decimal places to display for standard report item percentages.
        /// Derived classes may override this property to use a percent precision other than 2.
        /// </summary>
        protected virtual int StandardPercentPrecision => DefaultStandardPercentPrecision;

        /// <summary>
        /// When implemented in a derived class, creates the <see cref="IStandardReportSection"/>
        /// which provides report data for the standard report section.
        /// </summary>
        /// <param name="context">The current <see cref="ReportContext"/>.</param>
        /// <returns>A game-specific <see cref="IStandardReportSection"/>.</returns>
        protected abstract IStandardReportSection CreateStandardReportSection(ReportContext context);

        /// <summary>
        /// When implemented in a derived class, creates the <see cref="IProgressiveReportSection"/>
        /// which provides report data for the progressive report section.
        /// </summary>
        /// <param name="context">The current <see cref="ReportContext"/>.</param>
        /// <returns>If a game has progressive levels, a game-specific 
        /// <see cref="IProgressiveReportSection"/>; otherwise, null.</returns>
        protected abstract IProgressiveReportSection CreateProgressiveReportSection(ReportContext context);

        /// <summary>
        /// Gets the minimum playable credit balance, in base units, for the given report context.
        /// </summary>
        /// <remarks>
        /// Default implementation is to return the multiplication of the denomination by the min bet credits returned
        /// by <see cref="CreateStandardReportSection"/>, i.e. <see cref="IStandardReportSection.GetMinBetCredits"/>.
        /// 
        /// If desired, derived classes could override this method to return other appropriate value.
        /// </remarks>
        /// <param name="context">The current <see cref="ReportContext"/>.</param>
        /// <returns>The minimum playable credit balance, in base units.</returns>
        protected virtual long GetMinPlayableCreditBalance(ReportContext context)
        {
            checked
            {
                // Default implementation is to return the value based on the standard report section.
                return CreateStandardReportSection(context).GetMinBetCredits() * context.Denomination;
            }
        }

        /// <inheritdoc />
        public abstract void CleanUpResources(IReportLib reportLib);

        #endregion

        /// <summary>
        /// Constructs the service handler with <paramref name="reportLib"/>.
        /// </summary>
        /// <param name="reportLib">
        /// The game report interface to the Foundation.
        /// </param>
        /// <param name="paytableLoader">
        /// The paytable loader interface that loads a specific type of paytable file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="reportLib"/> is null.
        /// </exception>
        protected GameDataInspectionServiceHandlerBase(IReportLib reportLib, IPaytableLoader paytableLoader)
        {
            ReportLib = reportLib ?? throw new ArgumentNullException(nameof(reportLib));
            PaytableLoader = paytableLoader ?? throw new ArgumentNullException(nameof(paytableLoader));
        }

        #region IGameDataInspectionServiceHandler Members

        /// <inheritdoc/>
        public IGameInspectionReport GenerateInspectionReport(string themeIdentifier,
                                                              string paytableIdentifier,
                                                              long denomination,
                                                              string culture)
        {
            var paytableTag = ReportLib.GameInformation.GetPaytableTag(themeIdentifier, paytableIdentifier);

            var maxBet = ReportLib.GameInformation.GetMaxBet(themeIdentifier, paytableIdentifier, denomination);
            var creditFormatter = ReportLib.LocalizationInformation.GetCreditFormatter();

            var paytableLoadResult = PaytableLoader.LoadPaytable(paytableTag.PaytableFileName, paytableTag.PaytableName);
            var genericPaytableData = paytableLoadResult?.GenericPaytableData;

            var context = new ReportContext(culture,
                                            denomination,
                                            themeIdentifier,
                                            paytableTag,
                                            maxBet,
                                            creditFormatter,
                                            genericPaytableData);

            var progressiveLevelParts = GenerateProgressiveSection(context).ToList();
            var standardReportPart = GenerateStandardSection(context, progressiveLevelParts);

            var inspectionReport = new GameInspectionReport(standardReportPart, progressiveLevelParts);

            return inspectionReport;
        }

        /// <inheritdoc/>
        public void GetMinPlayableCreditBalances(IList<MinPlayableCreditBalanceRequest> requests)
        {
            foreach(var request in requests)
            {
                var paytableTag = ReportLib.GameInformation.GetPaytableTag(request.ThemeIdentifier, request.PaytableIdentifier);

                var maxBet = ReportLib.GameInformation.GetMaxBet(request.ThemeIdentifier, request.PaytableIdentifier, request.Denomination);

                var paytableLoadResult = PaytableLoader.LoadPaytable(paytableTag.PaytableFileName, paytableTag.PaytableName);
                var genericPaytableData = paytableLoadResult?.GenericPaytableData;

                var context = new ReportContext("en-US",
                                                request.Denomination,
                                                request.ThemeIdentifier,
                                                paytableTag,
                                                maxBet,
                                                CreditFormatter.DefaultUS,
                                                genericPaytableData);

                // Sets the output field in the request.
                request.MinPlayableCreditBalance = GetMinPlayableCreditBalance(context);
            }
        }

        #endregion

        #region Progressive Report Section

        /// <summary>
        /// Generates the progressive report section.
        /// </summary>
        /// <param name="context">The current <see cref="ReportContext"/>.</param>
        /// <returns>
        /// A collection of <see cref="ProgressiveLevelReportPart"/>s populated with report data.
        /// </returns>
        private IEnumerable<ProgressiveLevelReportPart> GenerateProgressiveSection(ReportContext context)
        {
            var progressiveSection = CreateProgressiveReportSection(context);

            if(progressiveSection != null)
            {
                foreach(var progressiveLevel in progressiveSection.ProgressiveLevels)
                {
                    if(progressiveLevel == null)
                    {
                        throw new NullReferenceException("A Progressive Level Data reference cannot be null.");
                    }

                    // Calculate Level Total RTP from Start and Contribution percentages.
                    var startPercent = GetRoundedValue(progressiveLevel.GetStartPercent(),
                                                       ProgressivePercentPrecision);

                    var contributionPercent = GetRoundedValue(progressiveLevel.GetContributionPercentage(),
                                                              ContributionPercentPrecision);

                    var levelTotalRtp = CalculateLevelTotalRtp(startPercent, contributionPercent);

                    var progressiveLevelPart = new ProgressiveLevelReportPart(progressiveLevel.ProgressiveLevel,
                                                                              levelTotalRtp);

                    // Add progressive report items.
                    progressiveLevelPart.AddReportItem(
                        new ProgressiveReportItem(ProgressiveReportLabel.GameLevelDescription,
                                                  progressiveLevel.GetGameLevelDescription()));

                    progressiveLevelPart.AddReportItem(
                        new ProgressiveReportItem(ProgressiveReportLabel.StartAmount,
                                                  GetCurrencyString(progressiveLevel.GetStartAmount(), context)));

                    progressiveLevelPart.AddReportItem(
                        new ProgressiveReportItem(ProgressiveReportLabel.MaxAmount,
                                                  GetCurrencyString(progressiveLevel.GetMaxAmount(), context)));

                    progressiveLevelPart.AddReportItem(
                        new ProgressiveReportItem(ProgressiveReportLabel.LinkStatus,
                                                  progressiveLevel.GetLinkStatus()));

                    progressiveLevelPart.AddReportItem(
                        new ProgressiveReportItem(ProgressiveReportLabel.StartPercent,
                                                  GetPercentString(startPercent, ProgressivePercentPrecision,
                                                                   context)));

                    progressiveLevelPart.AddReportItem(
                        new ProgressiveReportItem(ProgressiveReportLabel.ContributionPercentage,
                                                  GetPercentString(contributionPercent, ContributionPercentPrecision,
                                                                   context)));

                    progressiveLevelPart.AddReportItem(
                        new ProgressiveReportItem(ProgressiveReportLabel.LevelTotalRtp,
                                                  GetPercentString(levelTotalRtp, ProgressivePercentPrecision,
                                                                   context)));

                    // Add custom report items.
                    var customReportSection = progressiveLevel.CustomReportSection;

                    if(customReportSection != null)
                    {
                        progressiveLevelPart.AddReportItems(customReportSection.CustomReportItems);
                    }

                    yield return progressiveLevelPart;
                }
            }
        }

        #endregion

        #region Standard Report Section

        /// <summary>
        /// A dictionary mapping basic <see cref="StandardReportLabel"/>s to their corresponding
        /// methods of <see cref="IStandardReportSection"/>.
        /// None of these report items require any extra logic.
        /// </summary>
        private readonly Dictionary<StandardReportLabel, Func<IStandardReportSection, ReportContext, string>>
            basicReportItems =
                new Dictionary<StandardReportLabel, Func<IStandardReportSection, ReportContext, string>>
                {
                    {StandardReportLabel.GameDescription, (section, context) => section.GetGameDescription()},
                    {StandardReportLabel.LinkSeriesModel, (section, context) => section.GetLinkSeriesModel()},
                    {
                        StandardReportLabel.MinLines,
                        (section, context) => section.GetMinLines().ToString(context.CultureInfo)
                    },
                    {
                        StandardReportLabel.MaxLines,
                        (section, context) => section.GetMaxLines().ToString(context.CultureInfo)
                    },
                    {
                        StandardReportLabel.MinWays,
                        (section, context) => section.GetMinWays().ToString(context.CultureInfo)
                    },
                    {
                        StandardReportLabel.MaxWays,
                        (section, context) => section.GetMaxWays().ToString(context.CultureInfo)
                    }
                };

        /// <summary>
        /// Generates the standard report section.
        /// </summary>
        /// <param name="context">The current <see cref="ReportContext"/>.</param>
        /// <param name="progressiveLevelParts">
        /// Collection of <see cref="ProgressiveLevelReportPart"/>s used
        /// for calculating the Jackpot RTP.
        /// </param>
        /// <returns>
        /// A <see cref="StandardReportPart"/> populated with report data.
        /// </returns>
        private StandardReportPart GenerateStandardSection(ReportContext context,
            IList<ProgressiveLevelReportPart> progressiveLevelParts)
        {
            var standardPart = new StandardReportPart();
            var standardSection = CreateStandardReportSection(context);

            if(standardSection == null)
            {
                throw new NullReferenceException("The Standard Report Section cannot be null.");
            }

            // Basic Report Items
            foreach(var basicReportPair in basicReportItems)
            {
                standardPart.AddReportItem(new StandardReportItem(basicReportPair.Key,
                                                                  basicReportPair.Value(standardSection,
                                                                                        context)));
            }

            // Max Win
            var creditValue = standardSection.GetMaxWinCredits();
            var amountValue = Utility.ConvertToCents(creditValue, context.Denomination);

            standardPart.AddReportItem(new StandardReportItem(StandardReportLabel.MaxWinCredits,
                                                              creditValue.ToString(context.CultureInfo)));
            standardPart.AddReportItem(new StandardReportItem(StandardReportLabel.MaxWinAmount,
                                                              LocalizeCurrency(amountValue, context)));

            // Min Bet
            creditValue = standardSection.GetMinBetCredits();
            amountValue = Utility.ConvertToCents(creditValue, context.Denomination);

            standardPart.AddReportItem(new StandardReportItem(StandardReportLabel.MinBetCredits,
                                                              creditValue.ToString(context.CultureInfo)));
            standardPart.AddReportItem(new StandardReportItem(StandardReportLabel.MinBetAmount,
                                                              LocalizeCurrency(amountValue, context)));

            // Base RTP/Hold
            var percentPrecision = StandardPercentPrecision;
            var rtpPercent = Round(standardSection.GetBaseRtpPercent(), percentPrecision);
            var holdPercent = Round(CalculateHoldPercent(rtpPercent), percentPrecision);

            standardPart.AddReportItem(new StandardReportItem(StandardReportLabel.BaseRtpPercent,
                                                              LocalizePercent(rtpPercent,
                                                                              percentPrecision,
                                                                              context)));
            standardPart.AddReportItem(new StandardReportItem(StandardReportLabel.BaseHoldPercent,
                                                              LocalizePercent(holdPercent,
                                                                              percentPrecision,
                                                                              context)));

            // Total RTP/Hold
            rtpPercent = Round(standardSection.GetTotalRtpPercent(), percentPrecision);
            holdPercent = Round(CalculateHoldPercent(rtpPercent), percentPrecision);

            standardPart.AddReportItem(new StandardReportItem(StandardReportLabel.TotalRtpPercent,
                                                              LocalizePercent(rtpPercent,
                                                                              percentPrecision,
                                                                              context)));
            standardPart.AddReportItem(new StandardReportItem(StandardReportLabel.TotalHoldPercent,
                                                              LocalizePercent(holdPercent,
                                                                              percentPrecision,
                                                                              context)));

            // Jackpot RTP
            // Do not report Jackpot RTP if progressive levels are not present.
            if(progressiveLevelParts.Any())
            {
                // Verify if the link controller is needed for any progressive levels.
                var useLinkController =
                    progressiveLevelParts.Any(progressivePart => progressivePart.LevelTotalRtp == CheckLinkController);

                // Add the report item.
                standardPart.AddReportItem(
                    new StandardReportItem(StandardReportLabel.JackpotRtp,
                                           useLinkController
                                               ? CheckLinkControllerText
                                               : LocalizePercent(CalculateJackpotRtp(progressiveLevelParts),
                                                                 ProgressivePercentPrecision,
                                                                 context)));
            }

            // Custom Report Items
            var customReportSection = standardSection.CustomReportSection;

            if(customReportSection != null)
            {
                standardPart.AddReportItems(customReportSection.CustomReportItems);
            }

            return standardPart;
        }

        #endregion

        #region Localization Methods

        /// <summary>
        /// Formats a percentage with the specified number of decimal places
        /// and localizes it to the current culture.
        /// </summary>
        /// <param name="percentage">Percentage value to localize.</param>
        /// <param name="decimalPrecision">Number of decimal places to display.</param>
        /// <param name="context"><see cref="ReportContext"/> which provides culture information.</param>
        /// <returns>A localized string representing the percentage value.</returns>
        public static string LocalizePercent(decimal percentage, int decimalPrecision, ReportContext context)
        {
            // The Percent Format Specifier multiplies the value by 100 before formatting. 
            percentage = percentage / 100;
            return percentage.ToString($"P{decimalPrecision}", context.CultureInfo);
        }

        /// <summary>
        /// Localizes an amount value in base units to a currency string.
        /// </summary>
        /// <param name="amountBaseUnits">Amount value in base units.</param>
        /// <param name="context">
        /// <see cref="ReportContext"/> which provides denomination and culture information.
        /// </param>
        /// <returns>A localized currency string representing the amount value.</returns>
        public static string LocalizeCurrency(long amountBaseUnits, ReportContext context)
        {
            var amount = Amount.FromBaseValue(amountBaseUnits, context.Denomination);
            return context.CreditFormatter.FormatForCurrency(amount, CashDisplayMode.WholePlusBase);
        }

        #endregion

        #region Report Generation Utilities

        /// <summary>
        /// Calculates the Hold percent from the Return to Player percent.
        /// Hold = 100% - RTP.
        /// </summary>
        /// <param name="rtpPercent">Return to Player percent.</param>
        /// <returns>The Hold percent.</returns>
        private static decimal CalculateHoldPercent(decimal rtpPercent)
        {
            return 100 - rtpPercent;
        }

        /// <summary>
        /// Calculates the Jackpot Return to Player (RTP), which is the sum of
        /// all progressive level Total RTPs.
        /// </summary>
        /// <param name="progressiveLevelParts">Collection of <see cref="ProgressiveLevelReportPart"/>s.</param>
        /// <returns>The Jackpot RTP.</returns>
        private static decimal CalculateJackpotRtp(IEnumerable<ProgressiveLevelReportPart> progressiveLevelParts)
        {
            return progressiveLevelParts.Sum(progressivePart => Round(progressivePart.LevelTotalRtp,
                                                                      ProgressivePercentPrecision));
        }

        /// <summary>
        /// Calculates the Total Return to Player (RTP) for a progressive level.
        /// </summary>
        /// <param name="startPercent">The Return to Player (RTP) percent from the start amount.</param>
        /// <param name="contributionPercentage">The contribution rate for the progressive level.</param>
        /// <returns>
        /// The Level Total RTP or <see cref="CheckLinkControllerText"/> if either 
        /// <paramref name="startPercent"/> or <paramref name="contributionPercentage"/>
        /// are equal to <see cref="CheckLinkController"/>.
        /// </returns>
        private static decimal CalculateLevelTotalRtp(decimal startPercent, decimal contributionPercentage)
        {
            return startPercent == CheckLinkController || contributionPercentage == CheckLinkController
                       ? CheckLinkController
                       : Round(startPercent + contributionPercentage, ProgressivePercentPrecision);
        }

        /// <summary>
        /// Rounds a decimal value to the specified number of decimal places.
        /// </summary>
        /// <param name="value">Decimal value to round.</param>
        /// <param name="decimalPrecision">Number of decimal places in return value.</param>
        /// <returns>Rounded decimal value.</returns>
        private static decimal Round(decimal value, int decimalPrecision)
        {
            return Math.Round(value, decimalPrecision, MidpointRounding.AwayFromZero);
        }

        #endregion

        #region Link Controller Helper Methods

        /// <summary>
        /// If the specified value is <see cref="CheckLinkController"/>, gets
        /// <see cref="CheckLinkControllerText"/>; otherwise, calls <see cref="Round(decimal, int)"/>.
        /// </summary>
        /// <param name="value">Decimal value to round.</param>
        /// <param name="decimalPrecision">Number of decimal places in return value.</param>
        /// <returns>Rounded decimal value or <see cref="CheckLinkControllerText"/>.</returns>
        private static decimal GetRoundedValue(decimal value, int decimalPrecision)
        {
            return value == CheckLinkController
                       ? value
                       : Round(value, decimalPrecision);
        }

        /// <summary>
        /// If the specified value is <see cref="CheckLinkController"/>, gets
        /// <see cref="CheckLinkControllerText"/>; otherwise, calls 
        /// <see cref="LocalizeCurrency(long, ReportContext)"/>.
        /// </summary>
        /// <param name="amountBaseUnits">
        /// Amount value in base units.
        /// </param>
        /// <param name="context">
        /// <see cref="ReportContext"/> which provides denomination and culture information.
        /// </param>
        /// <returns>
        /// A localized currency string representing the amount value or <see cref="CheckLinkControllerText"/>.
        /// </returns>
        private static string GetCurrencyString(long amountBaseUnits, ReportContext context)
        {
            return amountBaseUnits == CheckLinkController
                       ? CheckLinkControllerText
                       : LocalizeCurrency(amountBaseUnits, context);
        }

        /// <summary>
        /// If the specified value is <see cref="CheckLinkController"/>, gets
        /// <see cref="CheckLinkControllerText"/>; otherwise, calls 
        /// <see cref="LocalizePercent(decimal, int, ReportContext)"/>.
        /// </summary>
        /// <param name="percentage">Percentage value to localize.</param>
        /// <param name="decimalPrecision">Number of decimal places to display.</param>
        /// <param name="context"><see cref="ReportContext"/> which provides culture information.</param>
        /// <returns>
        /// A localized string representing the percentage value or <see cref="CheckLinkControllerText"/>.
        /// </returns>
        private static string GetPercentString(decimal percentage, int decimalPrecision, ReportContext context)
        {
            return percentage == CheckLinkController
                       ? CheckLinkControllerText
                       : LocalizePercent(percentage, decimalPrecision, context);
        }

        #endregion
    }
}