using System.Collections.Generic;
using Midas.Core.StateMachine;
using UnityEngine;

namespace Midas.Presentation.DevHelpers
{
	public sealed class StateMachineView : TreeViewControl
	{
		#region Public

		public override void Draw(Rect position)
		{
			var treeHeight = position.height - LabelStyle.lineHeight * 7;
			base.Draw(new Rect(position.x, position.y, position.width, treeHeight));
			GUILayout.Space(position.y + treeHeight + LabelStyle.lineHeight);

			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Label($"StateMachine count: {StateMachines.AllStateMachines.Count}", LabelStyle);

				if (GUILayout.Button("Reset Max Steps", ButtonStyle, GUILayout.ExpandWidth(false)))
				{
					StateMachines.ResetAllMaxStepCounters();
				}
			}

			using (new GUILayout.HorizontalScope())
			{
				Debugging.BreakMode = GUILayout.Toggle(Debugging.BreakMode == BreakMode.On,
					"Debug On", ToggleStyle, GUILayout.ExpandWidth(false))
					? BreakMode.On
					: BreakMode.Off;

				using (new GUILayout.VerticalScope(GUILayout.ExpandWidth(false)))
				{
					if (GUILayout.Toggle(Debugging.BreakPosition == BreakPosition.BeforeEnter, "BeforeEnter",
							ToggleStyle, GUILayout.ExpandWidth(false)))
					{
						Debugging.BreakPosition = BreakPosition.BeforeEnter;
					}

					if (GUILayout.Toggle(Debugging.BreakPosition == BreakPosition.BeforeExecute, "BeforeExecute",
							ToggleStyle, GUILayout.ExpandWidth(false)))
					{
						Debugging.BreakPosition = BreakPosition.BeforeExecute;
					}

					if (GUILayout.Toggle(Debugging.BreakPosition == BreakPosition.BeforeExit, "BeforeExit",
							ToggleStyle, GUILayout.ExpandWidth(false)))
					{
						Debugging.BreakPosition = BreakPosition.BeforeExit;
					}
				}

				if (GUILayout.Button("Next", ButtonStyle, GUILayout.ExpandWidth(false)))
				{
					Debugging.NextDebugStep = true;
				}
			}
		}

		#endregion

		#region Protected

		protected override IEnumerable<object> GetChildren(object node)
		{
			switch (node)
			{
				case StateMachine sm:
				{
					foreach (var state in sm.States)
					{
						yield return state;
					}

					if (sm.States.Count > 1)
						yield return sm.StateHistory;

					foreach (var child in sm.Children)
					{
						yield return child;
					}

					break;
				}

				case IEnumerable<StateStep> history:
				{
					foreach (var state in history)
					{
						yield return state;
					}

					break;
				}

				case State state:
				{
					if (state.Args is CoroutineExtensionMethods.StateMachineCoroutineArgs args)
						yield return args;

					break;
				}

				case null:
					foreach (var s in StateMachines.AllRootStateMachines)
					{
						yield return s;
					}

					break;
			}
		}

		protected override bool DrawItem(object item, bool collapsed)
		{
			var pressed = false;
			if (item is StateMachine sm)
			{
				var oldColor = LabelStyle.normal.textColor;
				LabelStyle.normal.textColor = sm.MaxStepCounter > 2 ? sm.MaxStepCounter > 5 ? Color.red : Color.yellow : oldColor;
				pressed = GUILayout.Button((collapsed ? "+ " : "- ") + $"{sm.Name}({sm.MaxStepCounter})", LabelStyle);
				LabelStyle.normal.textColor = oldColor;
			}
			else if (item is IEnumerable<StateStep>)
			{
				pressed = GUILayout.Button((collapsed ? "+ " : "- ") + "History", LabelStyle);
				GUILayout.Space(LabelStyle.lineHeight / 2);
			}
			else if (item is State s)
			{
				if (s.Args is CoroutineExtensionMethods.StateMachineCoroutineArgs args)
				{
					pressed = GUILayout.Button((collapsed ? "+ " : "- ") + s.Name, LabelStyle);
					GUILayout.Space(LabelStyle.lineHeight / 2);
				}
				else
				{
					using (new GUILayout.HorizontalScope())
					{
						var oldColor = LabelStyle.normal.textColor;
						LabelStyle.normal.textColor = s.Entered ? Color.green : oldColor;

						s.DebugBreak = GUILayout.Toggle(s.DebugBreak, "", ToggleStyle, GUILayout.ExpandWidth(false));
						GUILayout.Toggle(s.Entered, "", ToggleStyle, GUILayout.ExpandWidth(false));
						GUILayout.Label(s.Name, LabelStyle);

						LabelStyle.normal.textColor = oldColor;
					}
				}
			}
			else if (item is StateStep step)
			{
				GUILayout.Label($"{step.State.Name}, time: {step.Time.ToString()}, frame: {step.FrameNumber.ToString()}", LabelStyle);
			}
			else if (item is CoroutineExtensionMethods.StateMachineCoroutineArgs args)
			{
				GUILayout.Label(args.Coroutine.ToString(), LabelStyle);
			}

			return pressed;
		}

		protected override string GetValueAsString(object item)
		{
			// Not needed.
			return string.Empty;
		}

		// not needed because draw item implemented itself
		protected override bool HasChildren(object node)
		{
			return false;
		}

		#endregion
	}
}