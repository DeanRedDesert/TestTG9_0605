using Gaff.Conditions;
using Gaff.Core;
using Gaff.Core.EditorTypes;
using Gaff.DecisionMakers;

namespace Gaff
{
	public static partial class Editors
	{
		private static GaffInterface CreateAwardedPrizeRangeConditionInterface(AwardedPrizeRangeCondition awardedPrizeRangeCondition)
		{
			return GeHelper.CreateInterface()
				.AddRow(GeHelper.CreateRow(0, 4).AddText("Prize Range (Min-Max)", 2, 10).AddNumberEdit(awardedPrizeRangeCondition.MinPrize, 0, 5).AddText("-", 0, 5).AddNumberEdit(awardedPrizeRangeCondition.MaxPrize));
		}

		private static object GetAwardedPrizeRangeCondition(GaffInterface gi)
		{
			var minPrize = gi.GetItemAt<GeNumber>(0, 1).Value;
			var maxPrize = gi.GetItemAt<GeNumber>(0, 3).Value;
			return new AwardedPrizeRangeCondition(minPrize, maxPrize);
		}

		private static GaffInterface CreateGameCountConditionInterface(GameCountCondition gameCountCondition)
		{
			return GeHelper.CreateInterface()
				.AddRow(GeHelper.CreateRow(0, 4).AddText("Game Count", 2, 10).AddNumberEdit(gameCountCondition.GameCount));
		}

		private static object GetGameCountCondition(GaffInterface gi)
		{
			var gameCount = gi.GetItemAt<GeNumber>(0, 1).Value;
			return new GameCountCondition(gameCount);
		}

		private static GaffInterface CreateWaitForStageAndCycleIdConditionInterface(WaitForStageAndCycleIdCondition waitForStageAndCycleIdCondition)
		{
			return GeHelper.CreateInterface()
				.AddRow(GeHelper.CreateRow(0, 4).AddText("Next Stage Name", 2, 10).AddTextEdit(waitForStageAndCycleIdCondition.StageName))
				.AddRow(GeHelper.CreateRow(0, 4).AddText("Next Cycle Id", 2, 10).AddTextEdit(waitForStageAndCycleIdCondition.CycleId));
		}

		private static object GetWaitForStageAndCycleIdCondition(GaffInterface gi)
		{
			var stageName = gi.GetItemAt<GeTextEdit>(0, 1).Text;
			var cycleId = gi.GetItemAt<GeTextEdit>(1, 1).Text;
			return new WaitForStageAndCycleIdCondition(stageName, cycleId);
		}

		private static GaffInterface CreateWaitForStageConditionInterface(WaitForStageCondition waitForStageCondition)
		{
			return GeHelper.CreateInterface()
				.AddRow(GeHelper.CreateRow(0, 4).AddText("Next Stage Name", 2, 10).AddTextEdit(waitForStageCondition.StageName));
		}

		private static object GetWaitForStageCondition(GaffInterface gi)
		{
			var stageName = gi.GetItemAt<GeTextEdit>(0, 1).Text;
			return new WaitForStageCondition(stageName);
		}

		private static GaffInterface CreateTriggerCountConditionInterface(TriggerCountCondition triggerCountCondition)
		{
			return GeHelper.CreateInterface()
				.AddRow(GeHelper.CreateRow(0, 4).AddText("Trigger Count (Min-Max)", 2, 10).AddNumberEdit((int)triggerCountCondition.MinCount, 0, 5).AddText("-", 0, 5).AddNumberEdit((int)triggerCountCondition.MaxCount));
		}

		private static object GetTriggerCountCondition(GaffInterface gi)
		{
			var minCount = gi.GetItemAt<GeNumber>(0, 1).Value;
			var maxCount = gi.GetItemAt<GeNumber>(0, 3).Value;
			return new TriggerCountCondition((uint)minCount, (uint)maxCount);
		}

		private static GaffInterface CreateNextStageConditionInterface(NextStageCondition nextStageCondition)
		{
			return GeHelper.CreateInterface()
				.AddRow(GeHelper.CreateRow(0, 4).AddText("Next Stage Name", 2, 10).AddTextEdit(nextStageCondition.NextStageName))
				.AddRow(GeHelper.CreateRow(0, 4).AddText("Next Cycle Id", 2, 10).AddTextEdit(nextStageCondition.NextCycleId))
				.AddRow(GeHelper.CreateRow(0, 4).AddText("Ignore Cycle Id", 2, 10).AddCheckBox(nextStageCondition.IgnoreCycleId));
		}

