using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Midas.Core;
using Midas.Core.LogicServices;
using Midas.LogicToPresentation.Data;
using UnityEngine;

namespace Midas.Presentation.DevHelpers
{
	public sealed class GameServicesView : TreeViewControl
	{
		private bool showTypes;

		#region Private Methods

		private static bool HasChildServices(CompositeGameService compositeGameService)
		{
			return GetChildServices(compositeGameService).Count > 0;
		}

		private static IReadOnlyList<IGameService> GetChildServices(CompositeGameService compositeGameService)
		{
			var field = compositeGameService.GetType().GetPrivateField("childServices");
			return (IReadOnlyList<IGameService>)field.GetValue(compositeGameService);
		}

		private static object GetServiceValue(IGameService consumer)
		{
			return consumer.GetType().GetPrivateField("presentationValue")?.GetValue(consumer);
		}

		#endregion

		#region TreeViewControl Overrides

		public override void Draw(Rect position)
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			showTypes = GUILayout.Toggle(showTypes, "Display Type Info", ButtonStyle, GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			position = new Rect { x = position.x, y = position.y + 20, width = position.width, height = position.height - 20 };
			base.Draw(position);
		}

		protected override IEnumerable<object> GetChildren(object node)
		{
			while (true)
			{
				switch (node)
				{
					case null:
						return ToNodeData((CompositeGameService)typeof(GameServices).GetField("gameServiceRoot", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null));

					case string _:
						return Array.Empty<object>();

					case CompositeGameService v:
						return GetChildServices(v);

					case IGameService v:
						var t = v.GetType();

						if (t.IsGenericType && !t.IsGenericTypeDefinition && t.GetGenericTypeDefinition() == typeof(GameService<>))
							node = v.GetType().GetPrivateField("presentationValue").GetValue(v);
						else
							return PropertyTreeHelper.GetChildren(node);

						break;

					default:
						return PropertyTreeHelper.GetChildren(node);
				}
			}

			IEnumerable<object> ToNodeData(CompositeGameService v)
			{
				if (v == null)
					yield break;

				var children = (IEnumerable<IGameService>)v.GetType().GetPrivateField("childServices").GetValue(v);
				children = children.OrderBy(c => c.Name);
				var compositeChildren = new List<CompositeGameService>();
				foreach (var child in children)
				{
					if (child is CompositeGameService cgs)
						compositeChildren.Add(cgs);
					else
						yield return child;
				}

				foreach (var cgs in compositeChildren)
					yield return cgs;
			}
		}

		protected override bool HasChildren(object node)
		{
			while (true)
			{
				switch (node)
				{
					case string _:
						return false;

					case IEnumerable _:
						return true;

					case CompositeGameService v:
						return HasChildServices(v);

					case IGameService v:
						var t = v.GetType();

						if (t.IsGenericType && !t.IsGenericTypeDefinition && t.GetGenericTypeDefinition() == typeof(GameService<>))
							return PropertyTreeHelper.HasChildren(v.GetType().GetPrivateField("presentationValue").GetValue(v));

						return PropertyTreeHelper.HasChildren(node);

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
						return $"{value}: (null)";

					case CompositeGameService v:
						return $"{v.Name.Substring(v.Name.IndexOf('.') + 1)}";

					case IGameService v:
						item = GetServiceValue(v);
						var name = v.Name.Substring(v.Name.LastIndexOf('.') + 1);
						value = showTypes ? $"{name} ({item?.GetType().ToDescription() ?? "null"})" : $"{name}";
						break;

					default:
						return value == string.Empty ? item.ToString() : $"{value}: {item}";
				}
			}
		}

		#endregion
	}
}