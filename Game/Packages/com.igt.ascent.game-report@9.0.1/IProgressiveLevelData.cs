//-----------------------------------------------------------------------
// <copyright file = "IProgressiveLevelData.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    /// <summary>
    /// Defines methods for retrieving Foundation-defined and custom report data for a
    /// progressive game level.  For each progressive game level, all Foundation-defined 
    /// report items will occur in the final report before any custom report items.  
    /// Ordering of <see cref="Interfaces.CustomReportItem"/>s will be preserved
    /// as retrieved from <see cref="ICustomReportSection"/>.
    /// 
    /// Any numerical value may be omitted to allow the progressive report item to state
    /// "Check Link Controller" by returning <see cref="GameDataInspectionServiceHandlerBase.CheckLinkController"/>.
    /// </summary>
    public interface IProgressiveLevelData
    {
        /// <summary>
        /// Gets an optional <see cref="ICustomReportSection"/> which provides custom report items
        /// for the progressive level.  May be null.
        /// </summary>
        ICustomReportSection CustomReportSection { get; }

        /// <summary>
        /// Gets or sets the progressive game level.
        /// </summary>
        int ProgressiveLevel { get; }
    
        /// <summary>
        /// Gets the localized description of the progressive level in terms of the game.
        /// </summary>
        /// <returns>The game level description.</returns>
        string GetGameLevelDescription();

        /// <summary>
        /// Gets the starting or reset amount in base units for the progressive level.
        /// </summary>
        /// <returns>
        /// The start amount or <see cref="GameDataInspectionServiceHandlerBase.CheckLinkController"/>
        /// if the value comes from the link controller.
        /// </returns>
        long GetStartAmount();

        /// <summary>
        /// Gets the contribution rate for the progressive level.
        /// </summary>
        /// <returns>
        /// The contribution percentage or <see cref="GameDataInspectionServiceHandlerBase.CheckLinkController"/>
        /// if the value comes from the link controller.
        /// </returns>
        decimal GetContributionPercentage();

        /// <summary>
        /// Gets the Return to Player (RTP) percent from the start amount.
        /// </summary>
        /// <remarks>
        /// This is a the RTP as if a progressive was awarded directly after a reset without any contributions.
        /// </remarks>
        /// <returns>
        /// The start percent or <see cref="GameDataInspectionServiceHandlerBase.CheckLinkController"/>
        /// if the value comes from the link controller.
        /// </returns>
        decimal GetStartPercent();

        /// <summary>
        /// Gets the maximum amount in base units that the progressive level can award.
        /// </summary>
        /// <returns>
        /// The maximum amount or <see cref="GameDataInspectionServiceHandlerBase.CheckLinkController"/>
        /// if the value comes from the link controller.
        /// </returns>
        long GetMaxAmount();

        /// <summary>
        /// Gets a localized description of the type of progressive level.
        /// </summary>
        /// <remarks>
        /// Example: "Stand Alone (Controller Level: 0)"
        /// </remarks>
        /// <returns>The link status.</returns>
        string GetLinkStatus();
    }
}