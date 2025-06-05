using System;

namespace Midas.Presentation.Dashboard
{
	[Flags]
	public enum GameMessage
	{
		None = 0,
		GameOver = 1,
		PressTakeWin = 2,
		GambleAvailable = 4
	}
}