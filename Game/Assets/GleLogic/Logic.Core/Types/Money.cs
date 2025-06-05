using System;
using Logic.Core.Utility;

namespace Logic.Core.Types
{
	/// <summary>
	/// This struct represents a monetary value.
	/// </summary>
	/// <remarks>
	/// This type does not perform any rollover checks and fall's back on default system behaviour under such circumstances.
	/// </remarks>
	public readonly struct Money : IComparable, IComparable<Money>, IEquatable<Money>, IToString, IFromString, IToCode
	{
		#region Fields

		/// <summary>
		/// A read-only field that represents the largest possible value of <see cref="Money"/>.
		/// </summary>
		// ReSharper disable once UnusedMember.Global
		public static readonly Money MaxValue = new Money(ulong.MaxValue);

		/// <summary>
		/// A read-only field that represents a <see cref="Money"/> instance that has been initialized to zero.
		/// </summary>
		public static readonly Money Zero = default;

		private readonly ulong cents;

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a new <see cref="Money"/> instance from the specified cents.
		/// </summary>
		/// <param name="cents">The value in cents.</param>
		public Money(ulong cents)
		{
			this.cents = cents;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Creates a new <see cref="Money"/> instance from the specified units.
		/// </summary>
		/// <param name="value">The value in units.</param>
		/// <returns>A new <see cref="Money"/> instance.</returns>
		public static Money FromCents(ulong value)
		{
			return new Money(value);
		}

		/// <summary>
		/// Convert to a <see cref="ulong"/>, in units.
		/// </summary>
		/// <returns>The value in units.</returns>
		public ulong ToCents()
		{
			return cents;
		}

		/// <summary>
		/// Convert to an <see cref="Credits"/> object.
		/// </summary>
		/// <param name="baseCreditValue">The value of a credit.</param>
		/// <returns>A new <see cref="Credits"/> object.</returns>
		public Credits ToCredits(Money baseCreditValue)
		{
			return new Credits(this, baseCreditValue);
		}

		/// <summary>
		/// Equality test.
		/// </summary>
		/// <param name="x">The first <see cref="Money"/> instance.</param>
		/// <param name="y">The second <see cref="Money"/> instance.</param>
		/// <returns>True if the two <see cref="Money"/> instances are equal, otherwise false.</returns>
		public static bool operator ==(Money x, Money y)
		{
			return x.cents == y.cents;
		}

		/// <summary>
		/// Inequality test.
		/// </summary>
		/// <param name="x">The first <see cref="Money"/> instance.</param>
		/// <param name="y">The second <see cref="Money"/> instance.</param>
		/// <returns>True if the two <see cref="Money"/> instances are not equal, otherwise false.</returns>
		public static bool operator !=(Money x, Money y)
		{
			return !(x == y);
		}

		/// <summary>
		/// Less-than test.
		/// </summary>
		/// <param name="x">The first <see cref="Money"/> instance.</param>
		/// <param name="y">The second <see cref="Money"/> instance.</param>
		/// <returns>True if the first <see cref="Money"/> instance is less than the second one, otherwise false.</returns>
		public static bool operator <(Money x, Money y)
		{
			return x.cents < y.cents;
		}

		/// <summary>
		/// Greater-than test.
		/// </summary>
		/// <param name="x">The first <see cref="Money"/> instance.</param>
		/// <param name="y">The second <see cref="Money"/> instance.</param>
		/// <returns>True if the first <see cref="Money"/> instance is greater than the second one, otherwise false.</returns>
		public static bool operator >(Money x, Money y)
		{
			return x.cents > y.cents;
		}

		/// <summary>
		/// Less-than-or-equal test.
		/// </summary>
		/// <param name="x">The first <see cref="Money"/> instance.</param>
		/// <param name="y">The second <see cref="Money"/> instance.</param>
		/// <returns>True if the first <see cref="Money"/> instance is less than or equal to the second one, otherwise false.</returns>
		public static bool operator <=(Money x, Money y)
		{
			return x.cents <= y.cents;
		}

		/// <summary>
		/// Greater-than-or-equal test.
		/// </summary>
		/// <param name="x">The first <see cref="Money"/> instance.</param>
		/// <param name="y">The second <see cref="Money"/> instance.</param>
		/// <returns>True if the first <see cref="Money"/> instance is greater than or equal to the second one, otherwise false.</returns>
		public static bool operator >=(Money x, Money y)
		{
			return x.cents >= y.cents;
		}

		/// <summary>
		/// The addition operator for <see cref="Money"/>.
		/// </summary>
		/// <param name="lhs">The first <see cref="Money"/> instance.</param>
		/// <param name="rhs">The second <see cref="Money"/> instance.</param>
		/// <returns>The new <see cref="Money"/> instance.</returns>
		/// <remarks>
		/// The addition is unchecked so watch out for overflows.
		/// In C# the compound assignment operator += is automatically overloaded when this operator is overloaded.
		/// </remarks>
		public static Money operator +(Money lhs, Money rhs)
		{
			return new Money(unchecked(lhs.ToCents() + rhs.ToCents()));
		}

		/// <summary>
		/// The subtraction operator for <see cref="Money"/>.
		/// </summary>
		/// <param name="lhs">The first <see cref="Money"/> instance.</param>
		/// <param name="rhs">The second <see cref="Money"/> instance.</param>
		/// <returns>The new <see cref="Money"/> instance.</returns>
		/// <remarks>
		/// The subtraction is unchecked so watch out for underflow.
		/// In C# the compound assignment operator -= is automatically overloaded when this operator is overloaded.
		/// </remarks>
		public static Money operator -(Money lhs, Money rhs)
		{
			return new Money(unchecked(lhs.ToCents() - rhs.ToCents()));
		}

		/// <summary>
		/// The multiplication operator for <see cref="Money"/>.
		/// </summary>
		/// <param name="lhs">The first operand of type <see cref="Money"/>.</param>
		/// <param name="rhs">The second operand which is a numeric value.</param>
		/// <returns>The new <see cref="Money"/> instance.</returns>
		/// <remarks>
		/// The multiplication is unchecked so watch out for overflows.
		/// In C# the compound assignment operator *= is automatically overloaded when this operator is overloaded.
		/// Hence the flip side of this operator with lhs of type <see cref="ulong"/> and rhs of <see cref="Money"/> is not supported.
		/// </remarks>
		public static Money operator *(Money lhs, ulong rhs)
		{
			return new Money(unchecked(lhs.ToCents() * rhs));
		}

		/// <summary>
		/// The multiplication operator for <see cref="Money"/>.
		/// </summary>
		/// <param name="lhs">The first operand of type <see cref="Money"/>.</param>
		/// <param name="rhs">The second operand which is a numeric value.</param>
		/// <returns>The new <see cref="Money"/> instance.</returns>
		/// <remarks>
		/// The multiplication is unchecked so watch out for overflows.
		/// In C# the compound assignment operator *= is automatically overloaded when this operator is overloaded.
		/// Hence the flip side of this operator with lhs of type <see cref="ulong"/> and rhs of <see cref="Money"/> is not supported.
		/// </remarks>
		public static Money operator *(Money lhs, long rhs)
		{
			return lhs * (ulong)rhs;
		}

		/// <summary>
		/// The division operator for <see cref="Money"/>.
		/// </summary>
		/// <param name="dividend">The dividend Money.</param>
		/// <param name="divisor">The second Money.</param>
		/// <returns>The new <see cref="Money"/> instance.</returns>
		/// <remarks>
		/// In C# the compound assignment operator /= is automatically overloaded when this operator is overloaded.
		/// </remarks>
		public static ulong operator /(Money dividend, Money divisor)
		{
			return dividend.ToCents() / divisor.ToCents();
		}

		/// <summary>
		/// Determines whether the specified object is equal to this instance.
		/// </summary>
		/// <param name="obj">The object to test.</param>
		/// <returns>True if the specified object is equal to this instance, otherwise false.</returns>
		public override bool Equals(object obj)
		{
			return obj is Money money && this == money;
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode()
		{
			return cents.GetHashCode();
		}

		#endregion

		#region IComparable

		/// <summary>
		/// Compares the current object with another object of the same type.
		/// </summary>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public int CompareTo(Money other)
		{
			return cents.CompareTo(other.cents);
		}

		#endregion

		#region IComparable Members

		/// <summary>
		/// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
		/// </summary>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than <paramref name="obj"/>. Zero This instance is equal to <paramref name="obj"/>. Greater than zero This instance is greater than <paramref name="obj"/>.
		/// </returns>
		/// <param name="obj">An object to compare with this instance. </param>
		/// <exception cref="T:System.ArgumentException"><paramref name="obj"/> is not the same type as this instance. </exception>
		public int CompareTo(object obj)
		{
			switch (obj)
			{
				case null: return 1;
				case Money m: return CompareTo(m);
				default: throw new ArgumentException("Money required");
			}
		}

		#endregion

		#region IEquatable<Money> Members

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(Money other)
		{
			return this == other;
		}

		#endregion

		#region IToString Members

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format)
		{
			if (string.IsNullOrEmpty(format))
				format = "G";

			switch (format)
			{
				case "SL":
				case "Z":
				{
					if (cents == 0)
						return "$0".ToSuccess();

					if (cents < 100)
						return $"{cents}c".ToSuccess();

					return cents % 100 == 0
						? $"${cents / 100}".ToSuccess()
						: $"${cents / 100.0:0.00}".ToSuccess();
				}
				case "C": return (cents / (double)100).ToString("C").ToSuccess();
				case "C0": return (cents / (double)100).ToString("C0").ToSuccess();
				case "G": return cents.ToString().ToSuccess(); // .Net 4.8 doesn't support ToString(CultureInfo)
				default: return new Error($"The {format} format string is not supported.");
			}
		}

		#endregion

		#region IFromString Members

		/// <summary>
		/// Parses a string into a <see cref="Money"/> object.
		/// Examples:
		/// '10c' => 10
		/// '$2' => 200
		/// '$1.5' => 150
		/// '$1.50' => 150
		/// '1000' => 1000
		/// </summary>
		// ReSharper disable once UnusedMember.Global
		public static IResult FromString(string s, string format)
		{
			var normalised = s.Trim().ToLower();

			if (normalised.StartsWith("$"))
			{
				var trimmed = normalised.Substring(1, normalised.Length - 1);

				// .Net 4.8 doesn't support ulong.TryParse(string, CultureInfo, out ulong) or double.TryParse(string, CultureInfo, out double)
				return ulong.TryParse(trimmed, out var wholeNumber)
					? new Money(wholeNumber * 100).ToSuccess()
					: double.TryParse(trimmed, out var fractionalNumber)
						? new Money((ulong)(fractionalNumber * 100)).ToSuccess()
						: new Error($"Could not convert '{s}' to a Money value");
			}

			if (normalised.EndsWith("c"))
			{
				var trimmed = normalised.Substring(0, normalised.Length - 1);

				// .Net 4.8 doesn't support ulong.TryParse(string, CultureInfo, out ulong)
				return ulong.TryParse(trimmed, out var cents)
					? new Money(cents).ToSuccess()
					: new Error($"Could not convert '{s}' to a Money value");
			}

			return new Error($"Could not convert '{s}' to a Money value");
		}

		#endregion

		#region IToCode Members

		/// <inheritdoc cref="IToCode.ToCode(CodeGenArgs?)" />
		public IResult ToCode(CodeGenArgs args) => $"new {CodeConverter.ToCode<Money>(args)}({ToCents()}UL)".ToSuccess();

		#endregion
	}
}