// -----------------------------------------------------------------------
// <copyright file = "BonusExtensionAward.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameCyclePlay
{
    using System;
    using System.IO;
    using System.Text;
    using CompactSerialization;
    using IGT.Ascent.OutcomeList.Interfaces;

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
        /// Constructor.  Creates a new BonusExtensionAward from an IBonusExtensionAward.
        /// </summary>
        /// <param name="award">An implementation of <see cref="IBonusExtensionAward"/> to use for initialization.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="award"/> is null.
        /// </exception>
        public BonusExtensionAward(IBonusExtensionAward award) : base(award)
        {
            if(award == null)
            {
                throw new ArgumentNullException(nameof(award));
            }

            if(award.BonusIdentifier != null)
            {
                BonusIdentifier = (long)award.BonusIdentifier;
                BonusIdentifierSpecified = true;
            }
            else
            {
                BonusIdentifier = 0;
                BonusIdentifierSpecified = false;
            }
        }

        #endregion Constructors

        #region IBonusExtensionAward Members

        /// <inheritdoc />
        long? IBonusExtensionAward.BonusIdentifier => BonusIdentifierSpecified ? (long?)BonusIdentifier : null;

        #endregion IBonusExtensionAward Members

        #region ICompactSerializable Members

        /// <inheritdoc />
        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            CompactSerializer.Write(stream, BonusIdentifier);
            CompactSerializer.Write(stream, BonusIdentifierSpecified);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            BonusIdentifier = CompactSerializer.ReadLong(stream);
            BonusIdentifierSpecified = CompactSerializer.ReadBool(stream);
        }

        #endregion

        #region ToString Override

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("BonusExtensionAward -");
            builder.AppendLine("\t  Bonus Identifier Specified = " + BonusIdentifierSpecified);
            builder.AppendLine("\t  Bonus Identifier = " + BonusIdentifier);
            builder.Append("\t\t  - " + base.ToString());

            return builder.ToString();
        }

        #endregion ToString Override
    }
}
