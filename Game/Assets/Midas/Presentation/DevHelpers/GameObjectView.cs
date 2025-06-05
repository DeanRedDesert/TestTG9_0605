using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Midas.Presentation.ExtensionMethods;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Midas.Presentation.DevHelpers
{
	public sealed class GameObjectView : TreeViewControl
	{
		#region Types

		private sealed class GameObjectOfScene
		{
			public GameObjectOfScene(GameObject gameObject)
			{
				GameObject = gameObject;
			}

			public override string ToString()
			{
				return
					$"{GameObject.name} C:{GameObject.transform.childCount} L:{GameObject.layer} A[{(GameObject.activeSelf ? "*" : " ")}] H[{(GameObject.activeInHierarchy ? "*" : " ")}]";
			}

			public GameObject GameObject { get; }
		}

		private sealed class ComponentOfScene
		{
			public ComponentOfScene(Component component)
			{
				Component = component;
			}

			public Component Component { get; }
		}

		private sealed class GameObjectComponents
		{
			public GameObjectComponents(IEnumerable<ComponentOfScene> components)
			{
				Components = components;
			}

			public int Count
			{
				get
				{
					count ??= Components.Count();
					return count.Value;
				}
			}

			public IEnumerable<ComponentOfScene> Components { get; }

			private int? count;
		}

		private class BaseInfo
		{
		}

		private sealed class NodeAndPropertyInfo : BaseInfo
		{
			private readonly object node;
			private readonly PropertyInfo propertyInfo;

			public NodeAndPropertyInfo(object node, PropertyInfo propertyInfo)
			{
				this.node = node;
				this.propertyInfo = propertyInfo;
			}

			public object TryGetValue()
			{
				try
				{
					return propertyInfo.GetValue(node);
				}
				catch (Exception e)
				{
					return e.Message;
				}
			}

			public string Formatted()
			{
				string result;
				try
				{
					var v = propertyInfo.GetValue(node);

					result = v switch
					{
						GameObject g => g.GetPath(),
						Component c => c.GetPath(),
						_ => v == null ? "null" : v.ToString()
					};
				}
				catch (Exception e)
				{
					result = e.Message;
				}

				var name = propertyInfo.Name;
				return result.Contains("\n") ? $"{name} :\n{result}" : $"{name} : {result}";
			}
		}

		private sealed class NodeAndFieldInfo : BaseInfo
		{
			private readonly object node;
			private readonly FieldInfo fieldInfo;

			public NodeAndFieldInfo(object node, FieldInfo fieldInfo)
			{
				this.node = node;
				this.fieldInfo = fieldInfo;
			}

			public object TryGetValue()
			{
				try
				{
					return fieldInfo.GetValue(node);
				}
				catch (Exception e)
				{
					return e.Message;
				}
			}

			public string Formatted()
			{
				var v = fieldInfo.GetValue(node);
				var result = v switch
				{
					GameObject g => g.GetPath(),
					Component c => c.GetPath(),
					_ => v == null ? "null" : v.ToString()
				};

				var name = fieldInfo.Name;
				return result.Contains("\n") ? $"{name} :\n{result}" : $"{name} : {result}";
			}
		}

		private static readonly Type[] usePropertiesFromThisTypes =
		{
			typeof(MeshFilter),
			typeof(MeshRenderer),
			typeof(SpriteRenderer),
			typeof(Transform),
			typeof(MonoBehaviour)
		};

		#endregion

		public GameObjectView()
		{
			MaxFilterDepth = 9;
		}

		protected override IEnumerable<object> GetChildren(object node)
		{
			while (true)
			{
				switch (node)
				{
					case Type _:
						return Array.Empty<object>();
					case GameObject _:
						return Array.Empty<object>();
					case Component _:
						return Array.Empty<object>();
					case GameObjectOfScene gameObjectOfScene:
						return GetChildrenAndComponents(gameObjectOfScene.GameObject);
					case ComponentOfScene componentOfScene:
						return GetNodeAndPropertyAndFieldInfos(componentOfScene.Component);
					case GameObjectComponents components:
						return components.Components;
					case string _:
						return Array.Empty<object>();
					case IEnumerable<dynamic> v:
						return v;
					case IEnumerable v:
						return v.Cast<object>();
					case NodeAndPropertyInfo v:
						node = v.TryGetValue();
						break;
					case NodeAndFieldInfo v:
						node = v.TryGetValue();
						break;
					case null:
						return RootGameObjects();
					default:
						return GetNodeAndPropertyAndFieldInfos(node);
				}
			}
		}

		protected override bool HasChildren(object node)
		{
			while (true)
			{
				switch (node)
				{
					case Type _:
						return false;
					case GameObjectOfScene _:
						return true;
					case ComponentOfScene _:
						return true;
					case GameObject _:
						return false;
					case Component _:
						return false;
					case GameObjectComponents components:
						return components.Count > 0;
					case NodeAndPropertyInfo v:
						node = v.TryGetValue();
						break;
					case NodeAndFieldInfo v:
						node = v.TryGetValue();
						break;
					case string _:
						return false;
					case IEnumerable _:
						return true;
					default:
						return HasNodeAndPropertyOrFieldInfos(node);
				}
			}
		}

		protected override string GetValueAsString(object node)
		{
			var value = string.Empty;
			while (true)
			{
				switch (node)
				{
					case GameObjectOfScene gameObjectOfScene:
						return gameObjectOfScene.ToString();
					case ComponentOfScene componentOfScene:
						return componentOfScene.Component.GetType().ToString();
					case GameObjectComponents components:
						return $"Components {components.Count}";
					case GameObject gameObject:
						return $"{gameObject.GetPath()}";
					case Component component:
						return $"{component.GetPath()}";
					case NodeAndPropertyInfo v:
						return v.Formatted();
					case NodeAndFieldInfo v:
						return v.Formatted();
					case IEnumerable v:
						return $"{value}{v} ({v.Cast<object>().Count()})";
					default:
						return $"{value}{node}";
				}
			}
		}

		private static IEnumerable<Scene> LoadedScenes()
		{
			var i = SceneManager.sceneCount;
			var scenes = new Scene[i];
			for (var j = 0; j < i; j++)
			{
				scenes[j] = SceneManager.GetSceneAt(j);
			}

			return scenes;
		}

		private static IEnumerable<GameObjectOfScene> RootGameObjects()
		{
			var result = new List<GameObjectOfScene>(30);
			foreach (var gameObject in LoadedScenes().SelectMany(s => s.GetRootGameObjects()))
			{
				result.Add(new GameObjectOfScene(gameObject));
			}

			return result;
		}

		private static IEnumerable<ComponentOfScene> GetComponents(GameObject gameObject)
		{
			return gameObject.GetComponents<Component>().Select(c => new ComponentOfScene(c));
		}

		private static IEnumerable<GameObjectOfScene> GetChildren(GameObject gameObject)
		{
			return gameObject.transform.Cast<Transform>().Select(t => new GameObjectOfScene(t.gameObject));
		}

		private static IEnumerable<object> GetChildrenAndComponents(GameObject gameObject)
		{
			return GetChildren(gameObject).Cast<object>().Concat(new[] { new GameObjectComponents(GetComponents(gameObject)) });
		}

		private static IEnumerable<BaseInfo> GetNodeAndPropertyAndFieldInfos(object node)
		{
			return node != null
				? GetNodeAndPropertyInfos(node).Concat(GetNodeAndFieldInfos(node))
				: Array.Empty<BaseInfo>();
		}

		private static bool HasNodeAndPropertyOrFieldInfos(object node)
		{
			if (node != null)
			{
				var type = node.GetType();
				var fieldBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
				var hasFields = type
					.GetFields(fieldBindingFlags)
					.Except(type.GetDefaultMembers().OfType<FieldInfo>())
					.Any(IsInterestingField);

				if (hasFields)
					return true;

				if (usePropertiesFromThisTypes.Any(t => type.IsSubclassOf(t) || t == type))
				{
					var propertyBindingFlags = BindingFlags.Instance | BindingFlags.Public;
					return type
						.GetProperties(propertyBindingFlags)
						.Except(type.GetDefaultMembers().OfType<PropertyInfo>())
						.Any(t => t.CanRead && !t.GetCustomAttributes().Any(a => a is ObsoleteAttribute));
				}
			}

			return false;
		}

		private static IEnumerable<BaseInfo> GetNodeAndPropertyInfos(object node)
		{
			if (node != null)
			{
				var type = node.GetType();
				if (usePropertiesFromThisTypes.Any(t => type.IsSubclassOf(t) || t == type))
				{
					var propertyBindingFlags = BindingFlags.Instance | BindingFlags.Public;
					return type
						.GetProperties(propertyBindingFlags)
						.Except(type.GetDefaultMembers().OfType<PropertyInfo>())
						.Where(t => t.CanRead && !t.GetCustomAttributes().Any(a => a is ObsoleteAttribute))
						.Select(pi => new NodeAndPropertyInfo(node, pi));
				}
			}

			return Array.Empty<NodeAndPropertyInfo>();
		}

		private static IEnumerable<BaseInfo> GetNodeAndFieldInfos(object node)
		{
			if (node != null)
			{
				var type = node.GetType();
				var fieldBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
				return type
					.GetFields(fieldBindingFlags)
					.Except(type.GetDefaultMembers().OfType<FieldInfo>())
					.Where(IsInterestingField)
					.Select(fi => new NodeAndFieldInfo(node, fi));
			}

			return Array.Empty<NodeAndFieldInfo>();
		}

		private static bool IsInterestingField(FieldInfo t)
		{
			var customAttributes = t.GetCustomAttributes().ToArray();
			if (t.IsPublic || customAttributes.Any(a => a is SerializeField))
			{
				return customAttributes.Any(a => a is ObsoleteAttribute);
			}

			return false;
		}
	}
}