using Midas.Core;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using UnityEngine;

namespace Midas.Presentation.DevHelpers
{
	public sealed class DisplayStateController : MonoBehaviour
	{
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Alpha0))
				Communication.ToLogicSender.Send(new DebugDisplayStateMessage(DisplayState.Normal));
			else if (Input.GetKeyDown(KeyCode.Alpha9))
				Communication.ToLogicSender.Send(new DebugDisplayStateMessage(DisplayState.Suspended));
			else if (Input.GetKeyDown(KeyCode.Alpha8))
				Communication.ToLogicSender.Send(new DebugDisplayStateMessage(DisplayState.Hidden));
		}
	}
}