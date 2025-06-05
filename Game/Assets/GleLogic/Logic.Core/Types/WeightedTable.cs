using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.DecisionGenerator;
using Logic.Core.DecisionGenerator.Decisions;
using Logic.Core.Utility;
using Logic.Core.WinCheck;

namespace Logic.Core.Types
{
	/// <summary>
	/// Class to contain a list of weights mapped to an arbitrary item.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class WeightedTable<T> : IWeights, IToString, IEquivalent
	{
		/// <summary>
		/// This list of weighted items.
		/// </summary>
		public IReadOnlyList<WeightedItem<T>> Items { get; }

		/// <summary>
		/// The total weight, this can be equal to OR greater than the sum of the weights
		/// in the Items collection.
		/// </summary>
		public ulong TotalWeight { get; }

		/// <summary>
		/// Create a WeightedTable.
		/// </summary>
		/// <param name="items">The items to use.</param>
		/// <param name="totalWeight">The total weight, if this is not provided the sum of all the item weights is used.</param>
		/// <exception cref="ArgumentException">If the sum of the item weights is greater than the total weight.</exception>
		public WeightedTable(IReadOnlyList<WeightedItem<T>> items, ulong totalWeight = 0UL)
		{
			Items = items;

			var itemsWeight = items.Aggregate(0UL, (current, item) => current + item.Weight);

			if (totalWeight == 0UL)
			{
				TotalWeight = itemsWeight;
			}
			else
			{
				if (itemsWeight > totalWeight)
					throw new ArgumentException("The weight of the items cannot be greater the total weight.", nameof(totalWeight));

				TotalWeight = totalWeight;
			}
		}

		/// <inheritdoc cref="IToString.ToString(string?)" />
		IResult IToString.ToString(string format)
		{
			if (format == "ML")
			{
				var r = StringConverter.ToString(Items, "ML");

				if (r is StringSuccess s)
				{
					var lines = s.Value.ToLines();
					var ret = new List<string>
					{
						$"TotalWeight: {TotalWeight}",
						$"Items: {lines[0]}"
					};
					ret.AddRange(lines.Skip(1).Select(l => $"       {l}"));

					return ret.Join().ToSuccess();
				}

				return r;
			}

			return new NotSupported();
		}

		/// <inheritdoc cref="IWeights.GetTotalWeight()" />
		ulong IWeights.GetTotalWeight() => TotalWeight;

		/// <inheritdoc cref="IWeights.GetIndexAtWeight(ulong)" />
		ulong IWeights.GetIndexAtWeight(ulong weight)
			=> StripHelper.GetIndexAtWeight(weight, (ulong)Items.Count, ul => Items[(int)ul].Weight);

		/// <inheritdoc cref="IWeights.GetIndexAtWeight(ulong,ICollection{ulong})" />
		ulong IWeights.GetIndexAtWeight(ulong weight, ICollection<ulong> indexesToSkip)
			=> StripHelper.GetIndexAtWeight(weight, (ulong)Items.Count, indexesToSkip, ul => Items[(int)ul].Weight);

		/// <inheritdoc cref="IWeights.GetLength()" />
		ulong IWeights.GetLength() => (ulong)Items.Count;

		/// <inheritdoc cref="IWeights.GetWeight(ulong)" />
		ulong IWeights.GetWeight(ulong index) => Items[(int)index].Weight;

		/// <inheritdoc />
		bool IEquivalent.IsEquivalent(object other)
		{
			if (other is WeightedTable<T> t && Items.Count == t.Items.Count && TotalWeight == t.TotalWeight)
			{
				for (var i = 0; i < Items.Count; i++)
				{
					var item1 = Items[i];
					var item2 = t.Items[i];

					if (item1.Weight != item2.Weight)
						return false;

					var value1 = item1.Value;
					var value2 = item2.Value;

					if (!ReferenceEquals(value1, value2))
					{
						if (value1 is IEquatable<T> e1 && value2 is IEquatable<T> e2)
						{
							if (!e1.Equals(e2))
								return false;
						}
						else
						{
							// If T doesn't implement IEquatable<T> then we have no way to compare, so we just return false.
							return false;
						}
					}
				}

				return true;
			}

			return false;
		}
	}
}