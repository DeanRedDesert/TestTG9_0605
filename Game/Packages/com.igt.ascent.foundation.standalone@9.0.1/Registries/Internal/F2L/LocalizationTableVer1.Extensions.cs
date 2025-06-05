//-----------------------------------------------------------------------
// <copyright file = "LocalizationTableVer1.Extensions.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// ReSharper disable CheckNamespace
namespace IGT.Game.Core.Registries.Internal.F2L.F2LLocalizationTableVer1
{
    using System.IO;
    using System.Xml.Serialization;

    /// <summary>
    /// Extensions to the LocalizationTable class.
    /// </summary>
    public partial class LocalizationTable
    {
        /// <summary>
        /// Object for parsing registry/localization files.
        /// </summary>
        private static readonly XmlSerializer RegistryFileSerializer = new XmlSerializer(typeof(LocalizationTable));

        /// <summary>
        /// Load an xml "Theme Content" LocalizationTable file, with an extension of usually : .xlocalization.
        /// </summary>
        /// <param name="localizationFile">
        /// The complete path of the file to load.
        /// </param>
        /// <returns>Registry object created from registry Xml.</returns>
        public static LocalizationTable Load(string localizationFile)
        {
            LocalizationTable result;

            using(var registryStream = new FileStream(localizationFile, FileMode.Open, FileAccess.Read))
            {
                result = RegistryFileSerializer.Deserialize(registryStream) as LocalizationTable;
            }

            return result;
        }
    }
}
