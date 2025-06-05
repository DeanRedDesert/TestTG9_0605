//-----------------------------------------------------------------------
// <copyright file = "BankStatus.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;

    /// <summary>
    /// Struct that represents the flags of the set of bank status
    /// maintained by the Foundation.
    /// </summary>
    [Serializable]
    public readonly struct BankStatus
    {
        /// <summary>
        /// This flag indicates whether the player wager is offerable, i.e betting permitted. 
        /// Player wager can be unavailable due to the bank being locked or other such conditions. 
        /// </summary>
        public bool IsPlayerWagerOfferable { get; }

        /// <summary>
        /// This flag indicates whether or not player requested cash outs are currently
        /// being accepted by the Foundation.
        /// </summary>
        public bool IsCashOutOfferable { get; }

        /// <summary>
        /// The flag indicates whether or not the player requested transfers of funds
        /// from the Player Bank Meter to the Player Wagerable Meter are currently
        /// accepted by the Foundation.
        /// </summary>
        public bool IsBankToWagerableOfferable { get; }

        /// <summary>
        /// Constructor taking parameters for all three flags.
        /// </summary>
        /// <param name="isPlayerWagerOfferable">Whether the player wage is offerable.</param>
        /// <param name="isCashOutOfferable">Whether cash out is offerable.</param>
        /// <param name="isBankToWagerableOfferable">Whether bank to wagerable
        ///                                          transfer is offerable.</param>
        public BankStatus(bool isPlayerWagerOfferable, bool isCashOutOfferable, bool isBankToWagerableOfferable)
            : this()
        {
            IsPlayerWagerOfferable = isPlayerWagerOfferable;
            IsCashOutOfferable = isCashOutOfferable;
            IsBankToWagerableOfferable = isBankToWagerableOfferable;
        }

        /// <summary>
        /// Overload Equals for enhanced performance.
        /// </summary>
        /// <param name="rightHand">The right hand object for the equality check.</param>
        /// <returns>True if the right hand object equals to this object.  False otherwise.</returns>
        public bool Equals(BankStatus rightHand)
        {
            return IsPlayerWagerOfferable == rightHand.IsPlayerWagerOfferable &&
                   IsCashOutOfferable == rightHand.IsCashOutOfferable &&
                   IsBankToWagerableOfferable == rightHand.IsBankToWagerableOfferable;
        }

        /// <summary>
        /// Override value type's implementation for better performance.
        /// </summary>
        /// <param name="obj">The right hand object for the equality check.</param>
        /// <returns>True if the right hand object equals to this object.  False otherwise.</returns>
        public override bool Equals(object obj)
        {
            var result = false;

            if(obj is BankStatus)
            {
                result = Equals((BankStatus)obj);
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
            hash = hash * 37 + IsPlayerWagerOfferable.GetHashCode();
            hash = hash * 37 + IsCashOutOfferable.GetHashCode();
            hash = hash * 37 + IsBankToWagerableOfferable.GetHashCode();

            return hash;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are considered equal.  False otherwise.</returns>
        public static bool operator ==(BankStatus left, BankStatus right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are not considered equal.  False otherwise.</returns>
        public static bool operator !=(BankStatus left, BankStatus right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return
                $"BankLocked({IsPlayerWagerOfferable})/CashOutOK({IsCashOutOfferable})/WagerableOK({IsBankToWagerableOfferable})";
        }
    }
}
