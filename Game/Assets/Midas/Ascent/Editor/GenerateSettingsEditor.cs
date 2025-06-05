using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using IGT.Ascent.Assets.StandaloneSafeStorage.Editor;
using Midas.Core.ExtensionMethods;
using Midas.Tools.Editor;
using UnityEditor;
using UnityEngine;

namespace Midas.Ascent.Editor
{
	public sealed class GenerateSettingsEditor : EditorWindow
	{
		#region Classes

		private sealed class PayvarData
		{
			public string Name { get; }
			public XDocument XDocument { get; }
			public Dictionary<string, IReadOnlyList<string>> Options { get; }

			public PayvarData(string name, XDocument xDocument, Dictionary<string, IReadOnlyList<string>> options)
			{
				Name = name;
				XDocument = xDocument;
				Options = options;
			}
		}

		#endregion

		#region Fields

		// UI state fields
		private bool isMultigame;
		private Vector2 scrollView;

		// Data to render and retrieve payvars
		private Dictionary<string, List<string>> singlegameConfigs = new Dictionary<string, List<string>>();
		private readonly List<(string GroupId, string VirtualGameName, List<(string ConfigName, string ConfigValue)> ConfigItems)> multigameGroups = new List<(string, string, List<(string, string)>)>();
		private readonly List<PayvarData> payvarDataList = new List<PayvarData>();
		private GenerateSettings generateSettings;

		#endregion

		#region Public Methods

		public void Init()
		{
			generateSettings = GenerateSettings.Load();
			scrollView = Vector2.zero;
			multigameGroups.Clear();
			payvarDataList.Clear();
			isMultigame = File.Exists(@"Registries\MultigamePackageData.xml");

			InitPayvarData();
			InitMultigame();
			InitSinglegame();
		}

		#endregion

		#region EditorWindow Methods

		private void OnGUI()
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();
			EditorGUILayout.Space();

			var lw = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 300;

			isMultigame = EditorGUILayout.Toggle("Use MultigamePackageData.xml", isMultigame);

			EditorGUIUtility.labelWidth = lw;
			EditorGUILayout.Space();

			if (isMultigame)
				RenderMultigame();
			else
				RenderSinglegame();

			EditorGUILayout.Space();

			// Show which payvars have been selected.
			RenderLine(Color.gray);

			var payvars = GetSelectedPayvars();
			RenderPayvars(payvars);

			var error = CheckForErrors(payvars);
			EditorGUILayout.LabelField(error ?? "", EditorStyles.boldLabel);

			// Show the Continue and Cancel buttons.
			EditorGUILayout.BeginHorizontal();

			GUI.enabled = error == null;

			if (GUILayout.Button("Continue"))
			{
				// Gather selected max bet for each payvar
				GenerateSettings.Save(generateSettings);
				GenerateSystemConfig.CreateSystemConfigFiles(payvars.Select(p => (p.Payvar.XDocument, p.MaxBet, p.Denom)).ToList());
				PlayerSettings.productName = Path.GetFileNameWithoutExtension(Directory.EnumerateFiles("Registries", "*.xsifinformation", SearchOption.AllDirectories).FirstOrDefault()) ?? "Game020-001TMP";
				PlayerSettings.companyName = "IGT";
				SafeStorageMenu.Clear();
				Debug.Log("Generation of SystemConfig.xml is complete.");
				Close();
			}

			GUI.enabled = true;

			if (GUILayout.Button("Cancel"))
				Close();

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Load required data from each payvar file.
		/// </summary>
		private void InitPayvarData()
		{
			foreach (var payvar in Directory.EnumerateFiles("Registries", "*.xpayvarreg", SearchOption.AllDirectories))
			{
				var options = new Dictionary<string, IReadOnlyList<string>>();
				var xml = XDocument.Load(payvar);
				var payvarName = Path.GetFileNameWithoutExtension(payvar);

				foreach (var config in xml.Descendants(Ns.Rt2 + "ConfigItem"))
				{
					var key = config.Elements(Ns.Rt2 + "Name").Single().Value;

					if (key != "MinimumBaseGameRtp" && key != "MaximumBaseGameRtp")
						options[key] = new List<string> { config.Descendants(Ns.Rt2 + "StringData").Single().Value };
				}

				options["CreditDenomination"] = xml.Descendants(Ns.Pr3 + "Denom").Select(d => d.Value).ToList();
				options["MaximumBet"] = xml.Descendants(Ns.Rt2 + "Enumeration").Select(d => d.Value).ToList();
				payvarDataList.Add(new PayvarData(payvarName, xml, options));
			}
		}

