using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Logic.Core.Engine;
using Logic.Core.Utility;
using Midas.Core;
using Midas.Core.Configuration;
using Midas.Core.ExtensionMethods;
using Midas.Core.General;
using Midas.Gle.LogicToPresentation;
using Newtonsoft.Json;
using Money = Midas.Core.General.Money;

namespace Midas.Gle.Logic
{
	public static class GlePaytable
	{
		private sealed class GleDenomBetData : IDenomBetData
		{
			public DenomLevel DenomLevel { get; }
			public long MaxLines { get; }
			public long MaxMultiway { get; }
			public long MaxAnteBet { get; }
			public Credit MaxBetAtMultiplierOne { get; }

			public GleDenomBetData(DenomLevel denomLevel, IReadOnlyList<GleGame.GleStakeCombination> stakeCombinations)
			{
				DenomLevel = denomLevel;
				MaxLines = GetMaxStakeVal(Stake.LinesBet);
				MaxMultiway = GetMaxStakeVal(Stake.Multiway);
				MaxAnteBet = GetMaxStakeVal(Stake.AnteBet);
				MaxBetAtMultiplierOne = GetMaxBetAtMultiplierOne();

				long GetMaxStakeVal(Stake stake)
				{
					if (stakeCombinations.Count == 0)
						return 0;
					return stakeCombinations.Max(sc =>
					{
						sc.Values.TryGetValue(stake, out var val);
						return val;
					});
				}

				Credit GetMaxBetAtMultiplierOne()
				{
					var maxBet = Credit.Zero;
					foreach (var combo in stakeCombinations)
					{
						if ((!combo.Values.TryGetValue(Stake.BetMultiplier, out var betMult) || betMult == 1) &&
							combo.TotalBet > maxBet)
						{
							maxBet = combo.TotalBet;
						}
					}

					return maxBet;
				}
			}

			public override string ToString() => $"{DenomLevel} denom, {MaxLines} lines, {MaxMultiway} ways, {MaxAnteBet} ante, {MaxBetAtMultiplierOne} max bet";
		}

		public static IGame GetConfiguredGame(string gameMountPoint, ConfigData config, bool doLogicResetOnConfigChange)
		{
			var wcm = GetWagerCategoryMappings(gameMountPoint);
			var logicConfigs = GetConfigurationData(config.CustomConfig.PayVarConfigItems);
			logicConfigs.Add("Denom", config.DenomConfig.CurrentDenomination.ToLogicMoney().ToStringOrThrow("G"));
			var stakeCombos = GetStakeCombinations(logicConfigs, wcm);
			stakeCombos = stakeCombos.Where(sc => sc.TotalBet <= config.GameConfig.MaxBetLimit && sc.TotalBet >= config.GameConfig.MinBetLimit).ToArray();
			return new GleGame(logicConfigs, stakeCombos, doLogicResetOnConfigChange);
		}

		public static IReadOnlyDictionary<Money, IDenomBetData> GetDenomBetData(ConfigData config, Func<Money, DenomLevel> getDenomLevel, Func<IReadOnlyList<IReadOnlyDictionary<string, string>>, IReadOnlyDictionary<string, string>> selectDenomConfig = null)
		{
			selectDenomConfig ??= list => list.First();

			var result = new Dictionary<Money, IDenomBetData>();

			// For the current denom, we have all the correct config data

			var lc = GetConfigurationData(config.CustomConfig.PayVarConfigItems);
			lc["Denom"] = config.DenomConfig.CurrentDenomination.ToLogicMoney().ToStringOrThrow("G");
			var stakeCombos = GetStakeCombinations(lc, null);
			result[config.DenomConfig.CurrentDenomination] = new GleDenomBetData(getDenomLevel(config.DenomConfig.CurrentDenomination), stakeCombos);

			// For all other denoms, we don't have config info. By default we use the first one, but offer a way to override that behaviour.

			foreach (var denom in config.DenomConfig.AvailableDenominations)
			{
				if (denom.Equals(config.DenomConfig.CurrentDenomination))
					continue;

				var logicConfigs = selectDenomConfig(GetDenomConfigs(denom));
				stakeCombos = GetStakeCombinations(logicConfigs, null);
				result.Add(denom, new GleDenomBetData(getDenomLevel(denom), stakeCombos));
			}

			return result;
		}

