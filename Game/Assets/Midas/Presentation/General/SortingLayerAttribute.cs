using UnityEngine;

namespace Midas.Presentation.General
{
	/// <summary>
	/// Indicates that an integer value is a sorting layer ID.
	/// </summary>
	public sealed partial class SortingLayerAttribute : PropertyAttribute
	{
		private readonly string displayName;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public SortingLayerAttribute()
			: this("Sorting Layer")
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="displayName">The display name of the sorting layer to use in the editor.</param>
		public SortingLayerAttribute(string displayName)
		{
			this.displayName = displayName;
		}
	}
}