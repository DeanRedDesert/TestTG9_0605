using Midas.Core;
using Midas.Core.General;
using Midas.Presentation.Data;

namespace Midas.Presentation.Game
{
	public static class GameInitiator
	{
		public static void StartGame()
		{
			switch (StatusDatabase.GameStatus.GameMode)
			{
				case FoundationGameMode.History:
					break;
				case FoundationGameMode.Utility:
					StatusDatabase.GameStatus.GamePlayRequested = true;
					break;
				case FoundationGameMode.Play:
					if (StatusDatabase.BankStatus.WagerableMeter > Money.Zero)
						StatusDatabase.GameStatus.GamePlayRequested = true;
					break;
			}
		}
	}
}