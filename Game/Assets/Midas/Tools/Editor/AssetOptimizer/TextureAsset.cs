using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace Midas.Tools.Editor.AssetOptimizer
{
	/// <summary>
	/// Container storing information pertaining to a texture asset in Unity.
	/// </summary>
	public sealed class TextureAsset
	{
		/// <summary>
		/// Reference to the importer pertaining to a texture asset.
		/// </summary>
		public TextureImporter Importer;

		/// <summary>
		/// Actual size of the raw image.
		/// </summary>
		public Vector2 Size = new Vector2(0, 0);

		/// <summary>
		/// Reference to the texture.
		/// </summary>
		public Texture Texture;

		/// <summary>
		/// Do the compression settings cause a warning.
		/// </summary>
		public bool CompressionWarning;

		/// <summary>
		/// Reference to the sprite atlas the asset is referenced in.
		/// </summary>
		public SpriteAtlas SpriteAtlas;
	}
}