// -----------------------------------------------------------------------
// <copyright file = "ShellThemeInfo.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Game.Core.Cloneable;
    using Game.Core.CompactSerialization;
    using Platform.Interfaces;

    /// <summary>
    /// Contains information about a shell theme.
    /// </summary>
    [Serializable]
    public class ShellThemeInfo : ThemeInfoBase, IDeepCloneable
    {
        #region Properties

        /// <summary>
        /// Gets all denominations in base units that are currently enabled and selectable for this theme.
        /// </summary>
        public IList<ShellThemeDenomInfo> Denominations { get; private set; }

        /// <summary>
        /// Gets the default denomination value in base units for the theme.
        /// </summary>
        public long DefaultDenomination { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for <see cref="ICompactSerializable"/>.
        /// </summary>
        public ShellThemeInfo()
        {
            Denominations = new List<ShellThemeDenomInfo> { new ShellThemeDenomInfo() };
            DefaultDenomination = 1;
        }

        /// <summary>
        /// Initializes a new instance of creating a <see cref="ShellThemeInfo"/>.
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
        /// <param name="denominations">
        /// Collection of a theme's denominations in base units and information if 
        /// they are progressive denominations.
        /// </param>
        /// <param name="defaultDenomination">
        /// Denomination value in base units to default to.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="themeIdentifier"/> or <paramref name="g2SThemeId"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="denominations"/> is null or empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="defaultDenomination"/> is less than 1.
        /// </exception>
        public ShellThemeInfo(IdentifierToken themeIdentifier,
                              string g2SThemeId,
                              string themeTag,
                              string themeTagDataFile,
                              IList<ShellThemeDenomInfo> denominations,
                              long defaultDenomination)
            : base(themeIdentifier, g2SThemeId, themeTag, themeTagDataFile)
        {
            if(denominations == null || denominations.Count == 0)
            {
                throw new ArgumentException("Denomination list cannot be null or empty.", nameof(denominations));
            }
            if(defaultDenomination < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(defaultDenomination), "Default denomination cannot be less than 1.");
            }

            Denominations = denominations;
            DefaultDenomination = defaultDenomination;
        }

        #endregion

        #region ICompactSerializable Overrides

        /// <inheritdoc />
        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);
            CompactSerializer.WriteList(stream, Denominations);
            CompactSerializer.Write(stream, DefaultDenomination);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);
            Denominations = CompactSerializer.ReadListSerializable<ShellThemeDenomInfo>(stream);
            DefaultDenomination = CompactSerializer.ReadLong(stream);
        }

        #endregion

        #region IDeepClonable Implementation

        /// <inheritdoc/>
        public virtual object DeepClone()
        {
            return new ShellThemeInfo(ThemeIdentifier, G2SThemeId, ThemeTag, ThemeTagDataFile,
                                      Denominations.Select(d => d.DeepClone() as ShellThemeDenomInfo).ToList(),
                                      DefaultDenomination);
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("ShellThemeInfo -");
            builder.Append(base.ToString());
            builder.AppendLine("\t DefaultDenomination: " + DefaultDenomination);
            builder.AppendFormat("\t Selectable Denominations: total " + Denominations.Count);
            builder.AppendLine();

            builder.Append("\t\t");
            foreach(var denomInfo in Denominations)
            {
                builder.Append(denomInfo + " ");
            }
            builder.AppendLine();

            return builder.ToString();
        }

        #endregion
    }
}