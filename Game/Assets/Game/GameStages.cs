using Midas.Presentation.StageHandling;

namespace Game
{
	[Stages("Game")]
	public static class GameStages
	{
		public static Stage Default { get; } = new Stage(Midas.Presentation.StageHandling.Stages.GameSpecificStartId);
	}
}