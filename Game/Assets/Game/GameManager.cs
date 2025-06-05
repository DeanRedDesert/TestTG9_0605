using Midas.Presentation.Game;

namespace Game
{
	public sealed class GameManager : GameManagerBase
	{
		protected override GameBase InstantiateGame() => new Game();
	}
}