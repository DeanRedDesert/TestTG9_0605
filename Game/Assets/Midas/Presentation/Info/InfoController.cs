using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.Core.Coroutine;
using Midas.Core.General;
using Midas.Core.StateMachine;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Data;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.ExtensionMethods;
using Midas.Presentation.Game;
using Midas.Presentation.General;
using Coroutine = Midas.Core.Coroutine.Coroutine;

namespace Midas.Presentation.Info
{
	public sealed partial class InfoController : IPresentationController, IButtonControllerOwner
	{
		private bool showInfo;
		private InfoMode requestedMode;
		private bool? requestedRulesPageChange;
		private bool? requestSessionChange;
		private InfoStatus infoStatus;
		private Coroutine infoCoroutine;
		private IRulesController rulesController;
		private PidStatus pidStatus;
		private readonly AutoUnregisterHelper autoUnregisterHelper = new AutoUnregisterHelper();

		private readonly IDictionary<string, InfoMode> infoModeMapping = new Dictionary<string, InfoMode>
		{
			{ "START SESSION TRACKING", InfoMode.StartSession },
			{ "VIEW SESSION", InfoMode.ViewSession },
			{ "GAME INFORMATION", InfoMode.ViewGameInfo },
			{ "GAME RULES", InfoMode.Rules },
			{ "REQUEST SERVICE", InfoMode.ToggleService },
			{ "CANCEL SERVICE", InfoMode.ToggleService },
		};

		private IReadOnlyList<string> lobbyButtonNames;

		public IReadOnlyList<IButtonController> ButtonControllers { get; }

		public InfoController()
		{
			infoStatus = StatusDatabase.InfoStatus;
			ButtonControllers = new[] { new InfoButtonController() };
		}

		public void Destroy()
		{
			infoStatus = null;
		}

		public void Init()
		{
			infoCoroutine = StateMachineService.FrameUpdateRoot.StartCoroutine(InfoHandler(), "Info");
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.PidService.PidSession, HandleSessionChanged);
			pidStatus = StatusDatabase.PidStatus;
		}

		public void DeInit()
		{
			autoUnregisterHelper.UnRegisterAll();
			infoCoroutine?.Stop();
			infoCoroutine = null;
			pidStatus = null;
		}

		public void InfoRequest(bool show) => showInfo = show;
		public void RequestLobbyOption(InfoMode option) => requestedMode = option;

		public void RequestRulesPageChange(bool next) => requestedRulesPageChange = next;

		public void RegisterRulesController(IRulesController rc) => rulesController = rc;

		public void RequestSessionChange(bool start) => requestSessionChange = start;

		private IEnumerator<CoroutineInstruction> InfoHandler()
		{
			requestedMode = InfoMode.None;
			showInfo = false;
			infoStatus.SetActiveMode(InfoMode.None);

			while (true)
			{
				while (!showInfo)
					yield return null;

				SetProgressiveCeilingText();
				SetAncillaryText();
				infoStatus.CurrentRulesPage = 0;
				lobbyButtonNames = CreateButtonNames();
				Communication.ToLogicSender.Send(new PidMessage(PidAction.Activated));

				var rulesPages = rulesController?.Setup() ?? Array.Empty<RulesPageType>();

				if (infoStatus.ActiveMode == InfoMode.None)
					requestedMode = pidStatus.Config.IsMainEntryEnabled ? InfoMode.Lobby : InfoMode.Rules;

				var infoEndTime = FrameTime.CurrentTime;

				while (showInfo)
				{
					yield return null;

					if (requestSessionChange.HasValue)
					{
						if (requestSessionChange.Value)
						{
							requestedMode = InfoMode.StartSession;
						}
						else
						{
							Communication.ToLogicSender.Send(new PidMessage(PidAction.StopSessionTracking));
							showInfo = false;
						}

						requestSessionChange = null;
					}

					if (requestedMode == InfoMode.ToggleService)
					{
						Communication.ToLogicSender.Send(new PidMessage(PidAction.ToggleService));
						requestedMode = InfoMode.None;
						showInfo = false;
					}

					if (requestedMode != InfoMode.None)
					{
						infoEndTime = SetEndTime(EnablePage(requestedMode, rulesPages));
						requestedMode = InfoMode.None;
					}

					if (requestedRulesPageChange.HasValue)
					{
						if (requestedRulesPageChange.Value)
							infoStatus.CurrentRulesPage = (infoStatus.CurrentRulesPage + 1) % rulesPages.Count;
						else
							infoStatus.CurrentRulesPage = infoStatus.CurrentRulesPage == 0 ? rulesPages.Count - 1 : infoStatus.CurrentRulesPage - 1;

						infoEndTime = SetEndTime(RulesPageTimeout(rulesPages));
						requestedRulesPageChange = null;
					}

					UpdateViewSessionTime();

					var timeRemaining = infoEndTime - FrameTime.CurrentTime;
					if (StatusDatabase.GameStatus.GameLogicPaused || timeRemaining <= TimeSpan.Zero || !StatusDatabase.GameFunctionStatus.GameButtonBehaviours.InfoButton.IsActive())
						showInfo = false;
				}

				// TODO. This isn't sent straight away if info is exited due to interruption.
				Communication.ToLogicSender.Send(new PidMessage(PidAction.Deactivated));
				infoStatus.SetActiveMode(InfoMode.None);
				lobbyButtonNames = Array.Empty<string>();
			}

			TimeSpan SetEndTime(TimeSpan time) => time == TimeSpan.Zero ? FrameTime.CurrentTime + TimeSpan.FromDays(3650) : FrameTime.CurrentTime + time;
		}

