// Copyright (c) 2021 IGT

#region Usings

using System;

#endregion

namespace Midas.Core.StateMachine
{
	public delegate void StateEnterDelegate(State currentState);

	public delegate void StateExecuteDelegate(State currentState);

	public delegate void StateExitDelegate(State currentState, State nextState);

	public sealed class State
	{
		private readonly StateMachine[] dependencies;

		private int cycleCount;
		private TimeSpan enterTime;
		private readonly TimeSpan timeOut;
		private readonly Func<TimeSpan> timeOutExpr;

		internal int Id = -1;

		public event StateEnterDelegate OnEnter;
		public event StateExecuteDelegate OnExecute;
		public event StateExitDelegate OnExit;

		public static State AnyState { get; } = new State("AnyState");
		public static State PreviousState { get; } = new State("PreviousState");

		public State Previous { get; internal set; }

		public string Name { get; }

		public bool IsReentry { get; internal set; }

		public int ReentryCount { get; internal set; }
		public bool IsStartState { get; }

		public bool IsCycleComplete { get; internal set; }

		public bool Entered { get; internal set; }

		public bool CanExit { get; internal set; }

		public bool WasTimeoutForced { get; private set; }

		public StateArgs Args { get; }

		public bool DebugBreak { get; set; }

		public State(StateArgs args = null)
		{
			timeOut = TimeSpan.Zero;
			IsStartState = false;
			OnEnter += OnStateEnter;
			OnExit += OnStateExit;
			Args = args;
		}

		public State(bool isStartState, StateArgs args = null)
			: this(args)
		{
			IsStartState = isStartState;
		}

		public State(string name, StateArgs args = null)
			: this()
		{
			Name = name;
			Args = args;
		}

		public State(string name, bool isStartState, StateArgs args = null)
			: this(name, args)
		{
			IsStartState = isStartState;
		}

		public State(string name, TimeSpan timeOut, StateArgs args = null)
			: this(name, args)
		{
			this.timeOut = timeOut;
		}

		public State(string name, TimeSpan timeOut, bool isStartState, StateArgs args = null)
			: this(timeOut, isStartState, args)
		{
			Name = name;
		}

		public State(string name, Func<TimeSpan> timeOut, StateArgs args = null)
			: this(name, args)
		{
			timeOutExpr = timeOut;
		}

		public State(string name, Func<TimeSpan> timeOut, bool isStartState, StateArgs args = null)
			: this(timeOut, isStartState, args)
		{
			Name = name;
		}

		public State(TimeSpan timeOut, StateArgs args = null)
			: this(args)
		{
			this.timeOut = timeOut;
		}

		public State(Func<TimeSpan> timeOut, StateArgs args = null)
			: this(args)
		{
			timeOutExpr = timeOut;
		}

		public State(TimeSpan timeOut, bool isStartState, StateArgs args = null)
			: this(timeOut, args)
		{
			IsStartState = isStartState;
		}

		public State(Func<TimeSpan> timeOut, bool isStartState, StateArgs args = null)
			: this(timeOut, args)
		{
			IsStartState = isStartState;
		}

		public State(string name, StateMachine[] dependencies, bool isStartState = false, StateArgs args = null)
			: this(isStartState, args)
		{
			Name = name;
			this.dependencies = dependencies;
		}

		public void ForceTimeout()
		{
			CanExit = true;
			WasTimeoutForced = true;
		}

		public void ResetTimer()
		{
			ResetTimer(TimeSpan.Zero);
		}

		public void ResetTimer(TimeSpan diffToTimeOriginalWaitTimeout)
		{
			enterTime = FrameTime.CurrentTime + diffToTimeOriginalWaitTimeout;
			CanExit = false;
		}

		public override string ToString()
		{
			return Name ?? "unnamed";
		}

		internal void Enter()
		{
			OnEnter?.Invoke(this);
			if (timeOutExpr == null)
			{
				if (timeOut > TimeSpan.Zero)
				{
					enterTime = FrameTime.CurrentTime;
					CanExit = false;
				}
				else
				{
					CanExit = true;
				}
			}
			else
			{
				if (timeOutExpr() >= TimeSpan.Zero)
				{
					enterTime = FrameTime.CurrentTime;
					CanExit = false;
				}
				else
				{
					CanExit = true;
				}
			}
		}

		internal void Execute()
		{
			OnExecute?.Invoke(this);
			if (timeOutExpr == null)
			{
				if (timeOut > TimeSpan.Zero &&
					FrameTime.CurrentTime >= enterTime + timeOut)
				{
					CanExit = true;
				}
			}
			else
			{
				if (timeOutExpr() >= TimeSpan.Zero &&
					FrameTime.CurrentTime >= enterTime + timeOutExpr())
				{
					CanExit = true;
				}
			}

			if (dependencies?.Length > 0)
			{
				CanExit = true;
				foreach (var stateMachine in dependencies)
				{
					if (!stateMachine.IsCycleComplete)
					{
						CanExit = false;
						break;
					}
				}
			}
		}

		internal void Exit(State nextState)
		{
			OnExit?.Invoke(this, nextState);
			WasTimeoutForced = false;
		}

		private void OnStateEnter(State _)
		{
			if (cycleCount == 0)
			{
				cycleCount++;
				IsCycleComplete = false;
			}
			else
			{
				cycleCount = 0;
				IsCycleComplete = true;
			}

			Entered = true;
		}

		private void OnStateExit(State _, State __)
		{
			Entered = false;
		}
	}
}