using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace Midas.Tools
{
	public static class SceneHelper
	{
		public static IReadOnlyList<Scene> GetAllLoadedScenes()
		{
			var scenes = new Scene[SceneManager.sceneCount];

			for (var i = 0; i < SceneManager.sceneCount; i++)
				scenes[i] = SceneManager.GetSceneAt(i);

			return scenes;
		}

		/// <summary>
		/// Note, this is really slow and thus should be used sparingly.
		/// </summary>
		public static IReadOnlyList<T> GetComponentsInAllLoadedScenes<T>(bool includeInactive)
		{
			var result = new List<T>();

			foreach (var scene in GetAllLoadedScenes())
				result.AddRange(scene.GetRootGameObjects().SelectMany(s => s.GetComponentsInChildren<T>(includeInactive)));

			return result;
		}
	}
}