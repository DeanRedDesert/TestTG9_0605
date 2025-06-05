namespace Midas.Core
{
	public static class GameButtonStatusExtensions
	{
		/// <summary>
		/// A status is considered visible if the status is not hidden.
		/// </summary>
		/// <param name="status"></param>
		/// <returns>True if status is not hidden.</returns>
		public static bool IsVisible(this GameButtonStatus status) => status != GameButtonStatus.Hidden;

		/// <summary>
		/// A status is considered active if the status is active or invalid.
		/// </summary>
		/// <param name="status"></param>
		/// <returns>True if status is active or invalid.</returns>
		public static bool IsActive(this GameButtonStatus status) => status == GameButtonStatus.Active || status == GameButtonStatus.Invalid;

		public static bool IsSoftDisabled(this GameButtonStatus status) => status == GameButtonStatus.SoftDisabled;
		public static bool IsHidden(this GameButtonStatus status) => status == GameButtonStatus.Hidden;
	}

	/// <summary>
	/// Represents the identifiers for game button status that may be requested, as defined in the GameFunctionStatus, v1.0.
	/// </summary>
	public enum GameButtonStatus
	{
		/// <summary>
		/// Game button is active and selectable.
		/// </summary>
		Active,

		/// <summary>
		/// Game button is not selectable but still shown on the select screen.
		/// </summary>
		SoftDisabled,

		/// <summary>
		/// Game button is not selectable and (TBD) hidden from the select screen but it is up to the game.
		/// </summary>
		Hidden,

		/// <summary>
		/// Unknown, invalid or uninitialised status. Game Will follows the original/current status/behavior of the game if requested and it could be ignored depending on the status of the game.
		/// </summary>
		Invalid
	}
}