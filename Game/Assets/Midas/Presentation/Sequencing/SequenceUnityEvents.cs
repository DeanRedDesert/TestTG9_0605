using System.Collections.Generic;
using System.Threading.Tasks;
using Midas.Core.General;
using UnityEngine;
using UnityEngine.Events;

namespace Midas.Presentation.Sequencing
{
	/// <summary>
	/// Use this component to activate other components on sequence events.
	/// </summary>
	public class SequenceUnityEvents : SequenceActivatorBase
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
		private UnityEvent activateEvent = new UnityEvent();

		#endregion

		#region SequenceActivatorBase Overrides

		public override IReadOnlyList<string> SequenceNames => sequenceNames ??= new[] { sequenceEventEntry.SequenceName };

		protected override void RegisterSequenceEvents(AutoUnregisterHelper autoUnregisterHelper, Sequence sequence)
		{
			void OnEnter(SequenceEventArgs a, TaskCompletionSource<int> source)
			{
				source.SetResult(0);
				OnStateEnter(a);
			}

			autoUnregisterHelper.RegisterSequenceEventHandler(sequence, sequenceEventEntry.Event, new SequenceEventHandler(this, OnEnter, null, OnStateExit));
		}

		#endregion

		#region Private Methods

		private void OnStateEnter(SequenceEventArgs sequenceEventArgs)
		{
			if (sequenceEventArgs == null)
			{
				Finish();
				return;
			}

			if (fireOn == FireOnState.OnStateEnter)
			{
				FireEvents(sequenceEventArgs.Intensity);
			}
		}

		private void OnStateExit(SequenceEventArgs sequenceEventArgs)
		{
			if (sequenceEventArgs == null)
			{
				Finish();
				return;
			}

			if (fireOn == FireOnState.OnStateExit)
			{
				FireEvents(sequenceEventArgs.Intensity);
			}
		}

		private void FireEvents(int intensity)
		{
			if (requiredIntensity.HasIntensity(intensity))
				activateEvent.Invoke();
		}

		#endregion

		#region Editor

#if UNITY_EDITOR
		public void ConfigureForMakeGame(SequenceEvent sequenceEvent, params (UnityAction<bool> Action, bool Value)[] boolActions)
		{
			sequenceEventEntry = sequenceEvent;

			foreach (var action in boolActions)
				UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(activateEvent, action.Action, action.Value);
		}
#endif

		#endregion
	}
}