using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Midas.Tools.Editor.AssetOptimizer
{
	/// <summary>
	/// Provides easy view of textures not referenced in the scene, their packing tags, and whether they are
	/// in a resource folder. User can remove packing tags and delete sprites.
	/// </summary>
	public sealed class UnusedSpriteManager : AssetOptimizerEditorWindow
	{
		#region Private

		private bool dataInitialized;
		private int scenesToOpenCount;
		private readonly List<Scene> scenesToOpen = new List<Scene>();
		private Object[] allRootObjects = Array.Empty<Object>();
		private Dictionary<int, string> sceneDictionary;
		private List<Object> dependencyList = new List<Object>();
		private Vector2 scrollRect = Vector2.zero;
		private bool controlKeyDown;
		private bool shiftKeyDown;
		private readonly List<SpriteReference> selectedSpriteReferences = new List<SpriteReference>();
		private List<SpriteReference> unusedSpriteReferences = new List<SpriteReference>();

		#endregion

		#region Public

		/// <summary>
		/// Easy reference to Window.
		/// </summary>
		public static UnusedSpriteManager Instance { get; private set; }

		#endregion

		/// <summary>
		/// Returns whether the window is open or not.
		/// </summary>
		public static bool IsOpen { get { return Instance != null; } }

		/// <summary>
		/// Initializes the internal dependency list with the argument 'objects'.
		/// </summary>
		/// <param name="objects"></param>
		public void Init(ref List<Object> objects)
		{
			dependencyList = objects;
		}

		[MenuItem("Midas/Asset Optimiser/Manage Unused Sprites")]
		private static void Init()
		{
			Instance = (UnusedSpriteManager)GetWindow(typeof(UnusedSpriteManager), false, "Manage Unused Sprites");
			Instance.Show();
		}

		#region AssetOptimizerEditorWindow

		/// <summary>
		/// Refreshes the Unused Sprite References.
		/// </summary>
		protected override void RefreshData()
		{
			unusedSpriteReferences = FindAllUnusedSprites();
		}

		/// <summary>
		/// Handles when any assets are changes by updating internal data and updating dirty flags.
		/// </summary>
		/// <param name="importedAssets"></param>
		/// <param name="deletedAssets"></param>
		/// <param name="movedAssets"></param>
		/// <param name="movedFromAssetPaths"></param>
		public override void HandleAssetsChanged(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			if (importedAssets.Length > 0)
				DataSetDirty = true;
			if (deletedAssets.Length > 0)
				DataSetDirty = true;
			if (movedAssets.Length > 0)
				DataSetDirty = true;
			if (movedFromAssetPaths.Length > 0)
				DataSetDirty = true;

			foreach (var deletedAsset in deletedAssets)
				DeleteSprite(deletedAsset);

			Selection.objects = AssetOptimizerUtilities.GetTexturesAsObjects(selectedSpriteReferences, true);
		}

		#endregion

		#region Unity Methods

		/// <summary>
		/// Tells the windows to redraw if the inspector updates.
		/// </summary>
		private void OnInspectorUpdate()
		{
			Repaint();
		}

		/// <summary>
		/// Draw the window GUI.
		/// </summary>
		private void OnGUI()
		{
			if (GUILayout.Button("Initialize Data (May take a few minutes)"))
				InitializeData();

			if (!DataSetDirty)
				DrawUnusedSprites();
		}

		/// <summary>
		/// Update our selection list when refocusing the window.
		/// </summary>
		private void OnFocus()
		{
			Selection.objects = AssetOptimizerUtilities.GetTexturesAsObjects(selectedSpriteReferences);
		}

		/// <summary>
		/// Update easy access Instance variable.
		/// </summary>
		private void OnEnable()
		{
			Instance = this;
		}

		#endregion

		#region Private

		/// <summary>
		/// Acquires a dependency list for every root object in every scene via opening the scenes.
		/// </summary>
		private void InitializeData()
		{
			// Start a coroutine to load all scenes into the hierarchy and acquire root data.
			EditorSceneManager.sceneOpened -= EditorSceneManagerSceneOpened;
			EditorSceneManager.sceneOpened += EditorSceneManagerSceneOpened;
			scenesToOpenCount = 0;
			allRootObjects = Array.Empty<Object>();
			scenesToOpen.Clear();

			var scenePaths = new List<string>();

			foreach (var entry in SceneDictionary())
			{
				var scene = SceneManager.GetSceneByPath(entry.Value);
				if (scene.isLoaded)
					continue;

				scenesToOpenCount++;
				scenePaths.Add(entry.Value);
			}

			// If all scenes are currently open, then update our root object list.
			if (scenesToOpenCount == 0)
				UpdateRootObjectsAndDependencyList();

			foreach (var sceneToLoad in scenePaths)
				EditorSceneManager.OpenScene(sceneToLoad, OpenSceneMode.Additive);
		}

		/// <summary>
		/// Handler for scene opened event that updates root objects of scenes and dependency list.
		/// </summary>
		/// <param name="scene"></param>
		/// <param name="mode"></param>
		private void EditorSceneManagerSceneOpened(Scene scene, OpenSceneMode mode)
		{
			scenesToOpen.Add(scene);
			scenesToOpenCount--;

			// On the last scene we open, update our root objects and close any scene that was previously not open.
			if (scenesToOpenCount > 0)
				return;

			scenesToOpenCount = 0;
			EditorSceneManager.sceneOpened -= EditorSceneManagerSceneOpened;
			UpdateRootObjectsAndDependencyList();

			// Close newly opened scenes
			foreach (var sceneToClose in scenesToOpen)
				EditorSceneManager.CloseScene(sceneToClose, true);
		}

		/// <summary>
		/// Updates the internal list of root objects and created the dependency list associated with those root objects.
		/// </summary>
		private void UpdateRootObjectsAndDependencyList()
		{
			// TODO. Search the localisation tables for sprites that are used.
			foreach (var entry in SceneDictionary())
			{
				var scene = SceneManager.GetSceneByPath(entry.Value);
				SceneManager.SetActiveScene(scene);
				var roots = SceneManager.GetActiveScene().GetRootGameObjects();
				allRootObjects = allRootObjects.Concat(roots).ToArray();
			}

			// Find all prefab asset paths in the project
			var prefabPaths = AssetDatabase.FindAssets("t:Prefab");
			var preFabRoots = new List<Object>();
			foreach (var prefabPath in prefabPaths)
			{
				var assetPath = AssetDatabase.GUIDToAssetPath(prefabPath);
				var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
				if (prefab != null)
					preFabRoots.Add(prefab);
			}

			allRootObjects = allRootObjects.Concat(preFabRoots).ToArray();

			dependencyList = EditorUtility.CollectDependencies(allRootObjects).ToList();

			dataInitialized = true;
		}

		/// <summary>
		/// Draws the list of unused sprites to the scroll view.
		/// </summary>
		private void DrawUnusedSprites()
		{
			var e = Event.current;
			controlKeyDown = e.control;
			shiftKeyDown = e.shift;

			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal();

			// Do not allow access to the "Managed Unused Sprites" window if our data has not been initialized.
			GUI.enabled = dataInitialized;
			if (GUILayout.Button("Find Unused Sprites"))
				Refresh = true;
			GUI.enabled = true;

			GUILayout.EndHorizontal();

			// Button to remove packing tags from all selected sprites
			if (unusedSpriteReferences.Count > 0)
			{
				GUILayout.BeginHorizontal();
				// Select All Button
				if (GUILayout.Button("Select All"))
				{
					Selection.objects = AssetOptimizerUtilities.GetTexturesAsObjects(unusedSpriteReferences);
					selectedSpriteReferences.Clear();
					foreach (var sprite in unusedSpriteReferences)
						selectedSpriteReferences.Add(sprite);
				}

				if (GUILayout.Button("Delete Selected"))
				{
					var assetPaths = new List<string>();
					selectedSpriteReferences.ForEach(item => assetPaths.Add(item.AssetPath));
					foreach (var path in assetPaths)
						DeleteSprite(path);

					var i = 0;
					foreach (var path in assetPaths)
					{
						EditorUtility.DisplayProgressBar("Deleting Sprites", $"{(float)i / assetPaths.Count * 100:0.}% Complete", (float)i / assetPaths.Count);
						AssetDatabase.DeleteAsset(path);
						i++;
					}

					EditorUtility.ClearProgressBar();

					Selection.objects = Array.Empty<Object>();

					SetDataSourcesDirty();
				}

				GUILayout.EndHorizontal();
			}

			scrollRect = GUILayout.BeginScrollView(scrollRect, false, true);

			for (var i = 0; i < unusedSpriteReferences.Count; i++)
			{
				var unusedSprite = unusedSpriteReferences[i];

				GUILayout.BeginHorizontal();

				GUI.backgroundColor = Selection.objects.Contains(unusedSprite.Sprite.texture) ? Color.black : Color.white;

				var toolTip = unusedSprite.AssetPath;
				// Tooltip content. If asset is in resources, add a disclaimer and update color.
				if (unusedSprite.AssetPath.Contains("/Resources/"))
				{
					toolTip += "\nThis asset is in a Resources folder indicating it may be dynamically called.";
					GUI.contentColor = Color.red;
				}

				if (GUILayout.Button(new GUIContent(unusedSprite.Sprite.name, toolTip), GUILayout.MinWidth(200), GUILayout.MaxWidth(300), GUILayout.Height(20)))
				{
					if (controlKeyDown)
					{
						var listOfSelectedSprites = Selection.objects.ToList();
						if (listOfSelectedSprites.Contains(unusedSprite.Sprite.texture))
						{
							listOfSelectedSprites.Remove(unusedSprite.Sprite.texture);
							selectedSpriteReferences.Remove(unusedSprite);
							if (listOfSelectedSprites.Any())
								Selection.activeObject = listOfSelectedSprites.LastOrDefault();
						}
						else
						{
							listOfSelectedSprites.Add(unusedSprite.Sprite.texture);
							selectedSpriteReferences.Add(unusedSprite);
							Selection.activeObject = unusedSprite.Sprite.texture;
						}

						Selection.objects = listOfSelectedSprites.ToArray();
					}
					else if (shiftKeyDown)
					{
						// Check if sprites have the same packing tag. If they don't, then don't allow shift functionality.
						if (selectedSpriteReferences.Any())
						{
							// Determine lowest between last picked and newly picked.
							var startingIndex = unusedSpriteReferences.IndexOf(selectedSpriteReferences.LastOrDefault());
							var endingIndex = i;
							if (startingIndex > endingIndex)
							{
								endingIndex = startingIndex;
								startingIndex = i;
							}

							for (var index = startingIndex; index <= endingIndex; index++)
								selectedSpriteReferences.Add(unusedSpriteReferences[index]);

							Selection.activeObject = unusedSprite.Sprite.texture;
							Selection.objects =
								AssetOptimizerUtilities.GetTexturesAsObjects(selectedSpriteReferences);
						}
					}
					else
					{
						Selection.objects = Array.Empty<Object>();
						Selection.activeObject = unusedSprite.Sprite.texture;
						selectedSpriteReferences.Clear();
						selectedSpriteReferences.Add(unusedSprite);
					}

					EditorGUIUtility.PingObject(unusedSprite.Sprite.texture);
				} // End of Sprite Button

				GUI.backgroundColor = Color.white;
				GUI.contentColor = Color.white;

				// Remove Tag
				if (GUILayout.Button("Delete Sprite", GUILayout.Height(20), GUILayout.MinWidth(150)))
				{
					selectedSpriteReferences.Remove(unusedSprite);
					Selection.objects = AssetOptimizerUtilities.GetTexturesAsObjects(selectedSpriteReferences);
					AssetDatabase.DeleteAsset(unusedSprite.AssetPath);
					unusedSpriteReferences.Remove(unusedSprite);
					SetDataSourcesDirty();
				}

				GUILayout.EndHorizontal();
			}

			GUILayout.EndScrollView();

			GUILayout.EndVertical();
		}

		/// <summary>
		/// Finds all sprites given a dependency list of all objects in a scene or multiple scenes.
		/// </summary>
		/// <returns></returns>
		private List<SpriteReference> FindAllUnusedSprites()
		{
			var sprites = new List<SpriteReference>();
			// Find all sprites in the scene and record them
			var spriteObjects = dependencyList.Where(item => item != null && !item.Equals(null) && item is Sprite).ToList();

			foreach (var spriteObj in spriteObjects)
			{
				sprites.Add(new SpriteReference
				{
					Sprite = spriteObj as Sprite, Importer = null,
					AssetPath = AssetDatabase.GetAssetPath(spriteObj as Sprite)
				});
			}

			// Find all the Sprites in the project.
			var projectSpriteReferences = FindAllSpritesInProject(false);

			// Determine which Sprite References are common between both lists.
			// We can assume ProjectSpriteReferences is the total set of sprites, therefore, the Sprite Dependency List from our Scene is only a subset of it.
			return projectSpriteReferences.Except(sprites, new SpriteReferenceComparer()).ToList();
		}

		/// <summary>
		/// Finds all sprites within the project and returns a list of them.
		/// </summary>
		/// <param name="excludeUnpacked"></param>
		/// <returns></returns>
		private static List<SpriteReference> FindAllSpritesInProject(bool excludeUnpacked)
		{
			var allTextures = AssetDatabase.FindAssets("t:Texture2D").Select(AssetDatabase.GUIDToAssetPath)
				.Where(t => !t.StartsWith("package", StringComparison.InvariantCultureIgnoreCase) && !t.StartsWith("assets/plugins", StringComparison.InvariantCultureIgnoreCase)).ToArray();
			var allSprites = new List<SpriteReference>();

			foreach (var texturePath in allTextures)
			{
				var sprite = AssetDatabase.LoadAssetAtPath(texturePath, typeof(Sprite)) as Sprite;
				if (sprite == null)
					continue;

				var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
				if (!(excludeUnpacked && importer != null && string.IsNullOrEmpty(importer.spritePackingTag)))
				{
					var reference = new SpriteReference { Sprite = sprite, Importer = importer, AssetPath = texturePath };
					allSprites.Add(reference);
				}
			}

			return allSprites.ToList();
		}

		/// <summary>
		/// Property that acquires all valid scenes and stores them in a dictionary, and returns it.
		/// </summary>
		private Dictionary<int, string> SceneDictionary()
		{
			if (sceneDictionary != null)
				return sceneDictionary;

			sceneDictionary = new Dictionary<int, string>();
			for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
			{
				var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
				if (File.Exists(scenePath))
					sceneDictionary[i] = scenePath;
				else
					Debug.LogWarning($"Failed to load the requested scene specified in the build settings: {scenePath}");
			}

			return sceneDictionary;
		}

		/// <summary>
		/// Informs the Sprite Reference Manager Window that it is dirty.
		/// </summary>
		private void SetDataSourcesDirty()
		{
			DataSetDirty = true;
		}

		/// <summary>
		/// Deletes a sprite from the project.
		/// </summary>
		/// <param name="deletedAsset"></param>
		private void DeleteSprite(string deletedAsset)
		{
			var unusedSpr = unusedSpriteReferences.Find(spr => spr.AssetPath == deletedAsset);
			if (unusedSpr != null)
				unusedSpriteReferences.Remove(unusedSpr);
			var selectedSpr = selectedSpriteReferences.Find(spr => spr.AssetPath == deletedAsset);
			if (selectedSpr != null)
				selectedSpriteReferences.Remove(selectedSpr);
		}

		#endregion

		#region SpriteReferenceComparer

		/// <summary>
		/// Comparison method based on the Asset Path of two sprites.
		/// </summary>
		private sealed class SpriteReferenceComparer : IEqualityComparer<SpriteReference>
		{
			public bool Equals(SpriteReference spr1, SpriteReference spr2)
			{
				if (spr1 == null || spr2 == null)
					return false;

				return spr1.AssetPath == spr2.AssetPath;
			}

			public int GetHashCode(SpriteReference spr) => spr.AssetPath.GetHashCode();
		}

		#endregion
	}
}