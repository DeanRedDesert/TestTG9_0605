using Logic.Core.Types;
using Midas.Presentation.Reels.SmartSymbols;
using Midas.Presentation.StageHandling;

namespace Midas.Gle.Presentation.SmartSymbols
{
	public abstract class GleSmartSymbolDetector : SmartSymbolDetector
	{
		private readonly string resultName;

		protected GleSmartSymbolDetector(Stage stage, string resultName) : base(stage)
		{
			this.resultName = resultName;
		}

		protected override SmartSymbolData FindSmartSymbols()
		{
			var results = GleGameController.GleStatus.CurrentGameResult.Current.StageResults;
			var (swr, lockMask) = results.GetReelDetails(resultName);
			return FindSmartSymbols(swr, lockMask);
		}

		protected abstract SmartSymbolData FindSmartSymbols(SymbolWindowResult symbolWindowResult, ReadOnlyMask lockMask);
	}
}