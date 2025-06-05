//-----------------------------------------------------------------------
// <copyright file = "IStandardReportSection.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    /// <summary>
    /// Defines methods for retrieving Foundation-defined and custom report data. 
    /// All Foundation-defined report items will occur in the final report before any
    /// custom report items.  Ordering of <see cref="Interfaces.CustomReportItem"/>s will be preserved
    /// as retrieved from <see cref="ICustomReportSection"/>.
    /// </summary>
    public interface IStandardReportSection
    {
        /// <summary>
        /// Gets an optional <see cref="ICustomReportSection"/> which provides custom report items
        /// for the standard report section.  May be null.
        /// </summary>
        ICustomReportSection CustomReportSection { get; }

        /// <summary>
        /// Retrieves the game description or the name of the theme.
        /// </summary>
        string GetGameDescription();

        /// <summary>
        /// Gets the progressive link name for the theme.
        /// If the theme is unlinked, this should be "Not Applicable".
        /// If the theme link is known, use that name. Examples include 
        /// "Megabucks(R)", "Powerbucks(TM)", etc.
        /// If the theme link is unknown, use the theme name.
        /// </summary>
        string GetLinkSeriesModel();

        /// <summary>
        /// Gets the theme's minimum number of line patterns for a valid bet.
        /// If line patterns are not supported, this should be zero.
        /// </summary>
        /// <returns>Minimum number of line patterns.</returns>
        uint GetMinLines();

        /// <summary>
        /// Gets the theme's maximum number of line patterns for a valid bet.
        /// If line patterns are not supported, this should be zero.
        /// </summary>
        /// <returns>Maximum number of line patterns.</returns>
        uint GetMaxLines();

        /// <summary>
        /// Gets the theme's minimum number of multiway patterns for a valid bet.
        /// If multiway patterns are not supported, this should be zero.
        /// </summary>
        /// <returns>Minimum number of multiway patterns.</returns>
        uint GetMinWays();

        /// <summary>
        /// Gets the theme's maximum number of multiway patterns for a valid bet.
        /// If multiway patterns are not supported, this should be zero.
        /// </summary>
        /// <returns>Maximum number of multiway patterns.</returns>
        uint GetMaxWays();

        /// <summary>
        /// Gets the highest pay in credits for a single Base Game win category on
        /// a single line/multiway pattern at maximum bet, excluding progressive wins.
        /// </summary>
        /// <returns>Credit value of highest pay excluding progressive wins.</returns>
        long GetMaxWinCredits();

        /// <summary>
        /// Gets the Return to Player (RTP) percent at minimum bet.
        /// </summary>
        /// <returns>The base RTP percent.</returns>
        decimal GetBaseRtpPercent();

        /// <summary>
        /// Gets the Return to Player (RTP) percent at maximum bet.
        /// </summary>
        /// <returns>The total RTP percent.</returns>
        decimal GetTotalRtpPercent();

        /// <summary>
        /// Gets the minimum bet in credits for the current theme configuration.
        /// </summary>
        /// <returns>Credit value of the theme's minimum bet.</returns>
        long GetMinBetCredits();
    }
}