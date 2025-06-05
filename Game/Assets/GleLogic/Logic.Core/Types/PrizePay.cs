using Logic.Core.Utility;

namespace Logic.Core.Types
{
	/// <summary>
	/// The value awarded by a prize, based on the count of clusters which contain required/desired symbols.
	/// The multiplier usage describes how to apply multipliers to the values.
	/// </summary>
	public sealed class PrizePay : IToString, IFromString
	{
		/// <summary>
		/// The exact number of clusters required for the value to be awarded.
		/// </summary>
		public int Count { get; }

		/// <summary>
		/// The value of the prize. Usually this will be credits but could also be free game count or any other number.
		/// How it is interpreted is determined by the processors that are looking for the prize.
		/// </summary>
		public int Value { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="count">The clusters required for the value to be awarded.</param>
		/// <param name="value">The value of the prize.</param>
		public PrizePay(int count, int value)
		{
			Count = count;
			Value = value;
		}

		#region IToString Members

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format) => $"{Count}:{Value}".ToSuccess();

		#endregion

		#region IFromString Members

		/// <summary>Implementation of IFromString.FromString(string?, string?)</summary>
		// ReSharper disable once UnusedMember.Global
		public static IResult FromString(string s, string format)
		{
			var sp = s.Split(':');

			// .Net 4.8 doesn't support int.TryParse(string, CultureInfo, out int)
			if (sp.Length == 2 &&
				int.TryParse(sp[0], out var c) &&
				int.TryParse(sp[1], out var v))
				return new PrizePay(c, v).ToSuccess();

			return new Error($"Could not convert '{s}' to a PrizePay value");
		}

		#endregion
	}
}