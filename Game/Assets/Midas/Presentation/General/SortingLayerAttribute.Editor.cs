#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using Midas.Core.ExtensionMethods;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Midas.Presentation.General
{
	public sealed partial class SortingLayerAttribute
	{
		/// <summary>
		/// Draws GUI components for the sorting layer property.
		/// </summary>
		[CustomPropertyDrawer(typeof(SortingLayerAttribute))]
		public class SortingLayerDrawer : PropertyDrawer
		{
			private static Object GetTagManager()
			{
				var prop = typeof(EditorApplication).GetProperty("tagManager", BindingFlags.Static | BindingFlags.NonPublic);

				if (prop != null)
					return (Object)prop.GetValue(null, Array.Empty<object>());

				return null;
			}

			private static void EditSortingLayers()
			{
				var tagManager = GetTagManager();
				var field = tagManager.GetType().GetField("m_DefaultExpandedFoldout", BindingFlags.Instance | BindingFlags.Public);
				if (field != null)
					field.SetValue(tagManager, "SortingLayers");
				Selection.activeObject = tagManager;
			}

			/// <summary>
			/// Draw the property inside the given Rect.
			/// </summary>
			/// <param name="position">The position of the GUI element.</param>
			/// <param name="property">The property being edited.</param>
			/// <param name="label">The label to show.</param>
			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				if (property.propertyType != SerializedPropertyType.Integer)
					Debug.LogError("Property must be of integer type: " + property.name);

				var propertyAttribute = (SortingLayerAttribute)attribute;

				EditorGUI.BeginProperty(position, label, property);

				var index = SortingLayer.layers.FindIndex(layer => layer.id == property.intValue);

				var content = SortingLayer.layers.Select(l => new GUIContent(l.name)).ToList();
				content.Add(new GUIContent(""));
				var addLayerIndex = content.Count;
				content.Add(new GUIContent("Add Sorting Layer..."));

				if (index == -1)
				{
					content.Add(new GUIContent("<unknown layer>"));
					index = content.Count - 1;
				}

				var selectedIndex = EditorGUI.Popup(position, new GUIContent(propertyAttribute.displayName), index, content.ToArray());
				if (selectedIndex != index)
				{
					if (selectedIndex < SortingLayer.layers.Length)
						property.intValue = SortingLayer.layers[selectedIndex].id;
					else if (selectedIndex == addLayerIndex)
						EditSortingLayers();
				}

				EditorGUI.EndProperty();
			}
		}
	}
}
#endif