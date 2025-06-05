using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace Midas.Tools.Editor.AssetOptimizer
{
	/// <summary>
	/// Advanced Texture Manager allows mass modification of textures as well as modifying single textures. Features are of follow:
	/// Easily adjust max size of sprites.
	/// Groups textures by compression and allows raising/lowering compression rate by a step.
	/// Allows user to set texture’s max sizes to their default rate.
	/// Provides a sorted means of viewing textures, their original sizes, current max sizes, compression rates, and modifying them.
	/// </summary>
	public sealed class AdvancedTextureManagerWindow : AssetOptimizerEditorWindow
	{
		/// <summary>
		/// Contains all the sorting actors that will be applied to the textures contained within the project.
		/// </summary>
		private readonly TextureSortContainer textureSortContainer = new TextureSortContainer();

		/// <summary>
		/// Keeps track of the position of the scroll view presenting the sorted view of the texture assets.
		/// </summary>
		private Vector2 scrollRectPosition;

		/// <summary>
		/// Size of the scroll view presenting the sorted view of the texture assets.
		/// </summary>
		private Vector2 scrollViewSize = new Vector2(600, 96);

		/// <summary>
		/// Subset of textureAssets based on sorting options.
		/// </summary>
		private List<TextureAsset> sortedTextures = new List<TextureAsset>();

		/// <summary>
		/// Indicates the last texture within our sorted texture list that is visible in the scroll view.
		/// </summary>
		private int sortedTexturesEndIndex;

		/// <summary>
		/// Indicates the first texture within our sorted texture list that is visible in the scroll view.
		/// </summary>
		private int sortedTexturesStartIndex;

		/// <summary>
		/// List of textures contained in the project.
		/// </summary>
		private List<TextureAsset> textureAssets = new List<TextureAsset>();

		/// <summary>
		/// Provides an easy means of accessing the Window.
		/// </summary>
		public static AdvancedTextureManagerWindow Instance { get; private set; }

		/// <summary>
		/// Returns status of whether the Editor Window is open for the tool.
		/// </summary>
		public static bool IsOpen => Instance != null;

		[MenuItem("Midas/Asset Optimiser/Texture Manager")]
		public static void Init()
		{
			Instance = (AdvancedTextureManagerWindow)GetWindow(typeof(AdvancedTextureManagerWindow), false, "Texture Manager");
			Instance.Show();
		}

		/// <summary>
		/// Refreshes the presentation to reflect any changed made in the data.
		/// </summary>
		protected override void RefreshData()
		{
			textureAssets.Clear();
			textureAssets = FindAllTextures(false);
		}

		#region Unity Methods

		/// <summary>
		/// Handles refreshing the data and keeping the sorted texture list up-to-date.
		/// </summary>
		public override void Update()
		{
			base.Update();
			sortedTextures = textureSortContainer.GetViewableTextures(textureAssets);
		}

		/// <summary>
		/// Draw the window GUI.
		/// </summary>
		private void OnGUI()
		{
			DrawTextures();
		}

		/// <summary>
		/// Refresh the data on focus of window if we have textures.
		/// </summary>
		public void OnFocus()
		{
			Refresh = textureAssets.Count == 0;
		}

		/// <summary>
		/// Sets instance variable on enable.
		/// </summary>
		private void OnEnable()
		{
			Instance = this;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Finds all sprites in the project and returns a list of them.
		/// </summary>
		/// <param name="excludeUnpacked"></param>
		/// <returns></returns>
		private static List<TextureAsset> FindAllTextures(bool excludeUnpacked)
		{
			var allTexturesPaths = AssetDatabase.FindAssets("t:Texture2D").Select(AssetDatabase.GUIDToAssetPath)
				.Where(t => !t.StartsWith("package", StringComparison.InvariantCultureIgnoreCase) && !t.StartsWith("assets/plugins", StringComparison.InvariantCultureIgnoreCase)).ToArray();

			var allSpriteAtlas = AssetDatabase.FindAssets("t:SpriteAtlas").Select(AssetDatabase.GUIDToAssetPath)
				.Where(t => !t.StartsWith("package", StringComparison.InvariantCultureIgnoreCase) && !t.StartsWith("assets/plugins", StringComparison.InvariantCultureIgnoreCase)).ToArray();

			var atlas = new List<(SpriteAtlas, IReadOnlyList<string>)>();
			foreach (var texturePath in allSpriteAtlas)
			{
				var texture = AssetDatabase.LoadAssetAtPath(texturePath, typeof(SpriteAtlas)) as SpriteAtlas;
				var sa = new Sprite[texture.spriteCount];
				texture.GetSprites(sa);
				atlas.Add((texture, sa.Select(s => AssetDatabase.GetAssetPath(s.texture)).ToList()));
			}

			var allTextures = new List<TextureAsset>();
			foreach (var texturePath in allTexturesPaths)
			{
				var texture = AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture)) as Texture;
				if (texture == null)
					continue;
				var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
				if (excludeUnpacked || importer == null)
					continue;

				var compressionWarning = false;
				var methodInfo = importer.GetType().GetMethod("GetImportWarnings", BindingFlags.NonPublic | BindingFlags.Instance);
				if (methodInfo != null)
				{
					var res = (string)methodInfo.Invoke(importer, null);
					compressionWarning = res.StartsWith("Only textures with width/height being multiple of 4 can be compressed to DXT5 format", StringComparison.InvariantCultureIgnoreCase);
				}

				var whMethodInfo = importer.GetType().GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
				if (whMethodInfo != null)
				{
					var args = new object[2];
					whMethodInfo.Invoke(importer, args);
					var size = new Vector2();
					if (float.TryParse(args[0].ToString(), out size.x) && float.TryParse(args[1].ToString(), out size.y))
					{
						var spriteAtlas = atlas.FirstOrDefault(at => at.Item2.Contains(texturePath));
						var reference = new TextureAsset { Texture = texture, Importer = importer, Size = size, CompressionWarning = compressionWarning, SpriteAtlas = spriteAtlas.Item1 };
						allTextures.Add(reference);
					}
				}
			}

			return allTextures.ToList();
		}

		/// <summary>
		/// Draws a list of all the textures along with sorting options and options to change their size and compressions.
		/// </summary>
		private void DrawTextures()
		{
			EditorGUILayout.LabelField("Filtering Options", EditorStyles.boldLabel);

			// Max Size options
			GUILayout.BeginHorizontal();
			GUILayout.Label("Max Size: ", GUILayout.Width(150));
			for (var i = 0; i < textureSortContainer.MaxSizes.Length; i++)
				textureSortContainer.MaxSizeSort[i] = GUILayout.Toggle(textureSortContainer.MaxSizeSort[i], textureSortContainer.MaxSizes[i].ToString());
			GUILayout.EndHorizontal();

			// Compression Sorting options
			GUILayout.BeginHorizontal();
			GUILayout.Label("Compression: ", GUILayout.Width(150));
			for (var i = 0; i < textureSortContainer.CompressionSort.Count; i++)
				textureSortContainer.CompressionSort[i] = GUILayout.Toggle(textureSortContainer.CompressionSort[i], ((TextureImporterCompression)i).ToString());
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Failed Compression: ", GUILayout.Width(150));
			textureSortContainer.ShowOnlyFailedCompression = GUILayout.Toggle(textureSortContainer.ShowOnlyFailedCompression, "");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Custom Sorting: ", GUILayout.Width(150));
			for (var i = 0; i < textureSortContainer.CustomSorts.Count; i++)
				textureSortContainer.CustomSorts[i] = GUILayout.Toggle(textureSortContainer.CustomSorts[i], textureSortContainer.CustomSortMethods[i].SortName);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Sort By: ", GUILayout.Width(150));
			textureSortContainer.SortBy = EditorGUILayout.Popup(textureSortContainer.SortBy, new[] { "Name", "Size", "Sprite Atlas", "Compression", "Max Size" }, GUILayout.Width(100));
			textureSortContainer.SortAscending = GUILayout.Toggle(textureSortContainer.SortAscending, "Ascending", GUILayout.Width(100));
			GUILayout.EndHorizontal();

			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

			// List out all of our texture labels.
			GUILayout.BeginHorizontal();
			GUILayout.Label("Texture", GUILayout.Width(200));
			GUILayout.Label("Image Size", GUILayout.Width(100));
			GUILayout.Label("Sprite Atlas", GUILayout.Width(200));
			GUILayout.Label("Failed Compression", GUILayout.Width(100));
			GUILayout.Label("Compression", GUILayout.Width(100));
			GUILayout.Label("Max Size", GUILayout.Width(100));
			GUILayout.EndHorizontal();

			// Draw all of our Textures now.
			scrollRectPosition = GUILayout.BeginScrollView(scrollRectPosition);

			if (Event.current.type == EventType.Layout)
				DetermineViewableIndices(sortedTextures, ref scrollRectPosition, ref scrollViewSize, out sortedTexturesStartIndex, out sortedTexturesEndIndex);

			// Due to limitations in Unity performance and rendering in Unity Editor Windows,
			// creating objects and moving them when scrolling causes a massive performance hit despite most objects
			// not being visible. Therefore, it's more effective to create one large block. This allows our scroll bar
			// to maintain the correct size and position as we scroll.
			if (sortedTexturesStartIndex > 0)
				GUILayout.Button("Start Blank Area", GUILayout.Height(sortedTexturesStartIndex * 20));

			var maxSizes = textureSortContainer.MaxSizes.Select(item => item.ToString()).ToArray();
			// Draw visible textures
			for (var i = sortedTexturesStartIndex; i < sortedTexturesEndIndex; i++)
			{
				var st = sortedTextures[i];
				GUILayout.BeginHorizontal();

				if (GUILayout.Button(st.Texture.name, GUILayout.Width(200)))
					Selection.activeObject = st.Texture;

				GUILayout.Label(st.Size.x + "x" + st.Size.y, GUILayout.Width(100));

				if (st.SpriteAtlas == null)
				{
					GUILayout.Label(string.Empty, GUILayout.Width(200));

					var oldColor = GUI.contentColor;
					GUI.contentColor = st.CompressionWarning ? Color.red : GUI.contentColor;
					GUILayout.Label(st.CompressionWarning && string.IsNullOrEmpty(st.Importer.spritePackingTag) ? "Yes" : "No", GUILayout.Width(100));
					GUI.contentColor = oldColor;

					var previousCompression = st.Importer.textureCompression;
					st.Importer.textureCompression = (TextureImporterCompression)EditorGUILayout.EnumPopup(st.Importer.textureCompression, GUILayout.Width(100));

					var previousMaxSize = st.Importer.maxTextureSize;
					st.Importer.maxTextureSize = EditorGUILayout.IntPopup(st.Importer.maxTextureSize, maxSizes, textureSortContainer.MaxSizes, GUILayout.Width(100));

					// If we change the max size or texture compression, reset the data set so the rest of our data is correct.
					if (previousCompression != st.Importer.textureCompression || previousMaxSize != st.Importer.maxTextureSize)
					{
						st.Importer.SaveAndReimport();
						Refresh = true;
					}
				}
				else
				{
					if (GUILayout.Button(st.SpriteAtlas?.name, GUILayout.Width(200)))
						Selection.activeObject = st.SpriteAtlas;
					GUILayout.Label("No - Atlas", GUILayout.Width(100));
					GUILayout.Label("No - Atlas", GUILayout.Width(100));
					GUILayout.Label("No - Atlas", GUILayout.Width(100));
				}

				GUILayout.EndHorizontal();
			}

			// Draw Block For Invisible Ending Textures
			if (sortedTexturesEndIndex < sortedTextures.Count)
				GUILayout.Button("Ending Blank Area", GUILayout.Height((sortedTextures.Count - sortedTexturesEndIndex) * 20));

			GUILayout.EndScrollView();

			if (Event.current.type == EventType.Repaint)
				scrollViewSize = GUILayoutUtility.GetLastRect().size;
		}

		/// <summary>
		/// Determines where are viewable portion of the textures are at for a scrollable view.
		/// </summary>
		/// <param name="textureAssets"></param>
		/// <param name="scrollPosition"></param>
		/// <param name="scrollViewSize"></param>
		/// <param name="startIndex"></param>
		/// <param name="endIndex"></param>
		private static void DetermineViewableIndices(IReadOnlyCollection<TextureAsset> textureAssets, ref Vector2 scrollPosition, ref Vector2 scrollViewSize, out int startIndex, out int endIndex)
		{
			startIndex = (int)Math.Floor(scrollPosition.y / 20);
			if (startIndex >= 1)
				startIndex--;
			endIndex = startIndex + (int)Math.Ceiling(scrollViewSize.y / 20);
			endIndex += 2;
			endIndex = Math.Min(endIndex, textureAssets.Count);
		}

		#endregion

		#region TextureSortContainer

		/// <summary>
		/// Container that stores various sort options that are applied to the view of the Texture Assets within the project.
		/// </summary>
		private sealed class TextureSortContainer
		{
			/// <summary>
			/// List containing the available max sizes to sort by.
			/// </summary>
			public readonly int[] MaxSizes = { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };

			/// <summary>
			/// List determining which compressions to show. (Uncompressed, Compressed, CompressedHQ, CompressedLQ)
			/// </summary>
			public readonly List<bool> CompressionSort = new List<bool> { true, true, true, true };

			/// <summary>
			/// Sort by ascending.
			/// </summary>
			public bool SortAscending = true;

			/// <summary>
			/// Sort by selected field.
			/// </summary>
			public int SortBy;

			/// <summary>
			/// Show only the textures that have been selected to be compressed but can't due to texture issues.
			/// </summary>
			public bool ShowOnlyFailedCompression;

			/// <summary>
			/// List of custom sorting methods that will be used for sorting which textures to show.
			/// </summary>
			public readonly List<CustomSort> CustomSortMethods = new List<CustomSort>();

			/// <summary>
			/// List determining which custom sort methods to be applied.
			/// </summary>
			public readonly List<bool> CustomSorts = new List<bool>();

			/// <summary>
			/// List determining whether to show a texture.
			/// </summary>
			public readonly List<bool> MaxSizeSort = new List<bool> { true, true, true, true, true, true, true, true, true };

			/// <summary>
			/// Constructor to initialize Custom Sort Data.
			/// </summary>
			public TextureSortContainer()
			{
				CustomSortMethods.Add(new ShowTexturesCompressedViaSize());
				CustomSorts.Add(false);
			}

			/// <summary>
			/// Returns a subset of the textureAssets list based on sorting criteria.
			/// </summary>
			/// <param name="textureAssets"></param>
			/// <returns></returns>
			public List<TextureAsset> GetViewableTextures(List<TextureAsset> textureAssets)
			{
				var list = textureAssets.Where(IncludeTexture);
				list = SortAscending ? list.OrderBy(ToLower) : list.OrderByDescending(ToLower);
				return list.ToList();

				object ToLower(TextureAsset t)
				{
					return SortBy switch
					{
						1 => t.Size.x * t.Size.y,
						2 => t.SpriteAtlas?.name,
						3 => t.Importer.textureCompression,
						4 => t.Importer.maxTextureSize,
						_ => t.Texture.name.ToLower()
					};
				}
			}

			/// <summary>
			/// Determines if a texture should be included in the sorted list.
			/// </summary>
			/// <param name="textureAsset"></param>
			/// <returns></returns>
			private bool IncludeTexture(TextureAsset textureAsset)
			{
				var includeTextureViaMaxSize = false;
				for (var i = 0; i < MaxSizes.Length; i++)
				{
					if (MaxSizeSort[i] && textureAsset.Importer.maxTextureSize == MaxSizes[i])
						includeTextureViaMaxSize = true;
				}

				var includeTextureViaCompression = false;
				for (var i = 0; i < CompressionSort.Count; i++)
				{
					if (CompressionSort[i] && (int)textureAsset.Importer.textureCompression == i)
						includeTextureViaCompression = true;
				}

				var includeTextureViaCustomSorts = true;
				for (var i = 0; i < CustomSorts.Count; i++)
				{
					if (CustomSorts[i] && !CustomSortMethods[i].IncludeTexture(textureAsset))
						includeTextureViaCustomSorts = false;
				}

				var showOnlyFailedCompression = !ShowOnlyFailedCompression || ShowOnlyFailedCompression && textureAsset.CompressionWarning;

				return includeTextureViaMaxSize && includeTextureViaCompression && includeTextureViaCustomSorts && showOnlyFailedCompression;
			}
		}

		#endregion
	}
}