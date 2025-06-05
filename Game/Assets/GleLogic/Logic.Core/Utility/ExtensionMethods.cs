using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Logic.Core.Engine;
using Logic.Core.Types;

namespace Logic.Core.Utility
{
	/// <summary>
	/// This class is for all the extension methods to be defined in Igt.Ugp.Logic.Core.
	/// </summary>
	public static class ExtensionMethods
	{
		/// <summary>
		/// Compares two strings with Ordinal comparison.
		/// </summary>
		public static bool IsSame(this string a, string b, bool ignoreCase = true)
			=> string.Compare(a, b, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0;

		/// <summary>
		/// Find the first index of an item in a list. Or returns -1 if item is not in the list.
		/// </summary>
		/// <typeparam name="T">Type of each element.</typeparam>
		/// <param name="list">The <see cref="IReadOnlyList{T}"/> to search.</param>
		/// <param name="item">The items to find.</param>
		public static int IndexOf<T>(this IReadOnlyList<T> list, T item)
		{
			if (list == null)
				return -1;

			for (var i = 0; i < list.Count; i++)
			{
				if (Equals(list[i], item))
					return i;
			}

			return -1;
		}

		/// <summary>
		/// Find the first index of an item in a list. Or returns -1 if item is not in the list.
		/// </summary>
		/// <typeparam name="T">Type of each element.</typeparam>
		/// <param name="list">The <see cref="IReadOnlyList{T}"/> to search.</param>
		/// <param name="predicate">A function to test if the item is the one we are looking for.</param>
		public static int IndexOf<T>(this IReadOnlyList<T> list, Func<T, bool> predicate)
		{
			if (list == null)
				return -1;

			for (var i = 0; i < list.Count; i++)
			{
				if (predicate(list[i]))
					return i;
			}

			return -1;
		}

		/// <summary>
		/// Converts a list of long values into a range string (e.g. '1 2 4-5 8-10')
		/// Duplicate values in the long values are removed and the order
		/// of the long values is irrelevant.
		/// </summary>
		/// <param name="longs">The list of long values.</param>
		public static string ToRangeString(this IReadOnlyList<long> longs)
		{
			if (longs.Count == 0)
				return string.Empty;

			var sb = new StringBuilder();
			var groups = longs
				.Distinct()
				.OrderBy(l => l)
				.Select((value, i) => (Value: value, Index: i))
				.GroupBy(t => t.Index - t.Value)
				.ToArray();

			for (var i = 0; i < groups.Length; i++)
			{
				var group = groups[i];
				sb.Append(group.First().Value);

				if (group.Count() > 1)
				{
					sb.Append('-');
					sb.Append(group.Last().Value);
				}

				if (i < groups.Length - 1)
					sb.Append(' ');
			}

			return sb.ToString();
		}

		/// <summary>
		/// Converts a list of Credit values into a range string (e.g. '1cr 2cr 4-5cr 8-10cr')
		/// Duplicate values are removed and the order of the credit values is irrelevant.
		/// </summary>
		/// <param name="credits">The list of credit values.</param>
		public static string ToRangeString(this IReadOnlyList<Credits> credits)
		{
			if (credits.Count == 0)
				return string.Empty;

			var sb = new StringBuilder();
			var groups = credits
				.Distinct()
				.OrderBy(l => l)
				.Select((value, i) => (Value: value, Index: (ulong)i))
				.GroupBy(t => t.Index - t.Value.ToUInt64())
				.ToArray();

			for (var i = 0; i < groups.Length; i++)
			{
				var group = groups[i];
				sb.Append(group.First().Value.ToUInt64());

				if (group.Count() > 1)
				{
					sb.Append('-');
					sb.Append(group.Last().Value.ToUInt64());
					sb.Append("cr");
				}
				else
					sb.Append("cr");

				if (i < groups.Length - 1)
					sb.Append(' ');
			}

			return sb.ToString();
		}

		/// <summary>
		/// Converts a range string (e.g. '1 2 4-5 8-10') to a list of long values.
		/// Duplicate values in the long values are removed and the order
		/// of the long values is irrelevant.
		/// </summary>
		/// <param name="rangeString">The range string to convert.</param>
		public static IReadOnlyList<long> ToRangeLongs(this string rangeString)
		{
			if (string.IsNullOrWhiteSpace(rangeString))
				return Array.Empty<long>();

			var longs = new HashSet<long>();
			var trimmed = rangeString.Trim();
			var tokens = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var token in tokens)
			{
				var range = token.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

				switch (range.Length)
				{
					case 1:
						longs.Add(long.Parse(range[0]));
						break;
					case 2:
					{
						var start = long.Parse(range[0]);
						var end = long.Parse(range[1]);

						for (var i = start; i <= end; i++)
							longs.Add(i);
						break;
					}
					default: throw new Exception($"Invalid range format '{rangeString}'");
				}
			}

			return longs.OrderBy(l => l).ToArray();
		}

