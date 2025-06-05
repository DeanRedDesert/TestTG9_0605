using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Midas.Core;
using Midas.Core.General;

namespace Midas.Presentation.DevHelpers
{
	public static class PropertyTreeHelper
	{
		private sealed class NodeAndPropertyInfo
		{
			public object Node { get; }
			public PropertyInfo PropertyInfo { get; }

			public NodeAndPropertyInfo(object node, PropertyInfo propertyInfo)
			{
				Node = node;
				PropertyInfo = propertyInfo;
			}
		}

		private sealed class RawEnumNode
		{
			public IEnumerable<NodeAndPropertyInfo> Properties { get; }

			public RawEnumNode(IEnumerable<NodeAndPropertyInfo> properties)
			{
				Properties = properties;
			}
		}

		public static bool HasChildren(object node)
		{
			switch (node)
			{
				case RawEnumNode _:
					return true;

				case NodeAndPropertyInfo v:
					node = v.PropertyInfo.GetValue(v.Node);
					break;
			}

			switch (node)
			{
				case null:
				case string _:
					return false;
				case Money money:
					return money.IsValid;
				case Credit credit:
					return credit.IsValid;
				case RationalNumber rn:
					return rn.IsValid;
			}

			var type = node.GetType();
			return type
				.GetProperties()
				.Except(type.GetDefaultMembers().OfType<PropertyInfo>())
				.Any();
		}

		public static IEnumerable<object> GetChildren(object node)
		{
			switch (node)
			{
				case RawEnumNode v:
					return v.Properties;

				case NodeAndPropertyInfo v:
					node = v.PropertyInfo.GetValue(v.Node);
					if (node == null)
						return Array.Empty<object>();

					break;
			}

			switch (node)
			{
				case IEnumerable<dynamic> v:
					return new object[] { new RawEnumNode(GetNodeAndPropertyInfos(v)) }.Concat(v);

				case IEnumerable v:
					return new object[] { new RawEnumNode(GetNodeAndPropertyInfos(v)) }.Concat(v.Cast<object>());

				default:
					return GetNodeAndPropertyInfos(node);
			}

			IEnumerable<NodeAndPropertyInfo> GetNodeAndPropertyInfos(object n)
			{
				if (n != null)
				{
					var type = n.GetType();
					return type
						.GetProperties()
						.Except(type.GetDefaultMembers().OfType<PropertyInfo>())
						.Select(pi => new NodeAndPropertyInfo(n, pi));
				}

				return Array.Empty<NodeAndPropertyInfo>();
			}
		}

		public static string GetValueAsString(object node, bool showTypes)
		{
			switch (node)
			{
				case RawEnumNode _:
					return "Raw View";

				case NodeAndPropertyInfo v:
					var value = PropertyToDesc(v.PropertyInfo);
					var item = v.PropertyInfo.GetValue(v.Node);

					switch (v.PropertyInfo.GetValue(v.Node))
					{
						case null:
							return $"{value}: (null)";
						case string s:
							return $"{value}: \"{s}\"";
						case IEnumerable e:
							return $"{value} ({e.Cast<object>().Count()})";
						default:
							return value == string.Empty ? item.ToString() : $"{value}: {item}";
					}

				default:
					return null;
			}

			string PropertyToDesc(PropertyInfo propertyInfo)
			{
				var t = propertyInfo.PropertyType;
				return showTypes ? $"{propertyInfo.Name} ({t.ToDescription()})" : $"{propertyInfo.Name}";
			}
		}
	}
}