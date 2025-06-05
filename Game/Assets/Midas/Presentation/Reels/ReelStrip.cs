using System;

namespace Midas.Presentation.Reels
{
	/// <summary>
	/// A reel strip implementation for spinning where the 0 index is where the reel stops. Supports wrapping positively and negatively around the strip.
	/// </summary>
	public sealed class ReelStrip
	{
		private readonly Func<long, string> getSymbol;
		private readonly long stripLength;
		private readonly long stopIndex;

		public ReelStrip(Func<long, string> getSymbol, long stripLength, long stopIndex)
		{
			this.getSymbol = getSymbol;
			this.stripLength = stripLength;
			this.stopIndex = stopIndex;
		}

		public string GetSymbol(long index)
		{
			var realReelPos = (index + stopIndex) % stripLength;
			if (realReelPos < 0)
				realReelPos += stripLength;
			return getSymbol(realReelPos);
		}
	}
}