namespace Midas.CreditPlayoff.LogicToPresentation
{
	public enum CreditPlayoffState
	{
		/// <summary>
		/// Credit playoff is disabled, declined or current credits == 0 or > bet.
		/// </summary>
		Unavailable,

		/// <summary>
		/// Credit playoff is available to select.
		/// </summary>
		Available,

		/// <summary>
		/// Credit playoff is idle, player is deciding to play or decline.
		/// </summary>
		Idle,

		/// <summary>
		/// Play has been initiated and we are now committed to credit playoff.
		/// </summary>
		Committed,

		/// <summary>
		/// Credit playoff resulted in a win.
		/// </summary>
		Win,

		/// <summary>
		/// Credit playoff resulted in a loss.
		/// </summary>
		Loss
	}
}