using System;
using Midas.Core.Debug;
using Midas.Core.General;

namespace Midas.Core
{
	public static class GamePresentationTimings
	{
		private static (TimeSpan changeDenomInProgress, TimeSpan changeDenomCancelled) currentChangeDenomTime;

		private static int maxStatusDatabaseStateMachineLoops;
		private static int maxStateMachineLoops;

		public static bool LogData { get; set; }

		public static void Reset()
		{
			L2PTime.Reset();
			CabinetLibTime.Reset();
			BeforeFrameUpdateTime.Reset();
			DataBaseTime.Reset();
			StateMachineTime.Reset();
			ButtonServiceTime.Reset();
			FrameUpdateTime.Reset();
			AfterFrameUpdateTime.Reset();
			GamingSystemUpdateTime.Reset();
			UnityUpdateTime.Reset();
			ExpressionManagerTime.Reset();
			OverallUpdateTime.Reset();
			AnimatorAndCoRoutineUpdateTime.Reset();
			FrameLateUpdateTime.Reset();
			UnityLateUpdateTime.Reset();
			OverallLateUpdateTime.Reset();
			OverallGamingSystemTime.Reset();
			OverallUnityTime.Reset();
			OverallTime.Reset();

			maxStatusDatabaseStateMachineLoops = 0;
			maxStateMachineLoops = 0;
		}

		public static string AsString()
		{
			return $@"
L2P:                 {L2PTime}
Cabinet:             {CabinetLibTime}
BeforeFrameUpdate:   {BeforeFrameUpdateTime}
Database:            {DataBaseTime}
StateMachine:        {StateMachineTime}
Buttons:             {ButtonServiceTime}
FrameUpdate:         {FrameUpdateTime}
AfterFrameUpdate:    {AfterFrameUpdateTime}
Gaming Sys Update:   {GamingSystemUpdateTime}
UnityUpdate:         {UnityUpdateTime}
OverallUpdate:       {OverallUpdateTime}
Anim + CoUpdateTime: {AnimatorAndCoRoutineUpdateTime}
FrameLateUpdate:     {FrameLateUpdateTime}
ExpressionManager:   {ExpressionManagerTime}
UnityLateUpdate:     {UnityLateUpdateTime}
OverallLateUpdate:   {OverallLateUpdateTime}
Overall Gaming Sys:  {OverallGamingSystemTime}
OverallUnity:        {OverallUnityTime}
Overall:             {OverallTime}
MaxGameDBStateLoops: {MaxStatusDatabaseStateMachineLoops}
MaxStateMachineLoops:{MaxStateMachineLoops}
";
		}

		public static void ReportChangeDenomInProgress()
		{
			currentChangeDenomTime = (FrameTime.CurrentTime, TimeSpan.MaxValue);
		}

		public static void ReportChangeDenomCancelled()
		{
			if (currentChangeDenomTime.changeDenomInProgress != TimeSpan.MaxValue)
			{
				currentChangeDenomTime.changeDenomCancelled = FrameTime.CurrentTime;
				ChangeDenomTimes.PushBack(currentChangeDenomTime);
				currentChangeDenomTime = (TimeSpan.MaxValue, TimeSpan.MaxValue);
			}
		}

		public static TimeSpanCollector L2PTime { get; } = new TimeSpanCollector();
		public static TimeSpanCollector CabinetLibTime { get; } = new TimeSpanCollector();
		public static TimeSpanCollector BeforeFrameUpdateTime { get; } = new TimeSpanCollector();
		public static TimeSpanCollector DataBaseTime { get; } = new TimeSpanCollector();
		public static TimeSpanCollector StateMachineTime { get; } = new TimeSpanCollector();
		public static TimeSpanCollector ButtonServiceTime { get; } = new TimeSpanCollector();
		public static TimeSpanCollector FrameUpdateTime { get; } = new TimeSpanCollector();
		public static TimeSpanCollector AfterFrameUpdateTime { get; } = new TimeSpanCollector();
		public static TimeSpanCollector GamingSystemUpdateTime { get; } = new TimeSpanCollector();
		public static TimeSpanCollector UnityUpdateTime { get; } = new TimeSpanCollector();
		public static TimeSpanCollector OverallUpdateTime { get; } = new TimeSpanCollector();
		public static TimeSpanCollector FrameLateUpdateTime { get; } = new TimeSpanCollector();
		public static TimeSpanCollector UnityLateUpdateTime { get; } = new TimeSpanCollector();
		public static TimeSpanCollector OverallLateUpdateTime { get; } = new TimeSpanCollector();
		public static TimeSpanCollector ExpressionManagerTime { get; } = new TimeSpanCollector();
		public static TimeSpanCollector AnimatorAndCoRoutineUpdateTime { get; } = new TimeSpanCollector(); //time between Update and LateUpdate
		public static TimeSpanCollector OverallGamingSystemTime { get; } = new TimeSpanCollector();
		public static TimeSpanCollector OverallUnityTime { get; } = new TimeSpanCollector();
		public static TimeSpanCollector OverallTime { get; } = new TimeSpanCollector();

		public static int MaxStatusDatabaseStateMachineLoops
		{
			get => maxStatusDatabaseStateMachineLoops;
			set
			{
				if (value > maxStatusDatabaseStateMachineLoops)
					maxStatusDatabaseStateMachineLoops = value;
			}
		}

		public static int MaxStateMachineLoops
		{
			get => maxStateMachineLoops;
			set
			{
				if (value > maxStateMachineLoops)
					maxStateMachineLoops = value;
			}
		}

		public static int MaxBufferSize { get; set; } = 10;

		public static CircularBuffer<(TimeSpan changeDenomInProgress, TimeSpan changeDenomCancelled)> ChangeDenomTimes { get; } =
			new CircularBuffer<(TimeSpan changeDenomInProgress, TimeSpan changeDenomCancelled)>(MaxBufferSize);
	}
}	