using System;
using System.IO;
using Midas.Core.General;

namespace Midas.Core.Configuration
{
	public sealed class AncillaryConfig
	{
		public bool Enabled { get; }
		public Money MoneyLimit { get; }
		public long CycleLimit { get; }

		public AncillaryConfig(bool enabled, Money moneyLimit, long cycleLimit)
		{
			Enabled = enabled;
			MoneyLimit = moneyLimit;
			CycleLimit = cycleLimit;
		}

		public override string ToString()
		{
			return $"{Enabled}, {MoneyLimit}, {CycleLimit}";
		}

		public void Serialize(BinaryWriter writer, Action<BinaryWriter, object> serializeComplex)
		{
			writer.Write(Enabled);
			serializeComplex(writer, MoneyLimit);
			writer.Write(CycleLimit);
		}

		public static AncillaryConfig Deserialize(BinaryReader reader, Func<BinaryReader, object> deserializeComplex)
		{
			var enabled = reader.ReadBoolean();
			var moneyLimit = (Money)deserializeComplex(reader);
			var cycleLimit = reader.ReadInt64();

			return new AncillaryConfig(enabled, moneyLimit, cycleLimit);
		}
	}
}