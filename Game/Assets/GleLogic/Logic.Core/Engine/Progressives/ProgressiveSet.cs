using System.Collections.Generic;
using System.Linq;
using Logic.Core.Utility;

namespace Logic.Core.Engine.Progressives
{
	/// <summary>
	/// A list of inputs sets that produce the same set of progressive levels.
	/// </summary>
	public sealed class ProgressiveSet : IToString
	{
		/// <summary>
		/// A list of input sets.
		/// </summary>
		public IReadOnlyList<IReadOnlyList<Input>> InputSets { get; }

		/// <summary>
		/// The set of unique progressive levels associated with these input sets.
		/// </summary>
		public IReadOnlyList<ProgressiveLevel> ProgressiveLevels { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		public ProgressiveSet(IReadOnlyList<IReadOnlyList<Input>> inputSets, IReadOnlyList<ProgressiveLevel> progressiveLevels)
		{
			InputSets = inputSets;
			ProgressiveLevels = progressiveLevels;
		}

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format) => new NotSupported();

		/// <summary>Implementation of IToString.ListToString(object, string?)</summary>
		// ReSharper disable once UnusedMember.Global
		public static IResult ListToString(object list, string format)
		{
			if (format != "ML")
				return new NotSupported();

			if (list is IReadOnlyList<ProgressiveSet> progressiveSets)
			{
				if (progressiveSets.Count == 0 || !progressiveSets.SelectMany(ps => ps.ProgressiveLevels).Any())
					return string.Empty.ToSuccess();

				var tableElements = new List<IReadOnlyList<string>>
				{
					new[] { "Set Id", "Level Name", "Type", "Standalone", "Contribution", "Startup", "Ceiling" }
				};

				for (var i = 0; i < progressiveSets.Count; i++)
				{
					foreach (var level in progressiveSets[i].ProgressiveLevels)
					{
						tableElements.Add(new[]
						{
							level.ProgressiveSetId,
							level.LevelName,
							level.ProgressiveType.ToString(),
							level.IsStandalone.ToString(),
							level.MinContribution.ToString("0.0000"),
							level.MinStartup.ToStringOrThrow("SL"),
							level.Ceiling.ToStringOrThrow("SL")
						});
					}
				}

				return tableElements.ToTableResult();
			}

			return new Error("Expecting be a list of progressive sets");
		}
	}
}