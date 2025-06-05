using System;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using UnityEngine;

namespace Midas.Presentation.DevHelpers.DebugWindows
{
	public sealed class DebugCrashTesterWindow : DebugWindow
	{
		private bool crashLogic;
		private bool crashPresentation;

		protected override void RenderWindowContent()
		{
			if (!crashLogic && !crashPresentation)
			{
				crashLogic = GUILayout.Button("Crash Logic", ButtonStyle);
				crashPresentation = GUILayout.Button("Crash Presentation", ButtonStyle);
				return;
			}

			GUILayout.Label("Are you sure?", LabelStyle);

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Yes", ButtonStyle))
			{
				if (crashLogic)
					Communication.ToLogicSender.Send(new DevCrashLogicMessage());
				else if (crashPresentation)
					throw new Exception("Test presentation crash");

				crashLogic = false;
				crashPresentation = false;
			}

			if (GUILayout.Button("No", ButtonStyle))
			{
				crashLogic = false;
				crashPresentation = false;
			}

			GUILayout.EndHorizontal();
		}
	}
}