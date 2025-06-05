using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using IGT.Game.Core.Communication.Standalone.Schemas;
using Midas.Tools.Editor;

namespace Midas.Ascent.Editor
{
	public static class GenerateSystemConfig
	{
		#region Public Methods

		public static void CreateSystemConfigFiles(List<(XDocument XDocument, string MaxBet, string Denom)> payvars)
		{
			var sc = SystemConfigurationHelper.Load();

			sc.PaytableList.Clear();
			sc.SystemControlledProgressives = null;

			foreach (var payvar in payvars)
			{
				var root = payvar.XDocument.Root;
				var themeRegistry = root.DescendantValue("ThemeRegistry");
				var paytableName = root.DescendantValue("PaytableName");
				var paytableId = $"{themeRegistry}/{paytableName}";
				var themeId = XDocument.Load(themeRegistry).Root.DescendantValue("G2SThemeId") ?? throw new NullReferenceException();

				sc.PaytableList.Add(CreatePaytableConfiguration(paytableName, themeId, paytableId, sc, payvar.Denom, payvar.MaxBet, root));

				var winLevels = root!.Descendants(Ns.Pr3 + "WinLevel").Where(e => e.Descendants(Ns.Pr3 + "ProgressiveGameLevelIndex").Any()).ToList();
				var startupLevels = root.Descendants(Ns.Pr3 + "ProgressiveGameLevel").Select(pgl => long.Parse(pgl.Descendant("StartCredit").Value) * long.Parse(payvar.Denom)).ToList();

				if (winLevels.Any())
				{
					sc.SystemControlledProgressives ??= new SystemControlledProgressives();

					var controllerName = GetControllerName(root);
					sc.SystemControlledProgressives.ProgressiveSetups.Add(CreateSetup(paytableName, paytableId, themeId, payvar.Denom, winLevels, controllerName));

					if (sc.SystemControlledProgressives.ProgressiveControllers.All(c => c.Name != controllerName))
						sc.SystemControlledProgressives.ProgressiveControllers.Add(CreateController(root, controllerName, winLevels, startupLevels));
				}
			}

			SystemConfigurationHelper.Save(sc);
		}

		#endregion

		#region SystemConfigurations Creation Methods

		private static PaytableListPaytableConfiguration CreatePaytableConfiguration(string paytableName, string themeId, string paytableId, SystemConfigurations sc, string denom, string maxBet, XElement payvar)
		{
			return new PaytableListPaytableConfiguration
			{
				PaytableName = paytableName,
				ThemeIdentifier = themeId,
				PaytableIdentifier = paytableId,
				PaytableFileName = "Paytables/Game.zaml",
				IsDefault = sc.PaytableList.Count == 0,
				Denomination = uint.Parse(denom),
				MaxBet = ulong.Parse(payvar.Descendant("MaxBet").Descendant(Ns.Rt2 + "Value", d => d.AttributeValue("Denom") == denom && d.Value == maxBet).Value),
				MaxBetSpecified = true,
				ButtonPanelMinBet = ulong.Parse(payvar.Descendant("ButtonPanelMinBet").Descendant(Ns.Rt2 + "Value", d => d.AttributeValue("Denom") == denom).Value),
				ButtonPanelMinBetSpecified = true,
			};
		}

		private static SystemControlledProgressivesProgressiveSetup CreateSetup(string paytableName, string paytableId, string themeId, string denomination, List<XElement> progressiveWinLevels, string controllerName)
		{
			return new SystemControlledProgressivesProgressiveSetup
			{
				PaytableConfiguration = new PaytableBinding
				{
					PaytableName = paytableName,
					PaytableIdentifier = paytableId,
					ThemeIdentifier = themeId,
					PaytableFileName = "Paytables/Game.zaml",
					Denomination = uint.Parse(denomination)
				},
				ProgressiveLink = progressiveWinLevels.Select((wl, i) => new SystemControlledProgressivesProgressiveSetupProgressiveLink
				{
					ControllerLevel = i,
					ControllerName = controllerName,
					GameLevel = int.Parse(wl.DescendantValue("ProgressiveGameLevelIndex"))
				}).ToList()
			};
		}

