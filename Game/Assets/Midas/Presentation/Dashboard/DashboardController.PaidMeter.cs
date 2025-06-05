using System;
using System.Collections.Generic;
using Midas.Core;
using Midas.Core.Coroutine;
using Midas.Core.General;
using Midas.Presentation.Data;
using Midas.Presentation.GameIdentity;
using Coroutine = Midas.Core.Coroutine.Coroutine;

namespace Midas.Presentation.Dashboard
{
	public sealed partial class DashboardController
	{
		private Coroutine paidCoroutine;

		private void InitPaidMeter()
		{
			paidCoroutine = FrameUpdateService.Update.StartCoroutine(PaidCoroutine());
		}

		private void DeInitPaidMeter()
		{
			paidCoroutine?.Stop();
			paidCoroutine = null;
		}

		private static IEnumerator<CoroutineInstruction> PaidCoroutine()
		{
			while (!StatusDatabase.ConfigurationStatus.GameIdentity.HasValue || StatusDatabase.BankStatus == null)
				yield return null;

			if (!StatusDatabase.ConfigurationStatus.GameIdentity.Value.IsGlobalGi())
				yield break;

			var waitTime = TimeSpan.FromSeconds(1);
			var toggleTime = FrameTime.CurrentTime + waitTime;
			var isIdle = true;
			while (true)
			{
				while (StatusDatabase.BankStatus.PaidMeter == Money.Zero || !StatusDatabase.GameStatus.GameIsIdle || StatusDatabase.GameStatus.GameLogicPaused)
				{
					toggleTime = FrameTime.CurrentTime + waitTime;
					if (!isIdle)
					{
						isIdle = true;
						UpdateMeters(true);
					}

					yield return null;
				}

				isIdle = false;
				if (toggleTime - FrameTime.CurrentTime < TimeSpan.Zero)
				{
					UpdateMeters(StatusDatabase.BankStatus.WinMeter != Money.Zero && !StatusDatabase.DashboardStatus.ShowWinMeter);
					toggleTime = FrameTime.CurrentTime + waitTime;
				}

				yield return null;
			}

			void UpdateMeters(bool showWinMeter)
			{
				StatusDatabase.DashboardStatus.ShowWinMeter = showWinMeter;
				StatusDatabase.DashboardStatus.ShowPaidMeter = !showWinMeter;
			}
		}
	}
}