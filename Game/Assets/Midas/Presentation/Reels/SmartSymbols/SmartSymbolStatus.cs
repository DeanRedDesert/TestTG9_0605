using System.Collections.Generic;
using Midas.Presentation.Data;
using Midas.Presentation.StageHandling;

namespace Midas.Presentation.Reels.SmartSymbols
{
	public sealed class SmartSymbolStatus : StatusBlock
	{
		private StatusProperty<SmartSymbolData> smartSymbolData;
		private readonly IReadOnlyList<SmartSymbolDetector> detectors;

		public SmartSymbolData SmartSymbolData
		{
			get => smartSymbolData.Value;
		}

		public SmartSymbolStatus(string name, IReadOnlyList<SmartSymbolDetector> detectors) : base(name)
		{
			this.detectors = detectors;
		}

		public void RefreshSmartSymbolData(Stage currentStage)
		{
			foreach (var detector in detectors)
			{
				if (detector.TryFindSmartSymbols(currentStage, out var newData))
				{
					// The first trigger detector that finds a result has its results used.

					smartSymbolData.Value = newData;
					return;
				}
			}

			smartSymbolData.Value = SmartSymbolData.Empty;
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();
			smartSymbolData = AddProperty<SmartSymbolData>(nameof(SmartSymbolData), null);
		}
	}
}