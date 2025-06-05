using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.Presentation.Data;
using UnityEngine;
using StatusDatabase = Midas.Presentation.Data.StatusDatabase;

namespace Midas.Presentation.DevHelpers
{
	public sealed class StatusDatabaseView : TreeViewControl
	{
		private bool showTypes;

		public override void Draw(Rect position)
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			showTypes = GUILayout.Toggle(showTypes, "Display Type Info", ButtonStyle, GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			position = new Rect { x = position.x, y = position.y + 20, width = position.width, height = position.height - 20 };
			base.Draw(position);
		}

		#region Private Methods

		protected override void DrawItem(object item)
		{
			var value = GetValueAsString(item);

#if DEBUG
			if (Application.isPlaying)
			{
				if (item is IStatusProperty statusProperty)
					statusProperty.LogChangedValue = GUILayout.Toggle(statusProperty.LogChangedValue, new GUIContent("", "Enable Logging"), GUILayout.Width(16));
			}
#endif

			GUILayout.Label(value, GetLabelStyle(item));
		}

		private static IEnumerable<object> GetCompoundItems(StatusBlockCompound compound)
		{
			return compound.Properties
				.Select(v => (object)v)
				.Concat(compound.StatusBlocks);
		}

		#endregion

		#region TreeViewControl Overrides

		protected override IEnumerable<object> GetChildren(object node)
		{
			while (true)
			{
				switch (node)
				{
					case null:
						return StatusDatabase.StatusBlocksInstance != null
							? StatusDatabase.StatusBlocksInstance.StatusBlocks.OrderBy(i => i.Name)
							: Enumerable.Empty<object>();
					case StatusBlockCompound v:
						return GetCompoundItems(v);
					case StatusBlock v:
						return v.Properties;
					case IStatusProperty v:
						node = v.Value;
						break;

					default:
						return PropertyTreeHelper.GetChildren(node);
				}
			}
		}

		protected override bool HasChildren(object node)
		{
			while (true)
			{
				switch (node)
				{
					case StatusBlockCompound compound:
						return compound.StatusBlocks.Count > 0 || compound.Properties.Count > 0;
					case StatusBlock i:
						return i.Properties.Count > 0;
					case IStatusProperty v:
						node = v.Value;
						break;
					default:
						return PropertyTreeHelper.HasChildren(node);
				}
			}
		}

		protected override string GetValueAsString(object item)
		{
			var value = PropertyTreeHelper.GetValueAsString(item, showTypes);

			if (value != null)
				return value;

			value = string.Empty;
			while (true)
			{
				switch (item)
				{
					case null:
						return $"{value} (null)";
					case string s:
						return $"{value} \"{s}\"";
					case StatusBlock v:
						return $"{(showTypes ? $"{v.Name}: {v.GetType().ToDescription()}" : v.Name)}";
					case IStatusProperty v:
						item = v.Value;
						value = showTypes ? $"{v.Name}: ({v.GetType().ToDescription()})" : v.Name;
						break;
					default:
						return $"{value}: {item}";
				}
			}
		}

		#endregion
	}
}