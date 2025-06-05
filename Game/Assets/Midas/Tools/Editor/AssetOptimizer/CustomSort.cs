namespace Midas.Tools.Editor.AssetOptimizer
{
	/// <summary>
	/// Sorting option to indicate whether a texture should be shown.
	/// </summary>
	public class CustomSort
	{
		/// <summary>
		/// Name of the custom sorting algorithm to use.
		/// </summary>
		public virtual string SortName => "Custom Sort";

		/// <summary>
		/// Returns whether the texture passes the sort check.
		/// </summary>
		/// <param name="textureAsset"></param>
		/// <returns></returns>
		public virtual bool IncludeTexture(TextureAsset textureAsset) => true;
	}

	/// <summary>
	/// Sorting options to only show textures who's size has been compressed by adjusting size.
	/// </summary>
	public sealed class ShowTexturesCompressedViaSize : CustomSort
	{
		/// <summary>
		/// Name of sorting method.
		/// </summary>
		public override string SortName => "Only Show Textures Compressed Via Size";

		/// <summary>
		/// Include texture if its size was forcibly reduces via max size setting.
		/// </summary>
		/// <param name="textureAsset"></param>
		/// <returns></returns>
		public override bool IncludeTexture(TextureAsset textureAsset) =>
			textureAsset.Size.x > textureAsset.Importer.maxTextureSize ||
			textureAsset.Size.y > textureAsset.Importer.maxTextureSize;
	}
}