//-----------------------------------------------------------------------
// <copyright file = "RiskAward.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.OutcomeList
{
    using System;
    using System.IO;
    using System.Text;
    using Game.Core.CompactSerialization;
    using Interfaces;

    /// <summary>
    /// Type used to communicate generic risk play awards,
    /// such as the outcome of a Round Wager Up Playoff feature.
    /// </summary>
    [Serializable]
    public class RiskAward : Award, IRiskAward
    {
        #region Constructors

        /// <summary>
        /// Default Parameter-less Constructor for ICompactSerializable.
        /// </summary>
        public RiskAward()
        {
            IsDisplayable = false;
        }

        /// <summary>
        /// Constructor.  Creates a RiskAward from an IRiskAward.
        /// </summary>
        /// <param name="award">An implementation of <see cref="IRiskAward"/> to use for initialization.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="award"/> is null.
        /// </exception>
        public RiskAward(IRiskAward award) : base(award)
        {
            if (award == null)
            {
                throw new ArgumentNullException(nameof(award));
            }

            RiskAmountValue = award.RiskAmountValue;
            AwardType = award.AwardType;
        }

        /// <summary>
        /// Constructor.  Initializes a new instance of <see cref="RiskAward"/> using passed in arguments.
        /// </summary>
        /// <param name="amount">
        /// The amount of the award in base units.
        /// </param>
        /// <param name="isDisplayable">
        /// Determines if this reward is displayable.
        /// </param>
        /// <param name="reason">
        /// The reason to display to the user for the award.
        /// </param>
        /// <param name="prize">
        /// The displayable prize string.
        /// </param>
        /// <param name="isTopAward">
        /// Optional. Determines if this is the top award available. Defaults to false.
        /// </param>
        /// <param name="type">
        /// The award type for this risk play.
        /// </param>
        /// <param name="riskAmount">
        /// The amount for the risk play.
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
        public RiskAward(
            OutcomeOrigin origin = OutcomeOrigin.Client,
            long riskAmount = 0,
            bool isDisplayable = false,
            string reason = null,
            string prize = null,
            RiskAwardType type = RiskAwardType.RoundWagerUpPlayoff,
            long amount = 0,
            bool isTopAward = false,
            string tag = null,
            string source = null,
            string sourceDetail = null)
        : base(origin, amount, isDisplayable, reason, prize, isTopAward, tag, source, sourceDetail)
        {
            AwardType = type;
            RiskAmountValue = riskAmount;
        }

        #endregion Constructors

        /// <InheritDoc />
        public override long GetDisplayableAmount()
        {
            // TODO:
            // When new type of RiskAward is introduced, make sure to modify the logic to throw the exception,
            // and add the applicable algorithm to calculate its displayable amount.
            if (IsDisplayable)
            {
                throw new NotSupportedException("So far it is not supported for IsDisplayable being true.");
            }

            return 0;
        }

        #region IRiskAward Members

        /// <inheritdoc />
        public long RiskAmountValue { get; private set; }

        /// <inheritdoc />
        public RiskAwardType AwardType { get; private set; }

        #endregion IRiskAward Members

        #region ICompactSerializable Members

        /// <inheritdoc />
        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            CompactSerializer.Write(stream, AwardType);
            CompactSerializer.Write(stream, RiskAmountValue);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            AwardType = CompactSerializer.ReadEnum<RiskAwardType>(stream);
            RiskAmountValue = CompactSerializer.ReadLong(stream);
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

            builder.AppendLine("RiskAward -");
            builder.AppendLine("\t  Award Type = " + AwardType);
            builder.AppendLine("\t  Risk Amount = " + RiskAmountValue);
            builder.Append("\t\t  - " + base.ToString());

            return builder.ToString();
        }

        #endregion ToString OVerride
    }
}

