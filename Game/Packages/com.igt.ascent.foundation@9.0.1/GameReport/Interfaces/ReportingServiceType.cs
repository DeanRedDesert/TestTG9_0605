// -----------------------------------------------------------------------
// <copyright file = "ReportingServiceType.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.Interfaces
{
    /// <summary>
    /// Flags that define the various supported reporting services.
    /// </summary>
    public enum ReportingServiceType
    {
        /// <summary>
        /// The Game Data Inspection reporting service, which provides a report to be
        /// displayed in the Audit operator menu.
        /// </summary>
        GameDataInspection,

        /// <summary>
        /// The Game Data Html Inspection reporting service, which provides a game defined Html formatted
        /// report to be displayed in the Audit operator menu.
        /// </summary>
        GameDataHtmlInspection,

        /// <summary>
        /// The Game Level Award reporting service, which provides game-level information
        /// to be displayed in Chooser.
        /// </summary>
        GameLevelAward,

        /// <summary>
        /// The Setup Validation reporting service, which upon Foundation's request, 
        /// validates a game's setup, and reports any validation fault back to the Foundation. 
        /// A validation error will prevent the game from being loaded and/or played.
        /// </summary>
        SetupValidation,

        /// <summary>
        /// The Game Performance reporting service, which provides a game performance report
        /// to the foundation on demand in Machine Data Export operator menu.
        /// </summary>
        GamePerformance,
    }
}