namespace Midas.LogicToPresentation.Messages
{
	public enum GameState
	{
		Idle,
		StartingCreditPlayoff,
		ShowCreditPlayoffResult,
		Starting,
		Continuing,
		ShowResult,
		OfferGamble,
		StartingGamble,
		ShowGambleResult,
		History
	}

	public static class GameStateExt
	{
		public static bool IsGambleState(this GameState? gs) => gs is GameState.StartingGamble || gs is GameState.ShowGambleResult;
	}
}