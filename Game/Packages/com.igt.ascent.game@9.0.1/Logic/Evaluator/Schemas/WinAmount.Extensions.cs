//-----------------------------------------------------------------------
// <copyright file = "WinAmount.Extensions.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.IO;
    using System.Linq;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the generated WinAmount class.
    /// </summary>
    public partial class WinAmount : ICompactSerializable, IEquatable<WinAmount>
    {
        /// <summary>
        /// Stores a processed version of the WinAmount's requiredTotalBetRange,
        /// to allow fast comparison to a given bet.
        /// </summary>
        [NonSerialized]
        private RequiredBetRange requiredTotalBetRange;

        /// <summary>
        /// Stores a backup of the requiredTotalBet, to detect when
        /// it is changed and reinstantiate requiredTotalBetRange.
        /// </summary>
        [NonSerialized]
        private string prevRequiredTotalBet = string.Empty;

        /// <summary>
        /// Stores a processed version of the WinAmount's requiredBetOnPattern,
        /// and requiredBetOnPatternMax, to allow fast comparison to a given bet.
        /// </summary>
        [NonSerialized]
        private RequiredBetRange requiredBetOnPatternRange;

        /// <summary>
        /// Stores a backup of requiredBetOnPatternSpecified, to detect when
        /// it is changed and reinstantiate requiredBetOnPatternRange.
        /// </summary>
        [NonSerialized]
        private bool prevRequiredBetOnPatternSpecified;

        /// <summary>
        /// Stores a backup of requiredBetOnPattern, to detect when
        /// it is changed and reinstantiate requiredBetOnPatternRange.
        /// </summary>
        [NonSerialized]
        private uint prevRequiredBetOnPattern;

        /// <summary>
        /// Stores a backup of requiredBetOnPatternMaxSpecified, to detect when
        /// it is changed and reinstantiate requiredBetOnPatternRange.
        /// </summary>
        [NonSerialized]
        private bool prevRequiredBetOnPatternMaxSpecified;

        /// <summary>
        /// Stores a backup of requiredBetOnPatternMax, to detect when
        /// it is changed and reinstantiate requiredBetOnPatternRange.
        /// </summary>
        [NonSerialized]
        private uint prevRequiredBetOnPatternMax;

        /// <summary>
        /// A public interface to the <see cref="RequiredBetRange"/> for the RequiredTotalBet.
        /// </summary>
        public RequiredBetRange RequiredTotalBetRange
        {
            get
            {
                if(requiredTotalBetRange == null || !string.Equals(requiredTotalBet, prevRequiredTotalBet))
                {
                    requiredTotalBetRange = new RequiredBetRange(requiredTotalBet);
                    prevRequiredTotalBet = requiredTotalBet;
                }

                return requiredTotalBetRange;
            }
        }

        /// <summary>
        /// A public interface to the <see cref="RequiredBetRange"/> for the RequiredBetOnPattern.
        /// </summary>
        public RequiredBetRange RequiredBetOnPatternRange
        {
            get
            {
                if(requiredBetOnPatternRange == null ||
                   prevRequiredBetOnPattern != RequiredBetOnPattern ||
                   prevRequiredBetOnPatternSpecified != RequiredBetOnPatternSpecified ||
                   prevRequiredBetOnPatternMax != RequiredBetOnPatternMax ||
                   prevRequiredBetOnPatternMaxSpecified != RequiredBetOnPatternMaxSpecified)
                {
                    if(RequiredBetOnPatternSpecified)
                    {
                        requiredBetOnPatternRange = RequiredBetOnPatternMaxSpecified
                            ? new RequiredBetRange(RequiredBetOnPattern, RequiredBetOnPatternMax)
                            : new RequiredBetRange(RequiredBetOnPattern);
                    }
                    else if(!RequiredBetOnPatternMaxSpecified)
                    {
                        // No requirements are specified, so allow the full range of all possible bets.
                        requiredBetOnPatternRange = new RequiredBetRange();
                    }
                    else
                    {
                        throw new ArgumentException("If RequiredBetOnPattern is not specified, RequiredBetOnPatternMax must not be specified.");
                    }

                    prevRequiredBetOnPattern = RequiredBetOnPattern;
                    prevRequiredBetOnPatternSpecified = RequiredBetOnPatternSpecified;
                    prevRequiredBetOnPatternMax = RequiredBetOnPatternMax;
                    prevRequiredBetOnPatternMaxSpecified = RequiredBetOnPatternMaxSpecified;
                }

                return requiredBetOnPatternRange;
            }
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, progressiveLevelField);
            CompactSerializer.WriteList(stream, requiredPatternsField);
            CompactSerializer.Write(stream, requiredBetOnPatternField);
            CompactSerializer.Write(stream, requiredBetOnPatternFieldSpecified);
            CompactSerializer.Write(stream, requiredBetOnPatternMaxField);
            CompactSerializer.Write(stream, requiredBetOnPatternMaxFieldSpecified);
            CompactSerializer.WriteList(stream, triggerField);
            CompactSerializer.Write(stream, valueField);
            CompactSerializer.Write(stream, valueFieldSpecified);
            CompactSerializer.Write(stream, requiredTotalBetField);
            CompactSerializer.Write(stream, averageBonusPayField);
            CompactSerializer.Write(stream, averageBonusPayFieldSpecified);
            CompactSerializer.Write(stream, winLevelIndexField);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            ProgressiveLevel = CompactSerializer.ReadListString(stream);
            RequiredPatterns = CompactSerializer.ReadListSerializable<RequiredPattern>(stream);
            RequiredBetOnPattern = CompactSerializer.ReadUint(stream);
            RequiredBetOnPatternSpecified = CompactSerializer.ReadBool(stream);
            RequiredBetOnPatternMax = CompactSerializer.ReadUint(stream);
            RequiredBetOnPatternMaxSpecified = CompactSerializer.ReadBool(stream);
            Trigger = CompactSerializer.ReadListSerializable<Trigger>(stream);
            value = CompactSerializer.ReadLong(stream);
            valueSpecified = CompactSerializer.ReadBool(stream);
            requiredTotalBet = CompactSerializer.ReadString(stream);
            averageBonusPay = CompactSerializer.ReadLong(stream);
            averageBonusPaySpecified = CompactSerializer.ReadBool(stream);
            winLevelIndex = CompactSerializer.ReadUint(stream);
        }

        #endregion

        #region IEquatable<WinAmount> Members

        /// <inheritdoc />
        public bool Equals(WinAmount other)
        {
            if(other == null)
            {
                return false;
            }

            return ProgressiveLevel.SequenceEqual(other.ProgressiveLevel)
                   && RequiredPatterns.SequenceEqual(other.RequiredPatterns)
                   && RequiredBetOnPattern == other.RequiredBetOnPattern
                   && RequiredBetOnPatternSpecified == other.RequiredBetOnPatternSpecified
                   && RequiredBetOnPatternMax == other.RequiredBetOnPatternMax
                   && RequiredBetOnPatternMaxSpecified == other.RequiredBetOnPatternMaxSpecified
                   && Trigger.SequenceEqual(other.Trigger)
                   && value == other.value
                   && valueSpecified == other.valueSpecified
                   && requiredTotalBet == other.requiredTotalBet
                   && averageBonusPay == other.averageBonusPay
                   && averageBonusPaySpecified == other.averageBonusPaySpecified
                   && winLevelIndex == other.winLevelIndex;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var result = false;
            var other = obj as WinAmount;
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

            hash = ProgressiveLevel != null ? hash * 37 + ProgressiveLevel.GetHashCode() : hash;
            hash = RequiredPatterns != null ? hash * 37 + RequiredPatterns.GetHashCode() : hash;
            hash = hash * 37 + RequiredBetOnPattern.GetHashCode();
            hash = hash * 37 + RequiredBetOnPatternSpecified.GetHashCode();
            hash = hash * 37 + RequiredBetOnPatternMax.GetHashCode();
            hash = hash * 37 + RequiredBetOnPatternMaxSpecified.GetHashCode();
            hash = Trigger != null ? hash * 37 + Trigger.GetHashCode() : hash;
            hash = hash * 37 + value.GetHashCode();
            hash = hash * 37 + valueSpecified.GetHashCode();
            hash = requiredTotalBet != null ? hash * 37 + requiredTotalBet.GetHashCode() : hash;
            hash = hash * 37 + averageBonusPay.GetHashCode();
            hash = hash * 37 + averageBonusPaySpecified.GetHashCode();
            hash = hash * 37 + winLevelIndex.GetHashCode();

            return hash;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are considered equal. False otherwise.</returns>
        public static bool operator ==(WinAmount left, WinAmount right)
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
        public static bool operator !=(WinAmount left, WinAmount right)
        {
            return !(left == right);
        }

        #endregion
    }
}
