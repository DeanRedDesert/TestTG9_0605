using System.Collections.Generic;
using Game;
using Game.Stages.Common.PreShow;
//NAMESPACES
using Game.WinPresentation;
using Midas.Gle.Presentation.SmartSymbols;
using Midas.Presentation.Game;
using Midas.Presentation.Reels.SmartSymbols;
using Midas.Presentation.Sequencing;
using Midas.Presentation.StageHandling;

namespace Midas.Gle.MakeGame.Editor.ScriptTemplates
{
	public static class GameConfigurator
	{
		public static Stage BaseGameStage /*BASESTAGE*/;

		public static IReadOnlyList<(string StageId, Stage Stage)> CreateStageMappings()
		{
			return new (string, Stage)[]
			{
//STAGEMAP
			};
		}

		private static bool CheckTrigger()
		{
			var prize = PreShowHelper.FindPrize("PRIZENAME");
			GameSpecificController.Data.PreShowWinHighlight = prize?.WinningPositions;
			return prize != null;
		}

		public static IReadOnlyList<IPresentationNode> CreateNodes()
		{
//NODESCONFIG

			return new IPresentationNode[]
			{
//NODES
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
//SEQUENCES
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
		/// private static readonly string[] freeGameSymbols = { "SCAT1" };
		/// 
		/// private static readonly SmartSymbolDetector[] freeGameDetectors =
		/// {
		/// 	new ScatterAnySmartSymbolDetector(GameStages.Base, "SymbolWindow", freeGameSymbols, 3, true, false),
		/// 	new ScatterAnySmartSymbolDetector(GameStages.FreeGames, "SymbolWindow", freeGameSymbols, 3, true, false),
		/// };
		/// 
		/// private static readonly string[] respinSymbols = { "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "C10", "C11", "C12", "MINI", "MINOR", "MAJOR", "GRAND", };
		/// 
		/// private static readonly SmartSymbolDetector[] respinDetectors =
		/// {
		/// 	new StackedScatterAnySmartSymbolDetector(GameStages.Base, "SymbolWindow", respinSymbols, 5, false, false),
		/// 	new StackedScatterAnySmartSymbolDetector(GameStages.FreeGames, "SymbolWindow", respinSymbols, 5, false, false),
		/// 	new LockedSymbolSmartSymbolDetector(GameStages.Respin, "LockedSymbolWindow", "RespinState")
		/// };
		/// 
		/// </code>
		/// </remarks>
		public static IReadOnlyList<IPresentationController> CreatePresentationControllers()
		{
			var smartSymbolController = new SmartSymbolController();

			// TODO: Add the smart symbol detectors your game requires here.
			// Each one needs to have a unique name and will create a status database block for you to get your smart anim and anticipation data from.
			// Example:
			// smartSymbolController.AddSmartSymbolDetectors("FreeGames", freeGameDetectors);
			// smartSymbolController.AddSmartSymbolDetectors("Respins", respinDetectors);

//CONTROLLERSCONFIG

			return new IPresentationController[]
			{
				smartSymbolController,
//CONTROLLERS
			};
		}

		public static string GameName => "//TEMPLATE GAME";
	}
}