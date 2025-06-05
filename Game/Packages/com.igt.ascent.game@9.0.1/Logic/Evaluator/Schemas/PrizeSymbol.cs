//-----------------------------------------------------------------------
// <copyright file = "PrizeSymbol.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// Describes the symbol information needed for wins.
    /// </summary>
    public partial class PrizeSymbol : ICompactSerializable, IEquatable<PrizeSymbol>
    {
        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, idField);
            CompactSerializer.Write(stream, requiredCount);
            CompactSerializer.Write(stream, requiredCountFieldSpecified);
            CompactSerializer.Write(stream, indexField);
            CompactSerializer.Write(stream, indexFieldSpecified);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            id = CompactSerializer.ReadString(stream);
            requiredCount = CompactSerializer.ReadUint(stream);
            requiredCountSpecified = CompactSerializer.ReadBool(stream);
            index = CompactSerializer.ReadUint(stream);
            indexSpecified = CompactSerializer.ReadBool(stream);
        }

        #endregion

        #region IEquatable<PrizeSymbol> Members

        /// <inheritdoc />
        public bool Equals(PrizeSymbol other)
        {
            if(other == null)
            {
                return false;
            }

            return id == other.id
                   && requiredCount == other.requiredCount
                   && requiredCountSpecified == other.requiredCountSpecified
                   && index == other.index
                   && indexSpecified == other.indexSpecified;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var result = false;
            var other = obj as PrizeSymbol;
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

            hash = id != null ? hash * 37 + id.GetHashCode() : hash;
            hash = hash * 37 + requiredCount.GetHashCode();
            hash = hash * 37 + requiredCountSpecified.GetHashCode();
            hash = hash * 37 + index.GetHashCode();
            hash = hash * 37 + indexSpecified.GetHashCode();

            return hash;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are considered equal. False otherwise.</returns>
        public static bool operator ==(PrizeSymbol left, PrizeSymbol right)
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
        public static bool operator !=(PrizeSymbol left, PrizeSymbol right)
        {
            return !(left == right);
        }

        #endregion
    }
}
