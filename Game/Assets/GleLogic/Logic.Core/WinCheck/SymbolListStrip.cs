using System;
using System.Collections.Generic;
using Logic.Core.Types;
using Logic.Core.Utility;

namespace Logic.Core.WinCheck
{
	public sealed class SymbolListStrip : ISymbolListStrip, IToString, IToCode
	{
		#region Fields

		private readonly SymbolList symbolList;
		private readonly IReadOnlyList<int> symbolIndexes;
		private readonly IReadOnlyList<IReadOnlyList<int>> symbolPositions;

		#endregion

		#region Construction

		public SymbolListStrip(SymbolList symbolList, IReadOnlyList<int> symbolIndexes)
		{
			this.symbolList = symbolList;
			this.symbolIndexes = symbolIndexes;
			symbolPositions = StripHelper.GetSymbolStripPositions(symbolList, symbolIndexes);
		}

		// ReSharper disable once UnusedMember.Global
		public static SymbolListStrip Create(string symbols, string symbolIndexes)
		{
			var sl = symbols.FromStringOrThrow<IReadOnlyList<string>>("SL");
			var si = symbolIndexes.FromStringOrThrow<IReadOnlyList<int>>("SL");
			return new SymbolListStrip(new SymbolList(sl), si);
		}

		#endregion

		#region ISymbolListStrip Implementation

		public ulong GetTotalWeight() => GetLength();

		public ulong GetIndexAtWeight(ulong weight) => weight;

		public ulong GetIndexAtWeight(ulong weight, ICollection<ulong> skipIndexes)
		{
			if (weight >= GetTotalWeight())
				throw new ArgumentException("Weight value exceeds total weight!");

			return StripHelper.GetIndexAtWeight(weight, (ulong)symbolIndexes.Count, skipIndexes, GetWeight);
		}

		public ulong GetLength() => (ulong)symbolIndexes.Count;

		public string GetSymbol(ulong index) => symbolList[symbolIndexes[(int)index]];

		public ulong GetWeight(ulong index) => 1;

		public int GetSymbolIndex(ulong index) => symbolIndexes[(int)index];

		public SymbolList GetSymbolList() => symbolList;

		public IReadOnlyList<int> GetSymbolPositions(int symbolIndex) => symbolPositions[symbolIndex];

		#endregion

		#region Implementation of IToString

		public IResult ToString(string format)
		{
			return this.StripToString(format);
		}

		#endregion

		#region Implementation of IToCode

		/// <inheritdoc cref="IToCode.ToCode(CodeGenArgs?)" />
		public IResult ToCode(CodeGenArgs args)
		{
			if (!StringConverter.TryToString(symbolList, "SL", out var s1))
				return new Error(s1);

			// ReSharper disable once ConvertIfStatementToReturnStatement
			if (!StringConverter.TryToString(symbolIndexes, "SL", out var s2))
				return new Error(s2);

			return $"{CodeConverter.ToCode<SymbolListStrip>(args)}.{nameof(Create)}(\"{s1}\", \"{s2}\")".ToSuccess();
		}

		#endregion
	}
}