		private static object GetNextStageCondition(GaffInterface gi)
		{
			var stageName = gi.GetItemAt<GeTextEdit>(0, 1).Text;
			var cycleId = gi.GetItemAt<GeTextEdit>(1, 1).Text;
			var ignoreCycleId = gi.GetItemAt<GeCheckBox>(2, 1).IsChecked;
			return new NextStageCondition(stageName, ignoreCycleId, cycleId);
		}

		private static GaffInterface CreateCurrentStageConditionInterface(CurrentStageCondition currentStageCondition)
		{
			return GeHelper.CreateInterface()
				.AddRow(GeHelper.CreateRow(0, 4).AddText("Stage Name", 2, 10).AddTextEdit(currentStageCondition.StageName))
				.AddRow(GeHelper.CreateRow(0, 4).AddText("Cycle Id", 2, 10).AddTextEdit(currentStageCondition.CycleId))
				.AddRow(GeHelper.CreateRow(0, 4).AddText("Ignore Cycle Id", 2, 10).AddCheckBox(currentStageCondition.IgnoreCycleId));
		}

		private static object GetCurrentStageCondition(GaffInterface gi)
		{
			var stageName = gi.GetItemAt<GeTextEdit>(0, 1).Text;
			var cycleId = gi.GetItemAt<GeTextEdit>(1, 1).Text;
			var ignoreCycleId = gi.GetItemAt<GeCheckBox>(2, 1).IsChecked;
			return new CurrentStageCondition(stageName, ignoreCycleId, cycleId);
		}

		private static GaffInterface CreateMultiStripFinderInterface(MultiStripFinder multiStripFinder)
		{
			return GeHelper.CreateInterface()
				.AddRow(CreateStringConditionRow("Context", multiStripFinder.ContextCondition))
				.AddRow(CreateStringConditionRow("Symbol", multiStripFinder.SymbolCondition))
				.AddRow(GeHelper.CreateRow(0, 4).AddText("Expected Decisions", 2, 10).AddNumberEdit(multiStripFinder.ExpectedDecisions))
				.AddRow(GeHelper.CreateRow(0, 4).AddText("Symbol Window", 2, 10).AddNumberEdit((int)multiStripFinder.SymbolWindow))
				.AddRow(GeHelper.CreateRow(0, 4).AddText("Count (Min-Max)", 2, 10).AddNumberEdit(multiStripFinder.MinCount, 0, 5).AddText("-", 0, 5).AddNumberEdit(multiStripFinder.MaxCount))
				.AddRow(GeHelper.CreateRow(0, 4).AddText("Selection Strategy", 2, 10).AddItem(multiStripFinder.Strategy.ToEnumCombo()));
		}

		private static MultiStripFinder GetMultiStripFinder(GaffInterface ge)
		{
			var contextCondition = FromRow(ge, 0);
			var symbolCondition = FromRow(ge, 1);
			var expectedDecisions = ge.GetItemAt<GeNumber>(2, 1).Value;
			var symbolWindow = ge.GetItemAt<GeNumber>(3, 1).Value;
			var minCount = ge.GetItemAt<GeNumber>(4, 1).Value;
			var maxCount = ge.GetItemAt<GeNumber>(4, 3).Value;
			var enumValue = ge.GetItemAt<GeCombo>(5, 1).GetEnumValue<SelectionStrategy>();

			return new MultiStripFinder(contextCondition, symbolCondition, (uint)symbolWindow, expectedDecisions, minCount, maxCount, enumValue);
		}

		private static GaffInterface CreateStripFinderInterface(StripFinder multiStripFinder)
		{
			return GeHelper.CreateInterface()
				.AddRow(CreateStringConditionRow("Context", multiStripFinder.ContextCondition))
				.AddRow(CreateStringConditionRow("Symbol", multiStripFinder.SymbolCondition))
				.AddRow(GeHelper.CreateRow(0, 10).AddText("Symbol Window").AddNumberEdit((int)multiStripFinder.SymbolWindow));
		}

