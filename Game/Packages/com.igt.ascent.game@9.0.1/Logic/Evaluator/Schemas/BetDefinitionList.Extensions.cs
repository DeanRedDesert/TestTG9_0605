// -----------------------------------------------------------------------
// <copyright file = "BetDefinitionList.Extensions.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.IO;
    using System.Linq;
    using Cloneable;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the BetDefinitionList class.
    /// </summary>
    public partial class BetDefinitionList : IEquatable<BetDefinitionList>, ICompactSerializable,
        IDeepCloneable
    {
        #region Overrides of Object

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as BetDefinitionList);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return totalBet.GetHashCode();
        }

        #endregion

        #region Implementation of IEquatable<BetDefinitionList>

        /// <inheritdoc />
        public bool Equals(BetDefinitionList other)
        {
            if(other == null)
            {
                return false;
            }

            if(ReferenceEquals(this, other))
            {
                return true;
            }

            return totalBet == other.totalBet && isMax == other.isMax &&
                   BetDefinition.SequenceEqual(other.BetDefinition);
        }

        #endregion

        #region Implementation of ICompactSerializable

        /// <inheritdoc />
        void ICompactSerializable.Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, totalBet);
            CompactSerializer.Write(stream, isMax);
            CompactSerializer.WriteList(stream, BetDefinition);
        }

        /// <inheritdoc />
        void ICompactSerializable.Deserialize(Stream stream)
        {
            totalBet = CompactSerializer.ReadUint(stream);
            isMax = CompactSerializer.ReadBool(stream);
            BetDefinition = CompactSerializer.ReadListSerializable<BetDefinition>(stream);
        }

        #endregion

        #region IDeepCloneable

        /// <inheritdoc />
        public object DeepClone()
        {
            var copy = new BetDefinitionList
                       {
                           totalBet = totalBet,
                           isMax = isMax,
                           BetDefinition = NullableClone.DeepCloneList(BetDefinition)
                       };
            return copy;
        }

        #endregion
    }
}