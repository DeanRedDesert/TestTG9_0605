//-----------------------------------------------------------------------
// <copyright file = "RequiredPattern.Extensions.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the generated WinAmount class.
    /// </summary>
    public partial class RequiredPattern : ICompactSerializable, IEquatable<RequiredPattern>
    {
        /// <summary>
        /// Stores a processed version of the RequiredPattern's BetAmountRequired 
        /// to allow fast comparison to a given bet.
        /// </summary>
        [NonSerialized]
        private RequiredBetRange betAmountRequiredRange;

        /// <summary>
        /// Stores a backup of the BetAmountRequired, to detect when
        /// it is changed and reinstantiate betAmountRequiredRange.
        /// </summary>
        [NonSerialized]
        private string prevBetAmountRequired = string.Empty;

        /// <summary>
        /// A public interface to the <see cref="RequiredBetRange"/> for the BetAmountRequired.
        /// </summary>
        public RequiredBetRange BetAmountRequiredRange
        {
            get
            {
                if(betAmountRequiredRange == null || !string.Equals(BetAmountRequired, prevBetAmountRequired))
                {
                    betAmountRequiredRange = new RequiredBetRange(BetAmountRequired);
                    prevBetAmountRequired = BetAmountRequired;
                }
                return betAmountRequiredRange;
            }
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, betAmountRequiredField);
            CompactSerializer.Write(stream, requiredActivePatternField);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            BetAmountRequired = CompactSerializer.ReadString(stream);
            RequiredActivePattern = CompactSerializer.ReadString(stream);
        }

        #endregion

        #region IEquatable<RequiredPattern> Members

        /// <inheritdoc />
        public bool Equals(RequiredPattern other)
        {
            if(other == null)
            {
                return false;
            }

            return BetAmountRequired == other.BetAmountRequired
                && RequiredActivePattern == other.RequiredActivePattern;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var result = false;
            var other = obj as RequiredPattern;
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

            hash = BetAmountRequired != null ? hash * 37 + BetAmountRequired.GetHashCode() : hash;
            hash = RequiredActivePattern != null ? hash * 37 + RequiredActivePattern.GetHashCode() : hash;
            
            return hash;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are considered equal. False otherwise.</returns>
        public static bool operator ==(RequiredPattern left, RequiredPattern right)
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
        public static bool operator !=(RequiredPattern left, RequiredPattern right)
        {
            return !(left == right);
        }

        #endregion
    }
}