		private static StripFinder GetStripFinder(GaffInterface ge)
		{
			var contextCondition = FromRow(ge, 0);
			var symbolCondition = FromRow(ge, 1);
			var symbolWindow = ge.GetItemAt<GeNumber>(2, 1).Value;

			return new StripFinder(contextCondition, symbolCondition, (uint)symbolWindow);
		}

		private static GaffInterface CreateSimpleDecisionMakerInterface(SimpleDecisionMaker simpleDecisionMaker)
		{
			return GeHelper.CreateInterface()
				.AddRow(CreateStringConditionRow("Context", simpleDecisionMaker.ContextCondition))
				.AddRow(GeHelper.CreateRow(0, 10).AddText("Hit/Success").AddCheckBox(simpleDecisionMaker.DesiredResult));
		}

		private static SimpleDecisionMaker GetSimpleDecisionMaker(GaffInterface ge)
		{
			var contextCondition = FromRow(ge, 0);
			var hit = ge.GetItemAt<GeCheckBox>(1, 1).IsChecked;

			return new SimpleDecisionMaker(contextCondition, hit);
		}

		private static GaffEditorRow CreateStringConditionRow(string title, StringCondition stringCondition)
		{
			return GeHelper.CreateRow(0, 10).AddText(title, 0, 10).AddItem(stringCondition.Type.ToEnumCombo(0, 5)).AddTextEdit(stringCondition.Text, 0, 5).AddText("Invert").AddCheckBox(stringCondition.Invert, 4);
		}

		private static StringCondition FromRow(GaffInterface ge, int row)
		{
			return new StringCondition(ge.GetItemAt<GeTextEdit>(row, 2).Text, ge.GetItemAt<GeCombo>(row, 1).GetEnumValue<Constraint>(), ge.GetItemAt<GeCheckBox>(row, 4).IsChecked);
		}

		private static GaffInterface CreateTotalAwardedPrizeRangeConditionInterface(TotalAwardedPrizeRangeCondition awardedPrizeRangeCondition)
		{
			return GeHelper.CreateInterface()
				.AddRow(GeHelper.CreateRow(0, 4).AddText("Prize Range (Min-Max)", 2, 10).AddNumberEdit(awardedPrizeRangeCondition.MinPrize, 0, 5).AddText("-", 0, 5).AddNumberEdit(awardedPrizeRangeCondition.MaxPrize));
		}

		private static object GetTotalAwardedPrizeRangeCondition(GaffInterface gi)
		{
			var minPrize = gi.GetItemAt<GeNumber>(0, 1).Value;
			var maxPrize = gi.GetItemAt<GeNumber>(0, 3).Value;
			return new TotalAwardedPrizeRangeCondition(minPrize, maxPrize);
		}

		private static GaffInterface CreateSelectSymbolsInterface(SelectSymbols selectSymbols)
		{
			return GeHelper.CreateInterface()
				.AddRow(CreateStringConditionRow("Context", selectSymbols.ContextCondition))
				.AddRow(CreateStringConditionRow("Symbol", selectSymbols.SymbolCondition))
				.AddRow(GeHelper.CreateRow(0, 4).AddText("Symbol Window", 2, 10).AddNumberEdit((int)selectSymbols.SymbolWindow))
				.AddRow(GeHelper.CreateRow(0, 4).AddText("Ratio (True-False)", 2, 10).AddNumberEdit((int)selectSymbols.TrueWeight, 0, 5).AddText("-", 0, 5).AddNumberEdit((int)selectSymbols.FalseWeight));
		}

		private static SelectSymbols GetSelectSymbols(GaffInterface ge)
		{
			var contextCondition = FromRow(ge, 0);
			var symbolCondition = FromRow(ge, 1);
			var symbolWindow = ge.GetItemAt<GeNumber>(2, 1).Value;
			var trueWeight = ge.GetItemAt<GeNumber>(3, 1).Value;
			var falseWeight = ge.GetItemAt<GeNumber>(3, 3).Value;

			return new SelectSymbols(contextCondition, symbolCondition, (uint)symbolWindow, (ulong)trueWeight, (ulong)falseWeight);
		}

