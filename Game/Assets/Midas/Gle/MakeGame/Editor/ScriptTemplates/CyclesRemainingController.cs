using System.Collections.Generic;
using System.Linq;
using Logic.Core.Utility;
using Midas.Core.Coroutine;
using Midas.Core.StateMachine;
using Midas.Gle.Presentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Data;
using Midas.Presentation.Game;
using Midas.Presentation.StageHandling;

namespace Game
{
	public enum CyclesRemainingState
	{
		Inactive,
		Plural,
		Singular,
		LastGame
	}

	public sealed class CyclesRemainingStatus : StatusBlock
	{
		#region Fields

		private StatusProperty<int> cyclesRemaining;
		private StatusProperty<CyclesRemainingState> counterState;
		private StatusProperty<bool> forceTriggerReveal;

		#endregion

		public int CyclesRemaining
		{
			get => cyclesRemaining.Value;
			set => cyclesRemaining.Value = value;
		}

		public CyclesRemainingState CurrentState
		{
			get => counterState.Value;
			set => counterState.Value = value;
		}

		public CyclesRemainingStatus(string name) : base(name)
		{
		}

		public bool ForceTriggerReveal
		{
			get => forceTriggerReveal.Value;
			set => forceTriggerReveal.Value = value;
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();

			cyclesRemaining = AddProperty(nameof(CyclesRemaining), 0);
			counterState = AddProperty(nameof(CurrentState), CyclesRemainingState.Inactive);
			forceTriggerReveal = AddProperty(nameof(ForceTriggerReveal), false);
		}
	}

	public sealed class CyclesRemainingController : IPresentationController
	{
		#region Fields

		private Coroutine coroutine;
		private readonly Stage stage;

		#endregion

		#region Properties

		public static CyclesRemainingStatus Data { get; private set; }

		#endregion

		#region Constructor

		public CyclesRemainingController(Stage stage)
		{
			this.stage = stage;
			Data = new CyclesRemainingStatus($"{this.stage.Name}RemainingStatus");
			StatusDatabase.AddStatusBlock(Data);
		}

		#endregion

		#region IPresentationController Implementation

		public void Init()
		{
			coroutine = StateMachineService.FrameUpdateRoot.StartCoroutine(CounterUpdate(), $"{stage}RemainingCounterUpdate");
		}

		public void DeInit()
		{
			coroutine?.Stop();
			coroutine = null;
		}

		public void Destroy()
		{
			StatusDatabase.RemoveStatusBlock(Data);
			Data = null;
		}

		#endregion

		#region Private Methods

		private IEnumerator<CoroutineInstruction> CounterUpdate()
		{
			while (GleGameController.GleStatus.CurrentGameResult == null || StatusDatabase.GameStatus.CurrentGameState == null)
				yield return null;

			var stageName = GameBase.GameInstance.GetStageIdFromLogicStage(stage);

			while (true)
			{
				while (!StatusDatabase.GameStatus.CurrentLogicStage.Equals(stage))
				{
					Data.CurrentState = CyclesRemainingState.Inactive;
					yield return null;
				}

				var cycles = GleGameController.GleStatus.CurrentGameResult.Current.Cycles.GetCompleteOrPendingCycles();
				if (StatusDatabase.GameStatus.CurrentGameState.Value == GameState.ShowResult || Data.ForceTriggerReveal)
					cycles = GleGameController.GleStatus.CurrentGameResult.Current.Inputs.GetCycles().PlayOne().MoveNext().GetCompleteOrPendingCycles();

				var total = 0;
				var completed = 0;
				foreach (var cycle in cycles.Where(v => v.Stage == stageName))
				{
					total += cycle.TotalCycles;
					completed += cycle.CompletedCycles;
				}

				var remaining = total - completed;
				Data.CyclesRemaining = remaining;
				Data.CurrentState = remaining switch
				{
					0 => CyclesRemainingState.LastGame,
					1 => CyclesRemainingState.Singular,
					_ => CyclesRemainingState.Plural
				};

				yield return null;
			}
		}

		#endregion
	}
}