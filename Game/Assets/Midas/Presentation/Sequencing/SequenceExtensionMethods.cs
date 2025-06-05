using System.Collections.Generic;
using Midas.Core.Coroutine;

namespace Midas.Presentation.Sequencing
{
	public static class SequenceExtensionMethods
	{
		public static IEnumerator<CoroutineInstruction> Run(this Sequence sequence)
		{
			sequence.Start();
			while (sequence.IsActive)
				yield return null;
		}
	}
}