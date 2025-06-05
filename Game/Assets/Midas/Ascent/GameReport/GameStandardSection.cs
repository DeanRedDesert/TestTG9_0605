using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Game;
using IGT.Game.Core.GameReport;
using IGT.Game.Core.GameReport.Interfaces;
using Logic.Core.Engine;
using Logic.Core.Utility;
using Midas.Ascent.Ugp;
using Midas.Core.ExtensionMethods;
using Credits = Logic.Core.Types.Credits;

namespace Midas.Ascent.GameReport
{
	/// <summary>
	/// Standard Report Section for the Game.
	/// </summary>
	public sealed class GameStandardSection : IStandardReportSection
	{
		#region Fields

		private readonly CustomReportSection customSection;
		private readonly ReportContext context;
		private readonly XmlElement gameConfiguration;
		private readonly GameProgressiveReportSection progressives;
		private readonly decimal sapProgressiveRtp;
		private readonly (uint MinLines, uint MaxLines, uint MinWays, uint MaxWays) betInformation;

		#endregion

		/// <summary>
		/// Instantiates a new <see cref="GameStandardSection"/>
		/// with a report lib and context.
		/// </summary>
		/// <param name="context">Context for the report.</param>
		/// <param name="gameConfiguration"></param>
		/// <param name="progressives"></param>
		public GameStandardSection(ReportContext context, XmlElement gameConfiguration, GameProgressiveReportSection progressives)
		{
			this.context = context;
			this.gameConfiguration = gameConfiguration;
			this.progressives = progressives;
			customSection = new CustomReportSection(new List<CustomReportItem>());

			sapProgressiveRtp = Round(progressives.ProgressiveLevels.Where(pl => pl.GetLinkStatus() == GameProgressiveLevel.StandaloneLinkStatus).Sum(pl => pl.GetStartPercent() + pl.GetContributionPercentage()), 4);
			betInformation = GetStakeCombinations(gameConfiguration);

			ReportCustomThemeConfigData();
		}

