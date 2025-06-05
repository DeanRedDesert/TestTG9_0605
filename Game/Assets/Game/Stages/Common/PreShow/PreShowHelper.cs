using System.Linq;
using Midas.Gle.Presentation;
using Midas.Presentation.Data.StatusBlocks;

namespace Game.Stages.Common.PreShow
{
	public static class PreShowHelper
	{
		public static IWinInfo FindPrize(string prizeName) => GleGameController.GleStatus.GetWinInfo().FirstOrDefault(p => p.PrizeName == prizeName);
	}
}