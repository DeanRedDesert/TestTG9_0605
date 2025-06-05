//-----------------------------------------------------------------------
// <copyright file = "LogType.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logging
{
    /// <summary>
    /// Type of the messages being logged.
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// Regular log messages.
        /// </summary>
        Log,

        /// <summary>
        /// Warnings, which will not terminate the application.
        /// </summary>
        Warning,

        /// <summary>
        /// Errors, which usually terminate the application.
        /// </summary>
        Error,

        /// <summary>
        /// Log messages that will be written even in a release environment,
        /// at the potential risks of impacting the performances.
        /// </summary>
        LogInRelease
    }
}
