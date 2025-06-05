using System;
using System.Text;
using Midas.Core;
using Midas.Presentation.Data;

namespace Midas.Presentation.Info
{
	public sealed partial class InfoController
	{
		private TimeSpan currentDuration;
		private TimeSpan startTime;
		private TimeSpan lastUpdateTime;

		private void UpdateStartSession()
		{
			var session = StatusDatabase.PidStatus.Session;
			var config = StatusDatabase.PidStatus.Config;
			if (!(session is { IsSessionTrackingActive: true }))
			{
				infoStatus.SessionData = string.Empty;
				return;
			}

			var text = $"At {session.SessionStarted:h:mm tt dd/MM/yy} \n" +
				"Press RETURN TO GAME at any time to return to the game\n" +
				"This machine will now track your activity\n" +
				"Press i at any time between games to enter\n" +
				"menu to view or stop tracking\n";

			if (config.SessionTimeoutInterval.TotalSeconds > 0)
			{
				text += $"\nSessions will Automatically end after {FormatTimeString(config.SessionTimeoutInterval)} of inactivity\n";

				if (config.SessionTimeoutStartOnZeroCredits)
					text += "on this Machine whilst the CREDIT meter is zero";
			}
			else
			{
				text += "\n\n";
			}

			infoStatus.SessionData = text;
		}

		private void UpdateViewSession()
		{
			var session = StatusDatabase.PidStatus.Session;
			if (!(session is { IsSessionTrackingActive: true }))
			{
				infoStatus.SessionData = string.Empty;
				return;
			}

			currentDuration = session.SessionDuration.Add(FrameTime.CurrentTime - startTime);

			var text = $"Cash in = {FormatCurrency(session.CashIn)} Credits Played = {FormatCurrency(session.CreditsPlayed)} Credits Won = {FormatCurrency(session.CreditsWon)}*\n" +
				$"Session Win or (Loss) = {(session.IsWinningSession ? FormatCurrency(session.SessionWinOrLoss) : "(" + FormatCurrency(session.SessionWinOrLoss) + ")")}* Cash Out = {FormatCurrency(session.CashOut)}\n\n" +
				$"{(session.IsCrown ? "Includes amounts played and won from Crown Credits.\n" : string.Empty)}*(These totals exclude jackpot amounts not included on the credit meter)\n\n" +
				$"Session started {session.SessionStarted.ToString("h:mm tt")} on {session.SessionStarted.ToString("dd/MM/yy")}\n" +
				$"Total time played {FormatTimeString(currentDuration)}\n" +
				$"Current time : {DateTime.Now:h:mm tt dd/MM/yy}  Credit Meter at start {FormatCurrency(session.CreditMeterAtStart)} Credits Available = {FormatCurrency(session.AvailableCredits)}";

			infoStatus.SessionData = text;
		}

		private void UpdateViewSessionTime()
		{
			if (infoStatus.ActiveMode != InfoMode.ViewSession)
				return;

			var delta = FrameTime.CurrentTime - lastUpdateTime;

			if (Math.Abs(delta.TotalSeconds) < 0.5f)
				return;

			UpdateViewSession();
			lastUpdateTime = FrameTime.CurrentTime;
		}

		private void HandleSessionChanged(PidSession sessionData)
		{
			if (infoStatus.ActiveMode == InfoMode.StartSession)
				UpdateStartSession();
		}

		private static string FormatTimeString(TimeSpan ts)
		{
			var hours = ts.Hours;
			var minutes = ts.Minutes;
			var seconds = ts.Seconds;
			var sb = new StringBuilder();

			if (hours > 0)
				Append(sb, hours, "hour");

			if (minutes > 0)
				Append(sb, minutes, "minute");

			if (seconds > 0)
			{
				if (minutes > 0 || hours > 0)
					sb.Append("and ");

				Append(sb, seconds, "second");
			}

			return sb.ToString();
		}

		private static void Append(StringBuilder sb, int periods, string singularPeriod)
		{
			sb.Append(periods);
			sb.Append(" ");
			sb.Append(periods == 1 ? singularPeriod : singularPeriod + "s");
			sb.Append(" ");
		}
	}
}