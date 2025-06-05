using System;
using System.Collections.Generic;
using Logic.Core.Utility;

namespace Logic.Core.WinCheck
{
	/// <summary>
	/// A strip object based on a strip length along with both getSymbol and getWeight functions.
	/// Supports zero weights for safety. If performance is required using the SimpleStrip without weights is preferred.
	/// </summary>
	public sealed class SimpleWeightedStrip : IStrip, IToString
	{
		#region Fields

		private readonly ulong stripLength;
		private readonly Func<ulong, string> getSymbol;
		private readonly Func<ulong, ulong> getWeight;
		private readonly ulong totalWeight;

		#endregion

		#region Construction

		public SimpleWeightedStrip(ulong stripLength, Func<ulong, string> getSymbol, Func<ulong, ulong> getWeight)
		{
			this.stripLength = stripLength;
			this.getSymbol = getSymbol;
			this.getWeight = getWeight;
			totalWeight = 0UL;

			for (var i = 0UL; i < stripLength; i++)
				totalWeight += getWeight(i);
		}

		#endregion

		#region Implementation of IWeights

		/// <inheritdoc />
		public ulong GetTotalWeight() => totalWeight;

		/// <inheritdoc />
		public ulong GetIndexAtWeight(ulong weight)
		{
			if (weight >= totalWeight)
				throw new ArgumentException("Weight value exceeds total weight!");

			return StripHelper.GetIndexAtWeight(weight, stripLength, getWeight);
		}

		/// <inheritdoc />
		public ulong GetIndexAtWeight(ulong weight, ICollection<ulong> indexesToSkip)
		{
			if (weight >= totalWeight)
				throw new ArgumentException("Weight value exceeds total weight!");

			return StripHelper.GetIndexAtWeight(weight, stripLength, indexesToSkip, getWeight);
		}

		/// <inheritdoc />
		public ulong GetLength() => stripLength;

		/// <inheritdoc />
		public ulong GetWeight(ulong index) => getWeight(index);

		#endregion

		#region Implementation of IStrip

		/// <inheritdoc />
		public string GetSymbol(ulong index) => getSymbol(index);

		#endregion

		#region Implementation of IToString

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format)
		{
			return this.StripToString(format);
		}

		#endregion
	}
}