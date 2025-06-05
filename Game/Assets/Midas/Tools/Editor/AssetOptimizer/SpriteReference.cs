using UnityEditor;
using UnityEngine;

namespace Midas.Tools.Editor.AssetOptimizer
{
	/// <summary>
	/// Container for Sprites found within a project.
	/// </summary>
	public sealed class SpriteReference
	{
		/// <summary>
		/// Path to the asset.
		/// </summary>
		public string AssetPath;

		/// <summary>
		/// Reference to the importer pertaining to the sprite.
		/// </summary>
		public TextureImporter Importer;

		/// <summary>
		/// Reference to the Sprite
		/// </summary>
		public Sprite Sprite;
	}
}