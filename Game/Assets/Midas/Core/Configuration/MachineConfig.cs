using System;
using System.IO;
using Midas.Core.General;

namespace Midas.Core.Configuration
{
	public sealed class MachineConfig
	{
		public string Jurisdiction { get; }
		public bool IsShowMode { get; }
		public bool AreShowFeaturesEnabled { get; }
		public bool AreDevFeaturesEnabled { get; }
		public bool IsStandardBuild { get; }
		public Money ShowMinimumCredits { get; }

		public MachineConfig(string jurisdiction, bool isShowMode, bool areShowFeaturesEnabled, bool areDevFeaturesEnabled, bool isStandardBuild, Money showMinimumCredits)
		{
			Jurisdiction = jurisdiction;
			IsShowMode = isShowMode;
			AreShowFeaturesEnabled = areShowFeaturesEnabled;
			AreDevFeaturesEnabled = areDevFeaturesEnabled;
			IsStandardBuild = isStandardBuild;
			ShowMinimumCredits = showMinimumCredits;
		}

		public override string ToString()
		{
			return $"'{Jurisdiction}', {AreShowFeaturesEnabled}, {AreDevFeaturesEnabled}, {IsStandardBuild}, {ShowMinimumCredits}";
		}

		public void Serialize(BinaryWriter writer, Action<BinaryWriter, object> serializeComplex)
		{
			writer.Write(Jurisdiction);
			writer.Write(IsShowMode);
			writer.Write(AreShowFeaturesEnabled);
			writer.Write(AreDevFeaturesEnabled);
			writer.Write(IsStandardBuild);
			serializeComplex(writer, ShowMinimumCredits);
		}

		public static MachineConfig Deserialize(BinaryReader reader, Func<BinaryReader, object> deserializeComplex)
		{
			var jurisdiction = reader.ReadString();
			var isShowMode = reader.ReadBoolean();
			var areShowFeaturesEnabled = reader.ReadBoolean();
			var areDevFeaturesEnabled = reader.ReadBoolean();
			var isStandardBuild = reader.ReadBoolean();
			var showMinimumCredits = (Money)deserializeComplex(reader);

			return new MachineConfig(jurisdiction, isShowMode, areShowFeaturesEnabled, areDevFeaturesEnabled, isStandardBuild, showMinimumCredits);
		}
	}
}