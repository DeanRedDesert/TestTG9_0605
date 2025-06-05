using System;
using System.Collections.Generic;
using Midas.Core.Coroutine;
using Midas.Core.General;
using Midas.Core.StateMachine;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Game;

namespace Midas.Presentation.WinPresentation
{
	public sealed class WinMeterResetController : IPresentationController
	{
		private enum State
		{
			WaitForGameStart,
			DelayBeforeReset
		}

		private readonly AutoUnregisterHelper autoUnregisterHelper = new AutoUnregisterHelper();
		private WinCountController winCountController;
		private Coroutine coroutine;
		private bool newGameStarted;

		void IPresentationController.Init()
		{
			winCountController = GameBase.GameInstance.GetPresentationController<WinCountController>();
			coroutine = StateMachineService.FrameUpdateRoot.StartCoroutine(WinMeterResetCoroutine, State.WaitForGameStart, nameof(WinMeterResetController));
			autoUnregisterHelper.RegisterPropertyChangedHandler<bool>(StatusDatabase.GameStatus, nameof(GameStatus.LogicGameActive), OnLogicGameActiveChanged);

			void OnLogicGameActiveChanged(StatusBlock sender, string propertyName, bool newValue, bool oldValue)
			{
				if (newValue)
					newGameStarted = true;
			}
		}

		void IPresentationController.DeInit()
		{
			autoUnregisterHelper.UnRegisterAll();
			coroutine?.Stop();
			coroutine = null;
			winCountController = null;
		}

		void IPresentationController.Destroy()
		{
		}

		private IEnumerator<CoroutineInstruction> WinMeterResetCoroutine(IStateInfo<State> stateInfo)
		{
			while (true)
			{
				while (!newGameStarted)
					yield return null;

				newGameStarted = false;

				if (StatusDatabase.BankStatus.WinMeter == Money.Zero)
					continue;

				if (StatusDatabase.WinPresentationStatus.WinMeterResetTimeout != TimeSpan.Zero)
				{
					yield return stateInfo.SetNextState(State.DelayBeforeReset);
					yield return new CoroutineDelayWithPredicate(StatusDatabase.WinPresentationStatus.WinMeterResetTimeout, () => winCountController.IsCounting);

					// If win count starts again, we don't reset the win meter.

					if (winCountController.IsCounting)
						continue;
				}

				StatusDatabase.BankStatus.WinMeter = Money.Zero;
				yield return stateInfo.SetNextState(State.WaitForGameStart);
			}

			// ReSharper disable once IteratorNeverReturns
		}
	}
}