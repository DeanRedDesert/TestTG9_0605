using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.Audio;
using Midas.Presentation.Data.PropertyReference;
using Midas.Presentation.Reels.SmartSymbols;
using UnityEngine;

namespace Midas.Presentation.Reels.Sound
{
	/// <summary>
	/// This is the typical smart stop sounds that are ordered to grow in intensity as you get more triggering symbols.
	/// </summary>
	[CreateAssetMenu(menuName = "Midas/Reels/Sound/Ordered Smart Stop Sounds")]
	public sealed class OrderedSmartStopSounds : ReelStopSounds
	{
		[SerializeField]
		private PropertyReference<SmartSymbolData> smartSymbolDataReference;

		[SerializeField]
		private List<SoundId> soundIds;

		public override IReadOnlyList<SoundId> SoundIds => soundIds;

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
					smartSoundSet.Add(soundIds[i]);
			}
		}
	}
}