//-----------------------------------------------------------------------
// <copyright file = "SlotPrizeScale.cs" company = "IGT">
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
    /// Describes a prize and its identifier.
    /// </summary>
    public partial class SlotPrizeScale : ICompactSerializable, IEquatable<SlotPrizeScale>
    {
        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, prizeField);
            CompactSerializer.Write(stream, nameField);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            Prize = CompactSerializer.ReadListSerializable<SlotPrize>(stream);
            name = CompactSerializer.ReadString(stream);
        }

        #endregion

        #region IEquatable<SlotPrizeScale> Members

        /// <inheritdoc />
        public bool Equals(SlotPrizeScale other)
        {
            if(other == null)
            {
                return false;
            }

            return Prize.SequenceEqual(other.Prize)
                   && name == other.name;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var result = false;
            var other = obj as SlotPrizeScale;
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

            hash = Prize != null ? hash * 37 + Prize.GetHashCode() : hash;
            hash = name != null ? hash * 37 + name.GetHashCode() : hash;

            return hash;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are considered equal. False otherwise.</returns>
        public static bool operator ==(SlotPrizeScale left, SlotPrizeScale right)
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
        public static bool operator !=(SlotPrizeScale left, SlotPrizeScale right)
        {
            return !(left == right);
        }

        #endregion
    }
}
