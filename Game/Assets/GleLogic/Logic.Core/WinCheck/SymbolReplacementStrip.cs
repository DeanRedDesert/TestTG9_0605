using System;
using System.Collections.Generic;
using Logic.Core.Types;
using Logic.Core.Utility;

namespace Logic.Core.WinCheck
{
	public sealed class SymbolReplacementStrip : ISymbolListStrip, IToString
	{
		#region Fields

		private readonly ISymbolListStrip originalStrip;
		private readonly Dictionary<int, IReadOnlyList<int>> cachedSymbolPositions = new Dictionary<int, IReadOnlyList<int>>();
		private readonly Func<ulong, ISymbolListStrip, int> getSymbolIndex;
		private readonly Func<int, IReadOnlyList<int>> getSymbolPositions;

		#endregion

		#region Construction

		public SymbolReplacementStrip(ISymbolListStrip originalStrip, Func<ulong, ISymbolListStrip, int> getSymbolIndex, Func<int, IReadOnlyList<int>> getSymbolPositions)
		{
			this.originalStrip = originalStrip;
			this.getSymbolIndex = getSymbolIndex;
			this.getSymbolPositions = getSymbolPositions;
		}

		#endregion

		#region ISymbolListStrip Implementation

		public ulong GetTotalWeight() => GetLength();

		public ulong GetIndexAtWeight(ulong weight) => weight;

		public ulong GetIndexAtWeight(ulong weight, ICollection<ulong> skipIndexes)
		{
			return originalStrip.GetIndexAtWeight(weight, skipIndexes);
		}

		public ulong GetLength() => originalStrip.GetLength();

		public string GetSymbol(ulong index)
		{
			return originalStrip.GetSymbolList()[getSymbolIndex(index, originalStrip)];
		}

		public ulong GetWeight(ulong index) => originalStrip.GetWeight(index);

		public int GetSymbolIndex(ulong index)
		{
			return getSymbolIndex(index, originalStrip);
		}

		public SymbolList GetSymbolList() => originalStrip.GetSymbolList();

		public IReadOnlyList<int> GetSymbolPositions(int symbolIndex)
		{
			if (!cachedSymbolPositions.ContainsKey(symbolIndex))
				cachedSymbolPositions[symbolIndex] = getSymbolPositions(symbolIndex);
			return cachedSymbolPositions[symbolIndex];
		}

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