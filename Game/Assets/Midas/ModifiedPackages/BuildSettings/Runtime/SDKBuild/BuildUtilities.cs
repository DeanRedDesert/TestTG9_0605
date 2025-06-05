// -----------------------------------------------------------------------
//  <copyright file = "BuildUtilities.cs" company = "IGT">
//      Copyright (c) 2017 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.SDKBuild
{
    using System.IO;
    using UnityEngine;

    /// <summary>
    /// Contains utility methods related to the build process.
    /// </summary>
    public static class BuildUtilities
    {
        /// <summary>
        /// Get the "root directory" of the application (e.g. where the player exists).
        /// </summary>
        public static string GetApplicationPath()
        {
            // This works for two reasons:
            // 1) All build targets place the data directory inside of the same directory as the player,
            // 2) The data path never includes a trailing "/"; GetDirectoryName will return its parent
            //    directory instead of itself.
            return Path.GetDirectoryName(Application.dataPath);
        }

        /// <summary>
        /// Clears the read only flag from the specified file if it exists and is read only.
        /// </summary>
        /// <param name="filePath">Full path to the file.</param>
        public static void ClearReadOnlyFlag(string filePath)
        {
            if(!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                if(fileInfo.IsReadOnly)
                {
                    Debug.Log("Clearing read only flag on " + filePath);
                    fileInfo.IsReadOnly = false;
                }
            }
        }

        /// <summary>
        /// Copies the entire folder specified by <paramref name="relativePath"/> 
        /// from <paramref name="projectDirectory"/> to <paramref name="buildDirectory"/>. 
        /// </summary>
        /// <param name="projectDirectory">Project directory to copy from.</param>
        /// <param name="buildDirectory">Output build directory to copy to.</param>
        /// <param name="relativePath">Path relative to the project and build directories that will be copied.</param>
        /// <remarks>
        /// Any missing directories or files will be logged as errors but will not break execution.
        /// </remarks>
        public static void CopyBuildDirectory(string projectDirectory, string buildDirectory, string relativePath)
        {
            if(!string.IsNullOrEmpty(projectDirectory) &&
               !string.IsNullOrEmpty(buildDirectory) &&
               !string.IsNullOrEmpty(relativePath))
            {
                // Normalize directory characters.
                projectDirectory = Path.GetFullPath(projectDirectory);
                buildDirectory = Path.GetFullPath(buildDirectory);

                // Only copy between different folders.
                if(projectDirectory != buildDirectory)
                {
                    var source = Path.Combine(projectDirectory, relativePath);
                    var destination = Path.Combine(buildDirectory, relativePath);

                    try
                    {
                        CopyDirectory(source, destination);
                    }
                    catch(FileNotFoundException e)
                    {
                        Debug.LogError(string.Format("Could not locate the following file: '{0}'", e.FileName));
                    }
                    catch(DirectoryNotFoundException e)
                    {
                        Debug.LogError(string.Format("Could not locate the following directory: '{0}'", e.Message));
                    }
                }
            }
        }

        /// <summary>
        /// Copies the file specified by <paramref name="relativeFileName"/> 
        /// from <paramref name="projectDirectory"/> to <paramref name="buildDirectory"/>. 
        /// </summary>
        /// <param name="projectDirectory">Project directory to copy from.</param>
        /// <param name="buildDirectory">Output build directory to copy to.</param>
        /// <param name="relativeFileName">File name path relative to the project and build directories
        /// that will be copied.</param>
        /// <remarks>
        /// Any missing directories or files will be logged as errors but will not break execution.
        /// </remarks>
        public static void CopyBuildFile(string projectDirectory, string buildDirectory, string relativeFileName)
        {
            if(!string.IsNullOrEmpty(projectDirectory) &&
               !string.IsNullOrEmpty(buildDirectory) &&
               !string.IsNullOrEmpty(relativeFileName))
            {
                // Normalize directory characters.
                projectDirectory = Path.GetFullPath(projectDirectory);
                buildDirectory = Path.GetFullPath(buildDirectory);

                // Only copy between different folders.
                if(projectDirectory != buildDirectory)
                {
                    var sourceFileName = Path.Combine(projectDirectory, relativeFileName);
                    var destinationFileName = Path.Combine(buildDirectory, relativeFileName);

                    var destinationDirectory = Path.GetDirectoryName(destinationFileName);

                    if(!string.IsNullOrEmpty(destinationDirectory))
                    {
                        if(!Directory.Exists(destinationDirectory))
                        {
                            Directory.CreateDirectory(destinationDirectory);
                        }

                        ClearReadOnlyFlag(destinationFileName);

                        try
                        {
                            File.Copy(sourceFileName, destinationFileName, true);
                        }
                        catch(FileNotFoundException e)
                        {
                            Debug.LogError(string.Format("Could not locate the following file: '{0}'", e.FileName));
                        }
                        catch(DirectoryNotFoundException e)
                        {
                            Debug.LogError(string.Format("Could not locate the following directory: '{0}'", e.Message));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Copies a directory and all of its files from one location to another
        /// and recursively copies its subdirectories.
        /// </summary>
        /// <param name="source">Full path to the source directory.</param>
        /// <param name="destination">Full path to the destination directory.</param>
        /// <exception cref="DirectoryNotFoundException">
        /// Thrown if the directory specified by <paramref name="source"/> does not exist.
        /// </exception>
        private static void CopyDirectory(string source, string destination)
        {
            var sourceDir = new DirectoryInfo(source);
            if(!sourceDir.Exists)
            {
                throw new DirectoryNotFoundException(source);
            }

            if(!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            foreach(var sourceFile in sourceDir.GetFiles())
            {
                var destinationFileName = Path.Combine(destination, sourceFile.Name);

                ClearReadOnlyFlag(destinationFileName);
                sourceFile.CopyTo(destinationFileName, true);
            }

            foreach(var dir in sourceDir.GetDirectories())
            {
                var path = Path.Combine(destination, dir.Name);
                CopyDirectory(dir.FullName, path);
            }
        }
    }
}