//-----------------------------------------------------------------------
// <copyright file = "StandardReportSection.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using System;
    using System.Linq;
    using Ascent.Communication.Platform.ReportLib.Interfaces;
    using Logic.Evaluator.Schemas;

    /// <summary>
    /// Abstract base class that provides a default implementation of most of
    /// <see cref="IStandardReportSection"/> using a Base Game <see cref="SlotPaytableSection"/>.
    /// All methods are virtual allowing a game-specific class to selectively
    /// override reporting logic.
    /// See the remarks section of any <see cref="IStandardReportSection"/> method
    /// implementation to see specific details.
    /// Requirements to use <see cref="StandardReportSection"/>:
    /// <list type="bullet">
    /// <item>Provide the name of the paytables's Base <see cref="SlotPaytableSection"/></item>
    /// <item>Provide a <see cref="BetDefinitionList"/> for Max Bet</item>
    /// </list>
    /// </summary>
    public abstract class StandardReportSection : IStandardReportSection
    {
        private Paytable paytable;
        private SlotPaytableSection basePaytableSection;

        /// <summary>
        /// Instantiates a new <see cref="StandardReportSection"/>.
        /// </summary>
        /// <param name="reportLib">The game report interface to the Foundation.</param>
        /// <param name="reportContext">Active <see cref="Core.GameReport.ReportContext"/> for future
        /// data requests.</param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown if the reportContext is null. 
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   Thrown if the underlying paytable type is not of
        /// <see cref="Paytable"/>,
        /// which is the legacy Ascent XPaytbale format. </exception>
        protected StandardReportSection(IReportLib reportLib, ReportContext reportContext)
        {
            if(reportContext == null)
            {
                throw new ArgumentNullException(nameof(reportContext));
            }

            paytable = reportContext.PaytableData.RawPaytable as Paytable;
            if(paytable == null)
            {
                throw new ArgumentException("ReportContext.PaytableType is not of type Xpaytable");
            }

            ReportLib = reportLib;
            ReportContext = reportContext;
        }

        /// <summary>
        /// Gets the game report interface to the Foundation.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        protected IReportLib ReportLib { get; }

        /// <summary>
        /// Gets the current <see cref="Core.GameReport.ReportContext"/>.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        protected ReportContext ReportContext { get; }

        /// <summary>
        /// Gets the Base <see cref="SlotPaytableSection"/>.
        /// When requested the first time, loads the <see cref="SlotPaytableSection"/>
        /// from the Paytable based on <see cref="BasePaytableSectionName"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the <see cref="PaytableSection"/> specified by 
        /// <see cref="BasePaytableSectionName"/> is not of type <see cref="SlotPaytableSection"/>.
        /// </exception>
        // ReSharper disable once MemberCanBePrivate.Global
        protected SlotPaytableSection BasePaytableSection
        {
            get
            {
                if(basePaytableSection == null)
                {
                    try
                    {
                        basePaytableSection = paytable.GetPaytableSection<SlotPaytableSection>(BasePaytableSectionName);
                    }
                    catch(InvalidSectionTypeException exception)
                    {
                        throw new InvalidOperationException(
                            "StandardReportSection requires the Base Paytable Section be of type " +
                            "IGT.Game.Core.Logic.Evaluator.Schemas.SlotPaytableSection", 
                            exception);
                    }
                }

                return basePaytableSection;
            }
        }

        #region Abstract Members

        /// <summary>
        /// When overridden in a derived class, gets the <see cref="SlotPaytableSection"/> name 
        /// for the Base Game.
        /// </summary>
        protected abstract string BasePaytableSectionName { get; }

        /// <inheritdoc/>
        public abstract string GetGameDescription();

        /// <inheritdoc/>
        public abstract long GetMinBetCredits();

        /// <summary>
        /// Gets a <see cref="BetDefinitionList"/> for max bet which is available through
        /// <see cref="IGT.Game.Core.GameReport.ReportContext.MaxBet"/>.
        /// This is used when calculating <see cref="GetMaxWinCredits"/>.
        /// </summary>
        /// <returns>A <see cref="BetDefinitionList"/> representing the maximum bet.</returns>
        protected abstract BetDefinitionList GetMaxBetDefinitionList();

        #endregion

        #region Virtual Members

        /// <inheritdoc/>
        /// <remarks>
        /// Default implementation returns null.
        /// </remarks>
        public virtual ICustomReportSection CustomReportSection => null;

        /// <inheritdoc/>
        /// <remarks>
        /// Default implementation returns the localized version of "Not Applicable".
        /// </remarks>
        public virtual string GetLinkSeriesModel()
        {
            // TODO: Localize "Not Applicable"
            return "Not Applicable";
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Default implementation returns 1 if <see cref="SlotPaytableSection.LinePatternList"/> exists;
        /// otherwise, returns 0.
        /// </remarks>
        public virtual uint GetMinLines()
        {
            uint minLines = 0;
            if(BasePaytableSection.LinePatternList != null)
            {
                minLines = 1;
            }
            return minLines;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Default implementation returns length of <see cref="SlotPaytableSection.LinePatternList"/> if it exists;
        /// otherwise, returns 0.
        /// </remarks>
        public virtual uint GetMaxLines()
        {
            uint maxLines = 0;
            if(BasePaytableSection.LinePatternList != null)
            {
                maxLines = (uint)BasePaytableSection.LinePatternList.Pattern.Count;
            }
            return maxLines;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Default implementation returns the smallest number of multiways of each 
        /// pattern in <see cref="SlotPaytableSection.MultiwayPatternList"/> if it exists;
        /// otherwise, returns 0.
        /// </remarks>
        public virtual uint GetMinWays()
        {
            uint minWays = 0;

            if(BasePaytableSection.MultiwayPatternList != null)
            {
                minWays = (uint)BasePaytableSection.MultiwayPatternList.Pattern.Min(pattern =>
                    pattern.Cluster.Aggregate(1, (current, cluster) => current * cluster.Cells.Count));
            }

            return minWays;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Default implementation returns the greatest number of multiways of each
        /// pattern in <see cref="SlotPaytableSection.MultiwayPatternList"/> if it exists;
        /// otherwise, returns 0.
        /// </remarks>
        public virtual uint GetMaxWays()
        {
            uint maxWays = 0;

            if(BasePaytableSection.MultiwayPatternList != null)
            {
                maxWays = (uint)BasePaytableSection.MultiwayPatternList.Pattern.Max(pattern =>
                    pattern.Cluster.Aggregate(1, (current, cluster) => current * cluster.Cells.Count));
            }

            return maxWays;
        }

        /// <inheritdoc/>
        /// <summary>
        /// Default implementation chooses the highest win out of <see cref="SlotPaytableSection.LinePrizeScale"/>,
        /// <see cref="SlotPaytableSection.ScatterPrizeScale"/>, and <see cref="SlotPaytableSection.MultiwayPrizeScale"/>.
        /// For each <see cref="SlotPrize"/>, the first <see cref="PrizePay"/> is considered for highest win.
        /// The <see cref="WinAmount"/>s are compared after applying the 
        /// <see cref="SlotPrizeAmountModificationStrategy"/> for the max bet <see cref="BetDefinitionList"/>.
        /// </summary>
        public virtual long GetMaxWinCredits()
        {
            return MaxWinEvaluator.GetMaxWin(BasePaytableSection,
                GetMaxBetDefinitionList(),
                paytable.ProgressiveLevels,
                MaxWinEvaluator.GetLinkedProgressiveLevels(ReportLib, ReportContext),
                ReportContext.Denomination);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Default implementation returns 
        /// <see cref="PaytableAbstract.MinPaybackPercentageWithoutProgressiveContributions"/>
        /// if it is specified; otherwise, returns <see cref="PaytableAbstract.MinPaybackPercentage"/>.
        /// </remarks>
        public virtual decimal GetBaseRtpPercent()
        {
            var paytableAbstract = paytable.Abstract;
            var rtpPercent = paytableAbstract.MinPaybackPercentage;

            if(paytableAbstract.MinPaybackPercentageWithoutProgressiveContributionsSpecified)
            {
                rtpPercent = paytableAbstract.MinPaybackPercentageWithoutProgressiveContributions;
            }

            return rtpPercent;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Default implementation returns <see cref="PaytableAbstract.MaxPaybackPercentage"/>.
        /// </remarks>
        public virtual decimal GetTotalRtpPercent()
        {
           return paytable.Abstract.MaxPaybackPercentage;
        }

        #endregion
    }
}
