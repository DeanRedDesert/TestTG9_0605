using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.Core.Coroutine;
using Midas.Core.ExtensionMethods;
using Midas.Core.General;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.AutoPlay;
using Midas.Presentation.Dashboard;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Game;

namespace Midas.Presentation.Denom
{
	public sealed class DenomController : IPresentationController
	{
		private readonly AutoUnregisterHelper autoUnregisterHelper = new AutoUnregisterHelper();
		private DenomStatus denomStatus = StatusDatabase.DenomStatus;
		private Coroutine denomSelectCoroutine;
		private Coroutine menuTimeoutCoroutine;
		private TimeSpan confirmTimeoutTime;
		private bool refresh;
		private CreditPlayoffStatusBase creditPlayoffStatus;

		public void Destroy() => denomStatus = null;

		public void Init()
		{
			autoUnregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.GameFunctionStatus, nameof(GameFunctionStatus.GameButtonBehaviours), OnDenomPlayableStatusChanged);
			autoUnregisterHelper.RegisterExpressionChangedHandler(typeof(DenomExpressions), nameof(DenomExpressions.IsMultiDenom), OnIsMultiDenomChanged);
			autoUnregisterHelper.RegisterMessageHandler<GameSetupPresentationDoneMessage>(Communication.PresentationDispatcher, OnGameSetupPresentationDoneMessage);
			autoUnregisterHelper.RegisterMessageHandler<ChangeGameDenomCancelledMessage>(Communication.PresentationDispatcher, OnChangeGameDenomCancelledMessage);
			autoUnregisterHelper.RegisterExpressionChangedHandler(typeof(PopupStatus), nameof(PopupStatus.IsCashoutConfirmOpen), OnCashoutConfirmPopupActive);
			autoUnregisterHelper.RegisterExpressionChangedHandler(typeof(PopupStatus), nameof(PopupStatus.IsInfoOpen), OnInfoOpen);
			autoUnregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.AutoPlayStatus, nameof(AutoPlayStatus.State), OnAutoPlayStateChanged);
			creditPlayoffStatus = StatusDatabase.QueryStatusBlock<CreditPlayoffStatusBase>();

