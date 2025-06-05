using System.Collections.Generic;
using Midas.Core;
using Midas.Core.Coroutine;
using Midas.Presentation.Data;
using Midas.Presentation.Denom;
using Midas.Presentation.Game;
using Midas.Presentation.General;

namespace Midas.Presentation.Dashboard
{
	public sealed partial class DashboardController
	{
		private static IEnumerator<CoroutineInstruction> GameNameCoroutine()
		{
			while (StatusDatabase.ConfigurationStatus?.DenomConfig == null)
				yield return null;

			var name = GameBase.GameInstance.GameName;
			if (StatusDatabase.GameStatus.GameMode == FoundationGameMode.Utility)
			{
				StatusDatabase.GameStatus.GameName = name;
				yield break;
			}

			while (true)
			{
				switch (GameBase.GameInstance.GameNameStyle)
				{
					// if there is more than one denomination, show the active denom appended to the game name.
					case GameNameStyle.Automatic:
						StatusDatabase.GameStatus.GameName = DenomExpressions.AllDenoms.Count > 1 ? $"{name} {FormatDenom()}" : name;
						break;

					// For any amount of possible denoms, append the active denom to the game name.
					case GameNameStyle.ForceGameNameIncludeDenom:
						StatusDatabase.GameStatus.GameName = $"{name} {FormatDenom()}";
						break;

					default:
					case GameNameStyle.ForceGameNameOnly:
						StatusDatabase.GameStatus.GameName = name;
						break;
				}

				var lastDenom = StatusDatabase.ConfigurationStatus.DenomConfig.CurrentDenomination;
				var lastDenomCount = DenomExpressions.AllDenoms.Count;
				while (lastDenom == StatusDatabase.ConfigurationStatus.DenomConfig.CurrentDenomination && lastDenomCount == DenomExpressions.AllDenoms.Count)
					yield return null;
			}

			string FormatDenom()
			{
				var cf = StatusDatabase.ConfigurationStatus.CreditAndMoneyFormatter;
				return cf.GetFormatted(MoneyAndCreditDisplayMode.MoneyBase, StatusDatabase.ConfigurationStatus.DenomConfig.CurrentDenomination, CreditDisplaySeparatorMode.NoSeparator);
			}
		}
	}
}