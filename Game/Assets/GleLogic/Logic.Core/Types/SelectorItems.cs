using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.Utility;

namespace Logic.Core.Types
{
	/// <summary>
	/// Container for a collection of <see cref="SelectorItem"/> objects.
	///
	/// This is primarily required to allow for optimised retrieval of a <see cref="SelectorItem"/> based on requirement values.
	/// </summary>
	public sealed class SelectorItems : IToString
	{
		/// <summary>
		/// Implementation of <see cref="IEqualityComparer{T}"/> for an array of objects.
		///
		/// Note: it is assumed all objects in the array are not null and have valid Equals and GetHashCode implementations.
		/// </summary>
		private sealed class KeyComparer : IEqualityComparer<object[]>
		{
			public static readonly KeyComparer Instance = new KeyComparer();

			public bool Equals(object[] x, object[] y)
			{
				if (x == null || y == null)
					throw new Exception();

				var count = x.Length;

				if (y.Length != count)
					throw new Exception();

				for (var i = 0; i < count; i++)
				{
					if (!Equals(x[i], y[i]))
						return false;
				}

				return true;
			}

			public int GetHashCode(object[] obj)
			{
				var hc = new HashCode();
				for (var i = 0; i < obj.Length; i++)
					hc.Add(obj[i]);

				return hc.ToHashCode();
			}
		}

		/// <summary>
		/// An empty <see cref="SelectorItems"/> object.
		/// </summary>
		public static readonly SelectorItems Empty = new SelectorItems(Array.Empty<SelectorItem>());

		private readonly IReadOnlyList<SelectorItem> items;
		private readonly Dictionary<object[], int> quickLookup = new Dictionary<object[], int>(KeyComparer.Instance);

		/// <summary>
		/// Get the type of the <see cref="SelectorItem.Data"/> property.
		/// All items should have the same Data type.
		/// If there are no items this returns <see cref="object"/>.
		/// </summary>
		public Type DataType => items.Count > 0 ? items[0].Data.GetType() : typeof(object);

		/// <summary>
		/// Get the number of <see cref="SelectorItem"/> objects.
		/// </summary>
		public int Count => items.Count;

		/// <summary>
		/// Get the <see cref="SelectorItem"/> object at the given index.
		/// </summary>
		public SelectorItem this[int index] => items[index];

		// ReSharper disable once MemberCanBePrivate.Global - Used in runner
		public SelectorItems(IReadOnlyList<SelectorItem> items)
		{
			this.items = items;

			// Populate the quick lookup dictionary with all the combinations of VALID requirements.
			for (var i = 0; i < items.Count; i++)
			{
				var requirements = items[i].Requirements;

				if (requirements == null)
					continue;

				var requirementsCount = requirements.Count;
				var lengths = requirements.Select(r => r.Values?.Count ?? 0).ToArray();

				foreach (var valueIndexes in lengths.Enumerate())
				{
					var values = new object[requirementsCount];

					for (var j = 0; j < valueIndexes.Count; j++)
						values[j] = requirements[j].Values[valueIndexes[j]];

					// Ignore duplicates here, only the first gets used. This situation is caught by GLE validation.
					// ReSharper disable once CanSimplifyDictionaryLookupWithTryAdd - Not supported by Unity
					if (!quickLookup.ContainsKey(values))
						quickLookup.Add(values, i);
				}
			}
		}

		/// <summary>
		/// Find the appropriate <see cref="SelectorItem"/> and return its <see cref="SelectorItem.Data"/> value.
		/// </summary>
		/// <param name="args">The <see cref="Requirement"/> values used to find a single <see cref="SelectorItem"/>.</param>
		// ReSharper disable once UnusedMember.Global
		public object Select(params object[] args)
		{
			if (quickLookup.TryGetValue(args, out var index))
				return items[index].Data;

			throw new Exception($"No match found in Select: {string.Join(" ", args.Select(SelectArgToString))}");
		}

		private static string SelectArgToString(object arg)
		{
			if (arg is IToString s && s.ToString("SL") is StringSuccess r)
				return r.Value;

			return arg.ToString();
		}

		public IResult ToString(string format) => StringConverter.ToString(items, format);
	}
}