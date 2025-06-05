using System.Collections.Generic;
using Midas.Core.Coroutine;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Data;
using Midas.Presentation.Game;
using UnityEngine;
using Coroutine = Midas.Core.Coroutine.Coroutine;

namespace Midas.Presentation.Gaff
{
	public sealed class SpeedController : IPresentationController
	{
		private GaffStatus gaffStatus;
		private Coroutine speedCoroutine;
		private bool isActive;
		private bool requestReset;

		public void Init()
		{
			gaffStatus = StatusDatabase.GaffStatus;
			speedCoroutine = FrameUpdateService.Update.StartCoroutine(SpeedHandler());
		}

		public void DeInit()
		{
			speedCoroutine?.Stop();
			speedCoroutine = null;
			gaffStatus = null;
		}

		public void Destroy()
		{
		}

		private IEnumerator<CoroutineInstruction> SpeedHandler()
		{
			while (StatusDatabase.ConfigurationStatus.MachineConfig == null)
				yield return null;

			// If we are not in a show or dev environment then leave.

			if (!StatusDatabase.ConfigurationStatus.MachineConfig.AreShowFeaturesEnabled)
			{
				speedCoroutine = null;
				yield break;
			}

			while (true)
			{
				var targetSpeed = gaffStatus.GameTimeScale;

				if (gaffStatus.IsFeatureFastForwarding)
				{
					if (StatusDatabase.GameStatus.CurrentGameState == GameState.Idle)
						gaffStatus.IsFeatureFastForwarding = false;
					else
						targetSpeed = 10f;
				}

				if (targetSpeed > 1f)
				{
					// Slow down to half speed if the frams per second goes lower than 16, otherwise speed up to the target speed.

					var fps = 1f / Time.unscaledDeltaTime;

					if (fps < 16.0f)
					{
						var newTimeScale = Time.timeScale / 2;

						if (newTimeScale < 1f)
							newTimeScale = 1f;

						Time.timeScale = newTimeScale;
					}
					else if (Time.timeScale < targetSpeed)
						Time.timeScale += 0.1f;
				}
				else
					Time.timeScale = targetSpeed;

				yield return null;
			}
		}
	}
}