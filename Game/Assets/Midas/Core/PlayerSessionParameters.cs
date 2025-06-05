using System.Collections.Generic;

namespace Midas.Core
{
	public sealed class PlayerSessionParameters
	{
		public bool IsPlayerSessionParameterResetEnabled { get; }

		public IReadOnlyList<PlayerSessionParameterType> PendingParametersToReset { get; }

		public PlayerSessionParameters(bool isPlayerSessionParameterResetEnabled, IReadOnlyList<PlayerSessionParameterType> pendingParametersToReset)
		{
			IsPlayerSessionParameterResetEnabled = isPlayerSessionParameterResetEnabled;
			PendingParametersToReset = pendingParametersToReset;
		}
	}
}