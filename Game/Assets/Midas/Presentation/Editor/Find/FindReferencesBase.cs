using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core.ExtensionMethods;
using Midas.Presentation.Editor.General;
using Midas.Presentation.ExtensionMethods;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Midas.Presentation.Editor.Find
{
	public abstract class FindReferencesBase : EditorWindow
	{
		private enum PrefabSceneFilterOptions
		{
			All,
			Prefabs,
			Scene
		}

		private bool selectOnClick = true;

		private PrefabSceneFilterOptions prefabSceneFilterOptions = PrefabSceneFilterOptions.All;

		private (Object Object, string Path)[] refs = Array.Empty<(Object GameObject, string Path)>();

		private Vector2 scrollPosition;
		private GUIStyle breadCrumbStyle;
		private int choiceIndex;

		protected abstract IEnumerable<(Object Object, string Path)> Find();
		protected abstract string HelpBoxMessage { get; }
		protected abstract string SearchFieldLabel { get; }
		protected abstract string[] PopupNameList { get; }

		protected string SelectionName = string.Empty;

		protected virtual void DrawSearchField()
		{
			EditorGUILayout.HelpBox(HelpBoxMessage, MessageType.Info);
			using (new EditorGUILayout.HorizontalScope())
			{
				SelectionName = EditorGUILayout.TextField(SearchFieldLabel, SelectionName);
				DrawPopupNameList();
			}
		}

		protected virtual void OnGUI()
		{
			if (breadCrumbStyle == null)
			{
				var style = GUI.skin.FindStyle("GUIEditor.Breadcrumbleft");
				if (style != null)
				{
					breadCrumbStyle = new GUIStyle(style);
				}

				breadCrumbStyle ??= GUI.skin.label;
			}

			var e = Event.current;
			if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Return)
			{
				SearchNow();
				e.Use();
			}

			DrawSearchField();
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Search Now"))
			{
				SearchNow();
			}

			selectOnClick = GUILayout.Toggle(selectOnClick, "Select on click", GUILayout.ExpandWidth(false));
			prefabSceneFilterOptions =
				(PrefabSceneFilterOptions)EditorGUILayout.Popup((int)prefabSceneFilterOptions, Enum.GetNames(typeof(PrefabSceneFilterOptions)), GUILayout.ExpandWidth(false));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.LabelField("");
			EditorGUILayout.LabelField("Results:", EditorStyles.boldLabel);
			EditorGUILayout.LabelField("");

			using (new ScrollViewScope(ref scrollPosition))
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					using (new EditorGUILayout.VerticalScope())
					{
						DrawReferences();
					}

					using (new EditorGUILayout.VerticalScope())
					{
						EditorGUILayout.LabelField($"Full {SearchFieldLabel} Name:");
						foreach (var r in refs.Where(FilterScenePrefabObjects))
						{
							var isPersistent = EditorUtility.IsPersistent(r.Object);
							if (isPersistent && prefabSceneFilterOptions == PrefabSceneFilterOptions.Scene ||
								!isPersistent && prefabSceneFilterOptions == PrefabSceneFilterOptions.Prefabs)
							{
								continue;
							}

							EditorGUILayout.LabelField(r.Path);
						}
					}
				}
			}

			var numFound = refs.Where(FilterScenePrefabObjects).Count();
			if (numFound == 0)
			{
				EditorGUILayout.LabelField("No references found!", EditorStyles.boldLabel);
			}
			else
			{
				EditorGUILayout.LabelField("Found " + numFound + " references!", EditorStyles.boldLabel);
			}
		}

		protected void SearchNow()
		{
			refs = Find().Distinct().ToArray();
		}

		private bool FilterScenePrefabObjects((Object Object, string Path) value)
		{
			var isPersistent = EditorUtility.IsPersistent(value.Object);
			return prefabSceneFilterOptions == PrefabSceneFilterOptions.All ||
				isPersistent && prefabSceneFilterOptions == PrefabSceneFilterOptions.Prefabs ||
				!isPersistent && prefabSceneFilterOptions == PrefabSceneFilterOptions.Scene;
		}

		private void DrawPopupNameList()
		{
			if (PopupNameList.Length > 0)
			{
				choiceIndex = PopupNameList.FindIndex(SelectionName);
				choiceIndex = EditorGUILayout.Popup(choiceIndex, PopupNameList, GUILayout.Width(25));
				if (choiceIndex >= 0)
				{
					SelectionName = PopupNameList[choiceIndex];
					choiceIndex = 0;
				}
			}
		}

		private void DrawReferences()
		{
			EditorGUILayout.LabelField("Game Object:");
			foreach (var r in refs.Where(FilterScenePrefabObjects))
			{
				var isPersistent = EditorUtility.IsPersistent(r.Object);
				breadCrumbStyle.normal.textColor = isPersistent ? Color.cyan : Color.white;

				if (GUILayout.Button(FullObjectPath(r.Object), breadCrumbStyle))
				{
					if (isPersistent)
					{
						var go = r.Object as GameObject ?? (r.Object as MonoBehaviour)?.gameObject;
						Selection.activeObject = go
							? AssetDatabase.LoadMainAssetAtPath(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go))
							: r.Object;
					}
					else
					{
						EditorGUIUtility.PingObject(r.Object);
						if (selectOnClick)
						{
							var go = r.Object as GameObject ?? (r.Object as MonoBehaviour)?.gameObject;
							if (go)
							{
								Selection.activeGameObject = go;
							}
						}
					}
				}
			}
		}

		private static string FullObjectPath(Object o)
		{
			if (!o)
				return "-removed-";

			if (o is MonoBehaviour mb)
				return mb.gameObject.GetPath();

			return AssetDatabase.GetAssetPath(o);
		}
	}
}