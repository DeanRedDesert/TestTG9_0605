// -----------------------------------------------------------------------
// <copyright file = "GameLevelLinkDownValue.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ReportLib.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Game.Core.CompactSerialization;

    /// <summary>
    /// Type indicating the value of game-level.
    /// It's used when game-level is linked, but current status is link-down.
    /// </summary>
    [Serializable]
    public class GameLevelLinkDownValue : ICompactSerializable
    {
        #region Fields

        /// <summary>
        /// The localized texts to display when the game level link is down.
        /// </summary>
        private List<TextLocalization> linkDownTextLocalizations;

        #endregion

        #region Read-Only Properties

        /// <summary>
        /// Gets the text to display when the game-level link is down.
        /// </summary>
        public IEnumerable<TextLocalization> LinkDownTextLocalizations => linkDownTextLocalizations;

        #endregion

        #region Contructors

        /// <summary>
        /// The default constructor which is supposed to be invoked for deserialization purpose only.
        /// </summary>
        public GameLevelLinkDownValue()
        {
        }

        /// <summary>
        /// Constructs an instance of <see cref="GameLevelLinkDownValue"/> with the given
        /// link-down text localizations.
        /// </summary>
        /// <param name="linkDownTextLocalizations">
        /// The localized texts to display when the progressive links are down.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="linkDownTextLocalizations"/> is null.
        /// </exception>
        /// <remarks>
        /// The link-down text localizations could be empty.
        /// </remarks>
        public GameLevelLinkDownValue(IEnumerable<TextLocalization> linkDownTextLocalizations)
        {
            if(linkDownTextLocalizations == null)
            {
                throw new ArgumentNullException(nameof(linkDownTextLocalizations));
            }

            this.linkDownTextLocalizations = linkDownTextLocalizations.ToList();
        }

        #endregion

        #region ICompactSerializable Members

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, linkDownTextLocalizations);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            linkDownTextLocalizations = CompactSerializer.ReadListSerializable<TextLocalization>(stream);
        }

        #endregion

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach(var linkDownTextLocalization in LinkDownTextLocalizations)
            {
                builder.AppendLine(linkDownTextLocalization.ToString());
            }

            return builder.ToString();
        }
    }
}