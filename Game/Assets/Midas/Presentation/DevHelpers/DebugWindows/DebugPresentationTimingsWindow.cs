using Midas.Core;
using Midas.Core.Debug;
using UnityEngine;

namespace Midas.Presentation.DevHelpers.DebugWindows
{
	public sealed class DebugPresentationTimingsWindow : DebugWindow
	{
		protected override void RenderWindowContent()
		{
			GamePresentationTimings.LogData = GUILayout.Toggle(GamePresentationTimings.LogData, "Log Data");
			GUILayout.Label("                      Current    Min      Max       Av", LabelStyle);
			GUILayout.Label($"L2P:                 {TimingString(GamePresentationTimings.L2PTime)}", LabelStyle);
			GUILayout.Label($"Cabinet:             {TimingString(GamePresentationTimings.CabinetLibTime)}", LabelStyle);
			GUILayout.Label($"BeforeFrameUpdate:   {TimingString(GamePresentationTimings.BeforeFrameUpdateTime)}", LabelStyle);
			GUILayout.Label($"Database:            {TimingString(GamePresentationTimings.DataBaseTime)}", LabelStyle);
			GUILayout.Label($"StateMachine:        {TimingString(GamePresentationTimings.StateMachineTime)}", LabelStyle);
			GUILayout.Label($"Buttons:             {TimingString(GamePresentationTimings.ButtonServiceTime)}", LabelStyle);
			GUILayout.Label($"FrameUpdate:         {TimingString(GamePresentationTimings.FrameUpdateTime)}", LabelStyle);
			GUILayout.Label($"AfterFrameUpdate:    {TimingString(GamePresentationTimings.AfterFrameUpdateTime)}", LabelStyle);
			GUILayout.Label($"Gaming Sys Update:   {TimingString(GamePresentationTimings.GamingSystemUpdateTime)}", LabelStyle);
			GUILayout.Label($"Expressions:         {TimingString(GamePresentationTimings.ExpressionManagerTime)}", LabelStyle);
			GUILayout.Label($"UnityUpdate:         {TimingString(GamePresentationTimings.UnityUpdateTime)}", LabelStyle);
			GUILayout.Label($"OverallUpdate:       {TimingString(GamePresentationTimings.OverallUpdateTime)}", LabelStyle);
			GUILayout.Label($"Anim + CoUpdateTime: {TimingString(GamePresentationTimings.AnimatorAndCoRoutineUpdateTime)}", LabelStyle);
			GUILayout.Label($"FrameLateUpdate:     {TimingString(GamePresentationTimings.FrameLateUpdateTime)}", LabelStyle);
			GUILayout.Label($"UnityLateUpdate:     {TimingString(GamePresentationTimings.UnityLateUpdateTime)}", LabelStyle);
			GUILayout.Label($"OverallLateUpdate:   {TimingString(GamePresentationTimings.OverallLateUpdateTime)}", LabelStyle);
			GUILayout.Label($"Overall Gaming Sys:  {TimingString(GamePresentationTimings.OverallGamingSystemTime)}", LabelStyle);
			GUILayout.Label($"OverallUnity:        {TimingString(GamePresentationTimings.OverallUnityTime)}", LabelStyle);
			GUILayout.Label($"Overall:             {TimingString(GamePresentationTimings.OverallTime)}", LabelStyle);

			GUILayout.Label("", LabelStyle);
			GUILayout.Label($"MaxGameDBStateLoops: {GamePresentationTimings.MaxStatusDatabaseStateMachineLoops}", LabelStyle);
			GUILayout.Label($"MaxStateMachineLoops:{GamePresentationTimings.MaxStateMachineLoops}", LabelStyle);

			GUILayout.Label("", LabelStyle);
			if (GUILayout.Button("Reset", ButtonStyle))
				GamePresentationTimings.Reset();
		}

		private static string TimingString(TimeSpanCollector timeSpanCollector)
		{
			return $"{timeSpanCollector.Current,8:0.000} {timeSpanCollector.Min,8:0.000} {timeSpanCollector.Max,9:0.000} {timeSpanCollector.Average,8:0.000}";
		}
	}
}