using System;

namespace Midas.Core.General
{
	/// <summary>
	/// Struct holds a credit value. Internally stored as a rational number.
	/// </summary>
	public readonly struct Credit : IEquatable<Credit>, IComparable<Credit>
	{
		#region Constants

		private const long SubCreditsFactor = 100;

		/// <summary>
		/// Zero credits value.
		/// </summary>
		public static Credit Zero { get; } = FromLong(0);

		#endregion

		#region Properties

		/// <summary>
		/// The number of credits this instance represents.
		/// </summary>
		public long Credits => Value.Numerator / Value.Denominator;

		/// <summary>
		/// Gets if the credit value is zero.
		/// </summary>
		public bool IsZero => Value.Numerator == 0;

		/// <summary>
		/// Gets the rational number that is the core of this credit value.
		/// </summary>
		public RationalNumber Value { get; }

		/// <summary>
		/// When created from Money, SubCredits indicates that the Credit value contains some partial credit.
		/// </summary>
		/// <remarks>
		/// This is primarily used when enrolling a game for credit playoff.
		/// </remarks>
		public long SubCredits
		{
			get
			{
				var totalSubCredits = Value * new RationalNumber(100, 1);
				return totalSubCredits.Numerator / totalSubCredits.Denominator - Credits * SubCreditsFactor;
			}
		}

		/// <summary>
		/// Indicates that this credit value has some partial credit.
		/// </summary>
		public bool HasSubCredits => SubCredits != 0;

		/// <summary>
		/// Gets whether the credit is a valid value.
		/// </summary>
		public bool IsValid => Value.IsValid;

		#endregion

		#region Construction

		/// <summary>
		/// Creates a credit value from a long.
		/// </summary>
		/// <param name="credit">The credit long.</param>
		/// <returns>A new Credit instance.</returns>
		public static Credit FromLong(long credit) => FromRationalNumber(credit, 1);

		/// <summary>
		/// Creates a credit value from a ulong.
		/// </summary>
		/// <param name="credit">The credit ulong.</param>
		/// <returns>A new Credit instance.</returns>
		public static Credit FromULong(ulong credit) => FromRationalNumber((long)credit, 1);

		/// <summary>
		/// Creates a credit value from a Money value. This uses Money.Denomination to calculate the credit value.
		/// </summary>
		/// <param name="money">The money value.</param>
		/// <returns>A new Credit instance.</returns>
		public static Credit FromMoney(Money money) => FromRationalNumber(money.AsMinorCurrency, Money.Denomination.AsMinorCurrency);

		/// <summary>
		/// Creates a credit value from a rational number.
		/// </summary>
		/// <param name="numerator">The numerator of the rational number.</param>
		/// <param name="denominator">The denominator of the rational number</param>
		/// <returns>A new credit value.</returns>
		public static Credit FromRationalNumber(long numerator, long denominator) => new Credit(new RationalNumber(numerator, denominator));

		/// <summary>
		/// Constructor is private. Use one of the "From*" static methods.
		/// </summary>
		private Credit(RationalNumber value)
		{
			Value = value;
		}

		/// <summary>
		/// Convenient helper to convert to money
		/// </summary>
		public Money ToMoney() => Money.FromCredit(this);

		#endregion

		#region IEquatable<Credit> and IComparable<Credit> implementation

		public int CompareTo(Credit other) => Value.CompareTo(other.Value);

		public bool Equals(Credit other) => Value.Equals(other.Value);

		#endregion

		#region Opertators

		public static bool operator ==(Credit lhs, Credit rhs) => lhs.Equals(rhs);

		public static bool operator !=(Credit lhs, Credit rhs) => !lhs.Equals(rhs);

		public static Credit operator +(Credit a, Credit b) => new Credit(a.Value + b.Value);

		public static Credit operator -(Credit a, Credit b) => new Credit(a.Value - b.Value);

		public static bool operator >(Credit a, Credit b) => a.Value > b.Value;

		public static bool operator >=(Credit a, Credit b) => a.Value >= b.Value;

		public static bool operator <=(Credit a, Credit b) => a.Value <= b.Value;

		public static bool operator <(Credit a, Credit b) => a.Value < b.Value;

		#endregion

		#region Object overrides

		public override bool Equals(object obj) => obj is Credit other && Equals(other);

		public override string ToString()
		{
			return Value.ToString();
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		#endregion
	}
}