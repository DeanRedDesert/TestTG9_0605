using Midas.Presentation.Info;
using UnityEditor;
using UnityEngine;

namespace Midas.Presentation.Editor.Info
{
	[CustomEditor(typeof(PageControllerSettings))]
	public sealed class PageControllerEditor : UnityEditor.Editor
	{
		#region Fields

		private PageControllerSettings settings;
		private GameObject go;
		private GameObject pc;
		private int currentPage;
		private int? lastCurrentPage;
		private PreviewRenderUtility previewRenderUtility;
		private UnityEditor.Editor gameObjectEditor;

		#endregion

		#region Unity Methods

		private void OnEnable()
		{
			settings = target as PageControllerSettings;
			if (settings == null)
				return;

			if (Application.isPlaying)
				return;

			previewRenderUtility ??= new PreviewRenderUtility();
			previewRenderUtility.camera.transform.position = new Vector3(0f, 0f, -10f);
			previewRenderUtility.camera.nearClipPlane = 5f;
			previewRenderUtility.camera.farClipPlane = 20f;
			previewRenderUtility.camera.orthographic = true;
			previewRenderUtility.camera.orthographicSize = 5.4f;
			previewRenderUtility.camera.aspect = 16 / 9f;
			previewRenderUtility.camera.backgroundColor = Color.black;

			go = new GameObject("RulesPreview");
			go.hideFlags = HideFlags.HideAndDontSave;
			go.AddComponent<SpriteRenderer>();

			pc = settings.CreateProgressiveCeiling(go.transform);
			pc.SetActive(false);

			previewRenderUtility.AddSingleGO(go);
		}

		private void OnDisable()
		{
			previewRenderUtility?.Cleanup();
			DestroyImmediate(go);
			DestroyImmediate(pc);

			settings = null;
			previewRenderUtility = null;
			go = null;
			pc = null;
		}

		#endregion

		#region Editor methods

		public override bool HasPreviewGUI() => true;

		public override void OnPreviewGUI(Rect r, GUIStyle background)
		{
			if (settings == null || Application.isPlaying)
				return;

			var folderName = settings.FolderName;
			if (string.IsNullOrEmpty(folderName))
				return;

			var sprites = Resources.LoadAll<Sprite>($"{folderName.Replace(@"\", "/")}");
			if (sprites.Length == 0)
				return;

			GUILayout.BeginHorizontal();
			GUILayout.Label("Display Page: ");
			currentPage = EditorGUILayout.IntSlider(currentPage, 0, sprites.Length - 1);
			GUILayout.EndHorizontal();

			if (lastCurrentPage != currentPage)
			{
				var n = sprites[currentPage].name.Replace($"Assets/{folderName.Replace(@"\", "/")}/", string.Empty).Replace(".png", string.Empty);
				go.GetComponent<SpriteRenderer>().sprite = sprites[currentPage];
				pc.SetActive(settings.IsProgressivePage(n));
				lastCurrentPage = currentPage;
			}

			pc.transform.localPosition = settings.ProgressiveCeilingOffset;
			previewRenderUtility.BeginPreview(r, background);
			previewRenderUtility.Render();
			var t = previewRenderUtility.EndPreview();
			GUI.DrawTexture(r, t);
		}

		public override string GetInfoString()
		{
			if (settings == null)
				return base.GetInfoString();

			var folder = $"{settings.FolderName}";

			if (string.IsNullOrEmpty(folder))
				return "The folder name for the rules pages is null or empty. Please enter a location relative to the Assets folder.";

			var sprites = Resources.LoadAll<Sprite>($"{folder.Replace(@"\", "/")}");

			if (Application.isPlaying)
				return "No preview is possible during play mode.";

			return sprites.Length == 0 ? $"No rules pages found in {folder}" : $"Filename: {sprites[currentPage].name}";
		}

		#endregion

		[MenuItem("Midas/Goto/Game Rules")]
		private static void FocusObject()
		{
			var asset = AssetDatabase.LoadAssetAtPath<PageControllerSettings>("Assets/Game/Information/PageControllerSettings.asset");
			if (asset == null)
			{
				AssetDatabase.CreateAsset(CreateInstance<PageControllerSettings>(), "Assets/Game/Information/PageControllerSettings.asset");
				asset = AssetDatabase.LoadAssetAtPath<PageControllerSettings>("Assets/Game/Information/PageControllerSettings.asset");
			}

			Selection.activeObject = asset;
		}
	}
}