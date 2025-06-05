using UnityEngine;

namespace Midas.Presentation.Paylines
{
	/// <summary>
	/// Standard implementation of a payline. This version takes the provided mesh and updates the mesh filter inside the GameObject.
	/// </summary>
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(Renderer))]
	public class StandardPayline : Payline
	{
		private Material material;

		private void Awake()
		{
			material = GetComponent<Renderer>().material;
		}

		/// <summary>
		/// Configure the payline using the provided data.
		/// </summary>
		/// <param name="data">The data generated for this payline.</param>
		public override void ConfigurePayline(PaylineData data)
		{
			GetComponent<MeshFilter>().mesh = data.Mesh;
		}

		public override void SetColor(Color color)
		{
			material.color = color;
		}
	}
}