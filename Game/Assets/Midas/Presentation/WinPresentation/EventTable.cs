using System;
using System.Collections.Generic;
using Midas.Presentation.Sequencing;

namespace Midas.Presentation.WinPresentation
{
	public enum SequenceEvent
	{
		WinSequenceStart,
		BellSound,
		FrameLightAnimation,
		ReelScreenZoom,
		ReelScreenShake,
		ReelScreenShakeSound,
		BackgroundLoopSound,
		CoinFlight,
		WinMeterZoom,
		WinMeterEffect,
		WinDumpAndSpaghettiLines,
		WinDumpSound,
		WinIncrement,
		WinIncrementSound,
		FeatureWinIncrementSound,
		WinMessageAndAnim,
		WinSequenceComplete,
		EnumSize
	}

	public enum WinLevel
	{
		LNoCredit = -1,
		L0 = 0,
		L1,
		L2,
		L3,
		L4,
		L5,
		L6,
		EnumSize
	}

	public sealed class EventTable
	{
		private readonly List<(int customEventId, int winLevel, int intensity)> customIntensities = new List<(int, int, int)>();

		public int[,] Entries { get; } = new int[(int)SequenceEvent.EnumSize, (int)WinLevel.EnumSize];

		public EventTable()
		{
			Array.Clear(Entries, 0, Entries.Length);
			//default assignment according to GI 3.3 Core Video and MLP Curve
			//https://developers.confluence.igt.com/x/FgKgBQ
			Entries[(int)SequenceEvent.WinSequenceStart, (int)WinLevel.L0] = 1;
			Entries[(int)SequenceEvent.WinSequenceStart, (int)WinLevel.L1] = 1;
			Entries[(int)SequenceEvent.WinSequenceStart, (int)WinLevel.L2] = 1;
			Entries[(int)SequenceEvent.WinSequenceStart, (int)WinLevel.L3] = 1;
			Entries[(int)SequenceEvent.WinSequenceStart, (int)WinLevel.L4] = 1;
			Entries[(int)SequenceEvent.WinSequenceStart, (int)WinLevel.L5] = 1;
			Entries[(int)SequenceEvent.WinSequenceStart, (int)WinLevel.L6] = 1;

			Entries[(int)SequenceEvent.BellSound, (int)WinLevel.L4] = 1;
			Entries[(int)SequenceEvent.BellSound, (int)WinLevel.L5] = 1;
			Entries[(int)SequenceEvent.BellSound, (int)WinLevel.L6] = 2;

			Entries[(int)SequenceEvent.FrameLightAnimation, (int)WinLevel.L3] = 1;
			Entries[(int)SequenceEvent.FrameLightAnimation, (int)WinLevel.L4] = 1;
			Entries[(int)SequenceEvent.FrameLightAnimation, (int)WinLevel.L5] = 2;
			Entries[(int)SequenceEvent.FrameLightAnimation, (int)WinLevel.L6] = 3;

			Entries[(int)SequenceEvent.ReelScreenZoom, (int)WinLevel.L5] = 1;
			Entries[(int)SequenceEvent.ReelScreenZoom, (int)WinLevel.L6] = 2;

			Entries[(int)SequenceEvent.ReelScreenShake, (int)WinLevel.L4] = 1;
			Entries[(int)SequenceEvent.ReelScreenShake, (int)WinLevel.L5] = 2;
			Entries[(int)SequenceEvent.ReelScreenShake, (int)WinLevel.L6] = 3;

			Entries[(int)SequenceEvent.ReelScreenShakeSound, (int)WinLevel.L4] = 1;
			Entries[(int)SequenceEvent.ReelScreenShakeSound, (int)WinLevel.L5] = 1;
			Entries[(int)SequenceEvent.ReelScreenShakeSound, (int)WinLevel.L6] = 1;

			Entries[(int)SequenceEvent.BackgroundLoopSound, (int)WinLevel.L3] = 1;
			Entries[(int)SequenceEvent.BackgroundLoopSound, (int)WinLevel.L4] = 1;
			Entries[(int)SequenceEvent.BackgroundLoopSound, (int)WinLevel.L5] = 1;
			Entries[(int)SequenceEvent.BackgroundLoopSound, (int)WinLevel.L6] = 1;

			Entries[(int)SequenceEvent.CoinFlight, (int)WinLevel.L4] = 1;
			Entries[(int)SequenceEvent.CoinFlight, (int)WinLevel.L5] = 2;
			Entries[(int)SequenceEvent.CoinFlight, (int)WinLevel.L6] = 3;

			Entries[(int)SequenceEvent.WinMeterZoom, (int)WinLevel.L2] = 1;
			Entries[(int)SequenceEvent.WinMeterZoom, (int)WinLevel.L3] = 2;
			Entries[(int)SequenceEvent.WinMeterZoom, (int)WinLevel.L4] = 2;
			Entries[(int)SequenceEvent.WinMeterZoom, (int)WinLevel.L5] = 3;
			Entries[(int)SequenceEvent.WinMeterZoom, (int)WinLevel.L6] = 4;

			Entries[(int)SequenceEvent.WinMeterEffect, (int)WinLevel.L2] = 1;
			Entries[(int)SequenceEvent.WinMeterEffect, (int)WinLevel.L3] = 2;
			Entries[(int)SequenceEvent.WinMeterEffect, (int)WinLevel.L4] = 2;
			Entries[(int)SequenceEvent.WinMeterEffect, (int)WinLevel.L5] = 3;
			Entries[(int)SequenceEvent.WinMeterEffect, (int)WinLevel.L6] = 4;

			Entries[(int)SequenceEvent.WinDumpAndSpaghettiLines, (int)WinLevel.L0] = 1;
			Entries[(int)SequenceEvent.WinDumpSound, (int)WinLevel.L0] = 1;

			Entries[(int)SequenceEvent.WinIncrement, (int)WinLevel.L1] = 1;
			Entries[(int)SequenceEvent.WinIncrement, (int)WinLevel.L2] = 1;
			Entries[(int)SequenceEvent.WinIncrement, (int)WinLevel.L3] = 1;
			Entries[(int)SequenceEvent.WinIncrement, (int)WinLevel.L4] = 1;
			Entries[(int)SequenceEvent.WinIncrement, (int)WinLevel.L5] = 1;
			Entries[(int)SequenceEvent.WinIncrement, (int)WinLevel.L6] = 1;

			Entries[(int)SequenceEvent.WinIncrementSound, (int)WinLevel.L1] = 1;
			Entries[(int)SequenceEvent.WinIncrementSound, (int)WinLevel.L2] = 2;
			Entries[(int)SequenceEvent.WinIncrementSound, (int)WinLevel.L3] = 3;
			Entries[(int)SequenceEvent.WinIncrementSound, (int)WinLevel.L4] = 4;
			Entries[(int)SequenceEvent.WinIncrementSound, (int)WinLevel.L5] = 5;
			Entries[(int)SequenceEvent.WinIncrementSound, (int)WinLevel.L6] = 6;

			Entries[(int)SequenceEvent.FeatureWinIncrementSound, (int)WinLevel.L1] = 1;
			Entries[(int)SequenceEvent.FeatureWinIncrementSound, (int)WinLevel.L2] = 2;
			Entries[(int)SequenceEvent.FeatureWinIncrementSound, (int)WinLevel.L3] = 3;
			Entries[(int)SequenceEvent.FeatureWinIncrementSound, (int)WinLevel.L4] = 4;
			Entries[(int)SequenceEvent.FeatureWinIncrementSound, (int)WinLevel.L5] = 5;
			Entries[(int)SequenceEvent.FeatureWinIncrementSound, (int)WinLevel.L6] = 6;

			Entries[(int)SequenceEvent.WinMessageAndAnim, (int)WinLevel.L4] = 4;
			Entries[(int)SequenceEvent.WinMessageAndAnim, (int)WinLevel.L5] = 5;
			Entries[(int)SequenceEvent.WinMessageAndAnim, (int)WinLevel.L6] = 6;

			Entries[(int)SequenceEvent.WinSequenceComplete, (int)WinLevel.L1] = 1;
			Entries[(int)SequenceEvent.WinSequenceComplete, (int)WinLevel.L2] = 1;
			Entries[(int)SequenceEvent.WinSequenceComplete, (int)WinLevel.L3] = 1;
			Entries[(int)SequenceEvent.WinSequenceComplete, (int)WinLevel.L4] = 1;
			Entries[(int)SequenceEvent.WinSequenceComplete, (int)WinLevel.L5] = 1;
			Entries[(int)SequenceEvent.WinSequenceComplete, (int)WinLevel.L6] = 1;
		}

