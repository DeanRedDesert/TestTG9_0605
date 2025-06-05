using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Game;
using Game.GameMessages;
using Game.Stages.Common;
using Game.Stages.Common.PreShow;
using Game.WinPresentation;
using Midas.Gle.LogicToPresentation;
using Midas.Gle.Presentation;
using Midas.Gle.Presentation.Editor;
using Midas.Presentation.Audio;
using Midas.Presentation.Dashboard;
using Midas.Presentation.Data;
using Midas.Presentation.Data.PropertyReference;
using Midas.Presentation.Editor.GameData;
using Midas.Presentation.Editor.General;
using Midas.Presentation.Editor.Paylines;
using Midas.Presentation.Editor.Reels;
using Midas.Presentation.Editor.Utilities;
using Midas.Presentation.ExtensionMethods;
using Midas.Presentation.General;
using Midas.Presentation.Meters;
using Midas.Presentation.Paylines;
using Midas.Presentation.Progressives;
using Midas.Presentation.Reels;
using Midas.Presentation.Reels.Sound;
using Midas.Presentation.SceneManagement;
using Midas.Presentation.Sequencing;
using Midas.Presentation.StageHandling;
using Midas.Presentation.Symbols;
using Midas.Presentation.Transitions;
using Midas.Presentation.WinPresentation;
using Midas.Utility.ResultPicker;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using Input = Logic.Core.Engine.Input;
using SequenceEvent = Midas.Presentation.Sequencing.SequenceEvent;
using Stage = Midas.Presentation.StageHandling.Stage;

namespace Midas.Gle.MakeGame.Editor
{
	public sealed partial class MakeGame
	{
		private const string PrefabsTemplatePath = @"Assets\Midas\Gle\MakeGame\Editor\PrefabTemplates\";

		private const string WhiteMaskPath = GamePath + @"\Assets\WhiteSquare.png";
		private const string SpinTimingsPath = GamePath + @"\Assets\SpinSettings";
		private const string PaylineColorPath = GamePath + @"\Assets\PaylineColorSelector.asset";
		private const string PaylineSettingsPath = GamePath + @"\Assets\PaylineSettings.asset";
		private const string UtilitySettingsPath = GamePath + @"\Assets\Utility\UtilitySymbolWindows.asset";
		private const string ReelShakePrefabPath = GamePath + @"\WinPresentation\Prefabs\ReelShake.prefab";
		private const string StagesCommonPath = GamePath + @"\Stages\Common\";
		private const string SymbolPrefabPath = StagesCommonPath + @"Symbols\Symbol.prefab";
		private const string FallbackSymbolPath = StagesCommonPath + @"Symbols\FallbackSymbol.prefab";
		private const string CashOnReelsSymbolPath = StagesCommonPath + @"Symbols\CashOnReelsSymbolBuilder.asset";
		private const string CommonPrefabPath = StagesCommonPath + @"Prefabs\Common.prefab";
		private const string StartFeaturePrefabPath = GamePath + @"\Stages\Common\Prefabs\StartFeatureBanner.prefab";
		private const string PromptPerCyclePrefabPath = GamePath + @"\Stages\Common\Prefabs\RespinPromptBanner.prefab";
		private const string GameMessagesPrefabPath = GamePath + @"\GameMessages\GameMessages.prefab";
		private const string ButtonPanelContentPrefabPath = GamePath + @"\Stages\Common\BashButton\ButtonPanelContent.prefab";
		private const string ProgressiveAwardPrefabPath = GamePath + @"\Progressives\ProgressiveAward.prefab";
		private const string BeanPaylinesPrefabPath = GamePath + @"\Stages\Common\Paylines\Bean Paylines.prefab";
		private const string CycleXOfYPrefabPath = PrefabsTemplatePath + @"\CyclesXOfY.prefab";
		private const string CycleRemainingPrefabPath = PrefabsTemplatePath + @"\CyclesRemaining.prefab";
		private const string CurrentWinStatusProp = nameof(StatusDatabase) + "." + nameof(DetailedWinPresStatus) + "." + nameof(DetailedWinPresStatus.HighlightedWin);
		private const string WinsStatusProp = nameof(StatusDatabase) + "." + nameof(DetailedWinPresStatus) + "." + nameof(DetailedWinPresStatus.Wins);
		private const string SlideInPropPath = "Expressions.Dashboard." + nameof(DashboardExpressions.ShowGameContentOnMain);

		private const string StartupScenePath = ScenesPath + "Startup.unity";
		private const string GameScenePath = ScenesPath + "Game.unity";

		private const string ScenesPath = GamePath + @"Scenes\";

		private const float SymbolSizeX = 3.32f;
		private const float SymbolSizeY = 2.80f;
		private const float ReelGap = 3.48f;

