//-----------------------------------------------------------------------
// <copyright file = "PlayerMeters.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// Structure that represents the values of the set of player meters
    /// maintained by the Foundation.
    /// </summary>
    [Serializable]
    public struct PlayerMeters : IPlayerMeters
    {
        /// <inheritdoc/>
        public long Wagerable { get; }

        /// <inheritdoc/>
        public long Bank { get; }

        /// <inheritdoc/>
        public long Paid { get; }

        /// <summary>
        /// Constructor taking parameters for all three meters.
        /// </summary>
        /// <param name="wagerable">Value of player wagerable meter.</param>
        /// <param name="bank">Value of player bank meter.</param>
        /// <param name="paid">Value of player paid meter.</param>
        public PlayerMeters(long wagerable, long bank, long paid) : this()
        {
            Wagerable = wagerable;
            Bank = bank;
            Paid = paid;
        }

        /// <summary>
        /// Overload Equals for enhanced performance.
        /// </summary>
        /// <param name="rightHand">The right hand object for the equality check.</param>
        /// <returns>True if the right hand object equals to this object.  False otherwise.</returns>
        public bool Equals(PlayerMeters rightHand)
        {
            return Wagerable == rightHand.Wagerable &&
                   Bank == rightHand.Bank &&
                   Paid == rightHand.Paid;
        }

        /// <summary>
        /// Override value type's implementation for better performance.
        /// </summary>
        /// <param name="obj">The right hand object for the equality check.</param>
        /// <returns>True if the right hand object equals to this object.  False otherwise.</returns>
        public override bool Equals(object obj)
        {
            var result = false;

            if (obj is PlayerMeters meters)
            {
                result = Equals(meters);
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
            hash = hash * 37 + Wagerable.GetHashCode();
            hash = hash * 37 + Bank.GetHashCode();
            hash = hash * 37 + Paid.GetHashCode();

            return hash;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are considered equal.  False otherwise.</returns>
        public static bool operator ==(PlayerMeters left, PlayerMeters right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are not considered equal.  False otherwise.</returns>
        public static bool operator !=(PlayerMeters left, PlayerMeters right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return $"Wagerable({Wagerable})/Bank({Bank})/Paid({Paid})";
        }
    }
}
