using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Midas.Presentation.Audio
{
	[CreateAssetMenu(fileName = "NewSoundDefinitions", menuName = "Midas/Audio/Create New Sound Definitions")]
	public sealed class SoundDefinitions : SoundDefinitionsBase
	{
		[SerializeField]
		private List<SoundDefinition> soundDefinitionsList = new List<SoundDefinition>();

		public override IEnumerable<string> Ids => soundDefinitionsList.Select(s => s.Id);

		public override bool HasSound(string id)
		{
			return soundDefinitionsList.Any(s => s.Id == id);
		}

		public override ISound CreateSound(string id)
		{
			foreach (var soundDefinition in soundDefinitionsList)
			{
				if (soundDefinition.Id == id)
				{
					return new SoundSingle(soundDefinition);
				}
			}

			return null;
		}
	}
}