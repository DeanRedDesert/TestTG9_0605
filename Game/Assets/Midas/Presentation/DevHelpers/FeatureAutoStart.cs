using System.Collections.Generic;
using Midas.Core.Coroutine;
using Midas.Presentation.Data;
using Midas.Presentation.Game;
using Midas.Presentation.Interruption;

namespace Midas.Presentation.DevHelpers
{
	public sealed class FeatureAutoStart : IPresentationController
	{
		private Coroutine featureAutoStartCoroutine;

		public void Init() => featureAutoStartCoroutine = FrameUpdateService.Update.StartCoroutine(AutoStartCoroutine());

		public void DeInit()
		{
			featureAutoStartCoroutine?.Stop();
			featureAutoStartCoroutine = null;
		}

		public void Destroy()
		{
		}

		private IEnumerator<CoroutineInstruction> AutoStartCoroutine()
		{
			while (StatusDatabase.ConfigurationStatus.MachineConfig == null)
				yield return null;

			var machineConfig = StatusDatabase.ConfigurationStatus.MachineConfig;
			if (!(machineConfig.AreDevFeaturesEnabled || machineConfig.AreShowFeaturesEnabled))
			{
				featureAutoStartCoroutine = null;
				yield break;
			}

			var cachedInterruptController = GameBase.GameInstance.GetPresentationController<InterruptController>();
			while (true)
			{
				yield return null;
				if (!StatusDatabase.ConfigurationStatus.IsFeatureAutoStartEnabled)
					continue;

				if (!cachedInterruptController.IsAnythingToAutoInterrupt)
					continue;

				cachedInterruptController.Interrupt(true);
			}
		}
	}
}