namespace Midas.LogicToPresentation
{
	/// <summary>
	/// Contains the objects that communicate back and forwards between the logic and presentation.
	/// </summary>
	public static class Communication
	{
		private static MessageDispatcher presentationDispatcher;
		private static MessageDispatcher logicDispatcher;

		/// <summary>
		/// Sends messages from logic to presentation.
		/// </summary>
		public static ISender ToPresentationSender => presentationDispatcher;

		/// <summary>
		/// Presentation uses this object to listen for messages.
		/// </summary>
		public static IDispatcher PresentationDispatcher => presentationDispatcher;

		/// <summary>
		/// Sends messages from presentation to logic.
		/// </summary>
		public static ISender ToLogicSender => logicDispatcher;

		/// <summary>
		/// Logic uses this object to listen for messages.
		/// </summary>
		public static IDispatcher LogicDispatcher => logicDispatcher;

		/// <summary>
		/// Initialises logic to presentation comms.
		/// </summary>
		public static void Init()
		{
			presentationDispatcher = new MessageDispatcher("L2P");
			logicDispatcher = new MessageDispatcher("P2L");
		}

		/// <summary>
		/// Cleans up logic to presentation comms.
		/// </summary>
		public static void DeInit()
		{
			presentationDispatcher = null;
			logicDispatcher = null;
		}
	}
}