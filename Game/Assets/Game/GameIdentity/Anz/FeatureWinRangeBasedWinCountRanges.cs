using System;
using Midas.Core.General;
using Midas.Presentation.Audio;
using Midas.Presentation.Data;
using Midas.Presentation.WinPresentation;

namespace Game.GameIdentity.Anz
{
	public sealed class FeatureWinRangeBasedWinCountRanges : IWinCountRanges
	{
		private readonly IWinCountRanges defaultWinRanges;
		private readonly IWinCountRanges featureWinRanges;
		private readonly int useDefaultAtWinLevel;

		public FeatureWinRangeBasedWinCountRanges(IWinCountRanges defaultWinRanges, IWinCountRanges featureWinRanges, int useDefaultAtWinLevel)
		{
			this.defaultWinRanges = defaultWinRanges;
			this.featureWinRanges = featureWinRanges;
			this.useDefaultAtWinLevel = useDefaultAtWinLevel;
		}

		public bool IsSequenceEligible(int sequenceId)
		{
			var player = FeatureMusicPlayer.GetFirstActiveInstance();
			var useFeatureIncrementSounds = player is { AllowsInteractionDuringWins: true } && StatusDatabase.WinPresentationStatus.CurrentWinLevel < useDefaultAtWinLevel;
			return sequenceId switch
			{
				(int)SequenceEvent.FeatureWinIncrementSound => useFeatureIncrementSounds,
				(int)SequenceEvent.WinIncrementSound => !useFeatureIncrementSounds,
				_ => true
			};
		}

		public (int WinCountLevel, TimeSpan Duration, TimeSpan Delay) GetWinCountLevel(Credit winAmount, Credit betAmount)
		{
			var player = FeatureMusicPlayer.GetFirstActiveInstance();
			var useFeatureIncrementSounds = player is { AllowsInteractionDuringWins: true } && StatusDatabase.WinPresentationStatus.CurrentWinLevel < useDefaultAtWinLevel;
			return useFeatureIncrementSounds ? featureWinRanges.GetWinCountLevel(winAmount, betAmount) : defaultWinRanges.GetWinCountLevel(winAmount, betAmount);
		}
	}
}