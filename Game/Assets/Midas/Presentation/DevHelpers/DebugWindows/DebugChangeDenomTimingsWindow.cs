using System.Linq;
using Midas.Core;
using UnityEngine;

namespace Midas.Presentation.DevHelpers.DebugWindows
{
	public sealed class DebugChangeDenomTimingsWindow : DebugWindow
	{
		#region Protected

		protected override void RenderWindowContent()
		{
			GUILayout.Label(" Duration", LabelStyle);
			foreach (var changeDenom in GamePresentationTimings.ChangeDenomTimes.Reverse())
				GUILayout.Label($"{(int)(changeDenom.changeDenomCancelled - changeDenom.changeDenomInProgress).TotalMilliseconds,6}", LabelStyle);

			for (var i = 0; i < GamePresentationTimings.ChangeDenomTimes.Capacity - GamePresentationTimings.ChangeDenomTimes.Size; i++)
				GUILayout.Label("", LabelStyle);

			if (GUILayout.Button("Reset", ButtonStyle))
				GamePresentationTimings.Reset();
		}

		#endregion
	}
}