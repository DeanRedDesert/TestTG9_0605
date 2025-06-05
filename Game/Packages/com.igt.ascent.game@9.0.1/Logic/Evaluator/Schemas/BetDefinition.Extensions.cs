// -----------------------------------------------------------------------
// <copyright file = "BetDefinition.Extensions.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.IO;
    using Cloneable;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the bet definition class.
    /// </summary>
    public partial class BetDefinition : IEquatable<BetDefinition>, ICompactSerializable,
        IDeepCloneable
    {
        #region Overrides of Object

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as BetDefinition);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return betAmount.GetHashCode();
        }

        #endregion

        #region Implementation of IEquatable<BetDefinition>

        /// <inheritdoc />
        public bool Equals(BetDefinition other)
        {
            if(other == null)
            {
                return false;
            }

            if(ReferenceEquals(this, other))
            {
                return true;
            }

            return betAmount == other.betAmount && name == other.name &&
                   betableTypeReference == other.betableTypeReference &&
                   isMaxBet == other.isMaxBet;
        }

        #endregion

        #region Implementation of ICompactSerializable

        /// <inheritdoc />
        void ICompactSerializable.Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, betAmount);
            CompactSerializer.Write(stream, name);
            CompactSerializer.Write(stream, betableTypeReference);
            CompactSerializer.Write(stream, isMaxBet);
        }

        /// <inheritdoc />
        void ICompactSerializable.Deserialize(Stream stream)
        {
            betAmount = CompactSerializer.ReadUint(stream);
            name = CompactSerializer.ReadString(stream);
            betableTypeReference = CompactSerializer.ReadString(stream);
            isMaxBet = CompactSerializer.ReadBool(stream);
        }

        #endregion

        #region IDeepCloneable

        /// <inheritdoc />
        public object DeepClone()
        {
            var copy = new BetDefinition
                       {
                           betAmount = betAmount,
                           name = name,
                           betableTypeReference = betableTypeReference,
                           isMaxBet = isMaxBet
                       };
            return copy;
        }

        #endregion
    }
}