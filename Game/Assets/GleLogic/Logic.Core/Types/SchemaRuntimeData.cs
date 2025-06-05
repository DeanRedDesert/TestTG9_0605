using System.Collections.Generic;
using System.Linq;
using Logic.Core.Utility;
using Logic.Core.WinCheck;

namespace Logic.Core.Types
{
	public sealed class SchemaRuntimeData : IToCode
	{
		public IStrip ReplacementStrip { get; }

		// ReSharper disable once UnusedAutoPropertyAccessor.Global
		public IReadOnlyList<ulong> Weights { get; }
		public IReadOnlyList<IReadOnlyList<int>> SchemafySymbolIndexes { get; }
		public IReadOnlyList<int> SymbolIndexes { get; }

		// ReSharper disable once UnusedAutoPropertyAccessor.Global
		public IReadOnlyList<string> DecisionSymbolNames { get; }
		public IReadOnlyList<string> GeneratedWithSymbolList { get; }
		public SchemafySymbolWindowData SourceData { get; }

		// ReSharper disable once MemberCanBePrivate.Global
		public SchemaRuntimeData(IStrip replacementStrip, IReadOnlyList<ulong> weights, IReadOnlyList<IReadOnlyList<int>> schemafySymbolIndexes, IReadOnlyList<int> symbolIndexes, IReadOnlyList<string> decisionSymbolNames, IReadOnlyList<string> generatedWithSymbolList,
			SchemafySymbolWindowData sourceData)
		{
			ReplacementStrip = replacementStrip;
			Weights = weights;
			SchemafySymbolIndexes = schemafySymbolIndexes;
			SymbolIndexes = symbolIndexes;
			DecisionSymbolNames = decisionSymbolNames;
			GeneratedWithSymbolList = generatedWithSymbolList;
			SourceData = sourceData;
		}

		public static SchemaRuntimeData Create(SymbolList symbolList, SchemafySymbolWindowData sourceData)
		{
			var symbolIndexes = new int[sourceData.SymbolsToReplace.Count];

			for (var i = 0; i < sourceData.SymbolsToReplace.Count; i++)
				symbolIndexes[i] = symbolList.IndexOf(sourceData.SymbolsToReplace[i]);

			var weights = new ulong[sourceData.SchemaEntries.Count];
			var schemafySymbolIndexes = new int[sourceData.SchemaEntries.Count][];
			var decisionSymbolNames = new string[sourceData.SchemaEntries.Count];

			for (var i = 0; i < sourceData.SchemaEntries.Count; i++)
			{
				schemafySymbolIndexes[i] = new int[sourceData.SymbolsToReplace.Count];
				weights[i] = sourceData.SchemaEntries[i].Weight;
				decisionSymbolNames[i] = string.Join(" ", sourceData.SchemaEntries[i].ReplacementSymbols.ToArray());
				for (var j = 0; j < sourceData.SymbolsToReplace.Count; j++)
					schemafySymbolIndexes[i][j] = symbolList.IndexOf(sourceData.SchemaEntries[i].ReplacementSymbols[j]);
			}

			var replacementStrip = StripHelper.CreateStrip((ulong)sourceData.SchemaEntries.Count, i => decisionSymbolNames[i], i => weights[i]);

			return new SchemaRuntimeData(replacementStrip, weights, schemafySymbolIndexes, symbolIndexes, decisionSymbolNames, symbolList, sourceData);
		}

		/// <inheritdoc cref="IToCode.ToCode(CodeGenArgs?)" />
		public IResult ToCode(CodeGenArgs args)
		{
			return CodeConverter.TryToCode(args, new SymbolList(GeneratedWithSymbolList), out var a) &&
				CodeConverter.TryToCode(args, SourceData, out var b)
					? $"{CodeConverter.ToCode<SchemaRuntimeData>(args)}.{nameof(Create)}({a}, {b})".ToSuccess()
					: new Error("unable to convert SchemaRuntimeData");
		}
	}
}