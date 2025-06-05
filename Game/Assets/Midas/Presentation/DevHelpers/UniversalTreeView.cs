using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DrawItemFunc = System.Func<object, bool, bool>;
using GetChildrenFunc = System.Func<object, System.Collections.Generic.IEnumerable<object>>;
using FilterFunc = System.Func<object, bool>;

namespace Midas.Presentation.DevHelpers
{
	public sealed class UniversalTreeView
	{
		#region Nested Type: NodeData

		private sealed class NodeData
		{
			#region Public

			public NodeData(bool collapsed, object item, NodeData parent, bool filteredOut)
			{
				IsCollapsed = collapsed;
				FilteredOut = filteredOut;
				Item = item;
				Parent = parent;
			}

			public bool FilteredOut { get; set; }
			public NodeData Parent { get; }
			public bool IsCollapsed { get; set; }
			public object Item { get; }
			public NodeData[] Children { get; set; } = Array.Empty<NodeData>();

			#endregion
		}

		#endregion

		#region Fields

		private readonly NodeData rootItem = new NodeData(false, null, null, false);
		private bool applyFilter;
		private bool filtered;

		#endregion

		#region Properties

		public int MaxFilterDepth { get; set; } = 5;

		#endregion

		#region Public

		public void EnableFilter(bool filter)
		{
			applyFilter = filter;
			filtered = filter;
			if (!filtered)
			{
				ResetFilteredOut(rootItem);
			}
		}

		public void CollapseAll()
		{
			rootItem.Children = rootItem.Children.Select(nodeData => new NodeData(true, nodeData.Item, null, false)).ToArray();
		}

		public void Draw(DrawItemFunc drawItem, GetChildrenFunc getChildren, FilterFunc filter)
		{
			if (applyFilter)
			{
				ApplyFilter(rootItem, getChildren, filter, 0);
				applyFilter = false;
			}
			else if (!filtered)
			{
				UpdateChildren(rootItem, getChildren(null), true);
			}

			using (new GUILayout.VerticalScope())
			{
				DrawRectNodes(rootItem.Children, drawItem, getChildren);
			}
		}

		private static void UpdateChildren(NodeData node, IEnumerable<object> childItems, bool copyOldData)
		{
			var oldChildren = node.Children;
			node.Children = childItems
				.Select(obj => new NodeData(true, obj, node, false))
				.ToArray();

			if (copyOldData && node.Children.Length > 0)
			{
				CopyNodeData(oldChildren, node.Children);
			}
		}

		private void ApplyFilter(NodeData node, GetChildrenFunc getChildren, FilterFunc filter, int recCount)
		{
			if (++recCount < MaxFilterDepth)
			{
				if (filter(node.Item))
				{
					PropagateFilterUp(node);
				}
				else
				{
					node.FilteredOut = true;
				}

				UpdateChildren(node, getChildren(node.Item), false);

				foreach (var child in node.Children)
					ApplyFilter(child, getChildren, filter, recCount);
			}
		}

		private static void ResetFilteredOut(NodeData node)
		{
			node.FilteredOut = false;
			foreach (var child in node.Children)
				ResetFilteredOut(child);
		}

		private static void PropagateFilterUp(NodeData node)
		{
			// self should be collapsed parents not
			var isCollapsed = true;
			while (node != null)
			{
				node.FilteredOut = false;
				node.IsCollapsed = isCollapsed;
				node = node.Parent;

				isCollapsed = false;
			}
		}

		private void DrawRectNodes(NodeData[] nodes, DrawItemFunc drawItem, GetChildrenFunc getChildren)
		{
			foreach (var node in nodes.Where(n => !n.FilteredOut && n.Item != null))
			{
				var updateChildren = !filtered;
				//expand on click
				if (drawItem(node.Item, node.IsCollapsed))
				{
					if (node.IsCollapsed)
					{
						node.IsCollapsed = false;
						updateChildren = true;
						ResetFilteredOut(node);
					}
					else
					{
						node.IsCollapsed = true;
						node.Children = Array.Empty<NodeData>();
					}
				}

				if (!node.IsCollapsed)
				{
					DrawRectChildren(node, drawItem, getChildren, updateChildren);
				}
			}
		}

		private void DrawRectChildren(NodeData node, DrawItemFunc drawItem, GetChildrenFunc getChildren, bool updateChildren)
		{
			if (updateChildren)
			{
				UpdateChildren(node, getChildren(node.Item), true);
			}

			if (node.Children.Length > 0)
			{
				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Label("", GUILayout.Width(16));
					using (new GUILayout.VerticalScope())
					{
						DrawRectNodes(node.Children, drawItem, getChildren);
					}
				}
			}
		}

		private static void CopyNodeData(IReadOnlyList<NodeData> oldChildren, IReadOnlyList<NodeData> newChildren)
		{
			if (oldChildren != null)
			{
				for (var i = 0; i < newChildren.Count; ++i)
				{
					if (oldChildren.Count > i)
					{
						newChildren[i].IsCollapsed = oldChildren[i].IsCollapsed;
						newChildren[i].Children = oldChildren[i].Children;
						newChildren[i].FilteredOut = oldChildren[i].FilteredOut;
					}
				}
			}
		}

		#endregion
	}
}