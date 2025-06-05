using System.Collections.Generic;

namespace Midas.Core
{
	public struct GameButtonBehaviours
	{
		public GameButtonStatus CollectButton { get; private set; }
		public GameButtonStatus VolumeButton { get; private set; }
		public GameButtonStatus DenominationSelectionButtons { get; private set; }
		public GameButtonStatus BetButtons { get; private set; }
		public GameButtonStatus ReserveButton { get; private set; }
		public GameButtonStatus InfoButton { get; private set; }
		public GameButtonStatus MoreGamesButton { get; private set; }

		public GameButtonBehaviours(IReadOnlyList<GameButtonBehaviour> gameButtonBehaviors)
		{
			CollectButton = GameButtonStatus.Active;
			VolumeButton = GameButtonStatus.Active;
			DenominationSelectionButtons = GameButtonStatus.Active;
			BetButtons = GameButtonStatus.Active;
			ReserveButton = GameButtonStatus.Active;
			InfoButton = GameButtonStatus.Active;
			MoreGamesButton = GameButtonStatus.Active;

			foreach (var behaviour in gameButtonBehaviors)
			{
				switch (behaviour.ButtonType)
				{
					case GameButton.Collect:
						CollectButton = behaviour.ButtonStatus;
						break;
					case GameButton.Volume:
						VolumeButton = behaviour.ButtonStatus;
						break;
					case GameButton.DenominationSelection:
						DenominationSelectionButtons = behaviour.ButtonStatus;
						break;
					case GameButton.Bets:
						BetButtons = behaviour.ButtonStatus;
						break;
					case GameButton.Reserve:
						ReserveButton = behaviour.ButtonStatus;
						break;
					case GameButton.Info:
						InfoButton = behaviour.ButtonStatus;
						break;
					case GameButton.MoreGames:
						MoreGamesButton = behaviour.ButtonStatus;
						break;
				}
			}
		}
	}
}