//-----------------------------------------------------------------------
// <copyright file = "AncillaryAward.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.OutcomeList
{
    using System;
    using System.IO;
    using System.Text;
    using Interfaces;
    using Game.Core.CompactSerialization;

    /// <summary>
    /// Type used to communicate ancillary/double up awards.
    /// </summary>
    [Serializable]
    public class AncillaryAward : Award, IAncillaryAward
    {

        #region Constructors

        /// <summary>
        /// Public parameter-less constructor for ICompactSerializable.
        /// </summary>
        public AncillaryAward()
        {
        }

        /// <summary>
        /// Constructor.  Create a new AncillaryAward from an IAncillaryAward.
        /// </summary>
        /// <param name="award">
        /// An implementation of <see cref="IAncillaryAward"/> to use for initialization.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="award"/> is null.
        /// </exception>
        public AncillaryAward(IAncillaryAward award) : base(award)
        {
            if(award == null)
            {
                throw new ArgumentNullException(nameof(award));
            }

            WinType = award.WinType;
            RiskAmountValue = award.RiskAmountValue;
        }

        /// <summary>
        /// Constructor.  Initializes a new instance of <see cref="AncillaryAward"/> using passed in arguments.
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
        /// Optional. The displayable prize string.</param>
        /// <param name="isTopAward">
        /// Optional. Determines if this is the top award available. Defaults to false.
        /// </param>
        /// <param name="winType">
        /// The <see cref="AncillaryAwardWinType"/> for this award.
        /// </param>
        /// <param name="riskAmount">
        /// Optional. The amount risked by the player in base units. Defaults to 0.
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
        public AncillaryAward(
            OutcomeOrigin origin = OutcomeOrigin.Client,
            long amount = 0,
            bool isDisplayable = false,
            string reason = null,
            string prize = null,
            AncillaryAwardWinType winType = AncillaryAwardWinType.Win,
            bool isTopAward = false,
            long riskAmount = 0,
            string tag = null,
            string source = null,
            string sourceDetail = null)
            : base(origin, amount, isDisplayable, reason, prize, isTopAward, tag, source, sourceDetail)
        {
            WinType = winType;
            RiskAmountValue = riskAmount;
        }

        #endregion Constructors

        #region Award Members

        /// <inheritdoc />
        public override long GetDisplayableAmount()
        {
            checked
            {
                return IsDisplayable ? AmountValue - RiskAmountValue : 0;
            }
        }

        #endregion Award Members

        #region IAncillaryAward Members

        /// <inheritdoc />
        public AncillaryAwardWinType WinType { get; private set; }

        /// <inheritdoc />
        public long RiskAmountValue { get; private set; }

        #endregion IAncillaryAward Members

        #region ICompactSerializable Members

        /// <inheritdoc />
        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            CompactSerializer.Write(stream, WinType);
            CompactSerializer.Write(stream, RiskAmountValue);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            WinType = CompactSerializer.ReadEnum<AncillaryAwardWinType>(stream);
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

            builder.AppendLine("AncillaryAward -");
            builder.AppendLine("\t  Win Type = " + WinType);
            builder.AppendLine("\t  Risk Amount = " + RiskAmountValue);
            builder.Append("\t\t  - " + base.ToString());

            return builder.ToString();
        }

        #endregion ToString Override
    }
}
