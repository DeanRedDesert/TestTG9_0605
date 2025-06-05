using Midas.Presentation.Data.PropertyReference;
using Midas.Presentation.Editor.Utilities;
using Midas.Tools.Editor;
using UnityEditor;
using UnityEngine;

namespace Midas.Presentation.Editor.Data
{
	[CustomPropertyDrawer(typeof(PropertyPathAttribute))]
	public class PropertyPathPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var statusPropertyPathAttribute = (PropertyPathAttribute)attribute;
			if (!property.serializedObject.TryGetFieldValueAtPath(property.propertyPath.Substring(0, property.propertyPath.LastIndexOf('.')), out var obj))
				return;

			var type = statusPropertyPathAttribute.GetRequiredType(obj);
			var statusItemProperties = PropertyPathResolver.CollectProperties(type);
			if (statusItemProperties == null || statusItemProperties.Count == 0)
				return;

			using (new PropertyScope(ref position, label, property))
			{
				var itemPath = property.stringValue;

				var newItemIndex = PropertyPathHelper.DrawItemSelector(statusItemProperties, position, itemPath);
				if (newItemIndex != -1)
					itemPath = statusItemProperties[newItemIndex].Name;

				if (property.stringValue != itemPath)
				{
					property.stringValue = itemPath;
					property.serializedObject.ApplyModifiedProperties();
				}
			}
		}
	}
}