		private void CreateGameObjects()
		{
			// Load the startup scene and update it.

			var startupScene = EditorSceneManager.OpenScene(StartupScenePath);

			EditorSceneManager.SaveScene(startupScene);

			var gameScene = EditorSceneManager.OpenScene(GameScenePath);
			var dg = FindObjectsOfType(typeof(GameObject)).Single(o => o.name == "DefaultGame");
			DestroyImmediate(dg);
			var stageRoot = (GameObject)FindObjectsOfType(typeof(GameObject)).Single(o => o.name == "Stages");
			var allSceneRoots = FindObjectsOfType<SceneRoot>(true);

			var coreSceneRoots = new List<SceneRoot>
			{
				allSceneRoots.Single(sr => sr.gameObject.name == "Cameras"),
				allSceneRoots.Single(sr => sr.gameObject.name == "GiMain"),
				allSceneRoots.Single(sr => sr.gameObject.name == "GiDpp"),
				allSceneRoots.Single(sr => sr.gameObject.name == "GiCommon")
			};

			var gameSceneRoots = new List<SceneRoot>
			{
				allSceneRoots.Single(sr => sr.gameObject.name == "Common")
			};

			var progressiveMeters = CreateProgressives(stageRoot);
			if (progressiveMeters)
				gameSceneRoots.Add(progressiveMeters);

			ConfigureUtility();

			var scenes = new List<(GleGameData.GleStageData stageData, string stageNameNoSpace, SceneRoot sceneRoot)>();
			var stageIndex = 0;
			var symbolList = CreateSymbolList();

			foreach (var gleStage in GleGameData.Stages)
			{
				var stageNameNoSpaces = gleStage.Name.Replace(" ", "");
				var stageGo = new GameObject(gleStage.Name);
				stageGo.transform.SetParent(stageRoot.transform, false);
				var stageActivator = stageGo.AddComponent<StageActivator>();
				var stage = new Stage(Stages.GameSpecificStartId + stageIndex, stageNameNoSpaces);
				var stageContent = new GameObject(gleStage.Name + " Content");
				var stageContentSceneRoot = stageContent.AddComponent<SceneRoot>();
				stageContent.transform.SetParent(stageGo.transform, false);
				CreateStageContent(stageContent, gleStage, symbolList);
				CreateGameMessage(stageContent, gleStage);
				if (!gleStage.IsEntryStage)
					CreateMusic(stageContent, stage, gleStage);
				CreateStagePrompts(stageGo, gleStage);
				stageGo.SetLayerRecursively(LayerMask.NameToLayer("MainGame"));

				var stageSceneRoots = new List<SceneRoot>();
				if (!gleStage.IsEntryStage)
					CreateFeatureDpp(stageGo, gleStage, GleGameData.Stages, stageSceneRoots);

				stageSceneRoots.Add(stageContentSceneRoot);
				stageActivator.ConfigureForMakeGame(stage, coreSceneRoots.Concat(gameSceneRoots).Concat(stageSceneRoots).ToArray());
				if (gleStage.IsEntryStage)
				{
					var gambleActivator = stageGo.AddComponent<StageActivator>();
					gambleActivator.ConfigureForMakeGame(Stages.Gamble, gameSceneRoots.Concat(stageSceneRoots).ToArray());
				}

				PrefabUtility.SaveAsPrefabAssetAndConnect(stageGo, $"{StageAssetsPath}{stageNameNoSpaces}/{gleStage.Name}.prefab", InteractionMode.AutomatedAction);

				scenes.Add((gleStage, stageNameNoSpaces, stageContentSceneRoot));

				stageIndex++;
			}

			var transitionsGo = new GameObject("Transitions");
			transitionsGo.transform.SetParent(stageRoot.transform, false);

			// Add transitions

			foreach (var scene in scenes)
			{
				foreach (var otherScene in scenes)
				{
					if (otherScene.stageData.Name == scene.stageData.Name)
						continue;

					var transitionName = scene.stageNameNoSpace + "To" + otherScene.stageNameNoSpace;

					BuildTransition(transitionName, $"Game/{transitionName}", "ShowStage",
						(otherScene.sceneRoot.gameObject.SetActive, true), (scene.sceneRoot.gameObject.SetActive, false));
				}
			}

			transitionsGo.SetLayerRecursively(LayerMask.NameToLayer("MainGame"));
			PrefabUtility.SaveAsPrefabAssetAndConnect(transitionsGo, $"{StageAssetsPath}Common/Transitions.prefab", InteractionMode.AutomatedAction);

			EditorSceneManager.SaveScene(gameScene);

			void BuildTransition(string goName, string seqName, string eventName, params (UnityAction<bool> Action, bool Value)[] boolActions)
			{
				var stageTransitionGo = new GameObject(goName);
				stageTransitionGo.transform.SetParent(transitionsGo.transform);
				var sequenceUnityEvents = stageTransitionGo.AddComponent<SequenceUnityEvents>();
				var sequenceEvent = new SequenceEvent(seqName, eventName);
				sequenceUnityEvents.ConfigureForMakeGame(sequenceEvent, boolActions);
			}
		}

