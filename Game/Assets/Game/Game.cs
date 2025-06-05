using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.Core.Configuration;
using Midas.Core.General;
using Midas.CreditPlayoff.Logic;
using Midas.CreditPlayoff.LogicToPresentation;
using Midas.CreditPlayoff.Presentation;
using Midas.Gamble.Logic;
using Midas.Gamble.LogicToPresentation;
using Midas.Gamble.Presentation;
using Midas.Gle.Logic;
using Midas.Gle.LogicToPresentation;
using Midas.Gle.Presentation;
using Midas.Presentation.Cabinet;
using Midas.Presentation.Dashboard;
using Midas.Presentation.Game;
using Midas.Presentation.Sequencing;
using Midas.Presentation.StageHandling;
using MidasStages = Midas.Presentation.StageHandling.Stages;

namespace Game
{
	public sealed class Game : GameBase
	{
		public override string GameName => GameConfigurator.GameName;
		public override Stage BaseGameStage => GameConfigurator.BaseGameStage;

		private static readonly IReadOnlyList<(string StageId, Stage Stage)> stageMappings = GameConfigurator.CreateStageMappings();

		private static readonly IReadOnlyList<(Stage Initial, Stage Final, Sequence Sequence)> gameStageTransitions = GameConfigurator.CreateStageTransitionSequences();

		private static readonly IReadOnlyList<(Stage Initial, Stage Final, Sequence Sequence)> allStageTransitions =
			new (Stage Initial, Stage Final, Sequence Sequence)[]
				{
					(GameConfigurator.BaseGameStage, MidasStages.Gamble, TrumpsController.ShowSequence),
					(MidasStages.Gamble, GameConfigurator.BaseGameStage, TrumpsController.HideSequence)
				}
				.Concat(gameStageTransitions)
				.ToArray();

		#region Types

		private sealed class LogicLoader : ILogicLoader
		{
			private readonly bool doLogicResetOnConfigChange;
			private static readonly (long MinIncl, long MaxExcl)[] denomRanges = { (0, 5), (5, 50), (50, long.MaxValue) };

			public LogicLoader(bool doLogicResetOnConfigChange)
			{
				this.doLogicResetOnConfigChange = doLogicResetOnConfigChange;
			}

			public IGame LoadGame(string gameMountPoint, string paytableFileName, ConfigData config)
			{
				return GlePaytable.GetConfiguredGame(gameMountPoint, config, doLogicResetOnConfigChange);
			}

			public IReadOnlyDictionary<Money, IDenomBetData> GetDenomBetData(string gameMountPoint, ConfigData configData)
			{
				return GlePaytable.GetDenomBetData(configData, GetDenomLevel);

				DenomLevel GetDenomLevel(Money denomVal)
				{
					var minDenom = configData.DenomConfig.AvailableDenominations[0].AsMinorCurrency;
					var scaledDenom = denomVal.AsMinorCurrency / minDenom;

					for (var i = 0; i < denomRanges.Length; i++)
					{
						var range = denomRanges[i];
						if (scaledDenom >= range.MinIncl && scaledDenom < range.MaxExcl)
							return (DenomLevel)i;
					}

					return DenomLevel.High;
				}
			}

			public IGamble LoadGamble(ConfigData config) => new Trumps(config.AncillaryConfig);
			public ICreditPlayoff LoadCreditPlayoff(ConfigData config) => new CreditPlayoff(config.GameConfig.IsCreditPlayoffEnabled);
		}

		#endregion

		public override ILogicLoader GetLogicLoader(bool doLogicResetOnConfigChange)
		{
			GameLog.Instance.Info($"Constructing logic loader, doLogicResetOnConfigChange is {doLogicResetOnConfigChange}");
			return new LogicLoader(doLogicResetOnConfigChange);
		}

		public override void CreateCustomGameServices()
		{
			GameLog.Instance.Info("Constructing game specific game services");
			base.CreateCustomGameServices();

			CreditPlayoffService.Create();
			GleService.Create();
			TrumpsService.Create();
		}

		public override void DestroyCustomGameServices()
		{
			GameLog.Instance.Info("Destroying game specific game services");
			base.DestroyCustomGameServices();

			TrumpsService.Destroy();
			GleService.Destroy();
			CreditPlayoffService.Destroy();
		}

		public override Stage GetLogicStage(bool next)
		{
			return GetLogicStageFromStageId(GleGameController.GleStatus.GetStageId(next));

			Stage GetLogicStageFromStageId(string stageId)
			{
				for (var index = 0; index < stageMappings.Count; index++)
				{
					var mapping = stageMappings[index];
					if (mapping.StageId == stageId)
						return mapping.Stage;
				}

				return null;
			}
		}

		public override string GetStageIdFromLogicStage(Stage stage)
		{
			foreach (var mapping in stageMappings)
			{
				if (mapping.Stage.Equals(stage))
					return mapping.StageId;
			}

			return null;
		}

		public override Sequence GetTransitionSequence(Stage currentStage, Stage desiredStage)
		{
			foreach (var transition in allStageTransitions)
			{
				if (transition.Initial.Equals(currentStage) && transition.Final.Equals(desiredStage))
					return transition.Sequence;
			}

			return null;
		}

		protected override StageController CreateStageController()
		{
			GameLog.Instance.Info($"Creating stage controller with {gameStageTransitions.Count} transitions");
			return new StageController(gameStageTransitions.Select(t => t.Sequence).ToArray());
		}

		protected override void CreateNodes()
		{
			GameLog.Instance.Info("Creating game specific nodes");

			base.CreateNodes();

			foreach (var node in GameConfigurator.CreateNodes())
				AddNode(node);

			AddNode(new GleAutoPlayerDecisionNode());
		}

		protected override void CreateCustomPresentationControllers()
		{
			GameLog.Instance.Info("Creating game specific presentation controllers");

			AddPresentationController(new GleGameController());
			AddPresentationController(new TrumpsController());
			AddPresentationController(new CreditPlayoffController());

			foreach (var controller in GameIdentity.Common.GameIdentity.GetPresentationControllers())
				AddPresentationController(controller);

			AddPresentationController(new GameSpecificController());

			foreach (var controllers in GameConfigurator.CreatePresentationControllers())
				AddPresentationController(controllers);

			AddPresentationController(new MonitorCheckerController(new[] { MonitorRole.Top, MonitorRole.ButtonPanel }));
		}

		// ReSharper disable once RedundantOverriddenMember - here as an example
		protected override void Configure()
		{
			GameLog.Instance.Info("Running game specific configuration");

			base.Configure();

			GetPresentationController<DashboardController>().SetGameMessageOverride(new DashboardGameMessages(Array.Empty<Stage>(), null, string.Empty));

			// var stakesController = GetPresentationController<StakesController>();
			// stakesController.ConfigureClassicButtons(6, 6, false, true);
			// stakesController.ConfigureCostToCoverButtons(12);
		}
	}
}