using System;

namespace Midas.Core
{
	public sealed class PlayerSession
	{
		public bool IsSessionActive { get; }
		public DateTime SessionStartTime { get; }

		public PlayerSession(bool isSessionActive, DateTime sessionStartTime)
		{
			IsSessionActive = isSessionActive;
			SessionStartTime = sessionStartTime;
		}
	}
}