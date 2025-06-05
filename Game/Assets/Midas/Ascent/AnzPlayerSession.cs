using System;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSession;

namespace Midas.Ascent
{
	public sealed class AnzPlayerSession : IPlayerSession
	{
		public PlayerSessionStatus PlayerSessionStatus { get; private set; } = new PlayerSessionStatus();
		public bool SessionTimerDisplayEnabled { get; } = false;
		public event EventHandler<PlayerSessionStatusChangedEventArgs> PlayerSessionStatusChangedEvent;

		public void SessionStart()
		{
			PlayerSessionStatus = new PlayerSessionStatus(true, DateTime.Now);
			PlayerSessionStatusChangedEvent?.Invoke(this, new PlayerSessionStatusChangedEventArgs(PlayerSessionStatus));
		}

		public void SessionStop()
		{
			PlayerSessionStatus = new PlayerSessionStatus(false, DateTime.Now);
			PlayerSessionStatusChangedEvent?.Invoke(this, new PlayerSessionStatusChangedEventArgs(PlayerSessionStatus));
		}
	}
}