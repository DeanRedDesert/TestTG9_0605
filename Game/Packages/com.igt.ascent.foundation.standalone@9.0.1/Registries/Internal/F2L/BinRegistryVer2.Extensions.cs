//-----------------------------------------------------------------------
// <copyright file = "BinRegistryVer2.Extensions.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// ReSharper disable CheckNamespace
namespace IGT.Game.Core.Registries.Internal.F2L.F2LBinRegistryVer2
{
    using System.IO;
    using System.Xml.Serialization;

    /// <summary>
    /// Extensions to the BinRegistry class.
    /// </summary>
    public partial class BinRegistry
    {
        /// <summary>
        /// Object for parsing reg files.
        /// </summary>
        private static readonly XmlSerializer RegistryFileSerializer = new XmlSerializer(typeof(BinRegistry));

        /// <summary>
        /// Load an xml "Bin" registry file, with an extension of usually : .xbinreg.
        /// </summary>
        /// <param name="registryFile">
        /// The name of the xml registry file, including the absolute
        /// or relative path, and the full file name with extension.
        /// </param>
        /// <returns>Registry object created from registry Xml.</returns>
        public static BinRegistry Load(string registryFile)
        {
            BinRegistry result;

            using (var registryStream = new FileStream(registryFile, FileMode.Open, FileAccess.Read))
            {
                result = RegistryFileSerializer.Deserialize(registryStream) as BinRegistry;
            }

            return result;
        }
    }
}
