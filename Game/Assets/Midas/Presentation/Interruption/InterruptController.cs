using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Game;

namespace Midas.Presentation.Interruption
{
	public sealed class InterruptController : IPresentationController
	{
		private readonly List<IInterruptable> interruptables = new List<IInterruptable>();
		private ButtonQueueController buttonQueueController;

		public event Action InterruptableStateChanged;
		public event Action<IInterruptable> OnInterrupt;

		public bool IsAnythingToInterrupt => interruptables.Count > 0;
		public bool IsAnythingToAutoInterrupt => interruptables.Any(i => i.CanBeInterrupted && i.CanBeAutoInterrupted);

		public void Init()
		{
			buttonQueueController = GameBase.GameInstance.GetPresentationController<ButtonQueueController>();
			buttonQueueController.AddHinderedChecker(BecauseInterruptableHinderedCheck);
			buttonQueueController.AddHindernessResolver(ButtonFunctionHinderedReasons.BecauseInterruptable, TryToResolveInterruptHinderness);
		}

		public void DeInit()
		{
			InterruptableStateChanged = null;
			OnInterrupt = null;
			buttonQueueController?.RemoveHinderedChecker(BecauseInterruptableHinderedCheck);
			buttonQueueController?.RemoveHindernessResolver(ButtonFunctionHinderedReasons.BecauseInterruptable, TryToResolveInterruptHinderness);
		}

		public void Destroy()
		{
		}

		public void AddInterruptable(IInterruptable interruptable)
		{
			if (!interruptables.Contains(interruptable))
			{
				interruptables.Add(interruptable);
				if (interruptables.Count == 1)
				{
					InterruptableStateChanged?.Invoke();
				}
			}
		}

		public void RemoveInterruptable(IInterruptable interruptable)
		{
			if (interruptables.Remove(interruptable) && interruptables.Count == 0)
			{
				InterruptableStateChanged?.Invoke();
			}
		}

		public void Interrupt(bool auto)
		{
			if (interruptables.Count == 0)
				return;

			var highestPriorityFound = FindHighestPriority();

			// we create a new array because in the interrupt method the object can remove itself
			var readyInterruptables = this.interruptables.Where(i => i.InterruptPriority == highestPriorityFound && i.CanBeInterrupted && (!auto || i.CanBeAutoInterrupted)).ToArray();
			foreach (var interruptable in readyInterruptables)
			{
				Log.Instance.Debug($"Interrupting {interruptable} from {readyInterruptables.Length}");
				interruptable.Interrupt();
				OnInterrupt?.Invoke(interruptable);
			}
		}

		private void TryToResolveInterruptHinderness()
		{
			if (IsAnythingToInterrupt)
			{
				Interrupt(false);
			}
		}

		private int FindHighestPriority()
		{
			return interruptables.Select(i => i.InterruptPriority).Max();
		}

		private ButtonFunctionHinderedReasons BecauseInterruptableHinderedCheck(ButtonFunctionHinderedReasons hinderedReasons)
		{
			var result = ButtonFunctionHinderedReasons.None;
			if ((hinderedReasons & ButtonFunctionHinderedReasons.BecauseInterruptable) != ButtonFunctionHinderedReasons.None
				&& IsAnythingToInterrupt)
			{
				result |= ButtonFunctionHinderedReasons.BecauseInterruptable;
			}

			return result;
		}
	}
}