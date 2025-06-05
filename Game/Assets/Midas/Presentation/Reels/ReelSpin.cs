using System;

namespace Midas.Presentation.Reels
{
	public sealed class ReelSpin
	{
		private readonly ReelStrip reelStrip;
		private readonly Reel targetReel;
		private long stripIndex;

		public float SpinSpeed { get; }
		public SpinSettings Settings { get; }

		public ReelSpin(SpinSettings settings, ReelStrip strip, TimeSpan spinTime, Reel targetReel)
		{
			Settings = settings;
			reelStrip = strip;
			this.targetReel = targetReel;

			var actualSpinTime = spinTime.TotalSeconds - (settings.WindupTime + settings.RecoveryTime);
			var symbolCount = actualSpinTime * settings.SpinSpeed;
			var minSymbols = targetReel.TotalSymbolCount;
			var additionalDistance = settings.WindupDistance + settings.OvershootDistance;
			var totalSymbols = Math.Max(minSymbols, (int)Math.Round(symbolCount - additionalDistance));
			SpinSpeed = (float)((totalSymbols + additionalDistance) / actualSpinTime);
			stripIndex = totalSymbols - targetReel.SymbolsAbove;
		}

		public string GetNextSymbol()
		{
			if (stripIndex > -targetReel.SymbolsAbove)
			{
				stripIndex--;
				return reelStrip.GetSymbol(stripIndex);
			}

			return null;
		}

		public void Slam()
		{
			stripIndex = Math.Min(stripIndex, targetReel.VisibleSymbols + targetReel.SymbolsBelow);
		}
	}
}