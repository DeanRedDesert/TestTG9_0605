using UnityEngine;

namespace Midas.Presentation.Reels
{
	/// <summary>
	/// Spin settings for the standard reels.
	/// </summary>
	[CreateAssetMenu(menuName = "Midas/Reels/Reel Spin Settings")]
	public class ReelSpinSettings : ScriptableObject
	{
		/// <summary>
		/// The spin speed, measured in symbols per second. A negative value will make the reels spin backwards.
		/// </summary>
		[Tooltip("The spin speed of the reel, in symbols per second")]
		[Range(-100, 100)]
		public float Speed;

		/// <summary>
		/// The overshoot amount to go past the end of the reel spin, measured in symbols.
		/// </summary>
		[Tooltip("The overshoot distance to spin past the final symbol, in symbols.")]
		[Range(0, 5)]
		public float OvershootDistance;

		/// <summary>
		/// The time it takes to recover after overshoot.
		/// </summary>
		[Tooltip("The time that the reel takes to recover from overshoot, in seconds.")]
		[Range(0, 5)]
		public float RecoveryTime;
	}
}