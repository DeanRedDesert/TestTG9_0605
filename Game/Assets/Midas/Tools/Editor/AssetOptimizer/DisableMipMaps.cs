using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Midas.Tools.Editor.AssetOptimizer
{
	public static class DisableMipMaps
	{
		[MenuItem("Midas/Asset Optimiser/Disable All Mip Maps")]
		private static void UpdateGenerateMipMapsFlagForAllTextures()
		{
			var allTextures = AssetDatabase.FindAssets("t:texture").Select(AssetDatabase.GUIDToAssetPath).Where(t => !t.StartsWith("Package")).ToArray();
			var mipMapsEnabledCount = 0;

			var assetIndex = 0;
			foreach (var texture in allTextures)
			{
				var importer = AssetImporter.GetAtPath(texture) as TextureImporter;
				if (importer != null)
				{
					if (importer.mipmapEnabled)
					{
						mipMapsEnabledCount++;
						importer.mipmapEnabled = false;
						importer.SaveAndReimport();
					}
				}
				else
				{
					Debug.Log($"Failed to find importer for texture: {texture}");
				}

				EditorUtility.DisplayProgressBar("Disabling Texture Mip Maps", $"{(float)assetIndex / allTextures.Length * 100:0.}% Complete", (float)assetIndex / allTextures.Length);
				assetIndex++;
			}

			EditorUtility.ClearProgressBar();
			Debug.Log($"Total Textures: {allTextures.Length}, Mip Maps disabled: {mipMapsEnabledCount}");
		}
	}
}