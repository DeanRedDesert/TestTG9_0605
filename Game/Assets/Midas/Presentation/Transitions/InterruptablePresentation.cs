using UnityEngine;

namespace Midas.Presentation.Transitions
{
	public abstract class InterruptablePresentation : MonoBehaviour
	{
		#region Abstract Methods

		/// <summary>
		/// Returns true when the presentation has completed.
		/// </summary>
		public abstract bool IsFinished { get; }

		/// <summary>
		/// Show the presentation.
		/// </summary>
		public abstract void Show();

		/// <summary>
		/// Interrupt the presentation.
		/// </summary>
		/// <remarks>
		/// The intention is that the presentation returns all the affected elements to the state they were in prior
		/// to the presentation starting. This of course depends on the scenario however.
		/// </remarks>
		public abstract void Interrupt();

		#endregion
	}
}