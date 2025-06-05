using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Gaff.Core.Conditions;
using Logic.Core.DecisionGenerator;
using Logic.Core.Engine;

namespace Gaff.Core.GaffEditor
{
	/// <summary>
	/// The general idea here is to define a set of decision rules/conditions that will lead to a desired set of GameCycleResult conditions.
	/// Once all GameCycleConditions are met in a single result we proceed to the next...
	/// If this result is a new stage we can load the next stage specific rule.  rinse and repeat until we run out.
	/// </summary>
	public static class GaffRunner
	{
		private const int MaxSearchGames = 20000;

		public static async Task<GaffResult> RunGaffAsync(this IReadOnlyList<GaffStep> gaffSteps, IRunner runner, IDecisionGenerator primaryDecisionGenerator, Inputs initialInputs, bool isWasm)
		{
			var decisionProvider = new DecisionProvider();
			var stepIndex = 0;
			var stepStateData = new Dictionary<StepCondition, object>();
			CycleResult currentResult = null;
			var gaffResult = new List<StageGaffResult>();
			var overallSearchCount = 0;
			var stepSearchCount = 0;
			var stepSearchCounts = new int[gaffSteps.Count];
			var stepFailCounts = new int[gaffSteps.Count];
			var initialResultForStep = (CycleResult)null;

			while (stepIndex < gaffSteps.Count && overallSearchCount < MaxSearchGames)
			{
				var selectedGaff = gaffSteps[stepIndex];

				// Process the stage.
				var inputs = runner.CreateInputs(initialInputs, currentResult);

				// Set the decision creators for this particular cycleGaff
				decisionProvider.SetMakers(selectedGaff.Decisions);

				var storeResult = false;
				var counter = 0;

				// Run the same game cycle until we get a match
				while (counter < MaxSearchGames)
				{
					// Need to periodically delay to allow the wasm UI to update.
					if (isWasm && counter % 500 == 0)
						await Task.Delay(1);

					try
					{
						// Reset Gaff decision generator to store next decisions
						var decisionGenerator = new GaffDecisionGenerator(primaryDecisionGenerator, decisionProvider);
						var testContext = new ContextTester(decisionGenerator);
						var missingVariables = runner.ResolveMissingVariables(testContext, inputs, currentResult);
						var localInputs = inputs.ReplaceOrAdd(missingVariables);
						var stageResults = runner.EvaluateStage(testContext, localInputs);
						var nextResult = runner.GenerateCycleResult(localInputs, currentResult, stageResults);
						storeResult = CheckForAllTrue(selectedGaff.NextResultConditions, initialResultForStep, nextResult, gaffResult);
						counter++;
						overallSearchCount++;

						if (storeResult)
						{
							// Add this result and the decisions made to the overall gaff result.
							gaffResult.Add(new StageGaffResult(selectedGaff.Title, decisionGenerator.OrderedDecisions, nextResult, decisionGenerator.GetErrorMessages()));
							// Set the previous now that we have a result that fits the conditions.
							currentResult = nextResult;
							// Move on
							break;
						}
					}
					catch (Exception e)
					{
						stepSearchCounts[stepIndex] += stepSearchCount;
						stepFailCounts[stepIndex]++;
						throw new Exception($"Failed to generate result at gaff result index {gaffResult.Count} in step '{gaffSteps[stepIndex].Title}': exception details below", e);
					}
				}

				stepSearchCount += counter;

				// If we broke without matching we failed the while condition.
				if (!storeResult)
				{
					stepSearchCounts[stepIndex] += stepSearchCount;
					stepFailCounts[stepIndex]++;
					return new GaffResult(gaffResult, stepSearchCounts, stepFailCounts, false, $"Failed to find a result in step '{gaffSteps[stepIndex].Title}'");
				}

				var moveToNextStep = CheckForAllTrue(selectedGaff.NextStepConditions, initialResultForStep, currentResult, gaffResult, stepStateData);

				switch (moveToNextStep)
				{
					case StepConditionResult.Fail:
					{
						stepSearchCounts[stepIndex] += stepSearchCount;
						stepFailCounts[stepIndex]++;
						stepSearchCount = 0;

						// If a step condition returned a fail then we have to restart.
						stepIndex = 0;
						stepStateData = new Dictionary<StepCondition, object>();
						currentResult = null;
						gaffResult = new List<StageGaffResult>();
						initialResultForStep = null;
						break;
					}
					case StepConditionResult.Found:
					{
						stepSearchCounts[stepIndex] += stepSearchCount;
						stepSearchCount = 0;
						initialResultForStep = currentResult;
						stepIndex++;
						break;
					}
					case StepConditionResult.KeepSearching:
					{
						// Don't start a new game in the search mode, only complete existing.
						if (currentResult != null && currentResult.Cycles.IsFinished)
						{
							stepSearchCounts[stepIndex] += stepSearchCount;
							stepFailCounts[stepIndex]++;
							stepSearchCount = 0;

							// If a step is not found and a game sequence is returning to the main game we have to call it a failure and reset the step.
							stepIndex = 0;
							stepStateData = new Dictionary<StepCondition, object>();
							currentResult = null;
							gaffResult = new List<StageGaffResult>();
							initialResultForStep = null;
						}

						break;
					}
				}
			}

			if (overallSearchCount != stepSearchCounts.Sum())
				Debug.WriteLine($"SearchCount mismatch {overallSearchCount} {stepSearchCounts.Sum()}");

			return stepIndex >= gaffSteps.Count ? new GaffResult(gaffResult, stepSearchCounts, stepFailCounts) : new GaffResult(gaffResult, stepSearchCounts, stepFailCounts, false, $"The gaff failed to complete.  Last step '{gaffSteps[stepIndex].Title}'");
		}

		private static bool CheckForAllTrue(IReadOnlyCollection<ResultCondition> cycleResultConditions, CycleResult initialResultForStep, CycleResult nextResult, IReadOnlyList<StageGaffResult> sequenceUpToNow)
		{
			if (cycleResultConditions == null || cycleResultConditions.Count == 0)
				return true;

			return cycleResultConditions.All(gr =>
			{
				var result = gr.CheckCondition(nextResult, initialResultForStep, sequenceUpToNow);

				return result;
			});
		}

		private static StepConditionResult CheckForAllTrue(IReadOnlyCollection<StepCondition> cycleResultConditions, CycleResult initialResultForStep, CycleResult nextResult, IReadOnlyList<StageGaffResult> sequenceUpToNow, IDictionary<StepCondition, object> stateData)
		{
			if (cycleResultConditions == null || cycleResultConditions.Count == 0)
				return StepConditionResult.Found;

			foreach (var gr in cycleResultConditions)
			{
				stateData.TryGetValue(gr, out var sd);

				var result = gr.CheckCondition(nextResult, initialResultForStep, sequenceUpToNow, ref sd);

				if (result == StepConditionResult.Fail)
					return StepConditionResult.Fail;

				if (sd != null)
					stateData[gr] = sd;

				if (result == StepConditionResult.KeepSearching)
					return StepConditionResult.KeepSearching;
			}

			return StepConditionResult.Found;
		}
	}
}