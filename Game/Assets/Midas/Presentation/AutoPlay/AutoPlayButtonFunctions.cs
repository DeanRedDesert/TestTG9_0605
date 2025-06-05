using Midas.Presentation.ButtonHandling;

namespace Midas.Presentation.AutoPlay
{
	[ButtonFunctions("AutoPlay")]
	public static class AutoPlayButtonFunctions
	{
		public static bool IsStartAutoplayOrConfirmButtonFunction(this ButtonFunction buttonFunction)
		{
			return StartAutoPlay.Equals(buttonFunction) || AutoPlayConfirmYes.Equals(buttonFunction);
		}

		public static ButtonFunction StartAutoPlay { get; } = ButtonFunction.Create(ButtonFunctions.AutoplayBase + 0);
		public static ButtonFunction StopAutoPlay { get; } = ButtonFunction.Create(ButtonFunctions.AutoplayBase + 1);
		public static ButtonFunction AutoPlayConfirmYes { get; } = ButtonFunction.Create(ButtonFunctions.AutoplayBase + 2);
		public static ButtonFunction AutoPlayConfirmNo { get; } = ButtonFunction.Create(ButtonFunctions.AutoplayBase + 3);
	}
}