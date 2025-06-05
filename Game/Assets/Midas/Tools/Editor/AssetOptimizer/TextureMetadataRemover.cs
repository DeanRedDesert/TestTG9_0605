using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Midas.Tools.Editor.AssetOptimizer
{
	/// <summary>
	/// Provides easy view of textures not referenced in the scene, their packing tags, and whether they are
	/// in a resource folder. User can remove packing tags and delete sprites.
	/// </summary>
	public sealed class TextureMetadataRemover : AssetOptimizerEditorWindow
	{
		#region Private

		private Vector2 scrollRect = Vector2.zero;
		private bool controlKeyDown;
		private bool shiftKeyDown;
		private readonly List<TextureMetaAsset> selectedSpriteReferences = new List<TextureMetaAsset>();
		private List<TextureMetaAsset> textureMetaAssets = new List<TextureMetaAsset>();
		private int sortBy = 3;
		private bool sortAscending;

		#endregion

		[MenuItem("Midas/Asset Optimiser/Texture Metadata Remover")]
		private static void Init()
		{
			var instance = (TextureMetadataRemover)GetWindow(typeof(TextureMetadataRemover), false, "Texture Metadata Remover");
			instance.Show();
		}

		#region AssetOptimizerEditorWindow

		/// <summary>
		/// Refreshes the Unused Sprite References.
		/// </summary>
		protected override void RefreshData()
		{
			textureMetaAssets = FindTexturesInProjectWithMetaData();
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
			if (GUILayout.Button("Find Textures with Metadata (May take a few minutes)"))
				Refresh = true;

			if (!DataSetDirty)
				DrawTextures();
		}

		/// <summary>
		/// Update our selection list when refocusing the window.
		/// </summary>
		private void OnFocus()
		{
			Selection.objects = selectedSpriteReferences.Select(t => (Object)t.Texture).ToArray();
		}

		#endregion

		#region Private

		/// <summary>
		/// Draws the list of textures that have Metadata.
		/// </summary>
		private void DrawTextures()
		{
			if (textureMetaAssets.Count == 0)
				return;

			var e = Event.current;
			controlKeyDown = e.control;
			shiftKeyDown = e.shift;

			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Remove Meta From All"))
			{
				foreach (var tma in textureMetaAssets)
					PngInspector.RemoveMetadata(tma.AssetPath);
				Refresh = true;
			}

			GUILayout.EndHorizontal();

			EditorGUILayout.LabelField("Filtering Options", EditorStyles.boldLabel);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Sort By: ", GUILayout.Width(150));
			sortBy = EditorGUILayout.Popup(sortBy, new[] { "Name", "Data Size", "Metadata Size", "Meta %" }, GUILayout.Width(100));
			sortAscending = GUILayout.Toggle(sortAscending, "Ascending", GUILayout.Width(100));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Texture", GUILayout.Width(200));
			GUILayout.Label("Total Size (bytes)", GUILayout.Width(100));
			GUILayout.Label("Meta Size (bytes)", GUILayout.Width(100));
			GUILayout.Label("Meta %", GUILayout.Width(100));
			GUILayout.Label(string.Empty, GUILayout.Width(100));
			GUILayout.EndHorizontal();

			scrollRect = GUILayout.BeginScrollView(scrollRect, false, true);

			var sorted = sortAscending ? textureMetaAssets.OrderBy(SortOrder).ToArray() : textureMetaAssets.OrderByDescending(SortOrder).ToArray();

			for (var i = 0; i < sorted.Length; i++)
			{
				var asset = sorted[i];
				GUILayout.BeginHorizontal();

				GUI.backgroundColor = Selection.objects.Contains(asset.Texture) ? Color.black : Color.white;

				if (GUILayout.Button(new GUIContent(asset.Texture.name), GUILayout.Width(200), GUILayout.Height(20)))
				{
					if (controlKeyDown)
					{
						var listOfSelectedSprites = Selection.objects.ToList();
						if (listOfSelectedSprites.Contains(asset.Texture))
						{
							listOfSelectedSprites.Remove(asset.Texture);
							selectedSpriteReferences.Remove(asset);
							if (listOfSelectedSprites.Any())
								Selection.activeObject = listOfSelectedSprites.LastOrDefault();
						}
						else
						{
							listOfSelectedSprites.Add(asset.Texture);
							selectedSpriteReferences.Add(asset);
							Selection.activeObject = asset.Texture;
						}

						Selection.objects = listOfSelectedSprites.ToArray();
					}
					else if (shiftKeyDown)
					{
						if (selectedSpriteReferences.Any())
						{
							// Determine lowest between last picked and newly picked.
							var startingIndex = textureMetaAssets.IndexOf(selectedSpriteReferences.LastOrDefault());
							var endingIndex = i;
							if (startingIndex > endingIndex)
							{
								endingIndex = startingIndex;
								startingIndex = i;
							}

							for (var index = startingIndex; index <= endingIndex; index++)
								selectedSpriteReferences.Add(textureMetaAssets[index]);

							Selection.activeObject = asset.Texture;
							Selection.objects = selectedSpriteReferences.Select(s => (Object)s.Texture).ToArray();
						}
					}
					else
					{
						Selection.objects = Array.Empty<Object>();
						Selection.activeObject = asset.Texture;
						selectedSpriteReferences.Clear();
						selectedSpriteReferences.Add(asset);
					}

					EditorGUIUtility.PingObject(asset.Texture);
				}

				GUI.backgroundColor = Color.white;
				GUI.contentColor = Color.white;

				GUILayout.Label($"{asset.DataSize}", GUILayout.Height(20), GUILayout.Width(100));
				GUILayout.Label($"{asset.MetadataSize}", GUILayout.Height(20), GUILayout.Width(100));
				GUILayout.Label($"{asset.MetadataPercent:P2}", GUILayout.Height(20), GUILayout.Width(100));

				if (GUILayout.Button(new GUIContent("Remove Meta"), GUILayout.Width(100), GUILayout.Height(20)))
				{
					PngInspector.RemoveMetadata(asset.AssetPath);
					Refresh = true;
				}

				if (GUILayout.Button(new GUIContent("Copy Meta To Clipboard "), GUILayout.Width(200), GUILayout.Height(20)))
					GUIUtility.systemCopyBuffer = string.Join(Environment.NewLine, asset.MetaText);

				GUILayout.EndHorizontal();
			}

			GUILayout.EndScrollView();

			GUILayout.EndVertical();
		}

		private static List<TextureMetaAsset> FindTexturesInProjectWithMetaData()
		{
			var allTextures = AssetDatabase.FindAssets("t:Texture2D").Select(AssetDatabase.GUIDToAssetPath)
				.Where(t => !t.StartsWith("package", StringComparison.InvariantCultureIgnoreCase) && !t.StartsWith("assets/plugins", StringComparison.InvariantCultureIgnoreCase)).ToArray();

			var emt = new List<TextureMetaAsset>();
			foreach (var texturePath in allTextures)
			{
				var texture = AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture)) as Texture;
				if (texture == null)
					continue;

				var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
				if (importer == null)
					continue;

				var s = PngInspector.GetMetadataSize(texturePath);
				if (s.MetaSize == 0)
					continue;

				var reference = new TextureMetaAsset
				{
					Texture = texture, MetadataSize = s.MetaSize, DataSize = s.DataSize, MetaText = s.MetaText, AssetPath = texturePath
				};
				emt.Add(reference);
			}

			return emt.ToList();
		}

		private object SortOrder(TextureMetaAsset t)
		{
			return sortBy switch
			{
				1 => t.DataSize,
				2 => t.MetadataSize,
				3 => t.MetadataPercent,
				_ => t.Texture.name.ToLower()
			};
		}

		#endregion

		private sealed class TextureMetaAsset
		{
			public Texture Texture;
			public long DataSize;
			public long MetadataSize;
			public double MetadataPercent => MetadataSize / (DataSize + (double)MetadataSize);
			public string AssetPath;
			public IReadOnlyList<string> MetaText;
		}
	}
}