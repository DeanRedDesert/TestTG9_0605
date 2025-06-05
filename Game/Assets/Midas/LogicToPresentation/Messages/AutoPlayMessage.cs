namespace Midas.LogicToPresentation.Messages
{
	public enum AutoPlayMode
	{
		Start,
		StartCancelled,
		Stop
	}

	public enum AutoPlayRequestSource
	{
		/// <summary>
		/// The player asked us to change autoplay state
		/// </summary>
		Player,

		/// <summary>
		/// The foundation asked us to change autoplay state
		/// </summary>
		Foundation,

		/// <summary>
		/// The game flow caused us to change autoplay state (ie, free game trigger)
		/// </summary>
		GameFlow
	}

	public sealed class AutoPlayMessage : IMessage
	{
		public AutoPlayMode Mode;
		public AutoPlayRequestSource Source;

		public AutoPlayMessage(AutoPlayMode mode, AutoPlayRequestSource source)
		{
			Mode = mode;
			Source = source;
		}
	}
}