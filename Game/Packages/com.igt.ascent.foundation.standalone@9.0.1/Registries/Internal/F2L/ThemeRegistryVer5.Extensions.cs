//-----------------------------------------------------------------------
// <copyright file = "ThemeRegistryVer5.Extensions.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// ReSharper disable CheckNamespace
namespace IGT.Game.Core.Registries.Internal.F2L.F2LThemeRegistryVer5
{
    using System.IO;
    using System.Xml.Serialization;

    /// <summary>
    /// Extensions to the ThemeRegistry class.
    /// </summary>
    public partial class ThemeRegistry
    {
        /// <summary>
        /// Object for parsing registry files.
        /// </summary>
        private static readonly XmlSerializer RegistryFileSerializer = new XmlSerializer(typeof(ThemeRegistry));

        /// <summary>
        /// Load an xml "Theme" registry file, with an extension of usually : .xthemereg.
        /// </summary>
        /// <param name="registryFile">
        /// The name of the xml registry file, including the absolute
        /// or relative path, and the full file name with extension.
        /// </param>
        /// <returns>Registry object created from registry Xml.</returns>
        public static ThemeRegistry Load(string registryFile)
        {
            ThemeRegistry result;

            using (var registryStream = new FileStream(registryFile, FileMode.Open, FileAccess.Read))
            {
                result = RegistryFileSerializer.Deserialize(registryStream) as ThemeRegistry;
            }

            return result;
        }
    }
}
