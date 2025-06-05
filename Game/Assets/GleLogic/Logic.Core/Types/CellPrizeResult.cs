using System.Collections.Generic;
using System.Linq;
using Logic.Core.Utility;

namespace Logic.Core.Types
{
	/// <summary>
	/// Stores the data for a cell prize.
	/// </summary>
	public sealed class CellPrizeResult : IToString
	{
		/// <summary>
		/// Gets or sets the name of the prize represented by this cell prize result.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets or sets the number of cells involved in this prize.
		/// </summary>
		public int Count { get; }

		/// <summary>
		/// Gets or sets the value of this prize.
		/// </summary>
		public int Value { get; }

		/// <summary>
		/// Gets or sets the pattern where this prize was found.
		/// </summary>
		public Pattern Pattern { get; }

		/// <summary>
		/// Gets or sets the winning cells for this prize.
		/// </summary>
		public IReadOnlyList<Cell> WinningMask { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the prize.</param>
		/// <param name="count">The count of winning pay.</param>
		/// <param name="value">The value of winning pay.</param>
		/// <param name="pattern">The pattern that the prize appeared on.</param>
		/// <param name="winningMask">The winning cells.</param>
		public CellPrizeResult(string name, int count, int value, Pattern pattern, IReadOnlyList<Cell> winningMask)
		{
			Name = name;
			Count = count;
			Value = value;
			Pattern = pattern;
			WinningMask = winningMask;
		}

		/// <summary>
		/// Creates a clone of this object replacing the given value.
		/// If the value is unchanged then this is returned.
		/// </summary>
		/// <param name="value">The new value to use.</param>
		public CellPrizeResult CloneWithValue(int value)
		{
			return value == Value
				? this
				: new CellPrizeResult(Name, Count, value, Pattern, WinningMask);
		}

		#region IToString

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format) => new NotSupported();

		/// <summary>Implementation of IToString.ListToString(object, string?)</summary>
		// ReSharper disable once UnusedMember.Global
		public static IResult ListToString(object list, string format)
		{
			if (format == "ML" && list is IReadOnlyList<CellPrizeResult> l)
			{
				return l.Select(AsString).Join().ToSuccess();

				string AsString(CellPrizeResult r)
				{
					if (r.Pattern == null)
						return $"{r.Count} {r.Name} paying {r.Value}";

					var mask = r.WinningMask == null
						? "WinningMask is null"
						: r.WinningMask.Count == 0
							? "WinningMask is empty"
							: string.Join(" ", r.WinningMask.Select(c => c.ToStringOrThrow("SL")));

					return $"{r.Count} {r.Name} paying {r.Value} on {r.Pattern.Name} ({mask})";
				}
			}

			return new NotSupported();
		}

		#endregion
	}
}