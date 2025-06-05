using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Midas.Presentation.ExtensionMethods
{
	/// <summary>
	/// Extension methods for the GameObject class.
	/// </summary>
	public static class GameObjectExtensionMethods
	{
		public static bool IsPrefab(this GameObject gameObject)
		{
			return gameObject.scene.name == null;
		}

		public static string GetPath(this GameObject gameObject, string separator = "/")
		{
			return string.Join(separator,
				gameObject.GetComponentsInParent<Transform>(true).Select(t => t.name).Reverse());
		}

		/// <summary>
		/// If a game object has a parent called "Stages", returns the name of the child of "Stages" that the object lives in, otherwise "None".
		/// </summary>
		public static string GetOwningStageName(this GameObject gameObject)
		{
			var names = gameObject.GetComponentsInParent<Transform>(true).Select(t => t.name).Reverse().ToArray();

			for (var i = 0; i < names.Length; i++)
			{
				var name = names[i];

				if (name.ToLowerInvariant() == "stages")
					return i == names.Length - 1 ? "None" : names[i + 1];
			}

			return "None";
		}

		public static void InstantiateOrActivateChildContent(this GameObject gameObject, IReadOnlyList<GameObject> content)
		{
			for (var i = 0; i < content.Count; i++)
			{
				var c = content[i];

				if (c.IsPrefab())
					c = Object.Instantiate(c, gameObject.transform, false);
				else if (!c.GetComponentsInParent<Transform>().Contains(gameObject.transform))
					throw new InvalidOperationException($"object {c.GetPath()} is not a child of {gameObject.GetPath()}");

				c.SetActive(true);
			}
		}

		/// <summary>
		/// Instantiate a prefab as the child of the given parent GameObject object. This also sets the local position to 0, 0, 0
		/// </summary>
		/// <typeparam name="T">The component prefab type to instantiate.</typeparam>
		/// <param name="parent">The parent to assign the new GameObject to.</param>
		/// <param name="prefab">The prefab to instantiate.</param>
		/// <returns>The new GameObject setup and ready to go.</returns>
		public static T InstantiatePreFabAsChild<T>(this GameObject parent, T prefab) where T : Component
		{
			return parent.InstantiatePreFabAsChild(prefab, Vector3.zero);
		}

		/// <summary>
		/// Instantiate a prefab as the child of the given parent GameObject object.
		/// </summary>
		/// <typeparam name="T">The component prefab type to instantiate.</typeparam>
		/// <param name="parent">The parent to assign the new GameObject to.</param>
		/// <param name="prefab">The prefab to instantiate.</param>
		/// <param name="localPosition">The local position relative to the new parent.</param>
		/// <returns>The new GameObject setup and ready to go.</returns>
		public static T InstantiatePreFabAsChild<T>(this GameObject parent, T prefab, Vector3 localPosition) where T : Component
		{
			var prefabInstance = Object.Instantiate(prefab, parent.transform, false);
			prefabInstance.transform.localPosition = localPosition;
			return prefabInstance;
		}

		/// <summary>
		/// Instantiate a prefab as the child of the given parent GameObject object. This also sets the local position to 0, 0, 0
		/// </summary>
		/// <param name="parent">The parent to assign the new GameObject to.</param>
		/// <param name="prefab">The prefab to instantiate.</param>
		/// <returns>The new GameObject setup and ready to go.</returns>
		public static GameObject InstantiatePreFabAsChild(this GameObject parent, GameObject prefab)
		{
			return parent.InstantiatePreFabAsChild(prefab, Vector3.zero);
		}

		/// <summary>
		/// Instantiate a prefab as the child of the given parent GameObject object.
		/// </summary>
		/// <param name="parent">The parent to assign the new GameObject to.</param>
		/// <param name="prefab">The prefab to instantiate.</param>
		/// <param name="localPosition">The local position relative to the new parent.</param>
		/// <returns>The new GameObject setup and ready to go.</returns>
		public static GameObject InstantiatePreFabAsChild(this GameObject parent, GameObject prefab, Vector3 localPosition)
		{
			var prefabInstance = Object.Instantiate(prefab, parent.transform, false);
			prefabInstance.transform.localPosition = localPosition;
			return prefabInstance;
		}

		/// <summary>
		/// Sets the game object layer on the object and all of its children recursively.
		/// </summary>
		/// <param name="gameObject">Game Object to set layer on.</param>
		/// <param name="layer">New layer for game object and its children.</param>
		public static void SetLayerRecursively(this GameObject gameObject, int layer)
		{
			gameObject.layer = layer;

			for (var childIndex = 0; childIndex < gameObject.transform.childCount; childIndex++)
			{
				gameObject.transform.GetChild(childIndex).gameObject.SetLayerRecursively(layer);
			}
		}

		/// <summary>
		/// Find a parent game object with the type specified.
		/// </summary>
		/// <param name="gameObject">Game Object to set find parent of.</param>
		public static T GetParentOfType<T>(this GameObject gameObject) where T : Component
		{
			var currentParent = gameObject.transform.parent;
			T foundParent = null;

			while (foundParent == null && currentParent != null)
			{
				foundParent = currentParent.GetComponent<T>();
				currentParent = currentParent.parent;
			}

			return foundParent;
		}

		/// <summary>
		/// Gets whether or not a component is attached to a Game Object
		/// </summary>
		/// <param name="gameObject">Game Object that may have the component attached.</param>
		/// <returns></returns>
		public static bool IsComponentAttached<T>(this GameObject gameObject) where T : Component
		{
			return gameObject.GetComponent<T>() != null;
		}

		/// <summary>
		/// Get an interface implementation from a component.
		/// </summary>
		/// <typeparam name="T">The type to get.</typeparam>
		/// <param name="gameObject">The game object to find the component in.</param>
		/// <returns>The first found component of type T, otherwise null.</returns>
		public static T GetInterfaceComponent<T>(this GameObject gameObject) where T : class
		{
			return gameObject.GetComponent(typeof(T)) as T;
		}

		/// <summary>
		/// Recursively set hide flags on a game object.
		/// </summary>
		/// <param name="gameObject">The game object to set hide flags on.</param>
		/// <param name="hideFlags">The new hide flags value.</param>
		public static void SetHideFlagsRecursively(this GameObject gameObject, HideFlags hideFlags)
		{
			gameObject.hideFlags = hideFlags;
			for (var childIndex = 0; childIndex < gameObject.transform.childCount; childIndex++)
			{
				gameObject.transform.GetChild(childIndex).gameObject.SetHideFlagsRecursively(hideFlags);
			}
		}
	}
}