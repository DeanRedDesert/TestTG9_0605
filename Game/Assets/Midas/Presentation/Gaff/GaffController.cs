using System;
using System.Collections.Generic;
using Midas.Core.Coroutine;
using Midas.Core.General;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Dashboard;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Game;
using UnityEngine;
using Coroutine = Midas.Core.Coroutine.Coroutine;

namespace Midas.Presentation.Gaff
{
	public sealed class GaffController : IPresentationController
	{
		private GaffStatus gaffStatus;
		private Coroutine dialUpKeyCoroutine;
		private Coroutine autoAddCreditsCoroutine;

		public void Init()
		{
			gaffStatus = StatusDatabase.GaffStatus;
			dialUpKeyCoroutine = FrameUpdateService.Update.StartCoroutine(HandleDialUpKey());
			autoAddCreditsCoroutine = FrameUpdateService.Update.StartCoroutine(HandleAutoAddCredits());
		}

		public void DeInit()
		{
			dialUpKeyCoroutine?.Stop();
			dialUpKeyCoroutine = null;
			autoAddCreditsCoroutine?.Stop();
			autoAddCreditsCoroutine = null;
			gaffStatus = null;
		}

		public void Destroy()
		{
		}

		private IEnumerator<CoroutineInstruction> HandleDialUpKey()
		{
			while (StatusDatabase.ConfigurationStatus.MachineConfig == null)
				yield return null;

			if (!StatusDatabase.ConfigurationStatus.MachineConfig.AreDevFeaturesEnabled)
			{
				dialUpKeyCoroutine = null;
				yield break;
			}

			while (true)
			{
				if (Input.GetKeyDown(KeyCode.Alpha7))
					gaffStatus.IsDialUpActive = !gaffStatus.IsDialUpActive;

				yield return null;
			}
		}

		private IEnumerator<CoroutineInstruction> HandleAutoAddCredits()
		{
			while (StatusDatabase.ConfigurationStatus.MachineConfig == null)
				yield return null;

			if (!StatusDatabase.ConfigurationStatus.MachineConfig.AreShowFeaturesEnabled ||
				StatusDatabase.ConfigurationStatus.MachineConfig.ShowMinimumCredits == Money.Zero)
			{
				autoAddCreditsCoroutine = null;
				yield break;
			}

			while (true)
			{
				while (!CashInExpected())
					yield return null;

				yield return new CoroutineDelayWithPredicate(GetCashInUrgency(), () => !CashInExpected());

				if (CashInExpected())
				{
					Communication.ToLogicSender.Send(new ShowAddMoneyMessage(StatusDatabase.ConfigurationStatus.MachineConfig.ShowMinimumCredits - StatusDatabase.BankStatus.BankMeter));
					yield return new CoroutineDelayWithPredicate(TimeSpan.FromSeconds(5), () => CashInExpected());
				}
			}

			bool CashInExpected() => StatusDatabase.GameStatus.GameIsIdle && !DashboardExpressions.IsAnyPopupOpen && StatusDatabase.BankStatus.BankMeter < StatusDatabase.ConfigurationStatus.MachineConfig.ShowMinimumCredits;

			TimeSpan GetCashInUrgency()
			{
				if (StatusDatabase.BankStatus.BankMeter < StatusDatabase.GameStatus.GameMinimumBet)
					return TimeSpan.FromSeconds(0.5);

				if (StatusDatabase.BankStatus.BankMeter < Money.FromCredit(GameStatus.TotalBet))
					return TimeSpan.FromSeconds(5);

				return TimeSpan.FromSeconds(10);
			}
		}
	}
}