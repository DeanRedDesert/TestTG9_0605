namespace Logic.Core.Engine
{
	public enum Lifespan
	{
		OneCycle, // Will appear in the results for the current cycle and then be removed.
		OneGame, // Will appear in the results for the current cycle and then be added as input to all following cycles until the end of the current game.
		Permanent // Will appear in the results for the current cycle and then be added as input to all following cycles.
	}
}