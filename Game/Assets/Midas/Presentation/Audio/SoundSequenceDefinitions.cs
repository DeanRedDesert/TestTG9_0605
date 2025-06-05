using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Midas.Presentation.Audio
{
	[CreateAssetMenu(fileName = "NewSoundSequenceDefinitions", menuName = "Midas/Audio/Create New Sound Sequence Definitions")]
	public sealed class SoundSequenceDefinitions : SoundDefinitionsBase
	{
		[FormerlySerializedAs("_sequenceDefinitionsList")]
		[SerializeField]
		private List<SoundSequenceDefinition> sequenceDefinitionsList = new List<SoundSequenceDefinition>();

		public override IEnumerable<string> Ids => sequenceDefinitionsList.Select(s => s.id);

		public override bool HasSound(string id)
		{
			return sequenceDefinitionsList.Any(s => s.id == id);
		}

		public override ISound CreateSound(string id)
		{
			foreach (var soundSequenceDefinition in sequenceDefinitionsList)
			{
				if (soundSequenceDefinition.id == id)
				{
					return new SoundSequence(soundSequenceDefinition);
				}
			}

			return null;
		}
	}
}