		/// <summary>
		/// Converts a list of indexes into a single index into and enumeration of lengths (see <see cref="Enumerate"/>).
		/// E.g.
		///		Lengths = [3, 2, 2]
		///		Indexes = [1, 1, 0]
		///		Returns: 6
		/// 
		///			0:  [0, 0, 0]
		///			1:  [0, 0, 1]
		///			2:  [0, 1, 0]
		///			3:  [0, 1, 1]
		///			4:  [1, 0, 0]
		///			5:  [1, 0, 1]
		///			6:  [1, 1, 0]
		///			7:  [1, 1, 1]
		///			8:  [2, 0, 0]
		///			9:  [2, 0, 1]
		///			10: [2, 1, 0]
		///			10: [2, 1, 1]
		/// </summary>
		public static int ToIndex(this IReadOnlyList<int> lengths, IReadOnlyList<int> indexes)
		{
			var mult = 1;
			var index = 0;

			for (var i = lengths.Count - 1; i >= 0; i--)
			{
				index += mult * indexes[i];
				mult *= lengths[i];
			}

			return index;
		}

		/// <summary>
		/// Enumerates all combinations of arrays of the given lengths.
		/// Will return the lists of indexes into the source arrays.
		/// E.g.
		///		Lengths = [3, 2, 2]
		///		Returns:
		///			[0, 0, 0]
		///			[0, 0, 1]
		///			[0, 1, 0]
		///			[0, 1, 1]
		///			[1, 0, 0]
		///			[1, 0, 1]
		///			[1, 1, 0]
		///			[1, 1, 1]
		///			[2, 0, 0]
		///			[2, 0, 1]
		///			[2, 1, 0]
		///			[2, 1, 1]
		/// </summary>
		/// <remarks>
		/// Do not store the results of this enumeration. The returned collection is reused every iteration to
		/// save on memory allocations and thus will change unexpectedly.
		/// </remarks>
		public static IEnumerable<IReadOnlyList<int>> Enumerate(this IReadOnlyList<int> lengths)
		{
			if (lengths.Count == 0 || lengths.Any(l => l == 0))
				yield break;

			var curr = new int[lengths.Count];
			var finished = false;

			while (!finished)
			{
				// Yield
				yield return curr;

				// Increment
				for (var c = lengths.Count - 1; c >= 0; c--)
				{
					curr[c]++;

					if (curr[c] == lengths[c])
					{
						curr[c] = 0;

						if (c != 0)
							continue;

						finished = true;
					}

					break;
				}
			}
		}

