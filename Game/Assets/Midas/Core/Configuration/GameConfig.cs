using System;
using System.IO;
using Midas.Core.General;

namespace Midas.Core.Configuration
{
	public sealed class GameConfig
	{
		public bool IsPlayerSelectableAutoplayAllowed { get; }
		public bool IsAutoPlayConfirmationRequired { get; }
		public bool IsAutoPlayChangeSpeedAllowed { get; }
		public bool IsCreditPlayoffEnabled { get; }
		public Credit MinBetLimit { get; }
		public Credit MaxBetLimit { get; }
		public DefaultStakeSettings DefaultStakeSettings { get; }

		public GameConfig(bool isPlayerSelectableAutoplayAllowed, bool isAutoPlayConfirmationRequired, bool isAutoplayChangeSpeedAllowed, bool isCreditPlayoffEnabled,
			Credit minBetLimit, Credit maxBetLimit, DefaultStakeSettings defaultStakeSettings)
		{
			IsPlayerSelectableAutoplayAllowed = isPlayerSelectableAutoplayAllowed;
			IsAutoPlayConfirmationRequired = isAutoPlayConfirmationRequired;
			IsAutoPlayChangeSpeedAllowed = isAutoplayChangeSpeedAllowed;
			IsCreditPlayoffEnabled = isCreditPlayoffEnabled;
			MinBetLimit = minBetLimit;
			MaxBetLimit = maxBetLimit;
			DefaultStakeSettings = defaultStakeSettings;
		}

		public override string ToString()
		{
			return $"{IsPlayerSelectableAutoplayAllowed}, {IsAutoPlayConfirmationRequired}, {IsAutoPlayChangeSpeedAllowed}, {IsCreditPlayoffEnabled}, {MinBetLimit}, {MaxBetLimit}, {DefaultStakeSettings}";
		}

		public void Serialize(BinaryWriter writer, Action<BinaryWriter, object> serializeComplex)
		{
			writer.Write(IsPlayerSelectableAutoplayAllowed);
			writer.Write(IsAutoPlayConfirmationRequired);
			writer.Write(IsAutoPlayChangeSpeedAllowed);
			writer.Write(IsCreditPlayoffEnabled);
			serializeComplex(writer, MinBetLimit);
			serializeComplex(writer, MaxBetLimit);
			DefaultStakeSettings.Serialize(writer);
		}

		public static GameConfig Deserialize(BinaryReader reader, Func<BinaryReader, object> deserializeComplex)
		{
			var isPlayerSelectableAutoplayAllowed = reader.ReadBoolean();
			var isAutoPlayConfirmationRequired = reader.ReadBoolean();
			var isAutoPlayChangeSpeedAllowed = reader.ReadBoolean();
			var isCreditPlayoffEnabled = reader.ReadBoolean();
			var minBetLimit = (Credit)deserializeComplex(reader);
			var maxBetLimit = (Credit)deserializeComplex(reader);
			var defaultStakeSettings = DefaultStakeSettings.Deserialize(reader);

			return new GameConfig(isPlayerSelectableAutoplayAllowed, isAutoPlayConfirmationRequired, isAutoPlayChangeSpeedAllowed, isCreditPlayoffEnabled, minBetLimit, maxBetLimit, defaultStakeSettings);
		}
	}
}