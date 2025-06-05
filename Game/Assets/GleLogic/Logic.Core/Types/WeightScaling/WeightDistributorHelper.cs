using System;
using System.Collections.Generic;
using System.Linq;

namespace Logic.Core.Types.WeightScaling
{
	/// <summary>
	/// A class of helper functions associated with the weight distributor system.
	/// NOTE: It is essential that for each receiver and sender pair the prize for the receiver is equal to transferFactor times the sender prize.
	/// </summary>
	// ReSharper disable once UnusedType.Global
	public static class WeightDistributorHelper
	{
		/// <summary>
		/// Creates a set of weighted items where all scaled adjustments have been fully applied.
		/// </summary>
		/// <param name="prizeTable">The original strip of weighted id entries</param>
		/// <param name="weightDistributorEntries">A list of item id pairs the apply the weight adjustments</param>
		/// <param name="scalingFactors">A lookup table of the actual current scaling factors for each scaling method</param>
		/// <param name="minimumSenderWeight">The minimum weight in a sender entry</param>
		/// <returns>An array of new weights for the strip where scaled adjustments have been fully applied</returns>
		// ReSharper disable once UnusedMember.Global
		public static IReadOnlyList<ulong> CreateScaledWeights(PrizeTable prizeTable, IReadOnlyList<WeightDistributorEntry> weightDistributorEntries, IReadOnlyDictionary<ScalingMethod, ulong> scalingFactors, ulong minimumSenderWeight = 1)
		{
			var extraWeight = CalculateExtraWeight(prizeTable, scalingFactors, out var weights, out var prizeIndexes);
			if (!weightDistributorEntries.Any())
			{
				if (extraWeight > 0)
					throw new Exception("There are no valid receiver and sender entries and an amount of extra weight to be shifted due to scaled entries.");
			}
			else if (extraWeight > 0)
			{
				foreach (var weightDistributorEntry in weightDistributorEntries)
				{
					if (!prizeIndexes.ContainsKey(weightDistributorEntry.ReceiverId) || !prizeIndexes.ContainsKey(weightDistributorEntry.SenderId))
						continue;

					var receiverIndex = prizeIndexes[weightDistributorEntry.ReceiverId];
					var senderIndex = prizeIndexes[weightDistributorEntry.SenderId];
					if (!(weights[(int)senderIndex] > minimumSenderWeight))
						continue;

					CalculateWeightToTake(extraWeight, weights[(int)senderIndex], minimumSenderWeight, weightDistributorEntry.TransferFactor, out var sendWeight, out var receiveWeight, out var adjustedWeight);

					weights[(int)senderIndex] -= sendWeight;
					weights[(int)receiverIndex] += receiveWeight;

					checked
					{
						extraWeight -= adjustedWeight;
					}

					if (extraWeight == 0)
						break;
				}

				if (extraWeight > 0)
					throw new Exception("There is not enough allocated weight to scale correctly.");
			}

			return weights;
		}

		private static ulong CalculateExtraWeight(PrizeTable prizeTable, IReadOnlyDictionary<ScalingMethod, ulong> scalingFactors, out ulong[] weights, out Dictionary<string, ulong> idIndexes)
		{
			weights = new ulong[prizeTable.GetLength()];
			idIndexes = new Dictionary<string, ulong>();
			var extraWeight = 0UL;

			for (var i = 0UL; i < prizeTable.GetLength(); i++)
			{
				idIndexes.Add(prizeTable.GetId(i), i);
				var weight = prizeTable.GetWeight(i);
				var scalingMethod = prizeTable.GetScalingMethod(i);
				var scalingFactor = scalingFactors.TryGetValue(scalingMethod, out var factor) ? factor : 1;
				var scaledWeight = scalingFactor * weight;

				extraWeight += scaledWeight - weight;
				weights[(int)i] = scaledWeight;
			}

			return extraWeight;
		}

		public static void CalculateWeightToTake(ulong extraWeight, ulong poolWeight, ulong minimumSenderWeight, ulong transferFactor, out ulong sendWeight, out ulong receiveWeight, out ulong adjustedWeight)
		{
			var weightShiftFactor = transferFactor - 1;
			var maximumAdjustmentInExtraWeight = extraWeight / weightShiftFactor;
			var availableSenderWeight = poolWeight - minimumSenderWeight;
			var baseValue = availableSenderWeight / transferFactor;
			var maximumAdjustmentInPool = baseValue * weightShiftFactor;
			var adjustmentToUse = maximumAdjustmentInExtraWeight > maximumAdjustmentInPool ? baseValue : maximumAdjustmentInExtraWeight;

			sendWeight = adjustmentToUse * transferFactor;
			receiveWeight = adjustmentToUse;
			adjustedWeight = adjustmentToUse * weightShiftFactor;
		}
	}
}
