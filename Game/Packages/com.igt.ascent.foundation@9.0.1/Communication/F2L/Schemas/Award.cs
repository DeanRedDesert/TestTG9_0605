//-----------------------------------------------------------------------
// <copyright file = "Award.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L.Schemas.Internal
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml.Serialization;
    using Ascent.OutcomeList.Interfaces;
    using CompactSerialization;

    /// <summary>
    /// This class contains methods which extend the functionality
    /// of the automatically generated type Award.
    /// </summary>
    public partial class Award : IAward
    {
        /// <summary>
        /// The Award class constructor.
        /// </summary>
        public Award()
        {
            amount = new AmountType();
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
                throw new ArgumentNullException("award");
            }

            amountField = award.AmountValue;
            is_displayableField = award.IsDisplayable;
            displayable_reasonField = award.DisplayableReason;
            prize_stringField = award.PrizeString;
            is_top_awardField = award.IsTopAward;
            is_top_awardFieldSpecified = true;
        }

        #region IAward Members

        /// <inheritdoc />
        [XmlIgnore]
        public long AmountValue
        {
            get { return amount.Value; }
            protected set { amount.Value = value; }
        }

        /// <inheritdoc />
        [XmlIgnore]
        public bool IsDisplayable
        {
            get { return is_displayableField; }
            protected set { is_displayableField = value; }
        }

        /// <inheritdoc />
        [XmlIgnore]
        public string DisplayableReason
        {
            get { return displayable_reasonField; }
            protected set { displayable_reasonField = value; }
        }

        /// <inheritdoc />
        [XmlIgnore]
        public string PrizeString
        {
            get { return prize_stringField; }
            protected set { prize_stringField = value; }
        }

        /// <inheritdoc />
        [XmlIgnore]
        public bool IsTopAward
        {
            get { return is_top_awardField; }
            protected set { is_top_awardField = value; }
        }

        /// <inheritdoc />
        public virtual long GetDisplayableAmount()
        {
            return IsDisplayable ? AmountValue : 0;
        }

        #endregion IAward Members

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("Award -");
            builder.AppendLine("\t  Is Displayable = " + is_displayable);
            builder.AppendLine("\t  Displayable Reason= " + displayable_reason);
            builder.AppendLine("\t  Amount = " + amount);
            builder.AppendLine("\t  Prize String = " + prize_string);
            builder.Append("\t\t  - " + base.ToString());

            return builder.ToString();
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            CompactSerializer.Write(stream, is_displayable);
            CompactSerializer.Write(stream, displayable_reason);
            CompactSerializer.Write(stream, amount as ICompactSerializable);
            CompactSerializer.Write(stream, prize_string);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            is_displayable = CompactSerializer.ReadBool(stream);
            displayable_reason = CompactSerializer.ReadString(stream);
            amount = CompactSerializer.ReadSerializable<AmountType>(stream);
            prize_string = CompactSerializer.ReadString(stream);
        }

        #endregion
    }
}
