//-----------------------------------------------------------------------
// <copyright file = "FeatureProgressiveAward.cs" company = "IGT">
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
    /// of the automatically generated type FeatureProgressiveAward.
    /// </summary>
    public partial class FeatureProgressiveAward : IFeatureProgressiveAward
    {
        /// <summary>
        /// Default Parameter-less Constructor for ICompactSerializable.
        /// </summary>
        public FeatureProgressiveAward()
        {
            consolation_amountField = new AmountType(0);
        }

        /// <summary>
        /// Constructor.  Creates a FeatureProgressiveAward from an IFeatureProgressiveAward.
        /// </summary>
        /// <param name="award">An implementation of <see cref="IFeatureProgressiveAward"/> to use for initialization.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="award"/> is null.
        /// </exception>
        public FeatureProgressiveAward(IFeatureProgressiveAward award) : base(award)
        {
            if(award == null)
            {
                throw new ArgumentNullException("award");
            }

            consolation_amountField = new AmountType(award.ConsolationAmountValue);
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("FeatureProgressiveAward -");
            builder.AppendLine("\t  Consolation Amount = " + consolation_amount);
            builder.Append("\t\t  - " + base.ToString());

            return builder.ToString();
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            CompactSerializer.Write(stream, consolation_amount as ICompactSerializable);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            consolation_amount = CompactSerializer.ReadSerializable<AmountType>(stream);
        }

        #endregion

        #region IFeatureProgressiveAward Members

        /// <inheritdoc />
        public long ConsolationAmountValue
        {
            get { return consolation_amountField.Value; }
        }

        #endregion IFeatureProgressiveAward Members
    }
}
