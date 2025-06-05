using UnityEditor;

namespace Midas.Tools.Editor.AssetOptimizer
{
	/// <summary>
	/// Handles updating various windows if assets change.
	/// </summary>
	internal sealed class AssetOptimizerPostprocessor : AssetPostprocessor
	{
		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			if (AdvancedTextureManagerWindow.IsOpen)
				AdvancedTextureManagerWindow.Instance.HandleAssetsChanged(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
			if (UnusedSpriteManager.IsOpen)
				UnusedSpriteManager.Instance.HandleAssetsChanged(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
		}
	}
}