using System;
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
using Midas.Gle.LogicToPresentation;
using Midas.Gle.Presentation;
using Midas.Presentation.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Midas.Gaff.ResultEditor
{
	public sealed class ResultEditorDisplay : MonoBehaviour
	{
		#region Private Fields

		private GleDialUpData gleDialUpData;
		private bool refreshing;
		private readonly List<DecisionView> decisionGameObjects = new List<DecisionView>();
		private readonly Dictionary<string, object> uiState = new Dictionary<string, object>();
		private GameObject menuPanel;
		private readonly List<IDecision> decisionViews = new List<IDecision>();
		private IReadOnlyList<Decision> previousDecisions;
		private IRunner runner;
		private Inputs inputs;
		private CycleResult previousResults;
		private IReadOnlyList<Decision> usedDecisions;

		private Action<(IDialUpResults Result, bool Continue, IReadOnlyList<Decision> Decisions)> onFinished;

		#endregion

		#region Public Fields

		[SerializeField]
		private DecisionSettings decisionSettings;

		[SerializeField]
		private GameObject decisionPrefab;

		[SerializeField]
		private Camera dualCamera;

		[SerializeField]
		private Camera curveCamera;

		[SerializeField]
		private Text awardText;

		[SerializeField]
		private Text pendingExits;

		[SerializeField]
		private Text takenExit;

		#endregion

		#region Unity Functions

		public void Init(bool isStartingGame, bool isDual, IReadOnlyList<Decision> decisions, IRunner gameRunner, Inputs gameInputs, CycleResult gameResults, Action<(IDialUpResults Result, bool Continue, IReadOnlyList<Decision> Decisions)> whenFinished)
		{
			runner = gameRunner;
			inputs = gameInputs;
			previousResults = gameResults;
			onFinished = whenFinished;
			previousDecisions = decisions;
			menuPanel = GameObject.Find("Menu");

			// Handling for win cap break out of previous game on the start of the next game.
			if (isStartingGame && !previousResults.Cycles.IsFinished)
			{
				var finishedCycles = Cycles.CreateInitial(GleGameData.EntryStage.Name).ReplaceCurrent(1, 1).MoveNext();
				previousResults = new CycleResult(gameResults.Inputs, finishedCycles, gameResults.AwardedPrize, gameResults.TotalAwardedPrize, gameResults.StageResults, gameResults.Progressives);
			}

			Refresh(false);

			dualCamera.gameObject.SetActive(isDual);
			curveCamera.gameObject.SetActive(!isDual);
			GetComponent<Canvas>().worldCamera = isDual ? dualCamera : curveCamera;
		}

		private void Update()
		{
			var update = false;
			foreach (var orderedDecision in decisionViews)
			{
				if (!(orderedDecision is IEditableDecision { Changed: true } editableDecision))
					continue;

				uiState[orderedDecision.Decision.DecisionDefinition.Context] = editableDecision.UIState;
				update = true;
				editableDecision.ResetChangedState();
				break;
			}

			if (update)
				RefreshInDesignMode();
		}

		#endregion

		#region Public Functions

		public void UseResult() => onFinished((new GleDialUpResults(new[] { GetRngs() }), true, GetResultEditorDecisions()));

		public void UseResultAndContinue() => onFinished((new GleDialUpResults(new[] { GetRngs() }), false, GetResultEditorDecisions()));

		// Ensure the decisions are ordered by the call order in logic.
		private IReadOnlyList<ulong> GetRngs()
		{
			var rngs = new List<ulong>();

			foreach (var decision in usedDecisions)
			{
				var finalDecision = decisionViews.SingleOrDefault(item => item.Decision == decision)?.Decision ?? decision;
				rngs.AddRange(DecisionHelper.ConvertToRng(finalDecision));
			}

			return rngs;
		}

		private void RefreshInDesignMode() => Refresh(true);

		public void UpdateData() => Refresh(false);

		#endregion

		#region Private Functions

		private IReadOnlyList<Decision> GetResultEditorDecisions()
		{
			var results = new List<Decision>();

			// Ensure the decisions are ordered by the call order in logic.
			foreach (var decision in decisionViews.OrderBy(dv => usedDecisions.IndexOf(dv.Decision)))
				results.Add(decision.Decision);

			return results;
		}

		private void Refresh(bool designMode)
		{
			if (refreshing)
				return;

			refreshing = true;

			GetPreviousDecisions(designMode, out var heldDecisions, out var isHeld);
			GenerateResult(heldDecisions, out var cycleResult, out usedDecisions);
			UpdateTextResults(cycleResult);
			CreateViews(isHeld, usedDecisions.Where(item => !(item.DecisionDefinition is PickIndexesDecision)).ToList());

			refreshing = false;
		}

		private void GetPreviousDecisions(bool designMode, out IReadOnlyList<Decision> heldDecisions, out Dictionary<string, bool> isHeld)
		{
			heldDecisions = new List<Decision>();
			isHeld = new Dictionary<string, bool>();
			heldDecisions = previousDecisions.ToList();
			previousDecisions = new List<Decision>();

			isHeld = new Dictionary<string, bool>();
			if (heldDecisions.Count == 0)
			{
				isHeld = decisionViews.ToDictionary(od => od.Decision.DecisionDefinition.Context, od => od.Hold);
				heldDecisions = decisionViews.Where(od => od.Hold || designMode).Select(od => od.Decision).ToList();
			}
		}

		private void GenerateResult(IReadOnlyList<Decision> decisions, out CycleResult cycleResult, out IReadOnlyList<Decision> generatedUsedDecisions)
		{
			var cycleResultEditorDecisionGenerator = new OverridableDecisionGenerator(new GleGame.RuntimeDecisionGen(new QuickRng(), StatusDatabase.QueryStatusBlock<GleStatus>().PlayerDecisions.ToDictionary(item => item.Name, item => item), false), decisions);
			cycleResult = runner.EvaluateCycle(cycleResultEditorDecisionGenerator, inputs, previousResults);
			generatedUsedDecisions = cycleResultEditorDecisionGenerator.OrderedDecisions;
		}

		private void UpdateTextResults(CycleResult result)
		{
			awardText.text = result.AwardedPrize.ToStringOrThrow("SL");
			var pendingCycle = result.Cycles.IsFinished ? null : result.Cycles.Current;

			takenExit.text = pendingCycle == null ? "" : $"{pendingCycle.Stage}(\"{pendingCycle.CycleId}\")";
			pendingExits.text = string.Join("\n", result.Cycles.Where(c => !c.IsFinished).Select(FormatCycleState).ToArray());
		}

		private static string FormatCycleState(CycleState cycleState) => $"{cycleState.TriggeringCycle.Stage}(\"{cycleState.TriggeringCycle.CycleId}\")->{cycleState.Stage}(\"{cycleState.CycleId}\")({cycleState.CompletedCycles}/{cycleState.TotalCycles})";

		private void CreateViews(IReadOnlyDictionary<string, bool> isHeld, IReadOnlyList<Decision> decisions)
		{
			foreach (var go in decisionGameObjects)
				Destroy(go.gameObject);
			decisionGameObjects.Clear();
			decisionViews.Clear();

			var contexts = decisions.Select(c => c.DecisionDefinition.Context).ToList();
			var contextStructure = decisions.Select(c => c.DecisionDefinition.Context).ToList().GenerateContextStructure(c => contexts.IndexOf(c));
			contextStructure = contextStructure.Select(c => c.Condense()).ToList();

			foreach (var context in contextStructure)
			{
				var groupContexts = GroupDecisions(context, decisions, context.Title);
				var groupedDecisionViews = groupContexts.Select(groupContext => groupContext.Decisions.Select((t, i) => CreateDecisionViews(t.Decision, t.Title, isHeld, t.CallIndex)).ToList()).ToList();

				decisionViews.AddRange(groupedDecisionViews.SelectMany(gdv => gdv));

				var counter = 0;
				foreach (var groupContext in groupContexts)
				{
					var decisionGameObject = Instantiate(decisionPrefab, menuPanel.transform, false);
					var decisionView = decisionGameObject.GetComponent<DecisionView>();

					decisionView.Construct(groupedDecisionViews[counter]);
					decisionView.Initialize(groupContext.Title);
					decisionGameObjects.Add(decisionView);
					counter++;
				}
			}

			var lastDecision = decisionGameObjects.LastOrDefault();
			foreach (var decisionViewItem in decisionGameObjects)
				decisionViewItem.LastDecision = lastDecision == decisionViewItem;
		}

		private IDecision CreateDecisionViews(Decision decision, string title, IReadOnlyDictionary<string, bool> isHeld, int callNumber)
		{
			isHeld.TryGetValue(decision.DecisionDefinition.Context, out var held);

			switch (decision.DecisionDefinition)
			{
				case WeightsIndexesDecision sid:
					return CreateItemDecision(decision, title, sid.Count > 1, held, callNumber);
				case WeightedIndexesDecision wid:
					return CreateItemDecision(decision, title, wid.Count > 1, held, callNumber);
				case IndexesDecision id:
					return CreateItemDecision(decision, title, id.Count > 1, held, callNumber);
				case SimpleDecision _:
					return CreateBoolDecision(decision, title, held, callNumber);
			}

			return null;
		}

		private static IReadOnlyList<(string Title, IReadOnlyList<(string Title, Decision Decision, int CallIndex)> Decisions)> GroupDecisions(ContextStructure contextStructure, IReadOnlyList<Decision> decisions, string parentTitle)
		{
			var groups = new List<(string, IReadOnlyList<(string Title, Decision Decision, int CallIndex)>)>();

			switch (contextStructure)
			{
				case LeafItem leaf:
				{
					var gr = new List<(string Title, Decision, int CallIndex)>();
					gr.Add((leaf.Title, decisions.First(od => od.DecisionDefinition.Context == leaf.Context), leaf.CallIndex));
					groups.Add(($"{parentTitle}", gr));
					break;
				}
				case GroupItem group:
				{
					var leafChildren = group.Children.OfType<LeafItem>().ToList();
					var gr = leafChildren.Select(item => (item.Title, decisions.First(od => od.DecisionDefinition.Context == item.Context), item.CallIndex)).ToList();

					// Create a group with the name of the leaf child if there is only 1.
					if (leafChildren.Count == 1)
					{
						var item = leafChildren[0];
						groups.Add(($"{parentTitle}_{item.Title}", new[] { (item.Title, gr[0].Item2, gr[0].CallIndex) }));
					}
					else
					{
						groups.Add(($"{parentTitle}", gr));
					}

					foreach (var item in group.Children.OfType<GroupItem>())
						groups.AddRange(GroupDecisions(item, decisions, $"{parentTitle}_{item.Title}"));
					break;
				}
			}

			return groups;
		}

		private IDecision CreateBoolDecision(Decision decision, string title, bool isHeld, int callNumber)
		{
			var decisionGameObject = Instantiate(decisionSettings.BoolDecisionCallPrefab);
			var decisionView = decisionGameObject.GetComponent<BoolDecisionCallView>();

			decisionView.Construct(decision, title, isHeld, callNumber);
			decisionView.Initialize();

			return decisionView;
		}

		private IDecision CreateItemDecision(Decision decision, string title, bool requiresMultipleItems, bool isHeld, int callNumber)
		{
			if (requiresMultipleItems)
			{
				var decisionGameObject = Instantiate(decisionSettings.MultiItemsDecisionCallPrefab);
				var decisionView = decisionGameObject.GetComponent<MultiItemsDecisionCallView>();

				decisionView.Construct(decision, title, isHeld, callNumber);
				uiState.TryGetValue(decision.DecisionDefinition.Context, out var state);
				decisionView.Initialize(state);

				return decisionView;
			}

			var singleDecisionGameObject = Instantiate(decisionSettings.SingleItemDecisionCallPrefab);
			var singleDecisionView = singleDecisionGameObject.GetComponent<ItemsDecisionCallView>();

			singleDecisionView.Construct(decision, title, isHeld, callNumber);
			singleDecisionView.Initialize();

			return singleDecisionView;
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