using System.Collections.Generic;
using Midas.Core.Configuration;
using Midas.Core.General;

namespace Midas.Core
{
	public enum DenomLevel
	{
		Low,
		Mid,
		High
	}

	public interface IDenomBetData
	{
		DenomLevel DenomLevel { get; }
		long MaxLines { get; }
		long MaxMultiway { get; }
		long MaxAnteBet { get; }
		Credit MaxBetAtMultiplierOne { get; }
	}

	public interface ILogicLoader
	{
		IGame LoadGame(string gameMountPoint, string paytableFileName, ConfigData config);
		IReadOnlyDictionary<Money, IDenomBetData> GetDenomBetData(string gameMountPoint, ConfigData configData);
		IGamble LoadGamble(ConfigData config);
		ICreditPlayoff LoadCreditPlayoff(ConfigData config);
	}
}