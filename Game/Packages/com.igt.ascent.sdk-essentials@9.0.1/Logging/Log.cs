//-----------------------------------------------------------------------
// <copyright file = "Log.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logging
{
    /// <summary>
    /// This class provides the APIs for Core assemblies
    /// to output messages to the outside logging handlers.
    /// </summary>
    public static class Log
    {
        #region Delegate Declaration

        /// <summary>
        /// Declares the signature of the method handling the log messages.
        /// </summary>
        /// <param name="logType">The type of the log entry.</param>
        /// <param name="message">The message of the log entry.</param>
        public delegate void LogHandler(LogType logType, string message);

        #endregion

        #region Properties

        /// <summary>
        /// Get or set the logging handlers.
        /// </summary>
        public static LogHandler LogHandlers { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Writes a regular message to the log.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public static void Write(string message)
        {
            WriteLog(LogType.Log, message);
        }

        /// <summary>
        /// Writes a regular message to the log using a format and an argument array.
        /// </summary>
        /// <param name="format">The string format pattern.</param>
        /// <param name="args">The arguments to place in the format.</param>
        public static void Write(string format, params object[] args)
        {
            Write(string.Format(format, args));
        }

        /// <summary>
        /// Writes a warning message to the log.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public static void WriteWarning(string message)
        {
            WriteLog(LogType.Warning, message);
        }

        /// <summary>
        /// Writes a warning message to the log using a format and an argument array.
        /// </summary>
        /// <param name="format">The string format pattern.</param>
        /// <param name="args">The arguments to place in the format.</param>
        public static void WriteWarning(string format, params object[] args)
        {
            WriteWarning(string.Format(format, args));
        }

        /// <summary>
        /// Writes an error message to the log,
        /// which most likely also terminates the running application.
        /// </summary>
        /// <remarks>
        /// Since this method usually leads to the termination of the running application, please use it with caution.
        /// In many cases, throwing an exception works better since it provides much better information on the error,
        /// such as the stack trace, inner exceptions etc.
        /// </remarks>
        /// <param name="message">The message to write.</param>
        public static void WriteError(string message)
        {
            WriteLog(LogType.Error, message);
        }

        /// <summary>
        /// Writes an error message to the log using a format and an argument array,
        /// which most likely also terminates the running application.
        /// </summary>
        /// <remarks>
        /// Since this method usually leads to the termination of the running application, please use it with caution.
        /// In many cases, throwing an exception works better since it provides much better information on the error,
        /// such as the stack trace, inner exceptions etc.
        /// </remarks>
        /// <param name="format">The string format pattern.</param>
        /// <param name="args">The arguments to place in the format.</param>
        public static void WriteError(string format, params object[] args)
        {
            WriteError(string.Format(format, args));
        }

        /// <summary>
        /// Writes a message in the release environment.
        /// !!! Writing logs in release environment could impact the run time performances !!!
        /// </summary>
        /// <param name="message">The message to write.</param>
        public static void WriteInRelease(string message)
        {
            WriteLog(LogType.LogInRelease, message);
        }

        /// <summary>
        /// Writes a message in the release environment using a format and an argument array.
        /// !!! Writing logs in release environment could impact the run time performances !!!
        /// </summary>
        /// <param name="format">The string format pattern.</param>
        /// <param name="args">The arguments to place in the format.</param>
        public static void WriteInRelease(string format, params object[] args)
        {
            WriteInRelease(string.Format(format, args));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Write a specific type of log message to the log.
        /// </summary>
        /// <param name="logType">The type of the log entry.</param>
        /// <param name="message">The message to write.</param>
        private static void WriteLog(LogType logType, string message)
        {
            LogHandlers?.Invoke(logType, message);
        }

        #endregion
    }
}
