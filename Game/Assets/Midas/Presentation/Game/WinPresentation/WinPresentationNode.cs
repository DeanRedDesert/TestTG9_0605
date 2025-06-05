using Midas.Core.General;
using Midas.Core.StateMachine;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Sequencing;
using Midas.Presentation.StageHandling;
using Midas.Presentation.WinPresentation;

namespace Midas.Presentation.Game.WinPresentation
{
	public abstract class WinPresentationNode : IHistoryPresentationNode
	{
		public string NodeId { get; }
		private Stage Stage { get; }

		protected abstract Sequence Sequence { get; }

		protected IDetailedWinPres DetailedWinPres { get; }

		private bool NeedsToSummarizeProgressiveWins { get; }

		private State IdleState { get; } = new State("Idle", true);
		private State ActiveState { get; } = new State("Active");
		private State WaitSwitchStageState { get; } = new State("WaitSwitchStage");

		protected WinPresentationNode(string nodeId, Stage stage, bool needsToSummarizeProgressiveWins = false)
		{
			NodeId = nodeId;
			Stage = stage;
			NeedsToSummarizeProgressiveWins = needsToSummarizeProgressiveWins;
			DetailedWinPres = new DetailedWinPres();
		}

		public virtual void Init()
		{
			DetailedWinPres.Init();

			var builder = new Builder(NodeId, StateMachineService.FrameUpdateRoot);
			if (CreateRegularWinPresStateMachine(builder))
			{
				stmRegularWinPres = builder.CreateStateMachine();
			}

			winPresentationStatus = StatusDatabase.WinPresentationStatus;
//			_winMeterResetController = GameBase.GameInstance.GetPresentationController<WinMeterResetController>();
		}

		public virtual void DeInit()
		{
			StateMachines.Destroy(ref stmRegularWinPres);
			DetailedWinPres.DeInit();
		}

		public virtual void Destroy()
		{
		}

		public bool ReadyToStart
		{
			get
			{
				var cs = StatusDatabase.GameStatus.CurrentGameState;

				return Sequence != null &&
					(cs == GameState.Continuing || cs == GameState.OfferGamble) &&
					StatusDatabase.GameStatus.CurrentLogicStage.Equals(Stage) &&
					StatusDatabase.ProgressiveStatus.CurrentProgressiveAward == null &&
					!winPresentationStatus.WinPresentationComplete;
			}
		}

		public bool IsMainActionComplete =>
			IdleState.Entered && !startRegularWinPres ||
			winPresentationStatus.WinPresentationComplete;

		public void Show()
		{
			startRegularWinPres = true;
			stopRegularWinPres = false;
		}

		public void Interrupt()
		{
			startRegularWinPres = false;
			stopRegularWinPres = true;
			DetailedWinPres?.Stop();
		}

		#region Protected

		private bool CreateRegularWinPresStateMachine(Builder builder)
		{
			var gameResultStatus = StatusDatabase.QueryStatusBlock<GameResultStatus>();

			builder.In(IdleState)
				.OnExitDo((_, __) => startRegularWinPres = false)
				.If(() => startRegularWinPres)
				.Then(WaitSwitchStageState);

			builder.In(WaitSwitchStageState)
				.OnEnterDo(_ => stageSwitcher.SwitchStage(Stage))
				.If(() => !stageSwitcher.IsStageTransitioning())
				.Then(ActiveState);

			builder.In(ActiveState)
				.OnEnterDo(_ =>
				{
					var winnerStartStartValue = Credit.FromMoney(StatusDatabase.BankStatus.TotalAward - StatusDatabase.BankStatus.CycleAward);
					Log.Instance.Info($"RegularWinPresActive of {Stage} WinnerStartValue={winnerStartStartValue} WinnerEndValue={StatusDatabase.BankStatus.TotalAward}");
					DetailedWinPres.Start(gameResultStatus.IsBaseGameCycle() ? CycleMode.Forever : CycleMode.AtLeastOnce);
					Sequence.Start();
				})
				.OnExitDo((_, __) =>
				{
					stopRegularWinPres = false;
					winPresentationStatus.WinPresentationComplete = true;
					if (!gameResultStatus.IsBaseGameCycle() || !gameResultStatus.IsGameFinished())
						DetailedWinPres.Stop();
				})
				.If(() => (!Sequence.IsActive || stopRegularWinPres) && DetailedWinPres.CanStop())
				.Then(IdleState);
			return true;
		}

		#endregion

		#region Private

		private bool startRegularWinPres;
		private bool stopRegularWinPres;

		private StateMachine stmRegularWinPres;

		private readonly StageSwitcher stageSwitcher = new StageSwitcher();
		private WinPresentationStatus winPresentationStatus;

		#endregion

		public bool ReadyToShowHistory
		{
			get
			{
				return StatusDatabase.HistoryStatus.HistoryStepType == HistoryStepType.Game && StatusDatabase.GameStatus.CurrentLogicStage.Equals(Stage);
			}
		}

		public void ShowHistory()
		{
			DetailedWinPres.Start(CycleMode.Forever);
		}

		public void HideHistory()
		{
			DetailedWinPres.Stop();
		}
	}
}