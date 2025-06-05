using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gaff.Core;
using Gaff.Core.CycleResultEditor;
using Logic.Core.DecisionGenerator;
using Logic.Core.DecisionGenerator.Decisions;
using Logic.Core.Engine;
using Logic.Core.Utility;
using Midas.Core;
using Midas.Gle.Logic;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Game;
using UnityEngine;

namespace Midas.Utility.ResultPicker
{
	public sealed class UtilityEditorDisplay : MonoBehaviour
	{
		#region Private Fields

		private GleDialUpData gleDialUpData;
		private bool refreshing;
		private IReadOnlyList<Decision> previousDecisions;
		private IRunner runner;
		private Inputs inputs;
		private CycleResult previousResults;
		private IReadOnlyList<Decision> usedDecisions;
		private IReadOnlyList<UtilitySymbolWindow> symbolWindows;
		private Action<(IDialUpResults Result, IReadOnlyList<Decision> Decisions)> onFinished;
		private readonly List<Decision> finalDecisions = new List<Decision>();

		#endregion

		#region Public Fields

		[SerializeField]
		private GameObject symbolWindowPrefab;

		[SerializeField]
		private Camera dualCamera;

		[SerializeField]
		private Camera curveCamera;

		#endregion

		#region Unity Functions

		public void Init(bool isDual, IReadOnlyList<Decision> decisions, IRunner gameRunner, Inputs gameInputs, CycleResult gameResults, IReadOnlyList<UtilitySymbolWindows> utilitySymbolWindows, Action<(IDialUpResults Result, IReadOnlyList<Decision> Decisions)> whenFinished)
		{
			runner = gameRunner;
			inputs = gameInputs;
			previousResults = gameResults;
			onFinished = whenFinished;
			previousDecisions = decisions;

			symbolWindows = utilitySymbolWindows.SelectMany(sw => sw.GetSymbolWindows()).ToArray();

			dualCamera.gameObject.SetActive(isDual);
			curveCamera.gameObject.SetActive(!isDual);
			GetComponent<Canvas>().worldCamera = isDual ? dualCamera : curveCamera;
			StartCoroutine(RunUtilityMode());
		}

		#endregion

		#region Private Functions

		private IEnumerator RunUtilityMode()
		{
			var engineIsRunning = true;
			Communication.PresentationDispatcher.AddHandler<GameStopGameEngineMessage>(OnGameStopGameEngine);

			GetPreviousDecisions(out var heldDecisions);
			GenerateResult(runner, inputs, previousResults, heldDecisions, out usedDecisions);
			var contexts = usedDecisions.Select(c => c.DecisionDefinition.Context).ToList();
			var contextStructure = usedDecisions.Select(c => c.DecisionDefinition.Context).ToList().GenerateContextStructure(c => contexts.IndexOf(c));
			contextStructure = contextStructure.Select(c => c.Condense()).ToList();

			foreach (var symbolWindow in symbolWindows)
			{
				if (symbolWindow.StageName != GameBase.GameInstance.GetLogicStage(true).Name)
					continue;

				var context = contextStructure.SingleOrDefault(c => c.Title == symbolWindow.DecisionName);
				if (context == null)
				{
					Log.Instance.Warn($"Utility Symbol Window decision not found. Stage: \"{symbolWindow.StageName}\", Dec: \"{symbolWindow.DecisionName}\", Res: \"{symbolWindow.ResultName}\"");
					continue;
				}

				var o = Instantiate(symbolWindowPrefab, gameObject.transform);
				var swp = o.GetComponent<SymbolWindowPicker>();
				swp.Initialize(runner, inputs, previousResults, usedDecisions, symbolWindow.DecisionName, symbolWindow.ResultName);
				while (!swp.IsFinished && engineIsRunning)
					yield return null;

				usedDecisions = swp.FinalDecisions;
			}

			finalDecisions.AddRange(usedDecisions);

			onFinished((new GleDialUpResults(new[] { GetRngs() }), GetResultEditorDecisions()));

			Communication.PresentationDispatcher.RemoveHandler<GameStopGameEngineMessage>(OnGameStopGameEngine);

			void OnGameStopGameEngine(GameStopGameEngineMessage obj) => engineIsRunning = false;
		}

		// Ensure the decisions are ordered by the call order in logic.
		private IReadOnlyList<ulong> GetRngs() => finalDecisions.OrderBy(dv => usedDecisions.IndexOf(dv)).SelectMany(DecisionHelper.ConvertToRng).ToList();

		private IReadOnlyList<Decision> GetResultEditorDecisions() => finalDecisions.OrderBy(dv => usedDecisions.IndexOf(dv)).Select(fd => fd).ToList();

		private void GetPreviousDecisions(out IReadOnlyList<Decision> heldDecisions)
		{
			heldDecisions = new List<Decision>();
			heldDecisions = previousDecisions.ToList();
			previousDecisions = new List<Decision>();
		}

		internal static CycleResult GenerateResult(IRunner runner, Inputs inputs, CycleResult previousResults, IReadOnlyList<Decision> decisions, out IReadOnlyList<Decision> generatedUsedDecisions)
		{
			var cycleResultEditorDecisionGenerator = new OverridableDecisionGenerator(new DecisionGenerator(new QuickRng()), decisions);
			var cycleResult = runner.EvaluateCycle(cycleResultEditorDecisionGenerator, inputs, previousResults);
			generatedUsedDecisions = cycleResultEditorDecisionGenerator.OrderedDecisions;
			return cycleResult;
		}

		#endregion

		private sealed class QuickRng : RandomNumberGenerator
		{
			private readonly System.Random random = new System.Random();

			public override ulong NextULong()
			{
				var bytes = new byte[8];
				random.NextBytes(bytes);
				return BitConverter.ToUInt64(bytes, 0);
			}
		}
	}
}