		/// <summary>
		/// Converts a type to a user friendly display name.
		/// </summary>
		public static string ToDisplayString(this Type type)
		{
			if (type == null) throw new NotSupportedException();
			if (type == typeof(object)) return "object";
			if (type == typeof(string)) return "string";
			if (type == typeof(bool)) return "bool";
			if (type == typeof(int)) return "int";
			if (type == typeof(uint)) return "uint";
			if (type == typeof(long)) return "long";
			if (type == typeof(ulong)) return "ulong";
			if (type == typeof(float)) return "float";
			if (type == typeof(double)) return "double";
			if (type == typeof(decimal)) return "decimal";

			if (type.IsArray)
				return $"{ToDisplayString(type.GetElementType())}[]";

			if (type.IsGenericType)
			{
				var args = type.GetGenericArguments();

				switch (args.Length)
				{
					case 1:
					{
						// Special case for lists
						var roList = typeof(IReadOnlyList<>).MakeGenericType(args);

						if (roList.IsAssignableFrom(type))
							return $"{ToDisplayString(args[0])}[]";

						// Special case for Nullable<T>
						if (args[0].IsValueType)
						{
							var nullable = typeof(Nullable<>).MakeGenericType(args);

							if (nullable.IsAssignableFrom(type))
								return $"{ToDisplayString(args[0])}?";
						}

						break;
					}
					case 2:
					{
						// Special case for dictionaries
						var roDict = typeof(IReadOnlyDictionary<,>).MakeGenericType(args);

						if (roDict.IsAssignableFrom(type))
							return $"Dictionary<{string.Join(", ", args.Select(ToDisplayString))}>";
						break;
					}
				}

				if (typeof(ITuple).IsAssignableFrom(type))
				{
					if (type.FullName?.StartsWith("System.Tuple`") ?? false)
						return $"[{string.Join(", ", args.Select(ToDisplayString))}]";

					if (type.FullName?.StartsWith("System.ValueTuple`") ?? false)
						return $"({string.Join(", ", args.Select(ToDisplayString))})";
				}

				// Handle all other generic types
				var i = type.Name.IndexOf('`');
				return $"{type.Name.Substring(0, i)}<{string.Join(", ", args.Select(ToDisplayString))}>";
			}

			return type.Name;
		}

		/// <summary>
		/// Get the default value of the ParameterInfo if it exists.
		/// </summary>
		public static bool TryCreateDefault(this ParameterInfo parameter, out object value)
		{
			value = null;

			if (parameter.IsOptional)
			{
				if ((parameter.Attributes & ParameterAttributes.HasDefault) == ParameterAttributes.HasDefault)
					value = parameter.DefaultValue;
				else if (parameter.ParameterType.IsValueType)
					value = Activator.CreateInstance(parameter.ParameterType);

				return true;
			}

			return false;
		}