		private ReelSymbolList CreateSymbolList()
		{
			var sb = CreateInstance<DefaultSymbolBuilder>();
			var symbolPrefab = AssetDatabase.LoadAssetAtPath<ReelSymbol>(SymbolPrefabPath);
			var fallback = AssetDatabase.LoadAssetAtPath<FallbackSymbolOverlay>(FallbackSymbolPath);
			var symbolPath = Path.Combine(StagesCommonPath, "Symbols");

			if (!Directory.Exists(symbolPath))
				Directory.CreateDirectory(symbolPath);

			var symbolAssets = CopySymbolGraphics(symbolPath);
			sb.ConfigureForMakeGame(symbolPrefab, symbolAssets, fallback);

			AssetCreator.CreateAsset(sb, symbolPath + @"\DefaultSymbolList.asset");
			EditorUtility.SetDirty(sb);

			var commonPrefab = PrefabUtility.LoadPrefabContents(CommonPrefabPath);
			var symListGameObject = new GameObject("SymbolList");
			var symList = symListGameObject.AddComponent<SymbolBuilderReelSymbolList>();
			symList.ConfigureForMakeGame(AssetDatabase.LoadAssetAtPath<ReelSymbolBuilder>(CashOnReelsSymbolPath), sb);
			symListGameObject.transform.SetParent(commonPrefab.transform, false);
			symList.gameObject.SetLayerRecursively(commonPrefab.layer);
			PrefabUtility.SaveAsPrefabAsset(commonPrefab, CommonPrefabPath);
			PrefabUtility.UnloadPrefabContents(commonPrefab);

			return FindObjectOfType<ReelSymbolList>(true);
		}

		private void ConfigureUtility()
		{
			var newSymbolWindows = new List<UtilitySymbolWindow>();

			foreach (var sc in stageConfigurations)
				newSymbolWindows.AddRange(sc.Value.reelConfigs.Select(rc => new UtilitySymbolWindow(sc.Key, "", rc.symbolWindowResultName)));

			var utilitySymbolWindows = AssetDatabase.LoadAssetAtPath<UtilitySymbolWindows>(UtilitySettingsPath);
			utilitySymbolWindows.ConfigureForMakeGame(newSymbolWindows);

			EditorUtility.SetDirty(utilitySymbolWindows);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		private SceneRoot CreateProgressives(GameObject stagesRoot)
		{
			// This gets the first set of progressives in the list. Should be fine in 99.9% of cases.

			var progs = GleGameData.Progressives.GetProgressiveLevels(Array.Empty<Input>());

			if (progs.Count == 0)
				return null;

			var progGo = new GameObject("Progressives");
			progGo.transform.SetParent(stagesRoot.transform, false);
			var sr = progGo.AddComponent<SceneRoot>();

			var progMetersGo = new GameObject("Meters");
			progMetersGo.transform.SetParent(progGo.transform, false);
			progMetersGo.transform.localPosition = new Vector2(0, 16);

			var awardGo = new GameObject("AwardBanners");
			awardGo.transform.SetParent(progGo.transform, false);
			var sortingGroup = awardGo.AddComponent<SortingGroup>();
			sortingGroup.sortingOrder = 30;

			var ySize = 10.8f / (progs.Count + 1);
			var yPos = 10.8f / 2 - ySize;
			var meterSize = new Vector2(10, ySize / 3f);
			var whiteSprite = AssetDatabase.LoadAssetAtPath<Sprite>(WhiteMaskPath);
			if (whiteSprite == null)
				Debug.LogWarning("Could not find stencil mask sprite...");

			foreach (var prog in progs)
			{
				var meterRootGo = new GameObject(prog.LevelName + "Meter");
				meterRootGo.AddComponent<SortingGroup>();
				meterRootGo.transform.SetParent(progMetersGo.transform, false);
				meterRootGo.transform.localPosition = new Vector2(0, yPos);
				yPos -= ySize;

				var meterGo = new GameObject("Meter");
				meterGo.transform.SetParent(meterRootGo.transform, false);
				meterGo.AddComponent<RollingMoneyMeter>().ConfigureForMakeGame(new Vector2(10, ySize / 3f), true, progGlyphFactory);
				meterGo.AddComponent<ProgressiveDisplay>().ConfigureForMakeGame(prog.Identifier);
				SpriteMaskHelper.CreateMask(meterSize, whiteSprite, meterRootGo.transform, "Mask", Vector3.zero);

				// Construct and configure progressive award

				var progAward = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ProgressiveAwardPrefabPath));
				progAward.transform.SetParent(awardGo.transform, false);
				progAward.name = $"{prog.LevelName}Award";

				var showObj = progAward.transform.Find("Show");
				var sa = showObj.GetComponent<SequenceActivator>();
				var progAwardSeq = $"ProgressiveAward/{prog.Identifier}Award";

				sa.ConfigureForMakeGame(progAwardSeq, ProgressiveAwardSequenceIds.ShowFanfare, ProgressiveAwardSequenceIds.ShowFanfare);
				var ss = showObj.GetComponent<SequenceSkippable>();
				ss.ConfigureForMakeGame(progAwardSeq, ProgressiveAwardSequenceIds.ShowFanfare, ProgressiveAwardSequenceIds.ShowFanfare);

				var hideObj = progAward.transform.Find("Hide");
				sa = hideObj.GetComponent<SequenceActivator>();
				sa.ConfigureForMakeGame(progAwardSeq, ProgressiveAwardSequenceIds.FanfareComplete, ProgressiveAwardSequenceIds.FanfareComplete);
			}

