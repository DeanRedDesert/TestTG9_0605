using Midas.Presentation.Stakes;
using UnityEngine;

namespace Game.GameIdentity.Common
{
	/// <summary>
	/// Base class for the text layer of a bet button.
	/// </summary>
	public abstract class StakeButtonTextBase : MonoBehaviour
	{
		/// <summary>
		/// Run initial configuration of the button.
		/// </summary>
		/// <param name="stakeButtonSpecificData">The specific data to present.</param>
		/// <param name="isEnabled">The initial button enable state.</param>
		/// <returns>true if configured, otherwise false.</returns>
		public abstract bool Configure(IStakeButtonSpecificData stakeButtonSpecificData, bool isEnabled);

		/// <summary>
		/// Set the enabled state of the button text.
		/// </summary>
		/// <param name="enabled">True if the text should be enabled, otherwise false.</param>
		public abstract void UpdateEnabledState(bool enabled);
	}
}