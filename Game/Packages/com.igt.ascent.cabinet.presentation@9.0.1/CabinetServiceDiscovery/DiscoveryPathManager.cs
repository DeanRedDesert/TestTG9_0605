//-----------------------------------------------------------------------
// <copyright file = "DiscoveryPathManager.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.CabinetServiceDiscovery
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// This class manages resolving pathing to default specified MEF containing directories.
    /// </summary>
    internal static class DiscoveryPathManager
    {
        #region Constants

        /// <summary>
        /// The filename of the XML file containing relative directories (from the game mount point)
        /// that indicate where MEF assemblies are located.
        /// </summary>
        /// <remarks>
        /// The game should have a file with this filename in the game root directory, containing one or more
        /// relative directories to be scanned for MEF assemblies.
        /// </remarks>
        private const string DiscoveryPathFileVer1 = @"MefDiscoveryDirectories.xml";

        #endregion

        /// <summary>
        /// Gets the mount point of the game package.
        /// </summary>
        public static string GameMountPoint { private set; get; }

        /// <summary>
        /// Gets the list of MEF discovery directories.
        /// </summary>
        /// <param name="gameMountPoint">
        /// The game root directory as passed by a game lib or other lib.
        /// </param>
        /// <param name="mefDirectories">
        /// The list of full directory names as specified in the MEF config. file.
        /// </param>
        /// <returns>
        /// A collection of <see cref="DiscoveryResult"/> results from parsing the MEF config. file.
        /// </returns>
        public static IList<DiscoveryResult> GetMefDirectories(string gameMountPoint, out IEnumerable<string> mefDirectories)
        {
            GameMountPoint = gameMountPoint;
            mefDirectories = new List<string>();
            var mefConfigFileParseResults = new List<DiscoveryResult>();

            var fullPathToXmlConfigFile = Path.Combine(gameMountPoint, DiscoveryPathFileVer1);

            if(!File.Exists(fullPathToXmlConfigFile))
            {
                mefConfigFileParseResults.Add( new DiscoveryResult(DiscoveryResultType.Warning,
                    $@"The MEF config file ({fullPathToXmlConfigFile}) cannot be located."));
            }
            else
            {
                MefDiscoveryDirectories pluginDirectoryReader = null;

                try
                {
                    MefDiscoveryDirectories.LoadFromFile(fullPathToXmlConfigFile, out pluginDirectoryReader);
                }
                catch(Exception ex)
                {
                    mefConfigFileParseResults.Add(new DiscoveryResult(DiscoveryResultType.Warning,
                                                $@"The MEF configuration file ({ex.Message} cannot be opened or contains malformed Xml. Exception: {ex.Message}"));
                }

                if(pluginDirectoryReader == null)
                {
                    mefConfigFileParseResults.Add(new DiscoveryResult(DiscoveryResultType.Warning,
                                            @"There was an internal issue constructing the MEF configuration file reader. " + 
                                                    "Check the file's full path and ensure the contents are not malformed."));
                }
                else
                {
                    foreach(var directory in pluginDirectoryReader.MefDirectories)
                    {
                        var fullPath = Path.GetFullPath(gameMountPoint + directory);
                        if(Directory.Exists(fullPath))
                        {
                            ((List<string>)mefDirectories).Add(fullPath);
                        }
                    }

                    mefConfigFileParseResults.Add(new DiscoveryResult(DiscoveryResultType.Success,
                        $@"MEF config file successfully loaded and parsed with {((List<string>)mefDirectories).Count} directories found."));
                }
            }

            return mefConfigFileParseResults;
        }
    }
}
