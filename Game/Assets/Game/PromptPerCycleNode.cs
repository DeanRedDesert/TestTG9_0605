using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Midas.Core.Coroutine;
using Midas.Core.General;
using Midas.Core.StateMachine;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Data;
using Midas.Presentation.Game;
using Midas.Presentation.Sequencing;
using Midas.Presentation.StageHandling;

namespace Game
{
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public enum PerCyclePromptSequenceIds
	{
		ShowBanner = CustomEvent.CustomEventStartId,
		WaitStartFeature,
		HideBanner
	}

	public sealed class PromptPerCycleNode : IPresentationNode, ISequenceOwner
	{
		private enum State
		{
			Idle,
			WaitForStage,
			ShowSequence
		}

		private readonly Stage stage;
		private readonly Sequence sequence;
		private Coroutine coroutine;
		private bool showRequested;
		private bool isShowing;
		private readonly AutoUnregisterHelper autoUnregisterHelper = new AutoUnregisterHelper();
		private bool wantsToStart;

		public string NodeId { get; }

		public bool ReadyToStart
		{
			get
			{
				return wantsToStart &&
					StatusDatabase.WinPresentationStatus.WinPresentationComplete;
			}
		}

		public bool IsMainActionComplete => !(showRequested || isShowing);
		public IReadOnlyList<Sequence> Sequences { get; }

		private PromptPerCycleNode(string nodeId, Stage stage, Sequence sequence)
		{
			NodeId = nodeId;
			this.stage = stage;
			this.sequence = sequence;
			Sequences = new[] { sequence };
		}

		public static PromptPerCycleNode Create(string nodeId, Stage stage)
		{
			var sequence = SimpleSequence.Create<PerCyclePromptSequenceIds>($"{stage.Name}/Prompt");
			return new PromptPerCycleNode(nodeId, stage, sequence);
		}

		public static PromptPerCycleNode Create<T>(string nodeId, Stage stage) where T : Enum
		{
			var sequence = SimpleSequence.Create<T>($"{stage.Name}/Prompt");
			return new PromptPerCycleNode(nodeId, stage, sequence);
		}

		public void Init()
		{
			coroutine = StateMachineService.FrameUpdateRoot.StartCoroutine(PresentationCoroutine, State.Idle, NodeId);
			autoUnregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.GameStatus, nameof(StatusDatabase.GameStatus.CurrentGameState), OnGameStateChanged);

			foreach (var s in Sequences)
				s.Init();
		}

		public void DeInit()
		{
			autoUnregisterHelper.UnRegisterAll();
			coroutine?.Stop();
			coroutine = null;

			foreach (var s in Sequences)
				s.DeInit();
		}

		public void Destroy() { }

		public void Show()
		{
			showRequested = true;
		}

		private void OnGameStateChanged(StatusBlock _, string __)
		{
			if (StatusDatabase.GameStatus.CurrentGameState == GameState.Continuing)
				wantsToStart = StatusDatabase.GameStatus.NextLogicStage.Equals(stage);
		}

		private IEnumerator<CoroutineInstruction> PresentationCoroutine(IStateInfo<State> stateInfo)
		{
			while (true)
			{
				while (!wantsToStart)
					yield return null;

				while (!showRequested)
					yield return null;

				isShowing = true;
				showRequested = false;

				if (StatusDatabase.StageStatus.CurrentStage != stage)
				{
					yield return stateInfo.SetNextState(State.WaitForStage);
					while (StatusDatabase.StageStatus.CurrentStage != stage)
						yield return null;
				}

				yield return stateInfo.SetNextState(State.ShowSequence);
				yield return new CoroutineRun(sequence.Run());
				yield return stateInfo.SetNextState(State.Idle);

				wantsToStart = false;
				isShowing = false;
			}

			// ReSharper disable once IteratorNeverReturns - By design
		}
	}
}