using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Midas.Presentation.Audio
{
	public static class AudioService
	{
		private const int InitialAudioSourceCount = 10;
		private static Transform audioSourceParent;
		private static readonly Stack<AudioSource> availableAudioSources = new Stack<AudioSource>();
		private static readonly List<WeakReference<ISound>> sounds = new List<WeakReference<ISound>>();
		private static readonly List<ISound> pausedSounds = new List<ISound>();

		public static int SoundCount => sounds.Count;

		public static int TotalAudioSourcesCreated { get; private set; }

		public static int NumAudioSourcesCurrentlyAcquired { get; private set; }

		public static ISound CreateSound(SoundId soundId)
		{
			var snd = SoundDefinitionsDatabase.Instance.CreateSound(soundId);
			if (snd != null)
			{
				sounds.Add(new WeakReference<ISound>(snd));
			}

			return snd;
		}

		public static void DestroySound(ISound iSound)
		{
			if (iSound != null)
			{
				iSound.Destroy();
				for (var i = 0; i < sounds.Count; i++)
				{
					var weakRef = sounds[i];
					if (weakRef.TryGetTarget(out var target))
					{
						if (iSound == target)
						{
							sounds.RemoveAt(i);
							return;
						}
					}
				}
			}
		}

		public static void DestroySounds(IReadOnlyList<ISound> soundsToDestroy)
		{
			foreach (var sound in soundsToDestroy)
				DestroySound(sound);
		}

		public static void PauseAllSounds()
		{
			pausedSounds.Clear();
			foreach (var createdSound in sounds)
			{
				createdSound.TryGetTarget(out var sound);
				if (sound is { IsPlaying: true })
				{
					pausedSounds.Add(sound);
					sound.Pause();
				}
			}
		}

		public static void Init()
		{
			TotalAudioSourcesCreated = 0;
			NumAudioSourcesCurrentlyAcquired = 0;
			audioSourceParent = new GameObject("AudioSourceParent").transform;
			for (var i = 0; i < InitialAudioSourceCount; i++)
			{
				GenerateNewAudioSource();
			}
		}

		public static void DeInit()
		{
			StopAllSounds();

			if (audioSourceParent != null)
			{
				Object.Destroy(audioSourceParent.gameObject);
				audioSourceParent = null;
			}

			if (NumAudioSourcesCurrentlyAcquired > 0)
			{
				Log.Instance.Error($"There are still {NumAudioSourcesCurrentlyAcquired} AudioSources acquired");
			}

			availableAudioSources.Clear();
			pausedSounds.Clear();
			if (sounds.Count > 0)
			{
				Log.Instance.Warn($"There are still {sounds.Count} ISounds created");
				foreach (var createdSound in sounds)
				{
					if (createdSound.TryGetTarget(out var sound))
					{
						Log.Instance.Warn($"ISound still created: {sound.Id}");
					}
				}
			}

			sounds.Clear();
		}

		public static AudioSource AcquireAudioSource()
		{
			NumAudioSourcesCurrentlyAcquired++;
			if (availableAudioSources.Count == 0)
			{
				GenerateNewAudioSource();
			}

			var audioSource = availableAudioSources.Pop();
			return audioSource;
		}

		public static void ReleaseAudioSource(AudioSource source)
		{
			Log.Instance.Debug($"Returning audio source to pool {source.clip.name}");
			NumAudioSourcesCurrentlyAcquired--;
			source.Stop();
			source.clip = null;
			source.volume = 1.0f;
			source.pitch = 1.0f;
			source.panStereo = 0.0f;
			source.loop = false;
			source.outputAudioMixerGroup = null;
			availableAudioSources.Push(source);
		}

		public static void UnPauseAllSounds()
		{
			if (pausedSounds.Count > 0)
			{
				pausedSounds.ForEach(x => x.UnPause());
			}
		}

		private static void GenerateNewAudioSource()
		{
			if (audioSourceParent == null)
			{
				Log.Instance.Error("_audioSourceParent is NULL.");
			}
			else
			{
				var newGameObject = new GameObject("AudioSource" + audioSourceParent.childCount);
				newGameObject.transform.parent = audioSourceParent;
				var newSource = newGameObject.AddComponent<AudioSource>();
				newSource.playOnAwake = false;
				availableAudioSources.Push(newSource);
				TotalAudioSourcesCreated++;
			}
		}

		private static void StopAllSounds()
		{
			foreach (var createdSound in sounds)
			{
				if (createdSound.TryGetTarget(out var sound))
				{
					sound.Stop();
				}
			}
		}
	}
}