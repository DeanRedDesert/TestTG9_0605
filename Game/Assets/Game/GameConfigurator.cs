﻿using System.Collections.Generic;
using Game;
using Game.Stages.Common.PreShow;
using Game.GameMessages;
using Midas.Presentation.Progressives;
using Game.WinPresentation;
using Midas.Gle.Presentation.SmartSymbols;
using Midas.Presentation.Game;
using Midas.Presentation.Reels.SmartSymbols;
using Midas.Presentation.Sequencing;
using Midas.Presentation.StageHandling;
using Logic.Core.Types;
using Logic.Types;
using Midas.Gle.Presentation;
using Logic.Core.WinCheck;
using System.Linq;

namespace Game
{
	public static class GameConfigurator
	{
		public static Stage BaseGameStage => GameStages.Base;

		public static IReadOnlyList<(string StageId, Stage Stage)> CreateStageMappings()
		{
			return new (string, Stage)[]
			{
				("Base", GameStages.Base),
				("FreeGames", GameStages.FreeGames),
				("Respin", GameStages.Respin),
			};
		}

		private static bool CheckTrigger()
		{
			var prize = PreShowHelper.FindPrize("SCAT1");
			GameSpecificController.Data.PreShowWinHighlight = prize?.WinningPositions;
			return prize != null;
		}

		private static bool CheckRespinTrigger()
		{
			// Get the respin state. In this game, a non-null RespinState result means we have a trigger.
			var result = GleGameController.GleStatus.CurrentGameResult.Current.StageResults.FirstOrDefault(r => r.Value is RespinState);
			if (result?.Value == null)
				return false;

			var respinState = (RespinState)result.Value;

			// Get the symbol window so we can determine the cells that each trigger symbol are in.
			result = GleGameController.GleStatus.CurrentGameResult.Current.StageResults.FirstOrDefault(r => r.Name == "SymbolWindow");
			if (result?.Value == null)
				return false;

			// Extract the cell information from the respin trigger and place it into GameSpecificController.Data.PreShowWinHighlight before returning true
			var symbolWindowStructure = ((SymbolWindowResult)result.Value).SymbolWindowStructure;
			GameSpecificController.Data.PreShowWinHighlight = symbolWindowStructure.IndexesToCells(respinState.Frames.EnumerateIndexes().ToArray()).Select(c => (c.Column, c.Row)).ToList();
			return true;
		}

		public static IReadOnlyList<IPresentationNode> CreateNodes()
		{
			var baseNode = new SimpleGameNode("BaseStage", GameStages.Base);
			baseNode.AddPreShowSequence("Trigger", CheckTrigger);
			baseNode.AddPreShowSequence("RespinTrigger", CheckRespinTrigger);
			var freeGamesNode = new SimpleGameNode("FreeGamesStage", GameStages.FreeGames);
			freeGamesNode.AddPreShowSequence("Trigger", CheckTrigger);
			freeGamesNode.AddPreShowSequence("RespinTrigger", CheckRespinTrigger);
			var respinNode = new SimpleGameNode("RespinStage", GameStages.Respin);
			//respinNode.AddPreShowSequence("Trigger", CheckTrigger); // can remove since there is no Trigger Free Games inside Respin

			return new IPresentationNode[]
			{
				new MainWinPresNode("BaseWinPres", GameStages.Base),
				baseNode,
				new SimpleTransitionNode("BaseTransition", GameStages.Base),
				new MainWinPresNode("FreeGamesWinPres", GameStages.FreeGames),
				freeGamesNode,
				new SimpleTransitionNode("FreeGamesTransition", GameStages.FreeGames),
				new MainWinPresNode("RespinWinPres", GameStages.Respin),
				respinNode,
				new SimpleTransitionNode("RespinTransition", GameStages.Respin),
				PromptPerCycleNode.Create("RespinPrompt", GameStages.Respin),
			};
		}

