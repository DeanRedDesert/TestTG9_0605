using System;
using System.Collections.Generic;
using Logic.Core.Utility;

namespace Logic.Core.WinCheck
{
	/// <summary>
	/// A strip object based on a strip length along with a getSymbol function.
	/// The weight is hard coded to 1 for each index.
	/// </summary>
	public sealed class SimpleStrip : IStrip, IToString
	{
		#region Fields

		private readonly ulong stripLength;
		private readonly Func<ulong, string> getSymbol;

		#endregion

		#region Construction

		public SimpleStrip(ulong stripLength, Func<ulong, string> getSymbol)
		{
			this.stripLength = stripLength;
			this.getSymbol = getSymbol;
		}

		#endregion

		#region Implementation of IStrip

		/// <inheritdoc />
		public ulong GetTotalWeight() => stripLength;

		/// <inheritdoc />
		public ulong GetIndexAtWeight(ulong weight) => weight;

		/// <inheritdoc />
		public ulong GetIndexAtWeight(ulong weight, ICollection<ulong> indexesToSkip)
		{
			if (weight >= stripLength)
				throw new ArgumentException("Weight value exceeds total weight!");

			return StripHelper.GetIndexAtWeight(weight, stripLength, indexesToSkip, GetWeight);
		}

		/// <inheritdoc />
		public ulong GetLength() => stripLength;

		/// <inheritdoc />
		public ulong GetWeight(ulong index) => 1;

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