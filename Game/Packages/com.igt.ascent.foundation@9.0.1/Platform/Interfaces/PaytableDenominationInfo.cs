// -----------------------------------------------------------------------
// <copyright file = "PaytableDenominationInfo.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// This data type represents a paytable-denomination pair.
    /// </summary>
    [Serializable]
    public readonly struct PaytableDenominationInfo : IEquatable<PaytableDenominationInfo>
    {
        /// <summary>
        /// A string represents a unique identifier of a paytable.
        /// </summary>
        public string PaytableIdentifier { get; }

        /// <summary>
        /// The denomination which the game is played with.
        /// </summary>
        public long Denomination { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="PaytableDenominationInfo"/>.
        /// </summary>
        /// <param name="paytableIdentifier">The paytable identifier.</param>
        /// <param name="denomination">The denomination.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="paytableIdentifier"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="paytableIdentifier"/> is empty or
        /// <paramref name="denomination"/> is less than or equals to zero.
        /// </exception>
        public PaytableDenominationInfo(string paytableIdentifier, long denomination)
            : this()
        {
            if(paytableIdentifier == null)
            {
                throw new ArgumentNullException(nameof(paytableIdentifier), "Paytable identifier may not be null.");
            }

            if(paytableIdentifier == string.Empty)
            {
                throw new ArgumentException("Paytable identifier may not be an empty string.", nameof(paytableIdentifier));
            }

            if(denomination <= 0)
            {
                throw new ArgumentException("Denomination must be greater than zero.", nameof(denomination));
            }

            PaytableIdentifier = paytableIdentifier;
            Denomination = denomination;
        }

        #region IEquatable<PayvarDenominationInfo> Members

        /// <inheritdoc />
        public bool Equals(PaytableDenominationInfo other)
        {
            return this == other;
        }

        #endregion

        /// <summary>
        /// Override value type's implementation for better performance.
        /// </summary>
        /// <param name="obj">The right hand object for the equality check</param>
        /// <returns>
        /// True if the right hand object equals to this object.
        /// False otherwise.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is PaytableDenominationInfo info && Equals(info);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = 23;

            hash = hash * 37 + PaytableIdentifier.GetHashCode();
            hash = hash * 37 + Denomination.GetHashCode();

            return hash;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>
        /// True if two operands are considered equal.
        /// False otherwise.
        /// </returns>
        public static bool operator ==(PaytableDenominationInfo left, PaytableDenominationInfo right)
        {
            return left.PaytableIdentifier == right.PaytableIdentifier && left.Denomination == right.Denomination;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>
        /// True if two operands are not considered equal.
        /// False otherwise.
        /// </returns>
        public static bool operator !=(PaytableDenominationInfo left, PaytableDenominationInfo right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Convert <see cref="PaytableDenominationInfo"/> to a string.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return $"Paytable Identifier({PaytableIdentifier})/Denomination({Denomination})";
        }
    }
}