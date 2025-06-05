using Midas.Presentation.Audio;
using Midas.Presentation.Data.PropertyReference;
using Midas.Presentation.Reels.SmartSymbols;
using UnityEngine;

namespace Midas.Presentation.Reels.Sound
{
	[CreateAssetMenu(menuName = "Midas/Reels/Sound/Trigger Qualifying Sound")]
	public sealed class ReelStopTriggerQualifyingSound : ScriptableObject
	{
		[SerializeField]
		private PropertyReference<SmartSymbolData> smartSymbolDataReference;

		[field: SerializeField] public SoundId SoundId { get; private set; }

		public void DeInit()
		{
			smartSymbolDataReference.DeInit();
		}

		public SoundId GetQualifyingStopSound(ReelContainer container, int reelGroupIndex)
		{
			var smartSymbolData = smartSymbolDataReference.Value;

			if (!smartSymbolData.QualifyingReelIndex.HasValue)
				return null;

			return container.Reels[smartSymbolData.QualifyingReelIndex.Value].Group == reelGroupIndex ? SoundId : null;
		}
	}
}