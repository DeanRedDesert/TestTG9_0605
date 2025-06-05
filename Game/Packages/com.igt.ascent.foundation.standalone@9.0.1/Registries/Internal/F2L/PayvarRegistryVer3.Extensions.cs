//-----------------------------------------------------------------------
// <copyright file = "PayvarRegistryVer3.Extensions.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// ReSharper disable CheckNamespace
namespace IGT.Game.Core.Registries.Internal.F2L.F2LPayvarRegistryVer3
{
    using System.IO;
    using System.Xml.Serialization;
    using Schemas.Serializers;

    /// <summary>
    /// Extensions to the PayvarRegistry class.
    /// </summary>
    public partial class PayvarRegistry
    {
        /// <summary>
        /// Object for parsing registry files.
        /// </summary>
        private static readonly XmlSerializer RegistryFileSerializer = new XmlSerializerContract().GetSerializer(typeof(PayvarRegistry));
         // new XmlSerializer(typeof(PayvarRegistry));
        /// <summary>
        /// Load an xml "Payvar" registry file, with an extension of usually : .xpayvarreg.
        /// </summary>
        /// <param name="registryFile">
        /// The name of the xml registry file, including the absolute
        /// or relative path, and the full file name with extension.
        /// </param>
        /// <returns>Registry object created from registry Xml.</returns>
        public static PayvarRegistry Load(string registryFile)
        {
            PayvarRegistry result;

            using (var registryStream = new FileStream(registryFile, FileMode.Open, FileAccess.Read))
            {
                result = RegistryFileSerializer.Deserialize(registryStream) as PayvarRegistry;
            }

            return result;
        }

        /// <summary>
        /// Gets the win level information for a specific win level index.
        /// </summary>
        /// <param name="winLevelIndex">The win level index to retrieve.</param>
        /// <returns>The win level information or null if the win level is not valid.</returns>
        public PayvarRegistryWinLevel GetWinLevel(int winLevelIndex)
        {
            PayvarRegistryWinLevel result = null;

            if(WinLevels != null && winLevelIndex >= 0 && winLevelIndex < WinLevels.Count)
            {
                result = WinLevels[winLevelIndex];
            }

            return result;
        }
    }
}
