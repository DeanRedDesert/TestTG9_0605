//-----------------------------------------------------------------------
// <copyright file = "HistoryRecord.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;

    /// <summary>
    /// Struct that represents a played game cycle stored in the history.
    /// </summary>
    [Serializable]
    internal struct HistoryRecord
    {
        /// <summary>
        /// Paytable variant used for this history game cycle.
        /// </summary>
        public PaytableVariant PaytableVariant { get; }

        /// <summary>
        /// Denomination used for this history game cycle.
        /// </summary>
        public long Denomination { get; }

        /// <summary>
        /// The location identification of this record in the
        /// Foundation owned history storage.
        /// </summary>
        public int StorageId { get; }

        /// <summary>
        /// Constructor takes arguments for both fields.
        /// </summary>
        /// <param name="paytableVariant">Paytable variant used for this history game cycle.</param>
        /// <param name="denomination">Denomination used for this history game cycle.</param>
        /// <param name="storageId">The location identification of this record in the
        ///                         Foundation owned history storage.</param>
        public HistoryRecord(PaytableVariant paytableVariant, long denomination, int storageId) : this()
        {
            PaytableVariant = paytableVariant;
            Denomination = denomination;
            StorageId = storageId;
        }

        /// <summary>
        /// Overload Equals for enhanced performance.
        /// </summary>
        /// <param name="rightHand">The right hand object for the equality check.</param>
        /// <returns>True if the right hand object equals to this object.  False otherwise.</returns>
        public bool Equals(HistoryRecord rightHand)
        {
            return PaytableVariant == rightHand.PaytableVariant &&
                   Denomination == rightHand.Denomination &&
                   StorageId == rightHand.StorageId;
        }

        /// <summary>
        /// Override value type's implementation for better performance.
        /// </summary>
        /// <param name="obj">The right hand object for the equality check.</param>
        /// <returns>True if the right hand object equals to this object.  False otherwise.</returns>
        public override bool Equals(object obj)
        {
            var result = false;

            if(obj is HistoryRecord rightHand)
            {
                result = Equals(rightHand);
            }

            return result;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <returns>The hash code generated.</returns>
        public override int GetHashCode()
        {
            var hash = 23;
            hash = hash * 37 + PaytableVariant.GetHashCode();
            hash = hash * 37 + Denomination.GetHashCode();
            hash = hash * 37 + StorageId.GetHashCode();

            return hash;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are considered equal.  False otherwise.</returns>
        public static bool operator ==(HistoryRecord left, HistoryRecord right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are not considered equal.  False otherwise.</returns>
        public static bool operator !=(HistoryRecord left, HistoryRecord right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return $"{PaytableVariant}/Denomination({Denomination})/HistoryStorageId({StorageId})";
        }
    }
}
