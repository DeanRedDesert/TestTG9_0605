using Logic;
using Logic.Core.DecisionGenerator;
using Logic.Core.Engine;
using Logic.Core.Types;
using Logic.Core.Types.Exits;
using Logic.Core.WinCheck;
using Logic.Types;
using System;
using System.Collections.Generic;

namespace Game
{
	[EntryStage("Base")]
	[StageNames(new string[] { "Base", "FreeGames", "Respin" })]
	public class Runner : IRunner
	{
		private static readonly IReadOnlyList<StageConnection> stageConnections = new StageConnection[]
		{
			new StageConnection("Base", "FreeGames", "FreeGames"),
			new StageConnection("Base", "Respins", "Respin"),
			new StageConnection("FreeGames", "Respins", "Respin"),
			new StageConnection("FreeGames", "Retrigger", "FreeGames"),
			new StageConnection("Respin", "Respins", "Respin")
		};

		// Game Data Fields
		[StageNames(new string[] { "Base", "FreeGames", "Respin" })]
		private static readonly GeneralData data_ExcelGeneralData = CreateExcelGeneralData();

		[StageNames(new string[] { "Base", "FreeGames" })]
		private static readonly SymbolWindowStructure data_ExcelFiveByThreeStructure = CreateExcelFiveByThreeStructure();

		[StageNames(new string[] { "Respin" })]
		private static readonly SymbolWindowStructure data_ExcelFifteenIndependentReels = CreateExcelFifteenIndependentReels();

		[StageNames(new string[] { "Base", "FreeGames" })]
		private static readonly Patterns data_ExcelLinePatternProvider = CreateExcelLinePatternProvider();

		[StageNames(new string[] { "Base" })]
		private static readonly SelectorItems data_ExcelBaseStripProvider = CreateExcelBaseStripProvider();

		[StageNames(new string[] { "Base", "FreeGames" })]
		private static readonly WeightedSets data_ExcelWeightedChoiceSelectatron = CreateExcelWeightedChoiceSelectatron();

		[StageNames(new string[] { "Base", "FreeGames", "Respin" })]
		private static readonly SelectorItems data_ExcelReplacementTablesLookup = CreateExcelReplacementTablesLookup();

		[StageNames(new string[] { "Base", "FreeGames" })]
		private static readonly SelectorItems data_ExcelLineEvaluator = CreateExcelLineEvaluator();

		[StageNames(new string[] { "Base", "FreeGames" })]
		private static readonly Patterns data_ExcelScatterPatternProvider = CreateExcelScatterPatternProvider();

		[StageNames(new string[] { "Base", "FreeGames" })]
		private static readonly MaskPrize[] data_ExcelScatterEvaluator = CreateExcelScatterEvaluator();

		[StageNames(new string[] { "Base", "FreeGames", "Respin" })]
		private static readonly Patterns data_ExcelRespinPatternProvider = CreateExcelRespinPatternProvider();

		[StageNames(new string[] { "Base", "FreeGames", "Respin" })]
		private static readonly SelectorItems data_ExcelRespinPrizes = CreateExcelRespinPrizes();

		[StageNames(new string[] { "Base", "FreeGames" })]
		private static readonly PrizeCountWithCycleSet[] data_ExcelTriggerCycleSetsFromPrizes = CreateExcelTriggerCycleSetsFromPrizes();

		[StageNames(new string[] { "FreeGames" })]
		private static readonly SelectorItems data_ExcelFreeStripProvider = CreateExcelFreeStripProvider();

		[StageNames(new string[] { "FreeGames" })]
		private static readonly SymbolMultiplierDetails data_ExcelPrizeSymbolMultiplier = CreateExcelPrizeSymbolMultiplier();

		[StageNames(new string[] { "Respin" })]
		private static readonly ISymbolListStrip[] data_ExcelRespinNoFrameStrips = CreateExcelRespinNoFrameStrips();

		[StageNames(new string[] { "Respin" })]
		private static readonly ISymbolListStrip[] data_ExcelRespinFrameStrips = CreateExcelRespinFrameStrips();

		public IReadOnlyList<Variable> ResolveMissingVariables(IDecisionGenerator decisionGenerator, Inputs inputs, CycleResult previousResult)
		{
			return Array.Empty<Variable>();
		}

		public CycleResult EvaluateCycle(IDecisionGenerator decisionGenerator, Inputs initiatingInputs, CycleResult previousResult)
		{
			var inputs = CreateInputs(initiatingInputs, previousResult);
			inputs = inputs.ReplaceOrAdd(ResolveMissingVariables(decisionGenerator, inputs, previousResult));
			var stageResults = EvaluateStage(decisionGenerator, inputs);
			return GenerateCycleResult(inputs, previousResult, stageResults);
		}

		public Inputs CreateInputs(Inputs initiatingInputs, CycleResult previousResult = null)
			=> CustomiseCycle.CreateInputsForCycle(previousResult, initiatingInputs);

		public StageResults EvaluateStage(IDecisionGenerator decisionGenerator, Inputs inputs)
		{
			switch (inputs.CurrentStage())
			{
				case "Base": return Stage0(decisionGenerator, inputs);
				case "FreeGames": return Stage1(decisionGenerator, inputs);
				case "Respin": return Stage2(decisionGenerator, inputs);
				default: throw new Exception("Invalid stage");
			}
		}

		public CycleResult GenerateCycleResult(Inputs inputs, CycleResult previousResult, StageResults stageResults)
		{
			switch (inputs.CurrentStage())
			{
				case "Base": return Result0(inputs, previousResult, stageResults);
				case "FreeGames": return Result1(inputs, previousResult, stageResults);
				case "Respin": return Result2(inputs, previousResult, stageResults);
				default: throw new Exception("Invalid stage");
			}
		}

		private StageResults Stage0(IDecisionGenerator decisionGenerator, Inputs inputs)
		{
			var input_TotalBet = inputs.GetInput<Credits>("TotalBet");
			var input_Percentage = inputs.GetInput<string>("Percentage");
			var input_BetMultiplier = inputs.GetInput<long>("BetMultiplier");
			var input_LinesBet = inputs.GetInput<Credits>("LinesBet");
			var input_Denom = inputs.GetInput<Money>("Denom");

			var rdg_8_0 = new ScopedDecisionGenerator(decisionGenerator, "Base");
			var rdg_7_0 = new ScopedDecisionGenerator(decisionGenerator, "SymbolReplacement");
			var rdg_6_0 = new ScopedDecisionGenerator(decisionGenerator, "SetChoice");

			var local_0 = ThrowIfReturnsNull(ProcessorFunctions.Select(data_ExcelLineEvaluator, input_Denom) as MaskPrize[], "ProcessorFunctions.Select");
			var local_1 = ThrowIfReturnsNull(ProcessorFunctions.Select(data_ExcelBaseStripProvider, input_Denom, input_Percentage) as ISymbolListStrip[], "ProcessorFunctions.Select");
			var local_2 = ThrowIfReturnsNull(ProcessorFunctions.Select(data_ExcelReplacementTablesLookup, input_Denom, input_Percentage, input_LinesBet, input_BetMultiplier) as ReplacementTables, "ProcessorFunctions.Select");
			var local_3 = ThrowIfReturnsNull(ProcessorFunctions.Select(data_ExcelRespinPrizes, input_Denom) as List<RespinPrize>, "ProcessorFunctions.Select");
			var local_4 = ThrowIfReturnsNull(ProcessorFunctions.ToReplacementSymbol(data_ExcelGeneralData, false), "ProcessorFunctions.ToReplacementSymbol");
			var local_5 = ThrowIfReturnsNull(ProcessorFunctions.WeightedSetSelect(data_ExcelWeightedChoiceSelectatron, input_Denom, input_Percentage, input_LinesBet, input_BetMultiplier), "ProcessorFunctions.WeightedSetSelect");
			var local_6 = ThrowIfReturnsNull(ProcessorFunctions.SetChoice(rdg_6_0, local_5), "ProcessorFunctions.SetChoice");
			var local_7 = ThrowIfReturnsNull(ProcessorFunctions.StripSymbolReplacement(rdg_7_0, local_1, local_6, local_2, local_4), "ProcessorFunctions.StripSymbolReplacement");
			var local_8 = ThrowIfReturnsNull(ProcessorFunctions.CreateSymbolWindow(rdg_8_0, data_ExcelFiveByThreeStructure, local_7), "ProcessorFunctions.CreateSymbolWindow");
			var local_9 = ThrowIfReturnsNull(ProcessorFunctions.EvaluateLines(local_8, data_ExcelLinePatternProvider, local_0, checked((int)input_LinesBet.ToUInt64()), true, checked((ulong)input_BetMultiplier)), "ProcessorFunctions.EvaluateLines");
			var local_10 = ThrowIfReturnsNull(ProcessorFunctions.EvaluateScatters(local_8, data_ExcelScatterPatternProvider, data_ExcelScatterEvaluator, true, input_TotalBet.ToUInt64()), "ProcessorFunctions.EvaluateScatters");
			var local_11 = ThrowIfReturnsNull(ProcessorFunctions.CheckForPrizeTriggers(local_10, data_ExcelTriggerCycleSetsFromPrizes, TriggerPosition.AtEnd), "ProcessorFunctions.CheckForPrizeTriggers");
			var local_11_0 = ThrowIfPropertyNull(local_11.Triggers, "TriggerCycleSetsResult.Triggers");
			var local_12 = ThrowIfReturnsNull(ProcessorFunctions.EvaluateRespinTrigger(local_8, data_ExcelRespinPatternProvider, local_3, 5, input_TotalBet.ToUInt64(), 3, "BaseRespin"), "ProcessorFunctions.EvaluateRespinTrigger");
			var local_12_0 = local_12.Trigger;
			var local_12_1 = local_12.RespinState;
			var local_12_2 = ThrowIfPropertyNull(local_12.Prizes, "RespinDetectorResult.Prizes");
			var local_12_3 = ThrowIfPropertyNull(local_12.BonusPrizes, "RespinDetectorResult.BonusPrizes");

			return new BaseStageResults(
				new StageResult(0, 10, "SetChoice", StageResultType.VariableOneGame, local_6),
				new StageResult(0, 0, "SymbolWindow", StageResultType.Presentation, local_8),
				new StageResult(0, 1, "LinePrizes", StageResultType.AwardCreditsList, local_9),
				new StageResult(0, 2, "ScatterPrizes", StageResultType.AwardCreditsList, local_10),
				new StageResult(0, 5, "FreeGames", StageResultType.ExitList, local_11_0.ToDesiredExits("FreeGames")),
				new StageResult(0, 8, "Respins", StageResultType.ExitList, local_12_0 == null ? Array.Empty<DesiredExit>() : local_12_0.ToDesiredExits("Respins")),
				new StageResult(0, 8, "RespinState", StageResultType.VariableOneGame, local_12_1),
				new StageResult(0, 8, "Prizes", StageResultType.AwardCreditsList, local_12_2),
				new StageResult(0, 8, "BonusPrizes", StageResultType.AwardCreditsList, local_12_3));
		}

		private CycleResult Result0(Inputs inputs, CycleResult previousResult, StageResults stageResults)
		{
			// Add the pending exits
			var newPendingExits = new List<DesiredExit>();
			newPendingExits.AddRange((IReadOnlyList<DesiredExit>)stageResults[4].Value);
			newPendingExits.AddRange((IReadOnlyList<DesiredExit>)stageResults[5].Value);

			// Process the triggers
			var cycles = inputs.GetInput<Cycles>("Cycles");
			var currentCycle = cycles.Current;
			cycles = cycles.PlayOne();
			cycles = CustomiseCycle.ProcessTriggers(cycles, currentCycle, newPendingExits, stageConnections);
			cycles = cycles.MoveNext();

			// Add the awarded credits
			var cycleAward = Credits.Zero;

			foreach (var r in (IReadOnlyList<CellPrizeResult>)stageResults[3].Value)
				cycleAward += new Credits((ulong)r.Value);

			foreach (var r in (IReadOnlyList<CellPrizeResult>)stageResults[2].Value)
				cycleAward += new Credits((ulong)r.Value);

			foreach (var r in (IReadOnlyList<CellPrizeResult>)stageResults[7].Value)
				cycleAward += new Credits((ulong)r.Value);

			foreach (var r in (IReadOnlyList<CellPrizeResult>)stageResults[8].Value)
				cycleAward += new Credits((ulong)r.Value);

			// Add the progressive triggers
			var newProgressiveTriggers = new List<string>();

			// Generate the CycleResult
			var cycleResult = new CycleResult(
				inputs,
				cycles,
				cycleAward,
				cycleAward + (previousResult == null || previousResult.Cycles.IsFinished ? Credits.Zero : previousResult.TotalAwardedPrize),
				stageResults,
				newProgressiveTriggers);

			// Modify the CycleResult
			return CustomiseCycle.ModifyResultPostCycle(cycleResult);
		}

		private StageResults Stage1(IDecisionGenerator decisionGenerator, Inputs inputs)
		{
			var input_Percentage = inputs.GetInput<string>("Percentage");
			var input_TotalBet = inputs.GetInput<Credits>("TotalBet");
			var input_Denom = inputs.GetInput<Money>("Denom");
			var input_BetMultiplier = inputs.GetInput<long>("BetMultiplier");
			var input_LinesBet = inputs.GetInput<Credits>("LinesBet");

			var rdg_8_0 = new ScopedDecisionGenerator(decisionGenerator, "Free");
			var rdg_6_0 = new ScopedDecisionGenerator(decisionGenerator, "SetChoice");
			var rdg_7_0 = new ScopedDecisionGenerator(decisionGenerator, "SymbolReplacement");

			var local_0 = ThrowIfReturnsNull(ProcessorFunctions.Select(data_ExcelLineEvaluator, input_Denom) as MaskPrize[], "ProcessorFunctions.Select");
			var local_1 = ThrowIfReturnsNull(ProcessorFunctions.Select(data_ExcelFreeStripProvider, input_Percentage, input_Denom) as ISymbolListStrip[], "ProcessorFunctions.Select");
			var local_2 = ThrowIfReturnsNull(ProcessorFunctions.Select(data_ExcelReplacementTablesLookup, input_Denom, input_Percentage, input_LinesBet, input_BetMultiplier) as ReplacementTables, "ProcessorFunctions.Select");
			var local_3 = ThrowIfReturnsNull(ProcessorFunctions.Select(data_ExcelRespinPrizes, input_Denom) as List<RespinPrize>, "ProcessorFunctions.Select");
			var local_4 = ThrowIfReturnsNull(ProcessorFunctions.ToReplacementSymbol(data_ExcelGeneralData, false), "ProcessorFunctions.ToReplacementSymbol");
			var local_5 = ThrowIfReturnsNull(ProcessorFunctions.WeightedSetSelect(data_ExcelWeightedChoiceSelectatron, input_Denom, input_Percentage, input_LinesBet, input_BetMultiplier), "ProcessorFunctions.WeightedSetSelect");
			var local_6 = ThrowIfReturnsNull(ProcessorFunctions.SetChoice(rdg_6_0, local_5), "ProcessorFunctions.SetChoice");
			var local_7 = ThrowIfReturnsNull(ProcessorFunctions.StripSymbolReplacement(rdg_7_0, local_1, local_6, local_2, local_4), "ProcessorFunctions.StripSymbolReplacement");
			var local_8 = ThrowIfReturnsNull(ProcessorFunctions.CreateSymbolWindow(rdg_8_0, data_ExcelFiveByThreeStructure, local_7), "ProcessorFunctions.CreateSymbolWindow");
			var local_9 = ThrowIfReturnsNull(ProcessorFunctions.EvaluateLines(local_8, data_ExcelLinePatternProvider, local_0, checked((int)input_LinesBet.ToUInt64()), true, checked((ulong)input_BetMultiplier)), "ProcessorFunctions.EvaluateLines");
			var local_10 = ThrowIfReturnsNull(ProcessorFunctions.EvaluateScatters(local_8, data_ExcelScatterPatternProvider, data_ExcelScatterEvaluator, true, input_TotalBet.ToUInt64()), "ProcessorFunctions.EvaluateScatters");
			var local_11 = ThrowIfReturnsNull(ProcessorFunctions.EvaluateRespinTrigger(local_8, data_ExcelRespinPatternProvider, local_3, 5, input_TotalBet.ToUInt64(), 3, "FreeRespin"), "ProcessorFunctions.EvaluateRespinTrigger");
			var local_11_0 = local_11.Trigger;
			var local_11_1 = local_11.RespinState;
			var local_11_2 = ThrowIfPropertyNull(local_11.Prizes, "RespinDetectorResult.Prizes");
			var local_11_3 = ThrowIfPropertyNull(local_11.BonusPrizes, "RespinDetectorResult.BonusPrizes");
			var local_12 = ThrowIfReturnsNull(ProcessorFunctions.CheckForPrizeTriggers(local_10, data_ExcelTriggerCycleSetsFromPrizes, true), "ProcessorFunctions.CheckForPrizeTriggers");
			var local_12_0 = ThrowIfPropertyNull(local_12.Triggers, "TriggerCycleSetsResult.Triggers");
			var local_13 = ThrowIfReturnsNull(ProcessorFunctions.ApplySymbolMultiplier(local_9, local_8, data_ExcelPrizeSymbolMultiplier), "ProcessorFunctions.ApplySymbolMultiplier");

			return new FreeGamesStageResults(
				new StageResult(1, 9, "SetChoice", StageResultType.VariableOneGame, local_6),
				new StageResult(1, 2, "SymbolWindow", StageResultType.Presentation, local_8),
				new StageResult(1, 4, "ScatterPrizes", StageResultType.AwardCreditsList, local_10),
				new StageResult(1, 8, "Respins", StageResultType.ExitList, local_11_0 == null ? Array.Empty<DesiredExit>() : local_11_0.ToDesiredExits("Respins")),
				new StageResult(1, 8, "Prizes", StageResultType.AwardCreditsList, local_11_2),
				new StageResult(1, 8, "BonusPrizes", StageResultType.AwardCreditsList, local_11_3),
				new StageResult(1, 8, "RespinState", StageResultType.VariableOneGame, local_11_1),
				new StageResult(1, 11, "Retrigger", StageResultType.ExitList, local_12_0.ToDesiredExits("Retrigger")),
				new StageResult(1, 13, "LinePrizes", StageResultType.AwardCreditsList, local_13));
		}

		private CycleResult Result1(Inputs inputs, CycleResult previousResult, StageResults stageResults)
		{
			// Add the pending exits
			var newPendingExits = new List<DesiredExit>();
			newPendingExits.AddRange((IReadOnlyList<DesiredExit>)stageResults[3].Value);
			newPendingExits.AddRange((IReadOnlyList<DesiredExit>)stageResults[7].Value);

			// Process the triggers
			var cycles = inputs.GetInput<Cycles>("Cycles");
			var currentCycle = cycles.Current;
			cycles = cycles.PlayOne();
			cycles = CustomiseCycle.ProcessTriggers(cycles, currentCycle, newPendingExits, stageConnections);
			cycles = cycles.MoveNext();

			// Add the awarded credits
			var cycleAward = Credits.Zero;

			foreach (var r in (IReadOnlyList<CellPrizeResult>)stageResults[4].Value)
				cycleAward += new Credits((ulong)r.Value);

			foreach (var r in (IReadOnlyList<CellPrizeResult>)stageResults[5].Value)
				cycleAward += new Credits((ulong)r.Value);

			foreach (var r in (IReadOnlyList<CellPrizeResult>)stageResults[2].Value)
				cycleAward += new Credits((ulong)r.Value);

			foreach (var r in (IReadOnlyList<CellPrizeResult>)stageResults[8].Value)
				cycleAward += new Credits((ulong)r.Value);

			// Add the progressive triggers
			var newProgressiveTriggers = new List<string>();

			// Generate the CycleResult
			var cycleResult = new CycleResult(
				inputs,
				cycles,
				cycleAward,
				cycleAward + (previousResult == null || previousResult.Cycles.IsFinished ? Credits.Zero : previousResult.TotalAwardedPrize),
				stageResults,
				newProgressiveTriggers);

			// Modify the CycleResult
			return CustomiseCycle.ModifyResultPostCycle(cycleResult);
		}

		private StageResults Stage2(IDecisionGenerator decisionGenerator, Inputs inputs)
		{
			var input_RespinState = inputs.GetInput<RespinState>("RespinState");
			var input_Percentage = inputs.GetInput<string>("Percentage");
			var input_Denom = inputs.GetInput<Money>("Denom");
			var input_LinesBet = inputs.GetInput<Credits>("LinesBet");
			var input_BetMultiplier = inputs.GetInput<long>("BetMultiplier");
			var input_Cycles = inputs.GetInput<Cycles>("Cycles");
			var input_TotalBet = inputs.GetInput<Credits>("TotalBet");
			var input_SetChoice = inputs.GetInput<string>("SetChoice");

			var rdg_4_0 = new ScopedDecisionGenerator(decisionGenerator, "RespinSymbolReplacement");
			var rdg_5_0 = new ScopedDecisionGenerator(decisionGenerator, "Respin");

			var local_0 = ThrowIfReturnsNull(ProcessorFunctions.RespinStripProvider(data_ExcelRespinNoFrameStrips, data_ExcelRespinFrameStrips, input_RespinState), "ProcessorFunctions.RespinStripProvider");
			var local_1 = ThrowIfReturnsNull(ProcessorFunctions.Select(data_ExcelReplacementTablesLookup, input_Denom, input_Percentage, input_LinesBet, input_BetMultiplier) as ReplacementTables, "ProcessorFunctions.Select");
			var local_2 = ThrowIfReturnsNull(ProcessorFunctions.Select(data_ExcelRespinPrizes, input_Denom) as List<RespinPrize>, "ProcessorFunctions.Select");
			var local_3 = ThrowIfReturnsNull(ProcessorFunctions.ToReplacementSymbol(data_ExcelGeneralData, true), "ProcessorFunctions.ToReplacementSymbol");
			var local_4 = ThrowIfReturnsNull(ProcessorFunctions.RespinStripSymbolReplacement(rdg_4_0, input_RespinState, local_0, input_SetChoice, local_1, local_3), "ProcessorFunctions.RespinStripSymbolReplacement");
			var local_5 = ThrowIfReturnsNull(ProcessorFunctions.CreateLockedSymbolWindow(rdg_5_0, data_ExcelFifteenIndependentReels, local_4, input_RespinState), "ProcessorFunctions.CreateLockedSymbolWindow");
			var local_5_0 = ThrowIfPropertyNull(local_5.SymbolWindowResult, "LockedSymbolWindowResult.SymbolWindowResult");
			var local_6 = ThrowIfReturnsNull(ProcessorFunctions.EvaluateRespin(input_Cycles, local_5_0, data_ExcelRespinPatternProvider, local_2, input_RespinState, input_TotalBet.ToUInt64(), 3), "ProcessorFunctions.EvaluateRespin");
			var local_6_0 = local_6.Trigger;
			var local_6_1 = ThrowIfPropertyNull(local_6.RespinState, "RespinResult.RespinState");
			var local_6_2 = ThrowIfPropertyNull(local_6.Prizes, "RespinResult.Prizes");
			var local_6_3 = ThrowIfPropertyNull(local_6.BonusPrizes, "RespinResult.BonusPrizes");
			var local_6_4 = ThrowIfPropertyNull(local_6.ProgressivePrizes, "RespinResult.ProgressivePrizes");

			return new RespinStageResults(
				new StageResult(2, 6, "LockedSymbolWindow", StageResultType.Presentation, local_5),
				new StageResult(2, 5, "Respins", StageResultType.ExitList, local_6_0 == null ? Array.Empty<DesiredExit>() : local_6_0.ToDesiredExits("Respins")),
				new StageResult(2, 5, "Prizes", StageResultType.AwardCreditsList, local_6_2),
				new StageResult(2, 5, "BonusPrizes", StageResultType.AwardCreditsList, local_6_3),
				new StageResult(2, 5, "RespinState", StageResultType.VariableOneGame, local_6_1),
				new StageResult(2, 5, "Progressives", StageResultType.ProgressiveList, local_6_4));
		}

		private CycleResult Result2(Inputs inputs, CycleResult previousResult, StageResults stageResults)
		{
			// Add the pending exits
			var newPendingExits = new List<DesiredExit>();
			newPendingExits.AddRange((IReadOnlyList<DesiredExit>)stageResults[1].Value);

			// Process the triggers
			var cycles = inputs.GetInput<Cycles>("Cycles");
			var currentCycle = cycles.Current;
			cycles = cycles.PlayOne();
			cycles = CustomiseCycle.ProcessTriggers(cycles, currentCycle, newPendingExits, stageConnections);
			cycles = cycles.MoveNext();

			// Add the awarded credits
			var cycleAward = Credits.Zero;

			foreach (var r in (IReadOnlyList<CellPrizeResult>)stageResults[2].Value)
				cycleAward += new Credits((ulong)r.Value);

			foreach (var r in (IReadOnlyList<CellPrizeResult>)stageResults[3].Value)
				cycleAward += new Credits((ulong)r.Value);

			// Add the progressive triggers
			var newProgressiveTriggers = new List<string>();

			foreach (var r in (IReadOnlyList<ProgressivePrizeResult>)stageResults[5].Value)
				newProgressiveTriggers.Add(r.Progressive);

			// Generate the CycleResult
			var cycleResult = new CycleResult(
				inputs,
				cycles,
				cycleAward,
				cycleAward + (previousResult == null || previousResult.Cycles.IsFinished ? Credits.Zero : previousResult.TotalAwardedPrize),
				stageResults,
				newProgressiveTriggers);

			// Modify the CycleResult
			return CustomiseCycle.ModifyResultPostCycle(cycleResult);
		}

		private static GeneralData CreateExcelGeneralData()
		{
			return new GeneralData("SCAT2", 11, "HIT", 13, 3, 5);
		}

		private static SymbolWindowStructure CreateExcelFiveByThreeStructure()
		{
			return new SymbolWindowStructure(
				new List<CellPopulation>()
				{
					new CellPopulation("Reel1", 0, Cell.CreateList(" C0R0 C0R1 C0R2")),
					new CellPopulation("Reel2", 0, Cell.CreateList(" C1R0 C1R1 C1R2")),
					new CellPopulation("Reel3", 0, Cell.CreateList(" C2R0 C2R1 C2R2")),
					new CellPopulation("Reel4", 0, Cell.CreateList(" C3R0 C3R1 C3R2")),
					new CellPopulation("Reel5", 0, Cell.CreateList(" C4R0 C4R1 C4R2"))
				}
			);
		}

		private static SymbolWindowStructure CreateExcelFifteenIndependentReels()
		{
			return new SymbolWindowStructure(
				new List<CellPopulation>()
				{
					new CellPopulation("C0R0", 0, Cell.CreateList(" C0R0")),
					new CellPopulation("C0R1", 0, Cell.CreateList(" C0R1")),
					new CellPopulation("C0R2", 0, Cell.CreateList(" C0R2")),
					new CellPopulation("C1R0", 0, Cell.CreateList(" C1R0")),
					new CellPopulation("C1R1", 0, Cell.CreateList(" C1R1")),
					new CellPopulation("C1R2", 0, Cell.CreateList(" C1R2")),
					new CellPopulation("C2R0", 0, Cell.CreateList(" C2R0")),
					new CellPopulation("C2R1", 0, Cell.CreateList(" C2R1")),
					new CellPopulation("C2R2", 0, Cell.CreateList(" C2R2")),
					new CellPopulation("C3R0", 0, Cell.CreateList(" C3R0")),
					new CellPopulation("C3R1", 0, Cell.CreateList(" C3R1")),
					new CellPopulation("C3R2", 0, Cell.CreateList(" C3R2")),
					new CellPopulation("C4R0", 0, Cell.CreateList(" C4R0")),
					new CellPopulation("C4R1", 0, Cell.CreateList(" C4R1")),
					new CellPopulation("C4R2", 0, Cell.CreateList(" C4R2"))
				}
			);
		}

		private static Patterns CreateExcelLinePatternProvider()
		{
			return new Patterns(
				new List<Pattern>()
				{
					new Pattern("Line 1", Cluster.CreateList(", C0R1, C1R1, C2R1, C3R1, C4R1")),
					new Pattern("Line 2", Cluster.CreateList(", C0R0, C1R0, C2R0, C3R0, C4R0")),
					new Pattern("Line 3", Cluster.CreateList(", C0R2, C1R2, C2R2, C3R2, C4R2")),
					new Pattern("Line 4", Cluster.CreateList(", C0R0, C1R1, C2R2, C3R1, C4R0")),
					new Pattern("Line 5", Cluster.CreateList(", C0R2, C1R1, C2R0, C3R1, C4R2")),
					new Pattern("Line 6", Cluster.CreateList(", C0R0, C1R0, C2R1, C3R0, C4R0")),
					new Pattern("Line 7", Cluster.CreateList(", C0R2, C1R2, C2R1, C3R2, C4R2")),
					new Pattern("Line 8", Cluster.CreateList(", C0R1, C1R2, C2R2, C3R2, C4R1")),
					new Pattern("Line 9", Cluster.CreateList(", C0R1, C1R0, C2R0, C3R0, C4R1")),
					new Pattern("Line 10", Cluster.CreateList(", C0R1, C1R1, C2R0, C3R1, C4R1")),
					new Pattern("Line 11", Cluster.CreateList(", C0R1, C1R1, C2R2, C3R1, C4R1")),
					new Pattern("Line 12", Cluster.CreateList(", C0R0, C1R1, C2R1, C3R1, C4R0")),
					new Pattern("Line 13", Cluster.CreateList(", C0R2, C1R1, C2R1, C3R1, C4R2")),
					new Pattern("Line 14", Cluster.CreateList(", C0R1, C1R2, C2R1, C3R2, C4R1")),
					new Pattern("Line 15", Cluster.CreateList(", C0R1, C1R0, C2R1, C3R0, C4R1")),
					new Pattern("Line 16", Cluster.CreateList(", C0R0, C1R1, C2R0, C3R1, C4R0")),
					new Pattern("Line 17", Cluster.CreateList(", C0R2, C1R1, C2R2, C3R1, C4R2")),
					new Pattern("Line 18", Cluster.CreateList(", C0R2, C1R0, C2R2, C3R0, C4R2")),
					new Pattern("Line 19", Cluster.CreateList(", C0R0, C1R2, C2R0, C3R2, C4R0")),
					new Pattern("Line 20", Cluster.CreateList(", C0R1, C1R0, C2R2, C3R0, C4R1")),
					new Pattern("Line 21", Cluster.CreateList(", C0R1, C1R2, C2R0, C3R2, C4R1")),
					new Pattern("Line 22", Cluster.CreateList(", C0R0, C1R0, C2R2, C3R0, C4R0")),
					new Pattern("Line 23", Cluster.CreateList(", C0R2, C1R2, C2R0, C3R2, C4R2")),
					new Pattern("Line 24", Cluster.CreateList(", C0R0, C1R2, C2R1, C3R2, C4R0")),
					new Pattern("Line 25", Cluster.CreateList(", C0R2, C1R0, C2R1, C3R0, C4R2")),
					new Pattern("Line 26", Cluster.CreateList(", C0R0, C1R2, C2R2, C3R2, C4R0")),
					new Pattern("Line 27", Cluster.CreateList(", C0R2, C1R0, C2R0, C3R0, C4R2")),
					new Pattern("Line 28", Cluster.CreateList(", C0R0, C1R0, C2R1, C3R2, C4R2")),
					new Pattern("Line 29", Cluster.CreateList(", C0R2, C1R2, C2R1, C3R0, C4R0")),
					new Pattern("Line 30", Cluster.CreateList(", C0R2, C1R1, C2R0, C3R0, C4R0")),
					new Pattern("Line 31", Cluster.CreateList(", C0R0, C1R1, C2R2, C3R2, C4R2")),
					new Pattern("Line 32", Cluster.CreateList(", C0R0, C1R0, C2R0, C3R1, C4R2")),
					new Pattern("Line 33", Cluster.CreateList(", C0R2, C1R2, C2R2, C3R1, C4R0")),
					new Pattern("Line 34", Cluster.CreateList(", C0R1, C1R0, C2R1, C3R2, C4R1")),
					new Pattern("Line 35", Cluster.CreateList(", C0R1, C1R2, C2R1, C3R0, C4R1")),
					new Pattern("Line 36", Cluster.CreateList(", C0R0, C1R1, C2R1, C3R1, C4R2")),
					new Pattern("Line 37", Cluster.CreateList(", C0R2, C1R1, C2R1, C3R1, C4R0")),
					new Pattern("Line 38", Cluster.CreateList(", C0R1, C1R1, C2R1, C3R1, C4R0")),
					new Pattern("Line 39", Cluster.CreateList(", C0R1, C1R1, C2R1, C3R1, C4R2")),
					new Pattern("Line 40", Cluster.CreateList(", C0R0, C1R0, C2R0, C3R0, C4R1")),
					new Pattern("Line 41", Cluster.CreateList(", C0R2, C1R2, C2R2, C3R2, C4R1")),
					new Pattern("Line 42", Cluster.CreateList(", C0R1, C1R0, C2R0, C3R0, C4R0")),
					new Pattern("Line 43", Cluster.CreateList(", C0R1, C1R2, C2R2, C3R2, C4R2")),
					new Pattern("Line 44", Cluster.CreateList(", C0R1, C1R1, C2R1, C3R0, C4R1")),
					new Pattern("Line 45", Cluster.CreateList(", C0R1, C1R1, C2R1, C3R2, C4R1")),
					new Pattern("Line 46", Cluster.CreateList(", C0R0, C1R1, C2R2, C3R1, C4R2")),
					new Pattern("Line 47", Cluster.CreateList(", C0R2, C1R1, C2R0, C3R1, C4R0")),
					new Pattern("Line 48", Cluster.CreateList(", C0R2, C1R1, C2R2, C3R1, C4R0")),
					new Pattern("Line 49", Cluster.CreateList(", C0R0, C1R1, C2R0, C3R1, C4R2")),
					new Pattern("Line 50", Cluster.CreateList(", C0R2, C1R0, C2R1, C3R2, C4R0")),
					new Pattern("Line 51", Cluster.CreateList(", C0R0, C1R2, C2R1, C3R0, C4R2")),
					new Pattern("Line 52", Cluster.CreateList(", C0R1, C1R0, C2R2, C3R1, C4R0")),
					new Pattern("Line 53", Cluster.CreateList(", C0R1, C1R2, C2R0, C3R1, C4R2")),
					new Pattern("Line 54", Cluster.CreateList(", C0R2, C1R1, C2R0, C3R2, C4R1")),
					new Pattern("Line 55", Cluster.CreateList(", C0R0, C1R1, C2R2, C3R0, C4R1")),
					new Pattern("Line 56", Cluster.CreateList(", C0R0, C1R1, C2R0, C3R2, C4R1")),
					new Pattern("Line 57", Cluster.CreateList(", C0R2, C1R1, C2R2, C3R0, C4R1")),
					new Pattern("Line 58", Cluster.CreateList(", C0R1, C1R0, C2R2, C3R1, C4R2")),
					new Pattern("Line 59", Cluster.CreateList(", C0R1, C1R2, C2R0, C3R1, C4R0")),
					new Pattern("Line 60", Cluster.CreateList(", C0R1, C1R0, C2R0, C3R1, C4R2"))
				},
				new SymbolWindowStructure(
					new List<CellPopulation>()
					{
						new CellPopulation("Reel1", 0, Cell.CreateList(" C0R0 C0R1 C0R2")),
						new CellPopulation("Reel2", 0, Cell.CreateList(" C1R0 C1R1 C1R2")),
						new CellPopulation("Reel3", 0, Cell.CreateList(" C2R0 C2R1 C2R2")),
						new CellPopulation("Reel4", 0, Cell.CreateList(" C3R0 C3R1 C3R2")),
						new CellPopulation("Reel5", 0, Cell.CreateList(" C4R0 C4R1 C4R2"))
					}
				)
			);
		}

