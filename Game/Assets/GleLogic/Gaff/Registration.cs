using System;
using System.Collections.Generic;
using Gaff.Conditions;
using Gaff.Core;
using Gaff.Core.Conditions;
using Gaff.Core.DecisionMakers;
using Gaff.DecisionMakers;

namespace Gaff
{
	// ReSharper disable UnusedMember.Global - Used in GLE?
	// ReSharper disable once UnusedType.Global - Used in GLE?
	/// <summary>
	/// NOTE: The public functions and properties in this class are required by the GLE7 interface and should not be renamed or removed.
	/// </summary>
	public static class Registration
	{
		/// <summary>
		/// Conditions that can be used to add a new result to the gaff sequence.
		/// NOTE: Do not rename or remove this property.
		/// </summary>
		public static IReadOnlyList<(ResultCondition, string)> AddableResultConditionTypes { get; } = new (ResultCondition, string)[]
		{
			// Core types
			(new NoTriggerCondition(), "No Triggers"),
			(new SymbolVisibleCondition("SymbolWindow", "M1:0.? M2:1.?, M3:1,2"), "Symbol Visible"),
			(new PrizesCondition(new StringCondition("Prizes", Constraint.Contains, false), "M1^5"), "Prize Condition"),
			(new TriggerCountCondition(1, 5), "Trigger Count"),
			(new AwardedPrizeRangeCondition(0, 10000), "Awarded Prize Range"),
			(new NextStageCondition("Free", false, "Gold"), "Next Stage"),
			(new CurrentStageCondition("Free", false, "Green"), "Current Stage"),
			(new HasProgressiveAwardedCondition("GRAND"), "Has Progressive Trigger"),
			(new CycleStatesResultCondition(CycleStates.InitialTrigger), "Cycle State")
			// Add custom conditions here.
		};

		/// <summary>
		/// Conditions that can be used to determine when a step is complete.
		/// NOTE: Do not rename or remove this property.
		/// </summary>
		public static IReadOnlyList<(StepCondition, string)> AddableStepConditionTypes { get; } = new (StepCondition, string)[]
		{
			// Core types
			(new GameCountCondition(1), "Game Count"),
			(new MaxGamesCondition(10), "Max Games"),
			(new TotalAwardedPrizeRangeCondition(0, 10000), "Total Awarded Prize Range"),
			(new WaitForStageCondition("Free"), "Play Until Next Stage"),
			(new WaitForStageAndCycleIdCondition("Free", "Max"), "Play Until Next Stage and CycleId"),
			(new CycleStatesStepCondition(CycleStates.InitialTrigger), "Cycle State"),
			(new WaitForPrizesCondition(new StringCondition("Prizes", Constraint.Contains, false), "M1^5"), "Wait For Prize Condition")
			// Add custom conditions here.
		};

		/// <summary>
		/// A collection of decision makers that can be used to override specific decisions to give fine control over what result to produce.
		/// NOTE: Do not rename or remove this property.
		/// </summary>
		public static IReadOnlyList<(DecisionMaker, string)> AddableDecisionMakerTypes { get; } = new (DecisionMaker, string)[]
		{
			// Core types
			(new SimpleDecisionMaker(new StringCondition("Context", Constraint.Exact, false), true), "Simple Decision Maker"),
			(new StripFinder(new StringCondition("_Reel5", Constraint.EndsWith, false), "GREENDRAGON", 3), "Strip Finder"),
			(new SelectSymbols(new StringCondition("_Reel5", Constraint.EndsWith, false), "GREENDRAGON", 3, 1, 1), "Select Symbols"),
			(new MultiStripFinder(new StringCondition("CreateSymbolWindow_Reel", Constraint.StartsWith, false), "GREENDRAGON", 3, 5, 3, 3, SelectionStrategy.Any), "Multi Strip Finder"),
			(new SelectChosenSymbols(new StringCondition("SelectSymbols", Constraint.Exact, false), "M1:1 M2 M2", 1), "Select Chosen Symbols")
			// Add custom conditions here.
		};

		/// <summary>
		/// A function that is called on a successfully loaded gaff sequence related object.
		/// NOTE: Do not rename or remove this function.
		/// </summary>
		/// <param name="obj">The object that that derives from DecisionMaker, ResultCondition and StepCondition.</param>
		/// <returns>Any errors found.</returns>
		public static IReadOnlyList<string> Validate(object obj)
		{
			switch (obj)
			{
				case MultiStripFinder multiStripFinder:
				{
					var errors = new List<string>();
					if (multiStripFinder.MinCount > multiStripFinder.MaxCount)
						errors.Add("MinCount must be less than or equal to MaxCount.");
					if (multiStripFinder.MaxCount >= multiStripFinder.ExpectedDecisions || multiStripFinder.MinCount >= multiStripFinder.ExpectedDecisions)
						errors.Add("ExpectedDecisions must be within or equal to MaxCount and MinCount");
					return errors;
				}
				// You can add validation here for any classes that derive from DecisionMaker, ResultCondition and StepCondition.  Just update the switch statement as shown above.
				// If validation is more complicated than a few lines please break your validation into a private function and call it from the switch statement.
				default:
					return Array.Empty<string>();
			}
		}
	}
}