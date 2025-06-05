using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Midas.Core.Coroutine;
using Midas.Core.General;
using Midas.Core.StateMachine;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Data;
using Midas.Presentation.Game;
using Midas.Presentation.Progressives;
using Midas.Presentation.Reels.SmartSymbols;
using Midas.Presentation.Sequencing;
using Midas.Presentation.StageHandling;

namespace Game
{
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public enum GamePlaySequenceIds
	{
		Play,
		PlayComplete
	}

	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public enum PreShowWinSequenceIds
	{
		HighlightTrigger,
		PreShowWinComplete
	}

	/// <summary>
	/// This is a simple game node that should have the features needed for good the majority of game play sequences. It allows
	/// </summary>
	public sealed class SimpleGameNode : IPresentationNode, ISequenceOwner
	{
		private enum State
		{
			Idle,
			RunningGame,
			AwardingProgressives,
			RunningPreShowWin
		}

		private readonly Stage stage;
		private Coroutine gameCoroutine;
		private bool showRequested;
		private bool isShowing;
		private readonly AutoUnregisterHelper autoUnregisterHelper = new AutoUnregisterHelper();
		private readonly List<Sequence> sequences;
		private readonly Sequence gamePlaySequence;
		private readonly List<(Func<bool> CheckAndConfigure, Sequence Sequence)> preShowSequences = new List<(Func<bool> CheckAndConfigure, Sequence Sequence)>();
		private IReadOnlyList<ProgressiveAwardController> progressiveAwardControllers;

		public SimpleGameNode(string nodeId, Stage stage)
		{
			NodeId = nodeId;
			this.stage = stage;

			gamePlaySequence = SimpleSequence.Create<GamePlaySequenceIds>($"{stage.Name}/Play");
			sequences = new List<Sequence>
			{
				gamePlaySequence
			};
		}

		public void AddPreShowSequence(string name, Func<bool> checkAndConfigure)
		{
			var seq = SimpleSequence.Create<PreShowWinSequenceIds>($"{stage.Name}/{name}");
			preShowSequences.Add((checkAndConfigure, seq));
			sequences.Add(seq);
		}

		public void AddPreShowSequence<T>(string name, Func<bool> checkAndConfigure) where T : Enum
		{
			var seq = SimpleSequence.Create<T>($"{stage.Name}/{name}");
			preShowSequences.Add((checkAndConfigure, seq));
			sequences.Add(seq);
		}

		#region IPresentationNode Implementation

		public string NodeId { get; }

		public void Init()
		{
			gameCoroutine = StateMachineService.FrameUpdateRoot.StartCoroutine(PresentationCoroutine, State.Idle, $"{stage.Name} Play Node");
			showRequested = false;
			autoUnregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.GameStatus, nameof(StatusDatabase.GameStatus.CurrentGameState), OnGameStateChanged);
			progressiveAwardControllers = GameBase.GameInstance.GetInterfaces<ProgressiveAwardController>().ToArray();

			foreach (var seq in Sequences)
				seq.Init();
		}

		private void OnGameStateChanged(StatusBlock _, string __)
		{
			if (StatusDatabase.GameStatus.CurrentGameState == GameState.ShowResult)
				ReadyToStart = StatusDatabase.GameStatus.CurrentLogicStage.Equals(stage);
		}

		public void DeInit()
		{
			foreach (var seq in Sequences)
				seq.DeInit();

			autoUnregisterHelper.UnRegisterAll();
			gameCoroutine.Stop();
			gameCoroutine = null;
			progressiveAwardControllers = null;
		}

		public void Destroy()
		{
		}

		public bool ReadyToStart { get; private set; }

		public bool IsMainActionComplete => !(showRequested || isShowing);

		public void Show()
		{
			showRequested = true;
		}

		#endregion

		#region ISequenceOwner Implementation

		public IReadOnlyList<Sequence> Sequences => sequences;

		#endregion

		private IEnumerator<CoroutineInstruction> PresentationCoroutine(IStateInfo<State> stateInfo)
		{
			var smartSymbolController = GameBase.GameInstance.GetPresentationController<SmartSymbolController>(false);

			while (true)
			{
				while (!ReadyToStart)
					yield return null;

				while (ReadyToStart && !showRequested)
					yield return null;

				isShowing = true;
				showRequested = false;

				if (ReadyToStart)
				{
					smartSymbolController?.RefreshSmartSymbolData();

					yield return stateInfo.SetNextState(State.RunningGame);
					yield return new CoroutineRun(gamePlaySequence.Run(), "Spin Reels");

					yield return stateInfo.SetNextState(State.AwardingProgressives);
					yield return new CoroutineRun(AwardProgressives(), "Awarding Progressives");

					yield return stateInfo.SetNextState(State.RunningPreShowWin);
					foreach (var preShow in preShowSequences)
					{
						if (preShow.CheckAndConfigure())
							yield return new CoroutineRun(preShow.Sequence.Run(), $"Running {preShow.Sequence.Name}");
					}

					yield return stateInfo.SetNextState(State.Idle);
				}

				ReadyToStart = false;
				isShowing = false;
			}
			// ReSharper disable once IteratorNeverReturns - by design
		}

		private IEnumerator<CoroutineInstruction> AwardProgressives()
		{
			var pa = StatusDatabase.ProgressiveStatus.ProgressiveAwards;

			if (pa == null || pa.Count == 0)
				yield break;

			for (var i = 0; i < pa.Count; i++)
			{
				var progAwardCont = progressiveAwardControllers.First(p => p.CanAwardProgressive(i));
				progAwardCont.StartProgressiveAward(i);
				while (progAwardCont.IsAwarding)
					yield return null;
			}
		}
	}
}