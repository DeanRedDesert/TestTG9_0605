using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Midas.Tools.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace Midas.Fuel.Editor
{
	[InitializeOnLoad]
	public class TrackedPropertyMetadataDrawer
	{
		private static MethodInfo tableRefFromStringMethod;
		private static Texture errorIcon;

		static TrackedPropertyMetadataDrawer()
		{
			EditorGUIUtilityBridge.BeginProperty += BeginProperty;
		}

		private static TableReference TableReferenceFromString(string value)
		{
			tableRefFromStringMethod ??= typeof(TableReference).GetMethod("TableReferenceFromString", BindingFlags.Static | BindingFlags.NonPublic);
			return (TableReference)tableRefFromStringMethod!.Invoke(null, new object[] { value });
		}

		private static void BeginProperty(Rect rect, SerializedProperty property)
		{
			if (property.propertyType == SerializedPropertyType.ManagedReference)
			{
				if ((property.managedReferenceFullTypename.EndsWith("LocalizedObject") || property.managedReferenceFullTypename.EndsWith("LocalizedSprite")) &&
					property.managedReferenceFieldTypename.EndsWith("LocalizedAssetBase"))
				{
					var tableReference = property.FindPropertyRelative("m_TableReference").FindPropertyRelative("m_TableCollectionName");
					var keyId = property.FindPropertyRelative("m_TableEntryReference").FindPropertyRelative("m_KeyId");
					var tableRef = TableReferenceFromString(tableReference.stringValue);

					var missingLocales = default(List<string>);
					foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
					{
						var table = tableRef.ReferenceType == TableReference.Type.Empty ? null : LocalizationSettings.AssetDatabase.GetTable(tableRef, locale);
						var entry = table?.GetEntry(keyId.longValue);

						if (entry?.MetadataEntries.FirstOrDefault(e => e is TranscriptionMetadata) == null)
						{
							missingLocales ??= new List<string>();
							missingLocales.Add(locale.ToString());
						}
					}

					if (missingLocales != null)
					{
						GUI.backgroundColor = new Color(1f, .5f, .5f);
						errorIcon ??= EditorGUIUtility.IconContent("console.erroricon.sml").image;
						rect.xMin -= errorIcon.width;
						rect.size = new Vector2(errorIcon.width, errorIcon.height);
						GUI.Label(rect, new GUIContent(errorIcon, $"Missing transcriptions for: {string.Join(", ", missingLocales)}"), GUI.skin.label);
					}
					else
						GUI.backgroundColor = Color.green;
				}
			}
		}
	}
}