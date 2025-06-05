using System;
using System.Collections.Generic;
using System.Globalization;
using Midas.Core;
using Midas.Core.Coroutine;
using Midas.Core.StateMachine;
using Midas.Presentation.Data;
using Coroutine = Midas.Core.Coroutine.Coroutine;

namespace Midas.Presentation.Dashboard
{
	public sealed partial class DashboardController
	{
		private Coroutine macauConfigRoutine;
		private Coroutine macauFlashRoutine;
		private bool immediateUpdate;
		private CultureInfo currentCulture;

		private void InitMacau()
		{
			macauConfigRoutine = StateMachineService.FrameUpdateRoot.StartCoroutine(UpdateMacauInfo(), "UpdateMacauInfo");
			macauFlashRoutine = StateMachineService.FrameUpdateRoot.StartCoroutine(MacauClockFlashCoroutine(), "MacauClockFlash");
		}

		private void DeInitMacau()
		{
			macauConfigRoutine?.Stop();
			macauConfigRoutine = null;
			macauFlashRoutine?.Stop();
			macauFlashRoutine = null;
		}

		private IEnumerator<CoroutineInstruction> UpdateMacauInfo()
		{
			while (StatusDatabase.ConfigurationStatus.MachineConfig == null)
				yield return null;

			dashboardStatus.ShowMacauInfo = StatusDatabase.ConfigurationStatus.MachineConfig.Jurisdiction == "MCAU";
		}

		private IEnumerator<CoroutineInstruction> MacauClockFlashCoroutine()
		{
			while (StatusDatabase.ConfigurationStatus.FlashingPlayerClock == null)
				yield return null;

			while (StatusDatabase.ConfigurationStatus.MachineConfig == null)
				yield return null;

			if (StatusDatabase.ConfigurationStatus.MachineConfig.Jurisdiction != "MCAU")
				yield break;

			while (true)
			{
				while (!StatusDatabase.ConfigurationStatus.FlashingPlayerClock.FlashPlayerClockEnabled)
					yield return null;

				var isFlashing = false;
				var timesFlashed = 0;
				var flashTime = TimeSpan.Zero;
				DateTime? lastSessionTime = null;
				var lastActivityTime = DateTime.MinValue;

				currentCulture = new CultureInfo(StatusDatabase.ConfigurationStatus.CurrentLanguage);
				dashboardStatus.IsMacauClockVisible = true;
				while (StatusDatabase.ConfigurationStatus.FlashingPlayerClock.FlashPlayerClockEnabled)
				{
					if (StatusDatabase.ConfigurationStatus.CurrentLanguage != currentCulture.Name)
					{
						currentCulture = new CultureInfo(StatusDatabase.ConfigurationStatus.CurrentLanguage);
						immediateUpdate = true;
					}

					var elapsedTime = DateTime.Now - lastActivityTime;
					if (Math.Abs(elapsedTime.TotalMilliseconds) > 500 || immediateUpdate)
					{
						dashboardStatus.CurrentTime = CurrentTime();
						lastActivityTime = DateTime.Now;
						immediateUpdate = false;
					}

					if (!CheckForActiveSession())
					{
						lastSessionTime = null;
						isFlashing = false;
						yield return null;
					}

					if (StatusDatabase.GameStatus.GameLogicPaused)
					{
						dashboardStatus.IsMacauClockVisible = true;
						yield return null;
						continue;
					}

					if (!lastSessionTime.HasValue)
						StartFlashing();

					if (!isFlashing)
					{
						if (DateTime.Now - lastSessionTime >= StatusDatabase.ConfigurationStatus.FlashingPlayerClock.MinutesBetweenSequences)
							StartFlashing();
					}

					// Do the flash.

					if (isFlashing)
					{
						flashTime += FrameTime.DeltaTime;
						if (flashTime >= StatusDatabase.ConfigurationStatus.FlashingPlayerClock.FlashSequenceLength)
						{
							flashTime = TimeSpan.Zero;
							dashboardStatus.IsMacauClockVisible = !dashboardStatus.IsMacauClockVisible;
							timesFlashed++;
							if (timesFlashed >= StatusDatabase.ConfigurationStatus.FlashingPlayerClock.NumberOfFlashesPerSequence * 2)
							{
								isFlashing = false;
								dashboardStatus.IsMacauClockVisible = true;
							}
						}
					}

					yield return null;
				}

				dashboardStatus.IsMacauClockVisible = false;

				void StartFlashing()
				{
					flashTime = TimeSpan.Zero;
					isFlashing = true;
					lastSessionTime = DateTime.Now;
					timesFlashed = 0;
				}
			}

			bool CheckForActiveSession()
			{
				return StatusDatabase.ConfigurationStatus.FlashingPlayerClock.IsSessionActive;
			}

			string CurrentTime()
			{
				return DateTime.Now.ToLocalTime().ToString("hh:mm tt", currentCulture);
			}
		}
	}
}