		private enum StartFeatureSequence
		{
			ShowBanner = CustomEvent.CustomEventStartId,
			WaitForStartFeature,
			HideBanner,
			ShowStage
		}

		private enum ShowStageSequence
		{
			ShowStage = StartFeatureSequence.ShowStage,
		}

		public static IReadOnlyList<(Stage Initial, Stage Final, Sequence Sequence)> CreateStageTransitionSequences()
		{
			return new (Stage Initial, Stage Final, Sequence Sequence)[]
			{
				(GameStages.Base, GameStages.FreeGames, SimpleSequence.Create<StartFeatureSequence>("Game/BaseToFreeGames")),
				(GameStages.Base, GameStages.Respin, SimpleSequence.Create<ShowStageSequence>("Game/BaseToRespin")),
				(GameStages.FreeGames, GameStages.Base, SimpleSequence.Create<ShowStageSequence>("Game/FreeGamesToBase")),
				(GameStages.FreeGames, GameStages.Respin, SimpleSequence.Create<ShowStageSequence>("Game/FreeGamesToRespin")),
				(GameStages.Respin, GameStages.Base, SimpleSequence.Create<ShowStageSequence>("Game/RespinToBase")),
				(GameStages.Respin, GameStages.FreeGames, SimpleSequence.Create<ShowStageSequence>("Game/RespinToFreeGames")),
			};
		}

		/// <summary>
		/// Create the presentation controllers required by your game
		/// </summary>
		/// <remarks>
		/// Here is where you will configure your smart symbol detectors. You will need some structures similar to these:
		/// <code>
		///
		/// // Example smart symbol detector configurations:
		/// 
		private static readonly string[] freeGameSymbols = { "SCAT1" };

		private static readonly SmartSymbolDetector[] freeGameDetectors =
		{
			new ScatterAnySmartSymbolDetector(GameStages.Base, "SymbolWindow", freeGameSymbols, 3, true, false),
			new ScatterAnySmartSymbolDetector(GameStages.FreeGames, "SymbolWindow", freeGameSymbols, 3, true, false),
		};

		private static readonly string[] respinSymbols = { "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "C10", "C11", "C12", "MINI", "MINOR", "MAJOR", "GRAND", };
		
		private static readonly SmartSymbolDetector[] respinDetectors =
		{
			new StackedScatterAnySmartSymbolDetector(GameStages.Base, "SymbolWindow", respinSymbols, 5, false, false),
			new StackedScatterAnySmartSymbolDetector(GameStages.FreeGames, "SymbolWindow", respinSymbols, 5, false, false),
			new LockedSymbolSmartSymbolDetector(GameStages.Respin, "LockedSymbolWindow", "RespinState")
		};
		
		/// </code>
		/// </remarks>
		public static IReadOnlyList<IPresentationController> CreatePresentationControllers()
		{
			var smartSymbolController = new SmartSymbolController();

			// TODO: Add the smart symbol detectors your game requires here.
			// Each one needs to have a unique name and will create a status database block for you to get your smart anim and anticipation data from.
			// Example:
			smartSymbolController.AddSmartSymbolDetectors("FreeGames", freeGameDetectors);
			smartSymbolController.AddSmartSymbolDetectors("Respins", respinDetectors);

			var progAwardCont = new SimpleProgressiveAwardController();
			progAwardCont.RegisterProgressiveAwardSequence("GRAND");
			progAwardCont.RegisterProgressiveAwardSequence("MAJOR");

			return new IPresentationController[]
			{
				new CashChingController(),
				smartSymbolController,
				new FreeGamesXOfYController(GameStages.FreeGames),
				new RespinRemainingController(GameStages.Respin),
				new GameMessageController(GameStages.Base),
				new GameMessageController(GameStages.FreeGames),
				new GameMessageController(GameStages.Respin),
				progAwardCont,
			};
		}

		public static string GameName => "NO NAME";
	}
}