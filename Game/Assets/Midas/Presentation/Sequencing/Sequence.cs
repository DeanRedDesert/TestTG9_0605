using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Midas.Core.StateMachine;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;

namespace Midas.Presentation.Sequencing
{
	/// <summary>
	/// Base class for a sequence.
	/// </summary>
	public abstract partial class Sequence : ISequenceDebug
	{
		#region Fields

		private readonly State idleState = new State(nameof(idleState), true);
		private readonly State finishedState = new State(nameof(finishedState));
		private readonly string stateMachinePrefix;
		private State[] states;
		private int handleCounter;
		private readonly Dictionary<string, int> eventNameToEventId = new Dictionary<string, int>();
		private readonly Dictionary<int, EventData> eventMap = new Dictionary<int, EventData>();
		private StateMachine stateMachine;

		#endregion

		#region Events

		public event Action<bool> PauseSequence;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the name of the sequence
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets whether the sequence is currently active.
		/// </summary>
		public bool IsActive { get; private set; }

		public bool IsPaused { get; private set; }

		/// <summary>
		/// Gets the list of event IDs that this sequence uses.
		/// </summary>
		public IReadOnlyList<(string eventName, int eventId)> SequenceEventIds { get; private set; }

		#endregion

		#region Methods

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">The name of the sequence.</param>
		/// <param name="stateMachinePrefix">Optional prefix to give to the sequence state machine.</param>
		protected Sequence(string name, string stateMachinePrefix = null)
		{
			Name = name;
			this.stateMachinePrefix = stateMachinePrefix;
		}

		/// <summary>
		/// Initialise the sequence.
		/// </summary>
		public virtual void Init()
		{
			SequenceEventIds = GenerateSequenceEventIds();

			foreach (var seid in SequenceEventIds)
				eventNameToEventId.Add(seid.eventName, seid.eventId);

			BuildEventStateMachine();
			StatusDatabase.GameStatus.AddPropertyChangedHandler(nameof(GameStatus.GameLogicPaused), PauseHandler);
		}

		private void PauseHandler(StatusBlock sender, string propertyname)
		{
			Pause(StatusDatabase.GameStatus.GameLogicPaused);
		}

		/// <summary>
		/// Clean up the sequence.
		/// </summary>
		public virtual void DeInit()
		{
			StatusDatabase.GameStatus.RemovePropertyChangedHandler(nameof(GameStatus.GameLogicPaused), PauseHandler);
			DestroyEventStateMachine();
			eventNameToEventId.Clear();
		}

		/// <summary>
		/// Register an event handler.
		/// </summary>
		/// <param name="eventPair">The event to register.</param>
		/// <param name="handler">The event handler.</param>
		/// <returns>A handle to use when calling <see cref="UnRegisterEventHandler"/>.</returns>
		public int RegisterEventHandler(string eventName, SequenceEventHandler handler)
		{
			if (!eventNameToEventId.TryGetValue(eventName, out var eventId))
				Log.Instance.Error($"Sequence ID not found {Name}/{eventName}");

			if (!eventMap.TryGetValue(eventId, out var eventData))
			{
				eventData = new EventData((eventName, eventId));
				eventMap.Add(eventId, eventData);
			}

			eventData.EventRegistrations.Add(new EventRegistration(handleCounter, handler));
			return handleCounter++;
		}

		/// <summary>
		/// Unregister an event handler.
		/// </summary>
		/// <param name="eventName">The event to unregister from.</param>
		/// <param name="handle">The handle that was returned from <see cref="RegisterEventHandler"/>.</param>
		public void UnRegisterEventHandler(string eventName, int handle)
		{
			if (!eventNameToEventId.TryGetValue(eventName, out var eventId))
				Log.Instance.Error($"Sequence ID not found {Name}/{eventName}");

			if (eventMap.TryGetValue(eventId, out var eventData))
				eventData.EventRegistrations.RemoveAll(tuple => tuple.Handle == handle);
		}

		/// <summary>
		/// Generates the list of event IDs for the sequence.
		/// </summary>
		public abstract IReadOnlyList<(string eventName, int eventId)> GenerateSequenceEventIds();

		/// <summary>
		/// Starts the sequence.
		/// </summary>
		public virtual void Start()
		{
			if (stateMachine == null)
			{
				Log.Instance.Error($"Sequence {this} has not be initialized yet.");
			}

			IsActive = true;
			UpdateEligibilityList();
		}

