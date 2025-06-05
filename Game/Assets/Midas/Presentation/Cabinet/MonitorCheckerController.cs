using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.Core.Configuration;
using Midas.Core.Coroutine;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Data;
using Midas.Presentation.Game;
using Coroutine = Midas.Core.Coroutine.Coroutine;

namespace Midas.Presentation.Cabinet
{
	public sealed class MonitorCheckerController : IPresentationController
	{
		private Coroutine checkerCoroutine;
		private readonly IReadOnlyList<MonitorRole> requiredRoles;

		#region IPresentationController Implementation

		public void Init()
		{
			checkerCoroutine = FrameUpdateService.Update.StartCoroutine(MonitorCheckerHandler());
		}

		public void DeInit()
		{
			checkerCoroutine?.Stop();
			checkerCoroutine = null;
		}

		public void Destroy()
		{
		}

		#endregion

		public MonitorCheckerController(IReadOnlyList<MonitorRole> requiredMonitors)
		{
			requiredRoles = requiredMonitors;
		}

		#region Private Methods

		private IEnumerator<CoroutineInstruction> MonitorCheckerHandler()
		{
			while (StatusDatabase.ConfigurationStatus.MachineConfig == null)
				yield return null;

			if (StatusDatabase.ConfigurationStatus.MachineConfig.Jurisdiction != "MCAU")
				yield break;

			var lastNotDetected = new List<MonitorRole>();
			CustomConfig lastConfig = null;
			DenomConfig denomConfig = null;
			while (true)
			{
				yield return null;

				if (StatusDatabase.ConfigurationStatus.CustomConfig == lastConfig && StatusDatabase.ConfigurationStatus.DenomConfig == denomConfig)
					continue;

				lastConfig = StatusDatabase.ConfigurationStatus.CustomConfig;
				denomConfig = StatusDatabase.ConfigurationStatus.DenomConfig;

				foreach (var rr in requiredRoles)
				{
					var connected = CabinetManager.Cabinet.MonitorConfigs.Any(mc => mc.Role == rr);

					switch (connected)
					{
						case true when lastNotDetected.Contains(rr):
							Communication.ToLogicSender.Send(new GameClearTiltMessage($"{rr}MonitorNotConnected"));
							lastNotDetected.Remove(rr);
							continue;
						case false when !lastNotDetected.Contains(rr):
							Communication.ToLogicSender.Send(new GamePostTiltMessage($"{rr}MonitorNotConnected", GameTiltPriority.High, $"{rr} Required Monitor Not Connected", $"{rr} required monitor was not detected.", true, true, true));
							lastNotDetected.Add(rr);
							break;
					}
				}
			}
		}

		#endregion
	}
}