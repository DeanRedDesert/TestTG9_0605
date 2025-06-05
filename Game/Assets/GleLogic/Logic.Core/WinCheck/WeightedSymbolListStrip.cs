using System;
using System.Collections.Generic;
using Logic.Core.Types;
using Logic.Core.Utility;

namespace Logic.Core.WinCheck
{
	/// <summary>
	/// Used to support collections of Stop objects.  Can be constructed in zero weight support or not.
	/// If true then the optimization in GetIndexAtWeight is disabled.  If false an exception is thrown on construction
	/// when a zero weighted stop is encountered.
	/// </summary>
	public sealed class WeightedSymbolListStrip : ISymbolListStrip, IToString, IToCode
	{
		#region Fields

		private readonly SymbolList symbolList;
		private readonly IReadOnlyList<int> symbolIndexes;
		private readonly IReadOnlyList<ulong> weights;
		private readonly bool supportZeroWeight;
		private readonly ulong totalWeight;
		private readonly IReadOnlyList<IReadOnlyList<int>> symbolPositions;

		#endregion

		#region Construction

		public WeightedSymbolListStrip(SymbolList symbolList, IReadOnlyList<int> symbolIndexes, IReadOnlyList<ulong> weights, bool supportZeroWeight = false)
		{
			this.symbolList = symbolList;
			this.symbolIndexes = symbolIndexes;
			this.weights = weights;
			this.supportZeroWeight = supportZeroWeight;

			for (var i = 0; i < symbolIndexes.Count; i++)
			{
				if (!supportZeroWeight && weights[i] == 0)
					throw new ArgumentException("Individual weights cannot be zero.");

				totalWeight += weights[i];
			}

			symbolPositions = StripHelper.GetSymbolStripPositions(symbolList, symbolIndexes);

			if (totalWeight == 0UL)
				throw new ArgumentException("TotalWeight cannot be zero.");
		}

		// ReSharper disable once UnusedMember.Global
		public static WeightedSymbolListStrip Create(string symbols, string symbolIndexes, string weights, bool supportsZeroWeight)
		{
			var sl = symbols.FromStringOrThrow<IReadOnlyList<string>>("SL");
			var si = symbolIndexes.FromStringOrThrow<IReadOnlyList<int>>("SL");
			var wl = weights.FromStringOrThrow<IReadOnlyList<ulong>>("SL");
			return new WeightedSymbolListStrip(new SymbolList(sl), si, wl, supportsZeroWeight);
		}

		#endregion

		#region Implementation of ISymbolListStrip

		/// <inheritdoc />
		public ulong GetTotalWeight() => totalWeight;

		/// <inheritdoc />
		public ulong GetIndexAtWeight(ulong weight)
		{
			if (weight >= totalWeight)
				throw new ArgumentException("Weight value exceeds total weight!");

			if (!supportZeroWeight && totalWeight == (ulong)symbolIndexes.Count)
				return weight;

			return StripHelper.GetIndexAtWeight(weight, (ulong)symbolIndexes.Count, GetWeight);
		}

		/// <inheritdoc />
		public ulong GetIndexAtWeight(ulong weight, ICollection<ulong> preChosenIndexes)
		{
			if (weight >= totalWeight)
				throw new ArgumentException("Weight value exceeds total weight!");

			return StripHelper.GetIndexAtWeight(weight, (ulong)symbolIndexes.Count, preChosenIndexes, GetWeight);
		}

		/// <inheritdoc />
		public ulong GetLength() => (ulong)symbolIndexes.Count;

		/// <inheritdoc />
		public string GetSymbol(ulong index) => symbolList[symbolIndexes[(int)index]];

		/// <inheritdoc />
		public ulong GetWeight(ulong index) => weights[(int)index];

		public int GetSymbolIndex(ulong index) => symbolIndexes[(int)index];

		public SymbolList GetSymbolList() => symbolList;

		public IReadOnlyList<int> GetSymbolPositions(int symbolIndex) => symbolPositions[symbolIndex];

		#endregion

		#region Implementation of IToString

		/// <inheritdoc cref="IToString.ToString(string?)" />
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

			if (!StringConverter.TryToString(symbolIndexes, "SL", out var s2))
				return new Error(s2);

			if (!StringConverter.TryToString(weights, "SL", out var s3))
				return new Error(s2);

			var s4 = CodeConverter.ToCodeOrThrow(args, supportZeroWeight);
			return $"{CodeConverter.ToCode<WeightedSymbolListStrip>(args)}.{nameof(Create)}(\"{s1}\", \"{s2}\", \"{s3}\", {s4})".ToSuccess();
		}

		#endregion
	}
}