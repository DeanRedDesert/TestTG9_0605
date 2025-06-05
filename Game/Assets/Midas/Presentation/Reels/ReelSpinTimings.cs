using System;
using System.Collections.Generic;
using UnityEngine;

namespace Midas.Presentation.Reels
{
	public abstract class ReelSpinTimings : ScriptableObject
	{
		public abstract IReadOnlyList<TimeSpan> GetStopTimings(TimeSpan spinTime, int groupCount, TimeSpan?[] overrideStopInterval);
		public abstract IReadOnlyList<SpinSettings> GetSpinSettings(int groupCount);
	}
}