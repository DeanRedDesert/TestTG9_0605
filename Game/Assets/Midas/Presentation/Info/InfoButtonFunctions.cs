using System.Collections.Generic;
using System.Collections.ObjectModel;
using Midas.Presentation.ButtonHandling;

namespace Midas.Presentation.Info
{
	[ButtonFunctions("Info")]
	public static class InfoButtonFunctions
	{
		public static ButtonFunction ExitInfo => ButtonFunction.Create(ButtonFunctions.InfoBase + 0);
		public static ButtonFunction LobbyButton1 => ButtonFunction.Create(ButtonFunctions.InfoBase + 1);

		public static ButtonFunction LobbyButton2 => ButtonFunction.Create(ButtonFunctions.InfoBase + 2);

		public static ButtonFunction LobbyButton3 => ButtonFunction.Create(ButtonFunctions.InfoBase + 3);
		public static ButtonFunction LobbyButton4 => ButtonFunction.Create(ButtonFunctions.InfoBase + 4);
		public static ButtonFunction LobbyButton5 => ButtonFunction.Create(ButtonFunctions.InfoBase + 5);
		public static ButtonFunction NextRulesPage => ButtonFunction.Create(ButtonFunctions.InfoBase + 6);
		public static ButtonFunction PreviousRulesPage => ButtonFunction.Create(ButtonFunctions.InfoBase + 7);

		public static ButtonFunction StartSession => ButtonFunction.Create(ButtonFunctions.InfoBase + 8);
		public static ButtonFunction StopSession => ButtonFunction.Create(ButtonFunctions.InfoBase + 9);

		public static IReadOnlyList<ButtonFunction> InfoButtons => new ReadOnlyCollection<ButtonFunction>(new[] { ExitInfo, NextRulesPage, PreviousRulesPage, StartSession, StopSession });
		public static IReadOnlyList<ButtonFunction> InfoLobbyButtons => new ReadOnlyCollection<ButtonFunction>(new[] { LobbyButton1, LobbyButton2, LobbyButton3, LobbyButton4, LobbyButton5 });
	}
}