		private static SystemControlledProgressivesProgressiveController CreateController(XContainer payvar, string pcName, List<XElement> progressiveWinLevels, IReadOnlyList<long> startupLevels)
		{
			var progressiveConceptCode = payvar.Descendants(Ns.Rt1 + "ProgressiveConceptCode").First().Value;
			var linkRegXml = LoadProgressiveRegistry($"*{progressiveConceptCode}.xlinkreg") ?? throw new Exception("Require an xlinkreg file");
			var spcRegXml = LoadProgressiveRegistry($"*{progressiveConceptCode}.xspcreg");
			var npcRegXml = LoadProgressiveRegistry($"*{progressiveConceptCode}.xnpcreg");

			var scppc = new SystemControlledProgressivesProgressiveController();
			scppc.Name = pcName;

			var linkRegLinkMap = linkRegXml.Descendant("LinkMap", e => e.DescendantValue(Ns.Rt1 + "ProgressiveConceptCode") == progressiveConceptCode);

			foreach (var wl in progressiveWinLevels)
			{
				var gameLevelIndex = wl.DescendantValue("ProgressiveGameLevelIndex");
				var linkRegLink = linkRegLinkMap.Descendant("ProgressiveLink", l => l.ElementValue("GameLevelEndpoint") == gameLevelIndex);
				var isLegacySas = linkRegLink.Descendants().Any(d => d.Name.LocalName == "LegacyLevelSpecification");

				if (!isLegacySas)
				{
					var controllerSpecName = linkRegLink.DescendantValue("ControllerSpecName");
					var controllerSpecIndex = int.Parse(linkRegLink.DescendantValue("SpecLevel"));
					var controllerSpec = linkRegLinkMap.Descendant("ControllerSpecifications").Elements().Single(e => e.DescendantValue("ControllerSpecName") == controllerSpecName);

					switch (controllerSpec.Name.LocalName)
					{
						case "SpcControllerSpecification":
							scppc.ControllerLevel.Add(CreateStandaloneLevel(spcRegXml, controllerSpec, controllerSpecIndex, gameLevelIndex));
							break;
						case "NetProgressiveControllerSpecification":
							scppc.ControllerLevel.Add(CreateLinkedLevel(npcRegXml, controllerSpec.DescendantValue(Ns.Rt1 + "GroupCode"), controllerSpecIndex, gameLevelIndex));
							break;
						default: throw new Exception($"Unsupported controller specification type: {controllerSpec.Name.LocalName}");
					}
				}
				else
				{
					var sasProgressive = new SystemControlledProgressivesProgressiveControllerControllerLevel
					{
						Id = ushort.Parse(gameLevelIndex),
						StartingAmount = Math.Min(startupLevels[int.Parse(gameLevelIndex)], 100000000000),
						ContributionPercentage = .005f,
						MaximumAmount = 100000000000,
						PrizeString = string.Empty
					};

					scppc.ControllerLevel.Add(sasProgressive);
				}
			}

			return scppc;
		}

		private static SystemControlledProgressivesProgressiveControllerControllerLevel CreateLinkedLevel(XElement npcRegXml, string groupCode, int controllerSpecIndex, string gameLevelIndex)
		{
			if (npcRegXml == null)
				throw new Exception("Require an xnpcreg file");

			// Need to get the group from the linkReg so we can lookup the details in the npcReg.
			var npcRegGroup = npcRegXml.Descendant("Group", e => e.DescendantValue(Ns.Rt1 + "GroupCode") == groupCode);
			var npcRegLevel = npcRegGroup.Descendant("ControllerLevel", controllerSpecIndex);

			return new SystemControlledProgressivesProgressiveControllerControllerLevel
			{
				Id = ushort.Parse(gameLevelIndex),
				StartingAmount = long.Parse(npcRegLevel.Descendant("StartAmount").ElementValue("Value")),
				ContributionPercentage = float.Parse(npcRegLevel.Descendant("BetContributionPercentage").ElementValue("Value")) / 100.0f,
				MaximumAmount = long.Parse(npcRegLevel.Descendant("MaxLimit").ElementValue("Value")),
				PrizeString = string.Empty
			};
		}

		private static SystemControlledProgressivesProgressiveControllerControllerLevel CreateStandaloneLevel(XElement spcRegXml, XElement controllerSpec, int controllerSpecIndex, string gameLevelIndex)
		{
			if (spcRegXml == null)
				throw new Exception("Require an xspcreg file");

			var levelName = controllerSpec.Descendant("SpcControllerSpecLevel", controllerSpecIndex).ElementValue("LevelName");
			var spcRegLevel = spcRegXml.Descendant("ControllerLevel", l => l.DescendantValue("Description") == levelName);

			return new SystemControlledProgressivesProgressiveControllerControllerLevel
			{
				Id = ushort.Parse(gameLevelIndex),
				StartingAmount = long.Parse(spcRegLevel.DescendantValue("StartAmount")),
				ContributionPercentage = float.Parse(spcRegLevel.DescendantValue("BetContributionPercentage")) / 100.0f,
				MaximumAmount = long.Parse(spcRegLevel.DescendantValue("MaxLimitAmount")),
				PrizeString = string.Empty
			};
		}

		#endregion

		#region Private Methods

		private static XElement LoadProgressiveRegistry(string filePattern)
		{
			var f = Directory.EnumerateFiles("Registries", filePattern, SearchOption.TopDirectoryOnly).FirstOrDefault();
			return f == null ? null : XDocument.Load(f).Root?.Element(Ns.R1 + "Body")?.Elements().FirstOrDefault();
		}

		/// <summary>
		/// There are some limitations on the progressive controller name. This method avoids the known issues.
		/// </summary>
		private static string GetControllerName(XElement payvar)
		{
			var progressiveSetConfig = payvar.Descendants(Ns.Rt2 + "ConfigItem").SingleOrDefault(e => e.ElementValue("Name") == "ProgressiveSetId");
			var progressiveSetId = progressiveSetConfig?.DescendantValue("StringData") ?? throw new NotSupportedException($"ProgressiveSetId not specified in payvar {payvar.ElementValue("PaytableName")}");

			progressiveSetId = progressiveSetId.Replace(" ", "_");
			progressiveSetId = progressiveSetId.Replace("$", "S");

			if (!Regex.IsMatch(progressiveSetId, "^[a-zA-Z0-9][a-zA-Z0-9._-]*$"))
				throw new NotSupportedException("ProgressiveSetId contains invalid characters: " + progressiveSetId);

			return $"Controller_{progressiveSetId}";
		}

		#endregion
	}
}