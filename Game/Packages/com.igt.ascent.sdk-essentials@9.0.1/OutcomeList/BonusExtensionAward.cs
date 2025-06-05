// -----------------------------------------------------------------------
// <copyright file = "BonusExtensionAward.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.OutcomeList
{
    using System;
    using System.IO;
    using System.Text;
    using Game.Core.CompactSerialization;
    using Interfaces;

    /// <summary>
    /// Bonus class extension awards declared during the game-cycle extended bonus states.
    /// </summary>
    [Serializable]
    public class BonusExtensionAward : Award, IBonusExtensionAward
    {
        #region Constructors

        /// <summary>
        /// Public parameter-less constructor for ICompactSerializable.
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
                throw new ArgumentNullException(nameof(award));
            }

            BonusIdentifier = award.BonusIdentifier;
        }

        /// <summary>
        /// Constructor.  Initializes a new instance of <see cref="BonusExtensionAward"/> using passed in arguments.
        /// </summary>
        /// <param name="amount">
        /// Optional. The amount of the award in base units.
        /// </param>
        /// <param name="isDisplayable">
        /// Optional. Determines if this reward is displayable.
        /// </param>
        /// <param name="reason">
        /// Optional. The reason to display to the user for the award.
        /// </param>
        /// <param name="prize">
        /// Optional. The displayable prize string.
        /// </param>
        /// <param name="isTopAward">
        /// Optional. Determines if this is the top award available. Defaults to false.
        /// </param>
        /// <param name="bonusIdentifier">
        /// Optional. The bonus extension identifier. If null, this is not specified. Defaults to null.
        /// </param>
        /// <param name="origin">
        /// The <see cref="OutcomeOrigin"/> for this outcome.
        /// </param>
        /// <param name="tag">
        /// Optional. The identifying tag for the outcome. Defaults to null.
        /// </param>
        /// <param name="source">
        /// Optional. The source field of who created this outcome. Defaults to null.
        /// </param>
        /// <param name="sourceDetail">
        /// Optional. Additional details for the source of this outcome. Defaults to null.
        /// </param>
        public BonusExtensionAward(
            OutcomeOrigin origin = OutcomeOrigin.Client,
            long amount = 0,
            bool isDisplayable = false,
            string reason = null,
            string prize = null,
            long? bonusIdentifier = null,
            bool isTopAward = false,
            string tag = null,
            string source = null,
            string sourceDetail = null)
            : base(origin, amount, isDisplayable, reason, prize, isTopAward, tag, source, sourceDetail)
        {
            BonusIdentifier = bonusIdentifier;
        }

        #endregion Constructors

        #region IBonusExtensionAward Members

        /// <inheritdoc />
        public long? BonusIdentifier { get; private set; }

        #endregion IBonusExtensionAward Members

        #region ICompactSerializable Members

        /// <inheritdoc />
        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            CompactSerializer.Write(stream, BonusIdentifier);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            BonusIdentifier = CompactSerializer.ReadNullable<long>(stream);
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
            builder.AppendLine("\t  Bonus Identifier = " + BonusIdentifier);
            builder.Append("\t\t  - " + base.ToString());

            return builder.ToString();
        }

        #endregion ToString Override
    }
}
