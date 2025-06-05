using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Midas.Presentation.Audio
{
	[Serializable]
	internal sealed class SoundSequenceDefinition
	{
		[FormerlySerializedAs("_id")]
		[SerializeField]
		internal string id = string.Empty;

		[FormerlySerializedAs("_looped")]
		[SerializeField]
		internal bool looped;

		[FormerlySerializedAs("_soundIdsList")]
		[SerializeField]
		internal List<SoundId> soundIdsList = new List<SoundId>();

		public override string ToString()
		{
			return $"{id}, {soundIdsList.Count}, {looped}";
		}
	}
}