namespace Midas.Presentation.Reels.SmartSymbols
{
	public interface IReelStopAnimation
	{
		bool IsReelStopAnimationFinished { get; }

		void PlayReelStopAnimation();

		void StopReelStopAnimation();
	}
}