using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Midas.Core.ExtensionMethods
{
	public static class DictionaryExtensionMethods
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TValue? GetValueOrNull<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key) where TValue : struct
		{
			if (dictionary.TryGetValue(key, out var value))
				return value;
			return null;
		}
	}
}