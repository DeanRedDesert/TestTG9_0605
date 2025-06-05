namespace Midas.LogicToPresentation.Messages
{
	public sealed class GameCycleStepMessage : IMessage
	{
		public GameState GameState { get; }

		public GameCycleStepMessage(GameState gameState)
		{
			GameState = gameState;
		}
	}
}