		/// <summary>
		/// Load the configuration options for rendering the single game UI.
		/// </summary>
		private void InitSinglegame()
		{
			// Merge the options for each payvar into a master list.
			foreach (var o in payvarDataList.Select(p => p.Options))
			{
				foreach (var kvp in o)
				{
					singlegameConfigs[kvp.Key] = singlegameConfigs.ContainsKey(kvp.Key)
						? singlegameConfigs[kvp.Key].Concat(kvp.Value).Distinct().OrderBy(s => s).ToList()
						: kvp.Value.Distinct().OrderBy(s => s).ToList();
				}
			}

			// Add -any- to the start of each option.
			singlegameConfigs = singlegameConfigs.ToDictionary(kvp => kvp.Key, kvp => new[] { "-any-" }.Concat(kvp.Value).ToList());

			// Merge the options with the existing settings.
			generateSettings.SelectedSingleGameOptions = singlegameConfigs.Select(kvp =>
			{
				var existingSelection = generateSettings.SelectedSingleGameOptions.FirstOrDefault(o => o.Name == kvp.Key)?.Value;

				return existingSelection != null && kvp.Value.Contains(existingSelection)
					? new NameValue(kvp.Key, existingSelection)
					: new NameValue(kvp.Key, "-any-");
			}).ToArray();
		}

		/// <summary>
		/// Load the MultigamePackageData file options for rendering the multigame UI.
		/// </summary>
		private void InitMultigame()
		{
			if (File.Exists(@"Registries\MultigamePackageData.xml"))
			{
				var multigameXml = XDocument.Load("Registries\\MultigamePackageData.xml");

				foreach (var cg in multigameXml.Descendants("Game").Single().Elements("ConfigGroup"))
				{
					var id = cg.Attribute("ID")?.Value ?? "";
					var att = cg.Attribute("VirtualGameName") ?? cg.Attribute("VirtualGameId");
					multigameGroups.Add((id, att?.Value ?? "-no name-", cg.Elements().Select(e => (Name: e.Attribute("Name")?.Value, Value: e.Attribute("Value")?.Value)).ToList()));
				}
			}
		}

		/// <summary>
		/// Render the configuration options.
		/// </summary>
		private void RenderSinglegame()
		{
			EditorGUILayout.LabelField("Apply these filters to reduce the payvars selected", EditorStyles.boldLabel);

			var lw = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 300;
			scrollView = EditorGUILayout.BeginScrollView(scrollView);

			foreach (var item in generateSettings.SelectedSingleGameOptions)
			{
				var array = singlegameConfigs.Single(o => o.Key == item.Name).Value.ToArray();
				Array.Sort(array, Comparison);
				var valueIndex = array.FindIndex(item.Value);
				item.Value = array[EditorGUILayout.Popup(item.Name, valueIndex < 0 ? 0 : valueIndex, array.ToArray())];
			}

			EditorGUILayout.EndScrollView();
			EditorGUIUtility.labelWidth = lw;
		}

