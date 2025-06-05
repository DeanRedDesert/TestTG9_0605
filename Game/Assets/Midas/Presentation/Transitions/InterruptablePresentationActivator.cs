using System.Collections.Generic;
using System.Threading.Tasks;
using Midas.Core.General;
using Midas.Presentation.Sequencing;
using UnityEngine;

namespace Midas.Presentation.Transitions
{
	public sealed class InterruptablePresentationActivator : SequenceActivatorBase
	{
		private IReadOnlyList<string> sequenceNames;

		[SerializeField]
		private SequenceEvent sequenceEventEntry;

		[SerializeField]
		private InterruptablePresentation presentation;

		public override IReadOnlyList<string> SequenceNames => sequenceNames ??= new[] { sequenceEventEntry.SequenceName };

		protected override void RegisterSequenceEvents(AutoUnregisterHelper autoUnregisterHelper, Sequence sequence)
		{
			autoUnregisterHelper.RegisterSequenceEventHandler(sequence, sequenceEventEntry.Event, new SequenceEventHandler(this, OnEnter, OnExecute, onPause: OnPause));

			void OnEnter(SequenceEventArgs a, TaskCompletionSource<int> awaiter)
			{
				SetAsDependency(awaiter);
				OnStateEnter(a);
			}
		}

		private void OnStateEnter(SequenceEventArgs sequenceEventArgs)
		{
			if (sequenceEventArgs == null)
			{
				Finish();
				return;
			}

			presentation.Show();
		}

		private void OnExecute(SequenceEventArgs sequenceEventArgs)
		{
			if (presentation.IsFinished)
				Finish();
		}

		private void OnPause(SequenceEventArgs sequenceEventArgs, bool pause)
		{
			if (sequenceEventArgs == null)
				return;

			if (pause)
				presentation.Interrupt();
			else
				presentation.Show();
		}

		#region Editor

#if UNITY_EDITOR
		public void ConfigureForMakeGame(SequenceEvent sequenceEvent)
		{
			sequenceEventEntry = sequenceEvent;
		}
#endif

		#endregion
	}
}