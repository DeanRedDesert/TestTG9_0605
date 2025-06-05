//-----------------------------------------------------------------------
// <copyright file = "FeatureProgressiveAward.cs" company = "IGT">
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
    /// Type used for progressive wins associated with <see cref="FeatureAward"/>.
    /// </summary>
    [Serializable]
    public class FeatureProgressiveAward : ProgressiveAward, IFeatureProgressiveAward
    {
        #region Constructors

        /// <summary>
        /// Public parameter-less constructor for ICompactSerializable.
        /// </summary>
        public FeatureProgressiveAward()
        {
        }

        /// <summary>
        /// Constructor.  Creates a <see cref="FeatureProgressiveAward"/> from an <see cref="IFeatureProgressiveAward"/>.
        /// </summary>
        /// <param name="award">An implementation of <see cref="IFeatureProgressiveAward"/> to use for initialization.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="award"/> is null.
        /// </exception>
        public FeatureProgressiveAward(IFeatureProgressiveAward award) : base(award)
        {
            if(award == null)
            {
                throw new ArgumentNullException(nameof(award));
            }

            ConsolationAmountValue = award.ConsolationAmountValue;
        }

        /// <summary>
        /// Constructor.  Initializes a new instance of <see cref="FeatureProgressiveAward"/> using passed in arguments.
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
        /// <param name="hitState">
        /// The <see cref="ProgressiveAwardHitState"/> for this progressive award.
        /// </param>
        /// <param name="consolationAmount">
        /// The consolation award in base units.
        /// </param>
        /// <param name="gameLevel">
        /// Optional. The game level for this award. If null, this is not specified. Defaults to null.
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
        public FeatureProgressiveAward(
            OutcomeOrigin origin = OutcomeOrigin.Client,
            long amount = 0,
            bool isDisplayable = false,
            string reason = null,
            string prize = null,
            long consolationAmount = 0,
            ProgressiveAwardHitState hitState = ProgressiveAwardHitState.PotentialHit,
            uint? gameLevel = null,
            bool isTopAward = false,
            string tag = null,
            string source = null,
            string sourceDetail = null)
            : base(origin, amount, isDisplayable, reason, prize, hitState, gameLevel, isTopAward, tag, source, sourceDetail)
        {
            ConsolationAmountValue = consolationAmount;
        }

        #endregion Constructors

        #region internal methods

        /// <summary>
        /// Internal method for updating the ConsolationAmountValue property.
        /// </summary>
        /// <param name="consolationAmount">The <see cref="long"/> to update to.</param>
        internal void UpdateConsolationAmountValue(long consolationAmount)
        {
            ConsolationAmountValue = consolationAmount;
        }

        #endregion internal methods

        #region IFeatureProgressiveAward Members

        /// <inheritdoc />
        public long ConsolationAmountValue { get; protected set; }

        #endregion IFeatureProgressiveAward Members

        #region ICompactSerializable Members

        /// <inheritdoc />
        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            CompactSerializer.Write(stream, ConsolationAmountValue);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            ConsolationAmountValue = CompactSerializer.ReadLong(stream);
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
            builder.AppendLine("\t  Consolation Amount = " + ConsolationAmountValue);
            builder.Append("\t\t  - " + base.ToString());

            return builder.ToString();
        }

        #endregion ToString Override
    }
}
