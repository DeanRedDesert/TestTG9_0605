using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using IGT.Game.Core.Communication.Foundation.F2L.Schemas.Internal;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Progressive;
using Midas.Core.General;

namespace Midas.Ascent.Ugp
{
	public static class AusRegReader
	{
		#region Private Statics

		private const string AttributeIdentifier = "Identifier";
		private const string AttributeName = "Name";
		private const string AttributeType = "Type";
		private const string AttributeIncrement = "Increment";
		private const string AttributeHiddenIncrement = "HiddenIncrement";
		private const string AttributeStartUpAsMoney = "StartUpAsMoney";
		private const string AttributeCeilingAsMoney = "CeilingAsMoney";
		private const string AttributeStartUpAsCredits = "StartUpAsCredits";
		private const string AttributeCeilingAsCredits = "CeilingAsCredits";
		private const string AttributeProgressiveSetId = "ProgressiveSetId";

		#endregion

		#region Public Methods

		/// <summary>
		/// Get all the progressive levels for the current configuration.
		/// </summary>
		/// <param name="gameMountPoint">The mount point for the game.</param>
		/// <param name="progressiveSetId">The current progressive set id.</param>
		/// <param name="percentage">The current percentage.</param>
		/// <param name="maximumBet">The current maximum bet.</param>
		/// <param name="denom">The current credit denomination.</param>
		/// <returns>A collection of progressive levels.</returns>
		public static IList<ProgressiveLevel> GetAllProgressiveLevels(string gameMountPoint, string progressiveSetId, string percentage, Credit maximumBet, Money denom)
		{
			var ausRegFile = Load(gameMountPoint);
			if (ausRegFile?.DocumentElement == null)
				return new List<ProgressiveLevel>();

			var mb = maximumBet.Credits + "cr";
			var cd = denom.AsMinorCurrency + "u";

			var progressiveLevelsXml = GetProgressiveLevels(ausRegFile, progressiveSetId);
			var gameConfiguration = GetGameConfiguration(ausRegFile, progressiveSetId, percentage, mb, cd);
			return progressiveLevelsXml.Select(pl => CreateProgressiveLevel(gameConfiguration, pl)).ToList();
		}

		public static XmlDocument Load(string gameMountPoint)
		{
			var filename = Path.Combine(gameMountPoint, "Registries/game.xausreg");
			if (!File.Exists(filename))
				return null;

			var templateFile = new XmlDocument();

			using var streamReader = new StreamReader(Path.Combine(gameMountPoint, "Registries/game.xausreg"));
			templateFile.Load(streamReader);
			streamReader.Close();

			return templateFile;
		}

		public static XmlElement GetGameConfiguration(XmlDocument ausRegFile, string progressiveSetId, string percentage, string maximumBet, string creditDenomination)
		{
			var gameConfigurations = ausRegFile.DocumentElement!.ChildNodes.OfType<XmlElement>().Where(e => e.LocalName == "GameConfig").ToList();

			return gameConfigurations.FirstOrDefault(gc =>
				ContainsConfigItem(gc, "Percentage", percentage) &&
				ContainsConfigItem(gc, "ProgressiveSetId", progressiveSetId) &&
				ContainsConfigItem(gc, "MaximumBet", maximumBet) &&
				ContainsConfigItem(gc, "CreditDenomination", creditDenomination));
		}

		/// <summary>
		/// Get the value for the configuration item.
		/// </summary>
		/// <param name="gameConfiguration">The game configuration data.</param>
		/// <param name="name">The configuration item to find.</param>
		/// <returns>The value for the configuration item.</returns>
		public static string GetConfigurationValue(XmlElement gameConfiguration, string name)
		{
			return gameConfiguration.ChildNodes.OfType<XmlElement>().Single(ci => ci.GetAttribute("Name").Equals(name)).GetAttribute("Value");
		}

		/// <summary>
		/// Get the value for the configuration item or return a default value if not found.
		/// </summary>
		/// <param name="gameConfiguration">The game configuration data.</param>
		/// <param name="name">The configuration item to find.</param>
		/// <returns>The value for the configuration item.</returns>
		public static string GetConfigurationValueOrDefault(XmlElement gameConfiguration, string name)
		{
			return gameConfiguration.ChildNodes.OfType<XmlElement>().SingleOrDefault(ci => ci.GetAttribute("Name").Equals(name))?.GetAttribute("Value");
		}

		#endregion

		#region Private Methods

