using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core.General;

namespace Midas.Core.StateMachine
{
	public sealed class StateMachine
	{
		#region Nested Type: TransitionTableItem

		private readonly struct TransitionTableItem
		{
			public bool IsElse { get; }
			public int Priority { get; }
			public State Target { get; }
			public Func<bool> Condition { get; }

			public TransitionTableItem(Func<bool> cond, State target, int priority, bool isElse)
			{
				Condition = cond;
				Target = target;
				Priority = priority;
				IsElse = isElse;
			}
		}

		#endregion

		private const int StateHistoryCount = 15;
		private const int MaxAllowedStepCounter = 100;

		internal StateMachine Parent;

		private readonly List<StateMachine> childrenToAdd = new List<StateMachine>();
		private readonly List<StateMachine> childrenToRemove = new List<StateMachine>();
		private readonly List<StateMachine> children = new List<StateMachine>();
		private readonly int priority;
		private TransitionTableItem[][] transitionTable;
		private Rule[] rules;
		private readonly List<State> states = new List<State>();
		private readonly CircularBuffer<StateStep> stateHistory = new CircularBuffer<StateStep>(StateHistoryCount);
		private int stepCounter;
		private bool isProcessingChildren;

		public event StateEnterDelegate OnEnter;
		public event StateExitDelegate OnExit;
		public event TransitionDelegate OnTransition;

		public State CurrentState { get; private set; }

		public IReadOnlyList<Rule> Rules => rules;
		public bool IsCycleComplete { get; private set; }
		public State StartState { get; private set; }
		public string Name { get; }
		public int MaxStepCounter { get; internal set; }

		public IReadOnlyList<State> States => states;
		public IReadOnlyList<StateMachine> Children => children;
		public IEnumerable<StateStep> StateHistory => stateHistory;

		// Return true while another step is possible
		// ReSharper disable once FunctionComplexityOverflow
		// This is caused by manual inlining and is on purpose for performance reasons.
		// -And yes, I measured the difference!
		public bool Step(long frameNumber)
		{
			if (!CurrentState.IsReentry)
			{
#if DEBUG
				if (Debugging.BreakMode == BreakMode.On &&
					!Debugging.CheckDebugBreak(BreakPosition.BeforeEnter, CurrentState))
				{
					return false;
				}
#endif

				stateHistory.PushFront(new StateStep(frameNumber, FrameTime.CurrentTime, CurrentState));
				OnEnter?.Invoke(CurrentState);
				CurrentState.Enter();
				CurrentState.IsReentry = true;
			}

#if DEBUG
			if (Debugging.BreakMode == BreakMode.On &&
				!Debugging.CheckDebugBreak(BreakPosition.BeforeExecute, CurrentState))
			{
				return false;
			}
#endif

			CurrentState.Execute();

			State matchTransition = null;
			var matchPriority = 0;
			State elseTransition = null;
			var transitions = transitionTable[CurrentState.Id];

			foreach (var transitionTableItem in transitions)
			{
				if (!transitionTableItem.IsElse)
				{
					if (matchTransition == null)
					{
						if (transitionTableItem.Condition())
						{
							matchTransition = transitionTableItem.Target;
							matchPriority = transitionTableItem.Priority;
						}
					}
					else
					{
						var eval = transitionTableItem.Condition();
						if (transitionTableItem.Priority < matchPriority && eval)
						{
							matchTransition = transitionTableItem.Target;
						}
#if DEBUG
						else if (transitionTableItem.Priority == matchPriority && eval)
						{
							var msg = "MatchRule: Integrity error, we matched already a transition the same priority!" +
								$" previous match: {matchTransition.Name}, new match: {transitionTableItem.Target.Name} , CurrentState: {CurrentState.Name}";
							throw new Exception(msg);
						}
#endif
					}
				}
				else
				{
					elseTransition = transitionTableItem.Target;
				}
			}

			var match = matchTransition == null && elseTransition != null ? elseTransition : matchTransition;

			if (CurrentState != null)
			{
				if (match != null && CurrentState.CanExit)
				{
					IsCycleComplete = false;
					var target = match.Id == State.PreviousState.Id ? CurrentState.Previous : match;

					if (target == null)
					{
						throw new Exception("StateMachine.Step: target state is null!");
					}

#if DEBUG
					if (Debugging.BreakMode == BreakMode.On &&
						!Debugging.CheckDebugBreak(BreakPosition.BeforeExit, CurrentState))
					{
						return false;
					}
#endif

					OnExit?.Invoke(CurrentState, target);
					CurrentState.Exit(target);
					target.IsReentry = false;
					target.ReentryCount = 0;
					if (target.Id == StartState.Id)
					{
						IsCycleComplete = true;
					}

					OnTransition?.Invoke(CurrentState, target);
					match.Previous = CurrentState;
					CurrentState = target;
				}
				else
				{
					CurrentState.IsReentry = true;
					CurrentState.ReentryCount++;
				}
			}
			else
			{
				//integrity error
				throw new Exception("StateMachine.Step: No Current State, integrity error!");
			}

			isProcessingChildren = true;

			var stepChildren = false;
			List<StateMachine> childsWithTrue = null;
			foreach (var child in children)
			{
				var childResult = child.Step(frameNumber);
				stepChildren |= childResult;
				if (childResult && stepCounter >= MaxAllowedStepCounter)
				{
					childsWithTrue ??= new List<StateMachine>();
					childsWithTrue.Add(child);
				}
			}

			isProcessingChildren = false;

			var returnValue = !CurrentState.IsReentry || stepChildren;
			stepCounter = returnValue ? stepCounter + 1 : 1;
			MaxStepCounter = Math.Max(stepCounter, MaxStepCounter);

			if (stepCounter > MaxAllowedStepCounter)
			{
				LogMaxStepCounterReached(stepChildren, childsWithTrue);
			}

			foreach (var stateMachine in childrenToAdd)
				children.Insert(GetPositionWhereToInsert(stateMachine), stateMachine);
			foreach (var stateMachine in childrenToRemove)
				children.Remove(stateMachine);
			childrenToAdd.Clear();
			childrenToRemove.Clear();

			return returnValue;
		}

