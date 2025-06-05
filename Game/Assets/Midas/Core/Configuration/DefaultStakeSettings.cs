using System.IO;

namespace Midas.Core.Configuration
{
	public sealed class DefaultStakeSettings
	{
		public bool? UseMaximumNumberOfLines { get; }
		public bool? UseMaximumBetMultiplier { get; }
		public bool? IncludeSideBet { get; }

		public DefaultStakeSettings(bool? useMaximumNumberOfLines, bool? useMaximumBetMultiplier, bool? includeSideBet)
		{
			UseMaximumNumberOfLines = useMaximumNumberOfLines;
			UseMaximumBetMultiplier = useMaximumBetMultiplier;
			IncludeSideBet = includeSideBet;
		}

		public override string ToString() => $"{UseMaximumNumberOfLines}, {UseMaximumBetMultiplier}, {IncludeSideBet}";

		public void Serialize(BinaryWriter writer)
		{
			writer.Write(UseMaximumNumberOfLines.HasValue);
			writer.Write(UseMaximumNumberOfLines.HasValue && UseMaximumNumberOfLines.Value);
			writer.Write(UseMaximumBetMultiplier.HasValue);
			writer.Write(UseMaximumBetMultiplier.HasValue && UseMaximumBetMultiplier.Value);
			writer.Write(IncludeSideBet.HasValue);
			writer.Write(IncludeSideBet.HasValue && IncludeSideBet.Value);
		}

		public static DefaultStakeSettings Deserialize(BinaryReader reader)
		{
			var mnol = ReadNullable();
			var mbm = ReadNullable();
			var isb = ReadNullable();

			return new DefaultStakeSettings(mnol, mbm, isb);

			bool? ReadNullable()
			{
				var defined = reader.ReadBoolean();
				var t = reader.ReadBoolean();
				bool? b = null;
				if (defined)
					b = t;
				return b;
			}
		}
	}
}