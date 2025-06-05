//-----------------------------------------------------------------------
// <copyright file = "Strip.Extensions.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.Linq;

    /// <summary>
    /// Extensions to the generated Strip class.
    /// </summary>
    public partial class Strip
    {
        /// <summary>
        /// The total weight of all of the stops.
        /// </summary>
        public int TotalWeight
        {
            get
            {
                return Stop.Aggregate<StopType, int>(0, (current, stop) => current + stop.weight);
            }
        }

        /// <summary>
        /// Get a stop index based on a weight. Used for converting virtual stops into physical stops.
        /// </summary>
        /// <param name="weight">The weight to find the index for.</param>
        /// <returns>The stop index associated with the weight.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when the desired weight it out of range of the total weight.
        /// </exception>
        public int GetStopIndexForWeight(int weight)
        {
            var currentWeight = 0;
            var stopIndex = 0;
            var found = false;

            for (var currentStopIndex = 0; currentStopIndex < Stop.Count; currentStopIndex++)
            {
                currentWeight += Stop[currentStopIndex].weight;
                if (weight < currentWeight)
                {
                    stopIndex = currentStopIndex;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                throw new IndexOutOfRangeException(
                    "The specified weight is not in range of the total weight of the strip. Weight: " + weight);
            }

            return stopIndex;
        }

        /// <summary>
        /// Get a virtual stop for the given physical stop.
        /// </summary>
        /// <param name="physicalStop">Physical stop to get a virtual stop for.</param>
        /// <returns>First virtual stop for the given physical stop.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when the given physical stop is out of range.
        /// </exception>
        public int GetWeightForStopIndex(int physicalStop)
        {
            if (physicalStop > Stop.Count - 1)
            {
                throw new IndexOutOfRangeException("Physical stop: " + physicalStop + " not in range on strip: " + name);
            }

            var currentWeight = 0;
            for (var stopIndex = 0; stopIndex < physicalStop; stopIndex++)
            {
                currentWeight += Stop[stopIndex].weight;
            }

            return currentWeight;
        }
    }
}
