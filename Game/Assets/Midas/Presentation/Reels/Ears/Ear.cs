using Midas.Core;
using UnityEngine;

namespace Midas.Presentation.Reels.Ears
{
	/// <summary>
	/// Provides an interface to manage the ears.
	/// </summary>
	public abstract class Ear : MonoBehaviour
	{
		/// <summary>
		/// Update the ear based upon the current data.
		/// </summary>
		/// <returns>Returns true if the object should be enabled.</returns>
		public abstract bool UpdateEar(IStakeCombination stakeCombination);
	}
}