		private void SetProgressiveCeilingText()
		{
			if (StatusDatabase.ConfigurationStatus.MachineConfig.Jurisdiction != "NSW")
			{
				infoStatus.ProgressiveCeilingText = string.Empty;
				return;
			}

			var standaloneProgressives = StatusDatabase.ProgressiveStatus.ProgressiveLevels.Where(p => p.IsStandalone);
			var list = standaloneProgressives.Select(progressive => $"HIGHEST {progressive.Name.ToUpper()} AMOUNT AVAILABLE IS {FormatCurrency(progressive.Ceiling)}").ToArray();
			infoStatus.ProgressiveCeilingText = string.Join(Environment.NewLine, list);
		}

		private void SetAncillaryText()
		{
			var text = $"Winnings may be gambled up to {StatusDatabase.ConfigurationStatus.AncillaryConfig.CycleLimit} times.\n";
			text += $"The maximum prize that may be won on a gamble feature cannot exceed {FormatCurrency(StatusDatabase.ConfigurationStatus.AncillaryConfig.MoneyLimit, MoneyAndCreditDisplayMode.MoneyWhole)}.";
			infoStatus.AncillaryText = text;
		}

		private TimeSpan EnablePage(InfoMode mode, IReadOnlyList<RulesPageType> rulesPages)
		{
			infoStatus.SetActiveMode(mode);
			switch (mode)
			{
				case InfoMode.Lobby:
					return pidStatus.Config.InformationMenuTimeout;
				case InfoMode.StartSession:
					UpdateStartSession();
					Communication.ToLogicSender.Send(new PidMessage(PidAction.StartSessionTracking));
					return pidStatus.Config.SessionStartMessageTimeout;
				case InfoMode.ViewSession:
					startTime = FrameTime.CurrentTime;
					UpdateViewSession();
					Communication.ToLogicSender.Send(new PidMessage(PidAction.SessionInfoEntered));
					return pidStatus.Config.ViewSessionScreenTimeout;
				case InfoMode.ViewGameInfo:
					UpdateGameInfoStatus();
					Communication.ToLogicSender.Send(new PidMessage(PidAction.GameInfoEntered));
					return pidStatus.Config.ViewGameInformationTimeout;
				case InfoMode.Rules:
					return RulesPageTimeout(rulesPages);
			}

			return TimeSpan.Zero;
		}

		internal InfoMode InfoModeFromName(string name) => infoModeMapping[name];

		private static string FormatCurrency(Money value, MoneyAndCreditDisplayMode mode = MoneyAndCreditDisplayMode.MoneyWholePlusBase) => StatusDatabase.ConfigurationStatus.CreditAndMoneyFormatter.GetFormatted(mode, value, CreditDisplaySeparatorMode.NoSeparator);

		private TimeSpan RulesPageTimeout(IReadOnlyList<RulesPageType> rulesPages) => rulesPages[infoStatus.CurrentRulesPage] == RulesPageType.Rules ? pidStatus.Config.ViewGameRulesTimeout : pidStatus.Config.ViewPayTableTimeout;
	}
}