using System;
using System.Runtime.Serialization;

namespace Midas.Ascent
{
	[Serializable]
	internal sealed class StopForcedException : Exception
	{
		public StopForcedException(string message)
			: base(message)
		{
		}

		private StopForcedException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}