		private static GaffInterface CreateCycleStatesStepConditionInterface(CycleStatesStepCondition cycleStatesCondition)
		{
			return GeHelper.CreateInterface()
				.AddRow(GeHelper.CreateRow(0, 4).AddText("CycleStates", 2, 10).AddItem(cycleStatesCondition.CycleStates.ToMultiEnumCombo(true)));
		}

		private static object GetCycleStatesStepCondition(GaffInterface gi)
		{
			var cycleStates = gi.GetItemAt<GeMultiCombo>(0, 1).GetMultiEnumValue<CycleStates>();

			return new CycleStatesStepCondition(cycleStates);
		}

		private static GaffInterface CreateCycleStatesResultConditionInterface(CycleStatesResultCondition cycleStatesCondition)
		{
			return GeHelper.CreateInterface()
				.AddRow(GeHelper.CreateRow(0, 4).AddText("Cycle States", 2, 10).AddItem(cycleStatesCondition.CycleStates.ToMultiEnumCombo(true)));
		}

		private static object GetCycleStatesResultCondition(GaffInterface gi)
		{
			var cycleStates = gi.GetItemAt<GeMultiCombo>(0, 1).GetMultiEnumValue<CycleStates>();

			return new CycleStatesResultCondition(cycleStates);
		}

		private static GaffInterface CreateHasProgressiveAwardedConditionInterface(HasProgressiveAwardedCondition hasProgressiveTriggerCondition)
		{
			return GeHelper.CreateInterface()
				.AddRow(GeHelper.CreateRow(0, 4).AddText("Progressive Level", 2, 10).AddTextEdit(hasProgressiveTriggerCondition.ProgressiveName));
		}

		private static object GetHasProgressiveAwardedCondition(GaffInterface gi)
		{
			var progressiveLevel = gi.GetItemAt<GeTextEdit>(0, 1).Text;
			return new HasProgressiveAwardedCondition(progressiveLevel);
		}

		private static GaffInterface CreateMaxGamesConditionInterface(MaxGamesCondition maxGamesCondition)
		{
			return GeHelper.CreateInterface()
				.AddRow(GeHelper.CreateRow(0, 4).AddText("Max Games", 2, 10).AddNumberEdit(maxGamesCondition.MaxGames));
		}

		private static object GetMaxGamesCondition(GaffInterface gi)
		{
			var gameCount = gi.GetItemAt<GeNumber>(0, 1).Value;
			return new MaxGamesCondition(gameCount);
		}

		private static GaffInterface CreatSymbolVisibleConditionInterface(SymbolVisibleCondition symbolVisibleCondition)
		{
			return GeHelper.CreateInterface()
				.AddRow(CreateStringConditionRow("SymbolWindow Output Name", symbolVisibleCondition.SymbolWindowOutputName))
				.AddRow(new GaffEditorRow(new GaffEditorItem[] { new GeText("Symbol Window Conditions"), new GeTextEdit(symbolVisibleCondition.SymbolConditionText, 3) }))
				.AddRow(new GaffEditorRow(new GaffEditorItem[]
				{
					new GeText("<Sym>[|<Sym>][:<Row>.<Column>] ... \n" +
						"Where <Row> and <Column> can be a number, '?' for any position and a '*' for all positions.\n" +
						"Examples:\n" +
						"SC:?.0 SC:?.1 SC:?.4 Ensure at least one SC is the first, second and fifth columns in any row position. \n" +
						"GRAND Ensure there is at least one GRAND symbol in the symbol window.\n" +
						"WW:*.2 Ensure that the 3rd column is all WW symbols.\n" +
						"M1|M2|M3:0.0 will ensure a M1 or M2 or M3 is the top left corner.\n")
				}));
		}

		private static SymbolVisibleCondition GetSymbolVisibleCondition(GaffInterface ge)
		{
			var contextCondition = FromRow(ge, 0);
			var selections = ge.GetItemAt<GeTextEdit>(1, 1).Text;

			return new SymbolVisibleCondition(contextCondition, selections);
		}

