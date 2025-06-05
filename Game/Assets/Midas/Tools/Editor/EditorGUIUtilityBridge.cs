using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Midas.Tools.Editor
{
	public class EditorGUIUtilityBridge
	{
		public static event Action<Rect, SerializedProperty> BeginProperty;

		static EditorGUIUtilityBridge()
		{
			var t =  typeof(EditorGUIUtility);
			var ev = t.GetEvent("beginProperty", BindingFlags.Static | BindingFlags.NonPublic);
			var del = Delegate.CreateDelegate(typeof(Action<Rect, SerializedProperty>), typeof(EditorGUIUtilityBridge).GetMethod(nameof(RaiseBeginProperty), BindingFlags.NonPublic | BindingFlags.Static));
			ev.GetAddMethod(true).Invoke(null, new object[] { del });
		}

		private static void RaiseBeginProperty(Rect rect, SerializedProperty property)
		{
			BeginProperty?.Invoke(rect, property);
		}
	}
}