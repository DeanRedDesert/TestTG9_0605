using System;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using UnityEngine;

namespace Midas.Presentation.DevHelpers.DebugWindows
{
	public sealed class DebugLogicTimingsWindow : DebugWindow
	{
		private static readonly char[] split = { '\n', '\r' };
		private string[] timings = Array.Empty<string>();

		private void OnEnable()
		{
			Communication.PresentationDispatcher.AddHandler<GameLogicTimingsMessage>(OnGameLogicTimingsMessage);
		}

		private void OnDisable()
		{
			Communication.PresentationDispatcher?.RemoveHandler<GameLogicTimingsMessage>(OnGameLogicTimingsMessage);
		}

		private void OnGameLogicTimingsMessage(GameLogicTimingsMessage message)
		{
			timings = message.LogicTimings.Split(split, StringSplitOptions.RemoveEmptyEntries);
		}

		protected override void RenderWindowContent()
		{
			foreach (var timing in timings)
				GUILayout.Label(timing, LabelStyle);

			if (GUILayout.Button("Reset", ButtonStyle))
				Communication.ToLogicSender.Send(new GameLogicTimingsResetMessage());
		}

		protected override void Reset()
		{
			base.Reset();

			WindowName = "Logic Timings";
			FontSize = 24;
			WindowRect = new Rect(900, 500, 1100, 1000);
			BackgroundColor = new Color(0.6f, 0, 0.2f);
		}
	}
}