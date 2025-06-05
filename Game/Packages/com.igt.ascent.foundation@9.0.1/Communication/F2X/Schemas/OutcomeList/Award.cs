//-----------------------------------------------------------------------
// <copyright file = "Award.cs" company = "IGT">
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
    /// of the automatically generated type Award.
    /// </summary>
    public partial class Award : IAward
    {
        #region Constructors

        /// <summary>
        /// Default Parameter-less Constructor for ICompactSerializable.
        /// </summary>
        public Award()
        {
            amountField = new Amount();
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

            amountField = new Amount(award.AmountValue);
            IsDisplayable = award.IsDisplayable;
            DisplayableReason = award.DisplayableReason;
            PrizeString = award.PrizeString;
            IsTopAward = award.IsTopAward;
        }

        #endregion Constructors

        #region IAward Members

        /// <inheritdoc />
        public long AmountValue => amountField?.Value ?? 0;

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
            CompactSerializer.Write(stream, amountField as ICompactSerializable);
            CompactSerializer.Write(stream, PrizeString);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            IsDisplayable = CompactSerializer.ReadBool(stream);
            DisplayableReason = CompactSerializer.ReadString(stream);
            amountField = CompactSerializer.ReadSerializable<Amount>(stream);
            PrizeString = CompactSerializer.ReadString(stream);
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
            builder.AppendLine("\t  Amount = " + amountField);
            builder.AppendLine("\t  Prize String = " + PrizeString);
            builder.Append("\t\t  - " + base.ToString());

            return builder.ToString();
        }

        #endregion ToString Override
    }
}
