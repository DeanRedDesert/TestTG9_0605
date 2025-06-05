using System.Collections.Generic;
using Midas.Core;
using Midas.Core.Coroutine;
using Midas.Core.StateMachine;
using Midas.CreditPlayoff.LogicToPresentation;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Data;
using Midas.Presentation.Game;
using Midas.Presentation.Sequencing;
using Coroutine = Midas.Core.Coroutine.Coroutine;

namespace Midas.CreditPlayoff.Presentation
{
	public sealed class CreditPlayoffController : IPresentationController, IPresentationNodeOwner, ISequenceOwner, IButtonControllerOwner
	{
		private CreditPlayoffStatus creditPlayoffStatus;
		private Coroutine idleCoroutine;
		private readonly SimpleSequence activate;
		private readonly SimpleSequence zoomBigSequence;
		private readonly SimpleSequence zoomBackToActiveSequence;

		public SimpleSequence Reset { get; }
		public SimpleSequence SpinToWinSequence { get; }
		public SimpleSequence SpinToLoseSequence { get; }
		public SimpleSequence OutroSequence { get; }

		public CreditPlayoffController()
		{
			creditPlayoffStatus = StatusDatabase.AddStatusBlock(new CreditPlayoffStatus());

			PresentationNodes = new IPresentationNode[] { new CreditPlayoffShowResultNode(), new CreditPlayoffDialUpNode() };
			ButtonControllers = new[] { new CreditPlayoffButtonController() };

			activate = new SimpleSequence("CreditPlayoff/Activate", CreditPlayoffSequenceEvent.Activate);
			zoomBigSequence = new SimpleSequence("CreditPlayoff/ZoomBig", CreditPlayoffSequenceEvent.ZoomBig);
			zoomBackToActiveSequence = new SimpleSequence("CreditPlayoff/ZoomBackToActive", CreditPlayoffSequenceEvent.ZoomBackToActive);
			SpinToWinSequence = new SimpleSequence("CreditPlayoff/SpinToWin", CreditPlayoffSequenceEvent.SpinToWin);
			SpinToLoseSequence = new SimpleSequence("CreditPlayoff/SpinToLose", CreditPlayoffSequenceEvent.SpinToLose);
			OutroSequence = new SimpleSequence("CreditPlayoff/Outro", CreditPlayoffSequenceEvent.Outro);
			Reset = new SimpleSequence("CreditPlayoff/Reset", CreditPlayoffSequenceEvent.Reset);

			Sequences = new Sequence[]
			{
				activate,
				zoomBigSequence,
				zoomBackToActiveSequence,
				SpinToWinSequence,
				SpinToLoseSequence,
				OutroSequence,
				Reset
			};
		}

		public void Init()
		{
			CreditPlayoffExpressions.Init(creditPlayoffStatus);
			foreach (var sequence in Sequences)
				sequence.Init();

			idleCoroutine = StateMachineService.FrameUpdateRoot.StartCoroutine(DoCreditPlayoff(), "CreditPlayoffIdle");
		}

		public void DeInit()
		{
			CreditPlayoffExpressions.DeInit();
			idleCoroutine?.Stop();
			idleCoroutine = null;

			foreach (var sequence in Sequences)
				sequence.DeInit();
		}

		public void Destroy()
		{
			if (creditPlayoffStatus != null)
			{
				StatusDatabase.RemoveStatusBlock(creditPlayoffStatus);
				creditPlayoffStatus = null;
			}
		}

		public IReadOnlyList<IPresentationNode> PresentationNodes { get; }
		public IReadOnlyList<Sequence> Sequences { get; }
		public IReadOnlyList<IButtonController> ButtonControllers { get; }

		private IEnumerator<CoroutineInstruction> DoCreditPlayoff()
		{
			while (!StatusDatabase.GameStatus.IsGameFlowReady)
				yield return null;

			while (true)
			{
				while (!IsAllowedGameState() || creditPlayoffStatus.State == CreditPlayoffState.Unavailable)
					yield return null;

				// If credit playoff is available, force it active unless in the global game identity.
				// Note: The ANZ hybrid GI is a global GI but still activates credit playoff.

				if (creditPlayoffStatus.State == CreditPlayoffState.Available && StatusDatabase.ConfigurationStatus.GameIdentity != GameIdentityType.Global)
					Communication.ToLogicSender.Send(new CreditPlayoffActivatedMessage());

				creditPlayoffStatus.SetPlayAllowed(false);

				while (creditPlayoffStatus.State != CreditPlayoffState.Unavailable)
				{
					if (!IsAllowedGameState() && !IsAllowedCreditPlayoffState())
						break;

					switch (creditPlayoffStatus.State)
					{
						case CreditPlayoffState.Available:
							activate.Start();
							while (activate.IsActive)
								yield return null;

							while ((creditPlayoffStatus.State == CreditPlayoffState.Available) && IsAllowedGameState())
								yield return null;
							break;

						case CreditPlayoffState.Idle:
							zoomBigSequence.Start();
							while (zoomBigSequence.IsActive)
								yield return null;

							creditPlayoffStatus.SetPlayAllowed(true);

							while (creditPlayoffStatus.State == CreditPlayoffState.Idle)
								yield return null;

							creditPlayoffStatus.SetPlayAllowed(false);

							if (!IsAllowedGameState() && !IsAllowedCreditPlayoffState())
								break;

							if (creditPlayoffStatus.State == CreditPlayoffState.Available)
							{
								zoomBackToActiveSequence.Start();
								while (zoomBackToActiveSequence.IsActive)
									yield return null;
							}

							yield return null;
							break;

						default:
							yield return null;
							break;
					}
				}

				Reset.Start();
				yield return null;
			}

			bool IsAllowedGameState() => StatusDatabase.GameStatus.CurrentGameState == GameState.Idle || StatusDatabase.GameStatus.CurrentGameState == GameState.OfferGamble;
			bool IsAllowedCreditPlayoffState() => StatusDatabase.GameStatus.CurrentGameState == GameState.StartingCreditPlayoff || StatusDatabase.GameStatus.CurrentGameState == GameState.ShowCreditPlayoffResult;
		}
	}
}