using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Game;
using IGT.Ascent.Assets.StandaloneSafeStorage.Editor;
using Logic.Core.Types;
using Logic.Core.Utility;
using Midas.Gle.LogicToPresentation;
using Midas.Gle.Presentation.Editor;
using Midas.Presentation.General;
using Midas.Presentation.Meters;
using Midas.Presentation.Progressives;
using Midas.Tools.Humanize;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using GameStages = Midas.Gle.MakeGame.Editor.ScriptTemplates.GameStages;
using Input = Logic.Core.Engine.Input;

namespace Midas.Gle.MakeGame.Editor
{
	public sealed partial class MakeGame : EditorWindow
	{
		#region Constants

		private const string GamePath = @"Assets\Game\";
		private const string GameConfiguratorPath = GamePath + "GameConfigurator.cs";
		private const string GameStagesPath = GamePath + "GameStages.cs";

		private const string ScriptTemplatePath = @"Assets\Midas\Gle\MakeGame\Editor\ScriptTemplates\";
		private const string GameConfiguratorTemplatePath = ScriptTemplatePath + "GameConfigurator.cs";
		private const string GameStagesTemplatePath = ScriptTemplatePath + "GameStages.cs";
		private const string CyclesXOfYControllerTemplatePath = ScriptTemplatePath + "CyclesXOfYController.cs";
		private const string CyclesRemainingControllerTemplatePath = ScriptTemplatePath + "CyclesRemainingController.cs";

		private const string StageAssetsPath = GamePath + @"Stages\";

		private static readonly string gameConfiguratorTemplate = File.ReadAllText(GameConfiguratorTemplatePath);
		private static readonly string gameStagesTemplate = File.ReadAllText(GameStagesTemplatePath);
		private static readonly string cyclesXOfYCounterTemplate = File.ReadAllText(CyclesXOfYControllerTemplatePath);
		private static readonly string cyclesRemainingCounterTemplate = File.ReadAllText(CyclesRemainingControllerTemplatePath);

		#endregion

		#region Types

		private enum BuildState
		{
			Idle,
			GenerateScripts,
			WaitForBuild,
			CreateGameObjects,
			Finishing
		}

		private enum PromptStyle
		{
			/// <summary>
			/// Plays continuously
			/// </summary>
			[Tooltip("No Prompt, game plays continuously")]
			None,

			/// <summary>
			/// Wait for start feature if entering stage (ie, free games)
			/// </summary>
			/// <remarks>
			/// Generates a transition sequence that has "wait for start feature" steps in it
			/// </remarks>
			[Tooltip("Wait for Start Feature, game waits on entry to the stage but plays continuously after player hits start feature")]
			WaitStartFeature,

			/// <summary>
			/// Waits per cycle for player input (ie, respins)
			/// </summary>
			/// <remarks>
			/// Generates a presentation node to run before the reels spin
			/// </remarks>
			[Tooltip("Wait Per Cycle, game prompts the player to hit play on every cycle")]
			WaitPerCycle
		}

		private enum CounterStyle
		{
			/// <summary>
			/// No cycle counter is used.
			/// </summary>
			[Tooltip("No Counter")]
			None,

			/// <summary>
			/// The X Of Y Cycles remaining style of feature cycle count.
			/// </summary>
			/// <remarks>
			/// Generates the classes and game objects to show X Of Y.
			/// </remarks>
			[Tooltip("Shows the feature cycle count as X of Y")]
			XOfY,

			/// <summary>
			/// The X Cycles remaining style of feature cycle count.
			/// </summary>
			/// <remarks>
			/// Generates the classes and game objects to show X Cycles Remaining.
			/// </remarks>
			[Tooltip("Shows the feature cycle count as X Remaining")]
			Remaining
		}

		private enum PaylineStyle
		{
			/// <summary>
			/// No paylines are generated.
			/// </summary>
			[Tooltip("No lines")]
			None,

			/// <summary>
			/// Classic line and box meshes.
			/// </summary>
			[Tooltip("Legacy line and box mesh style")]
			LineAndBoxMesh,

			/// <summary>
			/// Adds small "beans" between the reels to indicate the line.
			/// </summary>
			Beans
		}