			progGo.SetLayerRecursively(LayerMask.NameToLayer("MainGame"));
			PrefabUtility.SaveAsPrefabAssetAndConnect(progGo, $"{StageAssetsPath}Common/Progressives.prefab", InteractionMode.AutomatedAction);

			return sr;
		}

		private void CreateStageContent(GameObject contentRoot, GleGameData.GleStageData stageData, ReelSymbolList symbolList)
		{
			// If there is no populator selected for win cycle, then there are no reels enabled.

			var stageConfig = stageConfigurations[stageData.Name];
			var stageNameNoSpace = stageData.Name.Replace(" ", "");

			if (stageConfig.selectedPopulator != null)
			{
				var shakeInst = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ReelShakePrefabPath));
				shakeInst.transform.SetParent(contentRoot.transform, true);

				var reelsParent = new GameObject("Reels and Paylines");
				foreach (var shaker in shakeInst.GetComponentsInChildren<ScreenShake>())
					shaker.ConfigureForMakeGame(new[] { reelsParent.transform });

				reelsParent.transform.SetParent(contentRoot.transform, false);
				reelsParent.transform.SetLocalPosY(0.3f);
				var stageStatePath = Path.Combine(StageAssetsPath, stageNameNoSpace);
				var stage = (Stage)typeof(GameStages).GetProperty(stageNameNoSpace, BindingFlags.Static | BindingFlags.Public)?.GetValue(null);

				CreateReels(stageData, stageConfig, reelsParent, symbolList, stage, stageStatePath, stageNameNoSpace);

				switch (stageConfig.paylineStyle)
				{
					case PaylineStyle.LineAndBoxMesh:
						CreateMeshPaylines(stageData, reelsParent, Path.Combine(stageStatePath, stageNameNoSpace + "Paylines.asset"));
						break;

					case PaylineStyle.Beans:
						CreateBeanPaylines(reelsParent);
						break;
				}

				CreateCounters(stageData, stageConfig, contentRoot);

