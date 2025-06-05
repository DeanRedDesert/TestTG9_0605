using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Midas.Presentation.Audio
{
	[CreateAssetMenu(menuName = "Midas/Sound Doc", order = 100)]
	public sealed class SoundDoc : ScriptableObject
	{
		[SerializeField]
		private SoundDocData[] sounds;

		public IReadOnlyList<SoundDocData> GetSounds() => sounds.ToList();
	}

	[Serializable]
	public sealed class SoundDocData
	{
		[SerializeField]
		private string filename;

		[SerializeField]
		private string description;

		[SerializeField]
		private string transcript;

		[SerializeField]
		private string activationList;

		public string Filename => filename;
		public string Description => description;
		public string Transcript => transcript;
		public string ActivationList => activationList;

		public SoundDocData(string filename, string description, string transcript, string activationList)
		{
			this.filename = filename;
			this.description = description;
			this.transcript = transcript;
			this.activationList = activationList;
		}
	}
}