		private static SelectorItems CreateExcelBaseStripProvider()
		{
			return new SelectorItems(
				new List<SelectorItem>()
				{
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(1UL), new Money(2UL), new Money(5UL), new Money(10UL)), Requirement.Create("Percentage", "NSW01") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 0 4 7 5 3 8 2 1 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 1 5 7 4 6 1 8 4 2 7 8 6 0 4 7 8 4 6 3 9 0 5 6 3 0 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 7 0 9 1 5 2 6 8 1 7 3 4 2 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 3 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 2 7 9 1 6 11 11 11 8 3 4 12 6 0 5 8 2 3 7 10 1 6 7 4 6 12 8 2 9 0 8 6 3 10 7 1 8 5 4 3 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 1 9 0 12 2 1 8 6 2 1 5 3 6 7 4 6 2 9 5 0 7 1 2 5 8 4 3 6 1 4 7 0 6 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 1 3 8 10 7 6 5 0 3 4 12 6 10 3 0 8 11 11 0 6 7 10 4 3 8 5 2 0 3 5 7 12 4 1 9 6 4 3 12 12 12 6 7 10 4 0 8 5 3 9 4 5 6 12 1 4 9 6 4 2 11 11 1 6 2 9 5 4 2 0 12 6 3 0 7 2 9 0 8 1 7 0 2 5 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 0 6 12 12 12 4 8 3 0 5 10 9 1 12 12 12 8 5 3 6 0 12 12 12 5 1 0 8 7 3 8 0 4 5 10 1 0 7 12 12 4 6 3 8 1 11 11 11 9 3 8 4 2 1 0 12 5 9 3 8 0 7 9 2 4 10 5 1 6 3 8 12 12 12 1 6 4 7 0 2 6 3 8 4 9 6 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 1 8 7 2 4 11 11 9 3 0 6 12 12 12 7 0 9 5 4 7 6 3 4 12 12 12 8 9 4 2 6 1 4 5 7 3 12 12 12 8 10 3 5 7 12 12 12 8 2 0 9 1 6 4 7 9 2 12 12 12 6 8 11 11 11 3 8 10 1 5 0 4 12 12 6 4 9 2 8 10 3 0 2 7 5 1 12 12 6 4 0 12 7 4 9 3")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW01") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 2 4 7 5 3 8 2 5 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 8 5 7 4 6 1 8 4 2 7 8 6 1 0 7 4 8 6 3 9 0 1 6 3 0 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 1 0 9 1 5 2 6 8 1 7 3 4 0 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 0 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 12 7 9 1 6 11 11 11 8 3 4 1 6 0 5 8 2 3 9 10 1 12 7 4 6 0 8 2 9 0 8 6 3 10 7 1 8 12 4 6 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 1 9 0 12 2 1 8 6 2 1 5 3 8 7 4 6 2 9 12 0 7 1 2 5 8 0 3 6 1 4 7 0 12 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 4 3 8 10 7 2 5 0 3 4 12 6 10 3 5 8 11 11 0 6 7 10 4 3 8 7 2 6 3 5 7 12 4 5 9 6 4 3 12 12 12 6 7 10 4 2 8 5 3 9 4 5 6 12 3 4 9 6 4 2 11 11 1 8 2 9 5 4 8 0 12 6 3 5 7 2 9 4 8 1 7 3 2 9 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 3 8 1 0 12 4 8 3 0 5 10 9 1 12 12 8 2 5 3 6 0 12 12 9 5 1 3 8 7 3 8 0 4 5 10 1 6 7 3 0 4 6 3 8 1 11 11 11 9 3 8 4 2 1 3 12 5 9 3 8 0 6 9 2 4 10 5 1 6 3 8 12 12 12 1 6 4 7 5 2 6 3 8 4 9 5 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 3 8 6 1 4 11 11 7 9 0 6 8 3 12 0 6 9 0 4 7 2 3 4 12 12 12 8 9 4 2 0 7 4 5 6 3 0 12 12 8 10 3 5 0 12 12 12 6 2 0 9 1 6 4 7 8 3 12 12 12 6 8 11 11 11 3 8 10 1 5 0 4 12 12 8 4 9 2 8 10 3 4 2 9 5 1 12 12 6 4 0 12 7 4 9 3")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL), new Money(200UL)), Requirement.Create("Percentage", "NSW01") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 2 4 7 5 3 8 2 5 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 8 5 7 4 6 1 8 4 2 7 8 6 1 0 7 4 8 6 3 9 0 1 6 3 0 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 1 0 9 1 5 2 6 8 1 7 3 4 0 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 0 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 12 7 9 1 6 11 11 11 8 3 4 1 6 0 5 8 2 3 9 10 1 12 7 4 6 0 8 2 9 0 8 6 3 10 7 1 8 12 4 6 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 1 9 0 12 2 1 8 6 2 1 5 3 8 7 4 6 2 9 12 0 7 1 2 5 8 0 3 6 1 4 7 0 12 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 4 3 8 10 7 2 5 0 3 4 12 6 10 3 5 8 11 11 0 6 7 10 4 3 8 7 2 6 3 5 7 12 4 5 9 6 4 3 12 12 12 6 7 10 4 2 8 5 3 9 4 5 6 12 3 4 9 6 4 5 11 11 1 8 2 9 5 4 2 0 12 6 3 5 7 2 9 4 8 1 7 3 2 9 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 3 8 1 0 12 4 8 3 0 5 10 9 1 12 12 8 2 5 3 6 0 12 12 9 5 1 3 8 7 3 8 0 4 5 10 1 6 7 3 0 4 6 3 8 1 11 11 11 9 3 8 4 2 1 3 12 5 9 3 8 0 7 9 2 4 10 5 1 6 3 8 12 12 12 1 6 4 7 5 2 6 3 8 4 9 5 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 3 8 6 1 4 11 11 7 9 0 6 8 3 12 0 6 9 0 4 7 2 3 4 12 12 12 8 9 4 2 0 7 4 5 6 3 0 12 12 8 10 3 5 0 12 12 12 6 2 0 9 1 6 4 7 8 3 12 12 12 6 7 11 11 11 3 8 10 1 5 0 4 12 12 8 4 9 2 8 10 3 4 2 9 5 1 12 12 6 4 0 12 7 4 9 3")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(1UL), new Money(2UL), new Money(5UL), new Money(10UL)), Requirement.Create("Percentage", "NSW02") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 0 4 7 5 3 8 2 1 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 1 5 7 4 6 1 8 4 2 7 8 6 0 4 7 4 8 6 3 9 0 5 6 3 0 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 7 0 9 1 5 2 6 8 1 7 3 4 2 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 3 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 2 7 9 1 6 11 11 11 8 3 4 12 6 0 5 8 2 3 7 10 1 6 7 4 6 12 8 2 9 0 8 6 3 10 7 1 8 5 4 3 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 1 9 0 12 2 1 8 6 2 1 5 3 6 7 4 6 2 9 5 0 7 1 2 5 8 4 3 6 1 4 7 0 6 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 1 3 8 10 7 2 5 0 3 4 12 6 10 3 0 8 11 11 0 6 7 10 4 3 8 5 2 0 3 5 7 12 4 1 9 6 4 8 12 12 12 6 7 10 4 0 8 5 3 9 4 5 6 12 1 4 9 6 4 2 11 11 1 6 2 9 5 4 7 0 12 6 3 0 7 2 9 0 8 1 7 0 2 5 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 0 6 12 12 12 4 8 3 7 5 10 9 1 12 12 12 7 5 3 6 0 12 12 12 5 1 0 8 7 3 8 0 4 5 10 1 0 7 12 12 4 6 3 8 1 11 11 11 9 3 8 4 2 7 0 12 5 8 3 6 0 7 9 2 4 10 5 1 6 3 8 12 12 12 1 6 4 7 0 2 6 3 8 4 9 5 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 1 8 7 2 4 11 11 9 3 0 6 12 12 12 7 0 9 5 4 7 6 3 4 12 12 12 8 9 4 2 6 1 4 5 9 3 12 12 12 8 10 3 5 9 12 12 12 8 2 0 9 1 6 4 7 9 2 12 12 12 6 8 11 11 11 3 9 10 1 5 0 4 12 12 6 4 9 2 8 10 3 0 2 7 5 1 12 12 6 4 0 12 7 4 9 3")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW02") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 2 4 7 5 3 8 2 5 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 8 5 7 4 6 1 8 4 2 7 8 6 1 0 7 4 8 6 3 9 0 1 6 3 0 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 1 0 9 1 5 2 6 8 1 7 3 4 0 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 0 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 12 7 9 1 6 11 11 11 8 3 7 1 6 0 5 8 2 3 9 10 1 12 7 4 6 0 8 2 9 0 8 6 3 10 7 1 8 12 4 6 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 1 9 0 12 2 1 8 6 2 1 5 3 8 7 4 6 2 9 12 0 7 1 2 5 8 0 3 6 1 4 7 0 12 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 4 3 8 10 7 2 5 0 3 4 12 6 10 3 5 8 11 11 0 6 7 10 4 3 8 7 2 6 3 5 7 12 4 5 9 6 4 7 12 12 12 6 7 10 4 2 8 5 3 9 4 5 6 12 3 4 9 6 4 2 11 11 1 8 2 9 5 4 7 0 12 6 3 5 7 2 9 4 5 1 7 3 2 9 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 3 5 1 0 12 4 7 3 0 5 10 9 1 12 12 7 2 5 3 6 0 12 12 9 5 1 3 8 7 3 8 0 4 5 10 1 6 7 3 0 4 6 3 8 1 11 11 11 9 3 8 4 2 6 3 12 5 9 3 8 0 7 9 2 4 10 5 1 6 3 8 12 12 12 1 6 4 7 5 2 6 3 8 4 9 5 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 3 8 6 1 4 11 11 7 5 0 6 8 3 12 0 6 9 0 4 7 2 3 4 12 12 12 8 9 4 2 0 7 4 5 6 3 0 12 12 8 10 3 5 0 12 12 12 6 2 0 9 1 6 4 7 8 3 12 12 12 6 8 11 11 11 3 5 10 1 5 0 4 12 12 8 4 9 2 8 10 3 4 2 9 5 1 12 12 6 4 0 12 7 4 9 3")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL), new Money(200UL)), Requirement.Create("Percentage", "NSW02") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 2 4 7 5 3 8 2 5 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 8 5 7 4 6 1 8 4 2 7 8 6 1 0 7 4 8 6 3 9 0 1 6 3 0 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 1 0 9 1 5 2 6 8 1 7 3 4 0 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 0 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 12 7 9 1 6 11 11 11 8 3 4 1 6 0 5 8 2 3 9 10 1 12 7 4 6 0 8 2 9 0 8 6 3 10 7 1 8 12 4 6 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 1 9 0 12 7 1 8 6 2 1 5 3 8 7 4 6 2 9 12 0 7 1 2 5 8 0 3 6 1 4 7 0 12 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 4 3 8 10 7 2 5 0 3 4 12 6 10 3 5 8 11 11 0 6 7 10 4 3 8 7 2 6 3 5 7 12 4 5 9 6 4 7 12 12 12 6 7 10 4 2 8 5 3 9 4 5 6 12 3 4 9 6 4 2 11 11 1 8 2 9 5 4 7 0 12 6 3 5 7 2 9 4 8 1 7 3 2 9 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 3 8 1 0 12 4 7 3 0 5 10 9 6 12 12 8 2 5 3 6 0 12 12 9 5 1 3 8 7 3 8 0 4 5 10 1 6 7 3 0 4 6 3 8 1 11 11 11 9 3 8 4 2 7 3 12 5 9 3 8 0 7 9 2 4 10 5 1 6 3 8 12 12 12 1 6 4 7 5 2 6 3 8 4 9 5 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 3 8 6 1 4 11 11 7 9 0 6 8 3 12 0 6 9 0 4 7 2 3 6 12 12 12 8 9 4 2 0 7 4 5 6 3 0 12 12 8 10 3 5 0 12 12 12 6 2 0 9 1 6 4 7 8 3 12 12 12 6 8 11 11 11 3 8 10 1 5 0 4 12 12 8 4 9 2 8 10 3 4 2 9 5 1 12 12 6 4 0 12 7 4 9 3")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(1UL), new Money(2UL), new Money(5UL), new Money(10UL)), Requirement.Create("Percentage", "NSW03") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 0 4 7 5 3 8 2 1 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 2 5 7 4 6 1 8 4 2 7 8 6 0 5 7 4 8 6 3 9 0 5 6 3 0 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 7 5 9 1 5 2 6 8 1 7 3 4 2 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 3 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 2 7 9 1 6 11 11 11 8 3 4 12 6 0 5 8 2 3 7 10 1 6 7 4 6 12 8 2 9 0 8 6 3 10 7 1 8 5 4 5 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 1 9 0 12 2 1 8 6 2 1 5 3 6 7 4 6 2 9 5 0 7 1 2 5 8 4 3 6 1 4 7 0 6 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 1 3 8 10 7 2 5 0 3 4 12 6 10 3 0 8 11 11 0 6 7 10 4 3 8 5 2 0 3 5 7 12 5 1 9 6 4 8 12 12 12 6 7 10 4 0 8 5 3 9 4 5 6 12 1 4 9 6 4 2 11 11 1 6 2 9 5 4 7 0 12 6 3 0 7 2 9 0 8 1 7 0 2 5 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 0 6 12 12 12 4 8 3 7 5 10 9 1 12 12 12 8 5 3 6 0 12 12 12 5 1 0 8 7 3 6 0 4 5 10 1 0 7 12 12 4 6 3 8 1 11 11 11 9 3 8 4 2 7 0 12 5 8 3 6 0 7 9 2 4 10 5 1 6 3 8 12 12 12 1 6 4 7 0 2 6 3 8 4 9 5 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 1 8 7 2 4 11 11 9 3 0 6 12 12 12 7 0 9 5 4 7 6 3 4 12 12 12 8 6 4 2 6 1 4 5 9 3 12 12 12 8 10 3 5 9 12 12 12 8 2 0 9 1 6 4 7 9 2 12 12 12 6 8 11 11 11 3 9 10 1 5 0 4 12 12 6 4 9 2 8 10 3 0 2 7 5 1 12 12 6 4 0 12 7 4 9 3")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW03", "QLD02") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 2 4 7 5 3 8 2 5 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 8 5 7 4 6 1 8 4 2 7 8 6 1 0 7 4 8 6 3 9 7 1 6 3 0 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 1 0 9 1 5 2 6 8 1 7 3 4 0 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 0 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 12 7 9 1 6 11 11 11 8 3 7 1 6 0 5 8 2 6 9 10 1 12 7 4 6 0 8 2 9 0 8 6 3 10 7 1 8 12 4 6 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 1 9 0 12 2 1 8 6 2 1 5 3 8 7 4 6 2 9 12 0 7 1 2 5 8 0 3 6 1 4 7 0 12 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 4 3 8 10 7 2 5 0 3 4 12 6 10 3 5 8 11 11 0 6 7 10 4 3 8 7 2 6 3 5 7 12 4 5 9 6 4 7 12 12 12 6 7 10 4 2 8 5 3 9 4 5 6 12 3 4 9 6 4 7 11 11 1 8 2 9 5 4 7 0 12 6 3 5 7 2 9 4 5 1 7 3 2 9 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 3 5 1 0 12 4 8 3 0 5 10 9 7 12 12 8 2 5 3 6 0 8 12 9 5 1 3 8 7 3 8 0 4 5 10 1 6 7 3 0 9 6 3 8 1 11 11 11 9 3 8 4 2 6 7 12 5 9 3 8 0 7 9 2 6 10 5 1 6 3 8 12 12 12 1 6 4 7 5 2 6 3 8 4 9 5 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 3 8 6 1 4 11 11 7 5 0 6 8 3 12 0 6 9 0 4 7 2 3 4 12 12 12 8 9 4 2 0 7 4 5 6 3 0 12 12 8 10 3 5 0 12 12 12 6 9 0 5 1 6 4 7 8 3 12 12 12 6 7 11 11 11 3 5 10 1 5 0 4 12 12 8 4 9 2 7 10 3 4 2 9 5 1 12 12 6 4 0 12 7 4 9 3")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL), new Money(200UL)), Requirement.Create("Percentage", "NSW03") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 2 4 7 5 3 8 2 5 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 8 5 7 4 6 1 8 4 2 7 8 6 1 0 7 4 8 6 3 9 7 1 6 3 0 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 1 0 9 1 5 2 6 8 1 7 3 4 0 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 0 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 12 7 9 1 6 11 11 11 8 3 7 1 6 0 5 8 2 6 9 10 1 12 7 4 6 0 8 2 9 0 8 6 3 10 7 1 8 12 4 6 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 1 9 0 12 2 1 8 6 2 1 5 3 8 7 4 6 2 9 12 0 7 1 2 5 8 0 3 6 1 4 7 0 12 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 4 3 8 10 7 2 5 0 3 4 12 6 10 3 5 8 11 11 0 6 7 10 4 3 8 7 2 6 3 5 7 12 4 5 9 6 4 7 12 12 12 6 7 10 4 2 8 5 3 9 4 5 6 12 3 4 9 6 4 7 11 11 1 8 2 9 5 4 7 0 12 6 3 5 7 2 9 4 5 1 7 3 2 9 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 3 5 1 0 12 4 7 3 0 5 10 9 7 12 12 8 2 5 3 6 0 12 12 9 5 1 4 8 7 3 8 0 4 5 10 1 6 7 3 0 9 6 3 8 1 11 11 11 9 3 8 4 2 6 7 12 5 9 3 8 0 7 9 2 6 10 5 1 6 3 8 12 12 12 1 6 4 7 5 2 6 3 8 4 9 5 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 3 8 6 1 4 11 11 7 5 0 6 8 3 12 0 6 9 0 4 7 2 3 4 12 12 12 8 9 4 2 0 7 4 5 6 3 0 12 12 8 10 3 5 0 12 12 12 6 9 0 5 1 6 4 7 8 3 12 12 12 6 7 11 11 11 3 5 10 1 5 0 4 12 12 8 4 9 2 7 10 3 4 2 9 5 1 12 12 6 4 0 12 7 4 9 3")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "QLD01") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 2 4 7 5 3 8 2 5 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 8 5 7 4 6 1 8 4 2 7 8 6 1 0 7 4 8 6 3 9 0 1 6 3 0 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 1 0 9 1 5 2 6 8 1 7 3 4 0 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 0 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 12 7 9 1 6 11 11 11 8 3 4 1 6 0 5 8 2 3 9 10 1 12 7 4 6 0 8 2 9 0 8 6 3 10 7 1 8 12 4 6 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 1 9 0 12 2 1 8 6 2 1 5 3 8 7 4 6 2 9 12 0 7 1 2 5 8 0 3 6 1 4 7 0 12 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 4 3 8 10 7 2 5 0 3 4 12 6 10 3 5 8 11 11 0 6 7 10 4 3 8 7 2 6 3 5 7 12 4 5 9 6 4 3 12 12 12 6 7 10 4 2 8 5 3 9 4 5 6 12 3 4 9 6 4 5 11 11 1 8 2 9 5 4 2 0 12 6 3 5 7 2 9 4 8 1 7 3 2 9 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 3 8 1 0 12 4 8 3 0 5 10 9 1 12 12 8 2 5 3 6 0 12 12 9 5 1 3 8 7 3 8 0 4 5 10 1 6 7 3 0 4 6 3 8 1 11 11 11 7 3 8 4 2 1 3 12 5 9 3 8 0 7 9 2 4 10 5 1 6 3 8 12 12 12 1 6 4 7 5 2 6 3 8 4 9 5 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 3 8 6 1 4 11 11 7 9 0 6 8 3 12 0 6 9 0 4 7 2 3 4 12 12 12 8 9 4 2 0 7 4 5 6 3 0 12 12 8 10 3 5 0 12 12 12 6 2 0 9 1 6 4 7 8 3 12 12 12 6 8 11 11 11 3 8 10 1 5 0 4 12 12 8 4 9 2 8 10 3 4 2 9 5 1 12 12 6 4 0 12 7 4 9 3")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "QLD03") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 2 4 7 5 3 8 2 5 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 8 5 7 4 6 1 8 4 2 7 8 6 1 0 7 4 8 6 3 9 7 1 6 3 0 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 1 0 9 1 5 2 6 8 1 7 3 4 0 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 0 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 12 7 9 1 6 11 11 11 8 3 7 1 6 0 5 8 2 6 9 10 1 12 7 4 6 0 8 2 9 0 8 6 3 10 7 1 8 12 4 6 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 1 9 0 12 2 1 8 6 2 1 5 3 8 7 4 6 2 9 12 0 7 1 2 5 8 0 3 6 1 4 7 0 12 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 4 3 8 10 7 2 5 0 3 4 12 6 10 3 5 8 11 11 0 6 7 10 4 3 8 7 2 6 3 5 7 12 4 5 9 6 4 7 12 12 12 6 7 10 4 2 8 5 3 9 4 5 6 12 3 4 9 6 4 7 11 11 1 8 2 9 5 4 7 0 12 6 3 5 7 2 9 4 5 1 7 3 2 9 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 3 5 1 0 12 4 7 3 0 5 10 9 7 12 12 8 2 5 3 6 0 12 12 9 5 1 3 8 7 3 8 0 4 5 10 1 6 7 3 0 9 6 3 8 1 11 11 11 9 3 8 4 2 6 7 12 5 9 3 8 0 7 9 2 6 10 5 1 6 3 8 12 12 12 1 6 4 7 5 2 6 3 8 4 9 5 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 3 8 6 1 4 11 11 7 5 0 6 8 3 12 0 6 9 0 4 7 2 3 4 12 12 12 8 9 4 2 0 7 4 5 6 3 0 12 12 8 10 3 5 0 12 12 12 6 9 0 5 1 6 4 7 8 3 12 12 12 6 7 11 11 11 3 5 10 1 5 0 4 12 12 8 4 9 2 7 10 3 4 2 9 5 1 12 12 6 4 0 12 7 4 9 3")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL), new Money(200UL)), Requirement.Create("Percentage", "QLD01") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 2 4 7 5 3 8 2 5 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 8 5 7 4 6 1 8 4 2 7 8 6 1 0 7 4 8 6 3 9 0 1 6 3 0 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 1 0 9 1 5 2 6 8 1 7 3 4 0 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 0 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 12 7 9 1 6 11 11 11 8 3 4 1 6 0 5 8 2 3 9 10 1 12 7 4 6 0 8 2 9 0 8 6 3 10 7 1 8 12 4 6 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 1 9 0 12 2 1 8 6 2 1 5 3 8 7 4 6 2 9 12 0 7 1 2 5 8 0 3 6 1 4 7 0 12 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 4 3 8 10 7 2 5 0 3 4 12 6 10 3 5 8 11 11 0 6 7 10 4 3 8 7 2 6 3 5 7 12 4 5 9 6 4 3 12 12 12 6 7 10 4 2 8 5 3 9 4 5 6 12 3 4 9 6 4 5 11 11 1 8 2 9 8 4 2 0 12 6 3 5 7 2 9 4 8 1 7 3 2 9 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 3 8 1 0 12 4 8 3 0 5 10 9 1 12 12 8 2 5 3 6 0 12 12 9 5 1 3 8 7 3 8 0 4 5 10 1 6 7 3 0 4 6 3 8 1 11 11 11 9 3 8 4 2 1 3 12 5 9 3 8 0 7 9 2 4 10 5 1 6 3 8 12 12 12 1 6 4 7 5 2 6 3 8 4 9 5 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 3 8 6 1 7 11 11 8 9 0 6 8 3 12 0 6 9 0 4 7 2 3 6 12 12 12 8 9 4 2 0 7 4 5 6 3 0 12 12 8 10 3 5 0 12 12 12 6 2 0 9 1 6 4 7 8 3 12 12 12 6 8 11 11 11 3 8 10 1 5 0 4 12 12 8 4 9 2 8 10 3 4 2 9 5 1 12 12 6 4 0 12 7 4 9 3")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL), new Money(200UL)), Requirement.Create("Percentage", "QLD02") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 2 4 7 5 3 8 2 5 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 8 5 7 4 6 1 8 4 2 7 8 6 1 0 7 4 8 6 3 9 0 1 6 3 0 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 1 0 9 1 5 2 6 8 1 7 3 4 0 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 0 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 12 7 9 1 6 11 11 11 8 3 4 1 6 0 5 8 2 3 9 10 1 12 7 4 6 0 8 2 9 0 8 6 3 10 7 1 8 12 4 6 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 1 9 0 12 7 1 8 6 2 1 5 3 8 7 4 6 2 9 12 0 7 1 2 5 8 0 3 6 1 4 7 0 12 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 4 3 8 10 7 2 5 0 3 4 12 6 10 3 5 8 11 11 0 6 7 10 4 3 8 7 2 6 3 5 7 12 4 5 9 6 4 7 12 12 12 6 7 10 4 2 8 5 3 9 4 5 6 12 3 4 9 6 4 2 11 11 1 8 2 9 5 4 7 0 12 6 3 5 7 2 9 4 8 1 7 3 2 9 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 3 8 1 0 12 4 8 3 0 5 10 9 6 12 12 8 2 5 3 6 0 12 12 9 5 1 3 8 7 3 8 0 4 5 10 1 6 9 3 0 4 6 3 8 1 11 11 11 9 3 8 4 2 7 3 12 5 9 3 8 0 7 9 2 4 10 5 1 6 3 8 12 12 12 1 6 4 7 5 2 6 3 8 4 9 5 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 3 8 6 1 7 11 11 7 9 0 6 8 3 12 0 6 9 0 4 7 2 3 6 12 12 12 8 9 4 2 0 7 4 5 6 3 0 12 12 8 10 3 5 0 12 12 12 6 2 0 9 1 6 4 7 8 3 12 12 12 6 8 11 11 11 3 8 10 1 5 0 4 12 12 8 4 9 2 8 10 3 4 2 9 5 1 12 12 6 4 0 12 7 4 9 3")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL), new Money(200UL)), Requirement.Create("Percentage", "QLD03") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 2 4 7 5 3 8 2 5 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 8 5 7 4 6 1 8 4 2 7 8 6 1 0 7 4 8 6 3 9 0 1 6 3 5 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 1 0 9 1 5 2 6 8 1 7 3 4 0 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 0 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 12 7 9 1 6 11 11 11 8 3 4 1 6 0 5 8 2 3 9 10 1 12 7 4 6 0 8 2 9 0 8 6 3 10 7 1 8 12 4 6 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 5 9 0 12 7 1 8 6 2 1 5 3 8 7 4 6 2 9 12 0 7 1 2 5 8 0 3 6 1 4 7 0 12 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 4 3 8 10 7 2 5 0 3 4 12 6 10 3 5 8 11 11 0 6 7 10 4 3 8 7 2 6 3 5 7 12 4 5 9 6 4 7 12 12 12 6 7 10 4 2 8 5 3 9 4 5 6 12 3 7 9 6 4 2 11 11 1 8 2 9 5 4 7 0 12 6 3 5 7 2 9 4 8 1 7 3 2 9 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 3 8 1 0 12 4 8 3 0 5 10 9 7 12 12 8 2 5 3 6 0 12 12 9 5 1 3 8 7 3 9 0 4 5 10 1 6 7 9 0 4 6 3 8 1 11 11 11 9 3 5 4 2 7 3 12 5 9 3 8 0 7 9 2 4 10 5 1 6 3 7 12 12 12 1 6 4 7 5 2 6 3 8 4 9 5 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 3 8 6 1 4 11 11 7 9 0 6 8 3 12 0 6 9 0 4 7 2 3 6 12 12 12 8 9 4 2 0 7 4 5 9 3 0 12 12 8 10 3 5 0 12 12 12 6 2 8 9 1 6 4 7 8 3 12 12 12 6 8 11 11 11 3 8 10 1 5 0 4 12 12 8 4 9 2 8 10 3 4 2 9 5 1 12 12 6 4 0 12 7 4 9 3")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(1UL), new Money(2UL), new Money(5UL), new Money(10UL)), Requirement.Create("Percentage", "CAS01") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 0 4 7 5 3 8 2 1 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 1 5 7 4 6 1 8 4 2 7 8 6 0 4 7 4 8 6 3 9 0 5 6 3 0 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 7 0 9 1 5 2 6 8 1 7 3 4 2 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 3 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 2 7 9 1 6 11 11 11 8 3 4 12 6 0 5 8 2 3 7 10 1 6 7 4 6 12 8 2 9 0 8 6 3 10 7 1 8 5 4 3 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 1 9 0 12 2 1 8 6 2 1 5 3 6 7 4 6 2 9 5 0 7 1 2 5 8 4 3 6 1 4 7 0 6 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 1 3 8 10 7 2 5 0 3 4 12 6 10 3 0 7 11 11 0 6 7 10 4 3 8 5 2 0 3 5 7 12 4 1 9 6 4 3 12 12 12 6 7 10 4 0 8 5 3 9 4 5 6 12 1 4 9 6 4 8 11 11 1 6 2 9 5 4 2 0 12 6 3 0 7 2 9 0 8 1 7 0 2 5 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 0 6 12 12 12 4 8 3 0 5 10 9 1 12 12 12 8 5 3 6 0 12 12 12 5 1 0 8 7 3 8 0 4 5 10 1 0 7 12 12 4 6 3 8 1 11 11 11 9 3 7 4 2 1 0 12 5 9 3 8 0 7 9 2 4 10 5 1 6 3 8 12 12 12 1 6 4 7 0 2 6 3 8 4 9 5 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 1 8 7 2 4 11 11 9 3 0 6 12 12 12 7 0 9 5 4 7 6 3 4 12 12 12 8 9 4 2 6 1 4 5 9 3 12 12 12 8 10 3 5 9 12 12 12 8 2 0 9 1 6 4 7 9 2 12 12 12 6 8 11 11 11 3 8 10 1 5 0 4 12 12 6 4 9 2 8 10 3 0 2 7 5 1 12 12 6 4 0 12 7 4 9 3")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 2 4 7 5 3 8 2 5 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 8 5 7 4 6 1 8 4 2 7 8 6 1 0 7 4 8 6 3 9 0 1 6 3 0 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 1 0 9 1 5 2 6 8 1 7 3 4 0 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 0 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 12 7 9 1 6 11 11 11 8 3 4 1 6 0 5 8 2 3 9 10 1 12 7 4 6 0 8 2 9 0 8 6 3 10 7 1 8 12 4 6 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 1 9 0 12 2 1 8 6 2 1 5 3 8 7 4 6 2 9 12 0 7 1 2 5 8 0 3 6 1 4 7 0 12 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 4 3 8 10 7 2 5 0 3 4 12 6 10 3 7 8 11 11 0 6 7 10 4 3 8 7 2 6 3 5 7 12 4 5 9 6 4 3 12 12 12 6 7 10 4 2 8 5 3 9 4 7 6 12 3 4 9 6 4 5 11 11 1 8 2 9 5 4 2 0 12 6 3 5 7 2 9 4 8 1 7 3 2 9 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 3 7 1 0 12 4 8 3 0 5 10 9 1 12 12 8 2 5 3 6 0 12 12 9 5 1 3 8 7 3 8 0 4 5 10 1 6 7 3 0 4 6 3 8 1 11 11 11 9 3 7 4 2 1 3 12 5 9 3 8 0 7 9 2 4 10 5 1 6 3 8 12 12 12 1 6 4 7 5 2 6 3 8 4 9 5 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 3 8 6 1 4 11 11 7 9 0 6 8 3 12 0 6 9 0 4 7 2 3 4 12 12 12 8 9 4 2 0 7 4 5 6 3 0 12 12 8 10 3 9 0 12 12 12 6 2 0 9 1 6 4 7 8 3 12 12 12 6 8 11 11 11 3 8 10 1 5 0 4 12 12 7 4 9 2 8 10 3 6 2 9 5 1 12 12 6 4 0 12 7 4 9 3")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 2 4 7 5 3 8 2 5 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 8 5 7 4 6 1 8 4 2 7 8 6 1 0 7 4 8 6 3 9 0 1 6 3 0 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 1 0 9 1 5 2 6 8 1 7 3 4 0 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 0 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 12 7 9 1 6 11 11 11 8 3 4 1 6 0 5 8 2 3 9 10 1 12 7 4 6 0 8 2 9 0 8 6 3 10 7 1 8 12 4 6 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 1 9 0 12 2 1 8 6 2 1 5 3 8 7 4 6 2 9 12 0 7 1 2 5 8 0 3 6 1 4 7 0 12 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 4 3 8 10 7 2 5 0 3 4 12 6 10 3 7 8 11 11 0 6 7 10 4 3 8 7 2 6 3 5 7 12 4 5 9 6 4 3 12 12 12 6 7 10 4 2 8 5 3 9 4 7 6 12 3 4 0 6 4 5 11 11 1 8 2 9 5 4 2 0 12 6 3 5 7 2 9 4 8 1 7 3 2 9 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 3 7 1 0 12 4 9 3 0 5 10 9 1 12 12 7 2 5 3 6 0 12 12 9 5 1 3 0 7 3 8 0 4 5 10 1 6 7 3 0 4 6 3 8 1 11 11 11 9 3 8 4 2 1 3 12 5 9 3 8 0 7 9 2 4 10 5 1 6 3 8 12 12 12 1 6 4 7 5 2 6 3 8 4 9 5 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 3 8 6 1 4 11 11 7 5 0 6 8 3 12 0 6 9 0 4 7 2 3 4 12 12 12 8 9 4 2 0 7 4 5 6 3 0 12 12 8 10 3 9 0 12 12 12 6 2 0 9 1 6 4 7 8 3 12 12 12 6 8 11 11 11 3 8 10 1 5 0 4 12 12 7 4 9 2 8 10 3 6 2 9 5 1 12 12 6 4 0 12 7 4 9 3")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 2 4 7 5 3 8 2 5 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 8 5 7 4 6 1 8 4 2 7 8 6 1 0 7 4 8 6 3 9 0 1 6 3 0 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 1 0 9 1 5 2 6 8 1 7 3 4 0 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 0 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 12 7 9 1 6 11 11 11 8 3 4 1 6 0 5 8 2 3 9 10 1 12 7 4 6 0 8 2 9 0 8 6 3 10 7 1 8 12 4 6 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 1 9 0 12 2 1 8 6 2 1 5 3 8 7 4 6 2 9 12 0 7 1 2 5 8 0 3 6 1 4 7 0 12 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 4 3 8 10 7 2 5 0 3 4 12 6 10 3 7 8 11 11 0 6 7 10 4 3 8 7 2 6 3 5 7 12 4 5 9 6 4 3 12 12 12 6 7 10 4 2 8 5 3 9 4 7 6 12 3 4 9 6 4 5 11 11 1 8 2 9 5 4 2 0 12 6 3 5 7 2 9 4 8 1 7 3 2 9 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 3 7 1 0 12 4 9 3 0 5 10 9 1 12 12 8 2 5 3 6 0 12 12 9 5 1 3 8 7 3 8 0 4 5 10 1 6 7 3 0 4 6 3 5 1 11 11 11 9 3 8 4 2 1 3 12 5 9 3 8 0 7 9 2 4 10 5 1 6 3 8 12 12 12 1 6 4 7 5 2 6 3 8 4 9 5 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 3 8 6 1 4 11 11 7 9 0 6 8 3 12 0 6 9 0 4 7 2 3 4 12 12 12 8 9 4 2 0 7 4 5 6 3 0 12 12 8 10 3 9 0 12 12 12 6 2 0 9 1 6 4 7 8 3 12 12 12 6 8 11 11 11 3 8 10 1 5 0 4 12 12 7 4 9 2 8 10 3 6 2 9 5 1 12 12 6 4 0 12 7 4 9 3")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(1UL), new Money(2UL), new Money(5UL), new Money(10UL)), Requirement.Create("Percentage", "CAS02") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 0 4 7 5 3 8 2 1 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 1 5 7 4 6 1 8 4 2 7 8 6 0 4 7 4 8 6 3 9 0 5 6 3 0 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 7 0 9 1 5 2 6 8 1 7 3 4 2 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 3 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 2 7 9 1 6 11 11 11 8 3 4 12 6 0 5 8 2 3 7 10 1 6 7 4 6 12 8 2 9 0 8 6 3 10 7 1 8 5 4 3 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 1 9 0 12 2 1 8 6 2 1 5 3 6 7 4 6 2 9 5 0 7 1 2 5 8 4 3 6 1 4 7 0 6 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 1 3 8 10 7 2 5 0 3 4 12 6 10 3 0 7 11 11 0 6 7 10 4 3 8 5 2 0 3 5 7 12 4 1 9 6 4 8 12 12 12 6 7 10 4 0 8 5 3 9 4 5 6 12 1 4 9 6 4 2 11 11 1 6 2 9 5 4 7 0 12 6 3 0 7 2 9 0 8 1 7 0 2 5 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 0 6 12 12 12 4 8 3 7 5 10 9 1 12 12 12 7 8 3 6 0 12 12 12 5 1 0 8 7 3 8 0 4 5 10 1 0 7 12 12 4 6 3 8 1 11 11 11 9 3 8 4 2 7 0 12 5 8 3 6 0 7 9 2 4 10 5 1 6 3 8 12 12 12 1 6 4 7 0 2 6 3 8 4 9 5 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 1 8 7 2 4 11 11 9 3 0 6 12 12 12 7 0 8 5 4 7 6 3 4 12 12 12 8 9 4 2 6 1 4 5 9 3 12 12 12 8 10 3 5 9 12 12 12 8 2 0 9 1 6 4 7 9 2 12 12 12 6 8 11 11 11 3 9 10 1 5 0 4 12 12 6 4 9 2 8 10 3 0 2 7 5 1 12 12 6 4 0 12 7 4 9 3")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS02") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 2 4 7 5 3 8 2 5 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 8 5 7 4 6 1 8 4 2 7 8 6 1 0 7 4 8 6 3 9 0 1 6 3 0 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 1 0 9 1 5 2 6 8 1 7 3 4 0 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 0 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 12 7 9 1 6 11 11 11 8 3 7 1 6 0 5 8 2 3 9 10 1 12 7 4 6 0 8 2 9 0 8 6 3 10 7 1 8 12 4 6 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 1 9 0 12 2 1 8 6 2 1 5 3 8 7 4 6 2 9 12 0 7 1 2 5 8 0 3 6 1 4 7 0 12 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 4 3 8 10 7 2 5 0 3 4 12 6 10 3 5 7 11 11 0 6 7 10 4 3 8 7 2 6 3 5 7 12 4 5 9 6 4 7 12 12 12 6 7 10 4 2 8 5 3 9 4 5 6 12 3 4 9 6 4 2 11 11 1 8 2 9 5 4 7 0 12 6 3 5 7 2 9 4 5 1 7 3 2 9 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 3 5 1 0 12 4 7 3 0 5 10 9 1 12 12 7 2 5 3 6 0 12 12 9 5 1 3 8 7 3 8 0 4 5 10 1 6 7 3 0 4 6 3 8 1 11 11 11 9 3 8 4 2 6 3 12 5 9 3 8 0 7 9 2 4 10 5 1 6 3 8 12 12 12 1 6 4 7 5 2 6 3 8 4 9 5 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 3 8 6 1 4 11 11 7 5 0 6 8 3 12 0 6 8 0 4 7 2 3 4 12 12 12 8 6 4 2 0 7 4 5 6 3 0 12 12 8 10 3 5 0 12 12 12 6 2 0 9 1 6 4 7 8 3 12 12 12 6 8 11 11 11 3 5 10 1 5 0 4 12 12 8 4 9 2 8 10 3 4 2 9 5 1 12 12 6 4 0 12 7 4 9 3")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS02") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 2 4 7 5 3 8 2 5 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 8 5 7 4 6 1 8 4 2 7 8 6 1 0 7 4 8 6 3 9 0 1 6 3 0 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 1 0 9 1 5 2 6 8 1 7 3 4 0 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 0 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 12 7 9 1 6 11 11 11 8 3 7 1 6 0 5 8 2 3 9 10 1 12 7 4 6 0 8 2 9 0 8 6 3 10 7 1 8 12 4 6 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 1 9 0 12 2 1 8 6 2 1 5 3 8 7 4 6 2 9 12 0 7 1 2 5 8 0 3 6 1 4 7 0 12 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 4 3 8 10 7 2 5 0 3 4 12 6 10 3 5 7 11 11 0 6 7 10 4 3 8 7 2 6 3 5 7 12 4 5 0 6 4 7 12 12 12 6 7 10 4 2 8 5 3 9 4 5 6 12 3 4 9 6 4 2 11 11 1 8 2 9 5 4 7 0 12 6 3 5 7 2 9 4 5 1 7 3 2 9 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 3 5 1 0 12 4 7 3 0 5 10 9 1 12 12 7 2 5 3 6 0 12 12 6 5 1 3 8 6 3 8 0 4 5 10 1 6 7 3 0 4 6 3 8 1 11 11 11 9 3 8 4 2 6 3 12 5 9 3 8 0 7 9 2 4 10 5 1 6 3 8 12 12 12 1 6 4 7 5 2 6 3 8 4 9 5 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 3 8 6 1 4 11 11 7 5 0 6 8 3 12 0 6 9 0 4 7 2 3 4 12 12 12 8 9 4 2 0 7 4 5 6 3 0 12 12 8 10 3 5 0 12 12 12 6 2 0 9 1 6 4 7 8 3 12 12 12 6 8 11 11 11 3 5 10 1 5 0 4 12 12 8 4 9 2 8 10 3 4 2 9 5 1 12 12 6 4 0 12 7 4 9 3")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS02") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 2 4 7 5 3 8 2 5 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 8 5 7 4 6 1 8 4 2 7 8 6 1 0 7 4 8 6 3 9 0 1 6 3 0 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 1 0 9 1 5 2 6 8 1 7 3 4 0 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 0 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 12 7 9 1 6 11 11 11 8 3 7 1 6 0 5 8 2 3 9 10 1 12 7 4 6 0 8 2 9 0 8 6 3 10 7 1 8 12 4 6 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 1 9 0 12 2 1 8 6 2 1 5 3 8 7 4 6 2 9 12 0 7 1 2 5 8 0 3 6 1 4 7 0 12 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 4 3 8 10 7 2 5 0 3 4 12 6 10 3 5 7 11 11 0 6 7 10 4 3 8 7 2 6 3 5 7 12 4 5 9 6 4 7 12 12 12 6 7 10 4 2 8 5 3 9 4 5 6 12 3 4 9 6 4 2 11 11 1 8 2 9 5 4 7 0 12 6 3 5 7 2 9 4 5 1 7 3 2 9 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 3 5 1 0 12 4 7 3 0 5 10 9 1 12 12 7 2 5 3 6 0 12 12 9 5 1 3 8 7 3 8 0 4 5 10 1 6 7 3 0 4 6 3 8 1 11 11 11 9 3 8 4 2 6 3 12 5 9 3 8 0 7 9 2 4 10 5 1 6 3 8 12 12 12 1 6 4 7 5 2 6 3 8 4 9 5 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 3 8 6 1 4 11 11 7 5 0 6 8 3 12 0 6 9 0 4 7 2 3 4 12 12 12 8 9 4 2 0 7 4 5 6 3 0 12 12 8 10 3 5 0 12 12 12 6 2 0 9 1 6 4 7 8 3 12 12 12 6 8 11 11 11 3 5 10 1 5 0 4 12 12 8 4 9 2 8 10 3 4 2 9 5 1 12 12 6 4 0 12 7 4 9 3")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(1UL), new Money(2UL), new Money(5UL), new Money(10UL)), Requirement.Create("Percentage", "CAS03") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 0 4 7 5 3 8 2 1 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 2 5 7 4 6 1 8 4 2 7 8 6 0 5 7 4 8 6 3 9 0 5 6 3 0 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 7 5 9 1 5 2 6 8 1 7 3 4 2 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 3 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 2 7 9 1 6 11 11 11 8 3 4 12 6 0 5 8 2 3 7 10 1 6 7 4 6 12 8 2 9 0 8 6 3 10 7 1 8 5 4 5 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 1 9 0 12 2 1 8 6 2 1 5 3 6 7 4 6 2 9 5 0 7 1 2 5 8 4 3 6 1 4 7 0 6 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 1 3 8 10 7 2 5 0 3 4 12 6 10 3 0 7 11 11 0 6 7 10 4 3 8 5 2 0 3 5 7 12 4 1 9 6 4 8 12 12 12 6 7 10 4 0 8 5 3 9 4 5 6 12 1 4 9 6 4 2 11 11 1 6 2 9 5 4 7 0 12 6 3 0 7 2 9 0 8 1 7 0 2 5 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 0 6 12 12 12 4 9 3 7 5 10 9 1 12 12 12 8 5 3 6 0 12 12 12 5 1 0 8 7 3 6 0 9 5 10 1 0 7 12 12 4 6 3 8 1 11 11 11 9 3 8 4 2 7 0 12 5 8 3 6 0 7 9 2 4 10 5 1 6 3 8 12 12 12 1 6 4 7 0 2 6 3 8 4 9 5 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 1 8 7 2 4 11 11 9 3 0 6 12 12 12 7 0 9 5 4 7 6 3 4 12 12 12 8 9 4 2 6 1 4 5 7 3 12 12 12 8 10 3 5 6 12 12 12 8 2 0 9 1 6 4 7 9 2 12 12 12 6 8 11 11 11 3 9 10 1 5 0 4 12 12 6 4 9 2 8 10 3 0 2 7 5 1 12 12 6 4 0 12 7 4 9 3")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 2 4 7 5 3 8 2 5 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 8 5 7 4 6 1 8 4 2 7 8 6 1 0 7 4 8 6 3 9 7 1 6 3 0 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 1 0 9 1 5 2 6 8 1 7 3 4 0 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 0 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 12 7 9 1 6 11 11 11 8 3 7 1 6 0 5 8 2 6 9 10 1 12 7 4 6 0 8 2 9 0 8 6 3 10 7 1 8 12 4 6 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 1 9 0 12 2 1 8 6 2 1 5 3 8 7 4 6 2 9 12 0 7 1 2 5 8 0 3 6 1 4 7 0 12 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 4 3 8 10 7 2 5 0 3 4 12 6 10 3 5 7 11 11 0 6 7 10 4 3 8 7 2 6 3 5 7 12 4 5 9 6 4 7 12 12 12 6 7 10 4 2 8 5 3 9 4 5 6 12 3 4 9 6 4 7 11 11 1 8 2 9 5 4 7 0 12 6 3 5 7 2 9 4 5 1 7 3 2 9 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 3 5 1 0 12 4 6 3 0 5 10 9 7 12 12 8 2 5 3 6 0 12 12 9 5 1 3 8 7 3 8 0 4 5 10 1 6 7 3 0 9 6 3 8 1 11 11 11 9 3 8 4 2 6 7 12 5 9 3 8 0 5 9 2 6 10 5 1 6 3 9 12 12 12 1 6 4 7 5 2 6 3 8 4 0 5 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 3 8 6 1 4 11 11 7 5 0 6 8 3 12 0 6 9 0 4 7 2 3 4 12 12 12 8 9 4 2 0 7 4 5 6 3 0 12 12 8 10 3 5 0 12 12 12 6 9 0 5 1 6 4 7 8 3 12 12 12 6 7 11 11 11 3 5 10 1 5 0 4 12 12 8 4 9 2 7 10 3 4 2 9 8 1 12 12 6 4 0 12 7 4 9 3")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 2 4 7 5 3 8 2 5 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 8 5 7 4 6 1 8 4 2 7 8 6 1 0 7 4 8 6 3 9 7 1 6 3 0 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 1 0 9 1 5 2 6 8 1 7 3 4 0 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 0 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 12 7 9 1 6 11 11 11 8 3 7 1 6 0 5 8 2 6 9 10 1 12 7 4 6 0 8 2 9 0 8 6 3 10 7 1 8 12 4 6 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 1 9 0 12 2 1 8 6 2 1 5 3 8 7 4 6 2 9 12 0 7 1 2 5 8 0 3 6 1 4 7 0 12 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 4 3 8 10 7 2 5 0 3 4 12 6 10 3 5 7 11 11 0 6 7 10 4 3 8 7 2 6 3 5 7 12 4 5 9 6 4 7 12 12 12 6 7 10 4 2 8 5 3 9 4 5 6 12 3 4 9 6 4 7 11 11 1 8 2 9 5 4 7 0 12 6 3 5 7 2 9 4 5 1 7 3 2 0 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 3 5 1 0 12 4 8 3 0 5 10 9 7 12 12 8 2 5 3 6 0 12 12 9 5 1 3 8 7 3 8 0 4 5 10 1 6 7 3 0 9 6 3 8 1 11 11 11 9 3 8 4 2 6 7 12 5 0 3 8 0 7 9 2 6 10 5 1 6 3 9 12 12 12 1 6 4 7 5 2 6 3 8 4 0 5 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 3 8 6 1 4 11 11 7 5 0 6 8 3 12 0 6 9 0 4 7 2 3 4 12 12 12 8 9 4 2 0 7 4 5 6 3 0 12 12 8 10 3 5 0 12 12 12 6 9 0 5 1 6 4 7 8 3 12 12 12 6 7 11 11 11 3 5 10 1 5 0 4 12 12 8 4 9 2 7 10 3 4 2 9 5 1 12 12 6 4 0 12 7 4 9 3")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03") },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 3 2 4 7 5 3 8 2 5 6 4 3 11 11 11 5 3 10 9 4 11 11 8 2 3 7 4 8 5 7 4 6 1 8 4 2 7 8 6 1 0 7 4 8 6 3 9 7 1 6 3 0 7 2 9 0 8 3 1 5 4 6 1 2 4 8 3 1 0 9 1 5 2 6 8 1 7 3 4 0 8 1 7 6 4 7 3 4 0 6 1 5 8 2 9 0 7 2 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 8 3 9 4 10 5 4 0 12 7 9 1 6 11 11 11 8 3 7 1 6 0 5 8 2 6 9 10 1 12 7 4 6 0 8 2 9 0 8 6 3 10 7 1 8 12 4 6 0 7 9 2 4 10 5 1 6 0 7 11 11 5 6 2 1 9 0 12 2 1 8 6 2 1 5 3 8 7 4 6 2 9 12 0 7 1 2 5 8 0 3 6 1 4 7 0 12 8 1 9"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 4 3 8 10 7 2 5 0 3 4 12 6 10 3 5 7 11 11 0 6 7 10 4 3 8 7 2 6 3 5 7 12 4 5 9 6 4 7 12 12 12 6 7 10 4 2 8 5 3 9 4 5 6 12 3 4 9 6 4 7 11 11 1 8 2 9 5 4 7 0 12 6 3 5 7 2 9 4 5 1 7 3 2 9 8 4 6 1 3 5 2 3 7 0 6 4 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 3 0 2 10 1 8 11 11 4 9 3 5 1 0 12 4 8 3 0 5 10 9 7 12 12 8 2 5 3 6 0 12 12 9 5 1 3 8 7 3 8 0 4 5 10 1 6 7 3 0 9 6 3 8 1 11 11 11 9 3 8 4 2 6 7 12 5 9 3 8 0 7 9 2 6 10 5 1 6 3 8 12 12 12 1 6 4 7 5 2 6 3 8 4 9 5 7 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 3 8 6 1 4 11 11 7 5 0 6 8 3 12 0 6 9 0 4 7 2 3 4 12 12 12 8 9 4 2 0 7 4 5 6 3 0 12 12 8 10 3 5 0 12 12 12 6 9 0 5 1 6 4 7 8 3 12 12 12 6 7 11 11 11 3 5 10 1 5 0 4 12 12 8 4 9 2 7 10 3 4 2 9 5 1 12 12 6 4 0 12 7 4 9 3")
						}
					)
				}
			);
		}

		private static WeightedSets CreateExcelWeightedChoiceSelectatron()
		{
			return new WeightedSets(
				new Dictionary<string, WeightedSet>()
				{
					{ "1150CAS01", new WeightedSet(new Money(1UL), 1L, new Credits(50UL), "CAS01", 710403UL, 289597UL) },
					{ "1250CAS01", new WeightedSet(new Money(1UL), 2L, new Credits(50UL), "CAS01", 711797UL, 288203UL) },
					{ "1350CAS01", new WeightedSet(new Money(1UL), 3L, new Credits(50UL), "CAS01", 709014UL, 290986UL) },
					{ "1550CAS01", new WeightedSet(new Money(1UL), 5L, new Credits(50UL), "CAS01", 706745UL, 293255UL) },
					{ "11050CAS01", new WeightedSet(new Money(1UL), 10L, new Credits(50UL), "CAS01", 708883UL, 291117UL) },
					{ "2150CAS01", new WeightedSet(new Money(2UL), 1L, new Credits(50UL), "CAS01", 696410UL, 303590UL) },
					{ "2250CAS01", new WeightedSet(new Money(2UL), 2L, new Credits(50UL), "CAS01", 696828UL, 303172UL) },
					{ "2350CAS01", new WeightedSet(new Money(2UL), 3L, new Credits(50UL), "CAS01", 694021UL, 305979UL) },
					{ "2550CAS01", new WeightedSet(new Money(2UL), 5L, new Credits(50UL), "CAS01", 691605UL, 308395UL) },
					{ "21050CAS01", new WeightedSet(new Money(2UL), 10L, new Credits(50UL), "CAS01", 695896UL, 304104UL) },
					{ "5125CAS01", new WeightedSet(new Money(5UL), 1L, new Credits(25UL), "CAS01", 692601UL, 307399UL) },
					{ "5225CAS01", new WeightedSet(new Money(5UL), 2L, new Credits(25UL), "CAS01", 702855UL, 297145UL) },
					{ "5325CAS01", new WeightedSet(new Money(5UL), 3L, new Credits(25UL), "CAS01", 702483UL, 297517UL) },
					{ "5525CAS01", new WeightedSet(new Money(5UL), 5L, new Credits(25UL), "CAS01", 712672UL, 287328UL) },
					{ "51025CAS01", new WeightedSet(new Money(5UL), 10L, new Credits(25UL), "CAS01", 737636UL, 262364UL) },
					{ "10125CAS01", new WeightedSet(new Money(10UL), 1L, new Credits(25UL), "CAS01", 680707UL, 319293UL) },
					{ "10225CAS01", new WeightedSet(new Money(10UL), 2L, new Credits(25UL), "CAS01", 689175UL, 310825UL) },
					{ "10325CAS01", new WeightedSet(new Money(10UL), 3L, new Credits(25UL), "CAS01", 686873UL, 313127UL) },
					{ "10525CAS01", new WeightedSet(new Money(10UL), 5L, new Credits(25UL), "CAS01", 694358UL, 305642UL) },
					{ "101025CAS01", new WeightedSet(new Money(10UL), 10L, new Credits(25UL), "CAS01", 721576UL, 278424UL) },
					{ "5011CAS01", new WeightedSet(new Money(50UL), 1L, new Credits(1UL), "CAS01", 652583UL, 347417UL) },
					{ "5012CAS01", new WeightedSet(new Money(50UL), 1L, new Credits(2UL), "CAS01", 662035UL, 337965UL) },
					{ "5013CAS01", new WeightedSet(new Money(50UL), 1L, new Credits(3UL), "CAS01", 660814UL, 339186UL) },
					{ "5014CAS01", new WeightedSet(new Money(50UL), 1L, new Credits(4UL), "CAS01", 669001UL, 330999UL) },
					{ "5015CAS01", new WeightedSet(new Money(50UL), 1L, new Credits(5UL), "CAS01", 667762UL, 332238UL) },
					{ "5021CAS01", new WeightedSet(new Money(50UL), 2L, new Credits(1UL), "CAS01", 662035UL, 337965UL) },
					{ "5022CAS01", new WeightedSet(new Money(50UL), 2L, new Credits(2UL), "CAS01", 639551UL, 360449UL) },
					{ "5023CAS01", new WeightedSet(new Money(50UL), 2L, new Credits(3UL), "CAS01", 658299UL, 341701UL) },
					{ "5024CAS01", new WeightedSet(new Money(50UL), 2L, new Credits(4UL), "CAS01", 697552UL, 302448UL) },
					{ "5025CAS01", new WeightedSet(new Money(50UL), 2L, new Credits(5UL), "CAS01", 684727UL, 315273UL) },
					{ "5031CAS01", new WeightedSet(new Money(50UL), 3L, new Credits(1UL), "CAS01", 652377UL, 347623UL) },
					{ "5032CAS01", new WeightedSet(new Money(50UL), 3L, new Credits(2UL), "CAS01", 649918UL, 350082UL) },
					{ "5033CAS01", new WeightedSet(new Money(50UL), 3L, new Credits(3UL), "CAS01", 647459UL, 352541UL) },
					{ "5034CAS01", new WeightedSet(new Money(50UL), 3L, new Credits(4UL), "CAS01", 644999UL, 355001UL) },
					{ "5035CAS01", new WeightedSet(new Money(50UL), 3L, new Credits(5UL), "CAS01", 690910UL, 309090UL) },
					{ "5051CAS01", new WeightedSet(new Money(50UL), 5L, new Credits(1UL), "CAS01", 688298UL, 311702UL) },
					{ "5052CAS01", new WeightedSet(new Money(50UL), 5L, new Credits(2UL), "CAS01", 645702UL, 354298UL) },
					{ "5053CAS01", new WeightedSet(new Money(50UL), 5L, new Credits(3UL), "CAS01", 664088UL, 335912UL) },
					{ "5054CAS01", new WeightedSet(new Money(50UL), 5L, new Credits(4UL), "CAS01", 651741UL, 348259UL) },
					{ "5055CAS01", new WeightedSet(new Money(50UL), 5L, new Credits(5UL), "CAS01", 676022UL, 323978UL) },
					{ "50101CAS01", new WeightedSet(new Money(50UL), 10L, new Credits(1UL), "CAS01", 653987UL, 346013UL) },
					{ "50102CAS01", new WeightedSet(new Money(50UL), 10L, new Credits(2UL), "CAS01", 683808UL, 316192UL) },
					{ "50103CAS01", new WeightedSet(new Money(50UL), 10L, new Credits(3UL), "CAS01", 611104UL, 388896UL) },
					{ "50104CAS01", new WeightedSet(new Money(50UL), 10L, new Credits(4UL), "CAS01", 637956UL, 362044UL) },
					{ "50105CAS01", new WeightedSet(new Money(50UL), 10L, new Credits(5UL), "CAS01", 620010UL, 379990UL) },
					{ "10011CAS01", new WeightedSet(new Money(100UL), 1L, new Credits(1UL), "CAS01", 699711UL, 300289UL) },
					{ "10012CAS01", new WeightedSet(new Money(100UL), 1L, new Credits(2UL), "CAS01", 676929UL, 323071UL) },
					{ "10013CAS01", new WeightedSet(new Money(100UL), 1L, new Credits(3UL), "CAS01", 690406UL, 309594UL) },
					{ "10014CAS01", new WeightedSet(new Money(100UL), 1L, new Credits(4UL), "CAS01", 699584UL, 300416UL) },
					{ "10015CAS01", new WeightedSet(new Money(100UL), 1L, new Credits(5UL), "CAS01", 697978UL, 302022UL) },
					{ "10021CAS01", new WeightedSet(new Money(100UL), 2L, new Credits(1UL), "CAS01", 676929UL, 323071UL) },
					{ "10022CAS01", new WeightedSet(new Money(100UL), 2L, new Credits(2UL), "CAS01", 678213UL, 321787UL) },
					{ "10023CAS01", new WeightedSet(new Money(100UL), 2L, new Credits(3UL), "CAS01", 670179UL, 329821UL) },
					{ "10024CAS01", new WeightedSet(new Money(100UL), 2L, new Credits(4UL), "CAS01", 706683UL, 293317UL) },
					{ "10025CAS01", new WeightedSet(new Money(100UL), 2L, new Credits(5UL), "CAS01", 681378UL, 318622UL) },
					{ "10031CAS01", new WeightedSet(new Money(100UL), 3L, new Credits(1UL), "CAS01", 674808UL, 325192UL) },
					{ "10032CAS01", new WeightedSet(new Money(100UL), 3L, new Credits(2UL), "CAS01", 663657UL, 336343UL) },
					{ "10033CAS01", new WeightedSet(new Money(100UL), 3L, new Credits(3UL), "CAS01", 681363UL, 318637UL) },
					{ "10034CAS01", new WeightedSet(new Money(100UL), 3L, new Credits(4UL), "CAS01", 703010UL, 296990UL) },
					{ "10035CAS01", new WeightedSet(new Money(100UL), 3L, new Credits(5UL), "CAS01", 715974UL, 284026UL) },
					{ "10051CAS01", new WeightedSet(new Money(100UL), 5L, new Credits(1UL), "CAS01", 716374UL, 283626UL) },
					{ "10052CAS01", new WeightedSet(new Money(100UL), 5L, new Credits(2UL), "CAS01", 689449UL, 310551UL) },
					{ "10053CAS01", new WeightedSet(new Money(100UL), 5L, new Credits(3UL), "CAS01", 690171UL, 309829UL) },
					{ "10054CAS01", new WeightedSet(new Money(100UL), 5L, new Credits(4UL), "CAS01", 691881UL, 308119UL) },
					{ "10055CAS01", new WeightedSet(new Money(100UL), 5L, new Credits(5UL), "CAS01", 709900UL, 290100UL) },
					{ "100101CAS01", new WeightedSet(new Money(100UL), 10L, new Credits(1UL), "CAS01", 661992UL, 338008UL) },
					{ "100102CAS01", new WeightedSet(new Money(100UL), 10L, new Credits(2UL), "CAS01", 684914UL, 315086UL) },
					{ "100103CAS01", new WeightedSet(new Money(100UL), 10L, new Credits(3UL), "CAS01", 719084UL, 280916UL) },
					{ "100104CAS01", new WeightedSet(new Money(100UL), 10L, new Credits(4UL), "CAS01", 693273UL, 306727UL) },
					{ "100105CAS01", new WeightedSet(new Money(100UL), 10L, new Credits(5UL), "CAS01", 718492UL, 281508UL) },
					{ "20011CAS01", new WeightedSet(new Money(200UL), 1L, new Credits(1UL), "CAS01", 667253UL, 332747UL) },
					{ "20012CAS01", new WeightedSet(new Money(200UL), 1L, new Credits(2UL), "CAS01", 632737UL, 367263UL) },
					{ "20013CAS01", new WeightedSet(new Money(200UL), 1L, new Credits(3UL), "CAS01", 646864UL, 353136UL) },
					{ "20014CAS01", new WeightedSet(new Money(200UL), 1L, new Credits(4UL), "CAS01", 653221UL, 346779UL) },
					{ "20015CAS01", new WeightedSet(new Money(200UL), 1L, new Credits(5UL), "CAS01", 651389UL, 348611UL) },
					{ "20021CAS01", new WeightedSet(new Money(200UL), 2L, new Credits(1UL), "CAS01", 632161UL, 367839UL) },
					{ "20022CAS01", new WeightedSet(new Money(200UL), 2L, new Credits(2UL), "CAS01", 630348UL, 369652UL) },
					{ "20023CAS01", new WeightedSet(new Money(200UL), 2L, new Credits(3UL), "CAS01", 624539UL, 375461UL) },
					{ "20024CAS01", new WeightedSet(new Money(200UL), 2L, new Credits(4UL), "CAS01", 666122UL, 333878UL) },
					{ "20025CAS01", new WeightedSet(new Money(200UL), 2L, new Credits(5UL), "CAS01", 639394UL, 360606UL) },
					{ "20031CAS01", new WeightedSet(new Money(200UL), 3L, new Credits(1UL), "CAS01", 630398UL, 369602UL) },
					{ "20032CAS01", new WeightedSet(new Money(200UL), 3L, new Credits(2UL), "CAS01", 615940UL, 384060UL) },
					{ "20033CAS01", new WeightedSet(new Money(200UL), 3L, new Credits(3UL), "CAS01", 635069UL, 364931UL) },
					{ "20034CAS01", new WeightedSet(new Money(200UL), 3L, new Credits(4UL), "CAS01", 661175UL, 338825UL) },
					{ "20035CAS01", new WeightedSet(new Money(200UL), 3L, new Credits(5UL), "CAS01", 675401UL, 324599UL) },
					{ "20051CAS01", new WeightedSet(new Money(200UL), 5L, new Credits(1UL), "CAS01", 674290UL, 325710UL) },
					{ "20052CAS01", new WeightedSet(new Money(200UL), 5L, new Credits(2UL), "CAS01", 640809UL, 359191UL) },
					{ "20053CAS01", new WeightedSet(new Money(200UL), 5L, new Credits(3UL), "CAS01", 646617UL, 353383UL) },
					{ "20054CAS01", new WeightedSet(new Money(200UL), 5L, new Credits(4UL), "CAS01", 649525UL, 350475UL) },
					{ "20055CAS01", new WeightedSet(new Money(200UL), 5L, new Credits(5UL), "CAS01", 665485UL, 334515UL) },
					{ "200101CAS01", new WeightedSet(new Money(200UL), 10L, new Credits(1UL), "CAS01", 616056UL, 383944UL) },
					{ "200102CAS01", new WeightedSet(new Money(200UL), 10L, new Credits(2UL), "CAS01", 633358UL, 366642UL) },
					{ "200103CAS01", new WeightedSet(new Money(200UL), 10L, new Credits(3UL), "CAS01", 667639UL, 332361UL) },
					{ "200104CAS01", new WeightedSet(new Money(200UL), 10L, new Credits(4UL), "CAS01", 644759UL, 355241UL) },
					{ "200105CAS01", new WeightedSet(new Money(200UL), 10L, new Credits(5UL), "CAS01", 667080UL, 332920UL) },
					{ "1150CAS02", new WeightedSet(new Money(1UL), 1L, new Credits(50UL), "CAS02", 710403UL, 289597UL) },
					{ "1250CAS02", new WeightedSet(new Money(1UL), 2L, new Credits(50UL), "CAS02", 711797UL, 288203UL) },
					{ "1350CAS02", new WeightedSet(new Money(1UL), 3L, new Credits(50UL), "CAS02", 709014UL, 290986UL) },
					{ "1550CAS02", new WeightedSet(new Money(1UL), 5L, new Credits(50UL), "CAS02", 706745UL, 293255UL) },
					{ "11050CAS02", new WeightedSet(new Money(1UL), 10L, new Credits(50UL), "CAS02", 708883UL, 291117UL) },
					{ "2150CAS02", new WeightedSet(new Money(2UL), 1L, new Credits(50UL), "CAS02", 696410UL, 303590UL) },
					{ "2250CAS02", new WeightedSet(new Money(2UL), 2L, new Credits(50UL), "CAS02", 696828UL, 303172UL) },
					{ "2350CAS02", new WeightedSet(new Money(2UL), 3L, new Credits(50UL), "CAS02", 694021UL, 305979UL) },
					{ "2550CAS02", new WeightedSet(new Money(2UL), 5L, new Credits(50UL), "CAS02", 691605UL, 308395UL) },
					{ "21050CAS02", new WeightedSet(new Money(2UL), 10L, new Credits(50UL), "CAS02", 695896UL, 304104UL) },
					{ "5125CAS02", new WeightedSet(new Money(5UL), 1L, new Credits(25UL), "CAS02", 692601UL, 307399UL) },
					{ "5225CAS02", new WeightedSet(new Money(5UL), 2L, new Credits(25UL), "CAS02", 702855UL, 297145UL) },
					{ "5325CAS02", new WeightedSet(new Money(5UL), 3L, new Credits(25UL), "CAS02", 702483UL, 297517UL) },
					{ "5525CAS02", new WeightedSet(new Money(5UL), 5L, new Credits(25UL), "CAS02", 712672UL, 287328UL) },
					{ "51025CAS02", new WeightedSet(new Money(5UL), 10L, new Credits(25UL), "CAS02", 737636UL, 262364UL) },
					{ "10125CAS02", new WeightedSet(new Money(10UL), 1L, new Credits(25UL), "CAS02", 680707UL, 319293UL) },
					{ "10225CAS02", new WeightedSet(new Money(10UL), 2L, new Credits(25UL), "CAS02", 689175UL, 310825UL) },
					{ "10325CAS02", new WeightedSet(new Money(10UL), 3L, new Credits(25UL), "CAS02", 686873UL, 313127UL) },
					{ "10525CAS02", new WeightedSet(new Money(10UL), 5L, new Credits(25UL), "CAS02", 694358UL, 305642UL) },
					{ "101025CAS02", new WeightedSet(new Money(10UL), 10L, new Credits(25UL), "CAS02", 721576UL, 278424UL) },
					{ "5011CAS02", new WeightedSet(new Money(50UL), 1L, new Credits(1UL), "CAS02", 652583UL, 347417UL) },
					{ "5012CAS02", new WeightedSet(new Money(50UL), 1L, new Credits(2UL), "CAS02", 662035UL, 337965UL) },
					{ "5013CAS02", new WeightedSet(new Money(50UL), 1L, new Credits(3UL), "CAS02", 660814UL, 339186UL) },
					{ "5014CAS02", new WeightedSet(new Money(50UL), 1L, new Credits(4UL), "CAS02", 669001UL, 330999UL) },
					{ "5015CAS02", new WeightedSet(new Money(50UL), 1L, new Credits(5UL), "CAS02", 667762UL, 332238UL) },
					{ "5021CAS02", new WeightedSet(new Money(50UL), 2L, new Credits(1UL), "CAS02", 662035UL, 337965UL) },
					{ "5022CAS02", new WeightedSet(new Money(50UL), 2L, new Credits(2UL), "CAS02", 639551UL, 360449UL) },
					{ "5023CAS02", new WeightedSet(new Money(50UL), 2L, new Credits(3UL), "CAS02", 658299UL, 341701UL) },
					{ "5024CAS02", new WeightedSet(new Money(50UL), 2L, new Credits(4UL), "CAS02", 697552UL, 302448UL) },
					{ "5025CAS02", new WeightedSet(new Money(50UL), 2L, new Credits(5UL), "CAS02", 684727UL, 315273UL) },
					{ "5031CAS02", new WeightedSet(new Money(50UL), 3L, new Credits(1UL), "CAS02", 652377UL, 347623UL) },
					{ "5032CAS02", new WeightedSet(new Money(50UL), 3L, new Credits(2UL), "CAS02", 649918UL, 350082UL) },
					{ "5033CAS02", new WeightedSet(new Money(50UL), 3L, new Credits(3UL), "CAS02", 647459UL, 352541UL) },
					{ "5034CAS02", new WeightedSet(new Money(50UL), 3L, new Credits(4UL), "CAS02", 644999UL, 355001UL) },
					{ "5035CAS02", new WeightedSet(new Money(50UL), 3L, new Credits(5UL), "CAS02", 690910UL, 309090UL) },
					{ "5051CAS02", new WeightedSet(new Money(50UL), 5L, new Credits(1UL), "CAS02", 688298UL, 311702UL) },
					{ "5052CAS02", new WeightedSet(new Money(50UL), 5L, new Credits(2UL), "CAS02", 645702UL, 354298UL) },
					{ "5053CAS02", new WeightedSet(new Money(50UL), 5L, new Credits(3UL), "CAS02", 664088UL, 335912UL) },
					{ "5054CAS02", new WeightedSet(new Money(50UL), 5L, new Credits(4UL), "CAS02", 651741UL, 348259UL) },
					{ "5055CAS02", new WeightedSet(new Money(50UL), 5L, new Credits(5UL), "CAS02", 676022UL, 323978UL) },
					{ "50101CAS02", new WeightedSet(new Money(50UL), 10L, new Credits(1UL), "CAS02", 653987UL, 346013UL) },
					{ "50102CAS02", new WeightedSet(new Money(50UL), 10L, new Credits(2UL), "CAS02", 683808UL, 316192UL) },
					{ "50103CAS02", new WeightedSet(new Money(50UL), 10L, new Credits(3UL), "CAS02", 611104UL, 388896UL) },
					{ "50104CAS02", new WeightedSet(new Money(50UL), 10L, new Credits(4UL), "CAS02", 637956UL, 362044UL) },
					{ "50105CAS02", new WeightedSet(new Money(50UL), 10L, new Credits(5UL), "CAS02", 620010UL, 379990UL) },
					{ "10011CAS02", new WeightedSet(new Money(100UL), 1L, new Credits(1UL), "CAS02", 699711UL, 300289UL) },
					{ "10012CAS02", new WeightedSet(new Money(100UL), 1L, new Credits(2UL), "CAS02", 676929UL, 323071UL) },
					{ "10013CAS02", new WeightedSet(new Money(100UL), 1L, new Credits(3UL), "CAS02", 690406UL, 309594UL) },
					{ "10014CAS02", new WeightedSet(new Money(100UL), 1L, new Credits(4UL), "CAS02", 699584UL, 300416UL) },
					{ "10015CAS02", new WeightedSet(new Money(100UL), 1L, new Credits(5UL), "CAS02", 697978UL, 302022UL) },
					{ "10021CAS02", new WeightedSet(new Money(100UL), 2L, new Credits(1UL), "CAS02", 676929UL, 323071UL) },
					{ "10022CAS02", new WeightedSet(new Money(100UL), 2L, new Credits(2UL), "CAS02", 678213UL, 321787UL) },
					{ "10023CAS02", new WeightedSet(new Money(100UL), 2L, new Credits(3UL), "CAS02", 670179UL, 329821UL) },
					{ "10024CAS02", new WeightedSet(new Money(100UL), 2L, new Credits(4UL), "CAS02", 706683UL, 293317UL) },
					{ "10025CAS02", new WeightedSet(new Money(100UL), 2L, new Credits(5UL), "CAS02", 681378UL, 318622UL) },
					{ "10031CAS02", new WeightedSet(new Money(100UL), 3L, new Credits(1UL), "CAS02", 674808UL, 325192UL) },
					{ "10032CAS02", new WeightedSet(new Money(100UL), 3L, new Credits(2UL), "CAS02", 663657UL, 336343UL) },
					{ "10033CAS02", new WeightedSet(new Money(100UL), 3L, new Credits(3UL), "CAS02", 681363UL, 318637UL) },
					{ "10034CAS02", new WeightedSet(new Money(100UL), 3L, new Credits(4UL), "CAS02", 703010UL, 296990UL) },
					{ "10035CAS02", new WeightedSet(new Money(100UL), 3L, new Credits(5UL), "CAS02", 715974UL, 284026UL) },
					{ "10051CAS02", new WeightedSet(new Money(100UL), 5L, new Credits(1UL), "CAS02", 716374UL, 283626UL) },
					{ "10052CAS02", new WeightedSet(new Money(100UL), 5L, new Credits(2UL), "CAS02", 689449UL, 310551UL) },
					{ "10053CAS02", new WeightedSet(new Money(100UL), 5L, new Credits(3UL), "CAS02", 690171UL, 309829UL) },
					{ "10054CAS02", new WeightedSet(new Money(100UL), 5L, new Credits(4UL), "CAS02", 691881UL, 308119UL) },
					{ "10055CAS02", new WeightedSet(new Money(100UL), 5L, new Credits(5UL), "CAS02", 709900UL, 290100UL) },
					{ "100101CAS02", new WeightedSet(new Money(100UL), 10L, new Credits(1UL), "CAS02", 661992UL, 338008UL) },
					{ "100102CAS02", new WeightedSet(new Money(100UL), 10L, new Credits(2UL), "CAS02", 684914UL, 315086UL) },
					{ "100103CAS02", new WeightedSet(new Money(100UL), 10L, new Credits(3UL), "CAS02", 719084UL, 280916UL) },
					{ "100104CAS02", new WeightedSet(new Money(100UL), 10L, new Credits(4UL), "CAS02", 693273UL, 306727UL) },
					{ "100105CAS02", new WeightedSet(new Money(100UL), 10L, new Credits(5UL), "CAS02", 718492UL, 281508UL) },
					{ "20011CAS02", new WeightedSet(new Money(200UL), 1L, new Credits(1UL), "CAS02", 667253UL, 332747UL) },
					{ "20012CAS02", new WeightedSet(new Money(200UL), 1L, new Credits(2UL), "CAS02", 632737UL, 367263UL) },
					{ "20013CAS02", new WeightedSet(new Money(200UL), 1L, new Credits(3UL), "CAS02", 646864UL, 353136UL) },
					{ "20014CAS02", new WeightedSet(new Money(200UL), 1L, new Credits(4UL), "CAS02", 653221UL, 346779UL) },
					{ "20015CAS02", new WeightedSet(new Money(200UL), 1L, new Credits(5UL), "CAS02", 651389UL, 348611UL) },
					{ "20021CAS02", new WeightedSet(new Money(200UL), 2L, new Credits(1UL), "CAS02", 632161UL, 367839UL) },
					{ "20022CAS02", new WeightedSet(new Money(200UL), 2L, new Credits(2UL), "CAS02", 630348UL, 369652UL) },
					{ "20023CAS02", new WeightedSet(new Money(200UL), 2L, new Credits(3UL), "CAS02", 624539UL, 375461UL) },
					{ "20024CAS02", new WeightedSet(new Money(200UL), 2L, new Credits(4UL), "CAS02", 666122UL, 333878UL) },
					{ "20025CAS02", new WeightedSet(new Money(200UL), 2L, new Credits(5UL), "CAS02", 639394UL, 360606UL) },
					{ "20031CAS02", new WeightedSet(new Money(200UL), 3L, new Credits(1UL), "CAS02", 630398UL, 369602UL) },
					{ "20032CAS02", new WeightedSet(new Money(200UL), 3L, new Credits(2UL), "CAS02", 615940UL, 384060UL) },
					{ "20033CAS02", new WeightedSet(new Money(200UL), 3L, new Credits(3UL), "CAS02", 635069UL, 364931UL) },
					{ "20034CAS02", new WeightedSet(new Money(200UL), 3L, new Credits(4UL), "CAS02", 661175UL, 338825UL) },
					{ "20035CAS02", new WeightedSet(new Money(200UL), 3L, new Credits(5UL), "CAS02", 675401UL, 324599UL) },
					{ "20051CAS02", new WeightedSet(new Money(200UL), 5L, new Credits(1UL), "CAS02", 674290UL, 325710UL) },
					{ "20052CAS02", new WeightedSet(new Money(200UL), 5L, new Credits(2UL), "CAS02", 640809UL, 359191UL) },
					{ "20053CAS02", new WeightedSet(new Money(200UL), 5L, new Credits(3UL), "CAS02", 646617UL, 353383UL) },
					{ "20054CAS02", new WeightedSet(new Money(200UL), 5L, new Credits(4UL), "CAS02", 649525UL, 350475UL) },
					{ "20055CAS02", new WeightedSet(new Money(200UL), 5L, new Credits(5UL), "CAS02", 665485UL, 334515UL) },
					{ "200101CAS02", new WeightedSet(new Money(200UL), 10L, new Credits(1UL), "CAS02", 616056UL, 383944UL) },
					{ "200102CAS02", new WeightedSet(new Money(200UL), 10L, new Credits(2UL), "CAS02", 633358UL, 366642UL) },
					{ "200103CAS02", new WeightedSet(new Money(200UL), 10L, new Credits(3UL), "CAS02", 667639UL, 332361UL) },
					{ "200104CAS02", new WeightedSet(new Money(200UL), 10L, new Credits(4UL), "CAS02", 644759UL, 355241UL) },
					{ "200105CAS02", new WeightedSet(new Money(200UL), 10L, new Credits(5UL), "CAS02", 667080UL, 332920UL) },
					{ "1150CAS03", new WeightedSet(new Money(1UL), 1L, new Credits(50UL), "CAS03", 727790UL, 272210UL) },
					{ "1250CAS03", new WeightedSet(new Money(1UL), 2L, new Credits(50UL), "CAS03", 729282UL, 270718UL) },
					{ "1350CAS03", new WeightedSet(new Money(1UL), 3L, new Credits(50UL), "CAS03", 725979UL, 274021UL) },
					{ "1550CAS03", new WeightedSet(new Money(1UL), 5L, new Credits(50UL), "CAS03", 723733UL, 276267UL) },
					{ "11050CAS03", new WeightedSet(new Money(1UL), 10L, new Credits(50UL), "CAS03", 724930UL, 275070UL) },
					{ "2150CAS03", new WeightedSet(new Money(2UL), 1L, new Credits(50UL), "CAS03", 713793UL, 286207UL) },
					{ "2250CAS03", new WeightedSet(new Money(2UL), 2L, new Credits(50UL), "CAS03", 714755UL, 285245UL) },
					{ "2350CAS03", new WeightedSet(new Money(2UL), 3L, new Credits(50UL), "CAS03", 711424UL, 288576UL) },
					{ "2550CAS03", new WeightedSet(new Money(2UL), 5L, new Credits(50UL), "CAS03", 708579UL, 291421UL) },
					{ "21050CAS03", new WeightedSet(new Money(2UL), 10L, new Credits(50UL), "CAS03", 710612UL, 289388UL) },
					{ "5125CAS03", new WeightedSet(new Money(5UL), 1L, new Credits(25UL), "CAS03", 709912UL, 290088UL) },
					{ "5225CAS03", new WeightedSet(new Money(5UL), 2L, new Credits(25UL), "CAS03", 720523UL, 279477UL) },
					{ "5325CAS03", new WeightedSet(new Money(5UL), 3L, new Credits(25UL), "CAS03", 719650UL, 280350UL) },
					{ "5525CAS03", new WeightedSet(new Money(5UL), 5L, new Credits(25UL), "CAS03", 729876UL, 270124UL) },
					{ "51025CAS03", new WeightedSet(new Money(5UL), 10L, new Credits(25UL), "CAS03", 753317UL, 246683UL) },
					{ "10125CAS03", new WeightedSet(new Money(10UL), 1L, new Credits(25UL), "CAS03", 698011UL, 301989UL) },
					{ "10225CAS03", new WeightedSet(new Money(10UL), 2L, new Credits(25UL), "CAS03", 708155UL, 291845UL) },
					{ "10325CAS03", new WeightedSet(new Money(10UL), 3L, new Credits(25UL), "CAS03", 707104UL, 292896UL) },
					{ "10525CAS03", new WeightedSet(new Money(10UL), 5L, new Credits(25UL), "CAS03", 716517UL, 283483UL) },
					{ "101025CAS03", new WeightedSet(new Money(10UL), 10L, new Credits(25UL), "CAS03", 740595UL, 259405UL) },
					{ "5011CAS03", new WeightedSet(new Money(50UL), 1L, new Credits(1UL), "CAS03", 670630UL, 329370UL) },
					{ "5012CAS03", new WeightedSet(new Money(50UL), 1L, new Credits(2UL), "CAS03", 686209UL, 313791UL) },
					{ "5013CAS03", new WeightedSet(new Money(50UL), 1L, new Credits(3UL), "CAS03", 684984UL, 315016UL) },
					{ "5014CAS03", new WeightedSet(new Money(50UL), 1L, new Credits(4UL), "CAS03", 693512UL, 306488UL) },
					{ "5015CAS03", new WeightedSet(new Money(50UL), 1L, new Credits(5UL), "CAS03", 692269UL, 307731UL) },
					{ "5021CAS03", new WeightedSet(new Money(50UL), 2L, new Credits(1UL), "CAS03", 686209UL, 313791UL) },
					{ "5022CAS03", new WeightedSet(new Money(50UL), 2L, new Credits(2UL), "CAS03", 662963UL, 337037UL) },
					{ "5023CAS03", new WeightedSet(new Money(50UL), 2L, new Credits(3UL), "CAS03", 682459UL, 317541UL) },
					{ "5024CAS03", new WeightedSet(new Money(50UL), 2L, new Credits(4UL), "CAS03", 718813UL, 281187UL) },
					{ "5025CAS03", new WeightedSet(new Money(50UL), 2L, new Credits(5UL), "CAS03", 706181UL, 293819UL) },
					{ "5031CAS03", new WeightedSet(new Money(50UL), 3L, new Credits(1UL), "CAS03", 673343UL, 326657UL) },
					{ "5032CAS03", new WeightedSet(new Money(50UL), 3L, new Credits(2UL), "CAS03", 671385UL, 328615UL) },
					{ "5033CAS03", new WeightedSet(new Money(50UL), 3L, new Credits(3UL), "CAS03", 669428UL, 330572UL) },
					{ "5034CAS03", new WeightedSet(new Money(50UL), 3L, new Credits(4UL), "CAS03", 667470UL, 332530UL) },
					{ "5035CAS03", new WeightedSet(new Money(50UL), 3L, new Credits(5UL), "CAS03", 713052UL, 286948UL) },
					{ "5051CAS03", new WeightedSet(new Money(50UL), 5L, new Credits(1UL), "CAS03", 708499UL, 291501UL) },
					{ "5052CAS03", new WeightedSet(new Money(50UL), 5L, new Credits(2UL), "CAS03", 670658UL, 329342UL) },
					{ "5053CAS03", new WeightedSet(new Money(50UL), 5L, new Credits(3UL), "CAS03", 685900UL, 314100UL) },
					{ "5054CAS03", new WeightedSet(new Money(50UL), 5L, new Credits(4UL), "CAS03", 672457UL, 327543UL) },
					{ "5055CAS03", new WeightedSet(new Money(50UL), 5L, new Credits(5UL), "CAS03", 697248UL, 302752UL) },
					{ "50101CAS03", new WeightedSet(new Money(50UL), 10L, new Credits(1UL), "CAS03", 678132UL, 321868UL) },
					{ "50102CAS03", new WeightedSet(new Money(50UL), 10L, new Credits(2UL), "CAS03", 649544UL, 350456UL) },
					{ "50103CAS03", new WeightedSet(new Money(50UL), 10L, new Credits(3UL), "CAS03", 635176UL, 364824UL) },
					{ "50104CAS03", new WeightedSet(new Money(50UL), 10L, new Credits(4UL), "CAS03", 659985UL, 340015UL) },
					{ "50105CAS03", new WeightedSet(new Money(50UL), 10L, new Credits(5UL), "CAS03", 642712UL, 357288UL) },
					{ "10011CAS03", new WeightedSet(new Money(100UL), 1L, new Credits(1UL), "CAS03", 715854UL, 284146UL) },
					{ "10012CAS03", new WeightedSet(new Money(100UL), 1L, new Credits(2UL), "CAS03", 698793UL, 301207UL) },
					{ "10013CAS03", new WeightedSet(new Money(100UL), 1L, new Credits(3UL), "CAS03", 711835UL, 288165UL) },
					{ "10014CAS03", new WeightedSet(new Money(100UL), 1L, new Credits(4UL), "CAS03", 722283UL, 277717UL) },
					{ "10015CAS03", new WeightedSet(new Money(100UL), 1L, new Credits(5UL), "CAS03", 720669UL, 279331UL) },
					{ "10021CAS03", new WeightedSet(new Money(100UL), 2L, new Credits(1UL), "CAS03", 698793UL, 301207UL) },
					{ "10022CAS03", new WeightedSet(new Money(100UL), 2L, new Credits(2UL), "CAS03", 701348UL, 298652UL) },
					{ "10023CAS03", new WeightedSet(new Money(100UL), 2L, new Credits(3UL), "CAS03", 692013UL, 307987UL) },
					{ "10024CAS03", new WeightedSet(new Money(100UL), 2L, new Credits(4UL), "CAS03", 725891UL, 274109UL) },
					{ "10025CAS03", new WeightedSet(new Money(100UL), 2L, new Credits(5UL), "CAS03", 701061UL, 298939UL) },
					{ "10031CAS03", new WeightedSet(new Money(100UL), 3L, new Credits(1UL), "CAS03", 696664UL, 303336UL) },
					{ "10032CAS03", new WeightedSet(new Money(100UL), 3L, new Credits(2UL), "CAS03", 686776UL, 313224UL) },
					{ "10033CAS03", new WeightedSet(new Money(100UL), 3L, new Credits(3UL), "CAS03", 703175UL, 296825UL) },
					{ "10034CAS03", new WeightedSet(new Money(100UL), 3L, new Credits(4UL), "CAS03", 722193UL, 277807UL) },
					{ "10035CAS03", new WeightedSet(new Money(100UL), 3L, new Credits(5UL), "CAS03", 734091UL, 265909UL) },
					{ "10051CAS03", new WeightedSet(new Money(100UL), 5L, new Credits(1UL), "CAS03", 736330UL, 263670UL) },
					{ "10052CAS03", new WeightedSet(new Money(100UL), 5L, new Credits(2UL), "CAS03", 711928UL, 288072UL) },
					{ "10053CAS03", new WeightedSet(new Money(100UL), 5L, new Credits(3UL), "CAS03", 709800UL, 290200UL) },
					{ "10054CAS03", new WeightedSet(new Money(100UL), 5L, new Credits(4UL), "CAS03", 710505UL, 289495UL) },
					{ "10055CAS03", new WeightedSet(new Money(100UL), 5L, new Credits(5UL), "CAS03", 728965UL, 271035UL) },
					{ "100101CAS03", new WeightedSet(new Money(100UL), 10L, new Credits(1UL), "CAS03", 683798UL, 316202UL) },
					{ "100102CAS03", new WeightedSet(new Money(100UL), 10L, new Credits(2UL), "CAS03", 707926UL, 292074UL) },
					{ "100103CAS03", new WeightedSet(new Money(100UL), 10L, new Credits(3UL), "CAS03", 740744UL, 259256UL) },
					{ "100104CAS03", new WeightedSet(new Money(100UL), 10L, new Credits(4UL), "CAS03", 712595UL, 287405UL) },
					{ "100105CAS03", new WeightedSet(new Money(100UL), 10L, new Credits(5UL), "CAS03", 737911UL, 262089UL) },
					{ "20011CAS03", new WeightedSet(new Money(200UL), 1L, new Credits(1UL), "CAS03", 683390UL, 316610UL) },
					{ "20012CAS03", new WeightedSet(new Money(200UL), 1L, new Credits(2UL), "CAS03", 654586UL, 345414UL) },
					{ "20013CAS03", new WeightedSet(new Money(200UL), 1L, new Credits(3UL), "CAS03", 668271UL, 331729UL) },
					{ "20014CAS03", new WeightedSet(new Money(200UL), 1L, new Credits(4UL), "CAS03", 675889UL, 324111UL) },
					{ "20015CAS03", new WeightedSet(new Money(200UL), 1L, new Credits(5UL), "CAS03", 674042UL, 325958UL) },
					{ "20021CAS03", new WeightedSet(new Money(200UL), 2L, new Credits(1UL), "CAS03", 654586UL, 345414UL) },
					{ "20022CAS03", new WeightedSet(new Money(200UL), 2L, new Credits(2UL), "CAS03", 654062UL, 345938UL) },
					{ "20023CAS03", new WeightedSet(new Money(200UL), 2L, new Credits(3UL), "CAS03", 646905UL, 353095UL) },
					{ "20024CAS03", new WeightedSet(new Money(200UL), 2L, new Credits(4UL), "CAS03", 685786UL, 314214UL) },
					{ "20025CAS03", new WeightedSet(new Money(200UL), 2L, new Credits(5UL), "CAS03", 659531UL, 340469UL) },
					{ "20031CAS03", new WeightedSet(new Money(200UL), 3L, new Credits(1UL), "CAS03", 652232UL, 347768UL) },
					{ "20032CAS03", new WeightedSet(new Money(200UL), 3L, new Credits(2UL), "CAS03", 639013UL, 360987UL) },
					{ "20033CAS03", new WeightedSet(new Money(200UL), 3L, new Credits(3UL), "CAS03", 657392UL, 342608UL) },
					{ "20034CAS03", new WeightedSet(new Money(200UL), 3L, new Credits(4UL), "CAS03", 681294UL, 318706UL) },
					{ "20035CAS03", new WeightedSet(new Money(200UL), 3L, new Credits(5UL), "CAS03", 694864UL, 305136UL) },
					{ "20051CAS03", new WeightedSet(new Money(200UL), 5L, new Credits(1UL), "CAS03", 694752UL, 305248UL) },
					{ "20052CAS03", new WeightedSet(new Money(200UL), 5L, new Credits(2UL), "CAS03", 664430UL, 335570UL) },
					{ "20053CAS03", new WeightedSet(new Money(200UL), 5L, new Credits(3UL), "CAS03", 667744UL, 332256UL) },
					{ "20054CAS03", new WeightedSet(new Money(200UL), 5L, new Credits(4UL), "CAS03", 670048UL, 329952UL) },
					{ "20055CAS03", new WeightedSet(new Money(200UL), 5L, new Credits(5UL), "CAS03", 686983UL, 313017UL) },
					{ "200101CAS03", new WeightedSet(new Money(200UL), 10L, new Credits(1UL), "CAS03", 638365UL, 361635UL) },
					{ "200102CAS03", new WeightedSet(new Money(200UL), 10L, new Credits(2UL), "CAS03", 657434UL, 342566UL) },
					{ "200103CAS03", new WeightedSet(new Money(200UL), 10L, new Credits(3UL), "CAS03", 690807UL, 309193UL) },
					{ "200104CAS03", new WeightedSet(new Money(200UL), 10L, new Credits(4UL), "CAS03", 665881UL, 334119UL) },
					{ "200105CAS03", new WeightedSet(new Money(200UL), 10L, new Credits(5UL), "CAS03", 688769UL, 311231UL) },
					{ "1150NSW01", new WeightedSet(new Money(1UL), 1L, new Credits(50UL), "NSW01", 710403UL, 289597UL) },
					{ "1250NSW01", new WeightedSet(new Money(1UL), 2L, new Credits(50UL), "NSW01", 711797UL, 288203UL) },
					{ "1350NSW01", new WeightedSet(new Money(1UL), 3L, new Credits(50UL), "NSW01", 709014UL, 290986UL) },
					{ "1550NSW01", new WeightedSet(new Money(1UL), 5L, new Credits(50UL), "NSW01", 706745UL, 293255UL) },
					{ "1850NSW01", new WeightedSet(new Money(1UL), 8L, new Credits(50UL), "NSW01", 706607UL, 293393UL) },
					{ "11050NSW01", new WeightedSet(new Money(1UL), 10L, new Credits(50UL), "NSW01", 708883UL, 291117UL) },
					{ "2150NSW01", new WeightedSet(new Money(2UL), 1L, new Credits(50UL), "NSW01", 696410UL, 303590UL) },
					{ "2250NSW01", new WeightedSet(new Money(2UL), 2L, new Credits(50UL), "NSW01", 696828UL, 303172UL) },
					{ "2350NSW01", new WeightedSet(new Money(2UL), 3L, new Credits(50UL), "NSW01", 694021UL, 305979UL) },
					{ "2450NSW01", new WeightedSet(new Money(2UL), 4L, new Credits(50UL), "NSW01", 675072UL, 324928UL) },
					{ "2550NSW01", new WeightedSet(new Money(2UL), 5L, new Credits(50UL), "NSW01", 690686UL, 309314UL) },
					{ "21050NSW01", new WeightedSet(new Money(2UL), 10L, new Credits(50UL), "NSW01", 691551UL, 308449UL) },
					{ "5125NSW01", new WeightedSet(new Money(5UL), 1L, new Credits(25UL), "NSW01", 708876UL, 291124UL) },
					{ "5225NSW01", new WeightedSet(new Money(5UL), 2L, new Credits(25UL), "NSW01", 710581UL, 289419UL) },
					{ "5325NSW01", new WeightedSet(new Money(5UL), 3L, new Credits(25UL), "NSW01", 684425UL, 315575UL) },
					{ "5425NSW01", new WeightedSet(new Money(5UL), 4L, new Credits(25UL), "NSW01", 700815UL, 299185UL) },
					{ "5625NSW01", new WeightedSet(new Money(5UL), 6L, new Credits(25UL), "NSW01", 718391UL, 281609UL) },
					{ "5825NSW01", new WeightedSet(new Money(5UL), 8L, new Credits(25UL), "NSW01", 711224UL, 288776UL) },
					{ "10125NSW01", new WeightedSet(new Money(10UL), 1L, new Credits(25UL), "NSW01", 693355UL, 306645UL) },
					{ "10225NSW01", new WeightedSet(new Money(10UL), 2L, new Credits(25UL), "NSW01", 694225UL, 305775UL) },
					{ "10325NSW01", new WeightedSet(new Money(10UL), 3L, new Credits(25UL), "NSW01", 673181UL, 326819UL) },
					{ "10425NSW01", new WeightedSet(new Money(10UL), 4L, new Credits(25UL), "NSW01", 734980UL, 265020UL) },
					{ "5011NSW01", new WeightedSet(new Money(50UL), 1L, new Credits(1UL), "NSW01", 658790UL, 341210UL) },
					{ "5012NSW01", new WeightedSet(new Money(50UL), 1L, new Credits(2UL), "NSW01", 674574UL, 325426UL) },
					{ "5013NSW01", new WeightedSet(new Money(50UL), 1L, new Credits(3UL), "NSW01", 659841UL, 340159UL) },
					{ "5014NSW01", new WeightedSet(new Money(50UL), 1L, new Credits(4UL), "NSW01", 681094UL, 318906UL) },
					{ "5015NSW01", new WeightedSet(new Money(50UL), 1L, new Credits(5UL), "NSW01", 652744UL, 347256UL) },
					{ "5021NSW01", new WeightedSet(new Money(50UL), 2L, new Credits(1UL), "NSW01", 673998UL, 326002UL) },
					{ "5022NSW01", new WeightedSet(new Money(50UL), 2L, new Credits(2UL), "NSW01", 650544UL, 349456UL) },
					{ "5023NSW01", new WeightedSet(new Money(50UL), 2L, new Credits(3UL), "NSW01", 669031UL, 330969UL) },
					{ "5024NSW01", new WeightedSet(new Money(50UL), 2L, new Credits(4UL), "NSW01", 700744UL, 299256UL) },
					{ "5025NSW01", new WeightedSet(new Money(50UL), 2L, new Credits(5UL), "NSW01", 669942UL, 330058UL) },
					{ "5031NSW01", new WeightedSet(new Money(50UL), 3L, new Credits(1UL), "NSW01", 678466UL, 321534UL) },
					{ "5032NSW01", new WeightedSet(new Money(50UL), 3L, new Credits(2UL), "NSW01", 649338UL, 350662UL) },
					{ "5033NSW01", new WeightedSet(new Money(50UL), 3L, new Credits(3UL), "NSW01", 646625UL, 353375UL) },
					{ "5034NSW01", new WeightedSet(new Money(50UL), 3L, new Credits(4UL), "NSW01", 643911UL, 356089UL) },
					{ "5035NSW01", new WeightedSet(new Money(50UL), 3L, new Credits(5UL), "NSW01", 641198UL, 358802UL) },
					{ "5041NSW01", new WeightedSet(new Money(50UL), 4L, new Credits(1UL), "NSW01", 700408UL, 299592UL) },
					{ "5042NSW01", new WeightedSet(new Money(50UL), 4L, new Credits(2UL), "NSW01", 673036UL, 326964UL) },
					{ "5043NSW01", new WeightedSet(new Money(50UL), 4L, new Credits(3UL), "NSW01", 693421UL, 306579UL) },
					{ "5044NSW01", new WeightedSet(new Money(50UL), 4L, new Credits(4UL), "NSW01", 676387UL, 323613UL) },
					{ "5045NSW01", new WeightedSet(new Money(50UL), 4L, new Credits(5UL), "NSW01", 682346UL, 317654UL) },
					{ "10011NSW01", new WeightedSet(new Money(100UL), 1L, new Credits(1UL), "NSW01", 661571UL, 338429UL) },
					{ "10012NSW01", new WeightedSet(new Money(100UL), 1L, new Credits(2UL), "NSW01", 673527UL, 326473UL) },
					{ "10013NSW01", new WeightedSet(new Money(100UL), 1L, new Credits(3UL), "NSW01", 689222UL, 310778UL) },
					{ "10014NSW01", new WeightedSet(new Money(100UL), 1L, new Credits(4UL), "NSW01", 703812UL, 296188UL) },
					{ "10015NSW01", new WeightedSet(new Money(100UL), 1L, new Credits(5UL), "NSW01", 692293UL, 307707UL) },
					{ "10021NSW01", new WeightedSet(new Money(100UL), 2L, new Credits(1UL), "NSW01", 674068UL, 325932UL) },
					{ "10022NSW01", new WeightedSet(new Money(100UL), 2L, new Credits(2UL), "NSW01", 695510UL, 304490UL) },
					{ "10023NSW01", new WeightedSet(new Money(100UL), 2L, new Credits(3UL), "NSW01", 689710UL, 310290UL) },
					{ "10024NSW01", new WeightedSet(new Money(100UL), 2L, new Credits(4UL), "NSW01", 713712UL, 286288UL) },
					{ "10025NSW01", new WeightedSet(new Money(100UL), 2L, new Credits(5UL), "NSW01", 673192UL, 326808UL) },
					{ "20011NSW01", new WeightedSet(new Money(200UL), 1L, new Credits(1UL), "NSW01", 683885UL, 316115UL) },
					{ "20012NSW01", new WeightedSet(new Money(200UL), 1L, new Credits(2UL), "NSW01", 702987UL, 297013UL) },
					{ "20013NSW01", new WeightedSet(new Money(200UL), 1L, new Credits(3UL), "NSW01", 703080UL, 296920UL) },
					{ "20014NSW01", new WeightedSet(new Money(200UL), 1L, new Credits(4UL), "NSW01", 708989UL, 291011UL) },
					{ "20015NSW01", new WeightedSet(new Money(200UL), 1L, new Credits(5UL), "NSW01", 722584UL, 277416UL) },
					{ "1150NSW02", new WeightedSet(new Money(1UL), 1L, new Credits(50UL), "NSW02", 710403UL, 289597UL) },
					{ "1250NSW02", new WeightedSet(new Money(1UL), 2L, new Credits(50UL), "NSW02", 711797UL, 288203UL) },
					{ "1350NSW02", new WeightedSet(new Money(1UL), 3L, new Credits(50UL), "NSW02", 709014UL, 290986UL) },
					{ "1550NSW02", new WeightedSet(new Money(1UL), 5L, new Credits(50UL), "NSW02", 706745UL, 293255UL) },
					{ "1850NSW02", new WeightedSet(new Money(1UL), 8L, new Credits(50UL), "NSW02", 706607UL, 293393UL) },
					{ "11050NSW02", new WeightedSet(new Money(1UL), 10L, new Credits(50UL), "NSW02", 708883UL, 291117UL) },
					{ "2150NSW02", new WeightedSet(new Money(2UL), 1L, new Credits(50UL), "NSW02", 696410UL, 303590UL) },
					{ "2250NSW02", new WeightedSet(new Money(2UL), 2L, new Credits(50UL), "NSW02", 696828UL, 303172UL) },
					{ "2350NSW02", new WeightedSet(new Money(2UL), 3L, new Credits(50UL), "NSW02", 694021UL, 305979UL) },
					{ "2450NSW02", new WeightedSet(new Money(2UL), 4L, new Credits(50UL), "NSW02", 675072UL, 324928UL) },
					{ "2550NSW02", new WeightedSet(new Money(2UL), 5L, new Credits(50UL), "NSW02", 690686UL, 309314UL) },
					{ "21050NSW02", new WeightedSet(new Money(2UL), 10L, new Credits(50UL), "NSW02", 691551UL, 308449UL) },
					{ "5125NSW02", new WeightedSet(new Money(5UL), 1L, new Credits(25UL), "NSW02", 708876UL, 291124UL) },
					{ "5225NSW02", new WeightedSet(new Money(5UL), 2L, new Credits(25UL), "NSW02", 710581UL, 289419UL) },
					{ "5325NSW02", new WeightedSet(new Money(5UL), 3L, new Credits(25UL), "NSW02", 684425UL, 315575UL) },
					{ "5425NSW02", new WeightedSet(new Money(5UL), 4L, new Credits(25UL), "NSW02", 700815UL, 299185UL) },
					{ "5625NSW02", new WeightedSet(new Money(5UL), 6L, new Credits(25UL), "NSW02", 718391UL, 281609UL) },
					{ "5825NSW02", new WeightedSet(new Money(5UL), 8L, new Credits(25UL), "NSW02", 711224UL, 288776UL) },
					{ "10125NSW02", new WeightedSet(new Money(10UL), 1L, new Credits(25UL), "NSW02", 693355UL, 306645UL) },
					{ "10225NSW02", new WeightedSet(new Money(10UL), 2L, new Credits(25UL), "NSW02", 694225UL, 305775UL) },
					{ "10325NSW02", new WeightedSet(new Money(10UL), 3L, new Credits(25UL), "NSW02", 673181UL, 326819UL) },
					{ "10425NSW02", new WeightedSet(new Money(10UL), 4L, new Credits(25UL), "NSW02", 734980UL, 265020UL) },
					{ "5011NSW02", new WeightedSet(new Money(50UL), 1L, new Credits(1UL), "NSW02", 658790UL, 341210UL) },
					{ "5012NSW02", new WeightedSet(new Money(50UL), 1L, new Credits(2UL), "NSW02", 674574UL, 325426UL) },
					{ "5013NSW02", new WeightedSet(new Money(50UL), 1L, new Credits(3UL), "NSW02", 659841UL, 340159UL) },
					{ "5014NSW02", new WeightedSet(new Money(50UL), 1L, new Credits(4UL), "NSW02", 681094UL, 318906UL) },
					{ "5015NSW02", new WeightedSet(new Money(50UL), 1L, new Credits(5UL), "NSW02", 652744UL, 347256UL) },
					{ "5021NSW02", new WeightedSet(new Money(50UL), 2L, new Credits(1UL), "NSW02", 673998UL, 326002UL) },
					{ "5022NSW02", new WeightedSet(new Money(50UL), 2L, new Credits(2UL), "NSW02", 650544UL, 349456UL) },
					{ "5023NSW02", new WeightedSet(new Money(50UL), 2L, new Credits(3UL), "NSW02", 669031UL, 330969UL) },
					{ "5024NSW02", new WeightedSet(new Money(50UL), 2L, new Credits(4UL), "NSW02", 700744UL, 299256UL) },
					{ "5025NSW02", new WeightedSet(new Money(50UL), 2L, new Credits(5UL), "NSW02", 669942UL, 330058UL) },
					{ "5031NSW02", new WeightedSet(new Money(50UL), 3L, new Credits(1UL), "NSW02", 678466UL, 321534UL) },
					{ "5032NSW02", new WeightedSet(new Money(50UL), 3L, new Credits(2UL), "NSW02", 649338UL, 350662UL) },
					{ "5033NSW02", new WeightedSet(new Money(50UL), 3L, new Credits(3UL), "NSW02", 646625UL, 353375UL) },
					{ "5034NSW02", new WeightedSet(new Money(50UL), 3L, new Credits(4UL), "NSW02", 643911UL, 356089UL) },
					{ "5035NSW02", new WeightedSet(new Money(50UL), 3L, new Credits(5UL), "NSW02", 641198UL, 358802UL) },
					{ "5041NSW02", new WeightedSet(new Money(50UL), 4L, new Credits(1UL), "NSW02", 700408UL, 299592UL) },
					{ "5042NSW02", new WeightedSet(new Money(50UL), 4L, new Credits(2UL), "NSW02", 673036UL, 326964UL) },
					{ "5043NSW02", new WeightedSet(new Money(50UL), 4L, new Credits(3UL), "NSW02", 693421UL, 306579UL) },
					{ "5044NSW02", new WeightedSet(new Money(50UL), 4L, new Credits(4UL), "NSW02", 676387UL, 323613UL) },
					{ "5045NSW02", new WeightedSet(new Money(50UL), 4L, new Credits(5UL), "NSW02", 682346UL, 317654UL) },
					{ "10011NSW02", new WeightedSet(new Money(100UL), 1L, new Credits(1UL), "NSW02", 661959UL, 338041UL) },
					{ "10012NSW02", new WeightedSet(new Money(100UL), 1L, new Credits(2UL), "NSW02", 674608UL, 325392UL) },
					{ "10013NSW02", new WeightedSet(new Money(100UL), 1L, new Credits(3UL), "NSW02", 690619UL, 309381UL) },
					{ "10014NSW02", new WeightedSet(new Money(100UL), 1L, new Credits(4UL), "NSW02", 705984UL, 294016UL) },
					{ "10015NSW02", new WeightedSet(new Money(100UL), 1L, new Credits(5UL), "NSW02", 694699UL, 305301UL) },
					{ "10021NSW02", new WeightedSet(new Money(100UL), 2L, new Credits(1UL), "NSW02", 674068UL, 325932UL) },
					{ "10022NSW02", new WeightedSet(new Money(100UL), 2L, new Credits(2UL), "NSW02", 696046UL, 303954UL) },
					{ "10023NSW02", new WeightedSet(new Money(100UL), 2L, new Credits(3UL), "NSW02", 690861UL, 309139UL) },
					{ "10024NSW02", new WeightedSet(new Money(100UL), 2L, new Credits(4UL), "NSW02", 715233UL, 284767UL) },
					{ "10025NSW02", new WeightedSet(new Money(100UL), 2L, new Credits(5UL), "NSW02", 675053UL, 324947UL) },
					{ "20011NSW02", new WeightedSet(new Money(200UL), 1L, new Credits(1UL), "NSW02", 683885UL, 316115UL) },
					{ "20012NSW02", new WeightedSet(new Money(200UL), 1L, new Credits(2UL), "NSW02", 702987UL, 297013UL) },
					{ "20013NSW02", new WeightedSet(new Money(200UL), 1L, new Credits(3UL), "NSW02", 703080UL, 296920UL) },
					{ "20014NSW02", new WeightedSet(new Money(200UL), 1L, new Credits(4UL), "NSW02", 708989UL, 291011UL) },
					{ "20015NSW02", new WeightedSet(new Money(200UL), 1L, new Credits(5UL), "NSW02", 722584UL, 277416UL) },
					{ "1150NSW03", new WeightedSet(new Money(1UL), 1L, new Credits(50UL), "NSW03", 727790UL, 272210UL) },
					{ "1250NSW03", new WeightedSet(new Money(1UL), 2L, new Credits(50UL), "NSW03", 729730UL, 270270UL) },
					{ "1350NSW03", new WeightedSet(new Money(1UL), 3L, new Credits(50UL), "NSW03", 726872UL, 273128UL) },
					{ "1550NSW03", new WeightedSet(new Money(1UL), 5L, new Credits(50UL), "NSW03", 725111UL, 274889UL) },
					{ "1850NSW03", new WeightedSet(new Money(1UL), 8L, new Credits(50UL), "NSW03", 726239UL, 273761UL) },
					{ "11050NSW03", new WeightedSet(new Money(1UL), 10L, new Credits(50UL), "NSW03", 726233UL, 273767UL) },
					{ "2150NSW03", new WeightedSet(new Money(2UL), 1L, new Credits(50UL), "NSW03", 713793UL, 286207UL) },
					{ "2250NSW03", new WeightedSet(new Money(2UL), 2L, new Credits(50UL), "NSW03", 714755UL, 285245UL) },
					{ "2350NSW03", new WeightedSet(new Money(2UL), 3L, new Credits(50UL), "NSW03", 711870UL, 288130UL) },
					{ "2450NSW03", new WeightedSet(new Money(2UL), 4L, new Credits(50UL), "NSW03", 692342UL, 307658UL) },
					{ "2550NSW03", new WeightedSet(new Money(2UL), 5L, new Credits(50UL), "NSW03", 709038UL, 290962UL) },
					{ "21050NSW03", new WeightedSet(new Money(2UL), 10L, new Credits(50UL), "NSW03", 708874UL, 291126UL) },
					{ "5125NSW03", new WeightedSet(new Money(5UL), 1L, new Credits(25UL), "NSW03", 731465UL, 268535UL) },
					{ "5225NSW03", new WeightedSet(new Money(5UL), 2L, new Credits(25UL), "NSW03", 731705UL, 268295UL) },
					{ "5325NSW03", new WeightedSet(new Money(5UL), 3L, new Credits(25UL), "NSW03", 706879UL, 293121UL) },
					{ "5425NSW03", new WeightedSet(new Money(5UL), 4L, new Credits(25UL), "NSW03", 720387UL, 279613UL) },
					{ "5625NSW03", new WeightedSet(new Money(5UL), 6L, new Credits(25UL), "NSW03", 739608UL, 260392UL) },
					{ "5825NSW03", new WeightedSet(new Money(5UL), 8L, new Credits(25UL), "NSW03", 735296UL, 264704UL) },
					{ "10125NSW03", new WeightedSet(new Money(10UL), 1L, new Credits(25UL), "NSW03", 715935UL, 284065UL) },
					{ "10225NSW03", new WeightedSet(new Money(10UL), 2L, new Credits(25UL), "NSW03", 715332UL, 284668UL) },
					{ "10325NSW03", new WeightedSet(new Money(10UL), 3L, new Credits(25UL), "NSW03", 692887UL, 307113UL) },
					{ "10425NSW03", new WeightedSet(new Money(10UL), 4L, new Credits(25UL), "NSW03", 755462UL, 244538UL) },
					{ "5011NSW03", new WeightedSet(new Money(50UL), 1L, new Credits(1UL), "NSW03", 658787UL, 341213UL) },
					{ "5012NSW03", new WeightedSet(new Money(50UL), 1L, new Credits(2UL), "NSW03", 674567UL, 325433UL) },
					{ "5013NSW03", new WeightedSet(new Money(50UL), 1L, new Credits(3UL), "NSW03", 659830UL, 340170UL) },
					{ "5014NSW03", new WeightedSet(new Money(50UL), 1L, new Credits(4UL), "NSW03", 681079UL, 318921UL) },
					{ "5015NSW03", new WeightedSet(new Money(50UL), 1L, new Credits(5UL), "NSW03", 652726UL, 347274UL) },
					{ "5021NSW03", new WeightedSet(new Money(50UL), 2L, new Credits(1UL), "NSW03", 673991UL, 326009UL) },
					{ "5022NSW03", new WeightedSet(new Money(50UL), 2L, new Credits(2UL), "NSW03", 650530UL, 349470UL) },
					{ "5023NSW03", new WeightedSet(new Money(50UL), 2L, new Credits(3UL), "NSW03", 669009UL, 330991UL) },
					{ "5024NSW03", new WeightedSet(new Money(50UL), 2L, new Credits(4UL), "NSW03", 700718UL, 299282UL) },
					{ "5025NSW03", new WeightedSet(new Money(50UL), 2L, new Credits(5UL), "NSW03", 669910UL, 330090UL) },
					{ "5031NSW03", new WeightedSet(new Money(50UL), 3L, new Credits(1UL), "NSW03", 678457UL, 321543UL) },
					{ "5032NSW03", new WeightedSet(new Money(50UL), 3L, new Credits(2UL), "NSW03", 649320UL, 350680UL) },
					{ "5033NSW03", new WeightedSet(new Money(50UL), 3L, new Credits(3UL), "NSW03", 646597UL, 353403UL) },
					{ "5034NSW03", new WeightedSet(new Money(50UL), 3L, new Credits(4UL), "NSW03", 643874UL, 356126UL) },
					{ "5035NSW03", new WeightedSet(new Money(50UL), 3L, new Credits(5UL), "NSW03", 641151UL, 358849UL) },
					{ "5041NSW03", new WeightedSet(new Money(50UL), 4L, new Credits(1UL), "NSW03", 700397UL, 299603UL) },
					{ "5042NSW03", new WeightedSet(new Money(50UL), 4L, new Credits(2UL), "NSW03", 673014UL, 326986UL) },
					{ "5043NSW03", new WeightedSet(new Money(50UL), 4L, new Credits(3UL), "NSW03", 693392UL, 306608UL) },
					{ "5044NSW03", new WeightedSet(new Money(50UL), 4L, new Credits(4UL), "NSW03", 676348UL, 323652UL) },
					{ "5045NSW03", new WeightedSet(new Money(50UL), 4L, new Credits(5UL), "NSW03", 682297UL, 317703UL) },
					{ "10011NSW03", new WeightedSet(new Money(100UL), 1L, new Credits(1UL), "NSW03", 677462UL, 322538UL) },
					{ "10012NSW03", new WeightedSet(new Money(100UL), 1L, new Credits(2UL), "NSW03", 696218UL, 303782UL) },
					{ "10013NSW03", new WeightedSet(new Money(100UL), 1L, new Credits(3UL), "NSW03", 709224UL, 290776UL) },
					{ "10014NSW03", new WeightedSet(new Money(100UL), 1L, new Credits(4UL), "NSW03", 727683UL, 272317UL) },
					{ "10015NSW03", new WeightedSet(new Money(100UL), 1L, new Credits(5UL), "NSW03", 713914UL, 286086UL) },
					{ "10021NSW03", new WeightedSet(new Money(100UL), 2L, new Credits(1UL), "NSW03", 696758UL, 303242UL) },
					{ "10022NSW03", new WeightedSet(new Money(100UL), 2L, new Credits(2UL), "NSW03", 718527UL, 281473UL) },
					{ "10023NSW03", new WeightedSet(new Money(100UL), 2L, new Credits(3UL), "NSW03", 714999UL, 285001UL) },
					{ "10024NSW03", new WeightedSet(new Money(100UL), 2L, new Credits(4UL), "NSW03", 736469UL, 263531UL) },
					{ "10025NSW03", new WeightedSet(new Money(100UL), 2L, new Credits(5UL), "NSW03", 694541UL, 305459UL) },
					{ "20011NSW03", new WeightedSet(new Money(200UL), 1L, new Credits(1UL), "NSW03", 699172UL, 300828UL) },
					{ "20012NSW03", new WeightedSet(new Money(200UL), 1L, new Credits(2UL), "NSW03", 724175UL, 275825UL) },
					{ "20013NSW03", new WeightedSet(new Money(200UL), 1L, new Credits(3UL), "NSW03", 722017UL, 277983UL) },
					{ "20014NSW03", new WeightedSet(new Money(200UL), 1L, new Credits(4UL), "NSW03", 729628UL, 270372UL) },
					{ "20015NSW03", new WeightedSet(new Money(200UL), 1L, new Credits(5UL), "NSW03", 741649UL, 258351UL) },
					{ "5011QLD01", new WeightedSet(new Money(50UL), 1L, new Credits(1UL), "QLD01", 682426UL, 317574UL) },
					{ "5012QLD01", new WeightedSet(new Money(50UL), 1L, new Credits(2UL), "QLD01", 705664UL, 294336UL) },
					{ "5013QLD01", new WeightedSet(new Money(50UL), 1L, new Credits(3UL), "QLD01", 690932UL, 309068UL) },
					{ "5014QLD01", new WeightedSet(new Money(50UL), 1L, new Credits(4UL), "QLD01", 712628UL, 287372UL) },
					{ "5015QLD01", new WeightedSet(new Money(50UL), 1L, new Credits(5UL), "QLD01", 684278UL, 315722UL) },
					{ "5021QLD01", new WeightedSet(new Money(50UL), 2L, new Credits(1UL), "QLD01", 705088UL, 294912UL) },
					{ "5022QLD01", new WeightedSet(new Money(50UL), 2L, new Credits(2UL), "QLD01", 680106UL, 319894UL) },
					{ "5023QLD01", new WeightedSet(new Money(50UL), 2L, new Credits(3UL), "QLD01", 698970UL, 301030UL) },
					{ "5024QLD01", new WeightedSet(new Money(50UL), 2L, new Credits(4UL), "QLD01", 726593UL, 273407UL) },
					{ "5025QLD01", new WeightedSet(new Money(50UL), 2L, new Credits(5UL), "QLD01", 695521UL, 304479UL) },
					{ "5031QLD01", new WeightedSet(new Money(50UL), 3L, new Credits(1UL), "QLD01", 703072UL, 296928UL) },
					{ "5032QLD01", new WeightedSet(new Money(50UL), 3L, new Credits(2UL), "QLD01", 675473UL, 324527UL) },
					{ "5033QLD01", new WeightedSet(new Money(50UL), 3L, new Credits(3UL), "QLD01", 672266UL, 327734UL) },
					{ "5034QLD01", new WeightedSet(new Money(50UL), 3L, new Credits(4UL), "QLD01", 669059UL, 330941UL) },
					{ "5035QLD01", new WeightedSet(new Money(50UL), 3L, new Credits(5UL), "QLD01", 665853UL, 334147UL) },
					{ "5041QLD01", new WeightedSet(new Money(50UL), 4L, new Credits(1UL), "QLD01", 722841UL, 277159UL) },
					{ "5042QLD01", new WeightedSet(new Money(50UL), 4L, new Credits(2UL), "QLD01", 695469UL, 304531UL) },
					{ "5043QLD01", new WeightedSet(new Money(50UL), 4L, new Credits(3UL), "QLD01", 713386UL, 286614UL) },
					{ "5044QLD01", new WeightedSet(new Money(50UL), 4L, new Credits(4UL), "QLD01", 696353UL, 303647UL) },
					{ "5045QLD01", new WeightedSet(new Money(50UL), 4L, new Credits(5UL), "QLD01", 702311UL, 297689UL) },
					{ "10011QLD01", new WeightedSet(new Money(100UL), 1L, new Credits(1UL), "QLD01", 672426UL, 327574UL) },
					{ "10012QLD01", new WeightedSet(new Money(100UL), 1L, new Credits(2UL), "QLD01", 689744UL, 310256UL) },
					{ "10013QLD01", new WeightedSet(new Money(100UL), 1L, new Credits(3UL), "QLD01", 704120UL, 295880UL) },
					{ "10014QLD01", new WeightedSet(new Money(100UL), 1L, new Credits(4UL), "QLD01", 722279UL, 277721UL) },
					{ "10015QLD01", new WeightedSet(new Money(100UL), 1L, new Credits(5UL), "QLD01", 709614UL, 290386UL) },
					{ "5011QLD02", new WeightedSet(new Money(50UL), 1L, new Credits(1UL), "QLD02", 682426UL, 317574UL) },
					{ "5012QLD02", new WeightedSet(new Money(50UL), 1L, new Credits(2UL), "QLD02", 705664UL, 294336UL) },
					{ "5013QLD02", new WeightedSet(new Money(50UL), 1L, new Credits(3UL), "QLD02", 690932UL, 309068UL) },
					{ "5014QLD02", new WeightedSet(new Money(50UL), 1L, new Credits(4UL), "QLD02", 712628UL, 287372UL) },
					{ "5015QLD02", new WeightedSet(new Money(50UL), 1L, new Credits(5UL), "QLD02", 684278UL, 315722UL) },
					{ "5021QLD02", new WeightedSet(new Money(50UL), 2L, new Credits(1UL), "QLD02", 705088UL, 294912UL) },
					{ "5022QLD02", new WeightedSet(new Money(50UL), 2L, new Credits(2UL), "QLD02", 680106UL, 319894UL) },
					{ "5023QLD02", new WeightedSet(new Money(50UL), 2L, new Credits(3UL), "QLD02", 698970UL, 301030UL) },
					{ "5024QLD02", new WeightedSet(new Money(50UL), 2L, new Credits(4UL), "QLD02", 726593UL, 273407UL) },
					{ "5025QLD02", new WeightedSet(new Money(50UL), 2L, new Credits(5UL), "QLD02", 695521UL, 304479UL) },
					{ "5031QLD02", new WeightedSet(new Money(50UL), 3L, new Credits(1UL), "QLD02", 703072UL, 296928UL) },
					{ "5032QLD02", new WeightedSet(new Money(50UL), 3L, new Credits(2UL), "QLD02", 675473UL, 324527UL) },
					{ "5033QLD02", new WeightedSet(new Money(50UL), 3L, new Credits(3UL), "QLD02", 672266UL, 327734UL) },
					{ "5034QLD02", new WeightedSet(new Money(50UL), 3L, new Credits(4UL), "QLD02", 669059UL, 330941UL) },
					{ "5035QLD02", new WeightedSet(new Money(50UL), 3L, new Credits(5UL), "QLD02", 665853UL, 334147UL) },
					{ "5041QLD02", new WeightedSet(new Money(50UL), 4L, new Credits(1UL), "QLD02", 722841UL, 277159UL) },
					{ "5042QLD02", new WeightedSet(new Money(50UL), 4L, new Credits(2UL), "QLD02", 695469UL, 304531UL) },
					{ "5043QLD02", new WeightedSet(new Money(50UL), 4L, new Credits(3UL), "QLD02", 713386UL, 286614UL) },
					{ "5044QLD02", new WeightedSet(new Money(50UL), 4L, new Credits(4UL), "QLD02", 696353UL, 303647UL) },
					{ "5045QLD02", new WeightedSet(new Money(50UL), 4L, new Credits(5UL), "QLD02", 702311UL, 297689UL) },
					{ "10011QLD02", new WeightedSet(new Money(100UL), 1L, new Credits(1UL), "QLD02", 672426UL, 327574UL) },
					{ "10012QLD02", new WeightedSet(new Money(100UL), 1L, new Credits(2UL), "QLD02", 689744UL, 310256UL) },
					{ "10013QLD02", new WeightedSet(new Money(100UL), 1L, new Credits(3UL), "QLD02", 704120UL, 295880UL) },
					{ "10014QLD02", new WeightedSet(new Money(100UL), 1L, new Credits(4UL), "QLD02", 722279UL, 277721UL) },
					{ "10015QLD02", new WeightedSet(new Money(100UL), 1L, new Credits(5UL), "QLD02", 709614UL, 290386UL) },
					{ "5011QLD03", new WeightedSet(new Money(50UL), 1L, new Credits(1UL), "QLD03", 698753UL, 301247UL) },
					{ "5012QLD03", new WeightedSet(new Money(50UL), 1L, new Credits(2UL), "QLD03", 727535UL, 272465UL) },
					{ "5013QLD03", new WeightedSet(new Money(50UL), 1L, new Credits(3UL), "QLD03", 712799UL, 287201UL) },
					{ "5014QLD03", new WeightedSet(new Money(50UL), 1L, new Credits(4UL), "QLD03", 734803UL, 265197UL) },
					{ "5015QLD03", new WeightedSet(new Money(50UL), 1L, new Credits(5UL), "QLD03", 706450UL, 293550UL) },
					{ "5021QLD03", new WeightedSet(new Money(50UL), 2L, new Credits(1UL), "QLD03", 726959UL, 273041UL) },
					{ "5022QLD03", new WeightedSet(new Money(50UL), 2L, new Credits(2UL), "QLD03", 701287UL, 298713UL) },
					{ "5023QLD03", new WeightedSet(new Money(50UL), 2L, new Credits(3UL), "QLD03", 720826UL, 279174UL) },
					{ "5024QLD03", new WeightedSet(new Money(50UL), 2L, new Credits(4UL), "QLD03", 745827UL, 254173UL) },
					{ "5025QLD03", new WeightedSet(new Money(50UL), 2L, new Credits(5UL), "QLD03", 714929UL, 285071UL) },
					{ "5031QLD03", new WeightedSet(new Money(50UL), 3L, new Credits(1UL), "QLD03", 720378UL, 279622UL) },
					{ "5032QLD03", new WeightedSet(new Money(50UL), 3L, new Credits(2UL), "QLD03", 694191UL, 305809UL) },
					{ "5033QLD03", new WeightedSet(new Money(50UL), 3L, new Credits(3UL), "QLD03", 690975UL, 309025UL) },
					{ "5034QLD03", new WeightedSet(new Money(50UL), 3L, new Credits(4UL), "QLD03", 687760UL, 312240UL) },
					{ "5035QLD03", new WeightedSet(new Money(50UL), 3L, new Credits(5UL), "QLD03", 684544UL, 315456UL) },
					{ "5041QLD03", new WeightedSet(new Money(50UL), 4L, new Credits(1UL), "QLD03", 738914UL, 261086UL) },
					{ "5042QLD03", new WeightedSet(new Money(50UL), 4L, new Credits(2UL), "QLD03", 711532UL, 288468UL) },
					{ "5043QLD03", new WeightedSet(new Money(50UL), 4L, new Credits(3UL), "QLD03", 727947UL, 272053UL) },
					{ "5044QLD03", new WeightedSet(new Money(50UL), 4L, new Credits(4UL), "QLD03", 710904UL, 289096UL) },
					{ "5045QLD03", new WeightedSet(new Money(50UL), 4L, new Credits(5UL), "QLD03", 716853UL, 283147UL) },
					{ "10011QLD03", new WeightedSet(new Money(100UL), 1L, new Credits(1UL), "QLD03", 687929UL, 312071UL) },
					{ "10012QLD03", new WeightedSet(new Money(100UL), 1L, new Credits(2UL), "QLD03", 711354UL, 288646UL) },
					{ "10013QLD03", new WeightedSet(new Money(100UL), 1L, new Credits(3UL), "QLD03", 722726UL, 277274UL) },
					{ "10014QLD03", new WeightedSet(new Money(100UL), 1L, new Credits(4UL), "QLD03", 743977UL, 256023UL) },
					{ "10015QLD03", new WeightedSet(new Money(100UL), 1L, new Credits(5UL), "QLD03", 728830UL, 271170UL) }
				}
			);
		}

		private static SelectorItems CreateExcelReplacementTablesLookup()
		{
			return new SelectorItems(
				new List<SelectorItem>()
				{
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(1UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(50UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 585910UL, 588000UL, 639900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 2000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 90UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 557910UL, 560000UL, 494900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 25000UL, 25000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 2000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 90UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(2UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(50UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 583820UL, 588000UL, 639900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 180UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 555820UL, 560000UL, 494900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 25000UL, 25000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 180UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(5UL)), Requirement.Create("Percentage", "NSW01", "NSW02"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 585775UL, 591000UL, 642900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 8000UL, 8000UL, 7000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 5000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 225UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 573775UL, 579000UL, 497900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 7000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 5000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 225UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(10UL)), Requirement.Create("Percentage", "NSW01", "NSW02"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 580550UL, 591000UL, 642900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 8000UL, 8000UL, 7000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 450UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 568550UL, 579000UL, 497900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 7000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 450UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "QLD01", "QLD02"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 596910UL, 599500UL, 634500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 500UL, 500UL, 500UL),
								new ReplacementTable("MINOR", 28, 500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 2000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 90UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 451910UL, 454500UL, 289500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 200000UL),
								new ReplacementTable("C3", 17, 160000UL, 160000UL, 160000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 500UL, 500UL, 500UL),
								new ReplacementTable("MINOR", 28, 500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 2000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 90UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "QLD01", "QLD02"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 592820UL, 598500UL, 633500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1500UL, 1500UL, 1500UL),
								new ReplacementTable("MINOR", 28, 1500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 180UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 582820UL, 588500UL, 473500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1500UL, 1500UL, 1500UL),
								new ReplacementTable("MINOR", 28, 1500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 180UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "QLD01", "QLD02"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 575730UL, 583500UL, 633500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1500UL, 1500UL, 1500UL),
								new ReplacementTable("MINOR", 28, 1500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 6000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 270UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 475730UL, 483500UL, 298500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 200000UL),
								new ReplacementTable("C3", 17, 160000UL, 160000UL, 160000UL),
								new ReplacementTable("C4", 18, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1500UL, 1500UL, 1500UL),
								new ReplacementTable("MINOR", 28, 1500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 6000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 270UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "QLD01", "QLD02"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 4L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 572640UL, 583000UL, 633000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 360UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 442640UL, 453000UL, 268000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 200000UL),
								new ReplacementTable("C3", 17, 160000UL, 160000UL, 160000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 360UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "QLD01", "QLD02"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 593420UL, 599600UL, 634600UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 400UL, 400UL, 400UL),
								new ReplacementTable("MINOR", 28, 2000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 180UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 446320UL, 454500UL, 289600UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 200000UL),
								new ReplacementTable("C3", 17, 160000UL, 160000UL, 160000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 500UL, 500UL, 400UL),
								new ReplacementTable("MINOR", 28, 4000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 180UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "NSW01", "NSW02"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 582640UL, 599000UL, 634000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 8000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 360UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 570640UL, 589000UL, 474000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 360UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "NSW01", "NSW02"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 574240UL, 584600UL, 634600UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 400UL, 400UL, 400UL),
								new ReplacementTable("MINOR", 28, 2000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 360UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 442140UL, 454500UL, 269600UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 200000UL),
								new ReplacementTable("C3", 17, 160000UL, 160000UL, 160000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 500UL, 500UL, 400UL),
								new ReplacementTable("MINOR", 28, 4000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 360UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(1UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(50UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 570820UL, 575000UL, 629900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 25000UL, 25000UL, 25000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 180UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 525820UL, 530000UL, 484900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 70000UL, 70000UL, 40000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 180UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(2UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(50UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 566640UL, 575000UL, 629900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 25000UL, 25000UL, 25000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 360UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 521640UL, 530000UL, 484900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 70000UL, 70000UL, 40000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 360UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(5UL)), Requirement.Create("Percentage", "NSW01", "NSW02"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 572550UL, 583000UL, 634900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 15000UL, 15000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 450UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 556550UL, 567000UL, 489900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 3000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 450UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(10UL)), Requirement.Create("Percentage", "NSW01", "NSW02"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 562100UL, 583000UL, 634900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 15000UL, 15000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 900UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 546100UL, 567000UL, 489900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 3000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 900UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "QLD01", "QLD02"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 592820UL, 598500UL, 633500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1500UL, 1500UL, 1500UL),
								new ReplacementTable("MINOR", 28, 1500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 180UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 582820UL, 588500UL, 473500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1500UL, 1500UL, 1500UL),
								new ReplacementTable("MINOR", 28, 1500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 180UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "QLD01", "QLD02"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 595640UL, 607000UL, 642000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 360UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 575640UL, 587000UL, 472000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 360UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "QLD01", "QLD02"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 566460UL, 582000UL, 632000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 540UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 521460UL, 537000UL, 412000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 540UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "QLD01", "QLD02"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 4L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 561280UL, 581500UL, 631500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3500UL, 3500UL, 3500UL),
								new ReplacementTable("MINOR", 28, 3500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 720UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 431280UL, 451500UL, 266500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 200000UL),
								new ReplacementTable("C3", 17, 160000UL, 160000UL, 160000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 3500UL, 3500UL, 3500UL),
								new ReplacementTable("MINOR", 28, 3500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 720UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "QLD01", "QLD02"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 582640UL, 599000UL, 634000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 8000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 360UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 570640UL, 589000UL, 474000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 360UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "NSW01", "NSW02"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 573780UL, 602500UL, 642500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 12000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 720UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 550780UL, 587500UL, 472500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 20000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 720UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "NSW01", "NSW02"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 559280UL, 584000UL, 634000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 8000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 720UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 562280UL, 589000UL, 454000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 720UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(1UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(50UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 555730UL, 562000UL, 619900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("MINOR", 28, 8000UL, 8000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 6000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 270UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 483730UL, 495000UL, 474900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 80000UL, 80000UL, 30000UL),
								new ReplacementTable("MINOR", 28, 20000UL, 15000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 6000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 270UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(2UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(50UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 549460UL, 562000UL, 619900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("MINOR", 28, 8000UL, 8000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 540UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 477460UL, 495000UL, 474900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 80000UL, 80000UL, 30000UL),
								new ReplacementTable("MINOR", 28, 20000UL, 15000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 540UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(5UL)), Requirement.Create("Percentage", "NSW01", "NSW02"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 556325UL, 572000UL, 629900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 25000UL, 25000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 3000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 15000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 675UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 538325UL, 554000UL, 502900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 30000UL, 30000UL, 2000UL),
								new ReplacementTable("MINOR", 28, 6000UL, 6000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 15000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 675UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(10UL)), Requirement.Create("Percentage", "NSW01", "NSW02"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 545650UL, 577000UL, 629900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 3000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 30000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1350UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 522650UL, 554000UL, 484900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 6000UL, 6000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 30000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1350UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "QLD01", "QLD02"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 589730UL, 598000UL, 632500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2000UL, 2000UL, 2500UL),
								new ReplacementTable("MINOR", 28, 2000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 6000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 270UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 579730UL, 588000UL, 472500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2000UL, 2000UL, 2500UL),
								new ReplacementTable("MINOR", 28, 2000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 6000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 270UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "QLD01", "QLD02"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 578460UL, 595500UL, 630500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 4500UL, 4500UL, 4500UL),
								new ReplacementTable("MINOR", 28, 4500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 540UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 568460UL, 585500UL, 470500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 4500UL, 4500UL, 4500UL),
								new ReplacementTable("MINOR", 28, 4500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 540UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "QLD01", "QLD02"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 557190UL, 580500UL, 630500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 4500UL, 4500UL, 4500UL),
								new ReplacementTable("MINOR", 28, 4500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 18000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 810UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 512190UL, 535500UL, 410500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 4500UL, 4500UL, 4500UL),
								new ReplacementTable("MINOR", 28, 4500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 18000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 810UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "QLD01", "QLD02"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 4L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 549920UL, 580000UL, 630000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1080UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 369920UL, 400000UL, 265000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 200000UL),
								new ReplacementTable("C3", 17, 160000UL, 160000UL, 160000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("C6", 20, 40000UL, 40000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1080UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "QLD01", "QLD02"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 575860UL, 598400UL, 633400UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MINOR", 28, 10000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 540UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 555460UL, 588000UL, 473400UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2000UL, 2000UL, 1600UL),
								new ReplacementTable("MINOR", 28, 20000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 540UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "NSW01", "NSW02"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 549920UL, 595000UL, 630000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINOR", 28, 20000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1080UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 539920UL, 585000UL, 470000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINOR", 28, 20000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1080UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "NSW01", "NSW02"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 548320UL, 583400UL, 633400UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MINOR", 28, 10000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1080UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 544920UL, 588000UL, 453400UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2000UL, 2000UL, 1600UL),
								new ReplacementTable("MINOR", 28, 18000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1080UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(1UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(50UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 529550UL, 545000UL, 599900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 10000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 60000UL, 60000UL, 60000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 450UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 439550UL, 460000UL, 444900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 100000UL, 100000UL, 50000UL),
								new ReplacementTable("MINOR", 28, 40000UL, 30000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 450UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(2UL)), Requirement.Create("Percentage", "NSW01", "NSW02"), Requirement.Create("LinesBet", new Credits(50UL)), Requirement.Create("BetMultiplier", 4L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 534280UL, 551000UL, 609900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 40000UL, 40000UL, 40000UL),
								new ReplacementTable("MINOR", 28, 9000UL, 9000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 720UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 453280UL, 475000UL, 464900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 90000UL, 90000UL, 40000UL),
								new ReplacementTable("MINOR", 28, 30000UL, 25000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 720UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(5UL)), Requirement.Create("Percentage", "NSW01", "NSW02"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 4L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 544100UL, 565000UL, 629900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 35000UL, 35000UL, 25000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 900UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 519100UL, 540000UL, 484900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 60000UL, 60000UL, 40000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 900UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(10UL)), Requirement.Create("Percentage", "NSW01", "NSW02"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 4L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 513200UL, 560000UL, 639900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 10000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 35000UL, 35000UL, 25000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 498200UL, 540000UL, 474900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 60000UL, 60000UL, 40000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "QLD01", "QLD02"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 585640UL, 597000UL, 622000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 360UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 575640UL, 587000UL, 472000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 360UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "QLD01", "QLD02"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 571780UL, 594000UL, 629000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 5500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 720UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 561780UL, 584000UL, 459000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 5500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 720UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "QLD01", "QLD02"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 547920UL, 579000UL, 629000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 6000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1080UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 502920UL, 534000UL, 409000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 6000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1080UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "QLD01", "QLD02"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 4L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 539560UL, 579000UL, 629000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 6000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 32000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1440UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 359560UL, 399000UL, 264000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 200000UL),
								new ReplacementTable("C3", 17, 160000UL, 160000UL, 160000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("C6", 20, 40000UL, 40000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 6000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 32000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1440UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "QLD01", "QLD02"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 581780UL, 612500UL, 642500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 14000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 720UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 550780UL, 587500UL, 472500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 20000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 720UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "NSW01", "NSW02"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 538560UL, 592000UL, 627000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 8000UL, 8000UL, 8000UL),
								new ReplacementTable("MINOR", 28, 20000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 32000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1440UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 528560UL, 582000UL, 457000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 8000UL, 8000UL, 8000UL),
								new ReplacementTable("MINOR", 28, 20000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 32000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1440UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "NSW01", "NSW02"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 552060UL, 597500UL, 642500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 12000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 32000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1440UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 534060UL, 587500UL, 452500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 20000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 32000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1440UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(1UL)), Requirement.Create("Percentage", "NSW01", "NSW02"), Requirement.Create("LinesBet", new Credits(50UL)), Requirement.Create("BetMultiplier", 8L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 483280UL, 485000UL, 550000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 10000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 80000UL, 100000UL, 100000UL),
								new ReplacementTable("MINOR", 28, 15000UL, 15000UL, 10000UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 720UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 433280UL, 445000UL, 395000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 10000UL, 20000UL),
								new ReplacementTable("MINI", 27, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("MINOR", 28, 60000UL, 60000UL, 20000UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 720UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(2UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(50UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 519100UL, 545000UL, 599900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 10000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 60000UL, 60000UL, 60000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 900UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 429100UL, 460000UL, 444900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 100000UL, 100000UL, 50000UL),
								new ReplacementTable("MINOR", 28, 40000UL, 30000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 900UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(5UL)), Requirement.Create("Percentage", "NSW01", "NSW02"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 6L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 510650UL, 542000UL, 609900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("MINOR", 28, 8000UL, 8000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 30000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1350UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 488650UL, 520000UL, 444900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 60000UL, 60000UL, 50000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 30000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1350UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "QLD01", "QLD02"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 582550UL, 596500UL, 621500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3500UL, 3500UL, 3500UL),
								new ReplacementTable("MINOR", 28, 3500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 450UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 572550UL, 586500UL, 471500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 3500UL, 3500UL, 3500UL),
								new ReplacementTable("MINOR", 28, 3500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 450UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "QLD01", "QLD02"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 565100UL, 593000UL, 623000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 7000UL, 7000UL, 7000UL),
								new ReplacementTable("MINOR", 28, 7000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 900UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 535100UL, 563000UL, 478000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 7000UL, 7000UL, 7000UL),
								new ReplacementTable("MINOR", 28, 7000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 900UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "QLD01", "QLD02"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 538650UL, 577500UL, 627500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 7500UL, 7500UL, 7500UL),
								new ReplacementTable("MINOR", 28, 7500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 30000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1350UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 493650UL, 532500UL, 407500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 7500UL, 7500UL, 7500UL),
								new ReplacementTable("MINOR", 28, 7500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 30000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1350UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "QLD01", "QLD02"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 4L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 527200UL, 577000UL, 627000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 8000UL, 8000UL, 8000UL),
								new ReplacementTable("MINOR", 28, 8000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 347200UL, 397000UL, 262000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 200000UL),
								new ReplacementTable("C3", 17, 160000UL, 160000UL, 160000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("C6", 20, 40000UL, 40000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 8000UL, 8000UL, 8000UL),
								new ReplacementTable("MINOR", 28, 8000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "QLD01", "QLD02"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 577100UL, 612000UL, 642000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 14000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 900UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 535600UL, 586500UL, 471500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 3500UL, 3500UL, 3500UL),
								new ReplacementTable("MINOR", 28, 30000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 900UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "NSW01", "NSW02"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 528200UL, 590000UL, 640000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 20000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 498200UL, 580000UL, 485000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 40000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "NSW01", "NSW02"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 540200UL, 597000UL, 642000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 15000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 514700UL, 586500UL, 451500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 3500UL, 3500UL, 3500UL),
								new ReplacementTable("MINOR", 28, 30000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(1UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(50UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 449100UL, 475000UL, 550000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 10000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("MINOR", 28, 25000UL, 25000UL, 10000UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 900UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 389100UL, 405000UL, 385000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 10000UL, 20000UL),
								new ReplacementTable("MINI", 27, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("MINOR", 28, 100000UL, 100000UL, 30000UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 900UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(2UL)), Requirement.Create("Percentage", "NSW01", "NSW02", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(50UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 428200UL, 475000UL, 550000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 10000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("MINOR", 28, 25000UL, 25000UL, 10000UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 368200UL, 405000UL, 385000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 10000UL, 20000UL),
								new ReplacementTable("MINI", 27, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("MINOR", 28, 100000UL, 100000UL, 30000UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(5UL)), Requirement.Create("Percentage", "NSW01", "NSW02"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 8L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 495200UL, 537000UL, 587500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 10000UL),
								new ReplacementTable("C5", 19, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 20000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 8000UL, 8000UL, 2500UL),
								new ReplacementTable("MINI", 27, 70000UL, 70000UL, 70000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 468200UL, 507000UL, 435000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 8000UL, 5000UL),
								new ReplacementTable("MINI", 27, 90000UL, 90000UL, 90000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 15000UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(1UL)), Requirement.Create("Percentage", "NSW03", "CAS03"), Requirement.Create("LinesBet", new Credits(50UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 585840UL, 588000UL, 639900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 2000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 160UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 557840UL, 560000UL, 494900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 25000UL, 25000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 2000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 160UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(2UL)), Requirement.Create("Percentage", "NSW03", "CAS03"), Requirement.Create("LinesBet", new Credits(50UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 583680UL, 588000UL, 639900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 320UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 555680UL, 560000UL, 494900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 25000UL, 25000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 320UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(5UL)), Requirement.Create("Percentage", "NSW03"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 585600UL, 591000UL, 642900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 8000UL, 8000UL, 7000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 5000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 400UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 573600UL, 579000UL, 497900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 7000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 5000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 400UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(10UL)), Requirement.Create("Percentage", "NSW03"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 580200UL, 591000UL, 642900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 8000UL, 8000UL, 7000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 568200UL, 579000UL, 497900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 7000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW03", "QLD03"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 596840UL, 599500UL, 634500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 500UL, 500UL, 500UL),
								new ReplacementTable("MINOR", 28, 500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 2000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 160UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 451840UL, 454500UL, 289500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 200000UL),
								new ReplacementTable("C3", 17, 160000UL, 160000UL, 160000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 500UL, 500UL, 500UL),
								new ReplacementTable("MINOR", 28, 500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 2000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 160UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW03", "QLD03"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 592680UL, 598500UL, 633500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1500UL, 1500UL, 1500UL),
								new ReplacementTable("MINOR", 28, 1500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 320UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 582680UL, 588500UL, 473500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1500UL, 1500UL, 1500UL),
								new ReplacementTable("MINOR", 28, 1500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 320UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW03", "QLD03"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 575520UL, 583500UL, 633500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1500UL, 1500UL, 1500UL),
								new ReplacementTable("MINOR", 28, 1500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 6000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 480UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 475520UL, 483500UL, 298500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 200000UL),
								new ReplacementTable("C3", 17, 160000UL, 160000UL, 160000UL),
								new ReplacementTable("C4", 18, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1500UL, 1500UL, 1500UL),
								new ReplacementTable("MINOR", 28, 1500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 6000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 480UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW03", "QLD03"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 4L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 572360UL, 583000UL, 633000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 640UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 442360UL, 453000UL, 268000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 200000UL),
								new ReplacementTable("C3", 17, 160000UL, 160000UL, 160000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 640UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "NSW03", "QLD03"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 593280UL, 599600UL, 634600UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 400UL, 400UL, 400UL),
								new ReplacementTable("MINOR", 28, 2000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 320UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 446180UL, 454500UL, 289600UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 200000UL),
								new ReplacementTable("C3", 17, 160000UL, 160000UL, 160000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 500UL, 500UL, 400UL),
								new ReplacementTable("MINOR", 28, 4000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 320UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "NSW03"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 582360UL, 599000UL, 634000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 8000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 640UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 570360UL, 589000UL, 474000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 640UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "NSW03"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 573960UL, 584600UL, 634600UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 400UL, 400UL, 400UL),
								new ReplacementTable("MINOR", 28, 2000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 640UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 441860UL, 454500UL, 269600UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 200000UL),
								new ReplacementTable("C3", 17, 160000UL, 160000UL, 160000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 500UL, 500UL, 400UL),
								new ReplacementTable("MINOR", 28, 4000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 640UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(1UL)), Requirement.Create("Percentage", "NSW03", "CAS03"), Requirement.Create("LinesBet", new Credits(50UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 570680UL, 575000UL, 629900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 25000UL, 25000UL, 25000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 320UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 525680UL, 530000UL, 484900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 70000UL, 70000UL, 40000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 320UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(2UL)), Requirement.Create("Percentage", "NSW03", "CAS03"), Requirement.Create("LinesBet", new Credits(50UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 566360UL, 575000UL, 629900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 25000UL, 25000UL, 25000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 640UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 521360UL, 530000UL, 484900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 70000UL, 70000UL, 40000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 640UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(5UL)), Requirement.Create("Percentage", "NSW03"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 572200UL, 583000UL, 634900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 15000UL, 15000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 556200UL, 567000UL, 489900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 3000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(10UL)), Requirement.Create("Percentage", "NSW03"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 561400UL, 583000UL, 634900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 15000UL, 15000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1600UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 545400UL, 567000UL, 489900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 3000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1600UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW03", "QLD03"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 592680UL, 598500UL, 633500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1500UL, 1500UL, 1500UL),
								new ReplacementTable("MINOR", 28, 1500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 320UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 582680UL, 588500UL, 473500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1500UL, 1500UL, 1500UL),
								new ReplacementTable("MINOR", 28, 1500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 320UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW03", "QLD03"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 595360UL, 607000UL, 642000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 640UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 575360UL, 587000UL, 472000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 640UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW03", "QLD03"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 566040UL, 582000UL, 632000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 960UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 521040UL, 537000UL, 412000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 960UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW03", "QLD03"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 4L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 560720UL, 581500UL, 631500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3500UL, 3500UL, 3500UL),
								new ReplacementTable("MINOR", 28, 3500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1280UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 430720UL, 451500UL, 266500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 200000UL),
								new ReplacementTable("C3", 17, 160000UL, 160000UL, 160000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 3500UL, 3500UL, 3500UL),
								new ReplacementTable("MINOR", 28, 3500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1280UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "NSW03", "QLD03"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 582360UL, 599000UL, 634000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 8000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 640UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 570360UL, 589000UL, 474000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 640UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "NSW03"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 573220UL, 602500UL, 642500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 12000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1280UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 550220UL, 587500UL, 472500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 20000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1280UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "NSW03"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 558720UL, 584000UL, 634000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 8000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1280UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 561720UL, 589000UL, 454000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1280UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(1UL)), Requirement.Create("Percentage", "NSW03", "CAS03"), Requirement.Create("LinesBet", new Credits(50UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 555520UL, 562000UL, 619900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("MINOR", 28, 8000UL, 8000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 6000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 480UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 483520UL, 495000UL, 474900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 80000UL, 80000UL, 30000UL),
								new ReplacementTable("MINOR", 28, 20000UL, 15000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 6000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 480UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(2UL)), Requirement.Create("Percentage", "NSW03", "CAS03"), Requirement.Create("LinesBet", new Credits(50UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 549040UL, 562000UL, 619900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("MINOR", 28, 8000UL, 8000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 960UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 477040UL, 495000UL, 474900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 80000UL, 80000UL, 30000UL),
								new ReplacementTable("MINOR", 28, 20000UL, 15000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 960UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(5UL)), Requirement.Create("Percentage", "NSW03"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 555800UL, 572000UL, 629900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 25000UL, 25000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 3000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 15000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1200UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 537800UL, 554000UL, 502900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 30000UL, 30000UL, 2000UL),
								new ReplacementTable("MINOR", 28, 6000UL, 6000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 15000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1200UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(10UL)), Requirement.Create("Percentage", "NSW03"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 544600UL, 577000UL, 629900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 3000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 30000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2400UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 521600UL, 554000UL, 484900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 6000UL, 6000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 30000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2400UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW03", "QLD03"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 589520UL, 598000UL, 632500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2000UL, 2000UL, 2500UL),
								new ReplacementTable("MINOR", 28, 2000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 6000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 480UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 579520UL, 588000UL, 472500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2000UL, 2000UL, 2500UL),
								new ReplacementTable("MINOR", 28, 2000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 6000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 480UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW03", "QLD03"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 578040UL, 595500UL, 630500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 4500UL, 4500UL, 4500UL),
								new ReplacementTable("MINOR", 28, 4500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 960UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 568040UL, 585500UL, 470500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 4500UL, 4500UL, 4500UL),
								new ReplacementTable("MINOR", 28, 4500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 960UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW03", "QLD03"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 556560UL, 580500UL, 630500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 4500UL, 4500UL, 4500UL),
								new ReplacementTable("MINOR", 28, 4500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 18000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1440UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 511560UL, 535500UL, 410500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 4500UL, 4500UL, 4500UL),
								new ReplacementTable("MINOR", 28, 4500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 18000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1440UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW03", "QLD03"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 4L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 549080UL, 580000UL, 630000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1920UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 369080UL, 400000UL, 265000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 200000UL),
								new ReplacementTable("C3", 17, 160000UL, 160000UL, 160000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("C6", 20, 40000UL, 40000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1920UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "NSW03", "QLD03"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 575440UL, 598400UL, 633400UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MINOR", 28, 10000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 960UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 555040UL, 588000UL, 473400UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2000UL, 2000UL, 1600UL),
								new ReplacementTable("MINOR", 28, 20000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 960UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "NSW03"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 549080UL, 595000UL, 630000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINOR", 28, 20000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1920UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 539080UL, 585000UL, 470000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINOR", 28, 20000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1920UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "NSW03"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 547480UL, 583400UL, 633400UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MINOR", 28, 10000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1920UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 544080UL, 588000UL, 453400UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2000UL, 2000UL, 1600UL),
								new ReplacementTable("MINOR", 28, 18000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1920UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(1UL)), Requirement.Create("Percentage", "NSW03", "CAS03"), Requirement.Create("LinesBet", new Credits(50UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 529200UL, 545000UL, 599900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 10000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 60000UL, 60000UL, 60000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 439200UL, 460000UL, 444900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 100000UL, 100000UL, 50000UL),
								new ReplacementTable("MINOR", 28, 40000UL, 30000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(2UL)), Requirement.Create("Percentage", "NSW03"), Requirement.Create("LinesBet", new Credits(50UL)), Requirement.Create("BetMultiplier", 4L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 533720UL, 551000UL, 609900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 40000UL, 40000UL, 40000UL),
								new ReplacementTable("MINOR", 28, 9000UL, 9000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1280UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 452720UL, 475000UL, 464900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 90000UL, 90000UL, 40000UL),
								new ReplacementTable("MINOR", 28, 30000UL, 25000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1280UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(5UL)), Requirement.Create("Percentage", "NSW03"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 4L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 543400UL, 565000UL, 629900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 35000UL, 35000UL, 25000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1600UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 518400UL, 540000UL, 484900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 60000UL, 60000UL, 40000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1600UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(10UL)), Requirement.Create("Percentage", "NSW03"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 4L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 511800UL, 560000UL, 639900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 10000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 35000UL, 35000UL, 25000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 496800UL, 540000UL, 474900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 60000UL, 60000UL, 40000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW03", "QLD03"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 585360UL, 597000UL, 622000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 640UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 575360UL, 587000UL, 472000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 640UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW03", "QLD03"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 571220UL, 594000UL, 629000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 5500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1280UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 561220UL, 584000UL, 459000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 5500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1280UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW03", "QLD03"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 547080UL, 579000UL, 629000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 6000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1920UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 502080UL, 534000UL, 409000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 6000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1920UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW03", "QLD03"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 4L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 538440UL, 579000UL, 629000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 6000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 32000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2560UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 358440UL, 399000UL, 264000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 200000UL),
								new ReplacementTable("C3", 17, 160000UL, 160000UL, 160000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("C6", 20, 40000UL, 40000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 6000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 32000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2560UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "NSW03", "QLD03"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 581220UL, 612500UL, 642500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 14000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1280UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 550220UL, 587500UL, 472500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 20000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1280UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "NSW03"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 537440UL, 592000UL, 627000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 8000UL, 8000UL, 8000UL),
								new ReplacementTable("MINOR", 28, 20000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 32000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2560UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 527440UL, 582000UL, 457000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 8000UL, 8000UL, 8000UL),
								new ReplacementTable("MINOR", 28, 20000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 32000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2560UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "NSW03"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 550940UL, 597500UL, 642500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 12000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 32000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2560UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 532940UL, 587500UL, 452500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 20000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 32000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2560UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(1UL)), Requirement.Create("Percentage", "NSW03"), Requirement.Create("LinesBet", new Credits(50UL)), Requirement.Create("BetMultiplier", 8L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 482720UL, 485000UL, 550000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 10000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 80000UL, 100000UL, 100000UL),
								new ReplacementTable("MINOR", 28, 15000UL, 15000UL, 10000UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1280UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 432720UL, 445000UL, 395000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 10000UL, 20000UL),
								new ReplacementTable("MINI", 27, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("MINOR", 28, 60000UL, 60000UL, 20000UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1280UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(2UL)), Requirement.Create("Percentage", "NSW03", "CAS03"), Requirement.Create("LinesBet", new Credits(50UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 518400UL, 545000UL, 599900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 10000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 60000UL, 60000UL, 60000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1600UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 428400UL, 460000UL, 444900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 100000UL, 100000UL, 50000UL),
								new ReplacementTable("MINOR", 28, 40000UL, 30000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1600UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(5UL)), Requirement.Create("Percentage", "NSW03"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 6L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 509600UL, 542000UL, 609900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("MINOR", 28, 8000UL, 8000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 30000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2400UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 487600UL, 520000UL, 444900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 60000UL, 60000UL, 50000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 100UL),
								new ReplacementTable("MAJOR", 29, 30000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2400UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW03", "QLD03"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 582200UL, 596500UL, 621500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3500UL, 3500UL, 3500UL),
								new ReplacementTable("MINOR", 28, 3500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 572200UL, 586500UL, 471500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 3500UL, 3500UL, 3500UL),
								new ReplacementTable("MINOR", 28, 3500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW03", "QLD03"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 564400UL, 593000UL, 623000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 7000UL, 7000UL, 7000UL),
								new ReplacementTable("MINOR", 28, 7000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1600UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 534400UL, 563000UL, 478000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 7000UL, 7000UL, 7000UL),
								new ReplacementTable("MINOR", 28, 7000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1600UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW03", "QLD03"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 537600UL, 577500UL, 627500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 7500UL, 7500UL, 7500UL),
								new ReplacementTable("MINOR", 28, 7500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 30000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2400UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 492600UL, 532500UL, 407500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 7500UL, 7500UL, 7500UL),
								new ReplacementTable("MINOR", 28, 7500UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 30000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2400UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "NSW03", "QLD03"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 4L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 525800UL, 577000UL, 627000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 8000UL, 8000UL, 8000UL),
								new ReplacementTable("MINOR", 28, 8000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 345800UL, 397000UL, 262000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 200000UL),
								new ReplacementTable("C3", 17, 160000UL, 160000UL, 160000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("C6", 20, 40000UL, 40000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 8000UL, 8000UL, 8000UL),
								new ReplacementTable("MINOR", 28, 8000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "NSW03", "QLD03"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 576400UL, 612000UL, 642000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 14000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1600UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 534900UL, 586500UL, 471500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 3500UL, 3500UL, 3500UL),
								new ReplacementTable("MINOR", 28, 30000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1600UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "NSW03"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 526800UL, 590000UL, 640000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 20000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 496800UL, 580000UL, 485000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 40000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "NSW03"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 538800UL, 597000UL, 642000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 15000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 513300UL, 586500UL, 451500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 3500UL, 3500UL, 3500UL),
								new ReplacementTable("MINOR", 28, 30000UL, 0UL, 0UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(1UL)), Requirement.Create("Percentage", "NSW03", "CAS03"), Requirement.Create("LinesBet", new Credits(50UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 448400UL, 475000UL, 550000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 10000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("MINOR", 28, 25000UL, 25000UL, 10000UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1600UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 388400UL, 405000UL, 385000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 10000UL, 20000UL),
								new ReplacementTable("MINI", 27, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("MINOR", 28, 100000UL, 100000UL, 30000UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1600UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(2UL)), Requirement.Create("Percentage", "NSW03", "CAS03"), Requirement.Create("LinesBet", new Credits(50UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 426800UL, 475000UL, 550000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 10000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("MINOR", 28, 25000UL, 25000UL, 10000UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 366800UL, 405000UL, 385000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 10000UL, 20000UL),
								new ReplacementTable("MINI", 27, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("MINOR", 28, 100000UL, 100000UL, 30000UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(5UL)), Requirement.Create("Percentage", "NSW03"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 8L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 493800UL, 537000UL, 587500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 10000UL),
								new ReplacementTable("C5", 19, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 20000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 8000UL, 8000UL, 2500UL),
								new ReplacementTable("MINI", 27, 70000UL, 70000UL, 70000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 466800UL, 507000UL, 435000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 8000UL, 5000UL),
								new ReplacementTable("MINI", 27, 90000UL, 90000UL, 90000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 15000UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(5UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 588775UL, 594000UL, 644950UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 50UL),
								new ReplacementTable("MAJOR", 29, 5000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 225UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 569775UL, 575000UL, 499950UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 12500UL, 12500UL, 5000UL),
								new ReplacementTable("MINOR", 28, 2500UL, 2500UL, 50UL),
								new ReplacementTable("MAJOR", 29, 5000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 225UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(10UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 583550UL, 594000UL, 644950UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 50UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 450UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 564550UL, 575000UL, 499950UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 12500UL, 12500UL, 5000UL),
								new ReplacementTable("MINOR", 28, 2500UL, 2500UL, 50UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 450UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 597630UL, 599720UL, 634720UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 200UL, 200UL, 200UL),
								new ReplacementTable("MINOR", 28, 80UL, 80UL, 80UL),
								new ReplacementTable("MAJOR", 29, 2000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 90UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 452630UL, 454720UL, 289720UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 200000UL),
								new ReplacementTable("C3", 17, 160000UL, 160000UL, 160000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 200UL, 200UL, 200UL),
								new ReplacementTable("MINOR", 28, 80UL, 80UL, 80UL),
								new ReplacementTable("MAJOR", 29, 2000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 90UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 594820UL, 599000UL, 634000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 800UL, 800UL, 800UL),
								new ReplacementTable("MINOR", 28, 200UL, 200UL, 200UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 180UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 584820UL, 589000UL, 474000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 800UL, 800UL, 800UL),
								new ReplacementTable("MINOR", 28, 200UL, 200UL, 200UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 180UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 592530UL, 598800UL, 628800UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 900UL, 900UL, 900UL),
								new ReplacementTable("MINOR", 28, 300UL, 300UL, 300UL),
								new ReplacementTable("MAJOR", 29, 6000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 270UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 562530UL, 568800UL, 483800UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 900UL, 900UL, 900UL),
								new ReplacementTable("MINOR", 28, 300UL, 300UL, 300UL),
								new ReplacementTable("MAJOR", 29, 6000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 270UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 572550UL, 583000UL, 633000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 450UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 527550UL, 538000UL, 413000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 450UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 574100UL, 595000UL, 630000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 4000UL, 4000UL, 4000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 900UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 564100UL, 585000UL, 470000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 4000UL, 4000UL, 4000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 900UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 595220UL, 599400UL, 634500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 400UL, 400UL, 400UL),
								new ReplacementTable("MINOR", 28, 200UL, 200UL, 100UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 180UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 450120UL, 454300UL, 289500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 200000UL),
								new ReplacementTable("C3", 17, 160000UL, 160000UL, 160000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 500UL, 500UL, 400UL),
								new ReplacementTable("MINOR", 28, 200UL, 200UL, 100UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 180UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 590240UL, 598600UL, 633600UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 360UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 580240UL, 588600UL, 473600UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 360UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 585360UL, 597900UL, 632900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1500UL, 1500UL, 1500UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 540UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 575360UL, 587900UL, 472900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1500UL, 1500UL, 1500UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 540UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 574300UL, 595200UL, 630200UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 4000UL, 4000UL, 4000UL),
								new ReplacementTable("MINOR", 28, 800UL, 800UL, 800UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 900UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 564300UL, 585200UL, 440200UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 4000UL, 4000UL, 4000UL),
								new ReplacementTable("MINOR", 28, 800UL, 800UL, 800UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 900UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 547200UL, 589000UL, 624000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 537200UL, 579000UL, 464000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 591040UL, 599400UL, 634500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 400UL, 400UL, 400UL),
								new ReplacementTable("MINOR", 28, 200UL, 200UL, 100UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 360UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 445940UL, 454300UL, 289500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 200000UL),
								new ReplacementTable("C3", 17, 160000UL, 160000UL, 160000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 500UL, 500UL, 400UL),
								new ReplacementTable("MINOR", 28, 200UL, 200UL, 100UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 360UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 581880UL, 598600UL, 633600UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 720UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 571880UL, 588600UL, 473600UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 720UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 572820UL, 597900UL, 632900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1500UL, 1500UL, 1500UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1080UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 562820UL, 587900UL, 472900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1500UL, 1500UL, 1500UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1080UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 553400UL, 595200UL, 630200UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 4000UL, 4000UL, 4000UL),
								new ReplacementTable("MINOR", 28, 800UL, 800UL, 800UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 543400UL, 585200UL, 440200UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 4000UL, 4000UL, 4000UL),
								new ReplacementTable("MINOR", 28, 800UL, 800UL, 800UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 505400UL, 589000UL, 624000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 80000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3600UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 495400UL, 579000UL, 464000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 80000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3600UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(5UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 579550UL, 590000UL, 642450UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 12500UL, 12500UL, 12500UL),
								new ReplacementTable("MINOR", 28, 2500UL, 2500UL, 50UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 450UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 559550UL, 570000UL, 504950UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 35000UL, 35000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 50UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 450UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(10UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 569100UL, 590000UL, 642450UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 12500UL, 12500UL, 12500UL),
								new ReplacementTable("MINOR", 28, 2500UL, 2500UL, 50UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 900UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 549100UL, 570000UL, 504950UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 35000UL, 35000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 50UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 900UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 594820UL, 599000UL, 634000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 800UL, 800UL, 800UL),
								new ReplacementTable("MINOR", 28, 200UL, 200UL, 200UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 180UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 584820UL, 589000UL, 474000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 800UL, 800UL, 800UL),
								new ReplacementTable("MINOR", 28, 200UL, 200UL, 200UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 180UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 599640UL, 608000UL, 643000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 360UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 579640UL, 588000UL, 473000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 360UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 585060UL, 597600UL, 627600UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1800UL, 1800UL, 1800UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 540UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 555060UL, 567600UL, 482600UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 1800UL, 1800UL, 1800UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 540UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 580900UL, 601800UL, 611800UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2400UL, 2400UL, 2400UL),
								new ReplacementTable("MINOR", 28, 800UL, 800UL, 800UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 900UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 565900UL, 586800UL, 441800UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2400UL, 2400UL, 2400UL),
								new ReplacementTable("MINOR", 28, 800UL, 800UL, 800UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 900UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 553200UL, 595000UL, 635000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 8000UL, 8000UL, 8000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 538200UL, 580000UL, 465000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 8000UL, 8000UL, 8000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 590240UL, 598600UL, 633600UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 360UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 580240UL, 588600UL, 473600UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 360UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 585180UL, 601900UL, 641900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 720UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 570180UL, 586900UL, 471900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 720UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 575920UL, 601000UL, 641000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1080UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 560920UL, 586000UL, 471000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1080UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 556000UL, 597800UL, 607800UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 1200UL, 1200UL, 1200UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 541000UL, 582800UL, 437800UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 1200UL, 1200UL, 1200UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 499400UL, 583000UL, 623000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 80000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3600UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 484400UL, 568000UL, 453000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 80000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3600UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 581880UL, 598600UL, 633600UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 720UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 571880UL, 588600UL, 473600UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 720UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 568460UL, 601900UL, 641900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 32000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1440UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 553460UL, 586900UL, 471900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 32000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1440UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 550840UL, 601000UL, 641000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 48000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2160UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 535840UL, 586000UL, 471000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 48000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2160UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 514200UL, 597800UL, 607800UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 1200UL, 1200UL, 1200UL),
								new ReplacementTable("MAJOR", 29, 80000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3600UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 499200UL, 582800UL, 437800UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 1200UL, 1200UL, 1200UL),
								new ReplacementTable("MAJOR", 29, 80000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3600UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 415800UL, 583000UL, 623000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 160000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 7200UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 400800UL, 568000UL, 453000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 160000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 7200UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(5UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 565325UL, 581000UL, 634950UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 15000UL, 15000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 4000UL, 4000UL, 50UL),
								new ReplacementTable("MAJOR", 29, 15000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 675UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 524325UL, 542500UL, 489950UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 40000UL, 40000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 7500UL, 50UL),
								new ReplacementTable("MAJOR", 29, 15000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 675UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(10UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 549650UL, 581000UL, 634950UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 15000UL, 15000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 4000UL, 4000UL, 50UL),
								new ReplacementTable("MAJOR", 29, 30000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1350UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 508650UL, 542500UL, 489950UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 40000UL, 40000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 7500UL, 50UL),
								new ReplacementTable("MAJOR", 29, 30000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1350UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 592230UL, 598500UL, 633500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1200UL, 1200UL, 1200UL),
								new ReplacementTable("MINOR", 28, 300UL, 300UL, 300UL),
								new ReplacementTable("MAJOR", 29, 6000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 270UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 582230UL, 588500UL, 473500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1200UL, 1200UL, 1200UL),
								new ReplacementTable("MINOR", 28, 300UL, 300UL, 300UL),
								new ReplacementTable("MAJOR", 29, 6000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 270UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 584460UL, 597000UL, 632000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2400UL, 2400UL, 2400UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 540UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 574460UL, 587000UL, 472000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2400UL, 2400UL, 2400UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 540UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 577590UL, 596400UL, 626400UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2700UL, 2700UL, 2700UL),
								new ReplacementTable("MINOR", 28, 900UL, 900UL, 900UL),
								new ReplacementTable("MAJOR", 29, 18000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 810UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 547590UL, 566400UL, 481400UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 2700UL, 2700UL, 2700UL),
								new ReplacementTable("MINOR", 28, 900UL, 900UL, 900UL),
								new ReplacementTable("MAJOR", 29, 18000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 810UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 563050UL, 594400UL, 609400UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 4200UL, 4200UL, 4200UL),
								new ReplacementTable("MINOR", 28, 1400UL, 1400UL, 1400UL),
								new ReplacementTable("MAJOR", 29, 30000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1350UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 553050UL, 584400UL, 419400UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 4200UL, 4200UL, 4200UL),
								new ReplacementTable("MINOR", 28, 1400UL, 1400UL, 1400UL),
								new ReplacementTable("MAJOR", 29, 30000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1350UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 524300UL, 587000UL, 622000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MAJOR", 29, 60000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2700UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 514300UL, 577000UL, 462000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MAJOR", 29, 60000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2700UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 585260UL, 597800UL, 632800UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 540UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 574860UL, 587400UL, 472800UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2000UL, 2000UL, 1600UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 540UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 569120UL, 594200UL, 629200UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINOR", 28, 800UL, 800UL, 800UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1080UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 559120UL, 584200UL, 469200UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINOR", 28, 800UL, 800UL, 800UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1080UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 554780UL, 592400UL, 627400UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MAJOR", 29, 36000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1620UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 544780UL, 582400UL, 467400UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MAJOR", 29, 36000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1620UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 525300UL, 588000UL, 603000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 60000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2700UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 515300UL, 578000UL, 413000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 60000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2700UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 439600UL, 565000UL, 600000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 120000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 5400UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 429600UL, 555000UL, 440000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 120000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 5400UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 572720UL, 597800UL, 632800UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1080UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 562320UL, 587400UL, 472800UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2000UL, 2000UL, 1600UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1080UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 544040UL, 594200UL, 629200UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINOR", 28, 800UL, 800UL, 800UL),
								new ReplacementTable("MAJOR", 29, 48000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2160UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 534040UL, 584200UL, 469200UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINOR", 28, 800UL, 800UL, 800UL),
								new ReplacementTable("MAJOR", 29, 48000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2160UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 517160UL, 592400UL, 627400UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MAJOR", 29, 72000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3240UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 507160UL, 582400UL, 467400UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MAJOR", 29, 72000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3240UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 462600UL, 588000UL, 603000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 120000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 5400UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 452600UL, 578000UL, 413000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 120000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 5400UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 314200UL, 565000UL, 600000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 240000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 10800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 304200UL, 555000UL, 440000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 240000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 10800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(5UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 548875UL, 580000UL, 629950UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 10000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 50UL),
								new ReplacementTable("MAJOR", 29, 25000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1125UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 493875UL, 525000UL, 469950UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 50000UL, 50000UL, 25000UL),
								new ReplacementTable("MINOR", 28, 20000UL, 15000UL, 50UL),
								new ReplacementTable("MAJOR", 29, 25000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1125UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(10UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 522750UL, 580000UL, 629950UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 10000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 50UL),
								new ReplacementTable("MAJOR", 29, 50000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2250UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 467750UL, 525000UL, 469950UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 50000UL, 50000UL, 25000UL),
								new ReplacementTable("MINOR", 28, 20000UL, 15000UL, 50UL),
								new ReplacementTable("MAJOR", 29, 50000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2250UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 589640UL, 598000UL, 623000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 360UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 579640UL, 588000UL, 473000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 360UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 579280UL, 596000UL, 631000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3200UL, 3200UL, 3200UL),
								new ReplacementTable("MINOR", 28, 800UL, 800UL, 800UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 720UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 569280UL, 586000UL, 461000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 3200UL, 3200UL, 3200UL),
								new ReplacementTable("MINOR", 28, 800UL, 800UL, 800UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 720UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 570120UL, 595200UL, 625200UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3600UL, 3600UL, 3600UL),
								new ReplacementTable("MINOR", 28, 1200UL, 1200UL, 1200UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1080UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 540120UL, 565200UL, 480200UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 3600UL, 3600UL, 3600UL),
								new ReplacementTable("MINOR", 28, 1200UL, 1200UL, 1200UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1080UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 550200UL, 592000UL, 627000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 540200UL, 582000UL, 457000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 507400UL, 591000UL, 616000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 15000UL, 15000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 4000UL, 4000UL, 4000UL),
								new ReplacementTable("MAJOR", 29, 80000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3600UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 497400UL, 581000UL, 456000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 20000UL, 20000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 15000UL, 15000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 4000UL, 4000UL, 4000UL),
								new ReplacementTable("MAJOR", 29, 80000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3600UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 595080UL, 611800UL, 641800UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 700UL, 700UL, 700UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 720UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 570080UL, 586800UL, 471800UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 700UL, 700UL, 700UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 720UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 558560UL, 592000UL, 627000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 7000UL, 7000UL, 7000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 32000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1440UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 548560UL, 582000UL, 457000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 7000UL, 7000UL, 7000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 32000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1440UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 539840UL, 590000UL, 625000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 8000UL, 8000UL, 8000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 48000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2160UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 529840UL, 580000UL, 455000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 8000UL, 8000UL, 8000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 48000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2160UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 498400UL, 582000UL, 617000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 15000UL, 15000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MAJOR", 29, 80000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3600UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 488400UL, 572000UL, 447000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 15000UL, 15000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MAJOR", 29, 80000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3600UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 396800UL, 564000UL, 589000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 40000UL, 40000UL, 40000UL),
								new ReplacementTable("MINOR", 28, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MAJOR", 29, 160000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 7200UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 386800UL, 554000UL, 429000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 20000UL, 20000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 40000UL, 40000UL, 40000UL),
								new ReplacementTable("MINOR", 28, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MAJOR", 29, 160000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 7200UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 578360UL, 611800UL, 641800UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 700UL, 700UL, 700UL),
								new ReplacementTable("MAJOR", 29, 32000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1440UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 553360UL, 586800UL, 471800UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 700UL, 700UL, 700UL),
								new ReplacementTable("MAJOR", 29, 32000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1440UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 525120UL, 592000UL, 627000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 7000UL, 7000UL, 7000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 64000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2880UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 515120UL, 582000UL, 457000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 7000UL, 7000UL, 7000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 64000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2880UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 489680UL, 590000UL, 625000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 8000UL, 8000UL, 8000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 96000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 4320UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 479680UL, 580000UL, 455000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 8000UL, 8000UL, 8000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 96000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 4320UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 414800UL, 582000UL, 617000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 15000UL, 15000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MAJOR", 29, 160000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 7200UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 404800UL, 572000UL, 447000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 15000UL, 15000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MAJOR", 29, 160000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 7200UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 229600UL, 564000UL, 589000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 40000UL, 40000UL, 40000UL),
								new ReplacementTable("MINOR", 28, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MAJOR", 29, 320000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 14400UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 219600UL, 554000UL, 429000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 20000UL, 20000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 40000UL, 40000UL, 40000UL),
								new ReplacementTable("MINOR", 28, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MAJOR", 29, 320000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 14400UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(5UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 480250UL, 537500UL, 605000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 10000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("MINOR", 28, 12500UL, 12500UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 50000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2250UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 457750UL, 505000UL, 450000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 10000UL, 20000UL),
								new ReplacementTable("MINI", 27, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("MINOR", 28, 50000UL, 50000UL, 15000UL),
								new ReplacementTable("MAJOR", 29, 50000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2250UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(10UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 428000UL, 537500UL, 605000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 10000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("MINOR", 28, 12500UL, 12500UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 100000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 4500UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 405500UL, 505000UL, 450000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 10000UL, 20000UL),
								new ReplacementTable("MINI", 27, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("MINOR", 28, 50000UL, 50000UL, 15000UL),
								new ReplacementTable("MAJOR", 29, 100000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 4500UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 587050UL, 597500UL, 622500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MINOR", 28, 500UL, 500UL, 500UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 450UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 577050UL, 587500UL, 472500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MINOR", 28, 500UL, 500UL, 500UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 450UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 574100UL, 595000UL, 625000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 4000UL, 4000UL, 4000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 900UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 544100UL, 565000UL, 480000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 4000UL, 4000UL, 4000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 900UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 548050UL, 579400UL, 629400UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 4200UL, 4200UL, 4200UL),
								new ReplacementTable("MINOR", 28, 1400UL, 1400UL, 1400UL),
								new ReplacementTable("MAJOR", 29, 30000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1350UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 503050UL, 534400UL, 409400UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 4200UL, 4200UL, 4200UL),
								new ReplacementTable("MINOR", 28, 1400UL, 1400UL, 1400UL),
								new ReplacementTable("MAJOR", 29, 30000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1350UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 534750UL, 587000UL, 637000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MAJOR", 29, 50000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2250UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 524750UL, 577000UL, 482000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MAJOR", 29, 50000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2250UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 470500UL, 575000UL, 625000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 100000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 4500UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 460500UL, 565000UL, 470000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 100000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 4500UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 590200UL, 611100UL, 641100UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 900UL, 900UL, 900UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 900UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 565200UL, 586100UL, 471100UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 900UL, 900UL, 900UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 900UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 547000UL, 588800UL, 638800UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 1200UL, 1200UL, 1200UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 537000UL, 578800UL, 483800UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 1200UL, 1200UL, 1200UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 524800UL, 587500UL, 622500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MAJOR", 29, 60000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2700UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 514800UL, 577500UL, 422500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MAJOR", 29, 60000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2700UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 470500UL, 575000UL, 625000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 100000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 4500UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 460500UL, 565000UL, 470000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 100000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 4500UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 331000UL, 540000UL, 590000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MAJOR", 29, 200000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 9000UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 321000UL, 530000UL, 435000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MAJOR", 29, 200000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 9000UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 569300UL, 611100UL, 641100UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 900UL, 900UL, 900UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 544300UL, 586100UL, 471100UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 900UL, 900UL, 900UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 505200UL, 588800UL, 638800UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 1200UL, 1200UL, 1200UL),
								new ReplacementTable("MAJOR", 29, 80000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3600UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 495200UL, 578800UL, 483800UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 1200UL, 1200UL, 1200UL),
								new ReplacementTable("MAJOR", 29, 80000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3600UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 462100UL, 587500UL, 622500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MAJOR", 29, 120000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 5400UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 452100UL, 577500UL, 422500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MAJOR", 29, 120000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 5400UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 366000UL, 575000UL, 625000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 200000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 9000UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 356000UL, 565000UL, 470000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 200000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 9000UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS01", "CAS02"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 122000UL, 540000UL, 590000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MAJOR", 29, 400000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 18000UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 112000UL, 530000UL, 435000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MAJOR", 29, 400000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 18000UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(5UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 588600UL, 594000UL, 644950UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 50UL),
								new ReplacementTable("MAJOR", 29, 5000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 400UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 569600UL, 575000UL, 499950UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 12500UL, 12500UL, 5000UL),
								new ReplacementTable("MINOR", 28, 2500UL, 2500UL, 50UL),
								new ReplacementTable("MAJOR", 29, 5000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 400UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(10UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 583200UL, 594000UL, 644950UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 50UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 564200UL, 575000UL, 499950UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 12500UL, 12500UL, 5000UL),
								new ReplacementTable("MINOR", 28, 2500UL, 2500UL, 50UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 597560UL, 599720UL, 634720UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 200UL, 200UL, 200UL),
								new ReplacementTable("MINOR", 28, 80UL, 80UL, 80UL),
								new ReplacementTable("MAJOR", 29, 2000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 160UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 452560UL, 454720UL, 289720UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 200000UL),
								new ReplacementTable("C3", 17, 160000UL, 160000UL, 160000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 200UL, 200UL, 200UL),
								new ReplacementTable("MINOR", 28, 80UL, 80UL, 80UL),
								new ReplacementTable("MAJOR", 29, 2000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 160UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 594680UL, 599000UL, 634000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 800UL, 800UL, 800UL),
								new ReplacementTable("MINOR", 28, 200UL, 200UL, 200UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 320UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 584680UL, 589000UL, 474000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 800UL, 800UL, 800UL),
								new ReplacementTable("MINOR", 28, 200UL, 200UL, 200UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 320UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 592320UL, 598800UL, 628800UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 900UL, 900UL, 900UL),
								new ReplacementTable("MINOR", 28, 300UL, 300UL, 300UL),
								new ReplacementTable("MAJOR", 29, 6000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 480UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 562320UL, 568800UL, 483800UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 900UL, 900UL, 900UL),
								new ReplacementTable("MINOR", 28, 300UL, 300UL, 300UL),
								new ReplacementTable("MAJOR", 29, 6000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 480UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 572200UL, 583000UL, 633000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 527200UL, 538000UL, 413000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 573400UL, 595000UL, 630000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 4000UL, 4000UL, 4000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1600UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 563400UL, 585000UL, 470000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 4000UL, 4000UL, 4000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1600UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 595080UL, 599400UL, 634500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 400UL, 400UL, 400UL),
								new ReplacementTable("MINOR", 28, 200UL, 200UL, 100UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 320UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 449980UL, 454300UL, 289500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 200000UL),
								new ReplacementTable("C3", 17, 160000UL, 160000UL, 160000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 500UL, 500UL, 400UL),
								new ReplacementTable("MINOR", 28, 200UL, 200UL, 100UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 320UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 589960UL, 598600UL, 633600UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 640UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 579960UL, 588600UL, 473600UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 640UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 584940UL, 597900UL, 632900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1500UL, 1500UL, 1500UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 960UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 574940UL, 587900UL, 472900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1500UL, 1500UL, 1500UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 960UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 573600UL, 595200UL, 630200UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 4000UL, 4000UL, 4000UL),
								new ReplacementTable("MINOR", 28, 800UL, 800UL, 800UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1600UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 563600UL, 585200UL, 440200UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 4000UL, 4000UL, 4000UL),
								new ReplacementTable("MINOR", 28, 800UL, 800UL, 800UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1600UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 545800UL, 589000UL, 624000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 535800UL, 579000UL, 464000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 590760UL, 599400UL, 634500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 400UL, 400UL, 400UL),
								new ReplacementTable("MINOR", 28, 200UL, 200UL, 100UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 640UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 445660UL, 454300UL, 289500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 200000UL),
								new ReplacementTable("C3", 17, 160000UL, 160000UL, 160000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 500UL, 500UL, 400UL),
								new ReplacementTable("MINOR", 28, 200UL, 200UL, 100UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 640UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 581320UL, 598600UL, 633600UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1280UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 571320UL, 588600UL, 473600UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1280UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 571980UL, 597900UL, 632900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1500UL, 1500UL, 1500UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1920UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 561980UL, 587900UL, 472900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1500UL, 1500UL, 1500UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1920UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 552000UL, 595200UL, 630200UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 4000UL, 4000UL, 4000UL),
								new ReplacementTable("MINOR", 28, 800UL, 800UL, 800UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 542000UL, 585200UL, 440200UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 4000UL, 4000UL, 4000UL),
								new ReplacementTable("MINOR", 28, 800UL, 800UL, 800UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(1UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 502600UL, 589000UL, 624000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 80000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 6400UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 492600UL, 579000UL, 464000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 80000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 6400UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(5UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 579200UL, 590000UL, 642450UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 12500UL, 12500UL, 12500UL),
								new ReplacementTable("MINOR", 28, 2500UL, 2500UL, 50UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 559200UL, 570000UL, 504950UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 35000UL, 35000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 50UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(10UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 568400UL, 590000UL, 642450UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 12500UL, 12500UL, 12500UL),
								new ReplacementTable("MINOR", 28, 2500UL, 2500UL, 50UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1600UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 548400UL, 570000UL, 504950UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 35000UL, 35000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 50UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1600UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 594680UL, 599000UL, 634000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 800UL, 800UL, 800UL),
								new ReplacementTable("MINOR", 28, 200UL, 200UL, 200UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 320UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 584680UL, 589000UL, 474000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 800UL, 800UL, 800UL),
								new ReplacementTable("MINOR", 28, 200UL, 200UL, 200UL),
								new ReplacementTable("MAJOR", 29, 4000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 320UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 599360UL, 608000UL, 643000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 640UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 579360UL, 588000UL, 473000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 640UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 584640UL, 597600UL, 627600UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1800UL, 1800UL, 1800UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 960UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 554640UL, 567600UL, 482600UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 1800UL, 1800UL, 1800UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 960UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 580200UL, 601800UL, 611800UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2400UL, 2400UL, 2400UL),
								new ReplacementTable("MINOR", 28, 800UL, 800UL, 800UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1600UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 565200UL, 586800UL, 441800UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2400UL, 2400UL, 2400UL),
								new ReplacementTable("MINOR", 28, 800UL, 800UL, 800UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1600UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 556800UL, 600000UL, 635000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 8000UL, 8000UL, 8000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 536800UL, 580000UL, 465000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 8000UL, 8000UL, 8000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 589960UL, 598600UL, 633600UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 640UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 579960UL, 588600UL, 473600UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 640UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 584620UL, 601900UL, 641900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1280UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 569620UL, 586900UL, 471900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1280UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 575080UL, 601000UL, 641000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1920UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 560080UL, 586000UL, 471000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1920UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 554600UL, 597800UL, 607800UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 1200UL, 1200UL, 1200UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 539600UL, 582800UL, 437800UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 1200UL, 1200UL, 1200UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 496600UL, 583000UL, 623000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 80000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 6400UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 481600UL, 568000UL, 453000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 80000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 6400UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 581320UL, 598600UL, 633600UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1280UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 571320UL, 588600UL, 473600UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1280UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 567340UL, 601900UL, 641900UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 32000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2560UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 552340UL, 586900UL, 471900UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 32000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2560UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 549160UL, 601000UL, 641000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 48000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3840UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 534160UL, 586000UL, 471000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 48000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3840UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 511400UL, 597800UL, 607800UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 1200UL, 1200UL, 1200UL),
								new ReplacementTable("MAJOR", 29, 80000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 6400UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 496400UL, 582800UL, 437800UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 1200UL, 1200UL, 1200UL),
								new ReplacementTable("MAJOR", 29, 80000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 6400UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(2UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 410200UL, 583000UL, 623000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 20000UL, 20000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 160000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 12800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 395200UL, 568000UL, 453000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 160000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 12800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(5UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 564800UL, 581000UL, 634950UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 15000UL, 15000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 4000UL, 4000UL, 50UL),
								new ReplacementTable("MAJOR", 29, 15000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1200UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 523800UL, 542500UL, 489950UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 40000UL, 40000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 7500UL, 50UL),
								new ReplacementTable("MAJOR", 29, 15000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1200UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(10UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 548600UL, 581000UL, 634950UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 15000UL, 15000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 4000UL, 4000UL, 50UL),
								new ReplacementTable("MAJOR", 29, 30000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2400UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 507600UL, 542500UL, 489950UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 40000UL, 40000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 7500UL, 50UL),
								new ReplacementTable("MAJOR", 29, 30000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2400UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 592020UL, 598500UL, 633500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1200UL, 1200UL, 1200UL),
								new ReplacementTable("MINOR", 28, 300UL, 300UL, 300UL),
								new ReplacementTable("MAJOR", 29, 6000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 480UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 582020UL, 588500UL, 473500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1200UL, 1200UL, 1200UL),
								new ReplacementTable("MINOR", 28, 300UL, 300UL, 300UL),
								new ReplacementTable("MAJOR", 29, 6000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 480UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 584040UL, 597000UL, 632000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2400UL, 2400UL, 2400UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 960UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 574040UL, 587000UL, 472000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2400UL, 2400UL, 2400UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 960UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 576960UL, 596400UL, 626400UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2700UL, 2700UL, 2700UL),
								new ReplacementTable("MINOR", 28, 900UL, 900UL, 900UL),
								new ReplacementTable("MAJOR", 29, 18000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1440UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 546960UL, 566400UL, 481400UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 2700UL, 2700UL, 2700UL),
								new ReplacementTable("MINOR", 28, 900UL, 900UL, 900UL),
								new ReplacementTable("MAJOR", 29, 18000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1440UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 562000UL, 594400UL, 609400UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 4200UL, 4200UL, 4200UL),
								new ReplacementTable("MINOR", 28, 1400UL, 1400UL, 1400UL),
								new ReplacementTable("MAJOR", 29, 30000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2400UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 552000UL, 584400UL, 419400UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 4200UL, 4200UL, 4200UL),
								new ReplacementTable("MINOR", 28, 1400UL, 1400UL, 1400UL),
								new ReplacementTable("MAJOR", 29, 30000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2400UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 522200UL, 587000UL, 622000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MAJOR", 29, 60000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 4800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 512200UL, 577000UL, 462000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MAJOR", 29, 60000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 4800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 584840UL, 597800UL, 632800UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 960UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 574440UL, 587400UL, 472800UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2000UL, 2000UL, 1600UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 12000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 960UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 568280UL, 594200UL, 629200UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINOR", 28, 800UL, 800UL, 800UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1920UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 558280UL, 584200UL, 469200UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINOR", 28, 800UL, 800UL, 800UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1920UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 553520UL, 592400UL, 627400UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MAJOR", 29, 36000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2880UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 543520UL, 582400UL, 467400UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MAJOR", 29, 36000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2880UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 523200UL, 588000UL, 603000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 60000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 4800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 513200UL, 578000UL, 413000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 60000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 4800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 435400UL, 565000UL, 600000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 120000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 9600UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 425400UL, 555000UL, 440000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 120000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 9600UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 571880UL, 597800UL, 632800UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1920UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 561480UL, 587400UL, 472800UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2000UL, 2000UL, 1600UL),
								new ReplacementTable("MINOR", 28, 600UL, 600UL, 600UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1920UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 542360UL, 594200UL, 629200UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINOR", 28, 800UL, 800UL, 800UL),
								new ReplacementTable("MAJOR", 29, 48000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3840UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 532360UL, 584200UL, 469200UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINOR", 28, 800UL, 800UL, 800UL),
								new ReplacementTable("MAJOR", 29, 48000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3840UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 514640UL, 592400UL, 627400UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MAJOR", 29, 72000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 5760UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 504640UL, 582400UL, 467400UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MAJOR", 29, 72000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 5760UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 458400UL, 588000UL, 603000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 120000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 9600UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 448400UL, 578000UL, 413000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 120000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 9600UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(3UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 305800UL, 565000UL, 600000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 240000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 19200UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 295800UL, 555000UL, 440000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 240000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 19200UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(5UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 548000UL, 580000UL, 629950UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 10000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 50UL),
								new ReplacementTable("MAJOR", 29, 25000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2000UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 493000UL, 525000UL, 469950UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 50000UL, 50000UL, 25000UL),
								new ReplacementTable("MINOR", 28, 20000UL, 15000UL, 50UL),
								new ReplacementTable("MAJOR", 29, 25000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2000UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(10UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 521000UL, 580000UL, 629950UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 10000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 50UL),
								new ReplacementTable("MAJOR", 29, 50000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 4000UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 466000UL, 525000UL, 469950UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 50000UL, 50000UL, 25000UL),
								new ReplacementTable("MINOR", 28, 20000UL, 15000UL, 50UL),
								new ReplacementTable("MAJOR", 29, 50000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 4000UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 589360UL, 598000UL, 623000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 640UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 579360UL, 588000UL, 473000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 1600UL, 1600UL, 1600UL),
								new ReplacementTable("MINOR", 28, 400UL, 400UL, 400UL),
								new ReplacementTable("MAJOR", 29, 8000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 640UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 578720UL, 596000UL, 631000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3200UL, 3200UL, 3200UL),
								new ReplacementTable("MINOR", 28, 800UL, 800UL, 800UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1280UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 568720UL, 586000UL, 461000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 3200UL, 3200UL, 3200UL),
								new ReplacementTable("MINOR", 28, 800UL, 800UL, 800UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1280UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 569280UL, 595200UL, 625200UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3600UL, 3600UL, 3600UL),
								new ReplacementTable("MINOR", 28, 1200UL, 1200UL, 1200UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1920UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 539280UL, 565200UL, 480200UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 3600UL, 3600UL, 3600UL),
								new ReplacementTable("MINOR", 28, 1200UL, 1200UL, 1200UL),
								new ReplacementTable("MAJOR", 29, 24000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1920UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 548800UL, 592000UL, 627000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 538800UL, 582000UL, 457000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 504600UL, 591000UL, 616000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 15000UL, 15000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 4000UL, 4000UL, 4000UL),
								new ReplacementTable("MAJOR", 29, 80000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 6400UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 494600UL, 581000UL, 456000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 20000UL, 20000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 15000UL, 15000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 4000UL, 4000UL, 4000UL),
								new ReplacementTable("MAJOR", 29, 80000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 6400UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 594520UL, 611800UL, 641800UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 700UL, 700UL, 700UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1280UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 569520UL, 586800UL, 471800UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 700UL, 700UL, 700UL),
								new ReplacementTable("MAJOR", 29, 16000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1280UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 557440UL, 592000UL, 627000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 7000UL, 7000UL, 7000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 32000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2560UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 547440UL, 582000UL, 457000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 7000UL, 7000UL, 7000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 32000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2560UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 538160UL, 590000UL, 625000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 8000UL, 8000UL, 8000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 48000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3840UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 528160UL, 580000UL, 455000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 8000UL, 8000UL, 8000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 48000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3840UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 495600UL, 582000UL, 617000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 15000UL, 15000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MAJOR", 29, 80000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 6400UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 485600UL, 572000UL, 447000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 15000UL, 15000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MAJOR", 29, 80000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 6400UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 391200UL, 564000UL, 589000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 40000UL, 40000UL, 40000UL),
								new ReplacementTable("MINOR", 28, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MAJOR", 29, 160000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 12800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 381200UL, 554000UL, 429000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 20000UL, 20000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 40000UL, 40000UL, 40000UL),
								new ReplacementTable("MINOR", 28, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MAJOR", 29, 160000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 12800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 577240UL, 611800UL, 641800UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 700UL, 700UL, 700UL),
								new ReplacementTable("MAJOR", 29, 32000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2560UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 552240UL, 586800UL, 471800UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MINOR", 28, 700UL, 700UL, 700UL),
								new ReplacementTable("MAJOR", 29, 32000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2560UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 522880UL, 592000UL, 627000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 7000UL, 7000UL, 7000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 64000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 5120UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 512880UL, 582000UL, 457000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 7000UL, 7000UL, 7000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 64000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 5120UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 486320UL, 590000UL, 625000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 8000UL, 8000UL, 8000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 96000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 7680UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 476320UL, 580000UL, 455000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 8000UL, 8000UL, 8000UL),
								new ReplacementTable("MINOR", 28, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MAJOR", 29, 96000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 7680UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 409200UL, 582000UL, 617000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 15000UL, 15000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MAJOR", 29, 160000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 12800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 399200UL, 572000UL, 447000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 15000UL, 15000UL, 15000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MAJOR", 29, 160000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 12800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(4UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 218400UL, 564000UL, 589000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 40000UL, 40000UL, 40000UL),
								new ReplacementTable("MINOR", 28, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MAJOR", 29, 320000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 25600UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 208400UL, 554000UL, 429000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 20000UL, 20000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 40000UL, 40000UL, 40000UL),
								new ReplacementTable("MINOR", 28, 6000UL, 6000UL, 6000UL),
								new ReplacementTable("MAJOR", 29, 320000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 25600UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(5UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 478500UL, 537500UL, 605000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 10000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("MINOR", 28, 12500UL, 12500UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 50000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 4000UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 456000UL, 505000UL, 450000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 10000UL, 20000UL),
								new ReplacementTable("MINI", 27, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("MINOR", 28, 50000UL, 50000UL, 15000UL),
								new ReplacementTable("MAJOR", 29, 50000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 4000UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(10UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(25UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 424500UL, 537500UL, 605000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 10000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("MINOR", 28, 12500UL, 12500UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 100000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 8000UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 402000UL, 505000UL, 450000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 10000UL, 20000UL),
								new ReplacementTable("MINI", 27, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("MINOR", 28, 50000UL, 50000UL, 15000UL),
								new ReplacementTable("MAJOR", 29, 100000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 8000UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 586700UL, 597500UL, 622500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MINOR", 28, 500UL, 500UL, 500UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 576700UL, 587500UL, 472500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 2000UL, 2000UL, 2000UL),
								new ReplacementTable("MINOR", 28, 500UL, 500UL, 500UL),
								new ReplacementTable("MAJOR", 29, 10000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 573400UL, 595000UL, 625000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 4000UL, 4000UL, 4000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1600UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 543400UL, 565000UL, 480000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 4000UL, 4000UL, 4000UL),
								new ReplacementTable("MINOR", 28, 1000UL, 1000UL, 1000UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1600UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 547000UL, 579400UL, 629400UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 4200UL, 4200UL, 4200UL),
								new ReplacementTable("MINOR", 28, 1400UL, 1400UL, 1400UL),
								new ReplacementTable("MAJOR", 29, 30000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2400UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 502000UL, 534400UL, 409400UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 80000UL, 80000UL, 80000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 4200UL, 4200UL, 4200UL),
								new ReplacementTable("MINOR", 28, 1400UL, 1400UL, 1400UL),
								new ReplacementTable("MAJOR", 29, 30000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 2400UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 533000UL, 587000UL, 637000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MAJOR", 29, 50000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 4000UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 523000UL, 577000UL, 482000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MAJOR", 29, 50000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 4000UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 467000UL, 575000UL, 625000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 100000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 8000UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 457000UL, 565000UL, 470000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 100000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 8000UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 589500UL, 611100UL, 641100UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 900UL, 900UL, 900UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1600UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 564500UL, 586100UL, 471100UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 900UL, 900UL, 900UL),
								new ReplacementTable("MAJOR", 29, 20000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 1600UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 545600UL, 588800UL, 638800UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 1200UL, 1200UL, 1200UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 535600UL, 578800UL, 483800UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 1200UL, 1200UL, 1200UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 522700UL, 587500UL, 622500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MAJOR", 29, 60000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 4800UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 512700UL, 577500UL, 422500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MAJOR", 29, 60000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 4800UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 467000UL, 575000UL, 625000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 100000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 8000UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 457000UL, 565000UL, 470000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 100000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 8000UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 324000UL, 540000UL, 590000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MAJOR", 29, 200000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 16000UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 314000UL, 530000UL, 435000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MAJOR", 29, 200000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 16000UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 1L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 567900UL, 611100UL, 641100UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 10000UL, 10000UL, 5000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 900UL, 900UL, 900UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 542900UL, 586100UL, 471100UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 40000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 20000UL),
								new ReplacementTable("MINI", 27, 3000UL, 3000UL, 3000UL),
								new ReplacementTable("MINOR", 28, 900UL, 900UL, 900UL),
								new ReplacementTable("MAJOR", 29, 40000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 3200UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 2L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 502400UL, 588800UL, 638800UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 1200UL, 1200UL, 1200UL),
								new ReplacementTable("MAJOR", 29, 80000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 6400UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 492400UL, 578800UL, 483800UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 1200UL, 1200UL, 1200UL),
								new ReplacementTable("MAJOR", 29, 80000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 6400UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 3L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 457900UL, 587500UL, 622500UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 10000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MAJOR", 29, 120000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 9600UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 447900UL, 577500UL, 422500UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 50000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 50000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MINOR", 28, 2500UL, 2500UL, 2500UL),
								new ReplacementTable("MAJOR", 29, 120000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 9600UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 5L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 359000UL, 575000UL, 625000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 200000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 16000UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 349000UL, 565000UL, 470000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 20000UL, 20000UL, 20000UL),
								new ReplacementTable("MINOR", 28, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MAJOR", 29, 200000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 16000UL, 0UL, 0UL)
							}
						)
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(200UL)), Requirement.Create("Percentage", "CAS03"), Requirement.Create("LinesBet", new Credits(5UL)), Requirement.Create("BetMultiplier", 10L) },
						new ReplacementTables(
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 108000UL, 540000UL, 590000UL),
								new ReplacementTable("C2", 16, 200000UL, 200000UL, 170000UL),
								new ReplacementTable("C3", 17, 100000UL, 100000UL, 100000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 20000UL),
								new ReplacementTable("C6", 20, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C7", 21, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C8", 22, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C9", 23, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 5000UL),
								new ReplacementTable("MINI", 27, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MAJOR", 29, 400000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 32000UL, 0UL, 0UL)
							},
							new List<ReplacementTable>()
							{
								new ReplacementTable("C1", 15, 98000UL, 530000UL, 435000UL),
								new ReplacementTable("C2", 16, 170000UL, 170000UL, 140000UL),
								new ReplacementTable("C3", 17, 105000UL, 105000UL, 75000UL),
								new ReplacementTable("C4", 18, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C5", 19, 30000UL, 30000UL, 40000UL),
								new ReplacementTable("C6", 20, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C7", 21, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C8", 22, 10000UL, 10000UL, 30000UL),
								new ReplacementTable("C9", 23, 30000UL, 30000UL, 30000UL),
								new ReplacementTable("C10", 24, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C11", 25, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("C12", 26, 5000UL, 5000UL, 30000UL),
								new ReplacementTable("MINI", 27, 50000UL, 50000UL, 50000UL),
								new ReplacementTable("MINOR", 28, 10000UL, 10000UL, 10000UL),
								new ReplacementTable("MAJOR", 29, 400000UL, 0UL, 0UL),
								new ReplacementTable("GRAND", 30, 32000UL, 0UL, 0UL)
							}
						)
					)
				}
			);
		}

		private static SelectorItems CreateExcelLineEvaluator()
		{
			return new SelectorItems(
				new List<SelectorItem>()
				{
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(1UL), new Money(2UL), new Money(5UL), new Money(10UL)) },
						new MaskPrize[]
						{
							new MaskPrize("PIC1", PrizeStrategy.Left, 3, new List<int>() { 20, 40, 125 }, new List<int>() { 0, 12 }, null),
							new MaskPrize("PIC2", PrizeStrategy.Left, 3, new List<int>() { 20, 35, 100 }, new List<int>() { 1, 12 }, null),
							new MaskPrize("PIC3", PrizeStrategy.Left, 3, new List<int>() { 15, 30, 75 }, new List<int>() { 2, 12 }, null),
							new MaskPrize("PIC4", PrizeStrategy.Left, 3, new List<int>() { 10, 30, 75 }, new List<int>() { 3, 12 }, null),
							new MaskPrize("PIC5", PrizeStrategy.Left, 3, new List<int>() { 5, 20, 60 }, new List<int>() { 4, 12 }, null),
							new MaskPrize("K", PrizeStrategy.Left, 3, new List<int>() { 5, 20, 40 }, new List<int>() { 5, 12 }, null),
							new MaskPrize("Q", PrizeStrategy.Left, 3, new List<int>() { 5, 15, 25 }, new List<int>() { 6, 12 }, null),
							new MaskPrize("J", PrizeStrategy.Left, 3, new List<int>() { 5, 15, 25 }, new List<int>() { 7, 12 }, null),
							new MaskPrize("10", PrizeStrategy.Left, 3, new List<int>() { 2, 10, 20 }, new List<int>() { 8, 12 }, null),
							new MaskPrize("9", PrizeStrategy.Left, 3, new List<int>() { 2, 10, 20 }, new List<int>() { 9, 12 }, null)
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL), new Money(200UL), new Money(50UL)) },
						new MaskPrize[]
						{
							new MaskPrize("PIC1", PrizeStrategy.Left, 2, new List<int>() { 2, 15, 30, 100 }, new List<int>() { 0, 12 }, null),
							new MaskPrize("PIC2", PrizeStrategy.Left, 2, new List<int>() { 1, 10, 25, 80 }, new List<int>() { 1, 12 }, null),
							new MaskPrize("PIC3", PrizeStrategy.Left, 3, new List<int>() { 10, 25, 70 }, new List<int>() { 2, 12 }, null),
							new MaskPrize("PIC4", PrizeStrategy.Left, 3, new List<int>() { 8, 20, 60 }, new List<int>() { 3, 12 }, null),
							new MaskPrize("PIC5", PrizeStrategy.Left, 3, new List<int>() { 8, 20, 50 }, new List<int>() { 4, 12 }, null),
							new MaskPrize("K", PrizeStrategy.Left, 3, new List<int>() { 5, 15, 35 }, new List<int>() { 5, 12 }, null),
							new MaskPrize("Q", PrizeStrategy.Left, 3, new List<int>() { 5, 15, 30 }, new List<int>() { 6, 12 }, null),
							new MaskPrize("J", PrizeStrategy.Left, 3, new List<int>() { 5, 10, 25 }, new List<int>() { 7, 12 }, null),
							new MaskPrize("10", PrizeStrategy.Left, 3, new List<int>() { 2, 5, 15 }, new List<int>() { 8, 12 }, null),
							new MaskPrize("9", PrizeStrategy.Left, 3, new List<int>() { 2, 5, 15 }, new List<int>() { 9, 12 }, null)
						}
					)
				}
			);
		}

		private static Patterns CreateExcelScatterPatternProvider()
		{
			return new Patterns(
				new List<Pattern>() { new Pattern("Scatter", Cluster.CreateList(", C0R0 C0R1 C0R2, C1R0 C1R1 C1R2, C2R0 C2R1 C2R2, C3R0 C3R1 C3R2, C4R0 C4R1 C4R2")) },
				new SymbolWindowStructure(
					new List<CellPopulation>()
					{
						new CellPopulation("Reel1", 0, Cell.CreateList(" C0R0 C0R1 C0R2")),
						new CellPopulation("Reel2", 0, Cell.CreateList(" C1R0 C1R1 C1R2")),
						new CellPopulation("Reel3", 0, Cell.CreateList(" C2R0 C2R1 C2R2")),
						new CellPopulation("Reel4", 0, Cell.CreateList(" C3R0 C3R1 C3R2")),
						new CellPopulation("Reel5", 0, Cell.CreateList(" C4R0 C4R1 C4R2"))
					}
				)
			);
		}

		private static MaskPrize[] CreateExcelScatterEvaluator()
		{
			return new MaskPrize[] { new MaskPrize("SCAT1", PrizeStrategy.Any, 3, new List<int>() { 1, 2, 5 }, new List<int>() { 10 }, null) };
		}

		private static Patterns CreateExcelRespinPatternProvider()
		{
			return new Patterns(
				new List<Pattern>()
				{
					new Pattern("Scatter 1", Cluster.CreateList(", C0R0")),
					new Pattern("Scatter 2", Cluster.CreateList(", C0R1")),
					new Pattern("Scatter 3", Cluster.CreateList(", C0R2")),
					new Pattern("Scatter 4", Cluster.CreateList(", C1R0")),
					new Pattern("Scatter 5", Cluster.CreateList(", C1R1")),
					new Pattern("Scatter 6", Cluster.CreateList(", C1R2")),
					new Pattern("Scatter 7", Cluster.CreateList(", C2R0")),
					new Pattern("Scatter 8", Cluster.CreateList(", C2R1")),
					new Pattern("Scatter 9", Cluster.CreateList(", C2R2")),
					new Pattern("Scatter 10", Cluster.CreateList(", C3R0")),
					new Pattern("Scatter 11", Cluster.CreateList(", C3R1")),
					new Pattern("Scatter 12", Cluster.CreateList(", C3R2")),
					new Pattern("Scatter 13", Cluster.CreateList(", C4R0")),
					new Pattern("Scatter 14", Cluster.CreateList(", C4R1")),
					new Pattern("Scatter 15", Cluster.CreateList(", C4R2"))
				},
				new SymbolWindowStructure(
					new List<CellPopulation>()
					{
						new CellPopulation("Reel1", 0, Cell.CreateList(" C0R0 C0R1 C0R2")),
						new CellPopulation("Reel2", 0, Cell.CreateList(" C1R0 C1R1 C1R2")),
						new CellPopulation("Reel3", 0, Cell.CreateList(" C2R0 C2R1 C2R2")),
						new CellPopulation("Reel4", 0, Cell.CreateList(" C3R0 C3R1 C3R2")),
						new CellPopulation("Reel5", 0, Cell.CreateList(" C4R0 C4R1 C4R2"))
					}
				)
			);
		}

		private static SelectorItems CreateExcelRespinPrizes()
		{
			return new SelectorItems(
				new List<SelectorItem>()
				{
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(1UL), new Money(2UL), new Money(5UL), new Money(10UL)) },
						new List<RespinPrize>()
						{
							new RespinPrize("C1", 15, false, false, "CREDIT_1", 1UL),
							new RespinPrize("C2", 16, false, false, "CREDIT_2", 2UL),
							new RespinPrize("C3", 17, false, false, "CREDIT_3", 3UL),
							new RespinPrize("C4", 18, false, false, "CREDIT_4", 4UL),
							new RespinPrize("C5", 19, false, false, "CREDIT_5", 5UL),
							new RespinPrize("C6", 20, false, false, "CREDIT_6", 6UL),
							new RespinPrize("C7", 21, false, false, "CREDIT_7", 7UL),
							new RespinPrize("C8", 22, false, false, "CREDIT_8", 8UL),
							new RespinPrize("C9", 23, false, false, "CREDIT_10", 10UL),
							new RespinPrize("C10", 24, false, false, "CREDIT_12", 12UL),
							new RespinPrize("C11", 25, false, false, "CREDIT_15", 15UL),
							new RespinPrize("C12", 26, false, false, "CREDIT_30", 30UL),
							new RespinPrize("MINI", 27, true, false, "MINI", 1000UL),
							new RespinPrize("MINOR", 28, true, false, "MINOR", 5000UL),
							new RespinPrize("MAJOR", 29, false, true, "MAJOR", 0UL),
							new RespinPrize("GRAND", 30, false, true, "GRAND", 0UL)
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(50UL)) },
						new List<RespinPrize>()
						{
							new RespinPrize("C1", 15, false, false, "CREDIT_1", 1UL),
							new RespinPrize("C2", 16, false, false, "CREDIT_2", 2UL),
							new RespinPrize("C3", 17, false, false, "CREDIT_3", 3UL),
							new RespinPrize("C4", 18, false, false, "CREDIT_4", 4UL),
							new RespinPrize("C5", 19, false, false, "CREDIT_5", 5UL),
							new RespinPrize("C6", 20, false, false, "CREDIT_6", 6UL),
							new RespinPrize("C7", 21, false, false, "CREDIT_7", 7UL),
							new RespinPrize("C8", 22, false, false, "CREDIT_8", 8UL),
							new RespinPrize("C9", 23, false, false, "CREDIT_10", 10UL),
							new RespinPrize("C10", 24, false, false, "CREDIT_12", 12UL),
							new RespinPrize("C11", 25, false, false, "CREDIT_15", 15UL),
							new RespinPrize("C12", 26, false, false, "CREDIT_30", 30UL),
							new RespinPrize("MINI", 27, true, false, "MINI", 400UL),
							new RespinPrize("MINOR", 28, true, false, "MINOR", 2000UL),
							new RespinPrize("MAJOR", 29, false, true, "MAJOR", 0UL),
							new RespinPrize("GRAND", 30, false, true, "GRAND", 0UL)
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Denom", new Money(100UL), new Money(200UL)) },
						new List<RespinPrize>()
						{
							new RespinPrize("C1", 15, false, false, "CREDIT_1", 1UL),
							new RespinPrize("C2", 16, false, false, "CREDIT_2", 2UL),
							new RespinPrize("C3", 17, false, false, "CREDIT_3", 3UL),
							new RespinPrize("C4", 18, false, false, "CREDIT_4", 4UL),
							new RespinPrize("C5", 19, false, false, "CREDIT_5", 5UL),
							new RespinPrize("C6", 20, false, false, "CREDIT_6", 6UL),
							new RespinPrize("C7", 21, false, false, "CREDIT_7", 7UL),
							new RespinPrize("C8", 22, false, false, "CREDIT_8", 8UL),
							new RespinPrize("C9", 23, false, false, "CREDIT_10", 10UL),
							new RespinPrize("C10", 24, false, false, "CREDIT_12", 12UL),
							new RespinPrize("C11", 25, false, false, "CREDIT_15", 15UL),
							new RespinPrize("C12", 26, false, false, "CREDIT_30", 30UL),
							new RespinPrize("MINI", 27, true, false, "MINI", 250UL),
							new RespinPrize("MINOR", 28, true, false, "MINOR", 1250UL),
							new RespinPrize("MAJOR", 29, false, true, "MAJOR", 0UL),
							new RespinPrize("GRAND", 30, false, true, "GRAND", 0UL)
						}
					)
				}
			);
		}

		private static PrizeCountWithCycleSet[] CreateExcelTriggerCycleSetsFromPrizes()
		{
			return new PrizeCountWithCycleSet[] { new PrizeCountWithCycleSet("SCAT1", 5, "FreeGames", 8), new PrizeCountWithCycleSet("SCAT1", 4, "FreeGames", 8), new PrizeCountWithCycleSet("SCAT1", 3, "FreeGames", 8) };
		}

		private static SelectorItems CreateExcelFreeStripProvider()
		{
			return new SelectorItems(
				new List<SelectorItem>()
				{
					new SelectorItem(
						new Requirement[] { Requirement.Create("Percentage", "NSW02", "NSW01", "NSW03", "QLD01", "QLD02", "QLD03", "CAS01", "CAS02", "CAS03"), Requirement.Create("Denom", new Money(1UL), new Money(2UL), new Money(5UL), new Money(10UL)) },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 8 0 4 7 5 3 8 2 1 6 3 3 11 11 11 5 3 10 9 6 1 0 8 2 3 7 6 1 4 4 4 6 1 9 4 0 7 8 0 0 10 7 4 1 6 3 9 8 2 0 5 3 7 2 0 9 1 1 8 2 9 1 6 2 2 2 3 9 0 7 1 5 8 0 7 3 3 3 8 1 5 6 4 4 0 6 1 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 5 8 3 9 4 10 5 7 2 1 12 9 1 6 11 11 11 8 3 4 7 2 1 12 9 2 5 7 10 1 6 7 12 6 2 4 9 3 0 8 6 3 4 7 3 8 12 12 12 0 7 9 2 4 10 5 1 6 0 7 6 4 3 6 12 1 9 2 5 0 12 1 8 2 3 1 12 5 2 4 9 5 0 6 1 8 4 3 6 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 1 3 8 10 7 2 9 4 12 8 1 6 10 3 5 8 11 11 0 6 12 12 4 3 8 5 3 6 2 7 12 12 4 1 9 6 4 3 5 12 12 6 7 10 4 0 8 9 12 12 5 1 7 3 1 12 9 6 4 2 6 7 12 6 2 0 5 1 3 9 4 0 12 12 3 0 6 1 8 0 2 4 6 1 3 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 12 0 2 10 1 8 12 12 4 9 0 6 8 3 12 12 9 3 8 5 10 9 2 1 5 12 12 12 3 6 0 1 7 12 5 1 3 12 6 2 7 9 4 3 10 1 0 7 12 12 12 9 3 8 1 11 11 11 9 3 8 4 5 12 1 4 6 2 0 12 8 1 0 7 5 12 4 3 8 12 12 12 4 7 0 2 6"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 1 8 7 12 3 6 4 12 3 2 9 12 8 5 0 7 10 0 4 7 12 3 4 7 12 2 8 3 5 12 6 3 0 12 6 3 5 12 6 9 4 12 7 1 4 9 12 8 4 3 9 6 12 4 3 0 5 7 12 2 6 8 11 11 11 3 8 10 1 5 3 8 12 7 2 0 8 10 3 0 5 4 12 9 6")
						}
					),
					new SelectorItem(
						new Requirement[] { Requirement.Create("Percentage", "NSW02", "NSW01", "NSW03", "QLD01", "QLD02", "QLD03", "CAS01", "CAS02", "CAS03"), Requirement.Create("Denom", new Money(50UL), new Money(100UL), new Money(200UL)) },
						new ISymbolListStrip[]
						{
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 9 1 8 0 4 7 5 3 8 2 1 6 3 3 11 11 11 5 3 10 9 6 1 0 8 2 3 7 6 1 4 4 4 6 1 9 4 0 7 8 0 0 10 7 4 1 6 3 9 8 2 0 5 3 7 2 0 9 1 1 8 2 9 1 6 2 2 2 3 9 0 7 1 5 8 0 7 3 3 3 8 1 5 6 4 4 0 6 1 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 5 8 3 9 4 10 5 7 2 1 12 9 1 6 11 11 11 8 3 4 7 12 1 6 12 2 5 7 10 1 6 12 4 7 2 12 9 3 1 12 6 3 4 7 3 8 12 12 12 6 7 9 2 4 10 5 1 12 0 7 6 4 3 5 12 1 9 2 5 0 6 12 8 2 3 1 12 5 2 4 9 12 0 6 1 8 4 3 6 1"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 0 5 1 3 8 10 7 2 9 12 12 8 1 6 10 3 5 8 11 11 0 6 12 12 4 3 8 5 3 6 2 7 12 5 4 1 9 6 4 3 5 12 12 6 7 10 4 0 8 9 3 12 5 1 7 3 1 12 9 6 4 2 5 7 12 6 2 0 5 1 3 9 4 0 8 5 12 0 6 1 8 0 2 4 6 1 3 5"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 2 6 12 0 2 10 1 8 12 7 4 9 0 6 8 3 12 12 9 3 8 5 10 9 2 1 5 12 12 12 3 6 0 1 7 12 5 1 3 12 6 2 12 9 4 3 10 1 0 7 12 12 12 1 3 8 1 11 11 11 5 3 8 4 5 9 12 4 6 2 0 5 12 1 0 7 8 12 4 3 8 12 12 12 4 7 0 2 6"),
							SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 8 5 0 12 8 7 12 5 6 4 12 3 2 9 12 8 5 0 7 10 0 4 7 12 3 4 7 12 2 8 3 5 12 9 3 0 12 6 3 5 12 1 9 4 12 7 1 4 9 12 8 4 3 9 6 12 4 3 0 8 7 12 2 6 8 11 11 11 3 8 10 1 5 3 8 12 7 2 0 8 10 3 0 5 4 12 9 6")
						}
					)
				}
			);
		}

		private static SymbolMultiplierDetails CreateExcelPrizeSymbolMultiplier()
		{
			return new SymbolMultiplierDetails(new List<SymbolMultiplier>() { new SymbolMultiplier("WILD", 2) }, MultiplierUsage.Multiply, new List<string>());
		}

		private static ISymbolListStrip[] CreateExcelRespinNoFrameStrips()
		{
			return new ISymbolListStrip[]
			{
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 8 9 5 6 9 8 3 2 5 6 8 7 3 8 0 1 8 5 0 6 3 5 6 8 1 4 7 9 3 1 7 8 5 9 7 6 5 9 2 4 7 3 6 0 9 8 4 1 7 6 4 2 8 9 7 0 5 7 9 6 2 7 8 6 3 8 9 0 8 7 5 2 8 6 7 8 9 2 8 7 3 5 1 4 7 5 9 0 5 3 6 4 2 5 4 7 1 6 9 5 0 6 9 2 0 9 5 4 1 9 7 6 5 9 1 7 5 6 8"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 8 6 3 9 5 8 2 6 8 2 7 8 9 6 2 0 9 1 8 5 9 6 5 7 8 9 6 4 0 7 5 4 8 7 4 5 2 6 9 2 1 7 6 8 9 5 3 1 2 4 0 7 5 4 8 5 9 6 5 7 0 6 7 4 8 3 7 1 4 0 7 4 5 8 0 9 7 1 6 3 9 7 5 0 9 8 5 6 8 5 9 8 7 5 9 7 3 1 5 8 7 9 2 7 6 5 3 9 6 1 0 6 2 9 6 3 8 6 1 3"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 7 1 9 7 8 3 9 8 3 7 8 9 5 6 0 3 6 5 4 0 8 4 7 9 6 7 2 9 4 2 6 9 7 1 9 6 5 8 4 6 5 9 0 6 8 4 7 8 0 2 5 9 6 7 3 1 8 7 5 0 3 6 7 9 4 7 9 5 2 4 6 3 9 8 5 1 9 8 2 5 1 9 0 8 2 7 6 4 5 7 6 2 5 9 3 8 6 1 9 7 5 3 6 8 5 7 6 8 2 0 5 7 1 8 5 1 0 5 8"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 6 3 7 9 3 6 8 7 5 3 6 1 8 4 6 5 8 0 5 7 3 8 0 7 6 5 3 9 5 3 8 4 9 6 0 1 5 0 9 8 7 6 8 7 5 1 2 5 6 1 3 2 6 1 0 9 7 6 5 9 0 5 2 8 5 6 8 1 5 4 9 2 5 7 0 8 1 9 2 7 8 4 9 7 5 9 8 2 6 7 8 4 9 1 2 9 7 6 9 7 2 6 8 9 7 3 8 9 4 6 7 0 5 4 8 9 5 6 4"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 5 2 8 7 1 4 5 7 8 2 7 5 3 9 4 1 5 2 3 8 5 9 1 7 9 8 6 4 5 9 6 5 9 6 2 7 0 1 3 5 8 6 5 0 6 1 8 0 4 8 5 7 4 5 7 6 9 3 5 2 0 9 6 1 0 9 7 5 0 7 3 4 9 1 2 5 9 2 4 6 7 8 6 9 1 7 8 0 3 6 9 8 3 6 8 9 6 2 8 7 9 8 5 0 8 7 6 9 4 5 7 8 3 9 6 7 8 6 7"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 1 0 6 4 1 9 8 7 0 13 2 7 1 8 6 3 0 7 5 13 9 2 8 5 9 8 4 0 2 8 3 9 4 6 5 8 14 7 9 13 5 4 8 6 9 8 2 3 7 1 0 3 7 5 8 1 4 2 5 13 8 7 0 9 3 8 9 5 6 0 8 5 6 7 5 9 14 6 5 13 0 9 7 6 8 7 9 5 4 8 7 4 3 9 8 7 1 6 7 13 6 7 1 5 6 7 9 5 8 3 6 2 5 9 7 5 6 9 5 13"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 2 7 8 5 7 9 5 6 7 13 3 8 4 0 1 9 7 2 4 13 2 6 8 7 6 9 8 6 7 8 0 2 1 5 3 7 14 3 5 13 2 5 6 3 8 6 7 9 4 5 7 3 6 8 9 0 6 9 5 13 6 2 7 9 6 7 9 0 5 9 2 1 8 4 6 1 14 3 8 13 4 7 0 8 5 0 8 2 5 9 8 0 7 5 6 8 7 5 6 13 1 0 5 8 6 1 9 5 8 3 9 4 6 9 3 5 1 9 7 13"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 4 7 8 9 6 2 4 5 3 8 1 0 8 1 9 8 2 9 4 13 8 6 0 9 5 8 6 7 9 1 5 6 7 9 5 2 14 3 7 13 6 3 5 8 7 5 6 2 4 7 9 2 5 8 3 2 0 1 8 13 4 0 5 4 9 6 1 3 9 6 7 9 6 5 8 1 14 8 3 13 4 0 8 6 7 8 0 2 5 6 1 9 8 5 7 3 6 5 7 13 0 7 5 1 7 4 6 5 2 7 0 5 9 6 7 8 5 7 8 13"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 5 8 9 7 5 6 7 0 4 2 1 5 9 6 1 7 5 8 13 7 8 9 5 7 8 6 3 0 8 5 0 8 4 9 6 14 7 8 13 1 0 6 2 4 3 7 1 2 5 3 7 6 0 4 7 2 8 7 13 5 8 1 9 4 8 9 7 5 6 7 1 4 8 6 2 14 4 2 13 7 2 0 3 5 9 6 5 8 9 3 7 9 1 2 6 0 9 6 13 9 5 3 7 9 8 5 9 6 8 9 6 5 9 7 5 6 8 5 13"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 0 5 6 7 8 4 6 9 0 6 1 5 3 7 6 9 2 5 6 13 5 9 4 3 5 9 8 2 7 4 3 0 6 9 7 3 14 5 3 13 8 7 6 9 8 1 5 6 8 0 5 9 7 6 0 1 8 5 1 13 3 5 1 8 5 1 2 0 6 8 9 5 7 8 9 7 14 2 5 13 3 9 8 4 0 5 7 6 9 4 6 7 4 8 2 6 7 8 9 13 5 8 9 0 7 3 8 5 9 8 2 9 1 2 6 8 7 1 4 13"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 4 9 7 4 5 6 9 7 8 3 9 2 7 6 2 7 9 6 5 13 6 8 3 0 5 6 0 4 1 6 9 8 3 7 0 6 14 5 7 13 1 6 9 7 5 9 7 5 3 8 5 1 0 5 6 1 4 8 5 13 7 0 4 5 1 0 2 4 6 13 8 7 3 9 1 0 14 8 6 13 9 8 6 2 7 9 8 6 7 2 9 6 1 9 7 8 2 7 8 13 2 3 0 5 8 7 3 8 1 13 8 6 5 8 6 4 7 5 8 13"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 5 1 6 0 5 6 1 9 0 6 2 7 4 2 7 5 9 7 1 13 5 9 4 8 9 6 2 3 8 5 7 0 3 4 8 6 14 1 9 13 8 6 3 5 9 7 6 9 7 13 2 7 4 6 5 8 2 9 3 13 5 6 9 7 6 4 5 3 4 13 8 6 9 8 7 0 14 3 7 13 5 7 8 3 9 1 4 7 3 13 9 2 6 8 4 0 5 6 8 13 6 5 7 2 8 5 7 1 6 13 7 0 9 1 7 9 6 0 5 13"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 3 1 0 5 4 9 5 3 6 2 7 0 6 7 9 1 8 5 13 7 8 9 7 4 5 6 9 8 13 9 8 3 4 8 6 14 1 9 13 8 9 4 1 6 2 5 8 4 13 3 7 0 6 8 7 6 3 7 13 8 9 7 6 9 5 2 1 7 13 1 6 9 2 7 6 14 0 2 13 6 7 8 2 7 5 0 6 1 13 5 8 6 5 8 7 9 3 0 13 9 5 8 7 1 9 6 4 7 13 8 9 0 2 8 5 9 8 5 13"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 1 5 3 4 2 5 4 3 13 5 7 6 8 2 7 6 0 7 13 2 5 3 8 5 6 0 9 1 13 6 8 9 1 0 9 14 7 6 13 1 5 9 1 8 7 5 9 8 13 7 9 6 4 7 9 8 6 5 13 4 6 5 7 2 6 8 3 7 13 1 5 9 7 0 5 14 4 3 13 2 7 3 9 6 7 5 6 8 13 9 3 6 9 8 4 9 8 5 13 9 7 5 3 8 0 7 9 1 13 6 8 4 9 8 1 6 8 9 13"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 9 5 4 13 8 7 0 5 13 8 1 3 7 13 5 6 1 3 13 5 8 2 6 13 4 6 5 0 13 9 2 6 5 13 6 14 5 8 13 3 2 8 6 13 7 3 9 8 13 3 9 0 5 13 6 1 5 9 13 2 5 6 8 13 3 7 2 9 13 5 7 6 1 13 8 14 5 2 13 7 9 8 1 13 7 5 6 9 13 4 7 8 6 1 13 7 0 4 13 7 4 8 7 2 13 0 7 3 13 5 8 9 4 5 13 9 2 7 13")
			};
		}

		private static ISymbolListStrip[] CreateExcelRespinFrameStrips()
		{
			return new ISymbolListStrip[]
			{
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 5 6 9 8 5 6 1 0 8 9 2 7 8 0 2 7 0 6 5 13 9 5 3 8 5 3 7 6 5 13 1 7 5 6 7 4 9 13 5 9 2 1 3 5 7 6 13 2 3 5 6 0 9 6 3 13 1 8 0 1 4 8 3 6 13 7 2 9 6 2 7 9 8 7 4 8 9 4 1 8"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 7 9 8 6 7 9 3 4 9 6 8 4 7 9 5 3 7 6 3 13 8 9 2 7 1 9 0 2 5 13 6 5 7 0 4 1 6 13 3 5 9 1 6 4 9 1 13 8 5 2 3 0 8 2 0 13 7 6 5 7 8 0 2 6 13 7 9 6 5 3 4 0 8 5 4 8 1 5 1 8"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 1 7 0 5 7 9 1 6 9 7 5 6 8 5 3 7 6 9 5 13 8 9 6 1 0 9 8 5 3 13 4 8 3 4 7 9 8 13 2 5 8 7 3 0 4 2 13 9 6 8 7 6 1 8 6 13 4 2 1 8 4 9 5 1 13 4 6 2 5 0 6 9 5 0 2 8 6 7 5 2"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 8 3 1 8 6 3 2 4 6 3 5 8 4 2 6 1 4 9 7 13 3 8 9 6 8 1 7 0 1 13 6 5 7 9 0 5 8 13 0 8 2 4 5 7 3 8 13 9 6 0 5 2 9 7 5 13 4 0 5 1 9 6 7 5 13 2 6 7 0 5 4 8 5 9 2 6 3 9 7 9"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 1 8 7 2 5 0 1 7 9 3 8 9 5 3 4 5 9 8 3 13 8 6 4 0 6 7 4 6 2 13 1 0 8 7 9 8 1 13 2 4 9 2 8 5 6 1 8 5 7 6 2 5 6 7 2 13 8 5 0 4 9 7 5 6 13 1 6 7 0 8 3 0 5 3 7 6 4 9 9 9"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 5 2 9 4 2 9 8 4 5 1 6 7 0 6 2 5 0 1 2 13 4 3 8 0 4 7 9 2 7 13 3 7 9 3 8 9 2 13 5 6 7 4 9 6 1 5 9 6 7 9 6 1 8 9 5 13 0 5 3 0 6 8 9 7 13 1 3 6 8 7 4 3 6 0 8 5 7 8 5 8"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 2 9 0 5 2 1 4 5 8 9 0 7 1 8 0 1 5 4 9 13 4 7 9 3 1 7 5 6 8 13 3 9 2 6 9 7 4 13 6 1 8 6 7 8 9 7 2 0 6 9 3 8 5 7 8 13 6 9 3 7 2 8 6 5 13 4 3 6 7 3 5 6 0 8 2 6 5 1 5 9"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 2 7 6 4 0 3 7 5 8 4 7 6 0 9 7 8 6 9 8 13 7 9 2 1 5 6 4 8 2 13 9 5 0 9 5 1 0 13 8 2 4 9 6 5 4 8 3 5 9 6 2 5 1 8 5 13 1 8 0 6 9 7 6 8 13 7 1 9 4 1 6 0 7 5 8 6 3 7 5 3"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 5 6 2 7 6 9 7 6 0 8 9 3 7 0 4 7 8 1 9 13 2 4 8 2 3 5 6 9 7 13 8 6 1 3 5 2 9 13 8 9 5 8 3 6 9 5 8 1 0 4 7 8 4 9 5 13 0 8 4 7 6 0 7 9 13 6 2 7 0 5 3 2 1 6 9 1 4 3 5 7"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 6 2 9 0 4 9 5 6 8 5 9 7 6 8 5 0 8 7 6 13 7 5 2 9 8 3 1 8 6 13 7 1 5 6 7 3 9 13 3 9 6 4 5 9 3 7 0 2 7 9 2 1 9 8 4 13 0 5 8 4 7 6 9 0 13 5 7 2 3 5 2 8 4 6 8 1 7 0 8 6"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 5 8 1 4 7 1 9 6 8 3 0 5 9 6 3 5 1 7 8 13 2 9 0 6 9 0 7 6 8 13 4 2 3 5 6 0 3 13 9 7 8 4 6 8 1 9 6 7 8 6 5 0 7 4 5 13 4 5 6 2 7 8 5 4 13 8 5 7 9 0 2 9 3 1 8 2 7 5 9 2"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 4 5 7 3 5 6 2 0 1 7 5 8 9 1 0 8 2 5 4 13 0 7 3 6 9 8 4 3 5 13 1 8 6 9 4 2 7 13 6 7 9 8 5 3 6 1 8 7 6 4 9 6 8 4 2 13 7 8 0 9 1 6 7 9 13 8 1 0 5 3 9 7 8 5 6 7 5 2 9 9"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 3 4 8 5 2 9 6 8 7 2 6 5 0 8 4 6 0 5 7 13 5 8 1 3 7 8 1 3 0 8 9 3 4 9 6 2 4 13 1 6 9 7 2 6 7 5 2 4 6 9 0 1 9 5 1 2 8 5 9 0 8 5 6 7 13 9 7 1 3 7 6 5 8 9 5 8 6 7 9 7"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 2 0 7 1 9 2 6 1 5 9 8 5 9 6 7 8 9 2 0 13 8 7 9 2 7 6 2 5 8 1 5 3 9 4 5 0 4 13 9 6 7 5 3 6 5 4 9 8 6 5 0 7 8 2 5 7 8 6 7 4 8 0 6 8 13 4 3 7 1 6 4 5 0 7 3 8 6 1 3 3"),
				SymbolListStrip.Create(" PIC1 PIC2 PIC3 PIC4 PIC5 K Q J 10 9 SCAT1 SCAT2 WILD HIT FRAME C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 MINI MINOR MAJOR GRAND", " 1 0 2 6 4 5 8 4 0 7 1 4 8 2 6 3 5 8 1 13 5 8 3 5 7 2 5 8 9 3 5 7 3 8 6 5 1 13 6 9 8 6 3 0 4 7 1 9 6 3 8 9 4 8 9 7 5 6 1 7 0 6 7 9 5 7 6 4 2 7 9 2 6 9 0 8 7 5 0 9")
			};
		}

		private static T ThrowIfReturnsNull<T>(T t, string methodName)
		{
			if (t == null)
				throw new Exception($"Method {methodName} should not return null unless it declares the [return: NullOut] attribute");
			return t;
		}

		private static T ThrowIfPropertyNull<T>(T t, string propertyName)
		{
			if (t == null)
				throw new Exception($"Property {propertyName} should not return null unless it declares the [NullOut] attribute");
			return t;
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class StageNamesAttribute : Attribute
	{
		public IReadOnlyList<string> StageNames { get; }
		public StageNamesAttribute(string[] stageNames) => StageNames = stageNames;
	}

	[AttributeUsage(AttributeTargets.Class)]
	public sealed class EntryStageAttribute : Attribute
	{
		public string EntryStageName { get; }
		public EntryStageAttribute(string entryStageName) => EntryStageName = entryStageName;
	}

	public sealed class ResultTypeAttribute : Attribute
	{
		public StageResultType ResultType { get; }
		public ResultTypeAttribute(StageResultType resultType) => ResultType = resultType;
	}

	[StageNames(new[] { "Base" })]
	public sealed class BaseStageResults : StageResults
	{
		[ResultType(StageResultType.VariableOneGame)]
		public string SetChoice => (string)this[0].Value;

		[ResultType(StageResultType.Presentation)]
		public SymbolWindowResult SymbolWindow => (SymbolWindowResult)this[1].Value;

		[ResultType(StageResultType.AwardCreditsList)]
		public IReadOnlyList<CellPrizeResult> LinePrizes => (IReadOnlyList<CellPrizeResult>)this[2].Value;

		[ResultType(StageResultType.AwardCreditsList)]
		public IReadOnlyList<CellPrizeResult> ScatterPrizes => (IReadOnlyList<CellPrizeResult>)this[3].Value;

		[ResultType(StageResultType.VariableOneGame)]
		public RespinState RespinState => (RespinState)this[6].Value;

		[ResultType(StageResultType.AwardCreditsList)]
		public IReadOnlyList<CellPrizeResult> Prizes => (IReadOnlyList<CellPrizeResult>)this[7].Value;

		[ResultType(StageResultType.AwardCreditsList)]
		public IReadOnlyList<CellPrizeResult> BonusPrizes => (IReadOnlyList<CellPrizeResult>)this[8].Value;

		public BaseStageResults(params StageResult[] results) : base(results) { }
	}

	[StageNames(new[] { "FreeGames" })]
	public sealed class FreeGamesStageResults : StageResults
	{
		[ResultType(StageResultType.VariableOneGame)]
		public string SetChoice => (string)this[0].Value;

		[ResultType(StageResultType.Presentation)]
		public SymbolWindowResult SymbolWindow => (SymbolWindowResult)this[1].Value;

		[ResultType(StageResultType.AwardCreditsList)]
		public IReadOnlyList<CellPrizeResult> ScatterPrizes => (IReadOnlyList<CellPrizeResult>)this[2].Value;

		[ResultType(StageResultType.AwardCreditsList)]
		public IReadOnlyList<CellPrizeResult> Prizes => (IReadOnlyList<CellPrizeResult>)this[4].Value;

		[ResultType(StageResultType.AwardCreditsList)]
		public IReadOnlyList<CellPrizeResult> BonusPrizes => (IReadOnlyList<CellPrizeResult>)this[5].Value;

		[ResultType(StageResultType.VariableOneGame)]
		public RespinState RespinState => (RespinState)this[6].Value;

		[ResultType(StageResultType.AwardCreditsList)]
		public IReadOnlyList<CellPrizeResult> LinePrizes => (IReadOnlyList<CellPrizeResult>)this[8].Value;

		public FreeGamesStageResults(params StageResult[] results) : base(results) { }
	}

	[StageNames(new[] { "Respin" })]
	public sealed class RespinStageResults : StageResults
	{
		[ResultType(StageResultType.Presentation)]
		public LockedSymbolWindowResult LockedSymbolWindow => (LockedSymbolWindowResult)this[0].Value;

		[ResultType(StageResultType.AwardCreditsList)]
		public IReadOnlyList<CellPrizeResult> Prizes => (IReadOnlyList<CellPrizeResult>)this[2].Value;

		[ResultType(StageResultType.AwardCreditsList)]
		public IReadOnlyList<CellPrizeResult> BonusPrizes => (IReadOnlyList<CellPrizeResult>)this[3].Value;

		[ResultType(StageResultType.VariableOneGame)]
		public RespinState RespinState => (RespinState)this[4].Value;

		[ResultType(StageResultType.ProgressiveList)]
		public IReadOnlyList<ProgressivePrizeResult> Progressives => (IReadOnlyList<ProgressivePrizeResult>)this[5].Value;

		public RespinStageResults(params StageResult[] results) : base(results) { }
	}
}