		[Serializable]
		private class StageConfiguration
		{
			public List<ReelConfiguration> reelConfigs = new List<ReelConfiguration>();
			public List<string> patterns;
			public string selectedPatterns;
			public string selectedPopulator;
			public PromptStyle promptStyle;
			public CounterStyle counterStyle;
			public PaylineStyle paylineStyle;
		}

		[Serializable]
		private sealed class ReelConfiguration
		{
			public string symbolWindowResultName;
			public string symbolWindowStructureName;
			public bool inheritPrevResult;

			public ReelConfiguration(string symbolWindowResultName, string symbolWindowStructureName, bool inheritResult)
			{
				this.symbolWindowResultName = symbolWindowResultName;
				this.symbolWindowStructureName = symbolWindowStructureName;
				inheritPrevResult = inheritResult;
			}
		}

		[Serializable]
		private sealed class StageConfigurationDictionary : SerializableDictionary<string, StageConfiguration>
		{
			public StageConfigurationDictionary()
			{
			}

			public StageConfigurationDictionary(IDictionary<string, StageConfiguration> source)
			{
				foreach (var kvp in source)
					this[kvp.Key] = kvp.Value;
			}
		}

		#endregion

		#region Fields

		private StageConfigurationDictionary stageConfigurations;
		private BuildState buildState = BuildState.Idle;
		private GlyphFactory progGlyphFactory;
		private string imageFolder = string.Empty;
		private Vector2 scrollPos;
		private string gameName;

		#endregion

		/// <summary>
		/// This command will open the make game window.
		/// </summary>
		[MenuItem("Midas/Tools/Make Game")]
		public static void MenuCreateGameStateMachine()
		{
			var w = GetWindow<MakeGame>(true, "Make Game");
			w.progGlyphFactory = AssetDatabase.LoadAssetAtPath<GlyphFactory>("Assets/Game/Progressives/ProgressiveGlyphs.asset");

			var id = Path.Combine(Application.dataPath, "../../", "Logic/Player/Game/GameData/Images");
			if (!Directory.Exists(id))
				id = Path.Combine(Application.dataPath, "../../");

			w.imageFolder = new DirectoryInfo(id).FullName;
			w.gameName = GetGameName();
			w.Show();
		}

