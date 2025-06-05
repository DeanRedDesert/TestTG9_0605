// -----------------------------------------------------------------------
//  <copyright file = "EmbeddedSourcesBuilder.cs" company = "IGT">
//      Copyright (c) 2022 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------
// <auto-deployed>
//   This file is deployed by the UPM deployment tool.
// </auto-deployed>
// -----------------------------------------------------------------------

namespace EmbeddedResources.Editor
{
    using System;
    using System.Linq;
    using System.IO;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEngine;
    using Debug = UnityEngine.Debug;

    /// <summary>
    /// This class builds the resource files into a resource assembly.
    /// </summary>
    internal readonly struct EmbeddedSourcesBuilder
    {
        private readonly string _outputPath;

        /// <summary />
        public EmbeddedSourcesBuilder(string outputPath)
        {
            if(string.IsNullOrWhiteSpace(outputPath))
            {
                throw new ArgumentException("outputPath cannot be null or blank", nameof(outputPath));
            }
            
            _outputPath = outputPath;
        }

        /// <summary>
        /// Build the resource assembly into the target folder <see cref="_outputPath"/>.
        /// </summary>
        public void Build(bool forceRebuild = false)
        {
            FileUtil.DeleteFileOrDirectory(_outputPath);
            if(!forceRebuild)
            {
                var preBuiltAssemblyFiles = Directory.GetFiles(
                    ScriptPathLocator.GetEmbeddedResourcesAssemblyDir().NormalizePath(),
                    "*.dll");
                if(preBuiltAssemblyFiles.Any())
                {
                    Directory.CreateDirectory(_outputPath);
                    foreach(var file in preBuiltAssemblyFiles)
                    {
                        var copyToFile = Path.Combine(_outputPath, Path.GetFileName(file));
                        FileUtil.CopyFileOrDirectory(file, copyToFile);
                        new FileInfo(copyToFile.NormalizePath()).IsReadOnly = false;
                    }
                    return;
                }
            }

            var embeddedResourceBuildProject = Path.Combine(
                ScriptPathLocator.GetEmbeddedResourcesDir(),
                "EmbeddedResourcesBuild.proj");
            // Since the project path is passed to the commandline as an argument,
            // we double quote the path if it contains space characters.
            if(embeddedResourceBuildProject.Contains(" "))
            {
                embeddedResourceBuildProject = $@"""{embeddedResourceBuildProject}""";
            }

            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            // For MSBuild property values, we use "%20" to escape the space characters.
            // See https://docs.microsoft.com/en-us/visualstudio/msbuild/how-to-escape-special-characters-in-msbuild
            var intermediateOutputPath = Path.Combine(projectRoot, @"Obj\").Replace(" ", "%20");
            var outputPath = _outputPath.Replace(" ", "%20");

            // ReBuild
            var msbuildArguments = $"{embeddedResourceBuildProject} /t:ReBuild /p:OutputPath={outputPath}" +
                                   $" /p:IntermediateOutputPath={intermediateOutputPath} /p:DebugSymbols=false /p:DebugType=None";
            var (exitCode, output, errorOutput) =
                ProcessRunner.RunAndGetOutput(MsBuildPathLocator.GetPath(), msbuildArguments);
            PrintErrorMessages(exitCode, output, errorOutput);
            if(exitCode != 0)
            {
                throw new BuildFailedException("Failed to build embedded resources assembly.");
            }

            FileUtil.DeleteFileOrDirectory(intermediateOutputPath);
        }

        /// <summary />
        private static void PrintErrorMessages(int exitCode, string output, string errorOutput)
        {
            if(!string.IsNullOrWhiteSpace(errorOutput))
            {
                Debug.LogError(errorOutput);
            }

            if(exitCode != 0 && !string.IsNullOrWhiteSpace(output))
            {
                // Print the full log in error condition for troubleshooting purpose.
                Debug.LogError(output);
            }
        }
    }
}