				if (stageData.IsEntryStage)
				{
					var slideAndHide = reelsParent.AddComponent<SlideAndHide>();
					slideAndHide.ConfigureForMakeGame(reelsParent, new Vector3(0, -13, 0), 0.25f);
					var gameStageSceneInit = reelsParent.AddComponent<GameStageSceneInit>();
					gameStageSceneInit.ConfigureForMakeGame(new[] { Stages.Gamble }, (slideAndHide.Hide, true), (slideAndHide.Show, true));
					var propertyRef = contentRoot.AddComponent<PropertyRefBoolUnityEvent>();
					propertyRef.ConfigureForMakeGame(SlideInPropPath, (slideAndHide.Show, false), (slideAndHide.Hide, false));
				}
			}
		}

		private void CreateStagePrompts(GameObject stageGo, GleGameData.GleStageData stageData)
		{
			// Add a start feature if required

			var stageConfig = stageConfigurations[stageData.Name];

			switch (stageConfig.promptStyle)
			{
				case PromptStyle.WaitStartFeature:
					CreateStartFeature(stageGo, stageData);
					break;
				case PromptStyle.WaitPerCycle:
					CreateRespinPrompt(stageGo, stageData);
					break;
			}
		}

		private void CreateFeatureDpp(GameObject stageGo, GleGameData.GleStageData stageData, IReadOnlyList<GleGameData.GleStageData> stages, ICollection<SceneRoot> sceneRoots)
		{
			var stageNameNoSpace = stageData.Name.Replace(" ", "");

			var goName = $"{stageNameNoSpace}ButtonPanelContent";
			var gameMessageInst = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ButtonPanelContentPrefabPath));
			gameMessageInst.transform.SetParent(stageGo.transform, true);
			gameMessageInst.transform.SetLocalPosY(-30f);
			gameMessageInst.name = goName;
			var sr = gameMessageInst.GetComponentInChildren<SceneRoot>();
			var buttonPanelGo = gameMessageInst.transform.Find("ButtonPanel").gameObject;

			var showStageEvent = "ShowStage";
			var waitForStartFeatureEvent = "WaitForStartFeature";

			// Add in a sequence unity event to enable the feature dpp content during the transition to the new stage.
			// If the prompt style is PromptStyle.WaitStartFeature and the transition is occuring from the entry stage then with the wait for start feature event id.
			// Otherwise, use the show stage event id.
			foreach (var otherStage in stages)
			{
				if (otherStage.Name == stageData.Name)
					continue;

				var stageConfig = stageConfigurations[stageData.Name];

				var seqName = $"Game/{otherStage.Name.Replace(" ", "") + "To" + stageData.Name.Replace(" ", "")}";

				var sue = gameMessageInst.AddComponent<SequenceUnityEvents>();
				var sequenceEvent = new SequenceEvent(seqName, stageConfig.promptStyle == PromptStyle.WaitStartFeature && otherStage.IsEntryStage ? waitForStartFeatureEvent : showStageEvent);
				sue.ConfigureForMakeGame(sequenceEvent, (buttonPanelGo.SetActive, true));

				sue = gameMessageInst.AddComponent<SequenceUnityEvents>();
				seqName = $"Game/{stageData.Name.Replace(" ", "") + "To" + otherStage.Name.Replace(" ", "")}";
				sequenceEvent = new SequenceEvent(seqName, showStageEvent);
				sue.ConfigureForMakeGame(sequenceEvent, (buttonPanelGo.SetActive, false));
			}

			sceneRoots.Add(sr);
			gameMessageInst.SetLayerRecursively(LayerMask.NameToLayer("MainGameDpp"));
		}

		private static void CreateGameMessage(GameObject stageGo, GleGameData.GleStageData stageData)
		{
			var stageNameNoSpace = stageData.Name.Replace(" ", "");

			var goName = $"{stageNameNoSpace}GameMessages";
			var gameMessageInst = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(GameMessagesPrefabPath));
			gameMessageInst.transform.SetParent(stageGo.transform, true);
			gameMessageInst.transform.SetLocalPosX(5.5f);
			gameMessageInst.transform.SetLocalPosY(-4.3f);
			gameMessageInst.name = goName;

			var c = gameMessageInst.GetComponents<PropertyRefBool>();
			c[0].ConfigureForMakeGame($"{nameof(StatusDatabase)}.{stageData.Name}GameMessageStatus.IsGameInfoVisible");
			c[1].ConfigureForMakeGame($"{nameof(StatusDatabase)}.{stageData.Name}GameMessageStatus.IsWinInfoVisible");

			var child = gameMessageInst.transform.Find("GamePlayMessages");
			var gpmp = child.GetComponent<GamePlayMessageProvider>();

			gpmp.ConfigureForMakeGame($"{nameof(StatusDatabase)}.{stageData.Name}GameMessageStatus.ActiveGameInfoMessageIndex");

			child = gameMessageInst.transform.Find("WinInfoMessages");
			var wimp = child.GetComponent<WinInfoMessageProvider>();
			var stageStatePath = Path.Combine(StageAssetsPath, stageNameNoSpace);

			var allSymbols = GameDataFactory.GetAllSymbols(stageData.Name).ToList();
			var symbolPath = Path.Combine(stageStatePath, "Symbols");

			var assets = new List<(string, Sprite)>();
			foreach (var symbol in allSymbols)
			{
				var assetSymbolLocation = Path.Combine(symbolPath, symbol + ".png");

				var sprite = AssetDatabase.LoadAssetAtPath(assetSymbolLocation, typeof(Sprite)) as Sprite;
				assets.Add((symbol, sprite));
			}

			wimp.ConfigureForMakeGame(assets);
		}

		private static void CreateMusic(GameObject stageGo, Stage stage, GleGameData.GleStageData stageData)
		{
			var stageNameNoSpace = stageData.Name.Replace(" ", "");

			var gameMessageInst = new GameObject($"{stageNameNoSpace}Music");
			gameMessageInst.transform.SetParent(stageGo.transform, true);

			var c = gameMessageInst.AddComponent<StandardFeatureMusicPlayer>();
			c.ConfigureForMakeGame(stage);
		}

		private static void CreateStartFeature(GameObject stageGo, GleGameData.GleStageData stageData)
		{
			var stageNameNoSpace = stageData.Name.Replace(" ", "");

			foreach (var connection in GleGameData.StageConnections.Where(c => c.InitialStage != stageData && c.FinalStage == stageData))
			{
				var transName = $"{connection.InitialStage.Name.Replace(" ", "")}To{stageNameNoSpace}";
				var seqName = $"Game/{transName}";
				var startFeatureInst = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(StartFeaturePrefabPath));
				startFeatureInst.transform.SetParent(stageGo.transform, true);
				startFeatureInst.name = transName;

				// TODO: This may break if free games isn't the first stage!

				foreach (var ipa in startFeatureInst.GetComponentsInChildren<InterruptablePresentationActivator>(true))
				{
					switch (ipa.gameObject.name)
					{
						case "BannerIntro":
							ipa.ConfigureForMakeGame(new SequenceEvent(seqName, "ShowBanner"));
							break;

						case "BannerOutro":
							ipa.ConfigureForMakeGame(new SequenceEvent(seqName, "HideBanner"));
							break;
					}
				}

				var wait = startFeatureInst.GetComponentInChildren<SequenceWait>(true);
				wait.ConfigureForMakeGame(new SequenceEventPair(seqName, "WaitForStartFeature", "WaitForStartFeature"));
			}
		}

		private static void CreateRespinPrompt(GameObject stageGo, GleGameData.GleStageData stageData)
		{
			var stageNameNoSpace = stageData.Name.Replace(" ", "");

			var goName = $"{stageNameNoSpace}PromptBanner";
			var seqName = $"{stageNameNoSpace}/Prompt";
			var startFeatureInst = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PromptPerCyclePrefabPath));
			startFeatureInst.transform.SetParent(stageGo.transform, true);
			startFeatureInst.name = goName;

			foreach (var ipa in startFeatureInst.GetComponentsInChildren<InterruptablePresentationActivator>(true))
			{
				switch (ipa.gameObject.name)
				{
					case "BannerIntro":
						ipa.ConfigureForMakeGame(new SequenceEvent(seqName, "ShowBanner"));
						break;

					case "BannerOutro":
						ipa.ConfigureForMakeGame(new SequenceEvent(seqName, "HideBanner"));
						break;
				}
			}

			var wait = startFeatureInst.GetComponentInChildren<SequenceWait>(true);
			wait.ConfigureForMakeGame(new SequenceEventPair(seqName, nameof(PerCyclePromptSequenceIds.WaitStartFeature), nameof(PerCyclePromptSequenceIds.WaitStartFeature)));
		}

		private IReadOnlyList<Sprite> CopySymbolGraphics(string symbolPath)
		{
			var allSymbols = GleGameData.Stages.SelectMany(s => GameDataFactory.GetAllSymbols(s.Name)).Distinct().ToList();
			allSymbols.Sort(StringComparer.InvariantCultureIgnoreCase);

			var assetLocations = new List<string>();

			foreach (var symbol in allSymbols)
			{
				var playWindowSymbolLocation = Path.Combine(imageFolder, symbol + ".png");
				if (!File.Exists(playWindowSymbolLocation))
					continue;

				var assetSymbolLocation = Path.Combine(symbolPath, symbol + ".png");
				if (!File.Exists(assetSymbolLocation))
					File.Copy(playWindowSymbolLocation, assetSymbolLocation);

				assetLocations.Add(assetSymbolLocation.Replace("\\", "/"));
			}

			AssetDatabase.Refresh();

			return assetLocations.Select(AssetDatabase.LoadAssetAtPath<Sprite>).ToArray();
		}

		private void CreateReels(GleGameData.GleStageData stageData, StageConfiguration stageConfig, GameObject reelsParent, ReelSymbolList symbolList, Stage stage, string stagePath, string stageNameNoSpace)
		{
			var allReelData = new List<ReelData>();

			var reelPrefabObject = new GameObject();
			var reelPrefab = reelPrefabObject.AddComponent<Reel>();
			var reelInfoEntries = stageData.GetReelInfoEntries().ToList();

			var whiteSprite = AssetDatabase.LoadAssetAtPath<Sprite>(WhiteMaskPath);
			if (whiteSprite == null)
				Debug.LogWarning("Could not find stencil mask sprite...");

			var spinEvent = CreateInstance<ReelSpinStateEvent>();
			AssetCreator.CreateAsset(spinEvent, Path.Combine(stagePath, stageNameNoSpace + "SpinEvent.asset"));

			// Use the first reel config to determine the spin timings. By default, use the 5x3 spin timings.
			var spinTiming = AssetDatabase.LoadAssetAtPath<ReelSpinTimings>(Path.Combine(SpinTimingsPath, @"5x3\5x3SpinTimings.asset"));
			if (stageConfig.reelConfigs.Count > 0)
			{
				var rc = stageConfig.reelConfigs.First();

				var reelInfo = reelInfoEntries.Single(i => i.Name == rc.symbolWindowStructureName);
				var spinTimingName = $@"{reelInfo.ColumnCount}x{reelInfo.ColumnVisibleSymbols.Max()}\{reelInfo.ColumnCount}x{reelInfo.ColumnVisibleSymbols.Max()}SpinTimings.asset";
				var st = AssetDatabase.LoadAssetAtPath<ReelSpinTimings>(Path.Combine(SpinTimingsPath, spinTimingName));

				if (st != null)
					spinTiming = st;
			}

			var preShowRc = default(ReelContainer);
			foreach (var reelConfig in stageConfig.reelConfigs)
			{
				if (reelConfig.symbolWindowStructureName == null)
					continue;

				var reelInfo = reelInfoEntries.Single(i => i.Name == reelConfig.symbolWindowStructureName);

				var reelsObject = ReelContainerCreator.BuildReelsWithSpriteMask(reelInfo, reelPrefab, ReelGap, 1, 1, new Vector2(SymbolSizeX, SymbolSizeY), whiteSprite);
				preShowRc ??= reelsObject.GetComponentInChildren<ReelContainer>();
				reelsObject.transform.SetParent(reelsParent.transform, false);

				var inheritedResults = new List<(Stage Stage, string ResultName)>();

				if (reelConfig.inheritPrevResult)
				{
					if (stageData.IsEntryStage)
					{
						// Base game stage has no connections to it, assume here that all stages can go back to it (except itself).

						foreach (var sourceStageConfig in stageConfigurations.Where(kvp => kvp.Key.Replace(" ", "") != stage.Name))
						{
							ConfigureInheritedReel(sourceStageConfig.Value, sourceStageConfig.Key.Replace(" ", ""), inheritedResults);
						}
					}
					else
					{
						foreach (var conn in GleGameData.StageConnections.Where(c => c.FinalStage.Name.Replace(" ", "") == stage.Name && c.InitialStage.Name.Replace(" ", "") != stage.Name))
						{
							var sourceStageConfig = stageConfigurations[conn.InitialStage.Name];
							ConfigureInheritedReel(sourceStageConfig, conn.InitialStage.Name.Replace(" ", ""), inheritedResults);
						}
					}
				}

				var suffix = stageConfig.reelConfigs.Count > 1 ? reelConfig.symbolWindowResultName : string.Empty;
				var reelDataPath = Path.Combine(stagePath, stageNameNoSpace + $"ReelData{suffix}.asset");
				allReelData.Add(new ReelData(GleDefaultReelDataProvider.CreateForMakeGame(reelDataPath, reelConfig.symbolWindowResultName, inheritedResults), symbolList, reelsObject.GetComponent<ReelContainer>(), spinEvent));
				var sa = reelsObject.AddComponent<AllWinningSymbolAnimator>();
				sa.ConfigureForMakeGame(WinsStatusProp);
			}

			var reelController = reelsParent.AddComponent<ReelController>();
			var reelSpin = reelsParent.AddComponent<SuspenseReelSpin>();
			var stopSounds = reelsParent.AddComponent<ReelSoundController>();
			var slamSpin = reelsParent.AddComponent<ReelSlamSpin>();
			var activator = reelsParent.AddComponent<SequenceActivator>();
			reelController.ConfigureForMakeGame(stage, allReelData, reelSpin);
			reelSpin.ConfigureForMakeGame(spinTiming, 2.2f);
			stopSounds.ConfigureForMakeGame(spinEvent);
			slamSpin.ConfigureForMakeGame(spinEvent);
			activator.ConfigureForMakeGame($"{stage.Name}/Play", GamePlaySequenceIds.Play, GamePlaySequenceIds.Play, reelController);

			if (preShowRc)
			{
				var bell = FindObjectsOfType<SoundPlayer>(true).Single(o => o.name == "BellIntensity1");
				var preShowGo = new GameObject("PreShowWin");
				preShowGo.transform.SetParent(reelsParent.transform, true);
				var darkener = preShowGo.AddComponent<ReelPreShowDarkener>();
				darkener.ConfigureForMakeGame(bell, preShowRc, "StatusDatabase.GameSpecificStatus.PreShowWinHighlight");
				var sa = preShowGo.AddComponent<SequenceActivator>();
				sa.ConfigureForMakeGame($"{stageNameNoSpace}/Trigger", PreShowWinSequenceIds.HighlightTrigger, PreShowWinSequenceIds.HighlightTrigger, darkener);
			}

			DestroyImmediate(reelPrefabObject);

			void ConfigureInheritedReel(StageConfiguration sourceStageConfig, string initStageName, ICollection<(Stage Stage, string ResultName)> inheritedResults)
			{
				var sourceReel = sourceStageConfig.reelConfigs.FirstOrDefault(r => r.symbolWindowStructureName != null);

				if (sourceReel != null)
				{
					var sourceStage = (Stage)typeof(GameStages).GetProperty(initStageName, BindingFlags.Static | BindingFlags.Public)?.GetValue(null);
					inheritedResults.Add((sourceStage, sourceReel.symbolWindowResultName));
				}
			}
		}

		private static void CreateBeanPaylines(GameObject reelsParent)
		{
			var container = reelsParent.GetComponentInChildren<ReelContainer>();

			var paylineGo = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BeanPaylinesPrefabPath));
			paylineGo.transform.SetParent(reelsParent.transform, false);
			var paylineContainer = paylineGo.GetComponent<BeanPaylineContainer>();
			paylineContainer.ConfigureForMakeGame(container);
		}

		private void CreateMeshPaylines(GleGameData.GleStageData stageData, GameObject reelsParent, string paylinesPath)
		{
			var container = reelsParent.GetComponentInChildren<ReelContainer>();
			var paylineGo = new GameObject("Payline Container");
			paylineGo.transform.SetParent(reelsParent.transform, false);
			var paylineContainer = paylineGo.AddComponent<MeshPaylineContainer>();
			var paylines = CreateInstance<StandardPaylines>();
			AssetDatabase.CreateAsset(paylines, paylinesPath);

			if (stageConfigurations[stageData.Name].selectedPatterns != null)
			{
				// The line patterns may be null. In that case the payline creator will just make boxes.

				var linePatterns = stageData.GetLinePatternInfo().SingleOrDefault(p => p.Name == stageConfigurations[stageData.Name].selectedPatterns);
				paylines.PatternFieldName = linePatterns?.Name;
				paylines.Settings = (StandardPaylinesSettings)AssetDatabase.LoadAssetAtPath(PaylineSettingsPath, typeof(StandardPaylinesSettings));
				StandardPaylinesEditor.CreatePaylines(paylines, linePatterns, container);
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			paylineContainer.ConfigureForMakeGame(
				paylines,
				(ColorSelector)AssetDatabase.LoadAssetAtPath(PaylineColorPath, typeof(ColorSelector)),
				new CustomSortingLayer(SortingLayer.GetLayerValueFromName("Default"), 20),
				new CustomSortingLayer(SortingLayer.GetLayerValueFromName("Default"), 19));

			var paylineHighlighter = paylineGo.AddComponent<PaylineHighlighter>();
			paylineHighlighter.ConfigureForMakeGame(CurrentWinStatusProp);
		}

		private static void CreateCounters(GleGameData.GleStageData stageData, StageConfiguration stageConfig, GameObject parent)
		{
			switch (stageConfig.counterStyle)
			{
				case CounterStyle.XOfY:
				{
					var counter = parent.InstantiatePreFabAsChild(AssetDatabase.LoadAssetAtPath<GameObject>(CycleXOfYPrefabPath));
					counter.name = $"{stageData.Name}XOfYCounter";
					counter.transform.SetLocalPosY(5);
					var propRef = counter.GetComponent<PropertyRefBoolUnityEvent>();
					var child = counter.transform.GetChild(0).gameObject;
					propRef.ConfigureForMakeGame($"StatusDatabase.{stageData.Name}XOfYStatus.IsActive", (child.SetActive, true), (child.SetActive, false));

					var textPropRef = child.GetComponent<PropertyRefTextFormatter>();
					textPropRef.ConfigureForMakeGame(new[]
					{
						$"StatusDatabase.{stageData.Name}XOfYStatus.CyclesCurrent",
						$"StatusDatabase.{stageData.Name}XOfYStatus.CyclesTotal"
					});

					break;
				}

				case CounterStyle.Remaining:
				{
					var counter = parent.InstantiatePreFabAsChild(AssetDatabase.LoadAssetAtPath<GameObject>(CycleRemainingPrefabPath));
					counter.name = $"{stageData.Name}RemainingCounter";
					counter.transform.SetLocalPosY(5);

					var plural = counter.transform.GetChild(0).gameObject;
					var singular = counter.transform.GetChild(1).gameObject;
					var last = counter.transform.GetChild(2).gameObject;

					// Configure the text formatter objects.
					plural.GetComponent<PropertyRefTextFormatter>().ConfigureForMakeGame(new[] { $"StatusDatabase.{stageData.Name}RemainingStatus.CyclesRemaining" });
					singular.GetComponent<PropertyRefTextFormatter>().ConfigureForMakeGame(new[] { $"StatusDatabase.{stageData.Name}RemainingStatus.CyclesRemaining" });

					var t = Type.GetType($"Game.{stageData.Name}RemainingStatePropUnityEvents, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
					var enumType = Type.GetType($"Game.{stageData.Name}RemainingState, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

					dynamic propRef = counter.AddComponent(t);

					// Configure the enum property reference to control the child objects.
					var p = Enum.Parse(enumType, nameof(CyclesRemainingState.Plural));
					var s = (Enum)Enum.Parse(enumType, nameof(CyclesRemainingState.Singular));
					var l = (Enum)Enum.Parse(enumType, nameof(CyclesRemainingState.LastGame));

					var mappings = new List<(object key, IReadOnlyList<(UnityAction<bool> Action, bool Value)>)>
					{
						(p, new List<(UnityAction<bool> Action, bool Value)> { (plural.SetActive, true), (singular.SetActive, false), (last.SetActive, false) }),
						(s, new List<(UnityAction<bool> Action, bool Value)> { (plural.SetActive, false), (singular.SetActive, true), (last.SetActive, false) }),
						(l, new List<(UnityAction<bool> Action, bool Value)> { (plural.SetActive, false), (singular.SetActive, false), (last.SetActive, true) })
					};
					var defaultMappings = new List<(UnityAction<bool> Action, bool Value)> { (plural.SetActive, false), (singular.SetActive, false), (last.SetActive, false) };
					propRef.ConfigureForMakeGame($"StatusDatabase.{stageData.Name}RemainingStatus.CurrentState", mappings, defaultMappings);
					break;
				}
				default:
				case CounterStyle.None:
					break;
			}
		}
	}
}