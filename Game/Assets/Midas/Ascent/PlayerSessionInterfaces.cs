using System;
using System.Collections.Generic;
using System.Linq;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSession;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSessionParams;
using static Midas.Ascent.AscentFoundation;

namespace Midas.Ascent
{
	public sealed class PlayerSessionInterfaces
	{
		private readonly IPlayerSession playerSession;
		private readonly IPlayerSessionParameters playerSessionParameters;

		public event EventHandler<PlayerSessionStatusChangedEventArgs> PlayerSessionStatusChanged;
		public event EventHandler<CurrentResetParametersChangedEventArgs> CurrentResetParametersChanged;

		public PlayerSessionInterfaces(bool ugpInterfacesIsUgpFoundation)
		{
			playerSession = ugpInterfacesIsUgpFoundation ? new AnzPlayerSession() : GameLib.GetInterface<IPlayerSession>();

			if (playerSession != null)
				playerSession.PlayerSessionStatusChangedEvent += OnPlayerSessionPlayerSessionStatusChangedEvent;

			playerSessionParameters = ugpInterfacesIsUgpFoundation ? new AnzPlayerSessionParameters() : GameLib.GetInterface<IPlayerSessionParameters>();

			if (playerSessionParameters != null)
				playerSessionParameters.CurrentResetParametersChangedEvent += OnPlayerSessionParametersCurrentResetParametersChangedEvent;
		}

		public PlayerSessionStatus GetPlayerSessionStatus() => playerSession == null ? new PlayerSessionStatus() : playerSession.PlayerSessionStatus;

		public bool GetSessionTimerDisplayEnabled() => playerSession is { SessionTimerDisplayEnabled: true };

		public IReadOnlyList<PlayerSessionParameterType> PendingParametersToReset() => playerSessionParameters == null ? Array.Empty<PlayerSessionParameterType>() : playerSessionParameters.PendingParametersToReset.ToArray();

		public bool IsPlayerSessionParameterResetEnabled() => playerSessionParameters is { IsPlayerSessionParameterResetEnabled: true };

		public void ReportParametersBeingReset(IReadOnlyList<PlayerSessionParameterType> parametersBeingReset)
		{
			playerSessionParameters?.ReportParametersBeingReset(parametersBeingReset);
		}

		public void InitiatePlayerSessionReset(IReadOnlyList<PlayerSessionParameterType> parametersToReset)
		{
			if (playerSessionParameters is AnzPlayerSessionParameters ps)
				ps.ResetRequired(parametersToReset);
		}

		public void ActivatePlayerSession(bool isActive)
		{
			if (playerSession is AnzPlayerSession ps)
			{
				if (isActive)
					ps.SessionStart();
				else
					ps.SessionStop();
			}
		}

		public void DeInit()
		{
			if (playerSession != null)
				playerSession.PlayerSessionStatusChangedEvent -= OnPlayerSessionPlayerSessionStatusChangedEvent;
			if (playerSessionParameters != null)
				playerSessionParameters.CurrentResetParametersChangedEvent -= OnPlayerSessionParametersCurrentResetParametersChangedEvent;
		}

		private void OnPlayerSessionPlayerSessionStatusChangedEvent(object sender, PlayerSessionStatusChangedEventArgs e) => PlayerSessionStatusChanged?.Invoke(sender, e);
		private void OnPlayerSessionParametersCurrentResetParametersChangedEvent(object sender, CurrentResetParametersChangedEventArgs e) => CurrentResetParametersChanged?.Invoke(sender, e);
	}
}