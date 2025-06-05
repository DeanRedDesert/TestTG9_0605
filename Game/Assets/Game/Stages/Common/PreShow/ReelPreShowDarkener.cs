using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.Utility;
using Midas.Core.Coroutine;
using Midas.Gle.Presentation;
using Midas.Presentation;
using Midas.Presentation.Audio;
using Midas.Presentation.Data.PropertyReference;
using Midas.Presentation.Reels;
using Midas.Presentation.Sequencing;
using Midas.Presentation.Symbols;
using UnityEngine;
using Coroutine = Midas.Core.Coroutine.Coroutine;

namespace Game.Stages.Common.PreShow
{
	public sealed class ReelPreShowDarkener : MonoBehaviour, ISequencePlayable
	{
		private static readonly TimeSpan darkenTime = TimeSpan.FromSeconds(4);

		private Coroutine coroutine;
		private readonly List<IDarkener> currentDarkeners = new List<IDarkener>();
		private readonly List<IReelSymbolAnimation> currentAnimations = new List<IReelSymbolAnimation>();

		[SerializeField]
		private SoundPlayerBase bellSound;

		[SerializeField]
		private ReelContainer reelContainer;

		[SerializeField]
		private PropertyReference<IReadOnlyList<(int Column, int Row)>> highlightPositions;

		private void OnDestroy()
		{
			if (coroutine != null)
			{
				coroutine.Stop();
				coroutine = null;
			}

			highlightPositions?.DeInit();
		}

		public void StartPlay()
		{
			// Not sure if play while already playing should be an error

			if (coroutine != null)
				return;

			if (highlightPositions.Value == null)
				return;

			coroutine = FrameUpdateService.Update.StartCoroutine(RunPreShow());
		}

		public void StopPlay(bool reset)
		{
			coroutine.Stop();
			coroutine = null;

			StopAllCoroutines();
			foreach (var darkener in currentDarkeners)
				darkener.Idle(reset);

			foreach (var anim in currentAnimations)
				anim.Stop();

			if (reset)
				bellSound.ImmediateStop();
			else
				bellSound.Stop();
		}

		public bool IsPlaying() => coroutine != null;

		private IEnumerator<CoroutineInstruction> RunPreShow()
		{
			if (PreparePresentation() && (currentDarkeners.Count > 0 || currentAnimations.Count > 0))
			{
				bellSound.Play();
				foreach (var darkener in currentDarkeners)
					darkener.DarkenThenLighten(darkenTime);

				foreach (var anim in currentAnimations)
					anim.Play(null);

				yield return new CoroutineDelay(darkenTime);

				foreach (var anim in currentAnimations)
					anim.Stop();
			}

			coroutine = null;
		}

		private bool PreparePresentation()
		{
			currentDarkeners.Clear();
			currentAnimations.Clear();

			var triggerCells = highlightPositions?.Value;

			if (triggerCells == null)
				return false;

			foreach (var trigger in triggerCells)
			{
				var anim = reelContainer.GetSymbolByCell(trigger.Row, trigger.Column).GetComponent<IReelSymbolAnimation>();
				if (anim != null)
					currentAnimations.Add(anim);
			}

			foreach (var darkener in reelContainer.gameObject.GetComponentsInChildren<IDarkener>(true))
			{
				var sym = ((MonoBehaviour)darkener).GetComponent<ReelSymbol>();
				if (sym)
				{
					var index = reelContainer.Reels.IndexOf(sym.Reel);
					if (!triggerCells.Any(c => c.Column == index && c.Row == sym.IndexOnReel))
						currentDarkeners.Add(darkener);
				}
				else
					currentDarkeners.Add(darkener);
			}

			return true;
		}

#if UNITY_EDITOR

		public void ConfigureForMakeGame(SoundPlayerBase bell, ReelContainer reels, string propPath)
		{
			bellSound = bell;
			reelContainer = reels;
			highlightPositions = new PropertyReference<IReadOnlyList<(int Column, int Row)>>(propPath);
		}

#endif
	}
}