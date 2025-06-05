using System.Collections.Generic;
using Midas.Core.General;

namespace Midas.LogicToPresentation.Messages
{
	public sealed class UtilityModeDetailsMessage : IMessage
	{
		public bool IsAvailable { get; }
		public IReadOnlyList<string> RegistrySupportedThemes { get; }
		public IReadOnlyDictionary<KeyValuePair<string, string>, IReadOnlyList<long>> RegistrySupportedDenominations { get; }

		public UtilityModeDetailsMessage(bool isAvailable, IReadOnlyList<string> registrySupportedThemes, IReadOnlyDictionary<KeyValuePair<string, string>, IReadOnlyList<long>> registrySupportedDenominations)
		{
			IsAvailable = isAvailable;
			RegistrySupportedThemes = registrySupportedThemes;
			RegistrySupportedDenominations = registrySupportedDenominations;
		}
	}
}