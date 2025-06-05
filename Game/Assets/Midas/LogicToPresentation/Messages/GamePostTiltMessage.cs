using Midas.Core;

namespace Midas.LogicToPresentation.Messages
{
	/// <summary>
	/// Post a game tilt message.
	/// </summary>
	public sealed class GamePostTiltMessage : IMessage
	{
		public string TiltKey { get; }
		public GameTiltPriority Priority { get; }
		public string Title { get; }
		public string Message { get; }
		public bool IsBlocking { get; }
		public bool DiscardOnGameShutdown { get; }
		public bool UserInterventionRequired { get; }

		#region Public

		public GamePostTiltMessage(string tiltKey, GameTiltPriority priority, string title, string message, bool isBlocking, bool discardOnGameShutdown, bool userInterventionRequired)
		{
			TiltKey = tiltKey;
			Priority = priority;
			Title = title;
			Message = message;
			IsBlocking = isBlocking;
			DiscardOnGameShutdown = discardOnGameShutdown;
			UserInterventionRequired = userInterventionRequired;
		}

		#endregion
	}
}