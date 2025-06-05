using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.Core.Coroutine;
using Midas.Core.General;
using Midas.Core.StateMachine;
using Midas.Gle.LogicToPresentation;
using Midas.Presentation.Data;
using Midas.Presentation.Game;
using UnityEngine;
using Coroutine = Midas.Core.Coroutine.Coroutine;

namespace Midas.Gle.Presentation
{
	public sealed class GleAutoPlayerDecisionNode : IPresentationNode
	{
		private readonly AutoUnregisterHelper autoUnregisterHelper = new AutoUnregisterHelper();
		private GleStatus gleStatus;
		private Coroutine coroutine;
		private bool showRequested;
		private bool isShowing;

		public GleAutoPlayerDecisionNode()
		{
			NodeId = "GleAutoPlayerDecision";
		}

		public string NodeId { get; }

		public bool ReadyToStart { get; private set; }

		public bool IsMainActionComplete => !(showRequested || isShowing);

		public void Init()
		{
			gleStatus = StatusDatabase.QueryStatusBlock<GleStatus>();
			autoUnregisterHelper.RegisterPropertyChangedHandler(gleStatus, nameof(gleStatus.GameResults), OnCurrentResultChanged);
			coroutine = StateMachineService.FrameUpdateRoot.StartCoroutine(DoPlayerDecision(), "PlayerDecision");
		}

		public void DeInit()
		{
			coroutine?.Stop();
			coroutine = null;
			autoUnregisterHelper.UnRegisterAll();
			gleStatus = null;
		}

		public void Destroy()
		{
		}

		public void Show()
		{
			showRequested = true;
		}

		private void OnCurrentResultChanged(StatusBlock sender, string propertyname)
		{
			var pendingDecisions = gleStatus.CurrentGameResult?.NextStageDecisions;
			ReadyToStart = StatusDatabase.GameStatus.GameMode == FoundationGameMode.Play && pendingDecisions != null && pendingDecisions.Count != 0;
		}

		private IEnumerator<CoroutineInstruction> DoPlayerDecision()
		{
			while (true)
			{
				while (!ReadyToStart)
					yield return null;

				while (!showRequested && ReadyToStart)
					yield return null;

				if (!ReadyToStart)
					continue;

				isShowing = true;
				showRequested = false;
				ReadyToStart = false;

				var pendingDecisions = gleStatus.CurrentGameResult?.NextStageDecisions;
				if (pendingDecisions != null)
					GenerateRandomUserDecisions(pendingDecisions, gleStatus.CurrentGameResult.Current.Cycles.IsFinished ? GleDecisionPersistence.Permanent : GleDecisionPersistence.Cycle);

				isShowing = false;
			}
		}

		private void GenerateRandomUserDecisions(IReadOnlyList<GleDecisionInfo> pendingDecisions, GleDecisionPersistence persistence)
		{
			foreach (var req in pendingDecisions)
			{
				// Check if the decision has already been made.

				if (gleStatus.PlayerDecisions.Any(d => d.Name == req.Name))
					continue;

				var count = Random.Range(req.MinSelections, req.MaxSelections + 1);
				var dup = count == 1 || req.AllowDuplicates;
				var s = new int[count];

				for (var i = 0; i < count; i++)
				{
					if (dup)
						s[i] = Random.Range(0, req.Options.Count);
					else
					{
						var v = Random.Range(0, req.Options.Count - i);
						foreach (var c in s.OrderBy(c => c))
						{
							if (c <= v)
								v++;
						}

						s[i] = v;
					}
				}

				Log.Instance.DebugFormat("Made user selection \"{0}\": {1}", req.Name, string.Join(", ", s.Select(v => $"{v}: {req.Options[v]}")));
				gleStatus.AddOrReplacePlayerDecision(req.Name, s, persistence);
			}
		}
	}
}