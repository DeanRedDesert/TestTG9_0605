using System;
using System.IO;
using Midas.Core.Serialization;

namespace Midas.Core.Configuration
{
	public sealed partial class ConfigData
	{
		private sealed class CustomSerializer : ICustomSerializer
		{
			public bool SupportsType(Type t) => t == typeof(ConfigData);

			public void Serialize(BinaryWriter writer, Action<BinaryWriter, object> serializeComplex, object o)
			{
				var configData = (ConfigData)o;

				configData.MachineConfig.Serialize(writer, serializeComplex);
				configData.CurrencyConfig.Serialize(writer);
				configData.DenomConfig.Serialize(writer, serializeComplex);
				configData.AncillaryConfig.Serialize(writer, serializeComplex);
				configData.LanguageConfig.Serialize(writer, serializeComplex);
				configData.GameConfig.Serialize(writer, serializeComplex);
				configData.CustomConfig.Serialize(writer, serializeComplex);
			}

			public object Deserialize(Type t, BinaryReader reader, Func<BinaryReader, object> deserializeComplex)
			{
				var machineConfig = MachineConfig.Deserialize(reader, deserializeComplex);
				var currencyConfig = CurrencyConfig.Deserialize(reader);
				var denomConfig = DenomConfig.Deserialize(reader, deserializeComplex);
				var ancillaryConfig = AncillaryConfig.Deserialize(reader, deserializeComplex);
				var languageConfig = LanguageConfig.Deserialize(reader, deserializeComplex);
				var gameConfig = GameConfig.Deserialize(reader, deserializeComplex);
				var customConfig = CustomConfig.Deserialize(reader, deserializeComplex);

				return new ConfigData(machineConfig, currencyConfig, denomConfig, ancillaryConfig, languageConfig, gameConfig, customConfig);
			}
		}

		static ConfigData()
		{
			NvramSerializer.RegisterCustomSerializer(new CustomSerializer());
		}
	}
}