		private static GaffInterface CreatPrizesConditionInterface(PrizesCondition symbolVisibleCondition)
		{
			return GeHelper.CreateInterface()
				.AddRow(CreateStringConditionRow("Prizes Output Name", symbolVisibleCondition.PrizesOutputName))
				.AddRow(new GaffEditorRow(new GaffEditorItem[] { new GeText("Prize Conditions"), new GeTextEdit(symbolVisibleCondition.PrizeConditionText, 3) }))
				.AddRow(new GaffEditorRow(new GaffEditorItem[]
				{
					new GeText("<PrizeName>^<HitCount>[|<PrizeNAme>^<HitCount>]... ... \n" +
						"Where <HitCount> can be the hit count or a '?' for any hit count.\n" +
						"Examples:\n" +
						"M1^? - at least one M1 prize of any count.\n" +
						"M1^5 - at least one 5 of a kind M1 prize.\n" +
						"M1^? M1^? - at least two M1 prizes of any count.\n" +
						"M1^5 M1^4 - at least one 5 of a kind M1 prize and at least one 4 of a kind M1 prize .\n" +
						"M1^?|M2^? - at least one M1 or M2 prize of any count.")
				}));
		}

		private static GaffInterface CreateWaitForPrizesConditionInterface(WaitForPrizesCondition symbolVisibleCondition)
		{
			return GeHelper.CreateInterface()
				.AddRow(CreateStringConditionRow("Prizes Output Name", symbolVisibleCondition.PrizesOutputName))
				.AddRow(new GaffEditorRow(new GaffEditorItem[] { new GeText("Prize Conditions"), new GeTextEdit(symbolVisibleCondition.PrizeConditionText, 3) }))
				.AddRow(new GaffEditorRow(new GaffEditorItem[]
				{
					new GeText("<PrizeName>^<HitCount>[|<PrizeNAme>^<HitCount>]... ... \n" +
						"Where <HitCount> can be the hit count or a '?' for any hit count.\n" +
						"Examples:\n" +
						"M1^? - at least one M1 prize of any count.\n" +
						"M1^5 - at least one 5 of a kind M1 prize.\n" +
						"M1^? M1^? - at least two M1 prizes of any count.\n" +
						"M1^5 M1^4 - at least one 5 of a kind M1 prize and at least one 4 of a kind M1 prize .\n" +
						"M1^?|M2^? - at least one M1 or M2 prize of any count.")
				}));
		}

		private static WaitForPrizesCondition GetWaitForPrizesCondition(GaffInterface ge)
		{
			var contextCondition = FromRow(ge, 0);
			var selections = ge.GetItemAt<GeTextEdit>(1, 1).Text;

			return new WaitForPrizesCondition(contextCondition, selections);
		}

		private static PrizesCondition GetPrizesCondition(GaffInterface ge)
		{
			var contextCondition = FromRow(ge, 0);
			var selections = ge.GetItemAt<GeTextEdit>(1, 1).Text;

			return new PrizesCondition(contextCondition, selections);
		}

		private static GaffInterface CreateSelectChosenSymbolsInterface(SelectChosenSymbols selectChosenSymbols)
		{
			return GeHelper.CreateInterface()
				.AddRow(CreateStringConditionRow("Context", selectChosenSymbols.ContextCondition))
				.AddRow(GeHelper.CreateRow().AddText("Selections", 2, 10).AddTextEdit(selectChosenSymbols.Selections))
				.AddRow(GeHelper.CreateRow(0, 4).AddText("Symbol Window", 2, 10).AddNumberEdit(selectChosenSymbols.SymbolWindow))
				.AddRow(GeHelper.CreateRow().AddText("<Sym>[|<Sym>][:<Index>] or <Sym>[|<Sym>][?<Count>] ... \n" +
					"Where <Index> is the position in chosen array. <Count> indicates how many of a symbol condition to test for.\n" +
					"Examples:\n" +
					"M1|M2?2 will ask for 2 of M1 or M2. E.G in a two index request M2,M1 or M1,M2 or M1,M1 or M2,M2 all satisfy the condition.\n" +
					"M1:3 M2:0 in a 5 index request could look like M2,F1,SC,M1,SC.\n" +
					"GRAND Ensure there is at least one GRAND in the chosen items.\n"));
		}

		private static SelectChosenSymbols GetSelectChosenSymbols(GaffInterface ge)
		{
			var contextCondition = FromRow(ge, 0);
			var selections = ge.GetItemAt<GeTextEdit>(1, 1).Text;
			var symbolWindow = ge.GetItemAt<GeNumber>(2, 1).Value;

			return new SelectChosenSymbols(contextCondition, selections, symbolWindow);
		}
	}
}