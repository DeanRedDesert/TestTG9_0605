using System.Collections.Generic;
using Midas.Core;
using Midas.Core.Coroutine;
using Midas.CreditPlayoff.LogicToPresentation;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation;
using Midas.Presentation.Data;
using Midas.Presentation.Gaff;
using Midas.Presentation.Game;

namespace Midas.CreditPlayoff.Presentation
{
	public sealed class CreditPlayoffDialUpNode : IPresentationNode
	{
		#region Gaff Buttons

		private sealed class CreditPlayoffGaffSequence : IGaffSequence
		{
			public bool Win { get; }
			public string Name { get; }
			public GaffType GaffType => GaffType.Show;

			public CreditPlayoffGaffSequence(bool win)
			{
				Win = win;
				Name = win ? "Win" : "Loss";
			}
		}

		private static readonly IReadOnlyList<CreditPlayoffGaffSequence> dialUpButtons = new[]
		{
			new CreditPlayoffGaffSequence(true),
			new CreditPlayoffGaffSequence(false)
		};

		#endregion

		private GaffStatus gaffStatus;
		private Coroutine dialUpCoroutine;
		private bool showRequested;
		private bool isShowing;

		public CreditPlayoffDialUpNode()
		{
			NodeId = "CreditPlayoffStarting";
		}

		#region IPresentationNode implementation

		public string NodeId { get; }

		public bool ReadyToStart => StatusDatabase.GameStatus.CurrentGameState == GameState.StartingCreditPlayoff && gaffStatus.IsDialUpActive && !isShowing && IsMainActionComplete;

		public bool IsMainActionComplete { get; private set; }

		public void Init()
		{
			gaffStatus = StatusDatabase.GaffStatus;
			dialUpCoroutine = FrameUpdateService.Update.StartCoroutine(DoCreditPlayoffDialUp());
		}

		public void DeInit()
		{
			dialUpCoroutine?.Stop();
			dialUpCoroutine = null;
		}

		public void Destroy()
		{
		}

		public void Show()
		{
			IsMainActionComplete = false;
			showRequested = true;
		}

		private IEnumerator<CoroutineInstruction> DoCreditPlayoffDialUp()
		{
			IsMainActionComplete = true;
			while (true)
			{
				while (!showRequested)
					yield return null;

				isShowing = true;
				showRequested = false;

				gaffStatus.ActivateCustomDialUp(dialUpButtons);

				while (gaffStatus.IsPopupActive)
					yield return null;

				Communication.ToLogicSender.Send(new CreditPlayoffDialUpMessage(dialUpButtons[gaffStatus.SelectedCustomGaffIndex].Win));

				IsMainActionComplete = true;

				while (StatusDatabase.GameStatus.CurrentGameState == GameState.StartingCreditPlayoff)
					yield return null;

				isShowing = false;
			}
		}

		#endregion
	}
}