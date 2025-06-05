using System;
using System.Collections.Generic;
using Midas.Core;
using Midas.Core.Coroutine;
using Midas.Core.General;
using Midas.Core.StateMachine;
using Midas.Presentation.Game;
using Midas.Presentation.Sequencing;
using Midas.Presentation.Data;
using Midas.Presentation.Interruption;

namespace Midas.Presentation.WinPresentation
{
	public sealed class WinCountController : IPresentationController, ICompletionNotifier, IInterruptable
	{
		private Coroutine winCountCoroutine;
		private InterruptController interruptController;

		private bool startCounting;
		private bool isCounting;
		private bool interrupt;

		public event Action CountingStateChanged;
		public bool IsCounting => startCounting || isCounting;

		public void Reset(bool finalResult)
		{
			StatusDatabase.BankStatus.WinMeter = finalResult
				? WinPresentationStatus.WinMeterFinalValue
				: WinPresentationStatus.WinMeterInitialValue;

			isCounting = false;
		}

		public void StartWinCount()
		{
			startCounting = true;
		}

		private IEnumerator<CoroutineInstruction> WinCountCoroutine()
		{
			for (;;)
			{
				if (!isCounting)
				{
					CountingStateChanged?.Invoke();
					while (!startCounting)
						yield return null;

					startCounting = false;
					isCounting = true;
					interrupt = false;
				}
				else
				{
					interruptController.AddInterruptable(this);
					CountingStateChanged?.Invoke();
					var startValue = WinPresentationStatus.WinMeterInitialValue;
					var endValue = WinPresentationStatus.WinMeterFinalValue;
					var difference = endValue - startValue;
					var countTime = StatusDatabase.WinPresentationStatus.CountingTime;
					var countDelayTime = StatusDatabase.WinPresentationStatus.CountingDelayTime;
					var startTime = FrameTime.CurrentTime;

					if (countDelayTime != TimeSpan.Zero)
						yield return new CoroutineDelayWithPredicate(countDelayTime, () => interrupt);

					while (StatusDatabase.BankStatus.WinMeter < endValue && isCounting && !interrupt)
					{
						CountStep(startValue, difference, startTime, countTime);
						yield return null;
					}

					if (isCounting)
					{
						isCounting = false;
						interrupt = false;
						StatusDatabase.BankStatus.WinMeter = endValue;
						interruptController.RemoveInterruptable(this);

						Complete?.Invoke(this);
					}
				}
			}
			// ReSharper disable once IteratorNeverReturns - This is by design
		}

		private static void CountStep(Money startValue, Money difference, TimeSpan startTime, TimeSpan totalTime)
		{
			var duration = FrameTime.CurrentTime - startTime;
			if (duration < totalTime)
			{
				var percent = duration.Ticks / (double)totalTime.Ticks * TimeSpan.TicksPerMillisecond;
				var roundedPercent = new RationalNumber((int)percent, TimeSpan.TicksPerMillisecond);
				StatusDatabase.BankStatus.WinMeter = startValue + Money.FromRationalNumber(difference.Value * roundedPercent);
			}
			else
			{
				StatusDatabase.BankStatus.WinMeter = startValue + difference;
			}
		}

		#region IPresentationController Implementation

		void IPresentationController.Init()
		{
			interruptController = GameBase.GameInstance.GetInterface<InterruptController>();
			winCountCoroutine = StateMachineService.FrameUpdateRoot.StartCoroutine(WinCountCoroutine(), nameof(WinCountController));
		}

		void IPresentationController.DeInit()
		{
			winCountCoroutine.Stop();
			winCountCoroutine = null;
			interruptController.RemoveInterruptable(this);
			interruptController = null;
		}

		void IPresentationController.Destroy()
		{
		}

		#endregion

		#region IInterruptable Implementation

		public bool CanBeInterrupted => true;
		public bool CanBeAutoInterrupted => false;
		public int InterruptPriority => InterruptPriorities.Low;

		public void Interrupt()
		{
			if (isCounting)
				interrupt = true;
		}

		#endregion

		#region IResetProvider Implementation

		public event Action<ICompletionNotifier> Complete;

		#endregion
	}
}