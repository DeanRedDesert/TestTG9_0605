using System.Collections.Generic;
using Midas.Core.Coroutine;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Data;
using Midas.Presentation.Game;

namespace Midas.Gaff.Utp
{
	public sealed partial class GleGaffModule
	{
		private sealed class GaffController : IGaffHandler
		{
			public bool IsEnable { get { return true; } }

			public int Priority => 0;

			public IEnumerator<CoroutineInstruction> Run()
			{
				var egs = StatusDatabase.QueryStatusBlock<UtpGaffStatus>(false);
				if (egs == null)
					yield break;

				if (egs.IsWaiting && egs.GaffResults == null)
				{
					egs.IsShowing = true;
					while (egs.IsShowing)
						yield return null;
				}

				if (egs.GaffResults == null)
					yield break;

				Communication.ToLogicSender.Send(new DemoGaffResultsMessage(egs.GaffResults));
				egs.GaffResults = null;
			}
		}
	}
}