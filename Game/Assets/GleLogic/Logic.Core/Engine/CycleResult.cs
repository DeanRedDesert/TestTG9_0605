using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.Types;

namespace Logic.Core.Engine
{
	/// <summary>
	/// Provides all the results for a game cycle.
	/// </summary>
	public sealed class CycleResult
	{
		/// <summary>
		/// The inputs that were used to create this result.
		/// </summary>
		public Inputs Inputs { get; }

		/// <summary>
		/// The cycles (past/present and future) for the current game.
		/// </summary>
		public Cycles Cycles { get; }

		/// <summary>
		/// The prize awarded by the current cycle in credits.
		/// </summary>
		public Credits AwardedPrize { get; }

		/// <summary>
		/// The total prize awarded for the current game in credits.
		/// </summary>
		public Credits TotalAwardedPrize { get; }

		/// <summary>
		/// The collection of items that represent the results of the game cycle.
		/// </summary>
		public StageResults StageResults { get; }

		/// <summary>
		/// The progressives triggered in this cycle.
		/// </summary>
		public IReadOnlyList<string> Progressives { get; }

		/// <summary>
		/// Initialise a new instance of the CycleResult class.
		/// </summary>
		/// <param name="inputs">The inputs that were used to create this result.</param>
		/// <param name="cycles">The cycles (past/present and future) for the current game.</param>
		/// <param name="awardedPrize">The prize awarded by the current cycle.</param>
		/// <param name="totalAwardedPrize">The total awarded by all cycles of the current game.</param>
		/// <param name="stageResults">The collection of results.</param>
		/// <param name="progressives">The progressives triggered in this cycle.</param>
		public CycleResult(
			Inputs inputs,
			Cycles cycles,
			Credits awardedPrize,
			Credits totalAwardedPrize,
			StageResults stageResults,
			IReadOnlyList<string> progressives)
		{
			Inputs = inputs;
			Cycles = cycles;
			AwardedPrize = awardedPrize;
			TotalAwardedPrize = totalAwardedPrize;
			StageResults = stageResults;
			Progressives = progressives;
		}

		/// <summary>
		/// Helper method to retrieve the final value of all variables at the end of the cycle.
		/// </summary>
		/// <remarks>
		/// Looks for distinct Variables in the StageResults first and in the Inputs second.
		/// </remarks>
		// ReSharper disable once UnusedMember.Global
		public IReadOnlyDictionary<string, object> GetVariableValues()
		{
			var dict = StageResults.Where(r => r.IsVariable).ToDictionary(vr => vr.Name, vr => vr.Value);

			foreach (var v in Inputs.OfType<Variable>())
			{
				// ReSharper disable once CanSimplifyDictionaryLookupWithTryAdd - Doesn't exist in .Net 4.6
				if (!dict.ContainsKey(v.Name))
					dict.Add(v.Name, v.Value);
			}

			return dict;
		}

		/// <summary>
		/// Helper method to retrieve the final value of the Variable named <paramref name="variableName"/> at the end of the cycle.
		/// </summary>
		/// <remarks>
		/// Looks for the Variable in the StageResults first and in the Inputs second.
		/// </remarks>
		/// <exception cref="Exception">Throws if <paramref name="variableName"/> is not found.</exception>
		// ReSharper disable once UnusedMember.Global
		public object GetVariableValue(string variableName)
		{
			return TryGetVariableValue(variableName, out var v) ? v : throw new Exception($"Variable not found: {variableName}");
		}

		/// <summary>
		/// Helper method to try retrieve the final value of the Variable named <paramref name="variableName"/> at the end of the cycle.
		/// </summary>
		/// <remarks>
		/// Looks for the Variable in the StageResults first and in the Inputs second.
		/// </remarks>
		/// <returns>False if <paramref name="variableName"/> is not found.</returns>
		// ReSharper disable once MemberCanBePrivate.Global
		public bool TryGetVariableValue(string variableName, out object value)
		{
			// First look in the results.
			var variableResult = StageResults.FirstOrDefault(r => r.IsVariable && r.Name == variableName);

			if (variableResult != null)
			{
				value = variableResult.Value;
				return true;
			}

			// Second look in the inputs.
			var variable = Inputs.OfType<Variable>().FirstOrDefault(v => v.Name == variableName);

			if (variable != null)
			{
				value = variable.Value;
				return true;
			}

			// Not found.
			value = null;
			return false;
		}
	}
}