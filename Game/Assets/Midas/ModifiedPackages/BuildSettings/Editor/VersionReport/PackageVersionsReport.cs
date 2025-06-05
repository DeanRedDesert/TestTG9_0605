// -----------------------------------------------------------------------
//  <copyright file = "PackageVersionsReport.cs" company = "IGT">
//      Copyright (c) 2023 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.AscentBuildSettings.Editor
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Newtonsoft.Json;
    using UnityEditor.Build.Reporting;
    using UnityEditor.PackageManager;
    using UnityEngine;

    /// <summary>
    /// This class generates package versions report during build process.
    /// </summary>
    public static class PackageVersionsReport
    {
        /// <summary>
        /// Process the versions report.
        /// </summary>
        public static void BuildReport(BuildSummary report)
        {
            var reportPath = Path.Combine(Path.GetDirectoryName(report.outputPath), "PackageVersions.json");
            GenerateVersionReport(reportPath);
        }
        
        /// <summary>
        /// Generate the package versions report at the specified path.
        /// </summary>
        /// <param name="targetPath">The report file path.</param>
        private static void GenerateVersionReport(string targetPath)
        {
            var request = Client.List(true, true);

            var start = DateTime.Now;
            Debug.Log($"Package Versions Processor: ListRequest started on {start:G}. Waiting for completion.");
            while(!request.IsCompleted)
            {
                Thread.Sleep(100);
            }

            var elapse = DateTime.Now - start;
            Debug.Log($"Package Versions Processor: ListRequest completed in {elapse.TotalSeconds} seconds.");
            var packages = request.Result.Select(package =>
                new PackageVersion
                {
                    Package = package.name,
                    Version = package.version
                })
                .OrderBy(package => package.Package)
                .ToList();

            var reportString = JsonConvert.SerializeObject(packages, Formatting.Indented);
            File.WriteAllText(targetPath, reportString);
        }
    }
}