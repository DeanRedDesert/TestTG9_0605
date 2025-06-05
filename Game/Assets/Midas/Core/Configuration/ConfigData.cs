namespace Midas.Core.Configuration
{
	public sealed partial class ConfigData
	{
		public MachineConfig MachineConfig { get; }
		public CurrencyConfig CurrencyConfig { get; }
		public DenomConfig DenomConfig { get; }
		public AncillaryConfig AncillaryConfig { get; }
		public LanguageConfig LanguageConfig { get; }
		public GameConfig GameConfig { get; }
		public CustomConfig CustomConfig { get; }

		public ConfigData(MachineConfig machineConfig, CurrencyConfig currencyConfig,
			DenomConfig denomConfig, AncillaryConfig ancillaryConfig,
			LanguageConfig languageConfig, GameConfig gameConfig,
			CustomConfig customConfig)
		{
			MachineConfig = machineConfig;
			DenomConfig = denomConfig;
			CurrencyConfig = currencyConfig;
			AncillaryConfig = ancillaryConfig;
			LanguageConfig = languageConfig;
			GameConfig = gameConfig;
			CustomConfig = customConfig;
		}

		public override string ToString() => $"{MachineConfig.Jurisdiction} {DenomConfig.CurrentDenomination}";
	}
}