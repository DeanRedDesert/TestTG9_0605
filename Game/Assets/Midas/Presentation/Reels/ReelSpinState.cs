namespace Midas.Presentation.Reels
{
	/// <summary>
	/// The different states of a reel.
	/// </summary>
	public enum ReelSpinState
	{
		/// <summary>
		/// The reel is idle, not moving.
		/// </summary>
		Idle,

		/// <summary>
		/// The reel is winding up prior to spin.
		/// </summary>
		WindingUp,

		/// <summary>
		/// The reel is currently spinning.
		/// </summary>
		Spinning,

		/// <summary>
		/// The reel has finished spining, and is in the overshoot phase of its bounce.
		/// </summary>
		Overshooting,

		/// <summary>
		/// The reel has finished spinning and is in the recovery phase of its bounce.
		/// </summary>
		Recovering
	}
}