namespace Midas.Core
{
	public sealed class GameButtonBehaviour
	{
		/// <summary>
		/// Gets the Game Button type identifier.
		/// </summary>
		public GameButton ButtonType { get; }

		/// <summary>
		/// Gets the Game Button Status identifier.
		/// </summary>
		public GameButtonStatus ButtonStatus { get; }

		/// <summary>
		/// Instantiates a new <see cref="GameButtonBehaviour"/>.
		/// </summary>
		/// <param name="gameButtonTypeEnum">The gameButtonTypeEnum identifier.</param>
		/// <param name="gameButtonStatus">The gameButtonStatus identifier.</param>
		public GameButtonBehaviour(GameButton gameButtonTypeEnum, GameButtonStatus gameButtonStatus)
		{
			ButtonType = gameButtonTypeEnum;
			ButtonStatus = gameButtonStatus;
		}
	}
}