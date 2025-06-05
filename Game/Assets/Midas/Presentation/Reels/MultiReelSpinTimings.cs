using System;
using System.Collections.Generic;
using Midas.Core;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using UnityEngine;

namespace Midas.Presentation.Reels
{
	[CreateAssetMenu(menuName = "Midas/Reels/Multi Spin Timings")]
	public sealed class MultiReelSpinTimings : ReelSpinTimings
	{
		#region Inspector Fields

		[Tooltip("The minimum time that a reel can spin in seconds. Stop timings may be adjusted to fit this.")]
		[SerializeField]
		private ReelSpinTimings AnzSpinTimings;

		[Tooltip("The minimum time that a reel can spin in seconds. Stop timings may be adjusted to fit this.")]
		[SerializeField]
		private ReelSpinTimings NormalSpinTimings;

		[Tooltip("The minimum time that a reel can spin in seconds. Stop timings may be adjusted to fit this.")]
		[SerializeField]
		private ReelSpinTimings FastSpinTimings;

		[Tooltip("The minimum time that a reel can spin in seconds. Stop timings may be adjusted to fit this.")]
		[SerializeField]
		private ReelSpinTimings SuperFastSpinTimings;

		#endregion

		#region Overrides of ReelSpinTimings

		public override IReadOnlyList<TimeSpan> GetStopTimings(TimeSpan spinTime, int groupCount, TimeSpan?[] overrideStopInterval)
		{
			return GetSpinTimings().GetStopTimings(spinTime, groupCount, overrideStopInterval);
		}

		public override IReadOnlyList<SpinSettings> GetSpinSettings(int columnCount)
		{
			return GetSpinTimings().GetSpinSettings(columnCount);
		}

		#endregion

		private ReelSpinTimings GetSpinTimings()
		{
			if (StatusDatabase.ConfigurationStatus.GameIdentity?.IsGlobalGi() != true)
				return AnzSpinTimings;

			switch (StatusDatabase.GameSpeedStatus.GameSpeed)
			{
				default:
				case GameSpeed.Normal:
					return NormalSpinTimings;
				case GameSpeed.Fast:
					return FastSpinTimings;
				case GameSpeed.SuperFast:
					return SuperFastSpinTimings;
			}
		}
	}
}