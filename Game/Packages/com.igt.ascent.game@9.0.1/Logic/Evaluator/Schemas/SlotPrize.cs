//-----------------------------------------------------------------------
// <copyright file = "SlotPrize.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.IO;
    using System.Linq;
    using CompactSerialization;

    /// <summary>
    /// Describes everything about a win.
    /// </summary>
    public partial class SlotPrize : ICompactSerializable, IEquatable<SlotPrize>
    {
        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, prizeSymbolField);
            CompactSerializer.WriteList(stream, prizePayField);
            CompactSerializer.Write(stream, nameField);
            CompactSerializer.Write(stream, payStrategyField);
            CompactSerializer.Write(stream, orderStrategyField);
            CompactSerializer.Write(stream, amountModificationStrategyField);
            CompactSerializer.Write(stream, eligiblePatternField);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            PrizeSymbol = CompactSerializer.ReadListSerializable<PrizeSymbol>(stream);
            PrizePay = CompactSerializer.ReadListSerializable<PrizePay>(stream);
            name = CompactSerializer.ReadString(stream);
            PayStrategy = CompactSerializer.ReadEnum<SlotPrizePayStrategy>(stream);
            OrderStrategy = CompactSerializer.ReadEnum<SlotPrizeOrderStrategy>(stream);
            AmountModificationStrategy = CompactSerializer.ReadEnum<SlotPrizeAmountModificationStrategy>(stream);
            EligiblePattern = CompactSerializer.ReadString(stream);
        }

        #endregion

        #region IEquatable<SlotPrize> Members

        /// <inheritdoc />
        public bool Equals(SlotPrize other)
        {
            if(other == null)
            {
                return false;
            }

            return PrizeSymbol.SequenceEqual(other.PrizeSymbol)
                   && PrizePay.SequenceEqual(other.PrizePay)
                   && name == other.name
                   && PayStrategy == other.PayStrategy
                   && OrderStrategy == other.OrderStrategy
                   && AmountModificationStrategy == other.AmountModificationStrategy
                   && EligiblePattern == other.EligiblePattern;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var result = false;
            var other = obj as SlotPrize;
            if(other != null)
            {
                result = Equals(other);
            }

            return result;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = 23;

            hash = PrizeSymbol != null ? hash * 37 + PrizeSymbol.GetHashCode() : hash;
            hash = PrizePay != null ? hash * 37 + PrizePay.GetHashCode() : hash;
            hash = name != null ? hash * 37 + name.GetHashCode() : hash;
            hash = hash * 37 + PayStrategy.GetHashCode();
            hash = hash * 37 + OrderStrategy.GetHashCode();
            hash = hash * 37 + AmountModificationStrategy.GetHashCode();
            hash = EligiblePattern != null ? hash * 37 + EligiblePattern.GetHashCode() : hash;

            return hash;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are considered equal. False otherwise.</returns>
        public static bool operator ==(SlotPrize left, SlotPrize right)
        {
            if(ReferenceEquals(left, right))
            {
                return true;
            }

            if((object)left == null || (object)right == null)
            {
                return false;
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are not considered equal. False otherwise.</returns>
        public static bool operator !=(SlotPrize left, SlotPrize right)
        {
            return !(left == right);
        }

        #endregion
    }
}
