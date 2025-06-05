using System;

namespace Logic.Core.DecisionGenerator
{
	/// <summary>
	/// Defines the interface for generating a random number.
	/// </summary>
	public abstract class RandomNumberGenerator
	{
		/// <summary>
		/// Generates a random number greater than or equal to 0 and less than ulong.MaxValue.
		/// </summary>
		/// <returns>A random number greater than or equal to 0 and less than ulong.MaxValue.</returns>
		public abstract ulong NextULong();

		/// <summary>
		/// Generates an ulong random number greater than or equal to 0 and less than <paramref name="maxValue"/>.
		/// </summary>
		/// <param name="maxValue">The upper limit for the random number.</param>
		/// <returns>A random number greater than or equal to 0 and less than <paramref name="maxValue"/>.</returns>
		public ulong Next(ulong maxValue) => GetRandomNumber(maxValue);

		/// <summary>
		/// Generates an ulong random number greater than or equal to 0 and less than <paramref name="maxValue"/>.
		/// </summary>
		/// <param name="maxValue">The upper limit for the random number.</param>
		/// <returns>A random number greater than or equal to 0 and less than <paramref name="maxValue"/>.</returns>
		/// <exception cref="ArgumentException">Thrown if the max value is zero.</exception>
		// ReSharper disable once VirtualMemberNeverOverridden.Global
		private ulong GetRandomNumber(ulong maxValue)
		{
			if (maxValue == 0)
				throw new ArgumentException("0 is not a valid max value for random number generation.");

			var limit = ulong.MaxValue - ulong.MaxValue % maxValue;
			ulong value;
			do
			{
				value = NextULong();
			} while (value > limit);

			return value % maxValue;
		}
	}
}