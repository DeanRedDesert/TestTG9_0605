using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Midas.Tools.Editor.AssetOptimizer
{
	/// <summary>
	/// Provides functions for obtaining textures out of a list of sprite references.
	/// </summary>
	internal static class AssetOptimizerUtilities
	{
		/// <summary>
		/// Returns a list of objects pertaining to the sprite references.
		/// </summary>
		/// <param name="spriteReferences"></param>
		/// <returns></returns>
		public static Object[] GetTexturesAsObjects(IEnumerable<SpriteReference> spriteReferences)
		{
			return spriteReferences.Select(sprReference => (Object)sprReference.Sprite.texture).ToArray();
		}

		/// <summary>
		/// Returns a list of objects pertaining to the sprite references.
		/// </summary>
		/// <param name="spriteReferences"></param>
		/// <param name="ignoreNullEntries">Specify whether to include null entries or not</param>
		/// <returns></returns>
		public static Object[] GetTexturesAsObjects(IEnumerable<SpriteReference> spriteReferences, bool ignoreNullEntries)
		{
			var textures = new List<Object>();
			foreach (var sprReference in spriteReferences)
			{
				if (sprReference.Sprite != null && sprReference.Sprite.texture != null)
					textures.Add(sprReference.Sprite.texture);
			}

			return textures.ToArray();
		}
	}
}