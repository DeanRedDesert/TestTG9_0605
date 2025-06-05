namespace Midas.LogicToPresentation.Messages
{
	/// <summary>
	/// Start a game.
	/// </summary>
	/// <remarks>
	/// Pres -> Logic: Request for game start.
	/// Logic -> Pres: Indication on whether the request was successful (see WasCanceled).
	/// </remarks>
	public sealed class GameStartMessage : IMessage
	{
		public bool WasCanceled { get; }

		public GameStartMessage(bool wasCanceled = false)
		{
			WasCanceled = wasCanceled;
		}
	}
}