//-----------------------------------------------------------------------
// <copyright file = "PrizePay.cs" company = "IGT">
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
    /// Stores prize win amount and symbol information.
    /// </summary>
    public partial class PrizePay : ICompactSerializable, IEquatable<PrizePay>
    {
        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, winAmountField);
            CompactSerializer.Write(stream, countField);
            CompactSerializer.Write(stream, totalSymbolCountField);
            CompactSerializer.Write(stream, totalSymbolCountFieldSpecified);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            WinAmount = CompactSerializer.ReadListSerializable<WinAmount>(stream);
            count = CompactSerializer.ReadInt(stream);
            totalSymbolCount = CompactSerializer.ReadUint(stream);
            totalSymbolCountSpecified = CompactSerializer.ReadBool(stream);
        }

        #endregion

        #region IEquatable<PrizePay> Members

        /// <inheritdoc />
        public bool Equals(PrizePay other)
        {
            if(other == null)
            {
                return false;
            }

            return WinAmount.SequenceEqual(other.WinAmount)
                   && count == other.count
                   && totalSymbolCount == other.totalSymbolCount
                   && totalSymbolCountSpecified == other.totalSymbolCountSpecified;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var result = false;
            var other = obj as PrizePay;
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

            hash = WinAmount != null ? hash * 37 + WinAmount.GetHashCode() : hash;
            hash = hash * 37 + count.GetHashCode();
            hash = hash * 37 + totalSymbolCount.GetHashCode();
            hash = hash * 37 + totalSymbolCountSpecified.GetHashCode();

            return hash;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are considered equal. False otherwise.</returns>
        public static bool operator ==(PrizePay left, PrizePay right)
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
        public static bool operator !=(PrizePay left, PrizePay right)
        {
            return !(left == right);
        }

        #endregion
    }
}
