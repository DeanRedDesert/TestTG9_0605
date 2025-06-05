//-----------------------------------------------------------------------
// <copyright file = "MonoAOT.cs" company = "IGT">
//     Copyright(c) 2020 IGT.All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.MonoAOT.Editor
{
    using AscentBuildSettings.Editor;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;
    using Debug = UnityEngine.Debug;

    /// <summary>
    /// Class to handle Unity post build processing.
    /// </summary>
    internal class MonoAot : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        private bool _monoAotCompile;
        private string _monoExePath;
        
        #region IPreprocessBuildWithReport and IPostProcessBuildWithReport Implementation

        /// <inheritdoc/>
        public void OnPreprocessBuild(BuildReport report)
        {
            // Temporarily disable AOT due to the bug https://cspjira.igt.com/jira/browse/AS-9556
            // Same measure applied to IGTEGMBuildCustomInspector.cs
            _monoAotCompile = false;
            // _monoAotCompile = IGTEGMSettings.IgtEgmBuildSettings.MonoAoTCompile;

            if(!_monoAotCompile)
            {
                return;
            }

            // Locate LLVM 10.0.0
            if(LlvmLocator.LocateVersion(new Version(10, 0, 0), out var installedPath, out var needAddToSystemPath))
            {
                if(needAddToSystemPath)
                {
                    Debug.Log($"MonoAOT: Found LLVM 10.0.0 at {installedPath}.");
                    var pathString = Environment.GetEnvironmentVariable("PATH");
                    var pathList = pathString?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList() ??
                                   new List<string>();
                    pathList.RemoveAll(p => string.Equals(installedPath, p, StringComparison.OrdinalIgnoreCase));
                    pathList.Insert(0, installedPath);
                    pathString = pathList.Aggregate((accum, next) => accum + ";" + next);
                    Environment.SetEnvironmentVariable("PATH", pathString);
                }
                else
                {
                    Debug.Log($"MonoAOT: Found LLVM 10.0.0 from system PATH.");
                }
            }
            else
            {
                _monoAotCompile = false;
                const string message = "MonoAOT: LLVM 10.0.0 was not found. The installation file is in Perforce at \n" +
                                       "MonoAOT: (//Unity/Unity/Tools/LLVM10.0.0) or at (https://releases.llvm.org/download.html#10.0.0) .\n" +
                                       "MonoAOT: In order to complete AOT compilation, please install LLVM 10.0.0 and make sure LLVM bin is added to the system PATH.";
                Debug.LogWarning(message);
                return;
            }

            // path of the mono executable stored in the Unity application used to compile AOT
            // starting with Unity 2018.1.2 the x64 architectures are stored with the Unity installation
            var unityInstallationDirectory = Path.GetDirectoryName(EditorApplication.applicationPath);
            _monoExePath = Path.Combine(unityInstallationDirectory, @"Data\MonoBleedingEdge\bin-x64\mono-bdwgc.exe");
            if(!File.Exists(_monoExePath))
            {
                _monoAotCompile = false;
                const string message = "MonoAOT: Mono x64 launcher/runtime not found in the Unity installation.\n" +
                                       "MonoAOT: The x64 binaries were included in Unity 2018.1.2 and later versions.";
                                       Debug.LogWarning(message);
                return;
            }
            
            if(!Application.isBatchMode &&
               !IGTEGMSettings.IgtEgmBuildSettings.Release)
            {
                var selection = EditorUtility.DisplayDialogComplex("Mono AOT Compile",
                    "Mono AOT Compile is enabled for non-release build. The build may take several minutes longer to complete.\n" +
                    "Are you sure to continue with Mono AOT Compile?",
                    "Yes", "Disable Mono AOT Compile", "Skip Mono AOT Compile Once");
                if(selection == 1)
                {
                    IGTEGMSettings.IgtEgmBuildSettings.MonoAoTCompile = false;
                    _monoAotCompile = false;
                }
                else if(selection == 2)
                {
                    _monoAotCompile = false;
                }
            }
        }

        /// <inheritdoc/>
        // ReSharper disable once UnusedMember.Global  
        public void OnPostprocessBuild(BuildReport report)
        {
            if(!_monoAotCompile)
            {
                Debug.Log( "MonoAOT: Opt out Mono AOT compilation.");
                return;
            }
            
            // the game's managed folder
            var outputDir = Path.GetDirectoryName(report.summary.outputPath);
            var outputFileName = Path.GetFileNameWithoutExtension(report.summary.outputPath);
            var managedFolderPath = Path.Combine(outputDir, outputFileName + @"_Data\Managed");
            if(!Directory.Exists(managedFolderPath))
            {
                var message = $"MonoAOT: The game's managed folder was not found. Unable to locate {managedFolderPath}.";
                Debug.LogWarning(message);
                return;
            }

            // Starting from Visual Studio 2017, vswhere is the new tool to locate Visual Studio products(including build tools).
            // vswhere is installed at %ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe
            var batFileName = (FileUtil.GetUniqueTempPathInProject() + ".bat")
                .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            Debug.Log("MonoAOT: Starting Mono AOT compilation");
            var stopwatch = Stopwatch.StartNew();
            try
            {
                using(var batFile = new StreamWriter(batFileName))
                {
                    // How to find-VC: https://github.com/microsoft/vswhere/wiki/Find-VC
                    batFile.WriteLine("@echo off");
                    batFile.WriteLine(@"set vswhere=""%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe""");
                    batFile.WriteLine(@"for /f ""usebackq delims="" %%i in (`%vswhere% -latest -prerelease -version [16^,^)" +
                                      " -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property" +
                                      " installationPath`) do (");
                    batFile.WriteLine(@"  if exist ""%%i\Common7\Tools\vsdevcmd.bat"" (");
                    batFile.WriteLine(@"    call ""%%i\Common7\Tools\vsdevcmd.bat""");
                    batFile.WriteLine(@"    goto Build");
                    batFile.WriteLine(@"  )");
                    batFile.WriteLine(@")");

                    batFile.WriteLine(@"echo Visual Studio C++ build tools 2017 or newer version were not found. >&2");
                    batFile.WriteLine(@"exit /b 2");
                    batFile.WriteLine(@":Build");
                    batFile.WriteLine($"set MONO_PATH={managedFolderPath}");
                    batFile.WriteLine("cd %MONO_PATH%");
                    batFile.WriteLine($"for %%f in (\"*.dll\") do ( \"{_monoExePath}\" --aot=nodebug -O=all \"%%f\" )");
                    batFile.WriteLine($"for %%f in (\"*.exe\") do ( \"{_monoExePath}\" --aot=nodebug -O=all \"%%f\" )");
                    batFile.WriteLine("exit /b %ERRORLEVEL%");
                }

                var (exitCode, aotOutput, aotError) = ProcessRunner.RunAndGetOutput("cmd.exe", "/c " + batFileName);
                if(exitCode != 0)
                {
                    Debug.LogError("MonoAOT: AOT compilation did not fully complete.");
                    PrintErrorMessages(exitCode, aotOutput, aotError);
                }
                else
                {
                    if(!string.IsNullOrWhiteSpace(aotOutput))
                    {
                        Debug.Log("MonoAOT: " + aotOutput);
                    }

                    if(!string.IsNullOrWhiteSpace(aotError))
                    {
                        Debug.Log("MonoAOT: " + aotError);
                    }
                }
            }
            finally
            {
                File.Delete(batFileName);
            }

            stopwatch.Stop();
            Debug.Log($@"MonoAOT: Completed Mono AOT compilation in {stopwatch.Elapsed.TotalSeconds} seconds");
        }

        /// <inheritdoc/>
        public int callbackOrder => 1000;

        #endregion
        
        /// <summary />
        private static void PrintErrorMessages(int exitCode, string output, string errorOutput)
        {
            if(!string.IsNullOrWhiteSpace(errorOutput))
            {
                Debug.LogError("MonoAOT: " + errorOutput);
            }

            if(exitCode != 0 && !string.IsNullOrWhiteSpace(output))
            {
                // Print the full log in error condition for troubleshooting purpose.
                Debug.LogError("MonoAOT: " + output);
            }
        }
    }
}
