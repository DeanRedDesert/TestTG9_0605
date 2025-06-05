using System.Collections.Generic;
using UnityEngine;

namespace Midas.Presentation.Reels
{
	public interface IReelSpinStateEventHandler
	{
		void SpinStarted(ReelContainer container);
		void SpinSlammed(ReelContainer container);
		void SpinFinished(ReelContainer container);
		void ReelStateChanged(ReelContainer container, int reelGroupIndex, ReelSpinState spinState);
		void ReelAnticipating(ReelContainer container, int reelGroupIndex);

		void Interrupt(bool immediate);
		bool IsFinished { get; }
	}

	[CreateAssetMenu(menuName = "Midas/Reels/ReelSpinStateEvent")]
	public sealed class ReelSpinStateEvent : ScriptableObject
	{
		private readonly List<IReelSpinStateEventHandler> handlers = new List<IReelSpinStateEventHandler>();

		public void SpinStarted(ReelContainer container)
		{
			for (var i = handlers.Count - 1; i >= 0; i--)
				handlers[i].SpinStarted(container);
		}

		public void SpinSlammed(ReelContainer container)
		{
			for (var i = handlers.Count - 1; i >= 0; i--)
				handlers[i].SpinSlammed(container);
		}

		public void SpinFinished(ReelContainer container)
		{
			for (var i = handlers.Count - 1; i >= 0; i--)
				handlers[i].SpinFinished(container);
		}

		public void ReelStateChanged(ReelContainer container, int reelGroupIndex, ReelSpinState spinState)
		{
			for (var i = handlers.Count - 1; i >= 0; i--)
				handlers[i].ReelStateChanged(container, reelGroupIndex, spinState);
		}

		public void ReelAnticipating(ReelContainer container, int reelGroupIndex)
		{
			for (var i = handlers.Count - 1; i >= 0; i--)
				handlers[i].ReelAnticipating(container, reelGroupIndex);
		}

		public bool AreAllFinished()
		{
			foreach (var handler in handlers)
				if (!handler.IsFinished)
					return false;

			return true;
		}

		public void Interrupt(bool immediate)
		{
			for (var i = handlers.Count - 1; i >= 0; i--)
				handlers[i].Interrupt(immediate);
		}

		public void Register(IReelSpinStateEventHandler handler)
		{
			handlers.Add(handler);
		}

		public void Unregister(IReelSpinStateEventHandler handler)
		{
			handlers.Remove(handler);
		}
	}
}