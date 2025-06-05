//-----------------------------------------------------------------------
// <copyright file = "SifInformationVer1.Extensions.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// ReSharper disable CheckNamespace
namespace IGT.Game.Core.Registries.Internal.SifInformationVer1
{
    using System.IO;
    using System.Xml.Serialization;

    /// <summary>
    /// Extensions to the SifInformation class.
    /// </summary>
    public partial class SifInformation
    {
        /// <summary>
        /// Object for parsing sif files.
        /// </summary>
        private static readonly XmlSerializer SifInfoFileSerializer = new XmlSerializer(typeof(SifInformation));

        /// <summary>
        /// Load an xml "SifInformation" file, with an extension of usually : .xsifinformation.
        /// </summary>
        /// <param name="sifInfoFile">
        /// The name of the Sif Info., including the absolute or relative path, and the full file name with extension.
        /// </param>
        /// <returns>Object created from the underlying Xml.</returns>
        public static SifInformation Load(string sifInfoFile)
        {
            SifInformation result;

            using(var sifInfoStream = new FileStream(sifInfoFile, FileMode.Open, FileAccess.Read))
            {
                result = SifInfoFileSerializer.Deserialize(sifInfoStream) as SifInformation;
            }

            return result;
        }
    }
}
