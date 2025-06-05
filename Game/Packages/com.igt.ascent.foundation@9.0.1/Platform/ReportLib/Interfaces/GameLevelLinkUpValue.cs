// -----------------------------------------------------------------------
// <copyright file = "GameLevelLinkUpValue.cs" company = "IGT">
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
    /// It's used when the game-level is linked, and current status is link-up.
    /// </summary>
    [Serializable]
    public class GameLevelLinkUpValue : ICompactSerializable
    {
        #region Fields

        /// <summary>
        /// The localized text list of the progressive prizes associated to this game level.
        /// </summary>
        private List<TextLocalization> prizeLocalizations;

        #endregion

        #region Read-Only Properties

        /// <summary>
        /// Gets the amount of this game-level in base units.
        /// </summary>
        public long? Amount { get; private set; }

        /// <summary>
        /// Gets the localized text list of the progressive prizes of this game level.
        /// This value could be null.
        /// </summary>
        public IEnumerable<TextLocalization> PrizeLocalizations => prizeLocalizations;

        #endregion

        #region Contructors

        /// <summary>
        /// The default constructor which is supposed to be invoked for deserialization purpose only.
        /// </summary>
        public GameLevelLinkUpValue()
        {
        }

        /// <summary>
        /// Constructs an instance of the <see cref="GameLevelLinkUpValue"/> given amount in base units.
        /// </summary>
        /// <param name="amount">The amount value in base units.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="amount"/> is less than zero.
        /// </exception>
        public GameLevelLinkUpValue(long? amount) : this(amount, null)
        {
        }

        /// <summary>
        /// Constructs an instance of the <see cref="GameLevelLinkUpValue"/> given amount in base units
        /// and a list of the localized prize texts.
        /// </summary>
        /// <param name="amount">The amount value in base units.</param>
        /// <param name="prizeLocalization">The prizes.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="amount"/> is less than zero.
        /// </exception>
        public GameLevelLinkUpValue(long? amount, IEnumerable<TextLocalization> prizeLocalization)
        {
            if(amount.HasValue && amount < 0)
            {
                throw new ArgumentException("Amount value must not be less than zero.", nameof(amount));
            }

            Amount = amount;
            if(prizeLocalization != null)
            {
                prizeLocalizations = prizeLocalization.ToList();
            }
        }

        #endregion

        #region ICompactSerializable Members

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            if(Amount == null)
            {
                CompactSerializer.Write(stream, true);
            }
            else
            {
                CompactSerializer.Write(stream, false);
                CompactSerializer.Write(stream, Amount.Value);
            }
            
            CompactSerializer.WriteList(stream, prizeLocalizations);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            if(!CompactSerializer.ReadBool(stream))
            {
                Amount = CompactSerializer.ReadLong(stream);
            }
            else
            {
                Amount = null;
            }

            prizeLocalizations = CompactSerializer.ReadListSerializable<TextLocalization>(stream);
        }

        #endregion

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();
            if(Amount.HasValue)
            {
                builder.AppendLine($"Amount({Amount})");
            }

            if(PrizeLocalizations != null)
            {
                foreach(var textLocalization in PrizeLocalizations)
                {
                    builder.AppendLine(textLocalization.ToString());
                }
            }

            return builder.ToString();
        }
    }
}
