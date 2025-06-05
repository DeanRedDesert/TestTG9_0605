using System.Collections.Generic;
using Midas.Presentation.Dashboard;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.StageHandling;

namespace Game
{
	public sealed class DashboardGameMessages : IGameMessages
	{
		private readonly Stage freeGameStage;
		private readonly string freeGameTriggerPrizeName;
		private readonly List<Stage> bonusStages = new List<Stage>();

		public DashboardGameMessages(IReadOnlyList<Stage> bonusStages, Stage freeGameStage, string freeGameTriggerPrizeName)
		{
			this.freeGameStage = freeGameStage;
			this.freeGameTriggerPrizeName = freeGameTriggerPrizeName;
			this.bonusStages.AddRange(bonusStages);
		}

		public bool GetGameSpecificGameMessage(out GameMessageLeft gameInfoGameMessage)
		{
			gameInfoGameMessage = GameMessageLeft.Nothing;
			return false;
		}

		public bool IsFeatureTriggerPrize(IWinInfo winInfo, out bool isRetrigger)
		{
			isRetrigger = StatusDatabase.GameStatus.CurrentLogicStage.Equals(freeGameStage);
			return !string.IsNullOrEmpty(freeGameTriggerPrizeName) && winInfo.PrizeName.StartsWith(freeGameTriggerPrizeName);
		}

		public bool UseBonusPlay()
		{
			return StatusDatabase.StageStatus.DesiredStage != null && bonusStages.Contains(StatusDatabase.StageStatus.DesiredStage);
		}
	}
}