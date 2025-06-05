using System;
using System.Collections.Generic;
using Logic.Core.WinCheck;

namespace Logic.Core.DecisionGenerator
{
	/// <summary>
	/// Implements a decision generator.
	/// </summary>
	public sealed class DecisionGenerator : IDecisionGenerator
	{
		#region Fields

		private readonly RandomNumberGenerator rng;

		#endregion

		#region Construction

		/// <summary>
		/// Initialises a new instance of this type.
		/// </summary>
		/// <param name="random">Specifies the random number generator to use with this decision generator.</param>
		public DecisionGenerator(RandomNumberGenerator random)
		{
			rng = random;
		}

		#endregion

		#region Implementation of IDecisionGenerator

		/// <inheritdoc />
		public bool GetDecision(ulong trueWeight, ulong falseWeight, Func<string> getContext)
		{
			ulong totalWeight;

			checked
			{
				totalWeight = trueWeight + falseWeight;
			}

			if (totalWeight > 0x2000000000000)
				throw new ArgumentException($"Total weight cannot exceed 0x2000000000000: trueWeight (0x{trueWeight:X}) + falseWeight (0x{falseWeight:X})");

			if (totalWeight == 0UL)
				throw new ArgumentException($"Total weight cannot be zero: trueWeight (0x{trueWeight:X}) + falseWeight (0x{falseWeight:X})");

			return rng.Next(totalWeight) < trueWeight;
		}

		/// <inheritdoc />
		public IReadOnlyList<ulong> ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
		{
			if (indexCount == 0UL)
				throw new ArgumentException("indexCount must be greater than zero.");

			if (count == 0U)
				throw new ArgumentException("count must be greater than zero.");

			if (!allowDuplicates && count > indexCount)
				throw new ArgumentException("count must be less than or equal to indexCount.");

			if (indexCount > 0x2000000000000)
				throw new ArgumentException("indexCount cannot exceed 0x2000000000000");

			var chosenIndexes = new ulong[count];

			if (allowDuplicates)
			{
				for (var i = 0U; i < count; i++)
					chosenIndexes[i] = rng.Next(indexCount);
			}
			else
			{
				var orderedIndexes = new List<ulong>((int)count);

				for (var i = 0U; i < count; i++)
				{
					var next = rng.Next(indexCount - i);

					// Need to adjust next to account for values that have already been selected.
					var j = 0;
					while (j < orderedIndexes.Count && orderedIndexes[j] <= next)
					{
						next++;
						j++;
					}

					orderedIndexes.Insert(j, next);
					chosenIndexes[i] = next;
				}
			}

			return chosenIndexes;
		}

		public IReadOnlyList<ulong> ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, ulong> getWeight, ulong totalWeight, Func<ulong, string> getName, Func<string> getContext)
		{
			if (indexCount == 0UL)
				throw new ArgumentException("indexCount must be greater than zero.");

			if (totalWeight == 0UL)
				throw new ArgumentException("totalWeight must be greater than zero.");

			if (count == 0U)
				throw new ArgumentException("count must be greater than zero.");

			if (!allowDuplicates && count > indexCount)
				throw new ArgumentException("count must be less than or equal to indexCount.");

			if (totalWeight > 0x2000000000000)
				throw new ArgumentException("totalWeight cannot exceed 0x2000000000000");

			var chosenIndexes = new ulong[count];

			if (allowDuplicates)
			{
				for (var i = 0U; i < count; i++)
					chosenIndexes[i] = StripHelper.GetIndexAtWeight(rng.Next(totalWeight), indexCount, totalWeight, getWeight);
			}
			else
			{
				var usedWeight = 0UL;
				var chosenIndexesSet = new HashSet<ulong>();

				for (var i = 0U; i < count; i++)
				{
					var nextChosenWeight = rng.Next(totalWeight - usedWeight);
					var chosenIndex = StripHelper.GetIndexAtWeight(nextChosenWeight, indexCount, chosenIndexesSet, getWeight);

					usedWeight += getWeight(chosenIndex);
					chosenIndexes[i] = chosenIndex;
					chosenIndexesSet.Add(chosenIndex);
				}
			}

			return chosenIndexes;
		}

		/// <inheritdoc />
		public IReadOnlyList<ulong> ChooseIndexes(IWeights weights, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
		{
			var length = weights.GetLength();

			if (length == 0UL)
				throw new ArgumentException("strip length must be greater than zero.");

			var totalWeight = weights.GetTotalWeight();

			if (totalWeight == 0UL)
				throw new ArgumentException("totalWeight must be greater than zero.");

			if (count == 0U)
				throw new ArgumentException("count must be greater than zero.");

			if (!allowDuplicates && count > length)
				throw new ArgumentException("count must be less than or equal to the strip length.");

			if (totalWeight > 0x2000000000000)
				throw new ArgumentException("totalWeight cannot exceed 0x2000000000000");

			var chosenIndexes = new ulong[count];

			if (allowDuplicates)
			{
				for (var i = 0U; i < count; i++)
					chosenIndexes[i] = weights.GetIndexAtWeight(rng.Next(totalWeight));
			}
			else
			{
				var usedWeight = 0UL;
				var chosenIndexesSet = new HashSet<ulong>();

				for (var i = 0U; i < count; i++)
				{
					var nextChosenWeight = rng.Next(totalWeight - usedWeight);
					var chosenIndex = weights.GetIndexAtWeight(nextChosenWeight, chosenIndexesSet);

					usedWeight += weights.GetWeight(chosenIndex);
					chosenIndexes[i] = chosenIndex;
					chosenIndexesSet.Add(chosenIndex);
				}
			}

			return chosenIndexes;
		}

		public IReadOnlyList<ulong> PickIndexes(ulong indexCount, uint minCount, uint maxCount, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
		{
			if (indexCount == 0UL)
				throw new ArgumentException("indexCount must be greater than zero.");

			if (minCount == 0U)
				throw new ArgumentException("minCount must be greater than zero.");

			if (maxCount < minCount)
				throw new ArgumentException("maxCount must be greater than minCount.");

			if (!allowDuplicates && maxCount > indexCount)
				throw new ArgumentException("count must be less than or equal to indexCount.");

			if (indexCount > 0x2000000000000)
				throw new ArgumentException("indexCount cannot exceed 0x2000000000000");

			var count = rng.Next(maxCount - minCount + 1) + minCount;
			var chosenIndexes = new ulong[count];

			if (allowDuplicates)
			{
				for (var i = 0U; i < count; i++)
					chosenIndexes[i] = rng.Next(indexCount);
			}
			else
			{
				var orderedIndexes = new List<ulong>((int)count);

				for (var i = 0U; i < count; i++)
				{
					var next = rng.Next(indexCount - i);

					// Need to adjust next to account for values that have already been selected.
					var j = 0;
					while (j < orderedIndexes.Count && orderedIndexes[j] <= next)
					{
						next++;
						j++;
					}

					orderedIndexes.Insert(j, next);
					chosenIndexes[i] = next;
				}
			}

			return chosenIndexes;
		}

		#endregion
	}
}