using Midas.Core.General;
using UnityEngine;

namespace Midas.Presentation.Meters
{
	public abstract class MoneyMeter : MonoBehaviour
	{
		/// <summary>
		/// Set the value of the money meter.
		/// </summary>
		/// <remarks>
		/// The <paramref name="value"/> parameter may have sub-minor currency allowing for a progressive meter to scroll.
		/// </remarks>
		/// <param name="value">The value to set.</param>
		public abstract void SetValue(Money value);
	}
}