using System.Collections.Generic;
using Midas.Core;
using Midas.Core.General;

namespace Midas.LogicToPresentation.Messages
{
	public sealed class DemoChangeModeMessage : DebugMessage
	{
		public FoundationGameMode GameMode { get; }

		public string UtilityTheme { get; }
		public KeyValuePair<string, string> UtilityPaytables { get; }
		public Money UtilityDenomination { get; }

		public DemoChangeModeMessage(FoundationGameMode gameMode)
		{
			GameMode = gameMode;
		}

		public DemoChangeModeMessage(FoundationGameMode gameMode, string utilityTheme, KeyValuePair<string, string> utilityPaytables, Money utilityDenomination)
		{
			GameMode = gameMode;
			UtilityTheme = utilityTheme;
			UtilityPaytables = utilityPaytables;
			UtilityDenomination = utilityDenomination;
		}
	}
}