		/// <summary>
		/// Returns true if the type is Array, List[T], IReadOnlyList[T] or ImmutableArray[T] (.Net 6 only)
		/// Also emits the item type of the list.
		/// </summary>
		public static bool IsSupportedList(this Type type, [MaybeNullWhen(false)] out Type itemType)
		{
			var isArray = type.IsArray;
			var isList = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
			var isIReadOnlyList = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>);
			var implementsIReadOnlyList = type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyList<>));

			if (!isArray && !isList && !isIReadOnlyList && !implementsIReadOnlyList)
			{
				itemType = null;
				return false;
			}

			itemType = isArray
				? type.GetElementType()
				: implementsIReadOnlyList
					? type.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)).GenericTypeArguments[0]
					: type.GenericTypeArguments[0];
			return true;
		}

		/// <summary>
		/// Returns true if the type is Dictionary[T] or IReadOnlyDictionary[T]
		/// Also emits the key type and value type of the dictionary.
		/// </summary>
		public static bool IsSupportedDict(this Type type, [MaybeNullWhen(false)] out Type keyType, [MaybeNullWhen(false)] out Type valueType)
		{
			var isDict = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
			var isIReadOnlyDict = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>);

			keyType = isDict || isIReadOnlyDict ? type.GenericTypeArguments[0] : null;
			valueType = isDict || isIReadOnlyDict ? type.GenericTypeArguments[1] : null;
			return isDict || isIReadOnlyDict;
		}

		/// <summary>
		/// Tests if the sourceType can be cast to the destination type.
		/// </summary>
		/// <returns>False if the data cannot be cast to the expected type.</returns>
		public static bool CanCastTo(this Type sourceType, Type destinationType)
		{
			// A hack to enable connections when we don't know the processor output type.
			if (sourceType == typeof(object))
				return true;

			if (destinationType.IsAssignableFrom(sourceType))
				return true;

			var srcIsInteger = IsInteger(sourceType);
			var dstIsInteger = IsInteger(destinationType);

			return srcIsInteger && dstIsInteger
				|| sourceType == typeof(Credits) && dstIsInteger
				|| sourceType == typeof(Money) && dstIsInteger
				|| srcIsInteger && destinationType == typeof(Credits)
				|| srcIsInteger && destinationType == typeof(Money);

			bool IsInteger(Type t) => t == typeof(int) || t == typeof(uint) || t == typeof(long) || t == typeof(ulong);
		}

		/// <summary>
		/// Indents each line of the string with a tab for each level.
		/// Empty lines are not indented.
		/// </summary>
		public static string Indent(this string stringToIndent, int levels = 1, char indentChar = '\t')
			=> stringToIndent.ToLines(false, false).Indent(levels, indentChar).Join();

		/// <summary>
		/// Indents each string in array with one indentChar for each level.
		/// Empty lines are not indented.
		/// </summary>
		public static IReadOnlyList<string> Indent(this IReadOnlyList<string> linesToIndent, int levels = 1, char indentChar = '\t')
		{
			var indent = new string(Enumerable.Repeat(indentChar, levels).ToArray());
			return linesToIndent.Select(s => string.IsNullOrEmpty(s) ? s : $"{indent}{s}").ToArray();
		}

		/// <summary>
		/// Split a string into an array of lines.
		/// Uses both '\r\n' and '\n' as the newline character.
		/// If trimLines and removeEmpty lines are both true then the trim will be applied first.
		/// </summary>
		/// <param name="str">The string to split</param>
		/// <param name="removeEmptyLines">Remove any lines that are empty</param>
		/// <param name="trimLines">Remove whitespace from the start and end of lines</param>
		public static IReadOnlyList<string> ToLines(this string str, bool removeEmptyLines = true, bool trimLines = true)
		{
			if (str == null)
				return Array.Empty<string>();

			// .Net 4.8 doesn't support StringSplitOptions.TrimEntries so we implement this the manual way.
			var lines = str.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

			if (trimLines)
				lines = lines.Select(l => l.Trim()).ToArray();

			if (removeEmptyLines)
				lines = lines.Where(l => !string.IsNullOrEmpty(l)).ToArray();

			return lines;
		}

		/// <summary>
		/// Join the lines using the given delimiter, if the delimiter is null it will use <see cref="Environment.NewLine"/>
		/// </summary>
		public static string Join(this IEnumerable<string> lines, string delimiter = null)
			=> string.Join(delimiter ?? Environment.NewLine, lines);

		/// <summary>
		/// Gets all the complete or pending cycle states.
		/// </summary>
		public static IReadOnlyList<CycleState> GetCompleteOrPendingCycles(this Cycles cycles)
		{
			var result = new List<CycleState>();
			var inPendingCycles = false;

			for (var i = 0; i < cycles.Count; i++)
			{
				var c = cycles[i];
				if (!inPendingCycles && c == cycles.Current)
					inPendingCycles = true;

				if (inPendingCycles || c.IsFinished)
					result.Add(c);
			}

			return result;
		}
	}

	/// <summary>
	/// Place this attribute on parameters of your processor methods if (and only if) you want to
	/// declare that the parameter can have null passed in. The default is to assume that null is not supported.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public sealed class NullInAttribute : Attribute
	{
	}

	/// <summary>
	/// Place this attribute on the return of processor methods e.g. [return: NullOut] OR on properties of returned objects e.g. [NullOut].
	/// Doing this will declare that the method or property may return (or be) null.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
	public sealed class NullOutAttribute : Attribute
	{
	}

#if !NET6_0_OR_GREATER
	[AttributeUsage(AttributeTargets.Parameter)]
	public sealed class MaybeNullWhenAttribute : Attribute
	{
		public bool ReturnValue { get; }
		public MaybeNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;
	}
#endif
}