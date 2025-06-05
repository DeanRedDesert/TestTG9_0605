// -----------------------------------------------------------------------
//  <copyright file = "LongPathResolver.cs" company = "IGT">
//      Copyright (c) 2023 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.SDK.GameReport.Editor
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// This class resolves long string paths to make them work with mono.
    /// </summary>
    internal static class LongPathResolver
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetShortPathName(
            [MarshalAs(UnmanagedType.LPTStr)] string path,
            [MarshalAs(UnmanagedType.LPTStr)] StringBuilder shortPath,
            int shortPathLength
        );

        /// <summary>
        /// Make the path work with C# APIs on mono runtime.
        /// </summary>
        /// <param name="path">The input path.</param>
        /// <returns>The normalized path.</returns>
        public static string NormalizePath(this string path)
        {
            var longPath = path.LongPath();
            if(File.Exists(longPath))
            {
                var shortPath = new StringBuilder(260);
                if(GetShortPathName(longPath, shortPath, shortPath.Capacity) == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                return shortPath.ToString().StripLongPathPrefix();
            }

            return path;
        }

        /// <summary>
        /// Gets the long path format of the input path.
        /// </summary>
        /// <param name="path">The input path string.</param>
        /// <returns>The long path format of the path string.</returns>
        private static string LongPath(this string path)
        {
            if(string.IsNullOrEmpty(path) || path.StartsWith(@"\\?\"))
            {
                return path;
            }

            path = Path.GetFullPath(path);
            if(path.Length > 260 || path.Contains(@"~"))
            {
                // http://msdn.microsoft.com/en-us/library/aa365247.aspx
                if(path.StartsWith(@"\\"))
                {
                    // UNC.
                    return @"\\?\UNC\" + path.Substring(2);
                }

                return @"\\?\" + path;
            }

            return path;
        }

        /// <summary>
        /// Strip the long path prefix to get the normal path string.
        /// </summary>
        /// <param name="path">The input path string.</param>
        /// <returns>The path string without the long path prefix.</returns>
        private static string StripLongPathPrefix(this string path)
        {
            if(string.IsNullOrEmpty(path) || !path.StartsWith(@"\\?\"))
            {
                return path;
            }

            if(path.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase))
            {
                return @"\\" + path.Substring(@"\\?\UNC\".Length);
            }

            return path.Substring(@"\\?\".Length);
        }
    }
}