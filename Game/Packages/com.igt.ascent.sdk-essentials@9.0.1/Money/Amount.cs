//-----------------------------------------------------------------------
// <copyright file = "Amount.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Money
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Amount class used to centralize the math required to convert between base unit and game credit values.
    /// </summary>
    public class Amount : IComparable<Amount>, IEquatable<Amount>
    {
        #region Private Fields

        private long? gameCreditValue;
        private long? currencyValue;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="Amount"/> with a base unit denominator,
        /// a base value, and a game denomination.
        /// </summary>
        /// <param name="baseDenom">
        /// The "base unit denominator", which indicates the number of base units represented by,
        /// or contained in, one unit of the smallest unit of currency in use.
        /// For example, for US currency, if Foundation needed to keep track of 1/10 of a penny
        /// (i.e. we needed to support 1/10 of a cent for betting), then the "base unit denominator"
        /// would be 10.
        /// </param>
        /// <param name="baseValue">
        /// The amount value in base units.
        /// </param>
        /// <param name="gameDenom">
        /// The game credit denomination in base units.
        /// </param>
        /// <remarks>
        /// The Amount and Denomination values sent in all F2X messages are values in base units.
        /// For example, when "base unit denominator" is 10, Amount value of 100 base units would
        /// indicate a currency value of 10 cents, and Denomination of 50 would indicate 5 cents,
        /// while the credit balance would be 2 credits.
        /// 
        /// Currently, Foundation only supports "base unit denominator" of 1.
        /// New F2X message would be needed for Foundation to notify the clients if a value other than 1 is used.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="baseDenom"/> or <paramref name="gameDenom"/>
        /// is less than or equal to zero.
        /// </exception>
        private Amount(long baseDenom, long baseValue, long gameDenom)
        {
            if(baseDenom <= 0)
            {
                throw new ArgumentException("baseDenom cannot be less than or equal to zero.", nameof(baseDenom));
            }
            if(gameDenom <= 0)
            {
                throw new ArgumentException("gameDenom cannot be less than or equal to zero.", nameof(gameDenom));
            }

            BaseDenom = baseDenom;
            GameDenom = gameDenom;
            BaseValue = baseValue;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Amount"/> with the same
        /// value of <paramref name="copy"/>.
        /// </summary>
        /// <param name="copy"><see cref="Amount"/> to copy.</param>
        public Amount(Amount copy)
            : this(copy.BaseDenom, copy.BaseValue, copy.GameDenom)
        {
        }

        #endregion

        #region Static Creator Functions

        /// <summary>
        /// Creates a new instance of <see cref="Amount"/> from a game credit value,
        /// in units of the game denomination specified by <paramref name="gameDenom"/>.
        /// </summary>
        /// <param name="gameCreditValue">
        /// The game credit value, in units of game denomination.
        /// </param>
        /// <param name="gameDenom">
        /// The game credit denomination in base units.
        /// This parameter is optional.  If not specified, it is default to 1.
        /// </param>
        /// <param name="baseDenom">
        /// The "base unit denominator", which indicates the number of base units contained in
        /// one unit of the smallest unit of currency in use. Take US currency as example,
        /// a "base unit denominator" of 10 means each base unit is worth 1/10 of penny.
        /// This parameter is optional.  If not specified, it is default to 1.
        /// </param>
        /// <returns>
        /// The new instance created.
        /// </returns>
        public static Amount FromGameCredits(long gameCreditValue, long gameDenom = 1, long baseDenom = 1)
        {
            return new Amount(baseDenom, 0, gameDenom).AddGameCredits(gameCreditValue);
        }

        /// <summary>
        /// Creates a new instance of <see cref="Amount"/> from a base value in base units.
        /// The base unit value is determined by <paramref name="baseDenom"/>.
        /// </summary>
        /// <param name="baseValue">
        /// The amount value in base units.
        /// </param>
        /// <param name="gameDenom">
        /// The game credit denomination in base units.
        /// This parameter is optional.  If not specified, it is default to 1.
        /// </param>
        /// <param name="baseDenom">
        /// The "base unit denominator", which indicates the number of base units contained in
        /// one unit of the smallest unit of currency in use. Take US currency as example,
        /// a "base unit denominator" of 10 means each base unit is worth 1/10 of penny.
        /// This parameter is optional.  If not specified, it is default to 1.
        /// </param>
        /// <returns>
        /// The new instance created.
        /// </returns>
        public static Amount FromBaseValue(long baseValue, long gameDenom = 1, long baseDenom = 1)
        {
            return new Amount(baseDenom, baseValue, gameDenom);
        }

        /// <summary>
        /// Creates a new instance of <see cref="Amount"/> from a currency value
        /// in the smallest unit of currency in use.
        /// For example, for US currency, the smallest unit is 1 cent.
        /// </summary>
        /// <param name="currencyValue">
        /// The amount value in the smallest unit of currency in use.
        /// </param>
        /// <param name="gameDenom">
        /// The game credit denomination in base units.
        /// This parameter is optional.  If not specified, it is default to 1.
        /// </param>
        /// <param name="baseDenom">
        /// The "base unit denominator", which indicates the number of base units contained in
        /// one unit of the smallest unit of currency in use. Take US currency as example,
        /// a "base unit denominator" of 10 means each base unit is worth 1/10 of penny.
        /// This parameter is optional.  If not specified, it is default to 1.
        /// </param>
        /// <returns>
        /// The new instance created.
        /// </returns>
        public static Amount FromCurrencyValue(long currencyValue, long gameDenom = 1, long baseDenom = 1)
        {
            checked
            {
                return new Amount(baseDenom, currencyValue * baseDenom, gameDenom);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the "base unit denominator", i.e. the number of base units represented by,
        /// or contained in, one unit of the smallest unit of currency in use.
        /// For example, for US currency, if a base unit is worth 1/10 of a penny,
        /// then this value would be 10.
        /// </summary>
        public long BaseDenom { get; }

        /// <summary>
        /// Gets the game credit denomination in base units.
        /// </summary>
        public long GameDenom { get; }

        /// <summary>
        /// Gets the amount value of this <see cref="Amount"/> in base units.
        /// This is the value used to check against the amount values sent from Foundation,
        /// assuming the same <see cref="BaseDenom"/> for both.
        /// </summary>
        public long BaseValue { get; }

        /// <summary>
        /// Gets the currency value of this <see cref="Amount"/> in the smallest unit of currency in use.
        /// For example, for US currency, this is the amount value in cents.
        /// </summary>
        public long CurrencyValue => (currencyValue ?? (currencyValue = BaseValue / BaseDenom)).Value;

        /// <summary>
        /// Gets the value of this <see cref="Amount"/> in units of game denomination.
        /// </summary>
        public long GameCreditValue => (gameCreditValue ?? (gameCreditValue = BaseValue / GameDenom)).Value;

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a new <see cref="Amount"/> that adds the specified number of base units to
        /// <see cref="BaseValue"/> of this instance.
        /// </summary>
        /// <param name="baseUnitsToAdd">
        /// Amount to add to <see cref="BaseValue"/>. Can be negative or positive.
        /// </param>
        /// <returns>
        /// A new <see cref="Amount"/> whose value is the sum of this instance
        /// and the number of base units specified by <paramref name="baseUnitsToAdd"/>.
        /// </returns>
        /// <remarks>
        /// This method does not change the value of this <see cref="Amount"/> object.
        /// Instead it returns a new <see cref="Amount"/> object whose value is the result of this operation.
        /// </remarks>
        public Amount AddBaseUnits(long baseUnitsToAdd)
        {
            checked
            {
                return new Amount(BaseDenom, BaseValue + baseUnitsToAdd, GameDenom);
            }
        }

        /// <summary>
        /// Returns a new <see cref="Amount"/> that adds the specified number of credits to
        /// <see cref="GameCreditValue"/> of this instance.
        /// </summary>
        /// <param name="creditsToAdd">
        /// Amount to add in units of the game denomination. Can be negative or positive.
        /// </param>
        /// <returns>
        /// A new <see cref="Amount"/> whose value is the sum of this instance
        /// and the number of credits specified by <paramref name="creditsToAdd"/>.
        /// </returns>
        /// <remarks>
        /// This method does not change the value of this <see cref="Amount"/> object.
        /// Instead it returns a new <see cref="Amount"/> object whose value is the result of this operation.
        /// </remarks>
        public Amount AddGameCredits(long creditsToAdd)
        {
            checked
            {
                return AddBaseUnits(creditsToAdd * GameDenom);
            }
        }

        #endregion

        #region Equality members

        // Generated by ReSharper

        /// <inheritdoc/>
        public bool Equals(Amount other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            if(ReferenceEquals(this, other))
            {
                return true;
            }
            return BaseDenom == other.BaseDenom && GameDenom == other.GameDenom && BaseValue == other.BaseValue;
        }

        /// <inheritdoc/>
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
            if(obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((Amount)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = BaseDenom.GetHashCode();
                hashCode = (hashCode * 397) ^ GameDenom.GetHashCode();
                hashCode = (hashCode * 397) ^ BaseValue.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Overloads the operator ==.
        /// </summary>
        public static bool operator ==(Amount left, Amount right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Overloads the operator !=.
        /// </summary>
        public static bool operator !=(Amount left, Amount right)
        {
            return !Equals(left, right);
        }

        #endregion

        #region Relational members

        // Generated by ReSharper

        /// <inheritdoc/>
        public int CompareTo(Amount other)
        {
            if(ReferenceEquals(this, other))
            {
                return 0;
            }
            if(ReferenceEquals(null, other))
            {
                return 1;
            }

            return CurrencyValue.CompareTo(other.CurrencyValue);
        }

        /// <summary>
        /// Overloads the operator "less than".
        /// </summary>
        public static bool operator <(Amount left, Amount right)
        {
            return Comparer<Amount>.Default.Compare(left, right) < 0;
        }

        /// <summary>
        /// Overloads the operator >.
        /// </summary>
        public static bool operator >(Amount left, Amount right)
        {
            return Comparer<Amount>.Default.Compare(left, right) > 0;
        }

        /// <summary>
        /// Overloads the operator "less than or equals to".
        /// </summary>
        public static bool operator <=(Amount left, Amount right)
        {
            return Comparer<Amount>.Default.Compare(left, right) <= 0;
        }

        /// <summary>
        /// Overloads the operator >=.
        /// </summary>
        public static bool operator >=(Amount left, Amount right)
        {
            return Comparer<Amount>.Default.Compare(left, right) >= 0;
        }

        #endregion
    }
}