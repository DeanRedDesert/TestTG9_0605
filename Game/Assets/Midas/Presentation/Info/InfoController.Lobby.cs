using System.Collections.Generic;
using Midas.Core;
using Midas.Presentation.Data;

namespace Midas.Presentation.Info
{
	public sealed partial class InfoController
	{
		internal IReadOnlyList<string> GetLobbyButtonNames() => lobbyButtonNames;

		private static IReadOnlyList<string> CreateButtonNames()
		{
			var buttonNames = new List<string>();
			var pidStatus = StatusDatabase.PidStatus;
			if (pidStatus.Config.IsGameRulesEnabled)
				buttonNames.Add("GAME RULES");

			if (pidStatus.Config.GameInformationDisplayStyle != GameInformationDisplayStyle.None)
				buttonNames.Add("GAME INFORMATION");

			if (pidStatus.Config.IsRequestServiceEnabled)
				buttonNames.Add(pidStatus.IsServiceRequested ? "CANCEL SERVICE" : "REQUEST SERVICE");

			if (!StatusDatabase.GameStatus.GameIsIdle)
				return buttonNames;

			switch (pidStatus.Config.SessionTrackingOption)
			{
				case SessionTrackingOption.Disabled:
					break;

				case SessionTrackingOption.PlayerControlled:
					buttonNames.Add(pidStatus.Session.IsSessionTrackingActive ? "VIEW SESSION" : "START SESSION TRACKING");
					break;

				case SessionTrackingOption.Viewable:
					if (pidStatus.Session.IsSessionTrackingActive)
						buttonNames.Add("VIEW SESSION");
					break;
			}

			return buttonNames;
		}
	}
}