		/// <summary>
		/// Render the configuration groups from the MultigamePackageData file.
		/// </summary>
		private void RenderMultigame()
		{
			if (multigameGroups.Any())
			{
				var lw = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 300;
				generateSettings.SelectedMultiGameConfig = EditorGUILayout.Popup("Select a Multigame Configuration", Math.Min(generateSettings.SelectedMultiGameConfig, multigameGroups.Count - 1), multigameGroups.Select(g => g.GroupId).Distinct().OrderBy(i => i).ToArray());
				EditorGUIUtility.labelWidth = lw;

				scrollView = EditorGUILayout.BeginScrollView(scrollView);

				foreach (var configGroup in multigameGroups.GroupBy(g => g.GroupId))
				{
					var rows = configGroup.Select(g => new[] { g.VirtualGameName }.Concat(g.ConfigItems.Select(i => i.ConfigValue)).ToList()).ToList();
					rows.Insert(0, new[] { $"Configuration {configGroup.Key}" }.Concat(configGroup.First().ConfigItems.Select(i => i.ConfigName.Replace("CreditDenomination", "Denom"))).ToList());
					RenderTable(rows);
					EditorGUILayout.Space();
				}

				EditorGUILayout.EndScrollView();
			}
			else
			{
				scrollView = EditorGUILayout.BeginScrollView(scrollView);
				EditorGUILayout.LabelField("MultigamePackageData.xml not found!");
				EditorGUILayout.EndScrollView();
			}
		}

		/// <summary>
		/// Render the payvars that are will be put in the systemconfig file.
		/// </summary>
		private void RenderPayvars(IReadOnlyList<(PayvarData Payvar, string MaxBet, string Denom)> payvars)
		{
			EditorGUILayout.LabelField($"Payvars selected: {payvars.Count.ToString()}{(payvars.Count > 10 ? " (showing 10)" : "")}");
			EditorGUILayout.Space();

			if (payvars.Count == 0)
				return;

			var rows = payvars.Select(p => new[] { p.Payvar.Name }.Concat(p.Payvar.Options.Select(o => FormatValue(p, o))).ToList()).Take(10).ToList();
			rows.Insert(0, new[] { "Payvar" }.Concat(payvars[0].Payvar.Options.Select(o => o.Key.Replace("CreditDenomination", "Denom"))).ToList());
			RenderTable(rows);

			EditorGUILayout.Space();

			string FormatValue((PayvarData Payvar, string MaxBet, string Denom) valueTuple, KeyValuePair<string, IReadOnlyList<string>> option)
			{
				if (option.Value.Count == 1)
					return option.Value.First();

				var vals = option.Key switch
				{
					"CreditDenomination" => option.Value.Select(o => o == valueTuple.Denom ? $"*{o}" : o).ToList(),
					"MaximumBet" => option.Value.Select(o => o == valueTuple.MaxBet ? $"*{o}" : o).ToList(),
					_ => option.Value
				};

				return string.Join(" ", vals);
			}
		}

		/// <summary>
		/// Render a horizontal line on the UI.
		/// </summary>
		private static void RenderLine(Color color, float thickness = 2.0f, float padding = 40.0f)
		{
			var r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
			r.height = thickness;
			r.y += padding / 2.0f;
			r.x -= 2.0f;
			r.width += 6.0f;
			EditorGUI.DrawRect(r, color);
		}

		/// <summary>
		/// Render a table of string data.
		/// </summary>
		private static void RenderTable(IReadOnlyList<IReadOnlyList<string>> rows, bool highlightFirstRow = true)
		{
			// Get the column widths.
			var widths = Zip(rows, columns => columns.Max(t => GUI.skin.label.CalcSize(new GUIContent(t)).x * 1.2f)).ToArray();

			// Render the rows
			for (var r = 0; r < rows.Count; r++)
			{
				GUILayout.BeginHorizontal();

				for (var c = 0; c < rows[r].Count; c++)
				{
					if (highlightFirstRow && r == 0)
						GUILayout.Label(rows[r][c], EditorStyles.boldLabel, GUILayout.Width(widths[c]));
					else
						GUILayout.Label(rows[r][c], GUILayout.Width(widths[c]));
				}

				GUILayout.EndHorizontal();
			}
		}

