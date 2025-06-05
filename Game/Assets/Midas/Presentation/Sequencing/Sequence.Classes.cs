using System.Collections.Generic;
using System.Threading.Tasks;
using Midas.Core.StateMachine;

namespace Midas.Presentation.Sequencing
{
	public abstract partial class Sequence
	{
		/// <summary>
		/// Used in the state machine to connect a state to a sequence event.
		/// </summary>
		protected sealed class SequenceStateArgs : StateArgs
		{
			public SequenceStateArgs(int sequenceEventId)
			{
				SequenceEventId = sequenceEventId;
			}

			public int SequenceEventId { get; }

			public bool IsEligible { get; set; }

			public SequenceEventArgs CurrentSequenceEventArgs { get; set; }
		}

		private sealed class EventRegistration
		{
			public EventRegistration(int handle, SequenceEventHandler handler)
			{
				Handle = handle;
				Handler = handler;
				WasEnabledAtEnter = false;
			}

			public int Handle { get; }
			public bool WasEnabledAtEnter { get; set; }
			public SequenceEventHandler Handler { get; }
		}

		private sealed class EventData
		{
			public EventData((string eventName, int eventId ) eventPair)
			{
				Awaiters = new EventAwaiters();
				EventRegistrations = new List<EventRegistration>();
				EventPair = eventPair;
			}

			public EventAwaiters Awaiters { get; set; }
			public List<EventRegistration> EventRegistrations { get; set; }
			public (string eventName, int eventId) EventPair { get; }
		}

		private sealed class EventAwaiters
		{
			public bool CheckIfCompleted()
			{
				var complete = true;
				for (var i = 0; i < Awaiters.Count; ++i)
				{
					if (!Awaiters[i].task.IsCompleted)
					{
						complete = false;
						break;
					}
				}

				if (complete)
				{
					Awaiters.Clear();
				}

				return complete;
			}

			public List<((string eventName, int eventId) eventPair, object owner, Task<int> task)> Awaiters { get; set; } =
				new List<((string eventName, int eventId) eventPair, object owner, Task<int> task)>();
		}
	}
}