using System.Collections.Generic;
using System;
using System.Linq;
using Logic.Core.Types;

namespace Logic.Core.Utility
{
	public static class ConvertExtensions
	{
		/// <summary>
		/// Wraps a string in a <see cref="StringSuccess"/> result.
		/// </summary>
		public static IResult ToSuccess(this string str) => new StringSuccess(str);

		/// <summary>
		/// Wraps an object in an <see cref="ObjectSuccess"/> result.
		/// </summary>
		public static IResult ToSuccess(this object obj) => new ObjectSuccess(obj);

		/// <summary>
		/// Converts an object implementing IToString to a string.
		/// Throws an exception if there is an error or the conversion is not supported.
		/// </summary>
		public static string ToStringOrThrow(this IToString f, string format)
		{
			if (f == null)
				return "null";

			switch (f.ToString(format))
			{
				case StringSuccess s: return s.Value;
				case Error e: throw new Exception($"Error: {e.Description}");
				case NotSupported _: throw new Exception($"Not Supported: {f.GetType().ToDisplayString()}");
				default: throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Converts a string to an object implementing IFromString.
		/// Throws an exception if there is an error or the conversion is not supported.
		/// </summary>
		public static T FromStringOrThrow<T>(this string str, string format)
		{
			var r = StringConverter.FromString(str, format, typeof(T));

			switch (r)
			{
				case ObjectSuccess s: return (T)s.Value;
				case Error e: throw new Exception($"Error: {e.Description}");
				case NotSupported _: throw new Exception($"Not Supported: {typeof(T).ToDisplayString()}");
				default: throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Returns true result is a success or an error, returns false if it is not supported.
		/// </summary>
		public static bool IsSuccessOrError(this IResult result)
		{
			switch (result)
			{
				case StringSuccess _: return true;
				case ObjectSuccess _: return true;
				case Error _: return true;
				case NotSupported _: return false;
				default: throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Takes a jagged array of tokens and converts it to an IResult/StringResult table with appropriate padding and column separators.
		/// </summary>
		public static IResult ToTableResult(this IReadOnlyList<IReadOnlyList<string>> tokens, string separator = "|")
			=> ToTableLines(tokens, separator).Join().ToSuccess();

		/// <summary>
		/// Takes a jagged array of indexes and converts it to an IResult/StringResult table with appropriate padding and column separators.
		/// The contents of the table are resolved with the <see cref="indexToString"/> method.
		/// </summary>
		public static IResult ToTableResult(this IReadOnlyList<IReadOnlyList<int>> indexes, Func<int, string> indexToString, string separator = "")
			=> ToTableLines(indexes, indexToString, separator).Join().ToSuccess();

		/// <summary>
		/// Takes a jagged array of tokens and converts it to a multiline table with appropriate padding and column separators.
		/// </summary>
		public static IReadOnlyList<string> ToTableLines(this IReadOnlyList<IReadOnlyList<string>> tokens, string separator = "|", bool[] padLeft = null)
		{
			var widths = new int[tokens.Max(t => t.Count)];

			foreach (var line in tokens)
			{
				for (var i = 0; i < line.Count; i++)
					widths[i] = Math.Max(widths[i], line[i]?.Length ?? 0);
			}

			var lines = new string[tokens.Count];

			for (var col = 0; col < widths.Length; col++)
			{
				var left = padLeft != null && col < padLeft.Length && padLeft[col];

				for (var row = 0; row < tokens.Count; row++)
				{
					if (left)
						lines[row] += $"{Sep(col)}{(col < tokens[row].Count ? tokens[row][col] ?? "" : "").PadLeft(widths[col])}";
					else
						lines[row] += $"{Sep(col)}{(col < tokens[row].Count ? tokens[row][col] ?? "" : "").PadRight(widths[col])}";
				}
			}

			return lines;

			string Sep(int col) => col == 0
				? ""
				: separator == ""
					? " "
					: $" {separator} ";
		}

		/// <summary>
		/// Takes a jagged array of indexes and converts it to a multiline table with appropriate padding and column separators.
		/// The contents of the table are resolved with the <see cref="indexToString"/> method.
		/// </summary>
		public static IReadOnlyList<string> ToTableLines(this IReadOnlyList<IReadOnlyList<int>> indexes, Func<int, string> indexToString, string separator = "")
		{
			var widths = new int[indexes.Max(t => t.Count)];

			foreach (var line in indexes)
			{
				for (var i = 0; i < line.Count; i++)
					widths[i] = Math.Max(widths[i], indexToString(line[i]).Length);
			}

			var lines = new string[indexes.Count];

			for (var col = 0; col < widths.Length; col++)
			{
				for (var row = 0; row < indexes.Count; row++)
					lines[row] += $"{Sep(col)}{(col < indexes[row].Count ? indexToString(indexes[row][col]) : "").PadRight(widths[col])}";
			}

			return lines;

			string Sep(int col) => col == 0
				? ""
				: separator == ""
					? " "
					: $" {separator} ";
		}

		/// <summary>
		/// Converts a connection of items into a 2d collection of tokens that can be converted to a table.
		/// </summary>
		/// <typeparam name="T">The type of the collection items</typeparam>
		/// <param name="items">The list of items</param>
		/// <param name="header">The header tokens to use for the first row of the table</param>
		/// <param name="rowGenerator">A func that will turn an item into a list of tokens</param>
		public static IReadOnlyList<IReadOnlyList<string>> ToStringArrays<T>(this IReadOnlyCollection<T> items, IReadOnlyList<string> header, Func<T, IReadOnlyList<string>> rowGenerator)
		{
			var arrays = new IReadOnlyList<string>[items.Count + 1];
			var row = 0;
			arrays[row] = header;

			foreach (var i in items)
			{
				row++;
				arrays[row] = rowGenerator(i);
			}

			return arrays;
		}

		/// <summary>
		/// Converts a <see cref="SymbolWindowStructure"/> into a jagged array of cell indexes
		/// representing the row/column structure.
		/// </summary>
		public static int[][] GetStructureArrays(this SymbolWindowStructure sws)
		{
			var maxCol = 0;
			var maxRow = 0;

			foreach (var cell in sws.Cells)
			{
				maxCol = Math.Max(maxCol, cell.Column);
				maxRow = Math.Max(maxRow, cell.Row);
			}

			var res = new int[maxRow + 1][];

			for (var r = 0; r < maxRow + 1; r++)
			{
				var cols = maxCol + 1;
				res[r] = new int[cols];

				for (var c = 0; c < cols; c++)
					res[r][c] = sws.GetCellIndexOrDefault(new Cell(c, r));
			}

			return res;
		}
	}
}