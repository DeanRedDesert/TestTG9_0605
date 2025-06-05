using System.Collections.Generic;
using Midas.Core.Coroutine;
using Midas.Core.General;
using Midas.Core.StateMachine;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Data;
using Midas.Presentation.Game;
using Midas.Presentation.StageHandling;

namespace Game
{
	public sealed class SimpleTransitionNode : IPresentationNode
	{
		private enum State
		{
			Idle,
			Transitioning
		}

		private readonly Stage stage;
		private StageController stageController;
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

		public SimpleTransitionNode(string nodeId, Stage stage)
		{
			NodeId = nodeId;
			this.stage = stage;
		}

		public void Init()
		{
			stageController = GameBase.GameInstance.GetPresentationController<StageController>();
			coroutine = StateMachineService.FrameUpdateRoot.StartCoroutine(PresentationCoroutine, State.Idle, NodeId);
			autoUnregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.GameStatus, nameof(StatusDatabase.GameStatus.CurrentGameState), OnGameStateChanged);
		}

		public void DeInit()
		{
			autoUnregisterHelper.UnRegisterAll();
			coroutine?.Stop();
			coroutine = null;
		}

		public void Destroy() { }

		public void Show()
		{
			showRequested = true;
		}

		private void OnGameStateChanged(StatusBlock _, string __)
		{
			if (StatusDatabase.GameStatus.CurrentGameState == GameState.Continuing)
				wantsToStart = !StatusDatabase.StageStatus.CurrentStage.Equals(stage) && StatusDatabase.GameStatus.NextLogicStage.Equals(stage);
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

				yield return stateInfo.SetNextState(State.Transitioning);
				stageController.SwitchTo(stage);
				while (stageController.IsTransitioning())
					yield return null;

				yield return stateInfo.SetNextState(State.Idle);

				wantsToStart = false;
				isShowing = false;
			}

			// ReSharper disable once IteratorNeverReturns - By design
		}
	}
}