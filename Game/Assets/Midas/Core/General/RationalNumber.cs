using System;
using System.Globalization;

namespace Midas.Core.General
{
	/// <summary>
	/// Rational number implementation.
	/// </summary>
	public readonly struct RationalNumber : IEquatable<RationalNumber>, IComparable<RationalNumber>, IComparable
	{
		#region Properties

		/// <summary>
		/// Gets the numerator of the rational number.
		/// </summary>
		public long Numerator { get; }

		/// <summary>
		/// Gets the denominator of the rational number.
		/// </summary>
		public long Denominator { get; }

		/// <summary>
		/// Gets whether the number is negative.
		/// </summary>
		public bool IsNegative
		{
			get
			{
				var nN = Numerator < 0.0f;
				var dN = Denominator < 0.0f;
				return nN ^ dN;
			}
		}

		/// <summary>
		/// Gets whether the number is valid.
		/// </summary>
		public bool IsValid => Denominator != 0;

		/// <summary>
		/// Gets whether the number is zero.
		/// </summary>
		public bool IsZero => Numerator == 0;

		#endregion

		#region Construction

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="numerator">numerator of the rational number.</param>
		/// <param name="denominator">denominator of the rational number.</param>
		/// <exception cref="ArgumentException">Thrown if the denominator is zero.</exception>
		public RationalNumber(long numerator, long denominator)
		{
			if (denominator == 0)
			{
				throw new ArgumentException("Denominator cannot be 0.");
			}

			var gcd = GreatestCommonDivisor(numerator, denominator);
			Numerator = numerator / gcd;
			Denominator = denominator / gcd;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Returns the absolute value of the rational number.
		/// </summary>
		public RationalNumber Abs()
		{
			return new RationalNumber(Math.Abs(Numerator), Math.Abs(Denominator));
		}

		/// <summary>
		/// Converts the rational number to a float.
		/// </summary>
		public double ToDouble()
		{
			return (double)Numerator / Denominator;
		}

		private static long GreatestCommonDivisor(long a, long b)
		{
			while (b != 0)
				b = a % (a = b);

			return a;
		}

		#endregion

		#region Operators

		public static RationalNumber operator +(RationalNumber r1, RationalNumber r2)
		{
			var den = r2.Denominator == r1.Denominator ? r1.Denominator : r2.Denominator * r1.Denominator;
			return new RationalNumber(r1.Numerator * (den / r1.Denominator) + r2.Numerator * (den / r2.Denominator), den);
		}

		public static RationalNumber Add(RationalNumber r1, RationalNumber r2)
		{
			var den = r2.Denominator == r1.Denominator ? r1.Denominator : r2.Denominator * r1.Denominator;
			return new RationalNumber(r1.Numerator * (den / r1.Denominator) + r2.Numerator * (den / r2.Denominator), den);
		}

		public static RationalNumber operator -(RationalNumber r1, RationalNumber r2)
		{
			var den = r2.Denominator == r1.Denominator ? r1.Denominator : r2.Denominator * r1.Denominator;
			return new RationalNumber(r1.Numerator * (den / r1.Denominator) - r2.Numerator * (den / r2.Denominator), den);
		}

		public static RationalNumber operator *(RationalNumber r1, RationalNumber r2)
		{
			return new RationalNumber(r1.Numerator * r2.Numerator, r1.Denominator * r2.Denominator);
		}

		public static RationalNumber operator /(RationalNumber r1, RationalNumber r2)
		{
			return new RationalNumber(r1.Numerator * r2.Denominator, r2.Numerator * r1.Denominator);
		}

		public static bool operator ==(RationalNumber r1, RationalNumber r2)
		{
			return r1.Equals(r2);
		}

		public static bool operator !=(RationalNumber r1, RationalNumber r2)
		{
			return !r1.Equals(r2);
		}

		public static bool operator <(RationalNumber r1, RationalNumber r2)
		{
			return r1.CompareTo(r2) < 0;
		}

		public static bool operator <=(RationalNumber r1, RationalNumber r2)
		{
			return r1.CompareTo(r2) <= 0;
		}

		public static bool operator >(RationalNumber r1, RationalNumber r2)
		{
			return r1.CompareTo(r2) > 0;
		}

		public static bool operator >=(RationalNumber r1, RationalNumber r2)
		{
			return r1.CompareTo(r2) >= 0;
		}

		#endregion

		#region IEquatable<RationalNumber>, IComparable<RationalNumber> and IComparable implementation

		public bool Equals(RationalNumber other)
		{
			return Numerator == other.Numerator && Denominator == other.Denominator;
		}

		public int CompareTo(object obj)
		{
			return obj is RationalNumber other ? CompareTo(other) : throw new ArgumentException();
		}

		public int CompareTo(RationalNumber other)
		{
			var a = Numerator;
			var b = Denominator;
			var c = other.Numerator;
			var d = other.Denominator;

			// Equality defined by: a * sgn(b) * abs(d) < abs(b) * c * sgn(d)

			return (a * Math.Sign(b) * Math.Abs(d)).CompareTo(Math.Abs(b) * c * Math.Sign(d));
		}

		#endregion

		#region Object overrides

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}/{1}, ({2:F3})", Numerator, Denominator, (float)Numerator / Denominator);
		}

		public override bool Equals(object obj)
		{
			return obj is RationalNumber other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (int)((Numerator * 397) ^ Denominator);
			}
		}

		#endregion
	}
}