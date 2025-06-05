using System;
using System.Collections.Generic;
using System.Linq;
using Game.WinPresentation;
using Logic.Core.Engine;
using Midas.Gle.LogicToPresentation;
using Midas.Presentation.Game;
using Midas.Presentation.Progressives;
using Midas.Presentation.Sequencing;
using Midas.Presentation.StageHandling;

namespace Game
{
	public static class GameConfigurator
	{
		public static Stage BaseGameStage => GameStages.Default;

		public static IReadOnlyList<(string StageId, Stage Stage)> CreateStageMappings()
		{
			return GleGameData.Stages.Select(s => (s.Name, GameStages.Default)).ToArray();
		}

		public static IReadOnlyList<IPresentationNode> CreateNodes()
		{
			return new IPresentationNode[]
			{
				new SimpleGameNode("DefaultGame", GameStages.Default),
				new MainWinPresNode("DefaultWinPres", GameStages.Default)
			};
		}

		public static IReadOnlyList<(Stage Initial, Stage Final, Sequence Sequence)> CreateStageTransitionSequences()
		{
			return new (Stage Initial, Stage Final, Sequence Sequence)[]
			{
			};
		}

		public static IReadOnlyList<IPresentationController> CreatePresentationControllers()
		{
			var progAward = new SimpleProgressiveAwardController();

			foreach (var prog in GleGameData.Progressives.GetProgressiveLevels(Array.Empty<Input>()))
				progAward.RegisterProgressiveAwardSequence(prog.Identifier);

			return new IPresentationController[]
			{
				progAward
			};
		}

		public static string GameName => "TEMPLATE GAME";
	}
}