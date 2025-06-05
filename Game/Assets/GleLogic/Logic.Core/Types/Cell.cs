using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.Utility;

namespace Logic.Core.Types
{
	/// <summary>
	/// A Cell is the lowest common denominator of a the cell based win checking.
	/// It represents a specific location inside an 2D array of cells.
	/// </summary>
	public sealed class Cell : IToString, IFromString, IToCode
	{
		private IResult cellAsString;
		private int? hash;

		/// <summary>
		/// The column of the cell.
		/// </summary>
		public int Column { get; }

		/// <summary>
		/// The row of the cell.
		/// </summary>
		public int Row { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="column">The column of the cell.</param>
		/// <param name="row">The row of the cell.</param>
		public Cell(int column, int row)
		{
			Column = column;
			Row = row;
		}

		// ReSharper disable once UnusedMember.Global
		public static IReadOnlyList<Cell> CreateList(string str)
		{
			if (StringConverter.TryFromString(str, "SL", typeof(IReadOnlyList<Cell>), out var cells))
				return (IReadOnlyList<Cell>)cells;

			throw new Exception($"Cannot convert to list of cells: {str}");
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (obj is Cell other)
			{
				if (ReferenceEquals(this, other))
					return true;

				return Row == other.Row && Column == other.Column;
			}

			return false;
		}

		/// <inheritdoc />
		// ReSharper disable NonReadonlyMemberInGetHashCode - properties are not set till after construction
		public override int GetHashCode()
		{
			if (!hash.HasValue)
				hash = (Column << 8) ^ Row;

			return hash.Value;
		}
		// ReSharper enable NonReadonlyMemberInGetHashCode

		#region IToString Members

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format)
		{
			return cellAsString ?? (cellAsString = $"C{Column}R{Row}".ToSuccess());
		}

		/// <summary>Implementation of IToString.ListToString(object, string?)</summary>
		// ReSharper disable once UnusedMember.Global
		public static IResult ListToString(object list, string format)
		{
			if (format == "ML" && list is IReadOnlyList<Cell> cells)
				return cells
					.ToStringArrays(new[] { "Column", "Row" }, c => new[] { c.Column.ToString(), c.Row.ToString() })
					.ToTableResult();

			return new NotSupported();
		}

		#endregion

		#region IFromString Members

		/// <summary>Implementation of IFromString.FromString(string?, string?)</summary>
		// ReSharper disable once UnusedMember.Global
		public static IResult FromString(string s, string format)
		{
			var sp = s.Split('C', 'R');

			// .Net 4.8 doesn't support int.TryParse(string, CultureInfo, out int)
			if (sp.Length == 3 &&
				int.TryParse(sp[1], out var c) &&
				int.TryParse(sp[2], out var r))
				return new Cell(c, r).ToSuccess();

			return new Error($"Could not convert '{s}' to a Cell value");
		}

		#endregion

		#region IToCode Members

		/// <inheritdoc cref="IToCode.ToCode(CodeGenArgs?)" />
		public IResult ToCode(CodeGenArgs args) => new NotSupported();

		/// <summary>Implementation of IToCode.ListToCode(CodeGenArgs, object)</summary>
		// ReSharper disable once UnusedMember.Global
		public static IResult ListToCode(CodeGenArgs args, object list)
		{
			return StringConverter.TryToString(list, "SL", out var s)
				? $"{CodeConverter.ToCode<Cell>(args)}.{nameof(CreateList)}(\"{s}\")".ToSuccess()
				: new Error(s);
		}

		#endregion
	}
}