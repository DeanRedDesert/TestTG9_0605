//-----------------------------------------------------------------------
// <copyright file = "RiskAward.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameCyclePlay
{
    using System;
    using System.IO;
    using System.Text;
    using CompactSerialization;
    using IGT.Ascent.OutcomeList.Interfaces;
    using Types;

    /// <summary>
    /// This class contains methods which extend the functionality
    /// of the automatically generated type RiskAward.
    /// </summary>
    public partial class RiskAward : IRiskAward
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
            if(award == null)
            {
                throw new ArgumentNullException(nameof(award));
            }

            riskAmountField = new Amount(award.RiskAmountValue);
        }

        #endregion Constructors

        #region IRiskAward Properties

        /// <inheritdoc />
        public long RiskAmountValue => riskAmountField?.Value ?? 0;

        /// <inheritdoc />
        RiskAwardType IRiskAward.AwardType => (RiskAwardType)AwardType;

        /// <InheritDoc />
        public override long GetDisplayableAmount()
        {
            // TODO:
            // When new type of RiskAward is introduced, make sure to modify the logic to throw the exception,
            // and add the applicable algorithm to calculate its displayable amount.
            if(IsDisplayable)
            {
                throw new NotSupportedException("So far it is not supported for IsDisplayable being true.");
            }

            return 0;
        }

        #endregion IRiskAward Properties

        #region ICompactSerializable Members

        /// <inheritdoc />
        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            CompactSerializer.Write(stream, AwardType);
            CompactSerializer.Write(stream, riskAmountField as ICompactSerializable);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            AwardType = CompactSerializer.ReadEnum<RiskAwardAwardType>(stream);
            riskAmountField = CompactSerializer.ReadSerializable<Amount>(stream);
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
            builder.AppendLine("\t  Risk Amount = " + riskAmountField);
            builder.Append("\t\t  - " + base.ToString());

            return builder.ToString();
        }

        #endregion ToString OVerride
    }
}