		private static (uint MinLines, uint MaxLines, uint MinWays, uint MaxWays) GetStakeCombinations(XmlElement gameConfiguration)
		{
			var inputSets = new InputSets();
			var liDict = new Dictionary<string, int>();
			var ciDict = new Dictionary<string, int>();

			var configurationNames = inputSets.GetConfigurationInputNames();
			var configs = new Dictionary<string, string>();
			foreach (var name in configurationNames)
			{
				var n = name switch
				{
					"Denom" => "CreditDenomination",
					"ProgSetId" => "ProgressiveSetId",
					_ => name
				};

				var v = AusRegReader.GetConfigurationValue(gameConfiguration, n);
				v = name switch
				{
					"Denom" => v.Replace("u", string.Empty),
					_ => v
				};
				configs.Add(name, v);
			}

			var inputNames = inputSets.GetInputNames();
			for (var i = 0; i < inputSets.GetInputCount(); i++)
			{
				var inputName = inputNames[i];
				if (configurationNames.Contains(inputName))
					ciDict.Add(inputName, i);
				else
					liDict.Add(inputName, i);
			}

			var stakeCombinations = new List<Inputs>();
			var configIndexes = new int[configurationNames.Count];

			for (var i = 0; i < inputSets.GetCount(); i++)
			{
				var isValid = true;

				var logicInputs = new List<List<int>>();

				for (var j = 0; j < configurationNames.Count; j++)
				{
					var items = inputSets.GetInputValues(i, ciDict[configurationNames[j]]);
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
						logicInputs.Add(Enumerable.Range(0, inputSets.GetInputValues(i, item.Value).Count).ToList());

					var combos = GetAllPossibleCombos(logicInputs);

					foreach (var combo in combos)
						stakeCombinations.Add(inputSets.GetInputs(i, configIndexes.Concat(combo).ToArray()));
				}
			}

			var minLinesBet = uint.MaxValue;
			var maxLinesBet = 0U;
			var minWaysBet = uint.MaxValue;
			var maxWaysBet = 0U;
			foreach (var p in stakeCombinations)
			{
				if (p.TryGetInput("LinesBet", out var linesBet))
				{
					minLinesBet = Math.Min(minLinesBet, (uint)((Credits)linesBet).ToUInt64());
					maxLinesBet = Math.Max(maxLinesBet, (uint)((Credits)linesBet).ToUInt64());
				}

				if (p.TryGetInput("Multiway", out var waysBets))
				{
					minWaysBet = Math.Min(minWaysBet, (uint)((Credits)waysBets).ToUInt64());
					maxWaysBet = Math.Max(maxWaysBet, (uint)((Credits)waysBets).ToUInt64());
				}
			}

			if (minLinesBet == uint.MaxValue)
				minLinesBet = 0;

			if (minWaysBet == uint.MaxValue)
				minWaysBet = 0;

			return (minLinesBet, maxLinesBet, minWaysBet, maxWaysBet);
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

		/// <summary>
		/// Create unique custom data that will be displayed in the Game report
		/// This data will be used to overide the foundation defaults for items that the foundation isn't reporting correctly and for displaying unique items.
		/// </summary>
		private void ReportCustomThemeConfigData()
		{
			var hasSap = progressives.ProgressiveLevels.Any(pl => pl.GetLinkStatus() == GameProgressiveLevel.StandaloneLinkStatus);
			var hasLink = progressives.ProgressiveLevels.Any(pl => pl.GetLinkStatus() == GameProgressiveLevel.SasLinkedLinkStatus);
			var jackpotRtpMessage = $"{(hasLink || hasSap ? " (" : string.Empty)}{(hasLink ? "Excludes Link Progressive" : string.Empty)}{(hasLink && hasSap ? ", " : string.Empty)}{(hasSap ? "Includes SAP" : string.Empty)}{(hasLink || hasSap ? ")" : string.Empty)}";

			customSection.CustomReportItems.Add(new CustomReportItem("RTP Information", hasLink || hasSap ? jackpotRtpMessage : string.Empty));

			var minimumBaseGameRtp = Round(Round(decimal.Parse(AusRegReader.GetConfigurationValue(gameConfiguration, "MinimumBaseGameRtp")), 4) + sapProgressiveRtp, 4);
			var maximumBaseGameRtp = Round(Round(decimal.Parse(AusRegReader.GetConfigurationValue(gameConfiguration, "MaximumBaseGameRtp")), 4) + sapProgressiveRtp, 4);

			var holdMinimumBaseGameRtp = 100 - minimumBaseGameRtp;
			var holdMaximumBaseGameRtp = 100 - maximumBaseGameRtp;

			customSection.CustomReportItems.Add(new CustomReportItem("    Minimum RTP%", GameDataInspectionServiceHandlerBase.LocalizePercent(minimumBaseGameRtp, 4, context)));
			customSection.CustomReportItems.Add(new CustomReportItem("    Hold%", GameDataInspectionServiceHandlerBase.LocalizePercent(holdMinimumBaseGameRtp, 4, context)));
			customSection.CustomReportItems.Add(new CustomReportItem("    Maximum RTP%", GameDataInspectionServiceHandlerBase.LocalizePercent(maximumBaseGameRtp, 4, context)));
			customSection.CustomReportItems.Add(new CustomReportItem("    Hold%", GameDataInspectionServiceHandlerBase.LocalizePercent(holdMaximumBaseGameRtp, 4, context)));
		}

		/// <inheritdoc />
		public uint GetMinLines()
		{
			return betInformation.MinLines;
		}

		/// <inheritdoc />
		public uint GetMaxLines()
		{
			return betInformation.MaxLines;
		}

		/// <inheritdoc />
		public uint GetMinWays()
		{
			return betInformation.MinWays;
		}

		/// <inheritdoc />
		public uint GetMaxWays()
		{
			return betInformation.MaxWays;
		}

		/// <inheritdoc />
		public string GetGameDescription()
		{
			return "Not Set";
		}

		/// <inheritdoc />
		public string GetLinkSeriesModel()
		{
			return "Not Set";
		}

		/// <inheritdoc />
		public ICustomReportSection CustomReportSection
		{
			get { return customSection; }
		}

		/// <inheritdoc />
		public decimal GetTotalRtpPercent()
		{
			return GetBaseRtpPercent() + sapProgressiveRtp;
		}

		/// <inheritdoc />
		public long GetMinBetCredits()
		{
			return long.Parse(AusRegReader.GetConfigurationValue(gameConfiguration, "MinimumBet").Replace("cr", string.Empty));
		}

		/// <inheritdoc />
		public long GetMaxWinCredits()
		{
			var v = AusRegReader.GetConfigurationValueOrDefault(gameConfiguration, "MacauMaximumWin")?.Replace("cr", string.Empty);
			return string.IsNullOrEmpty(v) ? 0 : long.Parse(v);
		}

		/// <inheritdoc />
		public decimal GetBaseRtpPercent()
		{
			return decimal.Parse(AusRegReader.GetConfigurationValue(gameConfiguration, "MinimumBaseGameRtp"));
		}

		private static decimal Round(decimal value, int decimalPrecision)
		{
			return Math.Round(value, decimalPrecision, MidpointRounding.AwayFromZero);
		}
	}
}