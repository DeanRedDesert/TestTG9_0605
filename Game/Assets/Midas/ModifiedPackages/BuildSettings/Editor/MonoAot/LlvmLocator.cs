// -----------------------------------------------------------------------
//  <copyright file = "LlvmLocator.cs" company = "IGT">
//      Copyright (c) 2022 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.MonoAOT.Editor
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    /// This class locates the LLVM installation on the machine. 
    /// </summary>
    internal static class LlvmLocator
    {
        /// <summary />
        public static bool LocateVersion(Version version, out string installedPath, out bool needAddToSystemPath)
        {
            installedPath = null;
            needAddToSystemPath = false;
            if(TryLoadVersion("llvm-nm.exe", out var readVersion) && readVersion == version)
            {
                return true;
            }

            needAddToSystemPath = true;
            var (exitCode, stdOutput, _) = ProcessRunner.RunAndGetOutput("where.exe", "llvm-nm.exe");
            if(exitCode == 0)
            {
                var paths = stdOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach(var path in paths)
                {
                    if(TryLoadVersion(path, out readVersion) && readVersion == version)
                    {
                        installedPath = Path.GetDirectoryName(path);
                        return true;
                    }
                }
            }

            // Searching C:\ root for LLVM installations.
            installedPath = @$"C:\LLVM{version}\bin";
            var fallbackPath = Path.Combine(installedPath, "llvm-nm.exe");
            return File.Exists(fallbackPath) && 
                   TryLoadVersion(fallbackPath, out readVersion) &&
                   readVersion == version;
        }

        /// <summary />
        private static bool TryLoadVersion(string executablePath, out Version version)
        {
            version = default;
            var (exitCode, stdOutput, _) = ProcessRunner.RunAndGetOutput(executablePath, "--version");
            if(exitCode != 0)
            {
                return false;
            }
            
            const string pattern = @"^\s*LLVM version (?<version>\d+\.\d+\.\d+)$";
            var match = Regex.Match(stdOutput, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            return match.Success && Version.TryParse(match.Groups["version"].Value, out version);
        }
    }
}