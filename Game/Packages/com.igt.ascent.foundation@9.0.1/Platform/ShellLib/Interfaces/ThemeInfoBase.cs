// -----------------------------------------------------------------------
// <copyright file = "ThemeInfoBase.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using System.IO;
    using System.Text;
    using Game.Core.CompactSerialization;
    using Platform.Interfaces;

    /// <summary>
    /// Base class to hold a theme's information.
    /// </summary>
    [Serializable]
    public abstract class ThemeInfoBase : ICompactSerializable
    {
        #region Properties

        /// <summary>
        /// Gets the theme identifier of the theme.
        /// </summary>
        public IdentifierToken ThemeIdentifier { get; private set; }

        /// <summary>
        /// Gets the G2SThemeId defined in the theme registry.
        /// </summary>
        public string G2SThemeId { get; private set; }

        /// <summary>
        /// Gets the ThemeTag defined in the theme registry.
        /// </summary>
        public string ThemeTag { get; private set; }

        /// <summary>
        /// Gets the ThemeTagDataFile defined in the theme registry.
        /// </summary>
        public string ThemeTagDataFile { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for <see cref="ICompactSerializable"/>.
        /// </summary>
        protected ThemeInfoBase()
        {
            ThemeIdentifier = new IdentifierToken();
            G2SThemeId = string.Empty;
        }

        /// <summary>
        /// Constructor for creating a <see cref="ThemeInfoBase"/>.
        /// </summary>
        /// <param name="themeIdentifier">
        /// The theme's identifier.
        /// </param>
        /// <param name="g2SThemeId">
        /// The theme's G2SThemeId.
        /// </param>
        /// <param name="themeTag">
        /// The theme tag defined in the theme registry.
        /// </param>
        /// <param name="themeTagDataFile">
        /// The theme tag data file defined in the theme registry.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="themeIdentifier"/> or <paramref name="g2SThemeId"/> is null.
        /// </exception>
        protected ThemeInfoBase(IdentifierToken themeIdentifier,
                                string g2SThemeId,
                                string themeTag,
                                string themeTagDataFile)
        {
            if(themeIdentifier == null)
            {
                throw new ArgumentNullException(nameof(themeIdentifier));
            }

            ThemeIdentifier = themeIdentifier;
            G2SThemeId = g2SThemeId ?? throw new ArgumentNullException(nameof(g2SThemeId));
            ThemeTag = themeTag;
            ThemeTagDataFile = themeTagDataFile;
        }

        #endregion

        #region ICompactSerializable Implementation

        /// <inheritdoc />
        public virtual void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, ThemeIdentifier);
            CompactSerializer.Write(stream, G2SThemeId);
            CompactSerializer.Write(stream, ThemeTag);
            CompactSerializer.Write(stream, ThemeTagDataFile);
        }

        /// <inheritdoc />
        public virtual void Deserialize(Stream stream)
        {
            ThemeIdentifier = CompactSerializer.ReadSerializable<IdentifierToken>(stream);
            G2SThemeId = CompactSerializer.ReadString(stream);
            ThemeTag = CompactSerializer.ReadString(stream);
            ThemeTagDataFile = CompactSerializer.ReadString(stream);
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("ThemeInfoBase - ");
            builder.AppendLine("\t ThemeIdentifier Hash: " + ThemeIdentifier.GetHashCode());
            builder.AppendLine("\t G2SThemeId: " + G2SThemeId);
            builder.AppendLine("\t ThemeTag: " + ThemeTag);
            builder.AppendLine("\t ThemeTagDataFile: " + ThemeTagDataFile);

            return builder.ToString();
        }

        #endregion
    }
}