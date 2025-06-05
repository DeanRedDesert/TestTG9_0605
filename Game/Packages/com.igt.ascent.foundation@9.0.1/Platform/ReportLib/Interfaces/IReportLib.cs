//-----------------------------------------------------------------------
// <copyright file = "IReportLib.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ReportLib.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// The interface to the Foundation used by game reports.
    /// </summary>
    public interface IReportLib
    {
        #region Events

        /// <summary>
        /// Occurs when the report object should activate a new context.
        /// </summary>
        event EventHandler<ActivateContextEventArgs> ActivateContextEvent;

        /// <summary>
        /// Occurs when the Foundation tells the report object to inactivate the current context.
        /// </summary>
        event EventHandler<InactivateContextEventArgs> InactivateContextEvent;

        /// <summary>
        /// Occurs when the Foundation asks for an itemized game inspection report.
        /// </summary>
        event EventHandler<GenerateInspectionReportEventArgs> GenerateInspectionReportEvent;

        /// <summary>
        /// Occurs when the Foundation asks for an Html game inspection report.
        /// </summary>
        event EventHandler<GenerateHtmlInspectionReportEventArgs> GenerateHtmlInspectionReportEvent;

        /// <summary>
        /// Occurs when the Foundation asks for a game inspection report type. The handler of this event
        /// is expected to set the report type in the output field of the event arguments.
        /// </summary>
        event EventHandler<GetInspectionReportTypeEventArgs> GetInspectionReportTypeEvent;

        /// <summary>
        /// Occurs when the Foundation notifies the report object to initialize the game level data
        /// for the purpose of subsequent queries.
        /// </summary>
        event EventHandler<InitializeGameLevelDataEventArgs> InitializeGameLevelDataEvent;

        /// <summary>
        /// Occurs when the Foundation asks for the game level data of a specific theme.
        /// This event is non-transactional.
        /// </summary>
        event EventHandler<GetGameLevelValuesEventArgs> GetGameLevelValuesEvent;

        /// <summary>
        /// Occurs when the Foundation requires to validate a specific theme.
        /// </summary>
        event EventHandler<ValidateThemeSetupEventArgs> ValidateThemeSetupEvent;

        /// <summary>
        /// Occurs when the Foundation asks for a game performance report.
        /// </summary>
        event EventHandler<GenerateGamePerformanceReportEventArgs> GenerateGamePerformanceReportEvent;

        /// <summary>
        /// Occurs when the Foundation asks for the minimum playable credit balance amount required to commit a new game-cycle
        /// for the theme, payvar and denomination combination given.
        /// </summary>
        event EventHandler<GetMinPlayableCreditBalanceEventArgs> GetMinPlayableCreditBalanceEvent;

        /// <summary>
        /// Occurs when the Foundation shuts down the game report executable.
        /// </summary>
        /// <remarks>
        /// Handler of this event must be thread safe.
        /// </remarks>
        event EventHandler ShutDownEvent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the mount point of the report package.
        /// </summary>
        string MountPoint { get; }

        /// <summary>
        /// Gets the jurisdiction string value.
        /// </summary>
        /// <remarks>
        /// IMPORTANT:
        /// 
        /// DO NOT rely on a specific jurisdiction string value to implement a feature,
        /// as the jurisdiction string value is not enumerated, and could change over time.
        /// 
        /// For example, Nevada used to be reported as USDM, but later as 00NV.
        /// 
        /// This API is kept only for the purpose of temporary work-around, when the time-line
        /// of the official support for a feature in Foundation and/or SDK could not meet a client's
        /// specific timetable requirement.  The client should use this jurisdiction string at
        /// its own risks of breaking compatibility with future Foundation and/or SDK.
        /// </remarks>
        string Jurisdiction { get; }

        /// <summary>
        /// Gets the interface for requesting localization information.
        /// </summary>
        ILocalizationInformation LocalizationInformation { get; }

        /// <summary>
        /// Gets the interface for requesting EGM-wide data.
        /// </summary>
        IEgmConfigData EgmConfigData { get; }

        /// <summary>
        /// Gets the interface for requesting information about custom configuration items.
        /// </summary>
        IConfigurationRead ConfigurationRead { get; }

        /// <summary>
        /// Gets the interface for accessing critical data.
        /// </summary>
        ICriticalDataAccessor CriticalDataAccessor { get; }

        /// <summary>
        /// Gets the interface for requesting information for game report.
        /// </summary>
        IGameInformation GameInformation { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the progressive settings of the linked game levels
        /// correlating to the given paytable and given denomination.
        /// </summary>
        /// <param name="paytableIdentifier">The identifier of the paytable used for report.</param>
        /// <param name="denomination">The denomination used for report.</param>
        /// <returns>
        /// The progressive settings for the given denomination.
        /// Empty if no game level is linked.
        /// If a game level is linked but the settings are not available yet,
        /// its progressive settings value would be null.
        /// </returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        IDictionary<int, ProgressiveSettings> GetLinkedProgressiveSettings(string paytableIdentifier,
                                                                           long denomination);

        #endregion
    }
}
