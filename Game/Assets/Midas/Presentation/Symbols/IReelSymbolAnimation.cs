using Midas.Presentation.Data.StatusBlocks;

namespace Midas.Presentation.Symbols
{
	public interface IReelSymbolAnimation
	{
		/// <summary>
		/// Play a symbol animation.
		/// </summary>
		/// <remarks>
		/// The winInfo parameter will be null if the game is animating a pre-show-win prize or if the game is using AllWinningSymbolAnimator for its symbol animations.
		/// </remarks>
		/// <param name="winInfo">The winInfo if the game is cycling wins individually, or null if not.</param>
		void Play(IWinInfo winInfo);

		/// <summary>
		/// Stop animating.
		/// </summary>
		void Stop();
	}
}