//-----------------------------------------------------------------------
// <copyright file = "SystemRequirements.Extensions.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.ToolSupport.Schemas.SystemRequirements.Ver1
{
    using System.IO;
    using System.Xml.Serialization;

    /// <summary>
    /// Extensions to the SystemRequirements class.
    /// </summary>
    public partial class SystemRequirements
    {
        /// <summary>
        /// Object for parsing game system requirements files.
        /// </summary>
        private static XmlSerializer gameSysReqsFileSerializer = new XmlSerializer(typeof(SystemRequirements));

        /// <summary>
        /// Load an xml "SystemRequirements" file, with an extension of usually : .xrequirements.
        /// </summary>
        /// <param name="gameSysReqsFile">
        /// The full path name of the game system requirements file.
        /// </param>
        /// <returns>Object created from the underlying Xml.</returns>
        public static SystemRequirements Load(string gameSysReqsFile)
        {
            SystemRequirements result;

            using(var gameSysReqsStream = new FileStream(gameSysReqsFile, FileMode.Open, FileAccess.Read))
            {
                result = gameSysReqsFileSerializer.Deserialize(gameSysReqsStream) as SystemRequirements;
            }

            return result;
        }
    }
}
