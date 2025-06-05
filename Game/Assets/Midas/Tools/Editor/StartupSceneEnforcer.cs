using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Midas.Tools.Editor
{
	[InitializeOnLoad]
	public static class StartupSceneEnforcer
	{
		#region Private

		static StartupSceneEnforcer()
		{
			EditorApplication.playModeStateChanged += OnPlayModeChanged;
		}

		private static void OnPlayModeChanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.ExitingEditMode &&
				EditorBuildSettings.scenes.Length > 0 &&
				EditorBuildSettings.scenes.Any(x => x.path == SceneManager.GetActiveScene().path))
			{
				var startUpScene = EditorBuildSettings.scenes[0].path;
				EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(startUpScene);
			}
			else
			{
				EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneManager.GetActiveScene().path);
			}
		}

		#endregion
	}
}