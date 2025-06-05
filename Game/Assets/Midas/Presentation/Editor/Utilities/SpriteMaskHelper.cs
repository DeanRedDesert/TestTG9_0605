using UnityEngine;

namespace Midas.Presentation.Editor.Utilities
{
	public static class SpriteMaskHelper
	{
		public static void CreateMask(Vector2 size, Sprite maskSprite, Transform parent, string name, Vector3 localPosition)
		{
			var maskGo = new GameObject();

			maskGo.transform.localScale = size / maskSprite.bounds.size;
			var mask = maskGo.gameObject.AddComponent<SpriteMask>();
			mask.sprite = maskSprite;
			mask.alphaCutoff = 0.2f;
			mask.isCustomRangeActive = false;
			mask.spriteSortPoint = SpriteSortPoint.Center;

			maskGo.transform.SetParent(parent, false);
			maskGo.gameObject.name = name;
			maskGo.transform.localPosition = localPosition;
		}
	}
}