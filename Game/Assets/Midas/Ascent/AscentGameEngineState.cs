namespace Midas.Ascent
{
	internal enum AscentGameEngineState
	{
		Idle,
		AwaitEnrollComplete,
		StartGameCycle,
		AwaitOutcomeResponse,
		ShowResult,
		LogicGameComplete,
		OfferGamble,
		StartGamble,
		ContinueGamble,
		AwaitGambleOutcomeResponse,
		AwaitAbortGambleOutcomeResponse,
		ShowGambleResult,
		AwaitFinalize,
		Finalize,
		History,
		Utility
	}
}