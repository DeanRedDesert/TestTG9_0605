using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Metadata;

namespace Midas.Fuel
{
	[Serializable]
	[DisplayName("Transcription")]
	[Metadata(AllowedTypes = MetadataType.AllTableEntries, AllowMultiple = false)]
	public sealed class TranscriptionMetadata : IMetadata
	{
		[Multiline]
		[SerializeField]
		private string text;

		public string Text => text;
	}
}