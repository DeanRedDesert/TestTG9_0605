using System;
using System.Collections.Generic;
using Midas.Presentation.StageHandling;
using UnityEngine;

namespace Midas.Presentation.Reels
{
	public abstract class ReelSpinController : MonoBehaviour
	{
		/// <summary>
		/// Gets whether the spin controller is currently spinning.
		/// </summary>
		public abstract bool IsSpinning { get; }

		/// <summary>
		/// Spin the reels, and stop them at the defined intervals.
		/// </summary>
		/// <param name="spinTime">The overall spin time to target.</param>
		public abstract void SpinReels(Stage stage, IReadOnlyList<ReelData> reelData, TimeSpan spinTime);

		/// <summary>
		/// Abort a spin currently in operation.
		/// </summary>
		public abstract void AbortSpin();

		/// <summary>
		/// Slam all reels.
		/// </summary>
		public abstract void SlamReels(bool immediate);
	}
}