using System.Collections.Generic;
using Midas.Core.Coroutine;
using Midas.Core.StateMachine;
using Midas.CreditPlayoff.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Data;
using Midas.Presentation.Game;

namespace Midas.CreditPlayoff.Presentation
{
	public sealed class CreditPlayoffShowResultNode : IPresentationNode
	{
		private CreditPlayoffStatus creditPlayoffStatus;
		private CreditPlayoffController controller;
		private Coroutine idleCoroutine;
		private bool showRequested;
		private bool isShowing;

		#region IPresentaitonNode Implementation

		public string NodeId => "CreditPlayoffShowResult";

		public void Init()
		{
			controller = GameBase.GameInstance.GetPresentationController<CreditPlayoffController>();
			creditPlayoffStatus = StatusDatabase.QueryStatusBlock<CreditPlayoffStatus>();
			idleCoroutine = StateMachineService.FrameUpdateRoot.StartCoroutine(DoCreditPlayoff(), "CreditPlayoffShowResult");
		}

		public void DeInit()
		{
			idleCoroutine?.Stop();
			idleCoroutine = null;
		}

		public void Destroy()
		{
		}

		public bool ReadyToStart => StatusDatabase.GameStatus.CurrentGameState == GameState.ShowCreditPlayoffResult && !isShowing && IsMainActionComplete;

		public bool IsMainActionComplete { get; private set; }

		public void Show()
		{
			IsMainActionComplete = false;
			showRequested = true;
		}

		#endregion

		#region Private Methods

		private IEnumerator<CoroutineInstruction> DoCreditPlayoff()
		{
			IsMainActionComplete = true;
			while (true)
			{
				while (!showRequested)
					yield return null;

				isShowing = true;
				showRequested = false;

				switch (creditPlayoffStatus.State)
				{
					case CreditPlayoffState.Win:
					{
						controller.SpinToWinSequence.Start();
						while (controller.SpinToWinSequence.IsActive)
							yield return null;

						controller.OutroSequence.Start();
						while (controller.OutroSequence.IsActive)
							yield return null;
						break;
					}
					case CreditPlayoffState.Loss:
					{
						controller.SpinToLoseSequence.Start();
						while (controller.SpinToLoseSequence.IsActive)
							yield return null;

						controller.OutroSequence.Start();
						while (controller.OutroSequence.IsActive)
							yield return null;
						break;
					}
				}

				controller.Reset.Start();
				IsMainActionComplete = true;

				while (creditPlayoffStatus.State == CreditPlayoffState.Win || creditPlayoffStatus.State == CreditPlayoffState.Loss)
					yield return null;

				isShowing = false;
			}

			// ReSharper disable once IteratorNeverReturns - By design
		}
	}

	#endregion
}