		private static IReadOnlyList<IReadOnlyDictionary<string, string>> GetDenomConfigs(Money denom)
		{
			var denomValue = denom.ToLogicMoney().ToStringOrThrow("G");
			var configValues = new List<Dictionary<string, string>>();

			var ciDict = new Dictionary<string, int>();

			var configurationNames = GleGameData.InputSets.GetConfigurationInputNames();
			var inputNames = GleGameData.InputSets.GetInputNames();
			var denomIndex = -1;

			for (var i = 0; i < GleGameData.InputSets.GetInputCount(); i++)
			{
				var inputName = inputNames[i];
				if (configurationNames.Contains(inputName))
					ciDict.Add(inputName, i);
				if (inputName == "Denom")
					denomIndex = i;
			}

			for (var i = 0; i < GleGameData.InputSets.GetCount(); i++)
			{
				var items = GleGameData.InputSets.GetInputValues(i, denomIndex);
				var index = items.FindIndex(item => ConvertGleObject(item) == denomValue);

				if (index != -1)
				{
					var configInputs = new List<List<int>>();
					foreach (var item in ciDict)
					{
						configInputs.Add(item.Value == denomIndex
							? new List<int> { index }
							: Enumerable.Range(0, GleGameData.InputSets.GetInputValues(i, item.Value).Count).ToList());
					}

					var combos = GetAllPossibleCombos(configInputs);

					foreach (var combo in combos)
					{
						var inputSetIndex = i;

						var configs = new Dictionary<string, string>();
						for (var j = 0; j < configurationNames.Count; j++)
						{
							var configName = configurationNames[j];
							var inputValues = GleGameData.InputSets.GetInputValues(inputSetIndex, ciDict[configName]);
							configs[configName] = ConvertGleObject(inputValues[combo[j]]);
						}

						if (configValues.All(v => v.Values.Except(configs.Values).Any()))
							configValues.Add(configs);
					}
				}
			}

			return configValues;
		}

		private static Dictionary<string, string> GetConfigurationData(IReadOnlyDictionary<string, object> payVarConfigItems)
		{
			var configs = new Dictionary<string, string>();

			foreach (var name in GleGameData.InputSets.GetConfigurationInputNames())
			{
				switch (name)
				{
					case "Denom":
						break;
					case "ProgSetId":
						configs.Add(name, payVarConfigItems["ProgressiveSetId"].ToString());
						break;
					default:
						configs.Add(name, payVarConfigItems[name].ToString());
						break;
				}
			}

			return configs;
		}

		private static IReadOnlyList<GleGame.GleStakeCombination> GetStakeCombinations(IReadOnlyDictionary<string, string> configs, IReadOnlyList<WagerCategoryMapping> wagerCategoryMappings)
		{
			var liDict = new Dictionary<string, int>();
			var ciDict = new Dictionary<string, int>();

			var configurationNames = GleGameData.InputSets.GetConfigurationInputNames();
			var inputNames = GleGameData.InputSets.GetInputNames();
			for (var i = 0; i < GleGameData.InputSets.GetInputCount(); i++)
			{
				var inputName = inputNames[i];
				if (configurationNames.Contains(inputName))
					ciDict.Add(inputName, i);
				else
					liDict.Add(inputNames[i], i);
			}

			var stakeCombinations = new List<GleGame.GleStakeCombination>();
			var configIndexes = new int[configurationNames.Count];

			for (var i = 0; i < GleGameData.InputSets.GetCount(); i++)
			{
				var isValid = true;

				var logicInputs = new List<List<int>>();

				for (var j = 0; j < configurationNames.Count; j++)
				{
					var items = GleGameData.InputSets.GetInputValues(i, ciDict[configurationNames[j]]);
					var configValue = configs[configurationNames[j]];
					var index = items.FindIndex(item => ConvertGleObject(item) == configValue);

					if (index != -1)
						configIndexes[j] = index;
					else
					{
						isValid = false;
						break;
					}
				}

				if (isValid)
				{
					foreach (var item in liDict)
						logicInputs.Add(Enumerable.Range(0, GleGameData.InputSets.GetInputValues(i, item.Value).Count).ToList());

					var combos = GetAllPossibleCombos(logicInputs);

					foreach (var combo in combos)
					{
						var allInputs = GleGameData.InputSets.GetInputs(i, configIndexes.Concat(combo).ToArray());
						var betCategory = GetBetCategory(wagerCategoryMappings, allInputs);
						stakeCombinations.Add(new GleGame.GleStakeCombination(allInputs, betCategory));
					}
				}
			}

			stakeCombinations = stakeCombinations.OrderBy(c => c.TotalBet.Value).ToList();
			return stakeCombinations;
		}

