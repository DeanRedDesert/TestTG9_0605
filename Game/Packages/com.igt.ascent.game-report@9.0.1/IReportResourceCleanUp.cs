// -----------------------------------------------------------------------
// <copyright file = "IReportResourceCleanUp.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using Ascent.Communication.Platform.ReportLib.Interfaces;

    /// <summary>
    /// This interface is used for cleaning up resources for the Game Reporting,
    /// such as un-registering Report Lib events etc.
    /// </summary>
    /// <remarks>
    /// Game Reporting should implement this interface as needed for cleaning up
    /// resources.
    /// </remarks>
    public interface IReportResourceCleanUp
    {
        /// <summary>
        /// Cleans up resources of the Game Reporting.
        /// </summary>
        /// <param name="reportLib">Game report interface to the Foundation.</param>
        void CleanUpResources(IReportLib reportLib);
    }
}