		private static bool ContainsConfigItem(XmlNode gc, string item, string value) => string.IsNullOrEmpty(value) || gc.ChildNodes.OfType<XmlElement>().Any(ci => ci.GetAttribute("Name").Equals(item) && ci.GetAttribute("Value").Equals(value));

		private static IList<XmlElement> GetProgressiveLevels(XmlDocument ausRegFile, string progressiveSetId)
		{
			var progressiveConfigurations = ausRegFile.DocumentElement!.ChildNodes.OfType<XmlElement>().Where(e => e.LocalName == "ProgressiveConfiguration").ToList();

			if (progressiveConfigurations.Count == 0)
				return Array.Empty<XmlElement>();

			foreach (var progressiveSet in progressiveConfigurations)
			{
				if (progressiveSet.HasAttribute(AttributeProgressiveSetId) && progressiveSet.GetAttribute(AttributeProgressiveSetId).Equals(progressiveSetId))
					return progressiveSet.ChildNodes.OfType<XmlElement>().ToList();
			}

			return progressiveConfigurations.First().ChildNodes.OfType<XmlElement>().ToList();
		}

		private static ProgressiveLevel CreateProgressiveLevel(XmlNode element, XmlElement progressiveElement)
		{
			var startupValue = Money.FromMinorCurrency(1000);
			var ceilingValue = Money.FromMinorCurrency(10000);

			if (progressiveElement.HasAttribute(AttributeStartUpAsMoney))
				startupValue = Parse(progressiveElement.GetAttribute(AttributeStartUpAsMoney));

			if (progressiveElement.HasAttribute(AttributeStartUpAsCredits))
				startupValue = ParseFromCredits(progressiveElement.GetAttribute(AttributeStartUpAsCredits));

			if (progressiveElement.HasAttribute(AttributeCeilingAsMoney))
				ceilingValue = Parse(progressiveElement.GetAttribute(AttributeCeilingAsMoney));

			if (progressiveElement.HasAttribute(AttributeCeilingAsCredits))
				ceilingValue = ParseFromCredits(progressiveElement.GetAttribute(AttributeCeilingAsCredits));

			var trigProb = 0.0000005;
			var rtp = 1.5;
			var id = progressiveElement.HasAttribute(AttributeIdentifier) ? progressiveElement.GetAttribute(AttributeIdentifier) : null;

			if (!string.IsNullOrEmpty(id))
			{
				var gameConfigs = element.ChildNodes.OfType<XmlElement>().ToList();
				var trigProbConfigItem = gameConfigs.FirstOrDefault(ci => ci.GetAttribute("Name").Equals(id + "TriggerProbability"));
				if (trigProbConfigItem != null)
					trigProb = double.Parse(trigProbConfigItem.GetAttribute("Value"));

				var rtpConfigItem = gameConfigs.FirstOrDefault(ci => ci.GetAttribute("Name").Equals(id + "Rtp"));
				if (rtpConfigItem != null)
					rtp = double.Parse(rtpConfigItem.GetAttribute("Value"));
			}

			return new ProgressiveLevel
			{
				Name = progressiveElement.HasAttribute(AttributeName) ? progressiveElement.GetAttribute(AttributeName) : "Null",
				TriggerProbability = trigProb,
				Rtp = rtp,
				IsTriggered = !progressiveElement.HasAttribute(AttributeType) || progressiveElement.GetAttribute(AttributeType).Equals("Triggered"),
				Id = progressiveElement.HasAttribute(AttributeIdentifier) ? progressiveElement.GetAttribute(AttributeIdentifier) : "Null",
				IncrementPercentage = progressiveElement.HasAttribute(AttributeIncrement) ? double.Parse(progressiveElement.GetAttribute(AttributeIncrement)) : .01,
				HiddenIncrementPercentage = progressiveElement.HasAttribute(AttributeHiddenIncrement) ? double.Parse(progressiveElement.GetAttribute(AttributeHiddenIncrement)) : .00001,
				IsStandalone = true,
				Startup = new AmountType(startupValue.AsMinorCurrency),
				Ceiling = new AmountType(ceilingValue.AsMinorCurrency)
			};
		}

		private static Money Parse(string value)
		{
			value = value.Replace("u", "");
			return Money.FromMinorCurrency(long.Parse(value));
		}

		private static Money ParseFromCredits(string value)
		{
			value = value.Replace("cr", "");
			return Money.FromCredit(Credit.FromLong(long.Parse(value)));
		}

		#endregion
	}
}