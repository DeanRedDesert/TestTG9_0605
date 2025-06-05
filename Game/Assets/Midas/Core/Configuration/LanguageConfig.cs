using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Midas.Core.Configuration
{
	public sealed class LanguageConfig
	{
		public IReadOnlyList<string> AvailableLanguages { get; }

		public LanguageConfig(IEnumerable<string> availableLanguages)
		{
			AvailableLanguages = availableLanguages.ToArray();
		}

		public override string ToString()
		{
			return $"{string.Join(", ", AvailableLanguages)}";
		}

		public void Serialize(BinaryWriter writer, Action<BinaryWriter, object> serializeComplex)
		{
			serializeComplex(writer, AvailableLanguages);
		}

		public static LanguageConfig Deserialize(BinaryReader reader, Func<BinaryReader, object> deserializeComplex)
		{
			var availableLanguages = (IReadOnlyList<string>)deserializeComplex(reader);

			return new LanguageConfig(availableLanguages);
		}
	}
}