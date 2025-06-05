//-----------------------------------------------------------------------
// <copyright file = "FeatureProgressiveAward.cs" company = "IGT">
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
    /// of the automatically generated type FeatureProgressiveAward.
    /// </summary>
    public partial class FeatureProgressiveAward : IFeatureProgressiveAward
    {
        #region Constructors

        /// <summary>
        /// Default Parameter-less Constructor for ICompactSerializable.
        /// </summary>
        public FeatureProgressiveAward()
        {
        }

        /// <summary>
        /// Constructor.  Creates a FeatureProgressiveAward from an IFeatureProgressiveAward.
        /// </summary>
        /// <param name="award">
        /// An implementation of <see cref="IFeatureProgressiveAward"/> to use for initialization.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="award"/> is null.
        /// </exception>
        public FeatureProgressiveAward(IFeatureProgressiveAward award) : base(award)
        {
            if(award == null)
            {
                throw new ArgumentNullException(nameof(award));
            }

            consolationAmountField = new Amount(award.ConsolationAmountValue);
        }

        #endregion Constructors

        #region IFeatureProgressiveAward Members

        /// <inheritdoc />
        public long ConsolationAmountValue => consolationAmountField?.Value ?? 0;

        #endregion IFeatureProgressiveAward Members

        #region ICompactSerializable Members

        /// <inheritdoc />
        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            CompactSerializer.Write(stream, consolationAmountField as ICompactSerializable);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            consolationAmountField = CompactSerializer.ReadSerializable<Amount>(stream);
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

            builder.AppendLine("FeatureProgressiveAward -");
            builder.AppendLine("\t  Consolation Amount = " + consolationAmountField);
            builder.Append("\t\t  - " + base.ToString());

            return builder.ToString();
        }

        #endregion ToString Override
    }
}
