using System;
using UnityEngine;

namespace Midas.Presentation.Paylines
{
	/// <summary>
	/// Data that defines a single win box.
	/// </summary>
	[Serializable]
	public class WinBoxData
	{
		/// <summary>
		/// The cell row of the win box.
		/// </summary>
		public int Row;

		/// <summary>
		/// The cell column of the win box.
		/// </summary>
		public int Column;

		/// <summary>
		/// The mesh that was generated for this win box. Note that this mesh is shared amongst all other win boxes of the same size.
		/// </summary>
		public Mesh Mesh;

		/// <summary>
		/// The position of the win box.
		/// </summary>
		public Vector3 Position;

		/// <summary>
		/// The size of the win box.
		/// </summary>
		public Vector2 Size;
	}
}