		public State FindState(string name)
		{
			foreach (var state in states)
			{
				if (state.Name.Equals(name))
				{
					return state;
				}
			}

			return null;
		}

		public override string ToString()
		{
			return Name;
		}

		internal StateMachine(string name, List<Rule> rules, int priority)
		{
			this.priority = priority;
			Name = name;
			SetupRules(rules ?? new List<Rule>());
		}

		internal void AddChildStateMachine(StateMachine stateMachine)
		{
			stateMachine.Parent = this;
			if (isProcessingChildren)
				childrenToAdd.Add(stateMachine);
			else
				children.Insert(GetPositionWhereToInsert(stateMachine), stateMachine);
		}

		internal void RemoveChildStateMachine(StateMachine stateMachine)
		{
			stateMachine.Parent = null;
			if (isProcessingChildren)
				childrenToRemove.Add(stateMachine);
			else
			{
				childrenToAdd.Remove(stateMachine);
				children.Remove(stateMachine);
			}
		}

		private void SetupRules(IReadOnlyList<Rule> newRules)
		{
			var transitionIndex = new Dictionary<State, List<Transition>>();

			IsCycleComplete = false;
			CurrentState = null;
			transitionTable = null;
			states.Clear();
			rules = newRules.ToArray();
			var numRules = this.rules.Length;
			for (var i = 0; i < numRules; ++i)
			{
				var rule = this.rules[i];
				foreach (var state in rule.States)
				{
					//init time service
					if (transitionIndex.ContainsKey(state))
					{
						transitionIndex[state].AddRange(rule.Transitions);
					}
					else
					{
						var t = transitionIndex[state] = new List<Transition>();
						t.AddRange(rule.Transitions);
					}

					if (state.IsStartState)
					{
						if (CurrentState == null && StartState == null || StartState == state)
						{
							CurrentState = state;
							StartState = CurrentState;
						}
						else
						{
							//error, cannot have more than one start state
							throw new Exception("StateMachine.Rules: Cannot have more than one start state!");
						}
					}
				}
			}

			//add any states and sort everything
			var hasAnyStates = transitionIndex.TryGetValue(State.AnyState, out var anyStates);
			var stateId = 0;
			foreach (var transitions in transitionIndex)
			{
				transitions.Key.Id = stateId++;
				if (hasAnyStates && transitions.Key != State.AnyState)
				{
					transitions.Value.AddRange(anyStates);
				}

				transitions.Value.Sort((x, y) => x.Priority.CompareTo(y.Priority));
			}

			if (CurrentState == null)
			{
				//error, no start state
				throw new Exception("StateMachine.Rules: No start state!");
			}

			transitionTable = new TransitionTableItem[transitionIndex.Count][];
			//copy temp dict
			foreach (var entry in transitionIndex)
			{
				states.Add(entry.Key);
				transitionTable[entry.Key.Id] = new TransitionTableItem[entry.Value.Count];
				var tcnt = 0;
				var table = transitionTable[entry.Key.Id];
				foreach (var item in entry.Value)
				{
					table[tcnt++] = new TransitionTableItem(item.Condition, item.Target, item.Priority, item.IsElse);
				}
			}
		}

		private int GetPositionWhereToInsert(StateMachine stateMachine)
		{
			for (var i = 0; i < children.Count; i++)
			{
				if (children[i].priority < stateMachine.priority)
				{
					return i;
				}
			}

			return children.Count;
		}

		private void LogMaxStepCounterReached(bool stepChildren, List<StateMachine> childsWithTrue)
		{
			Log.Instance.Error($"StepCounter reached {stepCounter}. StateMachine={this}, CurrentState={CurrentState}");
			if (!CurrentState.IsReentry)
			{
				Log.Instance.Error($"Reason might be that {CurrentState}._isReentry is false");
			}

			if (stepChildren)
			{
				Log.Instance.Error($"Reason might be that these stepChildren are true: {string.Join(",", childsWithTrue)}");
			}

			Log.Instance.Fatal($"StepCounter reached {stepCounter}. StateMachine={this}, CurrentState={CurrentState}");
		}
	}
}