using System.Collections.Generic;
using UnityEngine;

namespace Midas.Presentation.Audio
{
	public abstract class SoundDefinitionsBase : ScriptableObject
	{
		public abstract bool HasSound(string id);
		public abstract ISound CreateSound(string id);
		public abstract IEnumerable<string> Ids { get; }
	}
}