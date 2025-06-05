using System;
using System.Collections.Generic;
using Gaff.Conditions;
using Gaff.Core.Conditions;
using Gaff.Core.DecisionMakers;
using Gaff.Core.EditorTypes;
using Gaff.DecisionMakers;

namespace Gaff
{
	/// <summary>
	/// A collection of editors used by the GLE7 for specific gaff types (<see cref="DecisionMaker"/>, <see cref="ResultCondition"/> and <see cref="StepCondition"/>).  If no editor is found the default text editor will be used.
	/// NOTE: The public functions and properties in this class are required by the GLE7 interface and should not be renamed or removed.
	/// </summary>
	public static partial class Editors
	{
		/// <summary>
		/// Any editors for <see cref="DecisionMaker"/>, <see cref="ResultCondition"/> and <see cref="StepCondition"/>'s should be in this collection.
		/// NOTE: Do not rename or remove this property.
		/// </summary>
		public static IReadOnlyList<(Type, Func<object, GaffInterface>, Func<GaffInterface, object>)> Interfaces { get; } = new (Type, Func<object, GaffInterface>, Func<GaffInterface, object>)[]
		{
			(typeof(CurrentStageCondition), o => CreateCurrentStageConditionInterface((CurrentStageCondition)o), GetCurrentStageCondition),
			(typeof(NextStageCondition), o => CreateNextStageConditionInterface((NextStageCondition)o), GetNextStageCondition),
			(typeof(TriggerCountCondition), o => CreateTriggerCountConditionInterface((TriggerCountCondition)o), GetTriggerCountCondition),
			(typeof(WaitForStageCondition), o => CreateWaitForStageConditionInterface((WaitForStageCondition)o), GetWaitForStageCondition),
			(typeof(WaitForStageAndCycleIdCondition), o => CreateWaitForStageAndCycleIdConditionInterface((WaitForStageAndCycleIdCondition)o), GetWaitForStageAndCycleIdCondition),
			(typeof(GameCountCondition), o => CreateGameCountConditionInterface((GameCountCondition)o), GetGameCountCondition),
			(typeof(AwardedPrizeRangeCondition), o => CreateAwardedPrizeRangeConditionInterface((AwardedPrizeRangeCondition)o), GetAwardedPrizeRangeCondition),
			(typeof(TotalAwardedPrizeRangeCondition), o => CreateTotalAwardedPrizeRangeConditionInterface((TotalAwardedPrizeRangeCondition)o), GetTotalAwardedPrizeRangeCondition),
			(typeof(MultiStripFinder), o => CreateMultiStripFinderInterface((MultiStripFinder)o), GetMultiStripFinder),
			(typeof(StripFinder), o => CreateStripFinderInterface((StripFinder)o), GetStripFinder),
			(typeof(SimpleDecisionMaker), o => CreateSimpleDecisionMakerInterface((SimpleDecisionMaker)o), GetSimpleDecisionMaker),
			(typeof(SelectSymbols), o => CreateSelectSymbolsInterface((SelectSymbols)o), GetSelectSymbols),
			(typeof(HasProgressiveAwardedCondition), o => CreateHasProgressiveAwardedConditionInterface((HasProgressiveAwardedCondition)o), GetHasProgressiveAwardedCondition),
			(typeof(MaxGamesCondition), o => CreateMaxGamesConditionInterface((MaxGamesCondition)o), GetMaxGamesCondition),
			(typeof(CycleStatesStepCondition), o => CreateCycleStatesStepConditionInterface((CycleStatesStepCondition)o), GetCycleStatesStepCondition),
			(typeof(CycleStatesResultCondition), o => CreateCycleStatesResultConditionInterface((CycleStatesResultCondition)o), GetCycleStatesResultCondition),
			(typeof(SymbolVisibleCondition), o => CreatSymbolVisibleConditionInterface((SymbolVisibleCondition)o), GetSymbolVisibleCondition),
			(typeof(PrizesCondition), o => CreatPrizesConditionInterface((PrizesCondition)o), GetPrizesCondition),
			(typeof(WaitForPrizesCondition), o => CreateWaitForPrizesConditionInterface((WaitForPrizesCondition)o), GetWaitForPrizesCondition),
			(typeof(SelectChosenSymbols), o => CreateSelectChosenSymbolsInterface((SelectChosenSymbols)o), GetSelectChosenSymbols),
			(typeof(NoTriggerCondition), o => GeHelper.CreateInterface(), _ => new NoTriggerCondition())
		};
	}
}