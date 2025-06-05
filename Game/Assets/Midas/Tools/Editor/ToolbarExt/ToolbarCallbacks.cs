// ReSharper disable All - This file is borrowed from https://github.com/marijnz/unity-toolbar-extender

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Midas.Tools.Editor.ToolbarExt
{
	public static class ToolbarCallback
	{
		private static Type toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
		private static Type guiViewType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GUIView");
		private static Type iWindowBackendType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.IWindowBackend");

		private static PropertyInfo windowBackend = guiViewType.GetProperty("windowBackend", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

		private static PropertyInfo viewVisualTree = iWindowBackendType.GetProperty("visualTree", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		private static FieldInfo imguiContainerOnGui = typeof(IMGUIContainer).GetField("m_OnGUIHandler", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

		private static Action forceRepaint;
		private static ScriptableObject currentToolbar;

		/// <summary>
		/// Callback for checking if any connected toolbars require a repaint.
		/// </summary>
		public static Func<bool> CheckDirty;

		/// <summary>
		/// Callback for toolbar OnGUI method.
		/// </summary>
		public static Action OnToolbarGUI;

		/// <summary>
		/// Callback for left toolbar OnGUI method.
		/// </summary>
		public static Action OnToolbarGUILeft;

		/// <summary>
		/// Callback for right toolbar OnGUI method.
		/// </summary>
		public static Action OnToolbarGUIRight;

		static ToolbarCallback()
		{
			EditorApplication.update -= OnUpdate;
			EditorApplication.update += OnUpdate;
		}

		private static void OnUpdate()
		{
			// Relying on the fact that toolbar is ScriptableObject and gets deleted when layout changes
			if (currentToolbar == null)
			{
				// Find toolbar
				var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
				currentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;
				if (currentToolbar != null)
				{
					var windowBackend = ToolbarCallback.windowBackend.GetValue(currentToolbar);

					// Get it's visual tree
					var visualTree = (VisualElement)viewVisualTree.GetValue(windowBackend, null);

					// Get first child which 'happens' to be toolbar IMGUIContainer
					var container = (IMGUIContainer)visualTree[0];
					forceRepaint = container.MarkDirtyRepaint;

					// (Re)attach handler
					var handler = (Action)imguiContainerOnGui.GetValue(container);
					handler -= OnGUI;
					handler += OnGUI;
					imguiContainerOnGui.SetValue(container, handler);
				}
			}

			if (CheckDirty?.Invoke() == true)
				forceRepaint?.Invoke();
		}

		static void OnGUI()
		{
			var handler = OnToolbarGUI;
			if (handler != null) handler();
		}
	}
}