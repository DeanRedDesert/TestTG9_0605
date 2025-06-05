using UnityEditor;

namespace Midas.Tools.Editor.AssetOptimizer
{
	/// <summary>
	/// Provides core functionality shared between all Asset Optimiser Windows.
	/// </summary>
	public class AssetOptimizerEditorWindow : EditorWindow
	{
		/// <summary>
		/// Flag indicating if the data has been made dirty.
		/// </summary>
		protected bool DataSetDirty { get; set; }

		/// <summary>
		/// Flag indicating if the data should be refreshed.
		/// </summary>
		protected bool Refresh { get; set; }

		/// <summary>
		/// Handles refreshing data based on refresh and dirty flags.
		/// </summary>
		public virtual void Update()
		{
			if (!Refresh && !DataSetDirty)
				return;

			RefreshData();
			Refresh = false;
			DataSetDirty = false;
		}

		/// <summary>
		/// Refreshes data that the window uses.
		/// </summary>
		protected virtual void RefreshData()
		{
		}

		/// <summary>
		/// Informs the window that assets gave been updated and the current window is dirty.
		/// </summary>
		/// <param name="importedAssets"></param>
		/// <param name="deletedAssets"></param>
		/// <param name="movedAssets"></param>
		/// <param name="movedFromAssetPaths"></param>
		public virtual void HandleAssetsChanged(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			if (importedAssets.Length > 0)
				DataSetDirty = true;
			if (deletedAssets.Length > 0)
				DataSetDirty = true;
			if (movedAssets.Length > 0)
				DataSetDirty = true;
			if (movedFromAssetPaths.Length > 0)
				DataSetDirty = true;
		}
	}
}