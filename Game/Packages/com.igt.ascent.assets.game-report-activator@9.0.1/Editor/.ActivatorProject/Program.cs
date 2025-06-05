//-----------------------------------------------------------------------
// <copyright file = "Program.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace GameReportActivator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    internal class Program
    {
        #region Constants

        /// <summary>
        /// Max length of the system error message.
        /// </summary>
        private const int SystemErrorMaxLength = 256;

        /// <summary>
        /// Prefix of the command line arguments set by game.
        /// </summary>
        private const string GameArgPrefix = "-g";

        /// <summary>
        /// Prefix of the command line arguments set by Foundation.
        /// </summary>
        private const string FoundationArgPrefix = "-";

        /// <summary>
        /// Name of the system error file.
        /// </summary>
        private const string SystemErrorFileName = "SystemError.txt";

        /// <summary>
        /// Suffix to the name of the report log file.
        /// </summary>
        /// <devdoc>
        /// If changed, this must also be updated in 
        /// com.igt.ascent.game-report-activator/Editor/StandaloneGameReportConfigurator.cs,
        /// and Core/GameReport/GameReportLog.cs.
        /// </devdoc>
        private const string LogFileNameSuffix = "-GameReportLog";

        /// <summary>
        /// Group name for the parsed foundation target.
        /// </summary>
        private const string FoundationGroupName = "Foundation";

        /// <summary>
        /// Regular expression for parsing the targeted foundation from the GameParameters file.
        /// </summary>
        private static readonly string TargetedFoundationPattern =
            string.Format("\"TargetedFoundation\"\\s*:\\s*\"(?<{0}>\\w+)\"",
                          FoundationGroupName);

        /// <summary>
        /// Path to the GameParameter config file relative to the GameReportActivator executable.
        /// </summary>
        /// <devdocs>
        /// GameReportActivator.exe is in Game-000XX0\Game-000XX0_Data\Managed\
        /// </devdocs>
        private const string GameParametersConfigPath = @"..\..\GameParameters.config";

        /// <summary>
        /// Path to the GameParameter config file relative to the GameReportActivator executable
        /// for standalone.
        /// </summary>
        /// <devdocs>
        /// GameReportActivator.exe is in Game\Temp\GameReportActivator\Bin\
        /// </devdocs>
        private const string StandaloneGameParametersConfigPath = @"..\..\..\GameParameters.config";

        #endregion

        #region Fields

        /// <summary>
        /// Full path of the system error file to be read by the Foundation.
        /// </summary>
        private static string systemErrorFilePath;

        /// <summary>
        /// Directory of the report log file.
        /// </summary>
        private static string reportLogDirectory;

        /// <summary>
        /// Name of assembly for the game report object.
        /// </summary>
        private static string currentAssemblyName;

        /// <summary>
        /// The parameters provided when launching this process.
        /// </summary>
        private static RunParameters runParameters;

        #endregion

        #region Nested Class

        /// <summary>
        /// This class stores the result of parsing command line arguments.
        /// </summary>
        private class RunParameters
        {
            /// <summary>
            /// Gets the directory to write system error and report logs.
            /// </summary>
            public string CaptureDirectory { get; set; }

            /// <summary>
            /// Gets the directory to write report logs for development.
            /// </summary>
            public string DevelopmentLogDirectory { get; set; }

            /// <summary>
            /// Gets the flag indicating whether it is a development environment.
            /// </summary>
            public bool IsDevelopment
            {
                get { return !string.IsNullOrEmpty(DevelopmentLogDirectory); }
            }

            /// <summary>
            /// Gets the assembly name of the game report object.
            /// </summary>
            public string AssemblyName { get; set; }

            /// <summary>
            /// Gets the type name of the game report object.
            /// </summary>
            public string ReportTypeName { get; set; }

            /// <summary>
            /// Whether game reporting is in standalone mode.
            /// </summary>
            public bool IsStandalone { get; set; }

            /// <summary>
            /// Used to resolve referenced assembly paths.
            /// </summary>
            public List<string> ReferencePaths { get; set; }
        }

        #endregion

        #region Methods

        public static int Main(string[] args)
        {
            int exitCode;

            try
            {
                // Parse command line arguments.
                runParameters = ParseCommandLineArguments(args);

                if(runParameters.ReferencePaths != null && runParameters.ReferencePaths.Any())
                {
                    // Resolve the assembly references from the custom paths.
                    AppDomain.CurrentDomain.AssemblyResolve += HandleAssemblyResolve;
                }
                currentAssemblyName = runParameters.AssemblyName;

                // Initialize the file paths.
                systemErrorFilePath = Path.Combine(runParameters.CaptureDirectory, SystemErrorFileName);
                reportLogDirectory = runParameters.CaptureDirectory;

                // Development mode only.
                if(runParameters.IsDevelopment)
                { 
                    // Create the directory if needed.
                    // Files won't be created if the directory does not exist.
                    if(!Directory.Exists(runParameters.DevelopmentLogDirectory))
                    {
                        Directory.CreateDirectory(runParameters.DevelopmentLogDirectory);
                    }

                    // Write any debug logs to the development directory so it isn't cleaned up by the Foundation
                    reportLogDirectory = runParameters.DevelopmentLogDirectory;
                }

                // Instantiate game report object.
                var handle = Activator.CreateInstance(runParameters.AssemblyName, runParameters.ReportTypeName);
                var gameReportObject = handle.Unwrap();
                var gameReportType = gameReportObject.GetType();

                var methodInfo = gameReportType.GetMethod("Run");

                if(methodInfo == null)
                {
                    throw new ApplicationException(
                        string.Format("Run method is not available in the type {0} in the assembly {1}.",
                                      runParameters.ReportTypeName, runParameters.AssemblyName));
                }

                var foundationTarget = ParseGameParametersFile(runParameters.IsStandalone
                                                                   ? StandaloneGameParametersConfigPath
                                                                   : GameParametersConfigPath);

                // Run game report object.
                methodInfo.Invoke(gameReportObject,
                                  new object[] { runParameters.IsStandalone, foundationTarget });

                // Return 0 if all went as planned.
                exitCode = 0;
            }
            catch(TargetInvocationException exception)
            {
                // If Run throws an exception, log only the inner exception.
                WriteSystemError(exception.InnerException);
                exitCode = 1;
            }
            catch(Exception exception)
            {
                WriteSystemError(exception);
                exitCode = 1;
            }
            
            return exitCode;
        }

        /// <summary>
        /// Try searching and loading the referenced assemblies from the custom paths.
        /// </summary>
        /// <param name="sender">The executable assembly.</param>
        /// <param name="args">The event payloads.</param>
        /// <returns>The resolved assembly; Return null if the assembly reference couldn't be resolved.</returns>
        private static Assembly HandleAssemblyResolve(object sender, ResolveEventArgs args)
        {
            foreach(var referencePath in runParameters.ReferencePaths)
            {
                var files = Directory.GetFiles(referencePath, new AssemblyName(args.Name).Name + ".dll",
                    SearchOption.AllDirectories);

                if(files.Any())
                {
                    var asm = Assembly.LoadFrom(files.First());
                    return asm;
                }
            }

            return null;
        }

        /// <summary>
        /// Parses command line arguments.
        /// </summary>
        /// <param name="args">Command line arguments to pars.</param>
        /// <returns>A data structure storing the results.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when any required argument is missing.
        /// </exception>
        private static RunParameters ParseCommandLineArguments(string[] args)
        {
            if(args == null || args.Length == 0)
            {
                throw new InvalidOperationException("No command line arguments are specified.");
            }

            // Parse command line arguments into list of tuples.
            var tuples = new Dictionary<string, string>(args.Length);

            foreach(var arg in args)
            {
                if(arg.StartsWith(GameArgPrefix) && arg.Length > 3)
                {
                    // Game side arguments have prefixes with an index, e.g. "-g0" and "-g1".
                    tuples[arg.Substring(0, 3)] = arg.Substring(3);
                }
                else if(arg.StartsWith(FoundationArgPrefix) && arg.Length > 2)
                {
                    // Foundation side arguments have prefixes like "-c" and "-d".
                    tuples[arg.Substring(0, 2)] = arg.Substring(2);
                }
            }

            // Catch the C flag argument.  Default to current directory.
            var captureDirectory = tuples.ContainsKey("-c") ? tuples["-c"] : Directory.GetCurrentDirectory();

            // Catch the D flag argument.
            var developmentDirectory = tuples.ContainsKey("-d") ? tuples["-d"] : null;

            // Catch all of the G flag arguments.
            var assemblyName = tuples.ContainsKey("-g0") ? tuples["-g0"] : null;
            var reportTypeName = tuples.ContainsKey("-g1") ? tuples["-g1"] : null;
            var referencePathString = tuples.ContainsKey("-g3") ? tuples["-g3"] : null;
            List<string> referencePaths = null;
            if(!string.IsNullOrWhiteSpace(referencePathString))
            {
                //unquote the parameter if any
                referencePaths = referencePathString.Replace("\"", string.Empty)
                    .Split(';').ToList();
            }

            if(string.IsNullOrEmpty(assemblyName) || string.IsNullOrEmpty(reportTypeName))
            {
                throw new InvalidOperationException(
                    string.Format("Failed to retrieve game report assembly or type name from command line: {0}",
                                  string.Join(" ", args)));
            }

            var isStandalone = tuples.Any(tuple => tuple.Key.StartsWith(GameArgPrefix) && tuple.Value == "Standalone");

            return new RunParameters
                       {
                           CaptureDirectory = captureDirectory,
                           DevelopmentLogDirectory = developmentDirectory,
                           AssemblyName = assemblyName,
                           ReportTypeName = reportTypeName,
                           IsStandalone = isStandalone,
                           ReferencePaths = referencePaths
                       };
        }

        /// <summary>
        /// Parses the game parameters config file for the foundation target.
        /// </summary>
        /// <param name="gameParametersConfigPath">Path to the game parameters config file
        /// relative to the GameReportActivator location.</param>
        /// <returns>SDK FoundationTarget enum value.</returns>
        /// <exception cref="DirectoryNotFoundException">
        /// Thrown if getting the path to the GameReportActivator fails.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Thrown if the game parameters file cannot be found.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the game parameters file doesn't contain a
        /// targeted foundation entry.
        /// </exception>
        private static string ParseGameParametersFile(string gameParametersConfigPath)
        {
            string parametersFoundationTarget = null;

            // Load GameParameters file relative to the GameReportActivator.exe
            var reportActivatorPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);

            if(string.IsNullOrEmpty(reportActivatorPath) ||
               !Directory.Exists(reportActivatorPath))
            {
                throw new DirectoryNotFoundException(
                    string.Format("Cannot find path to GameReportActivator: '{0}'.",
                                  reportActivatorPath));
            }

            var gameParametersFileName = Path.Combine(reportActivatorPath, gameParametersConfigPath);

            if(!File.Exists(gameParametersFileName))
            {
                throw new FileNotFoundException(
                    string.Format("Cannot find the file: '{0}'.",
                                  gameParametersFileName));
            }

            using(var reader = new StreamReader(gameParametersFileName))
            {
                while(!reader.EndOfStream)
                {
                    string input = reader.ReadLine();
                    if(!string.IsNullOrEmpty(input))
                    {
                        var match = Regex.Match(input, TargetedFoundationPattern);
                        if(match.Success)
                        {
                            parametersFoundationTarget = match.Groups[FoundationGroupName].Value;
                            break;
                        }
                    }
                }
            }
            
            if(string.IsNullOrEmpty(parametersFoundationTarget))
            {
                throw new InvalidOperationException("Cannot locate Foundation Target in Game Parameters file.");
            }

            return parametersFoundationTarget;
        }

        /// <summary>
        /// Writes the caught exception to the system error and log files.
        /// </summary>
        /// <param name="exception">The exception thrown to write out.</param>
        private static void WriteSystemError(Exception exception)
        {
            if(systemErrorFilePath == null || exception == null)
            {
                return;
            }

            var details = exception.ToString();

            // Write system error.
            using(var systemErrorWriter = new StreamWriter(systemErrorFilePath))
            {
                systemErrorWriter.Write(details.Substring(0,
                                                          Math.Min(details.Length, SystemErrorMaxLength)));
                systemErrorWriter.Flush();
                systemErrorWriter.Close();
            }

            // Write complete exception to the log file.
            if(reportLogDirectory == null)
            {
                return;
            }
            
            var currentFileName = GetReportLogFileName(reportLogDirectory,
                                                       string.Format("{0}{1}*.txt",
                                                                     currentAssemblyName,
                                                                     LogFileNameSuffix));

            var currentFilePath = Path.Combine(reportLogDirectory, currentFileName);

            using(var reportLogFileWriter = new StreamWriter(currentFilePath, true) { AutoFlush = true })
            {
                reportLogFileWriter.WriteLine(details);
            }
        }

        /// <summary>
        /// Gets the name of the log file that logs will be written into.
        /// </summary>
        /// <param name="fileLocation">The location to get the log file name.</param>
        /// <param name="filePattern">The pattern of the file names to search.</param>
        /// <returns>The name of the file that logs will be written into.</returns>
        private static string GetReportLogFileName(string fileLocation, string filePattern)
        {
            var directoryInfo = new DirectoryInfo(fileLocation);
            var fileInfoList = new List<FileInfo>(directoryInfo.GetFiles(filePattern));

            var logFileName = (from fileInfo in fileInfoList
                               orderby fileInfo.LastWriteTimeUtc descending
                               select fileInfo.Name).FirstOrDefault();
            
            return !string.IsNullOrEmpty(logFileName)
                ? logFileName
                : string.Format("{0}{1}{2}.txt", currentAssemblyName, LogFileNameSuffix, 1);
        }

        #endregion
    }
}