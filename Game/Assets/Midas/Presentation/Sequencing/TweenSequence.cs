using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Midas.Core.General;
using Midas.Presentation.Tween;
using UnityEngine;

namespace Midas.Presentation.Sequencing
{
	public sealed class TweenSequence : SequenceActivatorBase
	{
		// ReSharper disable once MemberCanBePrivate.Global - Used in the inspector
		public enum FireOnState
		{
			OnStateEnter,
			OnStateExit
		}

		private IReadOnlyList<string> sequenceNames;

		#region Inspector Fields

		[SerializeField]
		private SequenceEvent sequenceEventEntry;

		[SerializeField]
		private FireOnState fireOn = FireOnState.OnStateEnter;

		[Space(12)]
		[SerializeField]
		private RequiredIntensity requiredIntensity = RequiredIntensity.NoIntensity;

		[Space(12)]
		[SerializeField]
		private List<TweenAnimationAndClip> tweenAnimationsList = new List<TweenAnimationAndClip>();

		#endregion

		#region SequenceActivatorBase Overrides

		public override IReadOnlyList<string> SequenceNames => sequenceNames ??= new[] { sequenceEventEntry.SequenceName };

		protected override void RegisterSequenceEvents(AutoUnregisterHelper autoUnregisterHelper, Sequence sequence)
		{
			autoUnregisterHelper.RegisterSequenceEventHandler(sequence, sequenceEventEntry.Event, new SequenceEventHandler(this, OnEnter, OnExecute, OnStateExit, onPause: OnPause));

			void OnEnter(SequenceEventArgs a, TaskCompletionSource<int> source)
			{
				this.source = source;
				OnStateEnter(a);
			}
		}

		private bool isEnabled;
		private TaskCompletionSource<int> source;

		private void OnExecute(SequenceEventArgs obj)
		{
			isEnabled = tweenAnimationsList.Any(t => t.TweenAnimation.IsPlaying());
			if (!isEnabled)
				source.SetResult(0);
		}

		#endregion

		#region Private Methods

		private void OnStateEnter(SequenceEventArgs sequenceEventArgs)
		{
			isEnabled = true;

			if (sequenceEventArgs == null)
			{
				Finish();
				return;
			}

			if (fireOn == FireOnState.OnStateEnter)
				FireEvents(sequenceEventArgs);
		}

		private void OnStateExit(SequenceEventArgs sequenceEventArgs)
		{
			if (sequenceEventArgs == null)
			{
				Finish();
				return;
			}

			if (fireOn == FireOnState.OnStateExit)
				FireEvents(sequenceEventArgs);
		}

		private void FireEvents(SequenceEventArgs sequenceEventArgs)
		{
			if (requiredIntensity.HasIntensity(sequenceEventArgs.Intensity))
				tweenAnimationsList.ForEach(t => t.TweenAnimation.Play(t.ClipName, sequenceEventArgs));
		}

		private void OnPause(SequenceEventArgs sequenceEventArgs, bool pause)
		{
			if (sequenceEventArgs == null)
				return;

			if (pause)
				tweenAnimationsList.ForEach(t => t.TweenAnimation.Interrupt(t.ClipName, sequenceEventArgs));
			else
				tweenAnimationsList.ForEach(t => t.TweenAnimation.Play(t.ClipName, sequenceEventArgs));
		}

		#endregion
	}
}