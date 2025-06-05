using UnityEngine;

namespace Midas.Presentation.Paylines
{
	/// <summary>
	/// Settings for payline generation.
	/// </summary>
	[CreateAssetMenu(menuName = "Midas/Paylines/Standard Settings")]
	public sealed class StandardPaylinesSettings : ScriptableObject
	{
		/// <summary>
		/// The left side extension on the payline.
		/// </summary>
		[Tooltip("Extends the length of the left side of the payline.")]
		public float LeftExtension;

		/// <summary>
		/// The right side extension on the payline.
		/// </summary>
		[Tooltip("Extends the length of the right side of the payline.")]
		public float RightExtension;

		/// <summary>
		/// The number of segments generated for each corner.
		/// </summary>
		[Tooltip("The number of segments generated for each corner.")]
		public int CornerSegments = 6;

		/// <summary>
		/// The width of the line.
		/// </summary>
		[Tooltip("The width of the line.")]
		public float LineWidth = 5;

		/// <summary>
		/// The color to place on each vertex.
		/// </summary>
		[Tooltip("The color to place on each vertex.")]
		public Color VertexColor = Color.white;
	}
}