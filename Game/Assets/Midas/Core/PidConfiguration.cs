using System;

namespace Midas.Core
{
	public enum GameInformationDisplayStyle
	{
		/// <summary>
		/// Don't display the game information.
		/// </summary>
		None = 0,

		/// <summary>
		/// Display the game information using the "Victorian" style.
		/// </summary>
		Victoria = 1,

		/// <summary>
		/// Display the game information using the "Queensland" style.
		/// </summary>
		Queensland = 2,

		/// <summary>
		/// Display the game information using the "NewZealand" style.
		/// </summary>
		NewZealand = 3,
	}

	public enum SessionTrackingOption
	{
		/// <summary>
		/// Session tracking is disabled.
		/// </summary>
		Disabled = 0,

		/// <summary>
		/// Session tracking is viewable.
		/// </summary>
		Viewable = 1,

		/// <summary>
		/// Session tracking is controlled by player.
		/// </summary>
		PlayerControlled = 2,
	}

	/// <summary>
	/// This class contains information of PID configuration.
	/// </summary>
	public sealed class PidConfiguration
	{
		#region Properties

		/// <summary>
		/// Gets the flag indicating if the PID main entry screen is enabled.
		/// </summary>
		public bool IsMainEntryEnabled { get; }

		/// <summary>
		/// Gets the flag indicating if RequestService is enabled.
		/// </summary>
		public bool IsRequestServiceEnabled { get; }

		/// <summary>
		/// Gets the flag indicating if the request service is active.
		/// It is used to sync service activation state between foundation and game. True means active.
		/// </summary>
		public bool IsRequestServiceActivated { get; }

		/// <summary>
		/// Gets the game information display style.
		/// </summary>
		public GameInformationDisplayStyle GameInformationDisplayStyle { get; }

		/// <summary>
		/// Gets the session tracking option.
		/// </summary>
		public SessionTrackingOption SessionTrackingOption { get; }

		/// <summary>
		/// Gets the flag indicating if game rules is enabled.
		/// </summary>
		public bool IsGameRulesEnabled { get; }

		/// <summary>
		/// Gets the Information Menu Timeout.
		/// A value of zero indicates no timeout action.
		/// </summary>
		public TimeSpan InformationMenuTimeout { get; }

		/// <summary>
		/// Gets the Session Start Message Timeout.
		/// A value of zero indicates no timeout action.
		/// </summary>
		public TimeSpan SessionStartMessageTimeout { get; }

		/// <summary>
		/// Gets the View Session Screen Timeout.
		/// A value of zero indicates no timeout action.
		/// </summary>
		public TimeSpan ViewSessionScreenTimeout { get; }

		/// <summary>
		/// Gets the View Game Information Timeout.
		/// A value of zero indicates no timeout action.
		/// </summary>
		public TimeSpan ViewGameInformationTimeout { get; }

		/// <summary>
		/// Gets the View Game Rules Timeout.
		/// A value of zero indicates no timeout action.
		/// </summary>
		public TimeSpan ViewGameRulesTimeout { get; }

		/// <summary>
		/// Gets the View Pay Table Timeout.
		/// A value of zero indicates no timeout action.
		/// </summary>
		public TimeSpan ViewPayTableTimeout { get; }

		/// <summary>
		/// Gets the Session Timeout Interval.
		/// Session timeout interval indicates the amount of idle time in seconds before the EGM automatically
		/// stops session tracking. A value of zero indicates no timeout action.
		/// </summary>
		public TimeSpan SessionTimeoutInterval { get; }

		/// <summary>
		/// Gets the flag indicating if the Session Timeout Interval starts to count on zero.
		/// </summary>
		public bool SessionTimeoutStartOnZeroCredits { get; }

		/// <summary>
		/// Gets the total number of link enrolments.
		/// If the game does not support PID, reject this parameter.
		/// Total number of progressive and mystery link jackpots EGM is enrolled in.
		/// </summary>
		public ushort TotalNumberLinkEnrolments { get; }

		/// <summary>
		/// Gets the total link percentage contributions.
		/// </summary>
		public string TotalLinkPercentageContributions { get; }

		/// <summary>
		/// Gets the flag indicating if Jackpot display message includes count or not.
		/// </summary>
		public bool ShowLinkJackpotCount { get; }

		/// <summary>
		/// Gets the RTP of the link jackpot components.
		/// </summary>
		public double LinkRtpForGameRtp { get; }

		#endregion

		#region Construction

		public PidConfiguration(bool isMainEntryEnabled, bool isRequestServiceEnabled, bool isRequestServiceActivated, GameInformationDisplayStyle gameInformationDisplayStyle, SessionTrackingOption sessionTrackingOption, bool isGameRulesEnabled, TimeSpan informationMenuTimeout, TimeSpan sessionStartMessageTimeout, TimeSpan viewSessionScreenTimeout, TimeSpan viewGameInformationTimeout, TimeSpan viewGameRulesTimeout, TimeSpan viewPayTableTimeout, TimeSpan sessionTimeoutInterval, bool sessionTimeoutStartOnZeroCredits, ushort totalNumberLinkEnrolments, string totalLinkPercentageContributions, bool showLinkJackpotCount, double linkRtpForGameRtp)
		{
			IsMainEntryEnabled = isMainEntryEnabled;
			IsRequestServiceEnabled = isRequestServiceEnabled;
			IsRequestServiceActivated = isRequestServiceActivated;
			GameInformationDisplayStyle = gameInformationDisplayStyle;
			SessionTrackingOption = sessionTrackingOption;
			IsGameRulesEnabled = isGameRulesEnabled;
			InformationMenuTimeout = informationMenuTimeout;
			SessionStartMessageTimeout = sessionStartMessageTimeout;
			ViewSessionScreenTimeout = viewSessionScreenTimeout;
			ViewGameInformationTimeout = viewGameInformationTimeout;
			ViewGameRulesTimeout = viewGameRulesTimeout;
			ViewPayTableTimeout = viewPayTableTimeout;
			SessionTimeoutInterval = sessionTimeoutInterval;
			SessionTimeoutStartOnZeroCredits = sessionTimeoutStartOnZeroCredits;
			TotalNumberLinkEnrolments = totalNumberLinkEnrolments;
			TotalLinkPercentageContributions = totalLinkPercentageContributions;
			ShowLinkJackpotCount = showLinkJackpotCount;
			LinkRtpForGameRtp = linkRtpForGameRtp;
		}

		#endregion
	}
}
