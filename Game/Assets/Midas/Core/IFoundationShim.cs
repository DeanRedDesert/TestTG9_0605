using System.Collections.Generic;
using System.Threading;
using Midas.Core.Configuration;
using Midas.Core.General;

namespace Midas.Core
{
	public interface IFoundationShim
	{
		string GameMountPoint { get; }
		bool IsInitialising { get; }
		FoundationGameMode GameMode { get; }
		FoundationShowMode ShowMode { get; }
		bool ShouldGameLogicExit { get; }
		bool IsPaused { get; }
		PaytableConfig PaytableConfiguration { get; }
		GameIdentityType GameIdentity { get; }
		FoundationType FoundationType { get; }
		void ProcessEvents(WaitHandle waitHandle);

		/// <param name="count">
		/// How many random numbers to obtain. It includes the PrePickedNumbers.
		/// </param>
		/// <param name="rangeMinInclusive">
		/// The inclusive lower bound of the random numbers returned.
		/// </param>
		/// <param name="rangeMaxInclusive">
		/// The inclusive upper bound of the random numbers returned.
		/// RangeMax must be greater than or equal to RangeMin.
		/// </param>
		IReadOnlyList<int> GetRandomNumbers(uint count, int rangeMinInclusive, int rangeMaxInclusive);

		#region CriticalData

		void WriteNvram(NvramScope scope, string path, byte[] data);
		bool TryReadNvram(NvramScope scope, string path, out byte[] data);
		bool RemoveNvram(NvramScope scope, string path);

		#endregion

		#region Configuration

		ConfigData ReadConfiguration();
		bool ChangeGameDenom(Money newDenom);
		void ChangeLanguage(string msgLanguage);

		#endregion

		#region Progressives

		void StartProgressiveAward(int awardIndex, string levelId, Money amount);
		void FinishedProgressiveAwardDisplay(int awardIndex, string levelId, Money defaultPaidAmount);

		#endregion

		#region Dashboard

		void RequestCashout();
		bool RequestThemeSelectionMenu();

		#endregion

		#region PID

		void PidActivated(bool status);
		void PidGameInfoEntered();
		void PidSessionInfoEntered();
		void StartPidSessionTracking();
		void StopPidSessionTracking();
		void ToggleServiceRequested();

		#endregion

		#region AutoPlay

		bool IsAutoPlayOn();
		bool SetAutoPlayOn();
		void SetAutoPlayOff();

		#endregion

		#region Demo

		void DemoSetDisplayState(DisplayState displayState);
		void DemoEnterGameMode(FoundationGameMode mode);
		void DemoEnterUtilityMode(string theme, KeyValuePair<string, string> paytable, Money denomination);
		int DemoGetHistoryRecordCount();
		bool DemoIsNextHistoryRecordAvailable();
		bool DemoIsPreviousHistoryRecordAvailable();
		void DemoNextHistoryRecord();
		void DemoPreviousHistoryRecord();
		IReadOnlyList<string> DemoGetRegistrySupportedThemes();
		IReadOnlyDictionary<KeyValuePair<string, string>, IReadOnlyList<long>> DemoGetRegistrySupportedDenominations(string theme);

		#endregion

		#region Show

		void ShowAddMoney(Money amount);

		#endregion

		#region Runtime Events

		void SendWaitingForInput(bool status);
		void SendDenomSelectionActive(bool active);

		#endregion

		#region Debug Info

		string TimingsString { get; }
		void ResetTimings();

		#endregion

		#region Player Session

		void ReportParametersBeingReset(IReadOnlyList<PlayerSessionParameterType> parametersBeingReset);
		void InitiatePlayerSessionReset(IReadOnlyList<PlayerSessionParameterType> parametersToReset);
		void PlayerSessionActive(bool isActive);

		#endregion

		#region Tilt

		void PostTilt(string tiltKey, GameTiltPriority priority, string title, string message, bool isBlocking, bool discardOnGameShutdown, bool userInterventionRequired);

		void ClearTilt(string tiltKey);

		#endregion

		#region Win Capping

		WinCapStyle WinCapStyle { get; }
		Money WinCapLimit { get; }

		#endregion
	}

	public enum WinCapStyle
	{
		None,
		Clip,
		ClipAndBreakout
	}

	public enum NvramScope
	{
		Theme,
		Variation,
		GameCycle,
		History
	}

	public enum FoundationGameMode
	{
		Play,
		History,
		Utility
	}

	public enum FoundationShowMode
	{
		None,
		Show,
		Development
	}

	public enum FoundationType
	{
		Ugp,
		Ascent,
	}

	public interface IProgressiveHit
	{
		string LevelId { get; }
		string SourceName { get; }
		string SourceDetails { get; }
	}

	public interface IFoundationPrize
	{
		string PrizeName { get; }
		Money RiskAmount { get; }
		Money Amount { get; }
		IProgressiveHit ProgressiveHit { get; }
	}
}