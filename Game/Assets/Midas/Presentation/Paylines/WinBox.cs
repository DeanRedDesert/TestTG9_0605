using UnityEngine;

namespace Midas.Presentation.Paylines
{
	/// <summary>
	/// Base class of a win box. Provides the data required for win
	/// </summary>
	public abstract class WinBox : MonoBehaviour
	{
		/// <summary>
		/// The cell row of the win box.
		/// </summary>
		public int Row { get; private set; }

		/// <summary>
		/// The cell column of the win box.
		/// </summary>
		public int Column { get; private set; }

		/// <summary>
		/// Initialise the win box using the provided data.
		/// </summary>
		/// <param name="data">The data to set.</param>
		public void SetWinBoxData(WinBoxData data)
		{
			Row = data.Row;
			Column = data.Column;
			ConfigureWinBox(data);
		}

		/// <summary>
		/// Configure the win box using the provided data.
		/// </summary>
		/// <param name="data">The data generated for this win box.</param>
		protected abstract void ConfigureWinBox(WinBoxData data);

		/// <summary>
		/// Set the color of the win box.
		/// </summary>
		/// <param name="color">The new color</param>
		public abstract void SetColor(Color color);
	}
}