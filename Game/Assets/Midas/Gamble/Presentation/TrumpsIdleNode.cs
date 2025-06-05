using System.Collections.Generic;
using Midas.Core;
using Midas.Core.Coroutine;
using Midas.Core.StateMachine;
using Midas.Gamble.LogicToPresentation;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Data;
using Midas.Presentation.Gaff;
using Midas.Presentation.Game;
using Midas.Presentation.StageHandling;

namespace Midas.Gamble.Presentation
{
	public sealed class TrumpsIdleNode : IPresentationNode
	{
		#region Gaff Buttons

		private sealed class TrumpsGaffSequence : IGaffSequence
		{
			public TrumpsSuit Suit { get; }
			public string Name { get; }
			public GaffType GaffType => GaffType.Show;

			public TrumpsGaffSequence(TrumpsSuit suit)
			{
				Suit = suit;
				Name = suit.ToString();
			}
		}

		private static readonly IReadOnlyList<TrumpsGaffSequence> dialUpButtons = new[]
		{
			new TrumpsGaffSequence(TrumpsSuit.Heart),
			new TrumpsGaffSequence(TrumpsSuit.Diamond),
			new TrumpsGaffSequence(TrumpsSuit.Club),
			new TrumpsGaffSequence(TrumpsSuit.Spade)
		};

		#endregion

		private TrumpsStatus trumpsStatus;
		private GaffStatus gaffStatus;
		private StageController stageController;
		private Coroutine trumpsIdleCoroutine;
		private bool showRequested;
		private bool isShowing;

		#region IPresentaitonNode Implementation

		public string NodeId => "TrumpsIdle";

		public void Init()
		{
			trumpsStatus = StatusDatabase.QueryStatusBlock<TrumpsStatus>();
			gaffStatus = StatusDatabase.GaffStatus;
			stageController = GameBase.GameInstance.GetPresentationController<StageController>();
			trumpsIdleCoroutine = StateMachineService.FrameUpdateRoot.StartCoroutine(DoTrumpsIdle(), "TrumpsIdle");
		}

		public void DeInit()
		{
			trumpsIdleCoroutine?.Stop();
			trumpsIdleCoroutine = null;
		}

		public void Destroy()
		{
		}

		public bool ReadyToStart => StatusDatabase.GameStatus.CurrentGameState == GameState.StartingGamble && !isShowing;

		public bool IsMainActionComplete => trumpsStatus.Selection != null && !trumpsStatus.IsIdleActive;

		public void Show()
		{
			showRequested = true;
			trumpsStatus.Selection = null;
		}

		#endregion

		#region Private Methods

		private IEnumerator<CoroutineInstruction> DoTrumpsIdle()
		{
			trumpsStatus.IsIdleActive = false;

			while (true)
			{
				while (!showRequested)
					yield return null;

				trumpsStatus.IsIdleActive = true;
				isShowing = true;
				showRequested = false;

				stageController.SwitchTo(Stages.Gamble);

				while (stageController.IsTransitioning())
					yield return null;

				while (trumpsStatus.Selection == null)
				{
					if (StatusDatabase.GameStatus.GameLogicPaused)
					{
						while (StatusDatabase.GameStatus.GameLogicPaused)
							yield return null;
					}

					yield return null;
				}

				if (gaffStatus.IsDialUpActive && trumpsStatus.Selection != TrumpsSelection.Decline)
				{
					gaffStatus.ActivateCustomDialUp(dialUpButtons);

					while (gaffStatus.IsPopupActive)
						yield return null;

					Communication.ToLogicSender.Send(new TrumpsDialUpMessage(dialUpButtons[gaffStatus.SelectedCustomGaffIndex].Suit));
				}

				Communication.ToLogicSender.Send(new TrumpsSelectionMessage(trumpsStatus.Selection.Value));
				trumpsStatus.IsIdleActive = false;

				while (StatusDatabase.GameStatus.CurrentGameState == GameState.StartingGamble)
					yield return null;

				trumpsStatus.Selection = null;
				isShowing = false;
			}

			// ReSharper disable once IteratorNeverReturns - By design
		}

		#endregion
	}
}