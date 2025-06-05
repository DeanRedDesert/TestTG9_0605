using System.Collections.Generic;
using UnityEngine;

namespace Midas.Fuel
{
	/// <summary>
	/// Custom behavior to hold surrounding contextual game objects for the translation / object
	/// being screen shot.
	/// </summary>
	[DisallowMultipleComponent]
	public sealed class ScreenshotContext : MonoBehaviour
	{
		/// <summary>
		/// Backing field for <see cref="ContextObjects"/>.
		/// </summary>
		[SerializeField]
		private List<GameObject> contextObjects;

		/// <summary>
		/// Collection of object that are required for the context of screen shots.
		/// </summary>
		public List<GameObject> ContextObjects
		{
			get
			{
				contextObjects ??= new List<GameObject>();
				contextObjects.RemoveAll(obj => obj == null);

				return contextObjects;
			}
		}
	}
}