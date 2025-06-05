using UnityEngine;

namespace Midas.Presentation.Paylines
{
	/// <summary>
	/// Standard implementation of a win box. This version takes the provided mesh and updates the mesh filter inside the GameObject.
	/// </summary>
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(Renderer))]
	public class StandardWinBox : WinBox
	{
		private Material material;

		/// <summary>
		/// A quad used to mask out the centre of the win box.
		/// </summary>
		public Transform MaskQuad;

		private void Awake()
		{
			material = GetComponent<Renderer>().material;
		}

		/// <summary>
		/// Configure the win box using the provided data.
		/// </summary>
		/// <param name="data">The data generated for this win box.</param>
		protected override void ConfigureWinBox(WinBoxData data)
		{
			GetComponent<MeshFilter>().mesh = data.Mesh;
			if (MaskQuad != null)
				MaskQuad.localScale = new Vector3(data.Size.x, data.Size.y, 1);
		}

		public override void SetColor(Color color)
		{
			material.color = color;
		}
	}
}