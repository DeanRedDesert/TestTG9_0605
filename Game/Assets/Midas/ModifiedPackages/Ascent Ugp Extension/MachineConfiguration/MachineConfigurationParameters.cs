using System;

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MachineConfiguration
{
	/// <summary>
	/// Holds parameters related to machine configuration.
	/// </summary>
	[Serializable]
	public class MachineConfigurationParameters
	{
		/// <summary>
		/// Gets the flag indicating if the clock is visible.
		/// </summary>
		public bool IsClockVisible { get; internal set; }

		/// <summary>
		/// Gets the format of the time on the clock.
		/// </summary>
		public string ClockFormat { get; internal set; }

		/// <summary>
		/// Gets the tokenisation value (the cents value of one token).
		/// </summary>
		public long Tokenisation { get; internal set; }

		/// <summary>
		/// Gets the game cycle time in milli-seconds.
		/// </summary>
		public long GameCycleTime { get; internal set; }

		/// <summary>
		/// Gets the flag indicating if continuous play is allowed.
		/// This means that holding down any play button causes a game to start.
		/// </summary>
		public bool IsContinuousPlayAllowed { get; internal set; }

		/// <summary>
		/// Gets the flag indicating if the features should automatically start.
		/// </summary>
		public bool IsFeatureAutoStartEnabled { get; internal set; }

		/// <summary>
		/// Gets the current maximum bet (in cents).
		/// This is used to dynamically limit the amount a player can bet without needing to reconfigure the game.
		/// </summary>
		public long CurrentMaximumBet { get; internal set; }

		/// <summary>
		/// Gets the flag indicating if slam spin is allowed.
		/// </summary>
		public bool IsSlamSpinAllowed { get; internal set; }

		/// <summary>
		/// Gets the win cap style.
		/// </summary>
		public UgpMachineConfigurationWinCapStyle WinCapStyle { get; internal set; }

		/// <summary>
		/// Get the QCOM defined jurisdiction.
		/// </summary>
		public int QcomJurisdiction { get; internal set; }

		/// <summary>
		/// Get the detected CabinetId.
		/// </summary>
		public string CabinetId { get; internal set; }

		/// <summary>
		/// Get the detected Brain Box Id.
		/// </summary>
		public string BrainboxId { get; internal set; }

		/// <summary>
		/// Get the detected GPU name.
		/// </summary>
		public string Gpu { get; internal set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public MachineConfigurationParameters(bool isClockVisible, string clockFormat, long tokenisation, long gameCycleTime, bool isContinuousPlayAllowed, bool isFeatureAutoStartEnabled, long currentMaximumBet, bool isSlamSpinAllowed, UgpMachineConfigurationWinCapStyle winCapStyle, int qcomJurisdiction, string cabinetId, string brainboxId, string gpu)
		{
			IsClockVisible = isClockVisible;
			ClockFormat = clockFormat;
			Tokenisation = tokenisation;
			GameCycleTime = gameCycleTime;
			IsContinuousPlayAllowed = isContinuousPlayAllowed;
			IsFeatureAutoStartEnabled = isFeatureAutoStartEnabled;
			CurrentMaximumBet = currentMaximumBet;
			IsSlamSpinAllowed = isSlamSpinAllowed;
			WinCapStyle = winCapStyle;
			QcomJurisdiction = qcomJurisdiction;
			CabinetId = cabinetId;
			BrainboxId = brainboxId;
			Gpu = gpu;
		}
	}
}