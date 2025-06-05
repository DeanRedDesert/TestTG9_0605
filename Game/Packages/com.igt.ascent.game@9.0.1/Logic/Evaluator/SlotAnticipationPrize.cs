//-----------------------------------------------------------------------
// <copyright file = "SlotAnticipationPrize.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// TODO: http://cspjira:8080/jira/browse/AS-8697
// ReSharper disable NonReadonlyMemberInGetHashCode
namespace IGT.Game.Core.Logic.Evaluator
{
    using System;
    using CompactSerialization;

    /// <summary>
    /// Anticipation Pattern Data.
    /// </summary>
    [Serializable]
    public class SlotAnticipationPrize : IComparable<SlotAnticipationPrize>, IEquatable<SlotAnticipationPrize>, ICompactSerializable
    {
        /// <summary>
        /// Construct a new instance.
        /// </summary>
        /// <remarks>
        /// Required for serialization.
        /// </remarks>
        public SlotAnticipationPrize() : this(null, null, null)
        {
            
        }

        /// <summary>
        /// Constructor for SlotAnticipationPrize.
        /// </summary>
        /// <param name="prizeName">Prize Name.</param>
        /// <param name="betAmountRequired">Bet Amount Required Setting.</param>
        /// <param name="requiredActivePattern">Required Active Pattern Setting.</param>
        /// <param name="minimumSymbols">Minimum symbols required to anticipate.</param>
        public SlotAnticipationPrize(string prizeName, string betAmountRequired, string requiredActivePattern, int minimumSymbols = -1)
        {
            PrizeName = prizeName;
            BetAmountRequired = betAmountRequired;
            RequiredActivePattern = requiredActivePattern;
            MinimumSymbolsForAnticipation = minimumSymbols;
        }

        /// <summary>
        /// Prize Name.
        /// </summary>
        public string PrizeName { get; private set; }

        /// <summary>
        /// Bet Amount Required Setting.
        /// </summary>
        public string BetAmountRequired { get; private set; }

        /// <summary>
        /// Required Active Pattern Setting.
        /// </summary>
        public string RequiredActivePattern { get; private set; }

        /// <summary>
        /// The minimum number of symbols to anticipate. 
        /// </summary>
        public int MinimumSymbolsForAnticipation { get; private set; }

        /// <summary>
        /// Generates a hash code for this prize based on its contents.
        /// </summary>
        /// <returns>A value that is suitable for use as a hash code with a dictionary.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var result = PrizeName != null ? PrizeName.GetHashCode() : 0;
                result = (result * 397) ^ (BetAmountRequired != null ? BetAmountRequired.GetHashCode() : 0);
                result = (result * 397) ^ (RequiredActivePattern != null ? RequiredActivePattern.GetHashCode() : 0);
                result = (result * 397) ^ MinimumSymbolsForAnticipation.GetHashCode();
                return result;
            }
        }

        /// <summary>
        /// Compares this prize against another one by considering the <see cref="PrizeName"/>, 
        /// <see cref="BetAmountRequired"/> and <see cref="RequiredActivePattern"/> properties in that order.
        /// </summary>
        /// <param name="other">Another <see cref="SlotAnticipationPrize"/> object.</param>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>0 if both objects are equal.</item>
        ///         <item>A value less than 0 if this prize is less than the other prize.</item>
        ///         <item>A value greater than 0 if this prize is greater than the other prize.</item>
        ///     </list>
        /// </returns>
        public int CompareTo(SlotAnticipationPrize other)
        {
            var comparator = StringComparer.Ordinal;
            var result = comparator.Compare(PrizeName, other.PrizeName);
            if(result == 0)
            {
                result = comparator.Compare(BetAmountRequired, other.BetAmountRequired);
                if(result == 0)
                {
                    result = comparator.Compare(RequiredActivePattern, other.RequiredActivePattern);
                    if(result == 0)
                    {
                        result = MinimumSymbolsForAnticipation.CompareTo(other.MinimumSymbolsForAnticipation);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Determines if the given object is equal to this one.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><b>true</b> if the object is equal, <b>false</b> otherwise.</returns>
        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
            {
                return false;
            }

            if(ReferenceEquals(this, obj))
            {
                return true;
            }

            return Equals(obj as SlotAnticipationPrize);
        }

        #region Implementation of IEquatable<SlotAnticipationPrize>

        /// <summary>
        /// Determines if the given <see cref="SlotAnticipationPrize"/> is equal to this one.
        /// </summary>
        /// <param name="other">The <see cref="SlotAnticipationPrize"/> to compare.</param>
        /// <returns><b>true</b> if the prize is equal, <b>false</b> otherwise.</returns>
        public bool Equals(SlotAnticipationPrize other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            
            if(ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(other.PrizeName, PrizeName) && Equals(other.BetAmountRequired, BetAmountRequired) &&
                   Equals(other.RequiredActivePattern, RequiredActivePattern) && Equals(other.MinimumSymbolsForAnticipation, MinimumSymbolsForAnticipation);
        }

        #endregion

        #region ICompactSerializable Members

        /// <inheritdoc />
        void ICompactSerializable.Serialize(System.IO.Stream stream)
        {
            CompactSerializer.Write(stream, PrizeName);
            CompactSerializer.Write(stream, BetAmountRequired);
            CompactSerializer.Write(stream, RequiredActivePattern);
            CompactSerializer.Write(stream, MinimumSymbolsForAnticipation);
        }

        /// <inheritdoc />
        void ICompactSerializable.Deserialize(System.IO.Stream stream)
        {
            PrizeName = CompactSerializer.ReadString(stream);
            BetAmountRequired = CompactSerializer.ReadString(stream);
            RequiredActivePattern = CompactSerializer.ReadString(stream);
            MinimumSymbolsForAnticipation = CompactSerializer.ReadInt(stream);
        }

        #endregion
    }
}
