using System;
using System.Collections.Generic;
using UnityEngine;

namespace Midas.Tools.Editor.AssetOptimizer
{
	/// <summary>
	/// Assists in the presentation of Sprites for the Sprite Reference Manager.
	/// </summary>
	public sealed class SpriteReferenceGuiManager
	{
		// Size is based on Unity unit.
		private const float AtlasHeight = 20;

		/// <summary>
		/// List of all Atlases in the project.
		/// </summary>
		public readonly List<AtlasContainer> AtlasContainer = new List<AtlasContainer>();

		/// <summary>
		/// Indicates the height that a sprite will take.
		/// </summary>
		public static float SpriteHeight => 20;

		/// <summary>
		/// Initializes Atlas containers.
		/// </summary>
		/// <param name="spriteReferences"></param>
		/// <param name="atlasFoldouts"></param>
		public SpriteReferenceGuiManager(SortedDictionary<string, List<SpriteReference>> spriteReferences, ref List<bool> atlasFoldouts)
		{
			var count = 0;
			foreach (var entry in spriteReferences)
			{
				AtlasContainer.Add(new AtlasContainer(AtlasHeight, SpriteHeight, entry.Value.Count, atlasFoldouts[count]));
				count++;
			}
		}

		/// <summary>
		/// Determines the visible elements within the scroll view.
		/// </summary>
		/// <param name="scrollPosition"></param>
		/// <param name="windowSize"></param>
		public void DetermineVisibleElements(ref Vector2 scrollPosition, Vector2 windowSize)
		{
			// Find starting atlas that is within view.
			var end = 0.0f;

			var viewStart = scrollPosition.y;
			var viewEnd = viewStart + windowSize.y + 60;

			for (var i = 0; i < AtlasContainer.Count; i++)
			{
				var start = end + 5;
				end = start + AtlasContainer[i].Height;

				// If the start or ending of our Atlas exists within the viewing size, then it is visible to some extent.
				if (viewStart >= start && viewStart <= end || viewEnd >= start && viewEnd <= end || start >= viewStart && end <= viewEnd)
				{
					AtlasContainer[i].AtlasVisible = true;
					AtlasContainer[i].ViewableStart = viewStart > start ? viewStart - start : 0;
					AtlasContainer[i].ViewableEnd = viewEnd > end ? AtlasContainer[i].Height : Math.Min(viewEnd, AtlasContainer[i].Height);
				}
				else
				{
					AtlasContainer[i].AtlasVisible = false;
				}
			}
		}
	}

	/// <summary>
	/// Container for the Atlas presentation dealing with sizing of various parts of the atlas.
	/// </summary>
	public sealed class AtlasContainer
	{
		private readonly int spriteCount;
		private readonly float atlasHeight;
		private readonly bool isExpanded;
		private readonly float spriteHeight;

		/// <summary>
		/// Indicates whether the Atlas is visible in the Scroll View.
		/// </summary>
		public bool AtlasVisible { get; set; } = true;

		/// <summary>
		/// The start offset indicating what part of the list of Sprites is currently visible within the scroll view.
		/// </summary>
		public float ViewableStart { get; set; }

		/// <summary>
		/// The end offset indicating what part of the list of Sprites is currently visible within the scroll view.
		/// </summary>
		public float ViewableEnd { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="atlasSize"></param>
		/// <param name="spriteSize"></param>
		/// <param name="spriteCount"></param>
		/// <param name="expanded"></param>
		public AtlasContainer(float atlasSize, float spriteSize, int spriteCount, bool expanded)
		{
			this.spriteCount = spriteCount;
			this.isExpanded = expanded;
			spriteHeight = spriteSize;
			atlasHeight = atlasSize;
		}

		/// <summary>
		/// Gets the total height of the atlas, including sprites within that atlas if expanded.
		/// </summary>
		public float Height => atlasHeight + (isExpanded ? spriteCount * spriteHeight : 0);

		/// <summary>
		/// Returns how much of the start of the Atlas height is not being shown in blocks.
		/// </summary>
		public int InvisibleStartingBlocks => ViewableStart <= spriteHeight ? 0 : (int)Mathf.Floor(ViewableStart / spriteHeight);

		/// <summary>
		/// Returns how much of the end of the Atlas height is not being shown in blocks.
		/// </summary>
		public int InvisibleEndingBlocks => (int)Mathf.Floor((Height - ViewableEnd) / spriteHeight);

		/// <summary>
		/// Returns how much of the beginning of the atlas height is now being shown in units.
		/// </summary>
		public float InvisibleStartingHeight(int paddingSize) => InvisibleStartingBlocks * (spriteHeight + paddingSize);
	}
}