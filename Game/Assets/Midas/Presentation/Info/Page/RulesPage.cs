using UnityEngine;

namespace Midas.Presentation.Info.Page
{
	/// <summary>
	/// Base class all displayed rules pages must use to allow the <see cref="PageController"/> to detect and make control the display of the page.
	/// </summary>
	public abstract class RulesPage : MonoBehaviour
	{
		/// <summary>
		/// Does the page show rules or paytables.
		/// </summary>
		public RulesPageType PageType { get; protected set; } = RulesPageType.Rules;

		/// <summary>
		/// Checks to see if the rules pages should be enabled and visible to the player base on the current game state.
		/// </summary>
		/// <returns>Returns true if the rules page should be visible.</returns>
		public abstract bool CanEnable();
	}
}