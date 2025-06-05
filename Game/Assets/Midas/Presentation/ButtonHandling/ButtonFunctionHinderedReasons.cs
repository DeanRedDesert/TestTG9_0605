using System;

namespace Midas.Presentation.ButtonHandling
{
	[Flags]
	public enum ButtonFunctionHinderedReasons
	{
		None = 0,
		BecauseGameCycleActive = 1,
		BecauseCreditPlayoffStatusActive = 2,
		BecauseCreditPlayoffStatusChoosing = 4, //Confirm dialog for RWPO is open
		BecausePOSOpen = 8,
		BecauseInterruptable = 16, //also true if winPres running
		BecauseInterruptableWinPres = 32, //only true if winpres running
		BecauseNotPlayerWagerOfferable = 64 //true if BankStatus.IsPlayerWagerOfferable==false
	}
}