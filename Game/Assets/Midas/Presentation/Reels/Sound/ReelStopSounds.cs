using System.Collections.Generic;
using Midas.Presentation.Audio;
using UnityEngine;

namespace Midas.Presentation.Reels.Sound
{
	public abstract class ReelStopSounds : ScriptableObject
	{
		public abstract IReadOnlyList<SoundId> SoundIds { get; }

		public abstract void DeInit();
		public abstract void Apply(ReelContainer reelContainer, IReadOnlyList<ISet<SoundId>> smartSoundSets);
	}
}