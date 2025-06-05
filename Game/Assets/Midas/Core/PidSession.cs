using System;
using Midas.Core.General;

namespace Midas.Core
{
	/// <summary>
	/// This class contains information of PID session.
	/// </summary>
	public sealed class PidSession
	{
		#region Properties

		/// <summary>
		/// Gets the flag indicating if session tracking is active.
		/// </summary>
		public bool IsSessionTrackingActive { get; }

		/// <summary>
		/// Gets the credit meter at the start of the session, in units fo cent.
		/// </summary>
		public Money CreditMeterAtStart { get; }

		/// <summary>
		/// Gets the currently available credits.
		/// </summary>
		public Money AvailableCredits { get; }

		/// <summary>
		/// Gets the amount of cash in during the session.
		/// </summary>
		public Money CashIn { get; }

		/// <summary>
		/// Gets the amount of cash out during the session.
		/// </summary>
		public Money CashOut { get; }

		/// <summary>
		/// Gets the amount of credits played.
		/// </summary>
		public Money CreditsPlayed { get; }

		/// <summary>
		/// Gets the amount of credits won.
		/// </summary>
		public Money CreditsWon { get; }

		/// <summary>
		/// Gets the amount won or lost during the session.
		/// </summary>
		public Money SessionWinOrLoss { get; }

		/// <summary>
		/// Gets the flag indicating if the current or last PID tracking session is a winning session.
		/// </summary>
		public bool IsWinningSession { get; }

		/// <summary>
		/// Gets the flag indicating if the session is for Crown EGM.
		/// </summary>
		public bool IsCrown { get; }

		/// <summary>
		/// Gets the date/time the session was started.
		/// </summary>
		public DateTime SessionStarted { get; }

		/// <summary>
		/// Gets the current session duration.
		/// </summary>
		public TimeSpan SessionDuration { get; }

		#endregion

		#region Construction

		/// <summary>
		/// Constructor.
		/// </summary>
		public PidSession(bool isSessionTrackingActive, Money creditMeterAtStart, Money availableCredits, Money cashIn, Money cashOut, Money creditsPlayed, Money creditsWon, Money sessionWinOrLoss, bool isWinningSession, bool isCrown, DateTime sessionStarted, TimeSpan sessionDuration)
		{
			IsSessionTrackingActive = isSessionTrackingActive;
			CreditMeterAtStart = creditMeterAtStart;
			AvailableCredits = availableCredits;
			CashIn = cashIn;
			CashOut = cashOut;
			CreditsPlayed = creditsPlayed;
			CreditsWon = creditsWon;
			SessionWinOrLoss = sessionWinOrLoss;
			IsWinningSession = isWinningSession;
			IsCrown = isCrown;
			SessionStarted = sessionStarted;
			SessionDuration = sessionDuration;
		}

		#endregion
	}
}
