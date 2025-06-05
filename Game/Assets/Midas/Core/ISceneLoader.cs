using System;

namespace Midas.Core
{
	public interface ISceneLoader
	{
		#region Public

		void LoadInitialScenesAsync();
		void UnloadScenesAsync();
		void UnloadScenes();

		event Action AllInitialScenesLoaded;
		event Action AllScenesUnloaded;

		#endregion
	}
}