		/// <summary>
		/// The window processor for the make game GUI.
		/// </summary>
		private void OnGUI()
		{
			var goGoGo = true;
			var bigLabel = new GUIStyle(EditorStyles.boldLabel);
			bigLabel.fontSize += 4;
			GUILayout.Label("Build an initial structure for a game based off stage model data", bigLabel);
			GUILayout.Space(10);
			scrollPos = GUILayout.BeginScrollView(scrollPos);
			GUI.enabled = !Application.isPlaying && buildState == BuildState.Idle;

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Game Name", EditorStyles.boldLabel, GUILayout.MaxWidth(100.0f));
			EditorGUILayout.TextArea(gameName);
			EditorGUILayout.EndHorizontal();

			// If stageConfigurations is null then we have not initialised yet.

			var stages = GleGameData.Stages;
			if (stageConfigurations == null)
			{
				stageConfigurations = new StageConfigurationDictionary();

				foreach (var stage in stages)
				{
					var sc = new StageConfiguration();
					stageConfigurations.Add(stage.Name, sc);
					sc.patterns = stage.GetPatterns().Select(p => p.name).ToList();
					sc.selectedPatterns = sc.patterns.FirstOrDefault();
					sc.promptStyle = stage.IsEntryStage ? PromptStyle.None : PromptStyle.WaitStartFeature;
					sc.counterStyle = stage.IsEntryStage ? CounterStyle.None : stage.Name.ToLower().Contains("respin") ? CounterStyle.Remaining : CounterStyle.XOfY;
					sc.paylineStyle = sc.patterns.Count > 0 ? PaylineStyle.Beans : PaylineStyle.None;

					var reelInfoEntries = stage.GetReelInfoEntries();

					foreach (var swp in stage.GetSymbolWindowResultProperties())
					{
						var isLockedReels = !stage.IsEntryStage && swp.Prop.PropertyType == typeof(LockedSymbolWindowResult);
						if (isLockedReels)
							sc.promptStyle = PromptStyle.WaitPerCycle;
						sc.reelConfigs.Add(new ReelConfiguration(swp.Prop.Name, reelInfoEntries[0].Name, isLockedReels));
					}
				}
			}

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(new GUIContent("Image Folder", @"Enter the fully qualified path to a folder that contains symbol images for the game. EG. C:\MyGame\SymbolImages"), EditorStyles.boldLabel, GUILayout.MaxWidth(100.0f));
			imageFolder = EditorGUILayout.TextArea(imageFolder);
			if (GUILayout.Button("...", GUILayout.MaxWidth(50.0f)))
				imageFolder = EditorUtility.OpenFolderPanel("Symbol Image Folder", imageFolder, string.Empty);
			EditorGUILayout.EndHorizontal();
			if (!Directory.Exists(imageFolder))
			{
				var s = new GUIStyle();
				s.normal.textColor = Color.red;
				EditorGUILayout.LabelField(new GUIContent("Image Folder does not exist"), s);
				goGoGo = false;
			}

			foreach (var stage in stages)
			{
				var sc = stageConfigurations[stage.Name];
				GUILayout.Space(15);

				EditorGUILayout.LabelField(string.Format($"{stage.Name} Stage:"), EditorStyles.boldLabel);
				if (!stage.IsEntryStage)
				{
					sc.promptStyle = (PromptStyle)EditorGUILayout.EnumPopup(new GUIContent("Prompt Style", "What type of prompt should be used for this stage"), sc.promptStyle);
					sc.counterStyle = (CounterStyle)EditorGUILayout.EnumPopup(new GUIContent("Counter Style", "What type of cycle counter should be used for this stage"), sc.counterStyle);
				}

				EditorGUI.indentLevel++;

				if (sc.reelConfigs.Count > 0)
				{
					EditorGUILayout.LabelField("Reel Populators:");

					var reelInfoEntries = stage.GetReelInfoEntries();
					foreach (var reelConfig in sc.reelConfigs)
					{
						var isSelected = reelConfig.symbolWindowStructureName != null;
						if (isSelected != EditorGUILayout.ToggleLeft(reelConfig.symbolWindowResultName, isSelected))
						{
							isSelected = !isSelected;
							reelConfig.symbolWindowStructureName = isSelected ? reelInfoEntries[0].Name : null;
						}

						EditorGUI.indentLevel++;

						if (isSelected)
						{
							var options = reelInfoEntries.Select(ri => ri.Name).ToArray();
							var selectedIndex = options.IndexOf(reelConfig.symbolWindowStructureName);
							if (selectedIndex == -1)
								selectedIndex = 0;
							reelConfig.symbolWindowStructureName = options[EditorGUILayout.Popup("Window Structure", selectedIndex, options.ToArray())];
							reelConfig.inheritPrevResult = EditorGUILayout.Toggle(new GUIContent("Inherit Prev Result", "Inherit the symbol window from triggering stage"), reelConfig.inheritPrevResult);
						}

						EditorGUI.indentLevel--;
					}

					var selectablePops = sc.reelConfigs.Where(s => s.symbolWindowStructureName != null).Select(s => s.symbolWindowStructureName).ToArray();

					if (selectablePops.Length > 0)
					{
						sc.selectedPopulator ??= selectablePops[0];

						var index = Array.IndexOf(selectablePops, sc.selectedPopulator);
						sc.selectedPopulator = selectablePops[EditorGUILayout.Popup(new GUIContent("WinCycle Populator"), index, selectablePops)];
					}

					if (sc.patterns.Count > 0)
					{
						var generatePaylines = sc.selectedPatterns != null;
						generatePaylines = EditorGUILayout.Toggle("Generate Paylines?", generatePaylines);
						if (generatePaylines)
						{
							sc.selectedPatterns ??= sc.patterns[0];
							var index = sc.patterns.IndexOf(sc.selectedPatterns);
							sc.selectedPatterns = sc.patterns[EditorGUILayout.Popup(new GUIContent("Payline Data"), index, sc.patterns.ToArray())];
							sc.paylineStyle = (PaylineStyle)EditorGUILayout.EnumPopup("Payline Style", sc.paylineStyle);
						}
						else
						{
							sc.selectedPatterns = null;
						}
					}
				}

				EditorGUI.indentLevel--;
			}

			GUILayout.Space(15);
			EditorGUILayout.LabelField("Other Settings:", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;

			var progs = GleGameData.Progressives.GetProgressiveLevels(Array.Empty<Input>());

			if (progs.Count != 0)
			{
				progGlyphFactory = (GlyphFactory)EditorGUILayout.ObjectField(new GUIContent("Progressive Glyphs"), progGlyphFactory, typeof(GlyphFactory), false);
				if (progGlyphFactory == null)
				{
					var s = new GUIStyle();
					s.normal.textColor = Color.red;
					EditorGUILayout.LabelField(new GUIContent("Progressive glyph factory is required to build progressive meters"), s);
					goGoGo = false;
				}
			}

			EditorGUI.indentLevel--;

			GUILayout.EndScrollView();

			GUILayout.Space(15);
			GUILayout.Label("There is no undoing this. Make sure anything you want to save is backed up!", bigLabel);
			EditorGUILayout.BeginHorizontal();

			GUI.enabled = goGoGo;

			if (GUILayout.Button("Go"))
			{
				buildState = BuildState.GenerateScripts;
			}

			GUI.enabled = true;

			if (GUILayout.Button("Cancel"))
				Close();

			EditorGUILayout.EndHorizontal();

			GUI.enabled = true;
		}

		/// <summary>
		/// This is the meat of the state machine builder. This is called every frame while the make game dialog is visible.
		/// </summary>
		private void Update()
		{
			// Unfortunately this cannot be a coroutine, so a state machine it is.

			switch (buildState)
			{
				case BuildState.GenerateScripts:
				{
					if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
					{
						EditorUtility.DisplayDialog("MAKE GAME", "Game generation cancelled.", "OK");
						buildState = BuildState.Idle;
					}
					else
					{
						GenerateScripts();
						AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
						buildState = BuildState.WaitForBuild;
					}

					break;
				}

				case BuildState.WaitForBuild:
				{
					if (!EditorApplication.isCompiling)
					{
						buildState = BuildState.CreateGameObjects;
					}

					break;
				}

				case BuildState.CreateGameObjects:
				{
					CreateGameObjects();
					buildState = BuildState.Finishing;
					break;
				}

				case BuildState.Finishing:
				{
					if (!EditorApplication.isCompiling)
					{
						SafeStorageMenu.Clear();
						EditorUtility.DisplayDialog("MAKE GAME", "Game generation complete and Safe Storage cleared", "OK");
						var w = GetWindow<MakeGame>();
						w.Close();

						buildState = BuildState.Idle;
					}

					break;
				}
			}
		}

		private void GenerateScripts()
		{
			var baseStage = "";
			var gameStages = new StringBuilder();
			var stageMap = new StringBuilder();
			var gameNodesConfig = new StringBuilder();
			var gameNodesList = new StringBuilder();
			var sequenceEventIds = new StringBuilder();
			var sequences = new StringBuilder();
			var namespaces = new StringBuilder();
			var presentationControllersConfig = new StringBuilder();
			var presentationControllers = new StringBuilder();

			var stageIndex = 0;

			GenerateCounterScripts(presentationControllers);

			foreach (var gleStage in GleGameData.Stages)
			{
				var stageConfig = stageConfigurations[gleStage.Name];
				var stageNameNoSpaces = gleStage.Name.Replace(" ", "");
				if (gleStage.IsEntryStage)
					baseStage = $"=> {nameof(GameStages)}.{stageNameNoSpaces}";

				gameStages.AppendLine($"\t\tpublic static Stage {stageNameNoSpaces} {{ get; }} = new Stage(Midas.Presentation.StageHandling.Stages.GameSpecificStartId + {stageIndex});");
				stageMap.AppendLine($"\t\t\t\t(\"{gleStage.Name}\", {nameof(GameStages)}.{stageNameNoSpaces}),");
				gameNodesList.AppendLine($"\t\t\t\tnew MainWinPresNode(\"{stageNameNoSpaces}WinPres\", {nameof(GameStages)}.{stageNameNoSpaces}),");

				if (stageConfig.reelConfigs.Count > 0)
				{
					var nodeName = $"{stageNameNoSpaces}Node".Camelize();
					gameNodesConfig.AppendLine($"\t\t\tvar {nodeName} = new {nameof(SimpleGameNode)}(\"{stageNameNoSpaces}Stage\", {nameof(GameStages)}.{stageNameNoSpaces});");
					gameNodesConfig.AppendLine($"\t\t\t{nodeName}.{nameof(SimpleGameNode.AddPreShowSequence)}(\"Trigger\", CheckTrigger);");
					gameNodesList.AppendLine($"\t\t\t\t{nodeName},");

					var stageLocation = $"{StageAssetsPath}{stageNameNoSpaces}";
					if (!Directory.Exists(stageLocation))
						Directory.CreateDirectory(stageLocation);
				}

				gameNodesList.AppendLine($"\t\t\t\tnew {nameof(SimpleTransitionNode)}(\"{stageNameNoSpaces}Transition\", {nameof(GameStages)}.{stageNameNoSpaces}),");

				if (stageConfig.promptStyle == PromptStyle.WaitPerCycle)
					gameNodesList.AppendLine($"\t\t\t\tPromptPerCycleNode.Create(\"{stageNameNoSpaces}Prompt\", {nameof(GameStages)}.{stageNameNoSpaces}),");

				// Create all the possible transitions between stages. We don't have any info on what can and can't transition, so we just make them all.

				foreach (var nextGleStage in GleGameData.Stages)
				{
					// Can't transition one stage to itself.

					if (ReferenceEquals(gleStage, nextGleStage))
						continue;

					var nextStageNameNoSpaces = nextGleStage.Name.Replace(" ", "");

					var nextStageConfig = stageConfigurations[nextGleStage.Name];
					var sequenceName = $"Game/{stageNameNoSpaces}To{nextStageNameNoSpaces}";

					// Only make a start feature transition if there is a stage connection between the stages

					var makeStartFeatureTrans = nextStageConfig.promptStyle == PromptStyle.WaitStartFeature && GleGameData.StageConnections.Any(c => c.InitialStage == gleStage && c.FinalStage == nextGleStage);

					sequences.AppendLine(makeStartFeatureTrans
						? $"\t\t\t\t({nameof(GameStages)}.{stageNameNoSpaces}, {nameof(GameStages)}.{nextStageNameNoSpaces}, SimpleSequence.Create<StartFeatureSequence>(\"{sequenceName}\")),"
						: $"\t\t\t\t({nameof(GameStages)}.{stageNameNoSpaces}, {nameof(GameStages)}.{nextStageNameNoSpaces}, SimpleSequence.Create<ShowStageSequence>(\"{sequenceName}\")),");
				}

				stageIndex++;
			}

			// Include the built-in progressive award if the game has progressives

			var progressiveLevels = GleGameData.Progressives.GetProgressiveLevels(Array.Empty<Input>());

			foreach (var sc in stageConfigurations)
				presentationControllers.AppendLine($"\t\t\t\tnew GameMessageController(GameStages.{sc.Key}),");

			namespaces.AppendLine("using Game.GameMessages;");

			if (progressiveLevels.Count != 0)
			{
				namespaces.AppendLine($"using {typeof(SimpleProgressiveAwardController).Namespace};");
				presentationControllersConfig.AppendLine($"\t\t\tvar progAwardCont = new {nameof(SimpleProgressiveAwardController)}();");
				presentationControllers.AppendLine("\t\t\t\tprogAwardCont,");

				foreach (var progressiveLevel in progressiveLevels)
				{
					presentationControllersConfig.AppendLine($"\t\t\tprogAwardCont.{nameof(SimpleProgressiveAwardController.RegisterProgressiveAwardSequence)}(\"{progressiveLevel.Identifier}\");");
				}
			}

			var gameStagesText = new StringBuilder();
			gameStagesText.Append(gameStagesTemplate.Replace("Midas.Gle.MakeGame.Editor.ScriptTemplates", "Game"));
			gameStagesText.Replace("//STAGES" + Environment.NewLine, gameStages.ToString());
			File.WriteAllText(GameStagesPath, gameStagesText.ToString());

			var gameConfiguratorText = new StringBuilder();
			gameConfiguratorText.Append(gameConfiguratorTemplate.Replace("Midas.Gle.MakeGame.Editor.ScriptTemplates", "Game"));
			gameConfiguratorText.Replace("/*BASESTAGE*/", baseStage);
			gameConfiguratorText.Replace("//NAMESPACES" + Environment.NewLine, namespaces.ToString());
			gameConfiguratorText.Replace("//STAGEMAP" + Environment.NewLine, stageMap.ToString());
			gameConfiguratorText.Replace("//NODESCONFIG" + Environment.NewLine, gameNodesConfig.ToString());
			gameConfiguratorText.Replace("//NODES" + Environment.NewLine, gameNodesList.ToString());
			gameConfiguratorText.Replace("//SEQUENCEEVENTS" + Environment.NewLine, sequenceEventIds.ToString());
			gameConfiguratorText.Replace("//SEQUENCES" + Environment.NewLine, sequences.ToString());
			gameConfiguratorText.Replace("//TEMPLATE GAME", gameName);
			gameConfiguratorText.Replace("//CONTROLLERSCONFIG" + Environment.NewLine, presentationControllersConfig.ToString());
			gameConfiguratorText.Replace("//CONTROLLERS" + Environment.NewLine, presentationControllers.ToString());
			File.WriteAllText(GameConfiguratorPath, gameConfiguratorText.ToString());
		}

		private void GenerateCounterScripts(StringBuilder presentationControllers)
		{
			foreach (var sc in stageConfigurations)
			{
				switch (sc.Value.counterStyle)
				{
					case CounterStyle.XOfY:
					{
						var counterText = cyclesXOfYCounterTemplate.Replace("CyclesXOfYStatus", $"{sc.Key}XOfYStatus");
						counterText = counterText.Replace("CyclesXOfYController", $"{sc.Key}XOfYController");
						File.WriteAllText($"{GamePath}{sc.Key}XOfYController.cs", counterText);
						presentationControllers.AppendLine($"\t\t\t\tnew {sc.Key}XOfYController(GameStages.{sc.Key}),");
						break;
					}
					default:
					case CounterStyle.Remaining:
					{
						var counterText = cyclesRemainingCounterTemplate.Replace("CyclesRemainingStatus", $"{sc.Key}RemainingStatus");
						counterText = counterText.Replace("CyclesRemainingController", $"{sc.Key}RemainingController");
						counterText = counterText.Replace("CyclesRemainingState", $"{sc.Key}RemainingState");
						File.WriteAllText($"{GamePath}{sc.Key}RemainingController.cs", counterText);

						var countEventText = $"using Midas.Presentation.Data.PropertyReference;\n\nnamespace Game\n{{\n\tpublic sealed class {sc.Key}RemainingStatePropUnityEvents : PropertyRefEnumUnityEvent<{sc.Key}RemainingState>\n\t{{\n\t}}\n}}";
						File.WriteAllText($"{GamePath}{sc.Key}RemainingStatePropUnityEvents.cs", countEventText);
						presentationControllers.AppendLine($"\t\t\t\tnew {sc.Key}RemainingController(GameStages.{sc.Key}),");
						break;
					}
					case CounterStyle.None:
						break;
				}
			}
		}

		private static string GetGameName()
		{
			const string filename = "Registries/game.xausreg";
			if (!File.Exists(filename))
				return "TEMPLATE GAME";

			var ausRegFile = new XmlDocument();

			using var streamReader = new StreamReader("Registries/game.xausreg");
			ausRegFile.Load(streamReader);
			streamReader.Close();

			return ausRegFile.DocumentElement == null ? "TEMPLATE GAME" : ausRegFile.DocumentElement.GetAttribute("GameName").ToUpper();
		}
	}
}