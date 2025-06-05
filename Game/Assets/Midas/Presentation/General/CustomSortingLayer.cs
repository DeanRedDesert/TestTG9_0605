using System;
using UnityEngine;

namespace Midas.Presentation.General
{
	/// <summary>
	/// Gives the ability to provide sorting layer information for something that is not a renderer, for example an object that instantiates
	/// renderers but requires layer information to be present rather than using the Default layer.
	/// </summary>
	[Serializable]
	public sealed class CustomSortingLayer
	{
		[SortingLayer]
		[SerializeField]
		private int sortingLayerId;

		[SerializeField]
		private int orderInLayer;

		public CustomSortingLayer(int sortingLayerId, int orderInLayer)
		{
			this.orderInLayer = sortingLayerId;
			this.orderInLayer = orderInLayer;
		}

		/// <summary>
		/// Apply the sorting information to a renderer
		/// </summary>
		public void Apply(Renderer renderer)
		{
			renderer.sortingLayerID = sortingLayerId;
			renderer.sortingOrder = orderInLayer;
		}
	}
}