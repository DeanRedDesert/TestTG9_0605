// -----------------------------------------------------------------------
// <copyright file = "LocalizationTable.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using CompactSerialization;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This class contains localization table data.
    /// </summary>
    [Serializable]
    public class LocalizationTable : ICompactSerializable
    {
        #region Properties

        /// <summary>
        /// The locale of the resources.
        /// </summary>
        public string Locale { get; private set; }

        /// <summary>
        /// The collection of localized resources in the specified culture.
        /// </summary>
        public Dictionary<string, LocalizedResource> LocalizedResources { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates an instance of <see cref="LocalizationTable"/>.
        /// </summary>
        /// <param name="locale">The specified locale.</param>
        /// <param name="localizedResources">
        /// Localized resource list for the specified locale.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when either <paramref name="locale"/> or <paramref name="localizedResources"/> is null or empty.
        /// </exception>
        public LocalizationTable(string locale, Dictionary<string, LocalizedResource> localizedResources)
        {
            if(string.IsNullOrEmpty(locale))
            {
                throw new ArgumentException("The locale can not be null or empty.");
            }
            if(localizedResources == null || !localizedResources.Any())
            {
                throw new ArgumentException("The localizedResources can not be null or empty.");
            }

            Locale = locale;
            LocalizedResources = localizedResources;
        }

        /// <summary>
        /// Instantiates an instance of <see cref="LocalizationTable"/>.
        /// This parameterless constructor is used for serialization purposes.
        /// </summary>
        public LocalizationTable()
        {
        }

        #endregion

        #region ToString

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("LocalizationTableData -");
            builder.AppendLine(string.Format("\t Locale({0})", Locale));
            foreach(var localizedResource in LocalizedResources)
            {
                builder.AppendLine(string.Format("\t Resource(ID({0}), Value({1}))",
                    localizedResource.Key, localizedResource.Value.ToString()));
            }

            return builder.ToString();
        }

        #endregion

        #region ICompactSerializable Implementation

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Serialize(stream, Locale);
            CompactSerializer.Serialize(stream, LocalizedResources);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            Locale = CompactSerializer.ReadString(stream);
            LocalizedResources = CompactSerializer.ReadDictionary<string, LocalizedResource>(stream);
        }

        #endregion
    }
}