			denomSelectCoroutine = FrameUpdateService.Update.StartCoroutine(DenomSelectCoroutine());
			//menuTimeoutCoroutine = FrameUpdateService.Update.StartCoroutine(MenuTimeoutCoroutine());
			DenomExpressions.Init();
		}

		public void DeInit()
		{
			denomSelectCoroutine?.Stop();
			menuTimeoutCoroutine?.Stop();
			autoUnregisterHelper.UnRegisterAll();
			DenomExpressions.DeInit();
		}

		private void OnDenomPlayableStatusChanged(StatusBlock sender, string propertyname)
		{
			OnIsMultiDenomChanged();
		}

		private void OnIsMultiDenomChanged()
		{
			if (!DenomExpressions.IsMultiDenom || StatusDatabase.GameFunctionStatus.GameButtonBehaviours.DenominationSelectionButtons == GameButtonStatus.Hidden)
				HideMenu();
		}

		private void OnCashoutConfirmPopupActive()
		{
			if (PopupStatus.IsCashoutConfirmOpen && denomStatus.DenomMenuState != DenomMenuState.WaitForChange)
				HideMenu();
		}

		private void OnInfoOpen()
		{
			if (PopupStatus.IsInfoOpen && denomStatus.DenomMenuState != DenomMenuState.WaitForChange)
				HideMenu();
		}

		private void OnAutoPlayStateChanged(StatusBlock block, string propertyName)
		{
			if (StatusDatabase.AutoPlayStatus.State != AutoPlayState.Idle && denomStatus.DenomMenuState != DenomMenuState.WaitForChange)
				HideMenu();
		}

		private void OnGameSetupPresentationDoneMessage(GameSetupPresentationDoneMessage msg)
		{
			GamePresentationTimings.ReportChangeDenomCancelled();
			HideMenu();
		}

		private void OnChangeGameDenomCancelledMessage(ChangeGameDenomCancelledMessage msg)
		{
			GamePresentationTimings.ReportChangeDenomCancelled();
			HideMenu();
		}

		public void ShowMenu()
		{
			if (DenomExpressions.VisibleDenoms.Count > 1)
			{
				if (denomStatus.DenomMenuState == DenomMenuState.Hidden)
					SendDenomActive(true);

				denomStatus.SetState(DenomMenuState.Attract);
			}
		}

		public void HideMenu()
		{
			if (denomStatus.DenomMenuState != DenomMenuState.Hidden)
				SendDenomActive(false);

			denomStatus.SetState(DenomMenuState.Hidden);
			denomStatus.SelectedDenom = StatusDatabase.ConfigurationStatus.DenomConfig.CurrentDenomination;
		}

		/// <summary>
		/// Selects a denom and puts the denom selection into confirm state.
		/// </summary>
		/// <param name="denom">The selected denom.</param>
		public void SelectGameDenom(Money denom)
		{
			if (DenomExpressions.VisibleDenoms.Any(vd => vd.Denom == denom)
				&& DenomExpressions.VisibleDenoms.Count > 1)
			{
				if (denomStatus.DenomMenuState == DenomMenuState.Hidden)
					SendDenomActive(true);

				// Make sure the popup is visible and select the denom.
				denomStatus.SetState(DenomMenuState.Confirm);
				denomStatus.SelectedDenom = denom;
				confirmTimeoutTime = FrameTime.CurrentTime + TimeSpan.FromSeconds(30);
			}
		}

		public void ChangeDenomImmediately(Money denom)
		{
			if (StatusDatabase.ConfigurationStatus.DenomConfig.CurrentDenomination == denom)
			{
				HideMenu();
				return;
			}

			Communication.ToLogicSender.Send(new ChangeGameDenomMessage(denom));
			GamePresentationTimings.ReportChangeDenomInProgress();
		}

		public void ConfirmGameDenom(bool confirm)
		{
			if (DenomExpressions.VisibleDenoms.Any(vd => vd.Denom == denomStatus.SelectedDenom))
			{
				if (confirm)
					ChangeGameDenom(denomStatus.SelectedDenom);
				else
					denomStatus.SetState(DenomMenuState.Attract);
			}
		}

		private IEnumerator<CoroutineInstruction> DenomSelectCoroutine()
		{
			while (StatusDatabase.ConfigurationStatus.DenomConfig?.CurrentDenomination == default)
				yield return null;

			denomStatus.SelectedDenom = StatusDatabase.ConfigurationStatus.DenomConfig.CurrentDenomination;

			while (true)
			{
				switch (denomStatus.DenomMenuState)
				{
					case DenomMenuState.Hidden:
						yield return new CoroutineRun(DoDenomHidden(), "Denom Hidden");
						break;

					case DenomMenuState.Attract:
						yield return new CoroutineRun(DoDenomAttract(), "Denom Attract");
						break;

					case DenomMenuState.Confirm:
						yield return new CoroutineRun(DoDenomConfirm(), "Denom Confirm");
						break;

					case DenomMenuState.WaitForChange:
						yield return null;
						break;
				}
			}

			// ReSharper disable once IteratorNeverReturns - By design
		}

		private IEnumerator<CoroutineInstruction> DoDenomHidden()
		{
			while (!IsAllowed())
			{
				if (denomStatus.DenomMenuState != DenomMenuState.Hidden)
					yield break;

				yield return null;
			}

			var normalTimeout = StatusDatabase.ConfigurationStatus.MachineConfig.IsShowMode ? TimeSpan.FromSeconds(60) : TimeSpan.FromSeconds(30);
			var timespan = GetTimeout();
			var endTime = FrameTime.CurrentTime + timespan;

			while (IsAllowed() && endTime - FrameTime.CurrentTime > TimeSpan.Zero)
			{
				if (StatusDatabase.GameStatus.GameLogicPaused || timespan != GetTimeout())
				{
					endTime = FrameTime.CurrentTime + GetTimeout();
					timespan = StatusDatabase.GameFunctionStatus.Timeout;
				}

				yield return null;
			}

			if (IsAllowed())
				ShowMenu();

			// ReSharper disable once IteratorNeverReturns - By design

			TimeSpan GetTimeout() => StatusDatabase.GameFunctionStatus.IsTimeoutActive ? StatusDatabase.GameFunctionStatus.Timeout : normalTimeout;
		}

		private bool IsAllowed()
		{
			return CheckGameState() && (CheckMcpStatus() || CheckNormalStatus() || CheckShowState());

			bool CheckGameState() => denomStatus.DenomMenuState == DenomMenuState.Hidden && StatusDatabase.GameStatus.CurrentGameState == GameState.Idle;
			bool CheckMcpStatus() => StatusDatabase.GameFunctionStatus.IsTimeoutActive;
			bool CheckNormalStatus() => !DashboardExpressions.IsAnyPopupOpen && !creditPlayoffStatus.IsAvailable && StatusDatabase.BankStatus.BankMeter < Money.FromCredit(GameStatus.TotalBet);
			bool CheckShowState() => StatusDatabase.ConfigurationStatus.MachineConfig.IsShowMode && !DashboardExpressions.IsAnyPopupOpen && !creditPlayoffStatus.IsAvailable;
		}

		private IEnumerator<CoroutineInstruction> DoDenomAttract()
		{
			var denomIndex = DenomExpressions.AllDenoms.FindIndex(vd => vd.Denom == denomStatus.SelectedDenom);
			var denomIndexInitialised = true;

			while (denomStatus.DenomMenuState == DenomMenuState.Attract)
			{
				if (StatusDatabase.GameStatus.GameLogicPaused || DenomExpressions.ActiveDenoms.Count == 0)
				{
					denomStatus.SelectedDenom = StatusDatabase.ConfigurationStatus.DenomConfig.CurrentDenomination;

					while (StatusDatabase.GameStatus.GameLogicPaused || DenomExpressions.ActiveDenoms.Count == 0)
						yield return null;

					denomIndex = DenomExpressions.AllDenoms.FindIndex(vd => vd.Denom == denomStatus.SelectedDenom);
					denomIndexInitialised = true;
					yield return null;
				}

				if (!denomIndexInitialised)
					denomIndex = NextIndex(denomIndex);

				denomIndexInitialised = false;

				denomStatus.SelectedDenom = DenomExpressions.AllDenoms[denomIndex].Denom;
				yield return new CoroutineDelayWithPredicate(TimeSpan.FromSeconds(2), () => denomStatus.DenomMenuState != DenomMenuState.Attract || StatusDatabase.GameStatus.GameLogicPaused ||
					DenomExpressions.ActiveDenoms.FindIndex(vd => vd.Denom == denomStatus.SelectedDenom) == -1);
			}

			int NextIndex(int index)
			{
				var allDenoms = DenomExpressions.AllDenoms;
				var activeDenoms = DenomExpressions.ActiveDenoms;
				do
				{
					index = (index + 1) % allDenoms.Count;
				} while (activeDenoms.FindIndex(Match(allDenoms, index)) == -1);

				return index;
			}

			Predicate<DenomPlayableStatus> Match(IReadOnlyList<DenomPlayableStatus> allDenoms, int newIndex) => vd => vd.Denom == allDenoms[newIndex].Denom;
		}

		private IEnumerator<CoroutineInstruction> DoDenomConfirm()
		{
			while (FrameTime.CurrentTime < confirmTimeoutTime && denomStatus.DenomMenuState == DenomMenuState.Confirm)
			{
				if (StatusDatabase.GameStatus.GameLogicPaused || DenomExpressions.ActiveDenoms.FindIndex(vd => vd.Denom == denomStatus.SelectedDenom) == -1)
					denomStatus.SetState(DenomMenuState.Attract);

				yield return null;
			}

			if (denomStatus.DenomMenuState == DenomMenuState.Confirm)
				ConfirmGameDenom(false);
		}

		private void ChangeGameDenom(Money denom)
		{
			if (StatusDatabase.ConfigurationStatus.DenomConfig.CurrentDenomination != denom)
			{
				denomStatus.SetState(DenomMenuState.WaitForChange);
				Communication.ToLogicSender.Send(new ChangeGameDenomMessage(denom));
				GamePresentationTimings.ReportChangeDenomInProgress();
			}
			else
			{
				HideMenu();
			}
		}

		private static void SendDenomActive(bool isActive)
		{
			Communication.ToLogicSender.Send(new RunTimeDenomSelectionMessage(isActive));
		}
	}
}