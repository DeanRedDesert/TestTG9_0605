//-----------------------------------------------------------------------
// <copyright file = "AncillaryAward.cs" company = "IGT">
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
    /// of the automatically generated type AncillaryAward.
    /// </summary>
    public partial class AncillaryAward : IAncillaryAward
    {
        #region Constructors

        /// <summary>
        /// Default Parameter-less Constructor for ICompactSerializable.
        /// </summary>
        public AncillaryAward()
        {
            riskAmountField = new Amount();
        }

        /// <summary>
        /// Constructor.  Creates a new AncillaryAward from an IAncillaryAward.
        /// </summary>
        /// <param name="award">An implementation of <see cref="IAncillaryAward"/> to use for initialization.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="award"/> is null.
        /// </exception>
        public AncillaryAward(IAncillaryAward award) : base(award)
        {
            if(award == null)
            {
                throw new ArgumentNullException(nameof(award));
            }

            WinType = (AncillaryAwardWinType)award.WinType;
            riskAmountField = new Amount(award.RiskAmountValue);
        }

        #endregion Constructors

        #region IAncillaryAward Members

        /// <inheritdoc />
        Ascent.OutcomeList.Interfaces.AncillaryAwardWinType IAncillaryAward.WinType => (Ascent.OutcomeList.Interfaces.AncillaryAwardWinType)WinType;

        /// <inheritdoc />
        public long RiskAmountValue => riskAmountField?.Value ?? 0;

        /// <inheritdoc />
        public override long GetDisplayableAmount()
        {
            checked
            {
                return IsDisplayable ? AmountValue - RiskAmountValue : 0;
            }
        }

        #endregion IAncillaryAward Members

        #region ICompactSerializable Members

        /// <inheritdoc />
        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            CompactSerializer.Write(stream, WinType);
            CompactSerializer.Write(stream, riskAmountField as ICompactSerializable);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            WinType = CompactSerializer.ReadEnum<AncillaryAwardWinType>(stream);
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

            builder.AppendLine("AncillaryAward -");
            builder.AppendLine("\t  Win Type = " + WinType);
            builder.AppendLine("\t  Risk Amount = " + riskAmountField);
            builder.Append("\t\t  - " + base.ToString());

            return builder.ToString();
        }

        #endregion ToString Override
    }
}
