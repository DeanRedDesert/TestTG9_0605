using System;
using Midas.Core;
using Midas.LogicToPresentation.Data;

namespace Midas.Logic
{
	public partial class GameLogic
	{
		private void InitGameServices()
		{
			GameServices.ConfigurationService.ConfigurationDataService.SetValue(logicState.Configuration);
			GameServices.MachineStateService.GameModeService.SetValue(foundation.GameMode);
			GameServices.MachineStateService.HasWinCapBeenReachedService.SetValue(logicState.HasWinCapBeenReached);
			GameServices.MachineStateService.GameTimeService.SetValue(logicState.GameTime);
			CalculateRtps(Array.Empty<ProgressiveLevel>());
		}
	}
}