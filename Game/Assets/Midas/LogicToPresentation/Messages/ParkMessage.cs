namespace Midas.LogicToPresentation.Messages
{
	public sealed class ParkMessage : IMessage
	{
		#region Public

		public ParkMessage(bool park)
		{
			Park = park;
		}

		public bool Park { get; }

		#endregion
	}
}