using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.Audio;
using Midas.Presentation.Data.PropertyReference;
using Midas.Presentation.Reels.SmartSymbols;
using UnityEngine;

namespace Midas.Presentation.Reels.Sound
{
	/// <summary>
	/// Use this smart symbol class if you want to play a single sound on each reel,
	/// for instance if you have a respin game that needs a sound when new locks land.
	/// </summary>
	[CreateAssetMenu(menuName = "Midas/Reels/Sound/Single Smart Stop Sound")]
	public sealed class SingleSmartStopSound : ReelStopSounds
	{
		[SerializeField]
		private PropertyReference<SmartSymbolData> smartSymbolDataReference;

		[SerializeField]
		private SoundId soundId;

		public override IReadOnlyList<SoundId> SoundIds => new[] { soundId };

		public override void DeInit()
		{
			smartSymbolDataReference.DeInit();
		}

		public override void Apply(ReelContainer reelContainer, IReadOnlyList<ISet<SoundId>> smartSoundSets)
		{
			var smartSymbolData = smartSymbolDataReference.Value;
			var smartCells = smartSymbolData.SmartCells;

			if (smartCells.Count == 0)
				return;

			var reelGroups = smartCells.Select(r => reelContainer.GetReelByCell(r.Row, r.Column)).Select(r => r.Group).Distinct().OrderBy(r => r).ToArray();

			for (var i = 0; i < reelGroups.Length; i++)
			{
				var group = reelGroups[i];

				var smartSoundSet = smartSoundSets[group];
				if (smartSoundSet.Count == 0)
					smartSoundSet.Add(soundId);
			}
		}
	}
}