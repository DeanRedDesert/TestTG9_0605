using System;

namespace Midas.Core.General
{
	/// <summary>
	/// Struct holds a money value including minor currency (e.g. cents)
	/// </summary>
	public readonly struct Money : IEquatable<Money>, IComparable<Money>
	{
		#region Constants

		public static Money Zero { get; } = FromRationalNumber(0, 1);
		private const long MinorCurrencyFactor = 100;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the denomination the EGM is running in.
		/// </summary>
		/// <remarks>
		/// This is configured by the framework just before the game engine is started.
		/// This will occur on startup or any time a denom change or game reconfiguration happens.
		/// </remarks>
		public static Money Denomination { get; internal set; } = FromRationalNumber(1, 1);

		/// <summary>
		/// Gets the major currency value, i.e. the dollar value.
		/// </summary>
		public long MajorCurrency => Value.Numerator / Value.Denominator;

		/// <summary>
		/// Gets the minor currency value, i.e. the cents value.
		/// </summary>
		public long MinorCurrency => AsMinorCurrency - MajorCurrency * MinorCurrencyFactor;

		/// <summary>
		/// Gets the total minor currency value.
		/// </summary>
		public long AsMinorCurrency
		{
			get
			{
				var totalMinorCurrency = Value * new RationalNumber(MinorCurrencyFactor, 1);
				return totalMinorCurrency.Numerator / totalMinorCurrency.Denominator;
			}
		}

		/// <summary>
		/// Gets the rational number at the core of this instance.
		/// </summary>
		public RationalNumber Value { get; }

		/// <summary>
		/// Gets whether the money value is zero.
		/// </summary>
		public bool IsZero => Value.IsZero;

		/// <summary>
		/// Gets whether the money value is negative.
		/// </summary>
		public bool IsNegative => Value.IsNegative;

		/// <summary>
		/// Gets whether the money is a valid value.
		/// </summary>
		public bool IsValid => Value.IsValid;

		/// <summary>
		/// When created from a RationalNumber, SubMinorCurrency indicates that the Money value contains some partial minor currenct (ie, partial cents).
		/// </summary>
		/// <remarks>
		/// This is primarily used for scrolling meters.
		/// </remarks>
		public RationalNumber SubMinorCurrency
		{
			get
			{
				var totalMinorCurrency = new RationalNumber(AsMinorCurrency, 1);
				return Value * new RationalNumber(MinorCurrencyFactor, 1) - totalMinorCurrency;
			}
		}

		#endregion

		#region Construction

		/// <summary>
		/// Create a new instance using major and minor currency.
		/// </summary>
		/// <param name="major">The major currency value.</param>
		/// <param name="minor">The minor currency value.</param>
		/// <returns>A new Money instance.</returns>
		public static Money FromMajorAndMinorCurrency(long major, long minor)
		{
			return FromMinorCurrency(minor) + FromMajorCurrency(major);
		}

		/// <summary>
		/// Create a new instance using minor currency, i.e. cents.
		/// </summary>
		/// <param name="money">The minor currency value.</param>
		/// <returns>A new Money instance.</returns>
		public static Money FromMinorCurrency(long money)
		{
			return FromRationalNumber(money, MinorCurrencyFactor);
		}

		/// <summary>
		/// Create a new instance using major currency, i.e. dollars.
		/// </summary>
		/// <param name="money">The major currency value.</param>
		/// <returns>A new Money instance.</returns>
		public static Money FromMajorCurrency(long money)
		{
			return FromRationalNumber(money, 1);
		}

		/// <summary>
		/// Create a new instance using a credit value.
		/// </summary>
		/// <param name="credit">The credit value.</param>
		/// <returns>A new Money instance (credit * denomination).</returns>
		public static Money FromCredit(Credit credit)
		{
			return new Money(credit.Value * Denomination.Value);
		}

		/// <summary>
		/// Create a new instance directly from a rational number.
		/// </summary>
		/// <param name="numerator">The numerator of the rational number.</param>
		/// <param name="denominator">The denominator of the rational number.</param>
		/// <returns>A new money instance.</returns>
		public static Money FromRationalNumber(long numerator, long denominator)
		{
			return FromRationalNumber(new RationalNumber(numerator, denominator));
		}

		/// <summary>
		/// Create a new instance directly from a rational number.
		/// </summary>
		/// <param name="value">The rational number.</param>
		/// <returns>A new money instance.</returns>
		public static Money FromRationalNumber(RationalNumber value)
		{
			return new Money(value);
		}

		/// <summary>
		/// Private constructor. Use one of the "From*" static methods.
		/// </summary>
		private Money(RationalNumber value)
		{
			Value = value;
		}

		/// <summary>
		/// Convenient helper to convert to credit
		/// </summary>
		public Credit ToCredit() => Credit.FromMoney(this);

		#endregion

		#region IEquatable<Money> and IComparable<Money> implementation

		public int CompareTo(Money other)
		{
			return Value.CompareTo(other.Value);
		}

		public bool Equals(Money other)
		{
			return Value.Equals(other.Value);
		}

		#endregion

		#region Operators

		public static bool operator ==(Money lhs, Money rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(Money lhs, Money rhs)
		{
			return !lhs.Equals(rhs);
		}

		public static Money operator +(Money a, Money b)
		{
			return new Money(a.Value + b.Value);
		}

		public static Money operator -(Money a, Money b)
		{
			return new Money(a.Value - b.Value);
		}

		public static Money operator *(Money a, RationalNumber b)
		{
			return new Money(a.Value * b);
		}

		public static bool operator >(Money a, Money b)
		{
			return a.Value > b.Value;
		}

		public static bool operator >=(Money a, Money b)
		{
			return a.Value >= b.Value;
		}

		public static bool operator <=(Money a, Money b)
		{
			return a.Value <= b.Value;
		}

		public static bool operator <(Money a, Money b)
		{
			return a.Value < b.Value;
		}

		#endregion

		#region Object overrides

		public override string ToString()
		{
			return Value.ToString();
		}

		public override bool Equals(object obj)
		{
			return obj is Money other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		#endregion
	}
}