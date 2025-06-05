using System.Collections.Generic;
using Midas.Core.Coroutine;
using Midas.Core.StateMachine;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Game;
using Midas.Presentation.StageHandling;

namespace Game.GameMessages
{
	public sealed class GameMessageController : IPresentationController
	{
		private readonly Stage stage;
		private readonly GameMessageStatus gameInfoMessageStatus;
		private Coroutine coroutine;
		private DisplayState displayState = DisplayState.None;

		public GameMessageController(Stage stage)
		{
			this.stage = stage;
			gameInfoMessageStatus = new GameMessageStatus(stage);
			StatusDatabase.AddStatusBlock(gameInfoMessageStatus);
		}

		public void Init()
		{
			coroutine = StateMachineService.FrameUpdateRoot.StartCoroutine(GameInfoUpdate(), $"{stage.Name}GameInfoUpdate");
		}

		public void DeInit()
		{
			coroutine?.Stop();
			coroutine = null;
		}

		public void Destroy()
		{
			StatusDatabase.RemoveStatusBlock(gameInfoMessageStatus);
		}

		private IEnumerator<CoroutineInstruction> GameInfoUpdate()
		{
			displayState = DisplayState.None;
			while (true)
			{
				var newDisplayState = DisplayState.None;
				if (PopupStatus.IsDenomMenuOpen || PopupStatus.IsInfoOpen)
				{
					newDisplayState = DisplayState.Disabled;
				}
				else if (!StatusDatabase.StageStatus.DesiredStage.Equals(stage))
				{
					newDisplayState = DisplayState.Disabled;
				}
				else if (StatusDatabase.DetailedWinPresStatus.IsActive)
				{
					newDisplayState = DisplayState.ShowWinMessage;
				}
				else if (!StatusDatabase.DetailedWinPresStatus.IsActive)
				{
					if (StatusDatabase.StageStatus.DesiredStage.Equals(stage))
						newDisplayState = DisplayState.ShowGameMessage;
				}

				if (newDisplayState != displayState)
				{
					displayState = newDisplayState;

					switch (displayState)
					{
						case DisplayState.ShowWinMessage:
							gameInfoMessageStatus.IsGameInfoVisible = false;
							gameInfoMessageStatus.IsWinInfoVisible = true;
							gameInfoMessageStatus.ActiveGameInfoMessageIndex++;
							break;
						case DisplayState.ShowGameMessage:
							gameInfoMessageStatus.IsGameInfoVisible = true;
							gameInfoMessageStatus.IsWinInfoVisible = false;
							break;
						default:
						case DisplayState.Disabled:
						case DisplayState.None:
							gameInfoMessageStatus.IsGameInfoVisible = false;
							gameInfoMessageStatus.IsWinInfoVisible = false;
							break;
					}
				}

				yield return null;
			}
			// ReSharper disable once IteratorNeverReturns
		}

		private enum DisplayState
		{
			ShowGameMessage,
			ShowWinMessage,
			None,
			Disabled
		}
	}
}
