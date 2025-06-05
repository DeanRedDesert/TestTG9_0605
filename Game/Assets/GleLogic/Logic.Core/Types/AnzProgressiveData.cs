using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Logic.Core.Engine;
using Logic.Core.Engine.Progressives;
using Logic.Core.Utility;

namespace Logic.Core.Types
{
	/// <summary>
	/// A set progressive levels for the ANZ region. Ceilings and Startups are defined in currency.
	/// </summary>
	public sealed class AnzProgressiveData : IProgressiveData, IToString
	{
		/// <summary>
		/// ANZ specific progressive level information.
		/// </summary>
		public IReadOnlyList<AnzProgressiveLevel> ProgressiveLevels { get; }

		public AnzProgressiveData(IReadOnlyList<AnzProgressiveLevel> progressiveLevels)
		{
			ProgressiveLevels = progressiveLevels;
		}

		/// <inheritdoc />
		public bool TryGetProgressiveSets(IReadOnlyList<Input> inputSet, out IReadOnlyList<ProgressiveLevel> progressiveLevels, out string errorMsg)
		{
			var progSetId = inputSet.FirstOrDefault(i => i.Name == "ProgSetId")?.Value as string;

			if (progSetId == null && ProgressiveLevels.Count > 0)
			{
				progressiveLevels = null;
				errorMsg = "Progressive levels are defined but there is no 'ProgSetId' in the inputs.";
				return false;
			}

			var newProgressiveLevels = ProgressiveLevels
				.Where(pl => progSetId == pl.ProgressiveSetId)
				.Select(pl => new ProgressiveLevel(pl.ProgressiveSetId, pl.LevelName, pl.LevelName, pl.Startup, pl.Startup, pl.Ceiling, ProgressiveType.Triggered, pl.IsStandalone, pl.Contribution, pl.Contribution, 0.0))
				.ToArray();

			if (progSetId != null && newProgressiveLevels.Length == 0)
			{
				progressiveLevels = null;
				errorMsg = $"No progressive levels found for ProgSetId {progSetId}.";
				return false;
			}

			progressiveLevels = newProgressiveLevels;
			errorMsg = null;
			return true;
		}

		private static readonly string[] headers = { "ProgSetId", "LevelName", "IsStandalone", "Startup", "Contribution", "Ceiling" };

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format)
		{
			var tableElements = new List<IReadOnlyList<string>> { headers };

			foreach (var level in ProgressiveLevels)
			{
				var row = new List<string>
				{
					StringConverter.TryToString(level.ProgressiveSetId, "SL", out var psText) ? psText : "ERR",
					StringConverter.TryToString(level.LevelName, "SL", out var lnText) ? lnText : "ERR",
					level.IsStandalone.ToString(),
					level.Startup.ToStringOrThrow("SL"),
					level.Contribution.ToString("G", CultureInfo.CurrentCulture),
					level.Ceiling.ToStringOrThrow("SL")
				};

				tableElements.Add(row);
			}

			return tableElements.ToTableResult();
		}
	}

	/// <summary>
	/// A progressive level for the ANZ region. Ceilings and Startups are defined in currency.
	/// </summary>
	public sealed class AnzProgressiveLevel
	{
		/// <summary>
		/// The id that ties a set of progressives together.
		/// </summary>
		public string ProgressiveSetId { get; }

		/// <summary>
		/// A friendly name for the progressive level.
		/// </summary>
		public string LevelName { get; }

		/// <summary>
		/// Is this progressive level standalone or linked.
		/// </summary>
		public bool IsStandalone { get; }

		/// <summary>
		/// Contribution value (raw double and not a percentage).
		/// </summary>
		public double Contribution { get; }

		/// <summary>
		/// The maximum start up value.
		/// </summary>
		public Money Startup { get; }

		/// <summary>
		/// The main ceiling of this progressive level.
		/// </summary>
		public Money Ceiling { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		public AnzProgressiveLevel(string progressiveSetId, string levelName, bool isStandalone, double contribution, Money startup, Money ceiling)
		{
			ProgressiveSetId = progressiveSetId;
			LevelName = levelName;
			IsStandalone = isStandalone;
			Contribution = contribution;
			Startup = startup;
			Ceiling = ceiling;
		}
	}
}