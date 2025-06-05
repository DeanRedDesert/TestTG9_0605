using Gaff.Conditions;
using Gaff.Core;
using Gaff.Core.Conditions;
using Gaff.Core.DecisionMakers;
using Gaff.DecisionMakers;
using System.Collections.Generic;

namespace Game
{
	public class GaffSequences : IGaffSequences
	{
		private readonly IReadOnlyList<GaffSequence> gaffSequences = new GaffSequence[]
		{
			new GaffSequence("Free Games", false, new GaffStep[] { new GaffStep("Trigger", new List<DecisionMaker>() { new MultiStripFinder(new StringCondition("Base_Reel", Constraint.StartsWith, false), new StringCondition("SCAT1", Constraint.Exact, false), 3U, 5, 3, 3, SelectionStrategy.Any) }, new ResultCondition[] { new NextStageCondition("FreeGames", false, "FreeGames") }, new StepCondition[0]) }),
			new GaffSequence("Respins", false, new GaffStep[] { new GaffStep("Trigger", new DecisionMaker[] { new SelectSymbols(new StringCondition("Base_Reel", Constraint.StartsWith, false), new StringCondition("C", Constraint.StartsWith, false), 3U, 3UL, 1UL) }, new ResultCondition[] { new NextStageCondition("Respin", false, "BaseRespin") }, new StepCondition[0]) }),
			new GaffSequence(
				"Free Games With Retrigger",
				false,
				new GaffStep[]
				{
					new GaffStep("Trigger", new DecisionMaker[] { new SelectSymbols(new StringCondition("Base_Reel", Constraint.StartsWith, false), new StringCondition("SCAT1", Constraint.Exact, false), 3U, 3UL, 1UL) }, new ResultCondition[] { new NextStageCondition("FreeGames", false, "FreeGames") }, new StepCondition[0]),
					new GaffStep("Wait", new DecisionMaker[0], new ResultCondition[] { new NoTriggerCondition() }, new StepCondition[] { new GameCountCondition(3) }),
					new GaffStep("Retrigger", new DecisionMaker[] { new StripFinder(new StringCondition("Free_Reel", Constraint.StartsWith, false), new StringCondition("SCAT1", Constraint.Exact, false), 3U) }, new ResultCondition[0], new StepCondition[0])
				}
			),
			new GaffSequence(
				"Progressive(Grand)",
				false,
				new GaffStep[]
				{
					new GaffStep("Trigger", new DecisionMaker[] { new SelectSymbols(new StringCondition("Base_Reel", Constraint.StartsWith, false), new StringCondition("C", Constraint.StartsWith, false), 3U, 3UL, 1UL) }, new ResultCondition[] { new NextStageCondition("Respin", false, "BaseRespin") }, new StepCondition[0]),
					new GaffStep(
						"Find Grand",
						new DecisionMaker[]
						{
							new SelectSymbols(new StringCondition("RespinSymbolReplacement_Strip_", Constraint.StartsWith, false), new StringCondition("GRAND", Constraint.Exact, false), 1U, 1UL, 0UL),
							new SelectSymbols(new StringCondition("Respin_Reel", Constraint.StartsWith, false), new StringCondition("GRAND", Constraint.Exact, false), 3U, 1UL, 0UL)
						},
						new ResultCondition[] { new HasProgressiveAwardedCondition("GRAND") },
						new StepCondition[0]
					)
				}
			),
			new GaffSequence(
				"Progressive(Major)",
				false,
				new GaffStep[]
				{
					new GaffStep("Trigger", new DecisionMaker[] { new SelectSymbols(new StringCondition("Base_Reel", Constraint.StartsWith, false), new StringCondition("C", Constraint.StartsWith, false), 3U, 3UL, 1UL) }, new ResultCondition[] { new NextStageCondition("Respin", false, "BaseRespin") }, new StepCondition[0]),
					new GaffStep(
						"Find Major",
						new DecisionMaker[]
						{
							new SelectSymbols(new StringCondition("RespinSymbolReplacement_Strip_", Constraint.StartsWith, false), new StringCondition("MAJOR", Constraint.Exact, false), 1U, 1UL, 0UL),
							new SelectSymbols(new StringCondition("Respin_Reel", Constraint.StartsWith, false), new StringCondition("MAJOR", Constraint.Exact, false), 3U, 1UL, 0UL)
						},
						new ResultCondition[] { new HasProgressiveAwardedCondition("MAJOR") },
						new StepCondition[0]
					)
				}
			),
			new GaffSequence(
				"A full respin feature winning between 5000-10000 credits",
				false,
				new GaffStep[]
				{
					new GaffStep("Trigger", new DecisionMaker[] { new SelectSymbols(new StringCondition("Base_Reel", Constraint.StartsWith, false), new StringCondition("C", Constraint.StartsWith, false), 3U, 3UL, 1UL) }, new ResultCondition[] { new NextStageCondition("Respin", false, "BaseRespin") }, new StepCondition[0]),
					new GaffStep("Get a good feature with under 30 games.", new DecisionMaker[0], new ResultCondition[0], new StepCondition[] { new MaxGamesCondition(30), new TotalAwardedPrizeRangeCondition(5000, 10000), new CycleStatesStepCondition(CycleStates.None | CycleStates.FeatureComplete) })
				}
			)
		};

		public IReadOnlyList<GaffSequence> GetSequences() => gaffSequences;
	}
}
