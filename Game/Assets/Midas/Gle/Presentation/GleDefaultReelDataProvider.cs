using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.Types;
using Midas.Gle.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Data;
using Midas.Presentation.Game;
using Midas.Presentation.StageHandling;
using UnityEngine;

namespace Midas.Gle.Presentation
{
	[CreateAssetMenu(menuName = "Midas/Reels/GLE Default Reel Data Provider")]
	public sealed class GleDefaultReelDataProvider : GleReelDataProvider
	{
		[Serializable]
		public class InheritedResult
		{
			[SerializeField]
			private Stage stage;

			[SerializeField]
			private string resultName;

			public Stage Stage => stage;
			public string ResultName => resultName;

			public InheritedResult(Stage stage, string resultName)
			{
				this.stage = stage;
				this.resultName = resultName;
			}
		}

		private IReadOnlyList<string> stageIds;

		[SerializeField]
		private string resultName;

		[SerializeField]
		private InheritedResult[] inheritedResults;

		protected override SymbolWindowResult GetInitReelResult(Stage stage)
		{
			var gleStatus = StatusDatabase.QueryStatusBlock<GleStatus>();
			var stageId = GameBase.GameInstance.GetStageIdFromLogicStage(stage);
			var results = gleStatus.GetResultForGameState(stageId);
			var symbolWindowResult = results.GetReelDetails(resultName).SymbolWindowResult;

			if (inheritedResults == null || inheritedResults.Length == 0 || StatusDatabase.GameStatus.CurrentGameState == GameState.History)
				return symbolWindowResult;

			var targetSymbolWindowStructure = symbolWindowResult.SymbolWindowStructure;
			var resultNameToUse = default(string);
			results = gleStatus.GetResultForGameState(CheckResultPredicate);

			if (results == default)
			{
				Log.Instance.Fatal($"No reel result found for {stageId} in state {StatusDatabase.GameStatus.CurrentGameState}.");
				return default;
			}

			var inheritedSymbolWindowResult = results.GetReelDetails(resultNameToUse).SymbolWindowResult;

			// Check that the inherited symbol window is the same shape

			var targetCells = targetSymbolWindowStructure.Cells;
			var sourceCells = inheritedSymbolWindowResult.SymbolWindowStructure.Cells;
			if (targetCells.Count == sourceCells.Count && targetCells.All(sourceCells.Contains))
				return inheritedSymbolWindowResult;

			return symbolWindowResult;

			bool CheckResultPredicate(GleResult result)
			{
				if (result.CurrentCycle.Stage == stageId)
				{
					resultNameToUse = resultName;
					return true;
				}

				stageIds ??= inheritedResults.Select(ir => GameBase.GameInstance.GetStageIdFromLogicStage(ir.Stage)).ToArray();

				for (var i = 0; i < stageIds.Count; i++)
				{
					if (result.CurrentCycle.Stage == stageIds[i])
					{
						resultNameToUse = inheritedResults[i].ResultName;
						return true;
					}
				}

				return false;
			}
		}

		protected override (SymbolWindowResult SymbolWindowResult, ReadOnlyMask LockMask) GetReelResult(Stage stage)
		{
			var gleStatus = StatusDatabase.QueryStatusBlock<GleStatus>();
			var stageId = GameBase.GameInstance.GetStageIdFromLogicStage(stage);
			var results = gleStatus.CurrentGameResult.Current;

			if (results.Inputs.GetCycles().Current.Stage != stageId)
				Log.Instance.Fatal($"Attempting to spin reels for {stageId} but result is for {results.Cycles.Current.Stage}");

			return results.StageResults.GetReelDetails(resultName);
		}

		#region Editor

#if UNITY_EDITOR

		public static GleDefaultReelDataProvider CreateForMakeGame(string assetPath, string resultName, IReadOnlyList<(Stage Stage, string ResultName)> inheritedResults)
		{
			var resultProvider = CreateInstance<GleDefaultReelDataProvider>();
			UnityEditor.AssetDatabase.CreateAsset(resultProvider, assetPath);

			resultProvider.resultName = resultName;
			resultProvider.inheritedResults = inheritedResults.Select(r => new InheritedResult(r.Stage, r.ResultName)).ToArray();

			UnityEditor.EditorUtility.SetDirty(resultProvider);
			UnityEditor.AssetDatabase.SaveAssetIfDirty(resultProvider);

			return resultProvider;
		}

#endif

		#endregion
	}
}