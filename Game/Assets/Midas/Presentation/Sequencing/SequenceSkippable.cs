using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Midas.Core.General;
using Midas.Presentation.ExtensionMethods;
using Midas.Presentation.Game;
using Midas.Presentation.Interruption;
using UnityEngine;

namespace Midas.Presentation.Sequencing
{
	public abstract class SequenceSkippable : SequenceActivatorBase, IInterruptable
	{
		//sequence it, handle
		private readonly List<(Sequence sequence, string eventName, int handle)> eventHandles = new List<(Sequence, string, int)>();
		private SequenceEventArgs sequenceEventArgs;
		private bool interrupted;
		private bool hasFinished;
		private bool hasActivated;

		[SerializeField]
		private SequenceEventPair sequenceEventPair;

		[Space(12)]
		[SerializeField, Tooltip("If enabled, then interrupts are captured but not handled")]
		private bool swallowInterrupt;

		[SerializeField, Tooltip("Has no effect if SwallowInterrupt is enabled")]
		private bool canBeInterrupted;

		[SerializeField, Tooltip("Used for Interrupt and SwallowInterrupt")]
		private int interruptPrio = InterruptPriorities.Default + 50;

		[Space(12)]
		[SerializeField, Tooltip("Any integer value needed to fire the events. -1 means intensity is ignored")]
		private RequiredIntensity requiredIntensity = RequiredIntensity.NoIntensity; //valid is -1,1,2,4,5 where -1=>ignore

		[SerializeField]
		private bool canBeAutoInterrupted;

		protected abstract bool IsInterruptibleSequencePlaying { get; }

		public bool CanBeInterrupted => true;
		public bool CanBeAutoInterrupted => canBeAutoInterrupted;

		public int InterruptPriority => interruptPrio;

		public virtual void Interrupt()
		{
			if (!swallowInterrupt)
			{
				GameBase.GameInstance.GetPresentationController<InterruptController>().RemoveInterruptable(this);
				interrupted = true;
			}
		}

		public override IReadOnlyList<string> SequenceNames => sequenceEventPair.SequenceName == null ? Array.Empty<string>() : new[] { sequenceEventPair.SequenceName };

		protected virtual void OnStateEnter(SequenceEventArgs sequenceEventArgs, TaskCompletionSource<int> completionSource)
		{
			this.sequenceEventArgs = sequenceEventArgs;
			hasActivated = true;
			hasFinished = false;
			interrupted = false;
			completionSource.SetResult(0);

			if (this.sequenceEventArgs == null)
			{
				Finish();
			}

			if (canBeInterrupted || swallowInterrupt)
			{
				GameBase.GameInstance.GetPresentationController<InterruptController>().AddInterruptable(this);
			}

			if (HasNeededIntensity(this.sequenceEventArgs))
			{
				PlaySkippableSequence(this.sequenceEventArgs);
			}
		}

		protected virtual void OnStateExit(SequenceEventArgs sequenceEventArgs)
		{
			this.sequenceEventArgs = sequenceEventArgs;
			if (this.sequenceEventArgs == null)
			{
				Finish();
			}
		}

		protected virtual void OnWaitStateExecute(SequenceEventArgs sequenceEventArgs)
		{
			if (this.sequenceEventArgs == null)
			{
				Finish();
				return;
			}

			if (!hasActivated)
			{
				Log.Instance.Error($"_hasActivated=false in {this.GetPath()}?");
			}

			if (!hasFinished)
			{
				if (interrupted)
				{
					StopSkippableSequence(this.sequenceEventArgs);
				}

				if (!IsInterruptibleSequencePlaying)
				{
					Finish();
					GameBase.GameInstance.GetPresentationController<InterruptController>().RemoveInterruptable(this);
					hasFinished = true;
					this.sequenceEventArgs = null;
				}
			}
		}

		protected abstract void PlaySkippableSequence(SequenceEventArgs sequenceEventArgs);
		protected abstract void StopSkippableSequence(SequenceEventArgs sequenceEventArgs);

		protected override void RegisterSequenceEvents(AutoUnregisterHelper autoUnregisterHelper, Sequence sequence)
		{
			//attach start
			var handle = sequence.RegisterEventHandler(sequenceEventPair.EventA,
				new SequenceEventHandler(this,
					OnStateEnter,
					null,
					OnStateExit,
					() => isActiveAndEnabled));
			eventHandles.Add((sequence, sequenceEventPair.EventA, handle));

			//attach awaiters
			handle = sequence.RegisterEventHandler(sequenceEventPair.EventB,
				new SequenceEventHandler(this,
					(_, promise) => SetAsDependency(promise),
					OnWaitStateExecute,
					null,
					() => isActiveAndEnabled));
			eventHandles.Add((sequence, sequenceEventPair.EventB, handle));
		}

		protected override void OnDisable()
		{
			foreach (var (sequence, eventId, handle) in eventHandles)
			{
				sequence.UnRegisterEventHandler(eventId, handle);
			}

			base.OnDisable();
		}

		protected virtual bool HasNeededIntensity(SequenceEventArgs eventArgs)
		{
			return requiredIntensity.HasIntensity(eventArgs.Intensity);
		}

		#region Editor

#if UNITY_EDITOR
		public void ConfigureForMakeGame(SequenceEventPair sequenceEvent)
		{
			sequenceEventPair = sequenceEvent;
		}

		public void ConfigureForMakeGame(string seqName, Enum activateEvent, Enum blockEvent)
		{
			sequenceEventPair = new SequenceEventPair(seqName, activateEvent.ToString(), blockEvent.ToString());
		}
#endif

		#endregion
	}
}