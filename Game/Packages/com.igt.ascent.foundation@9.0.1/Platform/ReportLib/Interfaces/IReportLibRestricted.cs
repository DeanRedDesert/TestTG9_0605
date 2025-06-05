//-----------------------------------------------------------------------
// <copyright file = "IReportLibRestricted.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ReportLib.Interfaces
{
    using System.Collections.Generic;
    using System.Threading;
    using Game.Core.GameReport.Interfaces;

    /// <summary>
    /// ReportLib interface which contains functionality that should
    /// not be used by client code in most cases.
    /// </summary>
    public interface IReportLibRestricted
    {
        /// <summary>
        /// Connect to the Foundation.  Must be called after the construction
        /// of Report Lib.  It is part of the initialization that puts the Report
        /// Lib in a workable state, for both Standard and Standalone Report Lib.
        /// </summary>
        /// <param name="reportingServiceTypes">
        /// Reporting service types for filtering out service specific category.
        /// </param>
        /// <returns>
        /// True if connection is established successfully; False otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="reportingServiceTypes"/> is null.
        /// </exception>
        bool ConnectToFoundation(IEnumerable<ReportingServiceType> reportingServiceTypes);

        /// <summary>
        /// Disconnect from the Foundation.
        /// </summary>
        /// <returns>True if the connection is terminated successfully.</returns>
        bool DisconnectFromFoundation();

        /// <summary>
        /// Process any pending events, or wait for an event and then process it.
        /// If any of the passed waitHandles are signaled, then the function will 
        /// unblock.
        /// </summary>
        /// <param name="timeout">
        /// If no events are available, then this specifies the amount of time
        /// to wait for an event. If the timeout is 0, then the function will
        /// return immediately after processing any pending events. If the
        /// timeout is Timeout.Infinite, then the function will not return until
        /// an event has been processed.
        /// </param>
        /// <param name="waitHandles">
        /// An array of additional wait handles that will unblock the function.
        /// </param>
        /// <returns>
        /// The supplied wait handle that unblocked the function, or 
        /// <see langword="null"/> if the function was unblocked for another reason.
        /// </returns>
        WaitHandle ProcessEvents(int timeout, WaitHandle[] waitHandles);
    }
}
