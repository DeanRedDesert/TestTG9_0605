// Copyright (c) 2022 IGT

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.General;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

#endregion

namespace Midas.Presentation.Audio
{
	[Serializable]
	public sealed class SoundDefinitionsDictionary : SerializableDictionary<string, SoundDefinitionsBase>
	{
	}

	public sealed class SoundDefinitionsDatabase : ScriptableSingleton<SoundDefinitionsDatabase>
	{
		[SerializeField]
		private AudioMixer[] audioMixers;

		[SerializeField]
		private AudioMixerGroup defaultAudioMixerGroup;

		[FormerlySerializedAs("audioDefinitions")]
		[SerializeField]
		private SoundDefinitionsDictionary soundDefinitions = new SoundDefinitionsDictionary();

		public IReadOnlyList<AudioMixer> AudioMixers => audioMixers;
		public AudioMixerGroup DefaultAudioMixerGroup => defaultAudioMixerGroup;
		public IReadOnlyDictionary<string, SoundDefinitionsBase> SoundDefinitions => soundDefinitions;

		public ISound CreateSound(SoundId soundId)
		{
			if (this.soundDefinitions.TryGetValue(soundId.DefinitionName, out var soundDefinitionsBase))
			{
				var snd = soundDefinitionsBase.CreateSound(soundId.Id);
				if (snd != null)
				{
					return snd;
				}

				Log.Instance.Error($@"There was no Sound by the ID {soundId.Id} in the {soundId.DefinitionName} definition!");
				return null;
			}

			Log.Instance.Error($@"There is no SoundDefinition file with the name '{soundId.DefinitionName}' found!");
			return null;
		}

		public string[] GetOnlySoundNames()
		{
			return soundDefinitions
				.Where(d => d.Value is SoundDefinitions)
				.SelectMany(d => d.Value.Ids.Select(id => $"{d.Key}/{id}"))
				.OrderBy(s => s)
				.ToArray();
		}

		public string[] GetOnlySequenceNames()
		{
			return soundDefinitions
				.Where(d => d.Value is SoundSequenceDefinitions)
				.SelectMany(d => d.Value.Ids.Select(id => $"{d.Key}/{id}"))
				.OrderBy(s => s)
				.ToArray();
		}

		public string[] GetAllSoundNames()
		{
			return soundDefinitions
				.SelectMany(d => d.Value.Ids.Select(id => $"{d.Key}/{id}"))
				.OrderBy(s => s)
				.ToArray();
		}

		public bool HasSound(SoundId soundId)
		{
			return this.soundDefinitions.TryGetValue(soundId.DefinitionName, out var soundDefinitions) &&
				soundDefinitions.HasSound(soundId.Id);
		}
	}
}