		private static string ConvertGleObject(object item)
		{
			switch (StringConverter.ToString(item, "G"))
			{
				case StringSuccess s: return s.Value;
				case Error e: throw new Exception($"Error: {e.Description}");
				case NotSupported _: throw new Exception($"Not Supported: {item.GetType().ToDisplayString()}");
				default: throw new NotSupportedException();
			}
		}

		private static List<List<int>> GetAllPossibleCombos(List<List<int>> objects)
		{
			IEnumerable<List<int>> combos = new List<List<int>> { new List<int>() };

			foreach (var inner in objects)
			{
				combos = combos.SelectMany(r => inner
					.Select(x =>
					{
						var n = r.ToList();
						n.Add(x);
						return n;
					}).ToList());
			}

			// Remove combinations where all items are empty.

			return combos.Where(c => c.Count > 0).ToList();
		}

		private static List<WagerCategoryMapping> GetWagerCategoryMappings(string gameMountPoint)
		{
			var filename = Path.Combine(gameMountPoint, @"Registries\WagerCategories.json");
			if (!File.Exists(filename))
				return null;

			var jf = File.ReadAllText(filename);
			return JsonConvert.DeserializeObject<List<WagerCategoryMapping>>(jf);
		}

		private static BetCategory GetBetCategory(IReadOnlyList<WagerCategoryMapping> wagerCategoryMappings, Inputs allInputs)
		{
			if (wagerCategoryMappings == null)
				return BetCategory.BetCategory0;

			var config = wagerCategoryMappings.FirstOrDefault(m => m.GetConfigsAsInput().Any(c => c.All(jj => ConvertGleObject(allInputs.First(f => f.Name == jj.Name).Value) == ConvertGleObject(jj.Value))));
			var category = config?.Categories.FirstOrDefault(m => m.GetAsInput().Any(c => c.All(jj => ConvertGleObject(allInputs.First(f => f.Name == jj.Name).Value) == ConvertGleObject(jj.Value))));

			if (category == null)
				Log.Instance.Warn($"No bet category defined for {allInputs.ToStringOrThrow("ML")}");

			return category?.GetBetCategory() ?? BetCategory.BetCategory0;
		}

		#region Wager Category Classes

		// ReSharper disable ClassNeverInstantiated.Local
		// ReSharper disable CollectionNeverUpdated.Local
		// ReSharper disable MemberCanBePrivate.Local
		// ReSharper disable UnusedAutoPropertyAccessor.Local
		// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
		// ReSharper disable UnusedMember.Local
		private sealed class Category
		{
			public string BetCategory { get; set; }
			public string Rtp { get; set; }
			public List<string> Inputs { get; set; } = new List<string>();

			public IReadOnlyList<IReadOnlyList<Input>> GetAsInput()
			{
				return Inputs.Select(i => (IReadOnlyList<Input>)i.Split(new[] { "," }, StringSplitOptions.None).Select(j => j.FromStringOrThrow<Input>(string.Empty)).ToList()).ToList();
			}

			public BetCategory GetBetCategory()
			{
				Enum.TryParse<BetCategory>(BetCategory, true, out var bc);
				return bc;
			}
		}

		private sealed class WagerCategoryMapping
		{
			public List<string> Configs { get; set; } = new List<string>();
			public List<Category> Categories { get; set; } = new List<Category>();

			public IReadOnlyList<IReadOnlyList<Input>> GetConfigsAsInput()
			{
				return Configs.Select(i => (IReadOnlyList<Input>)i.Split(new[] { "," }, StringSplitOptions.None).Select(j => j.FromStringOrThrow<Input>(string.Empty)).ToList()).ToList();
			}
		}

		// ReSharper restore MemberCanBePrivate.Local
		// ReSharper restore ClassNeverInstantiated.Local
		// ReSharper restore CollectionNeverUpdated.Local
		// ReSharper restore UnusedAutoPropertyAccessor.Local
		// ReSharper restore AutoPropertyCanBeMadeGetOnly.Local
		// ReSharper restore UnusedMember.Local

		#endregion
	}
}