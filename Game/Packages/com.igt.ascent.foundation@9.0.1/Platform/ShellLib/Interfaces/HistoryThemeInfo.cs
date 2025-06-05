// -----------------------------------------------------------------------
// <copyright file = "HistoryThemeInfo.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using System.IO;
    using System.Text;
    using Game.Core.Cloneable;
    using Game.Core.CompactSerialization;
    using Platform.Interfaces;

    /// <summary>
    /// Contains information about a shell theme that is being displayed in history.
    /// </summary>
    [Serializable]
    public class HistoryThemeInfo : ThemeInfoBase, IDeepCloneable
    {
        #region Properties

        /// <summary>
        /// Gets the denomination of the game cycle being displayed in history.
        /// </summary>
        public long Denomination { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for <see cref="ICompactSerializable"/>.
        /// </summary>
        public HistoryThemeInfo()
        {
            Denomination = 1;
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
        /// <param name="denomination">
        /// The denomination of the game cycle being displayed in history.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="themeIdentifier"/> or <paramref name="g2SThemeId"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="denomination"/> is less than 1.
        /// </exception>
        public HistoryThemeInfo(IdentifierToken themeIdentifier,
                                string g2SThemeId,
                                string themeTag,
                                string themeTagDataFile,
                                long denomination)
            : base(themeIdentifier, g2SThemeId, themeTag, themeTagDataFile)
        {
            if(denomination < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination cannot be less than 1.");
            }

            Denomination = denomination;
        }

        #endregion

        #region ICompactSerializable Overrides

        /// <inheritdoc />
        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);
            CompactSerializer.Write(stream, Denomination);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);
            Denomination = CompactSerializer.ReadLong(stream);
        }

        #endregion

        #region IDeepClonable Implementation

        /// <inheritdoc/>
        public virtual object DeepClone()
        {
            return new HistoryThemeInfo(ThemeIdentifier, G2SThemeId, ThemeTag, ThemeTagDataFile, Denomination);
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("HistoryThemeInfo - ");
            builder.Append(base.ToString());
            builder.AppendLine("\t Denomination: " + Denomination);

            return builder.ToString();
        }

        #endregion
    }
}