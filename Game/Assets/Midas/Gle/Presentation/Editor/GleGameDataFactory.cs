using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.Editor.GameData;
using GleGameData = Midas.Gle.LogicToPresentation.GleGameData;

namespace Midas.Gle.Presentation.Editor
{
	// ReSharper disable once UnusedType.Global - This is discovered and instantiated by reflection in the Unity editor.
	public class GleGameDataFactory : GameDataFactory
	{
		private static IReadOnlyList<string> stageNames;
		private static IReadOnlyDictionary<string, IReadOnlyList<ReelInformation>> reelInfo;
		private static IReadOnlyDictionary<string, IReadOnlyList<string>> symbols;
		private static IReadOnlyDictionary<string, IReadOnlyList<LinePatternInformation>> linePatternInfo;

		static GleGameDataFactory()
		{
			stageNames = GleGameData.Stages.Select(s => s.Name).ToArray();
			var reels = new Dictionary<string, IReadOnlyList<ReelInformation>>();
			var syms = new Dictionary<string, IReadOnlyList<string>>();
			var lines = new Dictionary<string, IReadOnlyList<LinePatternInformation>>();

			foreach (var stage in GleGameData.Stages)
			{
				reels.Add(stage.Name, stage.GetReelInfoEntries());
				syms.Add(stage.Name, stage.GetAllSymbolStrips().SelectMany(s => s.GetSymbolList()).Distinct().ToArray());
				lines.Add(stage.Name, stage.GetLinePatternInfo());
			}

			reelInfo = reels;
			symbols = syms;
			linePatternInfo = lines;
		}

		protected override IReadOnlyList<string> GetStageNames() => stageNames;

		protected override IReadOnlyList<ReelInformation> GetReelInformation(string stageName)
		{
			return reelInfo[stageName];
		}

		protected override IReadOnlyList<string> GetSymbols(string stageName)
		{
			return symbols[stageName];
		}

		protected override IReadOnlyList<LinePatternInformation> GetLinePatterns(string stageName)
		{
			return linePatternInfo[stageName];
		}
	}
}