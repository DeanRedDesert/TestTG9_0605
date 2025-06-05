using Midas.Presentation.Audio;
using UnityEditor;

namespace Midas.Presentation.Editor.General
{
	[InitializeOnLoad]
	public static class AssetCreator
	{
		#region Public

		public static void CreateAsset<T>(T asset, string path) where T : UnityEngine.Object
		{
			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		#endregion

		#region Private

		static AssetCreator()
		{
			Presentation.General.ScriptableSingleton<SoundDefinitionsDatabase>.Creator = CreateAsset;
		}

		#endregion
	}
}