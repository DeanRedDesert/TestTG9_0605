using UnityEngine;

namespace Midas.Presentation.Paylines
{
	/// <summary>
	/// Base class for a payline.
	/// </summary>
	public abstract class Payline : MonoBehaviour
	{
		/// <summary>
		/// Gets the line name that this payline represents.
		/// </summary>
		public string LineName { get; private set; }

		/// <summary>
		/// Initialise the payline using the provided data.
		/// </summary>
		/// <param name="data">The payline data.</param>
		public void SetPaylineData(PaylineData data)
		{
			LineName = data.LineName;
			ConfigurePayline(data);
		}

		/// <summary>
		/// Configure the payline using the provided data.
		/// </summary>
		/// <param name="data">The data generated for this payline.</param>
		public abstract void ConfigurePayline(PaylineData data);

		/// <summary>
		/// Set the color of the win box.
		/// </summary>
		/// <param name="color">The new color</param>
		public abstract void SetColor(Color color);
	}
}