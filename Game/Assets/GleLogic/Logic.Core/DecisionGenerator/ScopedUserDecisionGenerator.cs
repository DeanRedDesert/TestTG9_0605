using System;
using System.Collections.Generic;
using System.Text;
using Logic.Core.Utility;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace Logic.Core.DecisionGenerator
{
	public sealed class ScopedUserDecisionGenerator : IToString
	{
		private readonly IDecisionGenerator decisionGenerator;

		public string Scope { get; }

		public ScopedUserDecisionGenerator(IDecisionGenerator decisionGenerator, string scope)
		{
			this.decisionGenerator = decisionGenerator;
			Scope = scope;
		}

		/// <summary>
		/// Resolves indexes in a non-random way. Used for extra inputs that the user must select from.
		/// </summary>
		/// <param name="indexCount">The number of indexes available.</param>
		/// <param name="minCount">The minimum number that the user must pick.</param>
		/// <param name="maxCount">The maximum number that the user must pick.</param>
		/// <param name="allowDuplicates">Are duplicates allowed?</param>
		/// <param name="getName">Specifies a callback to get the name of a specific item.</param>
		/// <param name="getAdditionalScopes">
		/// Additional scopes to allow the gaff system to distinguish between decision calls.
		/// Do all string creation inside this func as it will speed up profiling.
		/// </param>
		/// <returns>The list of indexes picked.</returns>
		public IReadOnlyList<ulong> PickIndexes(ulong indexCount, uint minCount, uint maxCount, bool allowDuplicates, Func<ulong, string> getName, Func<IReadOnlyList<string>> getAdditionalScopes = null)
			=> decisionGenerator.PickIndexes(indexCount, minCount, maxCount, allowDuplicates, getName, () => GetContext(getAdditionalScopes));

		/// <summary>
		/// Resolves an index in a non-random way. Used for extra inputs that the user must select from.
		/// </summary>
		/// <param name="indexCount">The number of indexes available.</param>
		/// <param name="getName">Specifies a callback to get the name of a specific item.</param>
		/// <param name="getAdditionalScopes">
		/// Additional scopes to allow the gaff system to distinguish between decision calls.
		/// Do all string creation inside this func as it will speed up profiling.
		/// </param>
		/// <returns>The index picked.</returns>
		public ulong PickOneIndex(ulong indexCount, Func<ulong, string> getName, Func<IReadOnlyList<string>> getAdditionalScopes = null)
			=> decisionGenerator.PickIndexes(indexCount, 1, 1, false, getName, () => GetContext(getAdditionalScopes))[0];

		// ReSharper disable once SuggestBaseTypeForParameter
		private string GetContext(Func<IReadOnlyList<string>> getAdditionalScopes)
		{
			if (getAdditionalScopes == null)
				return Scope;

			var additionalScopes = getAdditionalScopes();

			if (additionalScopes.Count == 0)
				return Scope;

			var sb = new StringBuilder();
			sb.Append(Scope);

			foreach (var scope in additionalScopes)
			{
				if (string.IsNullOrWhiteSpace(scope))
					continue;

				sb.Append('_');
				sb.Append(scope);
			}

			return sb.ToString();
		}

		#region IToString Members

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format) => $"Scope: {Scope}".ToSuccess();

		#endregion
	}
}