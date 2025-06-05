//-----------------------------------------------------------------------
// <copyright file = "AncillaryAward.cs" company = "IGT">
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
    /// of the automatically generated type AncillaryAward.
    /// </summary>
    public partial class AncillaryAward : IAncillaryAward
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public AncillaryAward()
        {
            risk_amount = new AmountType();
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
                throw new ArgumentNullException("award");
            }

            win_typeField = (AncillaryAwardWin_type)award.WinType;
            risk_amountField = new AmountType(award.RiskAmountValue);
        }

        /// <InheritDoc />
        public override long GetDisplayableAmount()
        {
            checked
            {
                return is_displayable ? amount.Value - risk_amount.Value : 0;
            }
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("AncillaryAward -");
            builder.AppendLine("\t  Win Type = " + win_type);
            builder.AppendLine("\t  Risk Amount = " + risk_amount);
            builder.Append("\t\t  - " + base.ToString());

            return builder.ToString();
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            CompactSerializer.Write(stream, (int)win_type);
            CompactSerializer.Write(stream, risk_amount as ICompactSerializable);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            win_type = (AncillaryAwardWin_type)CompactSerializer.ReadInt(stream);
            risk_amount = CompactSerializer.ReadSerializable<AmountType>(stream);
        }

        #endregion

        #region IAncillaryAward Members

        /// <inheritdoc />
        public AncillaryAwardWinType WinType
        {
            get { return (AncillaryAwardWinType)win_typeField; }
        }

        /// <inheritdoc />
        public long RiskAmountValue
        {
            get { return risk_amountField.Value; }
        }

        #endregion IAncillaryAward Members
    }
}
