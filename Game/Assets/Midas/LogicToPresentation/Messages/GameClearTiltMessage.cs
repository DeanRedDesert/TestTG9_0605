using Midas.Core;

namespace Midas.LogicToPresentation.Messages
{
	/// <summary>
	/// Clear a game tilt message.
	/// </summary>
	public sealed class GameClearTiltMessage : IMessage
	{
		public string TiltKey { get; }

		#region Public

		public GameClearTiltMessage(string tiltKey)
		{
			TiltKey = tiltKey;
		}

		#endregion
	}
}