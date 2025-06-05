using System.Collections;
using System.Collections.Generic;
using Midas.Presentation.Data.PropertyReference;
using UnityEngine;

namespace Midas.Presentation.Reels.SmartSymbols
{
	public sealed class SmartStopAnimationController : MonoBehaviour, IReelSpinStateEventHandler
	{
		private IReadOnlyList<(int Column, int Row)> currentSmartCells;
		private readonly List<IReelStopAnimation> reelStopAnimations = new List<IReelStopAnimation>();

		[SerializeField]
		private PropertyReference<IReadOnlyList<(int Column, int Row)>> smartCellsPropRef;

		[SerializeField]
		private ReelSpinStateEvent reelStateChangeGameEvent;

		private void OnEnable()
		{
			IsFinished = true;
			reelStateChangeGameEvent.Register(this);
		}

		private void OnDisable()
		{
			reelStateChangeGameEvent.Unregister(this);
			IsFinished = true;
		}

		private void OnDestroy()
		{
			smartCellsPropRef.DeInit();
		}

		public bool IsFinished { get; private set; }

		public void SpinStarted(ReelContainer container)
		{
			StopAllCoroutines();
			currentSmartCells = smartCellsPropRef.Value;
			IsFinished = false;
		}

		public void SpinSlammed(ReelContainer container)
		{
			// TODO
		}

		public void SpinFinished(ReelContainer container)
		{
			if (reelStopAnimations.Count > 0)
				StartCoroutine(Coroutine());
			else
				IsFinished = true;

			currentSmartCells = null;
		}

		public void ReelStateChanged(ReelContainer container, int reelGroupIndex, ReelSpinState spinState)
		{
			if (!IsFinished && spinState == ReelSpinState.Overshooting)
			{
				var reels = container.GetReelGroups()[reelGroupIndex].Reels;

				foreach (var cell in currentSmartCells)
				{
					foreach (var reel in reels)
					{
						if (IsCellOnReel(reel, cell))
						{
							var reelStopAnimation = reel.GetSymbol(cell.Row).GetComponentInChildren<IReelStopAnimation>();

							if (reelStopAnimation == null)
							{
								continue;
							}

							reelStopAnimation.PlayReelStopAnimation();
							reelStopAnimations.Add(reelStopAnimation);
						}
					}
				}
			}

			bool IsCellOnReel(Reel r, (int Column, int Row) c)
			{
				return c.Column == r.Column && c.Row >= r.Row && c.Row < r.Row + r.VisibleSymbols;
			}
		}

		public void ReelAnticipating(ReelContainer container, int reelGroupIndex) { }

		public void Interrupt(bool immediate)
		{
			StopAllCoroutines();

			// Only stop the Reel Stop Animations if they haven't finished;

			while (reelStopAnimations.Count > 0)
			{
				var reelStopAnimation = reelStopAnimations[0];

				if (!reelStopAnimation.IsReelStopAnimationFinished)
				{
					reelStopAnimation.StopReelStopAnimation();
				}

				reelStopAnimations.RemoveAt(0);
			}

			IsFinished = true;
		}

		private IEnumerator Coroutine()
		{
			IsFinished = false;

			// Wait for the smart symbol to stop animating.

			while (reelStopAnimations.Count > 0)
			{
				var reelStopAnimation = reelStopAnimations[0];

				while (!reelStopAnimation.IsReelStopAnimationFinished)
				{
					yield return null;
				}

				reelStopAnimations.RemoveAt(0);
			}

			// When all the Reel Stop Animations have finished remove them all from the list.

			IsFinished = true;
		}
	}
}