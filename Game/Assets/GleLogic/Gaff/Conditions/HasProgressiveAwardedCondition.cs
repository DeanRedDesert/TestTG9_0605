using System.Collections.Generic;
using System.Linq;
using Gaff.Core.Conditions;
using Gaff.Core.GaffEditor;
using Logic.Core.Engine;
using Logic.Core.Utility;

// ReSharper disable MemberCanBePrivate.Global

namespace Gaff.Conditions
{
	/// <summary>
	/// Check if a progressive named <see cref="ProgressiveName"/> has been awarded.  If no name is specified then check for any progressive awarded.
	/// </summary>
	public sealed class HasProgressiveAwardedCondition : ResultCondition
	{
		/// <summary>
		/// The name of the progressive to check for.  If null or empty then condition is met if any progressive is found.
		/// </summary>
		public string ProgressiveName { get; }

		public HasProgressiveAwardedCondition(string progressiveName)
		{
			ProgressiveName = progressiveName;
		}

		/// <inheritdoc />
		public override bool CheckCondition(CycleResult result, CycleResult initialResultForStep, IReadOnlyList<StageGaffResult> sequenceUpToNow)
		{
			return string.IsNullOrEmpty(ProgressiveName) ? result.Progressives.Any() : result.Progressives.Contains(ProgressiveName);
		}

		/// <inheritdoc />
		public override IResult ToString(string format)
		{
			return string.IsNullOrEmpty(ProgressiveName)
				? "The result has triggered a progressive".ToSuccess()
				: $"The result has triggered the {ProgressiveName} progressive".ToSuccess();
		}
	}
}