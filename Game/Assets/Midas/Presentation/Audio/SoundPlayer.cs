using Midas.Presentation.ExtensionMethods;
using UnityEngine;

namespace Midas.Presentation.Audio
{
	public sealed class SoundPlayer : SoundPlayerBase
	{
		private SoundId usedSoundId;
		private ISound sound;

		[SerializeField]
		private SoundId soundId = new SoundId();

		public SoundId SoundId => soundId;

		public override float Volume
		{
			get => Sound.Volume;
			set => Sound.Volume = value;
		}

		public override ISound Sound
		{
			get
			{
				if (!soundId.IsValid)
					return null;

				if (Application.isEditor && !Application.isPlaying)
				{
					var accessor = SoundDefinitionsDatabase.Instance;
					return accessor.CreateSound(soundId);
				}

				if (sound == null || !usedSoundId.Equals(soundId))
				{
					AudioService.DestroySound(sound);
					sound = AudioService.CreateSound(soundId);

					if (sound == null)
					{
						Log.Instance.Fatal($"Sound '{soundId}' not found in {gameObject.GetPath()}", this);
					}

					usedSoundId = new SoundId(soundId.Id, soundId.DefinitionName);
				}

				return sound;
			}
		}

		private void OnDestroy()
		{
			if (Application.isEditor && !Application.isPlaying && sound != null)
			{
				sound.Destroy();
				sound = null;
			}
			else if (sound != null)
			{
				AudioService.DestroySound(sound);
				sound = null;
			}
		}
	}
}