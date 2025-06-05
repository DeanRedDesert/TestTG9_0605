// -----------------------------------------------------------------------
// <copyright file = "GameReportLog.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Communication;
    using Logging;

    /// <summary>
    /// This class is for writing log files for the game report object.
    /// </summary>
    internal class GameReportLog
    {
        #region Nested Class

        /// <summary>
        /// This class stores the result of parsing command line arguments.
        /// </summary>
        private class RunParameters
        {
            /// <summary>
            /// Gets or sets the directory to write system error and report logs.
            /// </summary>
            public string CaptureDirectory { get; set; }

            /// <summary>
            /// Gets or sets the directory to write report logs for development.
            /// </summary>
            public string DevelopmentLogDirectory { get; set; }

            /// <summary>
            /// Gets the flag indicating whether it is a development environment.
            /// </summary>
            public bool IsDevelopment => !string.IsNullOrEmpty(DevelopmentLogDirectory);

            /// <summary>
            /// Gets or sets the assembly name of the game report object.
            /// </summary>
            public string AssemblyName { get; set; }
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// The prefix of the command line arguments for assembly name set by game.
        /// </summary>
        private const string GameArgPrefix = "-g0";

        /// <summary>
        /// The writer of writing logs to files.
        /// </summary>
        private RollingFilesWriter rollingFilesWriter;

        /// <summary>
        /// The maximum size, in bytes, of a single log file.
        /// </summary>
        /// <remarks>
        /// The defined maximum file size equivalent to 10MB.
        /// </remarks>
        private const int MaxLogFileSize = 10485760;

        /// <summary>
        /// The maximum number of log files to keep.
        /// </summary>
        private const int MaxLogFiles = 10;

        /// <summary>
        /// The runtime parameters of the game report object.
        /// </summary>
        private RunParameters runParameters;

        /// <summary>
        /// The directory of the report log file.
        /// </summary>
        private static string reportLogFileDirectory;

        /// <summary>
        /// The suffix to the name of the report log file.
        /// </summary>
        /// <devdoc>
        /// If changed, this must also be updated in 
        /// SDK Assets/GameReport/Editor/StandaloneGameReportConfigurator.cs,
        /// and GameReportActivator/GameReportActivator/Program.cs.
        /// </devdoc>
        private const string LogFileNameSuffix = "-GameReportLog";

        #endregion

        /// <summary>
        /// Gets or sets the command line arguments.
        /// </summary>
        /// <remarks>
        /// This property is added for custom usage purpose. It can be initialized from mocked command line arguments
        /// for being used by unit test for instance.
        /// </remarks>
        internal CommandLineArguments CommandLineArguments { get; set; }

        /// <summary>
        /// The static instance of <see cref="GameReportLog"/>.
        /// </summary>
        public static readonly GameReportLog Instance = new GameReportLog();

        /// <summary>
        /// Initializes a new instance of <see cref="GameReportLog"/> class.
        /// </summary>
        private GameReportLog()
        {
            CommandLineArguments = CommandLineArguments.Environment;
        }

        #region Public Methods

        /// <summary>
        /// Initializes for writing logs for the game report object.
        /// </summary>
        /// <remarks>
        /// If the game report object is in development environment, the log files are
        /// placed in a directory for developing logs that specified by the Foundation.
        /// Otherwise, the log files are placed in the Persistent\GameReport directory.
        /// </remarks>
        /// <devdoc>
        /// The Game Report Log is mainly responsible for writing messages to game report
        /// log files in development build.
        /// </devdoc>
        public void InitializeGameReportLog()
        {
            runParameters = ParseCommandLineArguments();

            reportLogFileDirectory = runParameters.CaptureDirectory;

            if(runParameters.IsDevelopment)
            {
                if(!Directory.Exists(runParameters.DevelopmentLogDirectory))
                {
                    Directory.CreateDirectory(runParameters.DevelopmentLogDirectory);
                }

                reportLogFileDirectory = runParameters.DevelopmentLogDirectory;
            }

            rollingFilesWriter = new RollingFilesWriter(reportLogFileDirectory,
                                                        runParameters.AssemblyName + LogFileNameSuffix,
                                                        MaxLogFiles,
                                                        MaxLogFileSize);
            rollingFilesWriter.Write(
                string.Format("{0}{1}{0}{2}", new string('=', 37), DateTime.Now, Environment.NewLine));

            Log.LogHandlers += HandleLog;
        }

        /// <summary>
        /// Cleans up the resources of game report object loggings.
        /// </summary>
        public void CleanUp()
        {
            Log.LogHandlers = null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handler for Core assembly logging.
        /// </summary>
        /// <param name="logType">The log type defined by the Core assembly.</param>
        /// <param name="message">The message to write to the log files.</param>
        /// <exception cref="ReportingException">
        /// Thrown if the <paramref name="logType"/> is error type, which indicates
        /// <paramref name="message"/> is for reporting a problem.
        /// </exception>
        private void HandleLog(LogType logType, string message)
        {
            if(logType == LogType.Error)
            {
                throw new ReportingException(message);
            }

            var logEntry = BuildLogString(message, logType);

            rollingFilesWriter.Write(logEntry);
        }

        /// <summary>
        /// Builds a string for the log message.
        /// </summary>
        /// <param name="logString">The string being logged.</param>
        /// <param name="type">The type of the log.</param>
        /// <returns>The string representing the log.</returns>
        private static string BuildLogString(string logString, LogType type)
        {
            var logStringBuilder = new StringBuilder(512);

            logStringBuilder.AppendLine(string.Concat("== ", type, " ", new string('=', 76 - type.ToString().Length)));
            logStringBuilder.AppendFormat("[{0}] ", DateTime.Now).AppendLine(logString).AppendLine();

            return logStringBuilder.ToString();
        }

        /// <summary>
        /// Parses command line arguments.
        /// </summary>
        /// <returns>A data structure storing the results.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when required argument for retrieving assembly name is missing.
        /// </exception>
        private RunParameters ParseCommandLineArguments()
        {   
            var captureDirectory = CommandLineArguments.GetValue("c") ?? Directory.GetCurrentDirectory();
            var developmentDirectory = CommandLineArguments.GetValue("d");

            var args = CommandLineArguments.CommandLineArgs;

            var assemblyName = (from arg in args
                                where arg.StartsWith(GameArgPrefix) && arg.Length > 3
                                select arg.Substring(3)).FirstOrDefault();

            if(string.IsNullOrEmpty(assemblyName))
            {
                throw new InvalidOperationException(
                    $"Failed to retrieve game report assembly or type name from command line: {string.Join(" ", args)}");
            }

            return new RunParameters
                       {
                           CaptureDirectory = captureDirectory,
                           DevelopmentLogDirectory = developmentDirectory,
                           AssemblyName = assemblyName
                       };
        }

        #endregion
    }
}
