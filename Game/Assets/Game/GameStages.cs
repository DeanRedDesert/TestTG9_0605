using Midas.Presentation.StageHandling;

namespace Game
{
	[Stages("Game")]
	public static class GameStages
	{
		public static Stage Base { get; } = new Stage(Midas.Presentation.StageHandling.Stages.GameSpecificStartId + 0);
		public static Stage FreeGames { get; } = new Stage(Midas.Presentation.StageHandling.Stages.GameSpecificStartId + 1);
		public static Stage Respin { get; } = new Stage(Midas.Presentation.StageHandling.Stages.GameSpecificStartId + 2);
	}
}