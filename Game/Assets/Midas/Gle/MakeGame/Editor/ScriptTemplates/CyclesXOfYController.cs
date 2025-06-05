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
	public sealed class CyclesXOfYStatus : StatusBlock
	{
		#region Fields

		private StatusProperty<bool> isActive;
		private StatusProperty<int> cyclesTotal;
		private StatusProperty<int> cyclesCurrent;
		private StatusProperty<bool> forceTriggerReveal;

		#endregion

		public bool IsActive
		{
			get => isActive.Value;
			set => isActive.Value = value;
		}

		public int CyclesTotal
		{
			get => cyclesTotal.Value;
			set => cyclesTotal.Value = value;
		}

		public int CyclesCurrent
		{
			get => cyclesCurrent.Value;
			set => cyclesCurrent.Value = value;
		}

		public bool ForceTriggerReveal
		{
			get => forceTriggerReveal.Value;
			set => forceTriggerReveal.Value = value;
		}

		public CyclesXOfYStatus(string name) : base(name)
		{
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();

			cyclesTotal = AddProperty(nameof(CyclesTotal), 0);
			cyclesCurrent = AddProperty(nameof(CyclesCurrent), 0);
			isActive = AddProperty(nameof(IsActive), false);
			forceTriggerReveal = AddProperty(nameof(ForceTriggerReveal), false);
		}
	}

	public sealed class CyclesXOfYController : IPresentationController
	{
		#region Fields

		private Coroutine coroutine;
		private readonly Stage stage;

		#endregion

		#region Properties

		public static CyclesXOfYStatus Data { get; private set; }

		#endregion

		#region Constructor

		public CyclesXOfYController(Stage stage)
		{
			this.stage = stage;
			Data = new CyclesXOfYStatus($"{this.stage.Name}XOfYStatus");
			StatusDatabase.AddStatusBlock(Data);
		}

		#endregion

		#region IPresentationController Implementation

		public void Init()
		{
			coroutine = StateMachineService.FrameUpdateRoot.StartCoroutine(CounterUpdate(), $"{stage}XOfYCounterUpdate");
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
					Data.IsActive = false;
					yield return null;
				}

				Data.IsActive = true;

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

				Data.CyclesTotal = total;
				Data.CyclesCurrent = completed;

				yield return null;
			}
		}

		#endregion
	}
}