		/// <summary>
		/// Sends an interrupt to all currently enabled event registrations.
		/// </summary>
		public void Pause(bool pause)
		{
			IsPaused = pause;
			PauseSequence?.Invoke(pause);

			var args = (SequenceStateArgs)stateMachine.CurrentState.Args;
			if (args?.IsEligible == true)
			{
				var sequenceEventId = args.SequenceEventId;
				if (eventMap.TryGetValue(sequenceEventId, out var eventData))
				{
					var cnt = eventData.EventRegistrations.Count;
					for (var i = 0; i < cnt; ++i)
					{
						var eventRegistration = eventData.EventRegistrations[i];
						if (eventRegistration.WasEnabledAtEnter)
							eventData.EventRegistrations[i].Handler.OnPause?.Invoke(args.CurrentSequenceEventArgs, pause);
					}
				}
			}
		}

		protected IEnumerable<(State state, int intensity)> GetDebugListOfEligibleEvents()
		{
			var list = new List<(State state, int intensity)>(5);
			foreach (var t in states)
			{
				var args = (SequenceStateArgs)t.Args;
				if (args.IsEligible)
				{
					list.Add((t, args.CurrentSequenceEventArgs.Intensity));
				}
			}

			return list;
		}

		private void UpdateEligibilityList()
		{
			foreach (var t in states)
			{
				var args = (SequenceStateArgs)t.Args;
				var sequenceEventId = args.SequenceEventId;
				args.IsEligible = IsSequenceEventEligible(sequenceEventId);
				args.CurrentSequenceEventArgs = args.IsEligible ? GetSequenceEventParameter(sequenceEventId) : null;
			}
		}

		#endregion

		#region State Machine

		private void BuildEventStateMachine()
		{
			var numStates = SequenceEventIds.Count;
			states = new State[numStates];
			for (var stateCount = 0; stateCount < numStates; ++stateCount)
			{
				var state = new State(SequenceEventIds[stateCount].eventName, new SequenceStateArgs(SequenceEventIds[stateCount].eventId));
				state.OnEnter += OnEnter;
				state.OnExecute += OnExecute;
				state.OnExit += OnExit;
				states[stateCount] = state;
			}

			//build rule list
			var rules = new List<Rule>();
			for (var i = 0; i < states.Length; ++i)
			{
				var stateArgs = (SequenceStateArgs)states[i].Args;
				rules.Add(new Rule(states[i],
					new Transition(() =>
					{
						if (eventMap.Count > 0 && eventMap.TryGetValue(stateArgs.SequenceEventId, out var eventData))
						{
							return eventData.Awaiters.CheckIfCompleted();
						}

						return true;
					}, i + 1 < states.Length ? states[i + 1] : finishedState)));
			}

			//idle and finish callbacks
			var builder = new Builder("baseBuilder" + Name);
			builder.In(idleState)
				.OnEnterDo(_ => OnEnterIdleDo())
				.OnExitDo((_, __) => OnExitIdleDo())
				.If(CanExitIdle)
				.Then(states[0]);

			builder.In(finishedState)
				.OnEnterDo(_ => OnEnterSequenceFinishedDo())
				.OnExitDo((_, __) => OnExitSequenceFinishedDo())
				.If(CanExitSequenceFinish)
				.Then(idleState);
			//add base rules
			rules.AddRange(builder.CreateRules());

			var stateMachineName = stateMachinePrefix != null ? $"{stateMachinePrefix}_Sequence_{Name}" : $"Sequence_{Name}";
			if (stateMachine != null)
			{
				Log.Instance.Error($"StateMachine {stateMachineName} already created");
				return;
			}

			stateMachine = StateMachines.Create(stateMachineName, rules, StateMachineService.FrameUpdateRoot);
		}

		private void DestroyEventStateMachine()
		{
			StateMachines.Destroy(ref stateMachine);
		}

		protected virtual bool CanExitIdle()
		{
			return IsActive;
		}

		protected virtual void OnEnterIdleDo() { }

		protected virtual void OnExitIdleDo() { }

		protected virtual bool CanExitSequenceFinish()
		{
			return true;
		}

		protected virtual void OnEnterSequenceFinishedDo() { }

