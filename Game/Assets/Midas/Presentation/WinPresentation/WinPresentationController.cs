using Midas.Core.General;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Data;
using Midas.Presentation.Game;
using Midas.Presentation.Interruption;

namespace Midas.Presentation.WinPresentation
{
	public class WinPresentationController : IPresentationController
	{
		private AutoUnregisterHelper autoUnregisterHelper;
		private WinPresentationStatus winPresentationStatus = StatusDatabase.WinPresentationStatus;
		private ButtonQueueController buttonQueueController;
		private InterruptController interruptController;

		#region IPresentationController Implementation

		public void Init()
		{
			autoUnregisterHelper ??= new AutoUnregisterHelper();
			interruptController = GameBase.GameInstance.GetPresentationController<InterruptController>();
			buttonQueueController = GameBase.GameInstance.GetPresentationController<ButtonQueueController>();

			buttonQueueController.AddHinderedChecker(BecauseInterruptableWinPresCheck);
			buttonQueueController.AddHindernessResolver(ButtonFunctionHinderedReasons.BecauseInterruptableWinPres, TryToResolveInterruptWinPresHinderness);
		}

		public void DeInit()
		{
			autoUnregisterHelper?.UnRegisterAll();
			buttonQueueController?.RemoveHinderedChecker(BecauseInterruptableWinPresCheck);
			buttonQueueController?.RemoveHindernessResolver(ButtonFunctionHinderedReasons.BecauseInterruptableWinPres, TryToResolveInterruptWinPresHinderness);

			buttonQueueController = null;
			interruptController = null;
		}

		public void Destroy()
		{
		}

		#endregion

		#region Methods

		private ButtonFunctionHinderedReasons BecauseInterruptableWinPresCheck(ButtonFunctionHinderedReasons hinderedReasons)
		{
			var result = ButtonFunctionHinderedReasons.None;
			if ((hinderedReasons & ButtonFunctionHinderedReasons.BecauseInterruptableWinPres) != ButtonFunctionHinderedReasons.None
				&& interruptController.IsAnythingToInterrupt && winPresentationStatus.WinPresActive)
			{
				result |= ButtonFunctionHinderedReasons.BecauseInterruptableWinPres;
			}

			return result;
		}

		private void TryToResolveInterruptWinPresHinderness()
		{
			if (winPresentationStatus.WinPresActive && interruptController.IsAnythingToInterrupt)
			{
				interruptController.Interrupt(false);
			}
		}

		#endregion
	}
}