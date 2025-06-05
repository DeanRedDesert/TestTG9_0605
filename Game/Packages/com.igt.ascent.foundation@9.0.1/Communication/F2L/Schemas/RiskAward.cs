//-----------------------------------------------------------------------
// <copyright file = "RiskAward.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L.Schemas.Internal
{
    using System;
    using System.IO;
    using System.Text;
    using Ascent.OutcomeList.Interfaces;
    using CompactSerialization;

    /// <summary>
    /// This class contains methods which extend the functionality
    /// of the automatically generated type RiskAward.
    /// </summary>
    public partial class RiskAward : IRiskAward
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public RiskAward()
        {
            risk_amount = new AmountType();
            is_displayable = false;
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
                throw new ArgumentNullException("award");
            }

            risk_amountField = award.RiskAmountValue;
            award_typeField = (RiskAwardAward_type)award.AwardType;
        }

        #endregion Constructors

        /// <inheritdoc />
        public override long GetDisplayableAmount()
        {
            // TODO:
            // When new type of RiskAward is introduced, make sure to modify the logic to throw the exception,
            // and add the applicable algorithm to calculate its displayable amount.
            if (is_displayable)
            {
                throw new NotSupportedException("So far it is not supported for is_displayable being true.");
            }
            return 0;
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("RiskAward -");
            builder.AppendLine("\t  Award Type = " + award_type);
            builder.AppendLine("\t  Risk Amount = " + risk_amount);
            builder.Append("\t\t  - " + base.ToString());

            return builder.ToString();
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            CompactSerializer.Write(stream, (int)award_type);
            CompactSerializer.Write(stream, risk_amount as ICompactSerializable);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            award_type = (RiskAwardAward_type)CompactSerializer.ReadInt(stream);
            risk_amount = CompactSerializer.ReadSerializable<AmountType>(stream);
        }

        #endregion

        #region IRiskAward Members

        /// <inheritdoc />
        public long RiskAmountValue
        {
            get { return risk_amountField.Value; }
        }

        /// <inheritdoc />
        public RiskAwardType AwardType
        {
            get { return (RiskAwardType)award_typeField; }
        }

        #endregion IRiskAward Members
    }
}