		protected virtual void OnExitSequenceFinishedDo()
		{
			IsActive = false;
		}

		protected virtual bool IsSequenceEventEligible(int sequenceEventId)
		{
			return true;
		}

		protected virtual SequenceEventArgs GetSequenceEventParameter(int sequenceEventId)
		{
			return new SequenceEventArgs(sequenceEventId);
		}

		protected virtual void OnEnter(State currentState)
		{
			var args = (SequenceStateArgs)currentState.Args;
			if (args.IsEligible)
			{
				var sequenceEventId = args.SequenceEventId;
				if (eventMap.TryGetValue(sequenceEventId, out var eventData))
				{
					var cnt = eventData.EventRegistrations.Count;
					for (var i = 0; i < cnt; ++i)
					{
						var eventRegistration = eventData.EventRegistrations[i];
						eventRegistration.WasEnabledAtEnter = eventRegistration.Handler.Enabled == null || eventRegistration.Handler.Enabled.Invoke();
						if (eventRegistration.WasEnabledAtEnter && eventRegistration.Handler.OnStateEnter != null)
						{
							var promise = new TaskCompletionSource<int>();
							eventData.Awaiters.Awaiters.Add((eventData.EventPair, eventRegistration.Handler.Owner, promise.Task));
							eventData.EventRegistrations[i].Handler.OnStateEnter.Invoke(args.CurrentSequenceEventArgs, promise);
						}
					}
				}
			}
		}

		protected virtual void OnExecute(State currentState)
		{
			var args = (SequenceStateArgs)currentState.Args;
			if (args.IsEligible)
			{
				var sequenceEventId = args.SequenceEventId;
				if (eventMap.TryGetValue(sequenceEventId, out var eventData))
				{
					var cnt = eventData.EventRegistrations.Count;
					for (var i = 0; i < cnt; ++i)
					{
						var eventRegistration = eventData.EventRegistrations[i];
						if (eventRegistration.WasEnabledAtEnter)
							eventData.EventRegistrations[i].Handler.OnStateExecute?.Invoke(args.CurrentSequenceEventArgs);
					}
				}
			}
		}

		protected virtual void OnExit(State currentState, State nextState)
		{
			var args = (SequenceStateArgs)currentState.Args;
			if (args.IsEligible)
			{
				var sequenceEventId = args.SequenceEventId;
				if (eventMap.TryGetValue(sequenceEventId, out var eventData))
				{
					var cnt = eventData.EventRegistrations.Count;
					for (var i = 0; i < cnt; ++i)
					{
						var eventRegistration = eventData.EventRegistrations[i];
						if (eventRegistration.WasEnabledAtEnter)
							eventData.EventRegistrations[i].Handler.OnStateExit?.Invoke(args.CurrentSequenceEventArgs);
					}
				}
			}
		}

		#endregion

		#region Object Overrides

		public override string ToString()
		{
			return $"{Name} [{string.Join(", ", GenerateSequenceEventIds().Select(i => i.eventName))}]";
		}

		#endregion

		#region ISequenceDebug Implementation

		public virtual IEnumerable<object> GetAwaiters(int eventId)
		{
			if (eventMap.TryGetValue(eventId, out var eventData))
			{
				return eventData.Awaiters.Awaiters.Where(a => !a.task.IsCompleted)
					.Select(a => a.owner);
			}

			return Array.Empty<object>();
		}

		public bool IsInEvent(int eventId)
		{
			return stateMachine?.CurrentState?.Args is SequenceStateArgs args && args.SequenceEventId == eventId;
		}

		public IEnumerable<(SequenceEventHandler callBack, bool isWaiting)> GetSequenceEventCallbacks(int eventId)
		{
			if (eventMap.TryGetValue(eventId, out var eventData))
			{
				bool IsWaiting(EventRegistration eventRegistration)
				{
					return eventData.Awaiters.Awaiters.Count(a =>
						a.owner == eventRegistration.Handler.Owner &&
						a.eventPair.eventId == eventId &&
						!a.task.IsCompleted) > 0;
				}

				return eventData.EventRegistrations
					.Select(reg => (Callbacks: reg.Handler, IsWaiting(reg)));
			}

			return Array.Empty<(SequenceEventHandler, bool)>();
		}

		#endregion
	}
}