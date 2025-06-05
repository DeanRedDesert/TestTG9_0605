namespace Midas.Core
{
	public sealed class DenominationPlayableStatus
	{
		/// <summary>
		/// Gets the Denomination identifier.
		/// </summary>
		public long Denomination { get; }

		/// <summary>
		/// Gets the Game Button Status identifier.
		/// </summary>
		public GameButtonStatus ButtonStatus { get; }

		/// <summary>
		/// Instantiates a new <see cref="DenominationPlayableStatus"/>.
		/// </summary>
		/// <param name="denomination">The denomination identifier.</param>
		/// <param name="gameButtonStatus">The gameButtonStyle identifier.</param>
		public DenominationPlayableStatus(long denomination, GameButtonStatus gameButtonStatus)
		{
			Denomination = denomination;
			ButtonStatus = gameButtonStatus;
		}
	}
}