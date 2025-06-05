using Midas.Presentation.StageHandling;

namespace Midas.Presentation.Reels.SmartSymbols
{
	public abstract class SmartSymbolDetector
	{
		private readonly Stage stage;

		protected SmartSymbolDetector(Stage stage)
		{
			this.stage = stage;
		}

		public bool TryFindSmartSymbols(Stage currentStage, out SmartSymbolData data)
		{
			if (currentStage.Equals(stage))
				return (data = FindSmartSymbols()) != null;

			data = null;
			return false;
		}

		protected abstract SmartSymbolData FindSmartSymbols();
	}
}