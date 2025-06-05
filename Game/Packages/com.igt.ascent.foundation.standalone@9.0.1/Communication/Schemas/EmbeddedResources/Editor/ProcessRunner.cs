// -----------------------------------------------------------------------
//  <copyright file = "ProcessRunner.cs" company = "IGT">
//      Copyright (c) 2022 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace EmbeddedResources.Editor
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// This class is used to run a process and optionally return the output.
    /// </summary>
    internal static class ProcessRunner
    {
        /// <summary>
        /// Runs the executable in a new process and returns the standard outputs in strings.
        /// </summary>
        /// <param name="executable">The executable to launch.</param>
        /// <param name="arguments">The commandline arguments for the new process.</param>
        /// <returns>A tuple of (exit code, standard output, error output).</returns>
        public static (int, string, string) RunAndGetOutput(string executable, string arguments)
        {
            try
            {
                // Don't check executable file existence. When the executable file is under system paths,
                // it can be executed without specifying the full path.
                var processStarInfo = new ProcessStartInfo(executable)
                {
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                var process = Process.Start(processStarInfo);
                var stdOut = string.Empty;
                var errOut = string.Empty;
                process.OutputDataReceived += (sender, args) => stdOut += args.Data + "\n";
                process.ErrorDataReceived += (sender, args) => errOut += args.Data + "\n";
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                return (process.ExitCode, stdOut, errOut);
            }
            catch(Exception exception)
            {
                // arbitrary exit number to indicate an exception
                return (-99, exception.Message, exception.ToString());
            }
        }
    }
}