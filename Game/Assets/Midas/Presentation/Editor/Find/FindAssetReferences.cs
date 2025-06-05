using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Midas.Presentation.Editor.Find
{
	public sealed class FindAssetReferences : FindReferencesBase
	{
		private const string Title = "Find Direct Asset Reference in Scene";
		private Object selectedAsset;

		protected override string[] PopupNameList => Array.Empty<string>();
		protected override string HelpBoxMessage => "Find Direct Asset Reference in Scene";
		protected override string SearchFieldLabel => "Asset";

		[MenuItem("Midas/Find/" + Title)]
		public static void ShowWindow()
		{
			var w = GetWindow<FindAssetReferences>("Midas-" + Title);
			if (w != null)
			{
				w.selectedAsset = Selection.activeObject;
			}
		}

		/// <summary>
		/// Finds references to selected asset, and creates a window to display the objects.
		/// </summary>
		[MenuItem("Assets/" + Title, false, 26)]
		public static void FindDirectAssetReferencesInScene()
		{
			var w = GetWindow<FindAssetReferences>("Midas-" + Title);
			if (w != null)
			{
				w.selectedAsset = Selection.activeObject;
				w.SearchNow();
			}
		}

		/// <summary>
		/// Validates if an asset is selectable.
		/// </summary>
		/// <returns></returns>
		[MenuItem("Assets/" + Title, true)]
		public static bool ValidateFindDirectAssetReferencesInScene()
		{
			var obj = Selection.activeObject;

			if (obj != null)
			{
				if (AssetDatabase.Contains(obj))
				{
					var path = AssetDatabase.GetAssetPath(obj);
					return !Directory.Exists(path);
				}
			}

			return false;
		}

		protected override IEnumerable<(Object Object, string Path)> Find()
		{
			var refs = new List<(Object GameObject, string Path)>();
			if (AssetDatabase.Contains(selectedAsset))
			{
				Object[] selectedAssetObjs;
				if (AssetDatabase.IsMainAsset(selectedAsset))
				{
					var path = AssetDatabase.GetAssetPath(selectedAsset);
					selectedAssetObjs = AssetDatabase.LoadAllAssetsAtPath(path);
				}
				else
				{
					selectedAssetObjs = new[] { selectedAsset };
				}

				var referencingGameObjects = FindReferences(selectedAssetObjs);
				refs = referencingGameObjects.Select(g => (g.Item1, g.Item2.ToString())).ToList();
			}

			EditorUtility.ClearProgressBar();
			return refs;
		}

		protected override void DrawSearchField()
		{
			EditorGUILayout.HelpBox(HelpBoxMessage, MessageType.Info);
			using (new EditorGUILayout.HorizontalScope())
			{
				selectedAsset = EditorGUILayout.ObjectField(SearchFieldLabel, selectedAsset, typeof(Object), false);
			}
		}

		/// <summary>
		/// Finds the GameObjects that directly reference the selected asset.
		/// </summary>
		/// <param name="selectedAssetObjs">The selected assets</param>
		private static List<(Object, Object)> FindReferences(Object[] selectedAssetObjs)
		{
			var referencedByObjects = new List<(Object, Object)>();
			var allObjects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => !AssetDatabase.Contains(obj));

			foreach (var gameObject in allObjects)
			{
				if (PrefabUtility.IsPartOfPrefabInstance(gameObject))
				{
					var prefabOfObject = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);

					foreach (var selectedAssetObj in selectedAssetObjs.Where(obj => prefabOfObject == obj))
					{
						(Object, Object) tuple = (gameObject, selectedAssetObj);
						if (!referencedByObjects.Contains(tuple))
						{
							referencedByObjects.Add(tuple);
						}
					}
				}

				var components = gameObject.GetComponents<Component>();

				foreach (var component in components)
				{
					if (component == null)
					{
						continue;
					}

					using var serializedObject = new SerializedObject(component);
					var serializedProperty = serializedObject.GetIterator();

					while (serializedProperty.Next(true))
					{
						if (serializedProperty.propertyType == SerializedPropertyType.ObjectReference)
						{
							foreach (var selectedAssetObj in selectedAssetObjs.Where(obj => serializedProperty.objectReferenceValue == obj))
							{
								(Object, Object) tuple = (gameObject, selectedAssetObj);
								if (!referencedByObjects.Contains(tuple))
								{
									referencedByObjects.Add(tuple);
								}
							}
						}
					}
				}
			}

			return referencedByObjects;
		}
	}
}