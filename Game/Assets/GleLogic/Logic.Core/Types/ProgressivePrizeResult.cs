using System.Collections.Generic;
using System.Linq;
using Logic.Core.Utility;

namespace Logic.Core.Types
{
	/// <summary>
	/// Stores the data for a progressive prize.
	/// </summary>
	public sealed class ProgressivePrizeResult : IToString
	{
		/// <summary>
		/// Gets the name of the prize represented by this progressive prize result.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets the number of cells involved in this prize.
		/// </summary>
		public int Count { get; }

		/// <summary>
		/// Gets the name of the associated progressive.
		/// </summary>
		// ReSharper disable once UnusedAutoPropertyAccessor.Global - Used in presentation
		public string Progressive { get; }

		/// <summary>
		/// Gets the pattern where this prize was found.
		/// </summary>
		public Pattern Pattern { get; }

		/// <summary>
		/// Gets the winning cells for this prize.
		/// </summary>
		public IReadOnlyList<Cell> WinningMask { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the prize.</param>
		/// <param name="count">The count of winning pay.</param>
		/// <param name="progressive">The name of the winning progressive.</param>
		/// <param name="pattern">The pattern that the prize appeared on.</param>
		/// <param name="winningMask">The winning cells.</param>
		public ProgressivePrizeResult(string name, int count, string progressive, Pattern pattern, IReadOnlyList<Cell> winningMask)
		{
			Name = name;
			Count = count;
			Progressive = progressive;
			Pattern = pattern;
			WinningMask = winningMask;
		}

		#region IToString

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format) => new NotSupported();

		/// <summary>Implementation of IToString.ListToString(object, string?)</summary>
		// ReSharper disable once UnusedMember.Global
		public static IResult ListToString(object list, string format)
		{
			if (format == "ML" && list is IReadOnlyList<ProgressivePrizeResult> l)
			{
				return l.Select(AsString).Join().ToSuccess();

				string AsString(ProgressivePrizeResult r)
				{
					if (r.Pattern == null)
						return $"{r.Count} {r.Name} triggering {r.Progressive}";

					var mask = r.WinningMask == null
						? "WinningMask is null"
						: r.WinningMask.Count == 0
							? "WinningMask is empty"
							: string.Join(" ", r.WinningMask.Select(c => c.ToStringOrThrow("SL")));

					return $"{r.Count} {r.Name} triggering {r.Progressive} on {r.Pattern.Name} ({mask})";
				}
			}

			return new NotSupported();
		}

		#endregion
	}
}