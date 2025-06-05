using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.Core.Coroutine;
using Midas.Core.General;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Game;
using Midas.Presentation.GameIdentity;

namespace Midas.Presentation.Dashboard
{
	public sealed class PlayerSessionController : IPresentationController
	{
		private readonly AutoUnregisterHelper autoUnregisterHelper = new AutoUnregisterHelper();
		private Coroutine idleCoroutine;

		public void Init()
		{
			autoUnregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.PlayerSessionStatus, nameof(PlayerSessionStatus.PlayerSessionParameters), Handler);
			idleCoroutine = FrameUpdateService.Update.StartCoroutine(IdleCoroutine());
		}

		public void DeInit()
		{
			autoUnregisterHelper.UnRegisterAll();
			idleCoroutine?.Stop();
		}

		public void Destroy()
		{
		}

		private void Handler(StatusBlock sender, string propertyname)
		{
			var psp = StatusDatabase.PlayerSessionStatus.PlayerSessionParameters;

			if (!IsResetRequired(psp))
				return;

			Log.Instance.Info($"PlayerSessionStatus.ParameterResetEnabled={psp.IsPlayerSessionParameterResetEnabled}");
			Log.Instance.Info($"PlayerSessionStatusPendingResetParams={string.Join(",", psp.PendingParametersToReset)}");

			var playerSessionInstances = GameBase.GameInstance.GetInterfaces<IPlayerSessionReset>();
			var pending = StatusDatabase.PlayerSessionStatus.PlayerSessionParameters.PendingParametersToReset;

			var resetDoneParams = new List<PlayerSessionParameterType>();
			foreach (var playerSessionInstance in playerSessionInstances)
				playerSessionInstance.ResetForNewPlayerSession(psp.PendingParametersToReset, resetDoneParams);

			// ThemeSpecific is always added if requested, this is because most games don't have anything themeSpecific, and then we would never report that parameter as being reset.
			if (pending.Contains(PlayerSessionParameterType.ThemeSpecific) && !resetDoneParams.Contains(PlayerSessionParameterType.ThemeSpecific))
				resetDoneParams.Add(PlayerSessionParameterType.ThemeSpecific);

			if (resetDoneParams.Count > 0)
			{
				Log.Instance.Info($"resetDoneParams={string.Join(",", resetDoneParams)}");
				Communication.ToLogicSender.Send(new PlayerSessionReportParametersBeingResetMessage(resetDoneParams));
			}

			bool IsResetRequired(PlayerSessionParameters p) => p.IsPlayerSessionParameterResetEnabled && p.PendingParametersToReset.Count > 0;
		}

		private IEnumerator<CoroutineInstruction> IdleCoroutine()
		{
			while (!StatusDatabase.ConfigurationStatus.GameIdentity.HasValue || StatusDatabase.PlayerSessionStatus.PlayerSession == null || StatusDatabase.GameStatus.CurrentGameState == null)
				yield return null;

			if (StatusDatabase.ConfigurationStatus.GameIdentity.Value.IsGlobalGi())
				yield break;

			var creditPlayoffStatus = StatusDatabase.QueryStatusBlock<CreditPlayoffStatusBase>();

			if (StatusDatabase.ConfigurationStatus.MachineConfig.IsShowMode)
			{
				yield return new CoroutineRun(ShowMode());
				yield break;
			}

			yield return new CoroutineRun(NormalMode(creditPlayoffStatus));
		}

		private static IEnumerator<CoroutineInstruction> NormalMode(CreditPlayoffStatusBase creditPlayoffStatus)
		{
			var inSession = InSession();
			if (inSession)
				Communication.ToLogicSender.Send(new PlayerSessionActiveMessage(true));

			while (true)
			{
				var timespan = TimeSpan.FromSeconds(30);

				if (inSession)
				{
					while (InSession())
						yield return null;

					var endTime = FrameTime.CurrentTime + timespan;
					do
					{
						if (StatusDatabase.GameStatus.GameLogicPaused)
							endTime = FrameTime.CurrentTime + timespan;

						if (endTime - FrameTime.CurrentTime < TimeSpan.Zero)
						{
							Communication.ToLogicSender.Send(new PlayerSessionActiveMessage(false));

							var p = new[] { PlayerSessionParameterType.PlayerVolume, PlayerSessionParameterType.CreditDisplay, PlayerSessionParameterType.BetSelection };
							Log.Instance.Info($"resetParamsToDo={string.Join(",", p)}");
							Communication.ToLogicSender.Send(new PlayerSessionInitiateResetMessage(p));
							inSession = false;
						}

						yield return null;
					} while (!InSession() && inSession);
				}
				else
				{
					while (!InSession())
						yield return null;

					Communication.ToLogicSender.Send(new PlayerSessionActiveMessage(true));
					inSession = true;
				}

				yield return null;
			}

			bool InSession() => StatusDatabase.GameStatus.CurrentGameState != GameState.Idle || creditPlayoffStatus.IsAvailable || StatusDatabase.BankStatus.BankMeter >= Money.FromCredit(GameStatus.TotalBet);
		}

		private static IEnumerator<CoroutineInstruction> ShowMode()
		{
			var lastDenom = StatusDatabase.ConfigurationStatus.DenomConfig.CurrentDenomination;
			var timespan = TimeSpan.FromSeconds(60);
			var endTime = FrameTime.CurrentTime + timespan;

			Communication.ToLogicSender.Send(new PlayerSessionActiveMessage(true));
			while (true)
			{
				while (StatusDatabase.GameStatus.CurrentGameState != GameState.Idle)
				{
					endTime = FrameTime.CurrentTime + timespan;
					yield return null;
				}

				if (StatusDatabase.GameStatus.GameLogicPaused)
					endTime = FrameTime.CurrentTime + timespan;

				if (endTime - FrameTime.CurrentTime < TimeSpan.Zero || lastDenom != StatusDatabase.ConfigurationStatus.DenomConfig.CurrentDenomination)
				{
					Communication.ToLogicSender.Send(new PlayerSessionActiveMessage(false));

					var p = new[] { PlayerSessionParameterType.PlayerVolume, PlayerSessionParameterType.CreditDisplay, PlayerSessionParameterType.BetSelection };
					Log.Instance.Info($"resetParamsToDo={string.Join(",", p)}");
					Communication.ToLogicSender.Send(new PlayerSessionInitiateResetMessage(p));
					lastDenom = Money.Denomination;
					endTime = FrameTime.CurrentTime + timespan;

					yield return null;
					Communication.ToLogicSender.Send(new PlayerSessionActiveMessage(true));
				}

				yield return null;
			}
		}
	}
}