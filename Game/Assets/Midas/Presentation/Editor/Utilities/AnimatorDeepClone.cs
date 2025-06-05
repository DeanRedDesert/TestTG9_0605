using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Midas.Presentation.Editor.Utilities
{
	/// <summary>
	/// Used to deep clone an animator controller.
	/// </summary>
	public static class AnimatorDeepClone
	{
		/// <summary>
		/// Checks if the deep clone menu item can be activated.
		/// </summary>
		[MenuItem("Assets/Deep Clone", true)]
		public static bool CanDeepClone()
		{
			return Selection.objects.All(o => o is AnimatorController);
		}

		/// <summary>
		/// Deep clones all selected animator controllers.
		/// </summary>
		[MenuItem("Assets/Deep Clone")]
		public static void CreateDeepClone()
		{
			foreach (var controller in Selection.objects.Cast<AnimatorController>())
			{
				var path = AssetDatabase.GetAssetPath(controller);
				if (!File.Exists(path))
				{
					Debug.LogWarning("Asset is not in the database.");
					return;
				}

				var fileName = Path.GetFileNameWithoutExtension(path);
				var newPath = EditorInputDialog.Show("Animator Deep Clone", "Enter the new animator name", fileName);

				if (newPath != null)
					CreateDeepClone(controller, Path.GetDirectoryName(path), fileName, newPath);
			}
		}

		/// <summary>
		/// Creates a deep clone of an animator controller.
		/// </summary>
		/// <remarks>
		/// The deep clone will attempt to use the same filename as the original controller, but if it cannot then a number suffix will be added
		/// to all generated files to ensure that the filenames do not clash.
		/// </remarks>
		/// <param name="controller">The controller to clone.</param>
		/// <param name="destinationPath">The directory to put the clone in.</param>
		/// <returns>The newly cloned animator controller.</returns>
		public static AnimatorController CreateDeepClone(AnimatorController controller, string destinationPath)
		{
			return CreateDeepClone(controller, destinationPath, null, null);
		}

		/// <summary>
		/// Creates a deep clone of an animator controller.
		/// </summary>
		/// <remarks>
		/// The deep clone will attempt to use the same filename as the original controller, but if it cannot then a number suffix will be added
		/// to all generated files to ensure that the filenames do not clash.
		/// </remarks>
		/// <param name="controller">The controller to clone.</param>
		/// <param name="destinationPath">The directory to put the clone in.</param>
		/// <param name="oldName">The part of the filename to replace.</param>
		/// <param name="newName">What to substitute in to the filename.</param>
		/// <returns>The newly cloned animator controller.</returns>
		public static AnimatorController CreateDeepClone(AnimatorController controller, string destinationPath, string oldName, string newName)
		{
			if (!Directory.Exists(destinationPath))
			{
				Debug.LogError($"Destination folder {destinationPath} does not exist.");
				return null;
			}

			var controllerPath = AssetDatabase.GetAssetPath(controller);
			if (!File.Exists(controllerPath))
			{
				Debug.LogWarning("Asset is not in the database.");
				return null;
			}

			var newControllerPath = GetNewFilePath(controllerPath, destinationPath, oldName, newName);

			AssetDatabase.CopyAsset(controllerPath, newControllerPath);
			controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(newControllerPath);

			foreach (var layer in controller.layers)
			{
				foreach (var state in layer.stateMachine.states)
				{
					var animation = state.state.motion;
					if (!animation)
						continue;

					var animPath = AssetDatabase.GetAssetPath(animation);
					if (!File.Exists(animPath))
					{
						Debug.LogWarning($"Animation for state {state.state.name} has no associated asset.");
						continue;
					}

					var newAnimPath = GetNewFilePath(animPath, destinationPath, oldName, newName);
					AssetDatabase.CopyAsset(animPath, newAnimPath);
					state.state.motion = AssetDatabase.LoadAssetAtPath<Motion>(newAnimPath);
				}
			}

			AssetDatabase.SaveAssets();

			return controller;
		}

		private static string GetNewFilePath(string filePath, string destinationPath, string oldName, string newName)
		{
			var baseFilename = Path.GetFileNameWithoutExtension(filePath);

			if (oldName != null && newName != null)
				baseFilename = baseFilename.Replace(oldName, newName);

			return SafeFilename.GetSafeFilename(destinationPath, baseFilename, Path.GetExtension(filePath));
		}
	}
}