using System.Collections.Generic;
using Midas.Core.Coroutine;
using Midas.Presentation.Cabinet;
using Midas.Presentation.Data;
using Midas.Presentation.Game;
using Coroutine = Midas.Core.Coroutine.Coroutine;

namespace Midas.Presentation.Gaff
{
	/// <summary>
	/// Used to allow a toggle the gaff menu status by request a cashout to the foundation. This will only toggle the gaff menu if the foundation allows for collect to toggle the gaff menu.
	/// </summary>
	public sealed class GaffToggleController : IPresentationController
	{
		private Coroutine gaffToggleCoroutine;

		public static bool IsTouched { get; set; }

		public void Init()
		{
			gaffToggleCoroutine = FrameUpdateService.Update.StartCoroutine(GaffToggleHandler());
		}

		public void DeInit()
		{
			gaffToggleCoroutine?.Stop();
			gaffToggleCoroutine = null;
		}

		public void Destroy()
		{
		}

		private IEnumerator<CoroutineInstruction> GaffToggleHandler()
		{
			while (StatusDatabase.ConfigurationStatus.MachineConfig == null)
				yield return null;

			// If we are not in a show or dev environment then leave.

			if (!StatusDatabase.ConfigurationStatus.MachineConfig.AreShowFeaturesEnabled)
			{
				gaffToggleCoroutine = null;
				yield break;
			}

			while (true)
			{
				if (IsTouched)
				{
					IsTouched = false;
					CabinetManager.Cabinet.RequestCashout();
				}

				yield return null;
			}
		}
	}
}