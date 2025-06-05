using System;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Pid;
using static Midas.Ascent.AscentFoundation;

namespace Midas.Ascent.Ugp
{
	public sealed partial class UgpInterfaces
	{
		private IUgpPid ugpPid;

		/// <summary>
		/// Gets the current status of the service request.
		/// </summary>
		public bool IsServiceRequested
		{
			get
			{
				return ugpPid is { IsServiceRequested: true };
			}
		}

		/// <summary>
		/// Event raised when receiving message of force PID activation.
		/// </summary>
		public event EventHandler<PidActivationEventArgs> PidActivated;

		/// <summary>
		/// Event raised when the Pid service requested has changed.
		/// </summary>
		public event EventHandler<PidServiceRequestedChangedEventArgs> IsServiceRequestedChanged;

		/// <summary>
		/// Event raised when the Pid configuration has changed.
		/// </summary>
		public event EventHandler<PidConfigurationChangedEventArgs> PidConfigurationChanged;

		private void InitPid()
		{
			ugpPid = GameLib.GetInterface<IUgpPid>();
			if (ugpPid != null)
			{
				ugpPid.PidActivated += OnPidActivated;
				ugpPid.IsServiceRequestedChanged += OnServiceRequestedChanged;
				ugpPid.PidConfigurationChanged += OnPidConfigurationChanged;
			}
		}

		private void DeInitPid()
		{
			if (ugpPid != null)
			{
				ugpPid.PidActivated -= OnPidActivated;
				ugpPid.IsServiceRequestedChanged -= OnServiceRequestedChanged;
				ugpPid.PidConfigurationChanged -= OnPidConfigurationChanged;
			}

			ugpPid = null;
		}

		/// <summary>
		/// Starts the PID Tracking process.
		/// </summary>
		public void StartTracking()
		{
			ugpPid?.StartTracking();
		}

		/// <summary>
		/// Stops the PID Tracking process.
		/// </summary>
		public void StopTracking()
		{
			ugpPid?.StopTracking();
		}

		/// <summary>
		/// Gets the PID related session data that is tracked.
		/// </summary>
		/// <returns>
		/// The PID session data.
		/// </returns>
		public PidSessionData GetSessionData()
		{
			return ugpPid != null
				? ugpPid.GetSessionData()
				: new PidSessionData();
		}

		/// <summary>
		/// Gets the PID configuration.
		/// </summary>
		/// <returns>
		/// The PID configuration.
		/// </returns>
		public PidConfiguration GetPidConfiguration()
		{
			return ugpPid != null
				? ugpPid.GetPidConfiguration()
				: new StandaloneAustralianPidFoundationSettings().ToPidConfiguration();
		}

		/// <summary>
		/// Sends notification that the player is entering or exiting the player information screens.
		/// </summary>
		/// <param name='currentStatus'>Specify the current activation status.</param>
		public void ActivationStatusChanged(bool currentStatus)
		{
			ugpPid?.ActivationStatusChanged(currentStatus);
		}

		/// <summary>
		/// Sends notification that the player is entering the game information screen.
		/// </summary>
		public void GameInformationScreenEntered()
		{
			ugpPid?.GameInformationScreenEntered();
		}

		/// <summary>
		/// Sends notification that the player is entering the session information screen.
		/// </summary>
		public void SessionInformationScreenEntered()
		{
			ugpPid?.SessionInformationScreenEntered();
		}

		/// <summary>
		/// Sends notification that the player is requesting attendant service.
		/// </summary>
		public void AttendantServiceRequested()
		{
			ugpPid?.AttendantServiceRequested();
		}

		/// <summary>
		/// Requests a force payout.
		/// </summary>
		public void RequestForcePayout()
		{
			ugpPid?.RequestForcePayout();
		}

		private void OnPidActivated(object s, PidActivationEventArgs e)
		{
			PidActivated?.Invoke(s, e);
		}

		private void OnServiceRequestedChanged(object s, PidServiceRequestedChangedEventArgs e)
		{
			IsServiceRequestedChanged?.Invoke(s, e);
		}

		private void OnPidConfigurationChanged(object s, PidConfigurationChangedEventArgs e)
		{
			PidConfigurationChanged?.Invoke(s, e);
		}
	}
}