using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.ExtensionMethods;
using UnityEngine;

namespace Midas.Presentation.Audio
{
	public sealed class SoundPlayerOneShot : MonoBehaviour
	{
		private List<ISound> soundCache = new List<ISound>();

		[SerializeField]
		private SoundId soundId = new SoundId();

		/// <summary>
		/// Plays the specified sound. Each time Play is called a new sound is started
		/// </summary>
		public void Play()
		{
			GetSound()?.Play();
		}

		private ISound GetSound()
		{
			var sound = soundCache.FirstOrDefault(s => !s.IsPlaying);
			if (sound != null)
			{
				return sound;
			}

			sound = AudioService.CreateSound(soundId);
			if (sound == null)
			{
				Log.Instance.Fatal($"Could not find sound '{soundId}' at {gameObject.GetPath()}", this);
			}

			soundCache.Add(sound);

			return sound;
		}

		private void OnDestroy()
		{
			if (soundCache != null)
			{
				foreach (var iSound in soundCache)
				{
					AudioService.DestroySound(iSound);
				}

				soundCache = null;
			}
		}
	}
}