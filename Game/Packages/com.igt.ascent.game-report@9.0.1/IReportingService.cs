// -----------------------------------------------------------------------
// <copyright file = "IReportingService.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using Ascent.Communication.Platform.ReportLib.Interfaces;
    using Interfaces;

    /// <summary>
    /// This interface defines reporting service.
    /// </summary>
    /// <remarks>
    /// A specific reporting service needs to implement the <see cref="IReportingService"/>
    /// to provide the flag of reporting service, and handle the registration and un-registration
    /// of events for requests from the Foundation.
    /// This interface and its implementations are for internal usage.
    /// </remarks>
    internal interface IReportingService : IReportResourceCleanUp
    {
        /// <summary>
        /// Gets flag of the reporting service.
        /// </summary>
        ReportingServiceType ReportingServiceType { get; }

        /// <summary>
        /// Registers event handlers with the <paramref name="reportLib"/>
        /// for reporting service.
        /// </summary>
        /// <param name="reportLib">
        /// Game report interface to the Foundation.
        /// </param>
        void RegisterReportLibEvents(IReportLib reportLib);

        /// <summary>
        /// Un-registers event handlers with the <paramref name="reportLib"/>
        /// for reporting service.
        /// </summary>
        /// <param name="reportLib">
        /// Game report interface to the Foundation.
        /// </param>
        void UnregisterReportLibEvents(IReportLib reportLib);
    }
}