using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Midas.Core.General;

namespace Midas.Core.ExtensionMethods
{
	public static class ListExtensionMethods
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int FindIndex<T>(this IReadOnlyList<T> list, T valueToFind, EqualityComparer<T> comparer = default)
		{
			comparer ??= EqualityComparer<T>.Default;

			for (var i = 0; i < list.Count; i++)
			{
				if (comparer.Equals(list[i], valueToFind))
					return i;
			}

			return -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int FindIndex<TValue>(this IReadOnlyList<TValue> list, Predicate<TValue> match)
		{
			for (var i = 0; i < list.Count; i++)
			{
				if (match(list[i]))
					return i;
			}

			return -1;
		}

		public static IReadOnlyList<Credit> ToCreditList(this IReadOnlyList<long> values)
		{
			var result = new Credit[values.Count];

			for (var i = 0; i < values.Count; i++)
				result[i] = Credit.FromLong(values[i]);

			return result;
		}

		public static IReadOnlyList<TimeSpan> ToTimeSpanList(this IReadOnlyList<double> values)
		{
			var result = new TimeSpan[values.Count];

			for (var i = 0; i < values.Count; i++)
				result[i] = TimeSpan.FromSeconds(values[i]);

			return result;
		}
	}
}