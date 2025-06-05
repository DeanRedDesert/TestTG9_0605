using Midas.Presentation.StageHandling;

namespace Midas.Presentation.SceneManagement
{
	public interface ISceneInitialiser
	{
		bool RemoveAfterFirstInit { get; }
		void SceneInit(Stage currentStage);
	}
}