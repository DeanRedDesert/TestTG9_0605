using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.Engine;
using Logic.Core.Types.Exits;

namespace Logic
{
	// ReSharper disable once UnusedType.Global - Used via code gen
	// ReSharper disable UnusedMember.Global - Used via code gen
	public static class CustomiseCycle
	{
		/// <summary>
		/// Customise this method to add or change inputs before a stage is evaluated.
		/// </summary>
		/// <param name="previousResult">The results from the previous cycle, will be null on cold boot.</param>
		/// <param name="initiatingInputs">Inputs to use when starting a new game.</param>
		public static Inputs CreateInputsForCycle(CycleResult previousResult, Inputs initiatingInputs)
		{
			if (previousResult == null)
				return initiatingInputs;

			if (previousResult.Cycles.IsFinished)
			{
				// Base game
				var permanentsFromPreviousInputs = previousResult.Inputs.OfType<Variable>()
					.Where(v => v.Lifespan == Lifespan.Permanent)
					.ToArray();
				var permanentsFromPreviousResults = previousResult.StageResults
					.Where(v => v.Type == StageResultType.VariablePermanent)
					.Select(v => new Variable(v.Name, v.Value, Lifespan.Permanent))
					.ToArray();

				// We add permanents from the results last as they are the most up to date value.
				return initiatingInputs
					.ReplaceOrAdd(permanentsFromPreviousInputs)
					.ReplaceOrAdd(permanentsFromPreviousResults);
			}

			// Non base game.
			var newInputs = previousResult.Inputs.ToList();

			// Remove any OneCycle inputs
			for (var i = newInputs.Count - 1; i >= 0; i--)
			{
				if (newInputs[i] is Variable v && v.Lifespan == Lifespan.OneCycle)
					newInputs.RemoveAt(i);
			}

			// Add the Cycles and all the inputs from the previous stage results.
			var inputsToOverwrite = new List<Input> { new Input("Cycles", previousResult.Cycles) };
			inputsToOverwrite.AddRange(previousResult.StageResults.Where(r => r.IsVariable).Select(r =>
			{
				Lifespan lifespan;

				switch (r.Type)
				{
					case StageResultType.VariablePermanent: lifespan = Lifespan.Permanent; break;
					case StageResultType.VariableOneGame: lifespan = Lifespan.OneGame; break;
					case StageResultType.VariableOneCycle: lifespan = Lifespan.OneCycle; break;
					case StageResultType.AwardCreditsList:
					case StageResultType.ExitList:
					case StageResultType.ProgressiveList:
					case StageResultType.Presentation:
					default: throw new NotSupportedException();
				}

				return (Input)new Variable(r.Name, r.Value, lifespan);
			}));

			return new Inputs(newInputs).ReplaceOrAdd(inputsToOverwrite);
		}

		/// <summary>
		/// Customise this method to change the way triggers are processed, add any new cycles and set the order of the cycles.
		/// </summary>
		/// <param name="cycles">The cycle that was just executed to generate triggers.</param>
		/// <param name="currentCycle">The cycle that created the exits.</param>
		/// <param name="desiredExits">The desired exits that appeared in the cycle that was just executed.</param>
		/// <param name="stageConnections">The connections between the stages.</param>
		public static Cycles ProcessTriggers(Cycles cycles, CycleState currentCycle, IReadOnlyList<DesiredExit> desiredExits, IReadOnlyList<StageConnection> stageConnections)
		{
			foreach (var exit in desiredExits)
			{
				var handled = false;

				foreach (var connection in stageConnections)
				{
					if (connection.InitialStage == currentCycle.Stage && connection.ExitName == exit.Name)
					{
						handled = true;
						cycles = exit.CyclesModifier.ApplyExit(cycles, currentCycle, connection.FinalStage);
						break;
					}
				}

				if (!handled)
				{
					// If there is no connection for the exit then we assume the final stage is the current stage (a loop back exit).
					cycles = exit.CyclesModifier.ApplyExit(cycles, currentCycle, currentCycle.Stage);
				}
			}

			return cycles;
		}

		/// <summary>
		/// Customise this method to modify results before they are handed to the platform.
		/// </summary>
		public static CycleResult ModifyResultPostCycle(CycleResult cr)
		{
			return cr;
		}
	}
}
