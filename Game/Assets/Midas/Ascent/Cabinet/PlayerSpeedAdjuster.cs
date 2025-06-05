using UnityEngine;

namespace Midas.Ascent.Cabinet
{
	/// <summary>
	/// This class controls the player frame rate.
	/// </summary>
	internal sealed class PlayerSpeedAdjuster
	{
		private const int HalfFrameRate = 30;

		/// <summary>
		/// The cached value of the framerate setting.
		/// </summary>
		private int? originalFrameRate;

		/// <summary>
		/// Slows down the player frame rate.
		/// </summary>
		public void SlowDownPlayerSpeed()
		{
			if (Application.isEditor)
				return;

			originalFrameRate ??= Application.targetFrameRate;
			Application.targetFrameRate = HalfFrameRate;
		}

		/// <summary>
		/// Restores the player frame rate to original.
		/// </summary>
		public void RestoreOriginalPlayerSpeed()
		{
			if (!Application.isEditor && originalFrameRate.HasValue)
				Application.targetFrameRate = originalFrameRate.Value;
		}
	}
}