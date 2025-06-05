//-----------------------------------------------------------------------
// <copyright file = "Award.cs" company = "IGT">
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
    /// Used as a base type for other types of extended awards such as <see cref="FeatureAward"/>
    /// and <see cref="ProgressiveAward"/>.
    /// </summary>
    [Serializable]
    public class Award : Outcome, IAward
    {
        #region Constructors

        /// <summary>
        /// Public parameter-less constructor for ICompactSerializable.
        /// </summary>
        public Award()
        {
        }

        /// <summary>
        /// Constructor.  Creates an Award from an IAward.
        /// </summary>
        /// <param name="award">An implementation of <see cref="IAward"/> to use for initialization.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="award"/> is null.
        /// </exception>
        public Award(IAward award) : base(award)
        {
            if(award == null)
            {
                throw new ArgumentNullException(nameof(award));
            }

            AmountValue = award.AmountValue;
            IsDisplayable = award.IsDisplayable;
            DisplayableReason = award.DisplayableReason;
            PrizeString = award.PrizeString;
            IsTopAward = award.IsTopAward;
        }

        /// <summary>
        /// Constructor.  Initializes a new instance of <see cref="Award"/> using passed in arguments.
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
        public Award(
            OutcomeOrigin origin = OutcomeOrigin.Client,
            long amount = 0,
            bool isDisplayable = false,
            string reason = null,
            string prize = null,
            bool isTopAward = false,
            string tag = null,
            string source = null,
            string sourceDetail = null)
            : base(origin, tag, source, sourceDetail)
        {
            AmountValue = amount;
            IsDisplayable = isDisplayable;
            DisplayableReason = reason;
            PrizeString = prize;
            IsTopAward = isTopAward;
        }

        #endregion Constructors

        #region internal methods

        /// <summary>
        /// Internal method for updating the AmountValue property.
        /// </summary>
        /// <param name="amount">The <see cref="long"/> to update to.</param>
        internal void UpdateAmountValue(long amount)
        {
            AmountValue = amount;
        }

        /// <summary>
        /// Internal method for updating the HitState property.
        /// </summary>
        /// <param name="isDisplayable">The <see cref="bool"/> to update to.</param>
        internal void UpdateIsDisplayable(bool isDisplayable)
        {
            IsDisplayable = isDisplayable;
        }

        /// <summary>
        /// Internal method for updating the PrizeString property.
        /// </summary>
        /// <param name="prizeString">The <see cref="string"/> to update to.</param>
        internal void UpdatePrizeString(string prizeString)
        {
            PrizeString = prizeString;
        }

        #endregion internal methods

        #region IAward Members

        /// <inheritdoc />
        public long AmountValue { get; protected set; }

        /// <inheritdoc />
        public bool IsDisplayable { get; protected set; }

        /// <inheritdoc />
        public string DisplayableReason { get; protected set; }

        /// <inheritdoc />
        public string PrizeString { get; protected set; }

        /// <inheritdoc />
        public bool IsTopAward { get; protected set; }

        /// <inheritdoc />
        public virtual long GetDisplayableAmount()
        {
            return IsDisplayable ? AmountValue : 0;
        }

        #endregion IAward Members

        #region ICompactSerializable Members

        /// <inheritdoc />
        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            CompactSerializer.Write(stream, IsDisplayable);
            CompactSerializer.Write(stream, DisplayableReason);
            CompactSerializer.Write(stream, AmountValue);
            CompactSerializer.Write(stream, PrizeString);
            CompactSerializer.Write(stream, IsTopAward);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            IsDisplayable = CompactSerializer.ReadBool(stream);
            DisplayableReason = CompactSerializer.ReadString(stream);
            AmountValue = CompactSerializer.ReadLong(stream);
            PrizeString = CompactSerializer.ReadString(stream);
            IsTopAward = CompactSerializer.ReadBool(stream);
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

            builder.AppendLine("Award -");
            builder.AppendLine("\t  Is Displayable = " + IsDisplayable);
            builder.AppendLine("\t  Displayable Reason= " + DisplayableReason);
            builder.AppendLine("\t  Amount = " + AmountValue);
            builder.AppendLine("\t  Prize String = " + PrizeString);
            builder.AppendLine("\t  Is Top Award = " + IsTopAward);
            builder.Append("\t\t  - " + base.ToString());

            return builder.ToString();
        }

        #endregion ToString Override
    }
}
