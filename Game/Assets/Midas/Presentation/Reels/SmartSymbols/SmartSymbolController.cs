using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Game;

namespace Midas.Presentation.Reels.SmartSymbols
{
	public sealed class SmartSymbolController : IPresentationController
	{
		private StatusBlockCompound smartSymbolStatus;
		private GameResultStatus gameResultStatus;

		public SmartSymbolController()
		{
			smartSymbolStatus = new StatusBlockCompound("SmartSymbolStatus");

			StatusDatabase.AddStatusBlock(smartSymbolStatus);
		}

		public void AddSmartSymbolDetectors(string name, params SmartSymbolDetector[] detectors)
		{
			smartSymbolStatus.AddStatusBlock(new SmartSymbolStatus(name, detectors));
		}

		public void RefreshSmartSymbolData()
		{
			var currentStage = StatusDatabase.GameStatus.CurrentLogicStage;
			foreach (var block in smartSymbolStatus.StatusBlocks)
			{
				(block as SmartSymbolStatus)?.RefreshSmartSymbolData(currentStage);
			}
		}

		public void Init()
		{
			gameResultStatus = StatusDatabase.QueryStatusBlock<GameResultStatus>();
		}

		public void DeInit()
		{
			gameResultStatus = null;
		}

		public void Destroy()
		{
			StatusDatabase.RemoveStatusBlock(smartSymbolStatus);
			smartSymbolStatus = null;
		}
	}
}