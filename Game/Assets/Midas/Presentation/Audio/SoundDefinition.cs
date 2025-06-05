// Copyright (c) 2022 IGT

#region Usings

using System;
using UnityEngine;
using UnityEngine.Audio;

#endregion

namespace Midas.Presentation.Audio
{
	[Serializable]
	public sealed class SoundDefinition
	{
		[SerializeField]
		private string id = string.Empty;

		[SerializeField]
		private AudioClip clip;

		[SerializeField]
		private bool isLooped;

		[SerializeField]
		private float volume = 1f;

		[SerializeField]
		private AudioMixerGroup group;

		internal string Id => id;
		internal AudioClip Clip => clip;
		internal bool IsLooped => isLooped;
		internal float Volume => volume;
		internal AudioMixerGroup Group => group;

		internal SoundDefinition CreateWithChangedClip(AudioClip newClip)
		{
			return new SoundDefinition { id = Id, clip = newClip, isLooped = IsLooped, volume = Volume, group = Group };
		}

		public override string ToString()
		{
			return $"{id}, {isLooped}, {volume}";
		}
	}
}