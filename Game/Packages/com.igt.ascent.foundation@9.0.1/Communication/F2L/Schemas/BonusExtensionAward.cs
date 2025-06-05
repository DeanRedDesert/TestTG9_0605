// -----------------------------------------------------------------------
// <copyright file = "BonusExtensionAward.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L.Schemas.Internal
{
    using System;
    using System.IO;
    using System.Text;
    using Ascent.OutcomeList.Interfaces;
    using CompactSerialization;

    /// <summary>
    /// This class contains methods which extend the functionality
    /// of the automatically generated type BonusExtensionAward.
    /// </summary>
    public partial class BonusExtensionAward : IBonusExtensionAward
    {
        #region Constructors

        /// <summary>
        /// Default Parameter-less Constructor for ICompactSerializable.
        /// </summary>
        public BonusExtensionAward()
        {
        }

        /// <summary>
        /// Constructor.  Create a new BonusExtensionAward from an IBonusExtensionAward.
        /// </summary>
        /// <param name="award">An implementation of <see cref="IBonusExtensionAward"/> to use for initialization.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="award"/> is null.
        /// </exception>
        public BonusExtensionAward(IBonusExtensionAward award) : base(award)
        {
            if(award == null)
            {
                throw new ArgumentNullException("award");
            }

            bonus_identifierFieldSpecified = award.BonusIdentifier.HasValue;
            bonus_identifierField = award.BonusIdentifier.GetValueOrDefault();
        }

        #endregion Constructors
        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("BonusExtensionAward -");
            builder.AppendLine("\t  Bonus Identifier Specified = " + bonus_identifierSpecified);
            builder.AppendLine("\t  Bonus Identifier = " + bonus_identifier);
            builder.Append("\t\t  - " + base.ToString());

            return builder.ToString();
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            CompactSerializer.Write(stream, bonus_identifier);
            CompactSerializer.Write(stream, bonus_identifierSpecified);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            bonus_identifier = CompactSerializer.ReadLong(stream);
            bonus_identifierSpecified = CompactSerializer.ReadBool(stream);
        }

        #endregion

        #region IBonusExtensionAward Members

        /// <inheritdoc />
        public long? BonusIdentifier
        {
            get { return bonus_identifierField; }
        }

        #endregion IBonusExtensionAward Members
    }
}