		public int GetIntensity(int sequenceEventId, int winLevel)
		{
			if (sequenceEventId < CustomEvent.CustomEventStartId && sequenceEventId < Entries.GetLength(0))
			{
				if (winLevel == -1)
				{
					return 0;
				}

				return Entries[sequenceEventId, winLevel];
			}

			if (sequenceEventId >= CustomEvent.CustomEventStartId)
			{
				return GetCustomIntensity(sequenceEventId, winLevel);
			}

			if (winLevel == -1)
			{
				return 0;
			}

			Log.Instance.Error("sequenceEventId is not a CustomEvent and not a default SequenceEvent");
			return 0;
		}

		public void SetCustomIntensity(int customEventId, int winLevel, int intensity)
		{
			for (int i = 0; i < customIntensities.Count; ++i)
			{
				var tuple = customIntensities[i];
				if (tuple.customEventId == customEventId && tuple.winLevel == winLevel)
				{
					tuple.intensity = intensity;
					customIntensities[i] = tuple;
					return;
				}
			}

			customIntensities.Add((customEventId, winLevel, intensity));
		}

		public int GetCustomIntensity(int customEventId, int winLevel)
		{
			foreach ((int eventId, int winLevel1, int intensity) in customIntensities)
			{
				if (eventId == customEventId && winLevel1 == winLevel)
				{
					return intensity;
				}
			}

			//by default, the CustomEvents return same intensity as follows
			//WinLevel0= 1
			//WinLevel1= 1
			//WinLevel2= 2
			//WinLevel3= 3
			//WinLevel4= 4
			//WinLevel5= 5
			//WinLevel5= 6
			if (winLevel == -1)
			{
				return 0;
			}

			return winLevel == 0 ? 1 : winLevel;
		}


	}
}