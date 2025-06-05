using System;
using Logic.Core.Utility;

namespace Logic.Core.Types
{
	/// <summary>
	/// This struct represents a credit value.
	/// </summary>
	/// <remarks>
	/// This type does not perform any rollover checks and fall's back on default system behaviour under such circumstances.
	/// </remarks>
	public readonly struct Credits : IComparable, IComparable<Credits>, IEquatable<Credits>, IToString, IFromString, IToCode
	{
		#region Fields

		/// <summary>
		/// A read-only field that represents the largest possible value of <see cref="Credits"/>.
		/// </summary>
		// ReSharper disable once UnusedMember.Global
		public static readonly Credits MaxValue = new Credits(ulong.MaxValue);

		/// <summary>
		/// A read-only field that represents a <see cref="Credits"/> instance that has been initialized to zero.
		/// </summary>
		public static readonly Credits Zero = default;

		private readonly ulong value;

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="value">The number of credits this object represents.</param>
		public Credits(ulong value)
		{
			this.value = value;
		}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="monetaryValue">The Money value to use.</param>
		/// <param name="creditValue">The value of a credit.</param>
		public Credits(Money monetaryValue, Money creditValue)
		{
			value = monetaryValue.ToCents() / creditValue.ToCents();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Convert to a <see cref="Money"/> object.
		/// </summary>
		/// <param name="moneyPerCreditValue">The value of a credit.</param>
		/// <returns>A new <see cref="Money"/> object.</returns>
		public Money ToMoney(Money moneyPerCreditValue)
		{
			return new Money(value * moneyPerCreditValue.ToCents());
		}

		/// <summary>
		/// Converts to a <see cref="ulong"/>.
		/// </summary>
		/// <returns>The credit value.</returns>
		public ulong ToUInt64()
		{
			return value;
		}

		/// <summary>
		/// Equality test.
		/// </summary>
		/// <param name="x">The first <see cref="Credits"/> instance.</param>
		/// <param name="y">The second <see cref="Credits"/> instance.</param>
		/// <returns>True if the two <see cref="Credits"/> instances are equal, otherwise false.</returns>
		public static bool operator ==(Credits x, Credits y)
		{
			return x.value == y.value;
		}

		/// <summary>
		/// Inequality test.
		/// </summary>
		/// <param name="x">The first <see cref="Credits"/> instance.</param>
		/// <param name="y">The second <see cref="Credits"/> instance.</param>
		/// <returns>True if the two <see cref="Credits"/> instances are not equal, otherwise false.</returns>
		public static bool operator !=(Credits x, Credits y)
		{
			return !(x == y);
		}

		/// <summary>
		/// Less-than test.
		/// </summary>
		/// <param name="x">The first <see cref="Credits"/> instance.</param>
		/// <param name="y">The second <see cref="Credits"/> instance.</param>
		/// <returns>True if the first <see cref="Credits"/> instance is less than the second one, otherwise false.</returns>
		public static bool operator <(Credits x, Credits y)
		{
			return x.value < y.value;
		}

		/// <summary>
		/// Greater-than test.
		/// </summary>
		/// <param name="x">The first <see cref="Credits"/> instance.</param>
		/// <param name="y">The second <see cref="Credits"/> instance.</param>
		/// <returns>True if the first <see cref="Credits"/> instance is greater than the second one, otherwise false.</returns>
		public static bool operator >(Credits x, Credits y)
		{
			return x.value > y.value;
		}

		/// <summary>
		/// Less-than-or-equal test.
		/// </summary>
		/// <param name="x">The first <see cref="Credits"/> instance.</param>
		/// <param name="y">The second <see cref="Credits"/> instance.</param>
		/// <returns>True if the first <see cref="Credits"/> instance is less than or equal to the second one, otherwise false.</returns>
		public static bool operator <=(Credits x, Credits y)
		{
			return x.value <= y.value;
		}

		/// <summary>
		/// Greater-than-or-equal test.
		/// </summary>
		/// <param name="x">The first <see cref="Credits"/> instance.</param>
		/// <param name="y">The second <see cref="Credits"/> instance.</param>
		/// <returns>True if the first <see cref="Credits"/> instance is greater than or equal to the second one, otherwise false.</returns>
		public static bool operator >=(Credits x, Credits y)
		{
			return x.value >= y.value;
		}

		/// <summary>
		/// The addition operator for <see cref="Credits"/>.
		/// </summary>
		/// <param name="lhs">The first <see cref="Credits"/> instance.</param>
		/// <param name="rhs">The second <see cref="Credits"/> instance.</param>
		/// <returns>The new <see cref="Credits"/> instance.</returns>
		/// <remarks>
		/// The addition is unchecked so watch out for overflows.
		/// In C# the compound assignment operator += is automatically overloaded when this operator is overloaded.
		/// </remarks>
		public static Credits operator +(Credits lhs, Credits rhs)
		{
			return new Credits(unchecked(lhs.value + rhs.value));
		}

		/// <summary>
		/// The subtraction operator for <see cref="Credits"/>.
		/// </summary>
		/// <param name="lhs">The first <see cref="Credits"/> instance.</param>
		/// <param name="rhs">The second <see cref="Credits"/> instance.</param>
		/// <returns>The new <see cref="Credits"/> instance.</returns>
		/// <remarks>
		/// The subtraction is unchecked so watch out for underflow.
		/// In C# the compound assignment operator -= is automatically overloaded when this operator is overloaded.
		/// </remarks>
		public static Credits operator -(Credits lhs, Credits rhs)
		{
			return new Credits(unchecked(lhs.value - rhs.value));
		}

		/// <summary>
		/// The multiplication operator for <see cref="Credits"/>.
		/// </summary>
		/// <param name="lhs">The first Credit.</param>
		/// <param name="rhs">The second Credit.</param>
		/// <returns>The new <see cref="Credits"/> instance.</returns>
		/// <remarks>
		/// The multiplication is unchecked so watch out for overflows.
		/// In C# the compound assignment operator *= is automatically overloaded when this operator is overloaded.
		/// </remarks>
		public static Credits operator *(Credits lhs, Credits rhs)
		{
			return new Credits(unchecked(lhs.value * rhs.value));
		}

		/// <summary>
		/// The multiplication operator for <see cref="Credits"/>.
		/// </summary>
		/// <param name="lhs">The first Credit.</param>
		/// <param name="rhs">The second Credit.</param>
		/// <returns>The new <see cref="Credits"/> instance.</returns>
		/// <remarks>
		/// The multiplication is unchecked so watch out for overflows.
		/// In C# the compound assignment operator *= is automatically overloaded when this operator is overloaded.
		/// Hence the flip side of this operator with lhs of type <see cref="ulong"/> and rhs of <see cref="Credits"/> is not supported.
		/// </remarks>
		public static Credits operator *(Credits lhs, ulong rhs)
		{
			return new Credits(unchecked(lhs.value * rhs));
		}

		/// <summary>
		/// The multiplication operator for <see cref="Credits"/>.
		/// </summary>
		/// <param name="lhs">The first Credit.</param>
		/// <param name="rhs">The second Credit.</param>
		/// <returns>The new <see cref="Credits"/> instance.</returns>
		/// <remarks>
		/// The multiplication is unchecked so watch out for overflows.
		/// In C# the compound assignment operator *= is automatically overloaded when this operator is overloaded.
		/// Hence the flip side of this operator with lhs of type <see cref="long"/> and rhs of <see cref="Credits"/> is not supported.
		/// </remarks>
		public static Credits operator *(Credits lhs, long rhs)
		{
			return new Credits(unchecked(lhs.value * (ulong)rhs));
		}

		/// <summary>
		/// The division operator for <see cref="Credits"/>.
		/// </summary>
		/// <param name="dividend">The dividend Credit.</param>
		/// <param name="divisor">The second Credit.</param>
		/// <returns>The new <see cref="Credits"/> instance.</returns>
		/// <remarks>
		/// In C# the compound assignment operator /= is automatically overloaded when this operator is overloaded.
		/// </remarks>
		public static Credits operator /(Credits dividend, Credits divisor)
		{
			return new Credits(dividend.value / divisor.value);
		}

		/// <summary>
		/// Determines whether the specified object is equal to this instance.
		/// </summary>
		/// <param name="obj">The object to test.</param>
		/// <returns>True if the specified object is equal to this instance, otherwise false.</returns>
		public override bool Equals(object obj)
		{
			return obj is Credits credits && this == credits;
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode()
		{
			return value.GetHashCode();
		}

		#endregion

		#region IComparable

		/// <inheritdoc cref="IComparable.CompareTo(object)" />
		public int CompareTo(object obj)
		{
			switch (obj)
			{
				case null: return 1;
				case Credits c: return CompareTo(c);
				default: throw new ArgumentException("Credits required");
			}
		}

		/// <inheritdoc cref="IComparable{T}.CompareTo(T)" />
		public int CompareTo(Credits other)
		{
			return value.CompareTo(other.value);
		}

		#endregion

		#region IEquatable<Credits> Members

		/// <inheritdoc cref="IEquatable{T}.Equals(T?)" />
		public bool Equals(Credits other)
		{
			return this == other;
		}

		#endregion

		#region IToString Members

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format) => $"{value}cr".ToSuccess();

		#endregion

		#region IFromString Members

		/// <summary>Implementation of IFromString.FromString(string?, string?)</summary>
		// ReSharper disable once UnusedMember.Global
		public static IResult FromString(string s, string format)
		{
			var normalised = s.Trim().ToLower();

			if (normalised.EndsWith("cr"))
				normalised = normalised.Substring(0, normalised.Length - 2);

			// .Net 4.8 doesn't support ulong.TryParse(string, CultureInfo, out ulong)
			return ulong.TryParse(normalised, out var v)
				? new Credits(v).ToSuccess()
				: new Error($"Could not convert '{s}' to a Credits value");
		}

		#endregion

		#region IToCode Members

		/// <inheritdoc cref="IToCode.ToCode(CodeGenArgs?)" />
		public IResult ToCode(CodeGenArgs args) => $"new {CodeConverter.ToCode<Credits>(args)}({ToUInt64()}UL)".ToSuccess();

		#endregion
	}
}