namespace Midas.Presentation.Game
{
	public enum GameNameStyle
	{
		/// <summary>
		/// If there is more than 1 available denom the game name is formatted with the denom after the game name. Otherwise only the game name is used.
		/// </summary>
		Automatic,

		/// <summary>
		/// The game name is used.
		/// </summary>
		ForceGameNameOnly,

		/// <summary>
		/// The game name is followed by the current denom.
		/// </summary>
		ForceGameNameIncludeDenom
	}
}