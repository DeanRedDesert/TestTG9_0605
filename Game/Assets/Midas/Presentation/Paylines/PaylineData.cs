using System;
using System.Collections.Generic;
using UnityEngine;

namespace Midas.Presentation.Paylines
{
	/// <summary>
	/// Data that defines a single payline.
	/// </summary>
	[Serializable]
	public class PaylineData
	{
		[SerializeField]
		private string lineName;

		[SerializeField]
		private Mesh mesh;

		[SerializeField]
		private List<Vector3> linePositions;

		/// <summary>
		/// The line number of this payline.
		/// </summary>
		public string LineName => lineName;

		/// <summary>
		/// The mesh that was generated for this payline.
		/// </summary>
		public Mesh Mesh => mesh;

		/// <summary>
		/// The position of the vertices used to generate the mesh.
		/// </summary>
		public IReadOnlyList<Vector3> LinePositions => linePositions;

		public static PaylineData Create(string lineName, Mesh mesh, List<Vector3> pos)
		{
			return new PaylineData
			{
				lineName = lineName,
				mesh = mesh,
				linePositions = pos
			};
		}
	}
}