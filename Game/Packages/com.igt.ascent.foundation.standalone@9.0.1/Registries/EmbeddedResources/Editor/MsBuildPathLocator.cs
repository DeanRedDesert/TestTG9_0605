// -----------------------------------------------------------------------
//  <copyright file = "MsBuildPathLocator.cs" company = "IGT">
//      Copyright (c) 2022 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace EmbeddedResources.Editor
{
    using System;
    using System.IO;
    using System.Text;
    using UnityEditor.Build;

    /// <summary>
    /// This class is used to locate the MSBuild tool.
    /// </summary>
    internal static class MsBuildPathLocator
    {
        private static string _msbuildExecutablePath;

        /// <summary />
        public static string GetPath()
        {
            if(_msbuildExecutablePath != null)
            {
                return _msbuildExecutablePath;
            }

            var logBuilder = new StringBuilder();

            var msbuildPath = Environment.GetEnvironmentVariable("MSBUILD_PATH");
            if(msbuildPath == null)
            {
                logBuilder.AppendLine("Environment variable 'MSBUILD_PATH' is not defined. Searching MSBuild installations on the machine...");
                
                var vswherePath = Environment.GetEnvironmentVariable("ProgramFiles(x86)") +
                                  "\\Microsoft Visual Studio\\Installer\\vswhere.exe";
                var vswhereArguments =
                    "-version [16,) -latest -prerelease -products * -requires Microsoft.Component.MSBuild" +
                    " -property installationPath";
                var (_, msbuildInstallationPath, errorLog) = ProcessRunner.RunAndGetOutput(vswherePath, vswhereArguments);

                msbuildPath = string.IsNullOrWhiteSpace(msbuildInstallationPath)
                    ? null
                    : Path.Combine(msbuildInstallationPath.Trim(), "MSBuild\\Current\\Bin\\MSBuild.exe");

                logBuilder.AppendLine(msbuildPath == null
                    ? "Searching completed and the MSBuild installation with version >=16.0 was not found."
                    : $"Searching completed and the MSBuild installation is found at '{msbuildPath}'.");

                if(!string.IsNullOrWhiteSpace(errorLog))
                {
                    logBuilder.AppendLine($"vswhere process error output: {errorLog}");
                }
            }
            else
            {
                logBuilder.AppendLine($"Environment variable 'MSBUILD_PATH' is defined as '{msbuildPath}'.");
            }

            if(msbuildPath == null || !File.Exists(msbuildPath))
            {
                throw new BuildFailedException(
                    "Cannot locate the MSBuild installation with version 16 or greater on this machine.\n"
                    + "Detailed Logs:\n" + logBuilder);
            }

            _msbuildExecutablePath = msbuildPath;
            return _msbuildExecutablePath;
        }
    }
}