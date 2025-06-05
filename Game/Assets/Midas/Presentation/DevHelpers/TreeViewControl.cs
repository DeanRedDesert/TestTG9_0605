using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Midas.Presentation.DevHelpers
{
	public abstract class TreeViewControl
	{
		#region Fields

		private string filter = "";
		private readonly UniversalTreeView treeView = new UniversalTreeView();
		private Vector2 scrollPosition = Vector2.zero;
		private GUIStyle buttonStyle;
		private GUIStyle labelStyle;
		private GUIStyle toggleStyle;
		private GUIStyle textFieldStyle;

		#endregion

		#region Properties

		public GUIStyle ButtonStyle
		{
			get => buttonStyle ??= GUI.skin.button;
			set => buttonStyle = new GUIStyle(value) { alignment = TextAnchor.MiddleLeft };
		}

		public GUIStyle LabelStyle
		{
			get => labelStyle ??= GUI.skin.label;
			set => labelStyle = value;
		}

		public GUIStyle ToggleStyle
		{
			get => toggleStyle ??= GUI.skin.toggle;
			set => toggleStyle = value;
		}

		public GUIStyle TextFieldStyle
		{
			get => textFieldStyle ??= GUI.skin.textField;
			set => textFieldStyle = value;
		}

		public int MaxFilterDepth
		{
			set => treeView.MaxFilterDepth = value;
			get => treeView.MaxFilterDepth;
		}

		#endregion

		#region Methods

		public void Refresh()
		{
			treeView?.CollapseAll();
		}

		private bool Filter(object node)
		{
			var str = GetValueAsString(node);
			return str.ToLower().Contains(filter.ToLower());
		}

		private void DrawSearchBar()
		{
			using (new GUILayout.HorizontalScope())
			{
				filter = GUILayout.TextField(filter, TextFieldStyle, GUILayout.MinWidth(150), GUILayout.ExpandWidth(true));
				if (GUILayout.Button("Find", ButtonStyle, GUILayout.ExpandWidth(false)))
				{
					treeView.EnableFilter(true);
				}

				if (filter != string.Empty &&
					GUILayout.Button(new GUIContent("CLR", "Clear Filter"), ButtonStyle, GUILayout.ExpandWidth(false)))
				{
					filter = string.Empty;
					treeView.EnableFilter(false);
				}

				if (GUILayout.Button(new GUIContent("R", "Clear filter and collapse all"), ButtonStyle, GUILayout.ExpandWidth(false)))
				{
					filter = "";
					treeView.EnableFilter(false);
					treeView.CollapseAll();
				}
			}
		}

		#endregion

		#region Virtual Methods

		public virtual void Draw(Rect position)
		{
			if (treeView != null)
			{
				using (new GUILayout.AreaScope(position))
				{
					GUILayout.Space(LabelStyle.lineHeight / 2);
					DrawSearchBar();
					GUILayout.Space(LabelStyle.lineHeight / 2);

					using (var scrollViewScope = new GUILayout.ScrollViewScope(scrollPosition,
								GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
					{
						scrollPosition = scrollViewScope.scrollPosition;
						treeView?.Draw(DrawItem, GetChildren, Filter);
					}
				}
			}
		}

		protected virtual bool DrawItem(object item, bool collapsed)
		{
			using (new GUILayout.HorizontalScope())
			{
				var toggled = false;
				if (HasChildren(item))
				{
					toggled = GUILayout.Button(collapsed ? "\u25B6" : "\u25BC", GetLabelStyle(item), GUILayout.Width(LabelStyle.lineHeight));
				}
				else
				{
					GUILayout.Label("", GUILayout.Width(LabelStyle.lineHeight));
				}

				DrawItem(item);
				return toggled;
			}
		}

		protected virtual void DrawItem(object item)
		{
			var value = GetValueAsString(item);
			GUILayout.Label(value, GetLabelStyle(item));
		}

		protected virtual GUIStyle GetLabelStyle(object item)
		{
			return LabelStyle;
		}

		protected abstract IEnumerable<object> GetChildren(object node);
		protected abstract bool HasChildren(object node);
		protected abstract string GetValueAsString(object item);

		#endregion
	}
}