		/// <summary>
		/// Get the payvars that result from the UI filtering.
		/// </summary>
		private IReadOnlyList<(PayvarData Payvar, string MaxBet, string Denom)> GetSelectedPayvars()
		{
			var payvars = new List<(PayvarData Payvar, string MaxBet, string Denom)>();
			if (isMultigame)
			{
				if (!multigameGroups.Any())
					return payvars;

				var configGroupId = multigameGroups.GroupBy(g => g.GroupId).ToArray()[generateSettings.SelectedMultiGameConfig].Key;

				foreach (var cg in XDocument.Load("Registries\\MultigamePackageData.xml").Descendants("Game").Single().Elements("ConfigGroup").Where(g => g.Attribute("ID")?.Value == configGroupId))
				{
					var configItems = cg.Elements("ConfigItem").Select(ci =>
					{
						var n = ci.Attribute("Name")?.Value ?? "";
						var v = ci.Attribute("Value")?.Value ?? "";

						v = n switch
						{
							"CreditDenomination" => v.Replace("u", ""),
							"MaximumBet" => v.Replace("cr", ""),
							_ => v
						};

						return (n, v);
					}).ToList();

					var data = payvarDataList;
					foreach (var (n, v) in configItems)
					{
						foreach (var t in data.Where(t => t.Options.ContainsKey(n) && !t.Options[n].Contains(v)))
							data = data.Except(new[] { t }).ToList();
					}

					if (data.Count > 1)
						Debug.LogWarning("More than 1 match for a MCG.");

					var creditDenom = configItems.Single(ci => ci.n == "CreditDenomination").v;
					var maxBet = configItems.Single(ci => ci.n == "MaximumBet").v;
					payvars.AddRange(data.Select(t => (t, maxBet, creditDenom)));
				}

				return payvars.OrderBy(p => p.Payvar.Name).ToList();
			}

			var removeNotSelectedOptions = generateSettings.SelectedSingleGameOptions.Where(item => item.Name != "CreditDenomination" && item.Name != "MaximumBet" && item.Value != "-any-").ToList();
			foreach (var data in payvarDataList)
			{
				if (!removeNotSelectedOptions.All(t => data.Options[t.Name].Contains(t.Value)))
					continue;

				var mb = generateSettings.SelectedSingleGameOptions.Single(o => o.Name == "MaximumBet");
				var cd = generateSettings.SelectedSingleGameOptions.Single(o => o.Name == "CreditDenomination");

				if (mb.Value != "-any-" && !data.Options["MaximumBet"].Contains(mb.Value))
					continue;
				if (cd.Value != "-any-" && !data.Options["CreditDenomination"].Contains(cd.Value))
					continue;

				var maxBet = mb.Value != "-any-" ? mb.Value : data.Options["MaximumBet"].Last();

				if (cd.Value == "-any-")
					payvars.AddRange(data.Options["CreditDenomination"].Select(denom => (data, maxBet, denom)));
				else
					payvars.Add((data, maxBet, cd.Value));
			}

			return payvars;
		}

		/// <summary>
		/// Check for common errors that will cause the systemconfig file to be invalid 
		/// </summary>
		private string CheckForErrors(IReadOnlyList<(PayvarData Payvar, string MaxBet, string Denom)> payvars)
		{
			if (payvars.Count == 0)
				return "No payvars are selected";

			// Check that 2 payvars do not have the same denom.
			if (payvars.SelectMany(p => p.Payvar.XDocument.Descendants(Ns.Pr3 + "Denom").Select(d => d.Value)).Distinct().Count() < payvars.Count)
				return "More than one payvar with the same denomination";

			return null;
		}

		/// <summary>
		/// Helper method to zip multiple enumerations together.
		/// </summary>
		private static IEnumerable<TResult> Zip<T, TResult>(IEnumerable<IEnumerable<T>> sequences, Func<T[], TResult> resultSelector)
		{
			var enumerators = sequences.Select(s => s.GetEnumerator()).ToArray();
			while (enumerators.All(e => e.MoveNext()))
				yield return resultSelector(enumerators.Select(e => e.Current).ToArray());
		}

		/// <summary>
		/// Helper method to sort configuration values nicely.
		/// </summary>
		private static int Comparison(string a, string b)
		{
			if (a == "-any-")
				return -1;

			if (b == "-any-")
				return 1;

			return int.TryParse(a, out var ai) && int.TryParse(b, out var bi)
				? ai.CompareTo(bi)
				: string.Compare(a, b, StringComparison.CurrentCulture);
		}

		#endregion
	}
}