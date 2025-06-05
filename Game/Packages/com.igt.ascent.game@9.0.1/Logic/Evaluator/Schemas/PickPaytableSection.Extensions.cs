//-----------------------------------------------------------------------
// <copyright file = "PickPaytableSection.Extensions.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Extensions to the schema generated <seealso cref="PickPaytableSection"/> class.
    /// </summary>
    public partial class PickPaytableSection
    {
        
        /// <summary>
        /// Get pick based on the given weight. When given a number between 0 and the sum of all pick weights, this
        /// function returns the pick in which that weight falls. So if there are 3 picks with weights of 10, 15, and
        /// 20 a passed weight of 0-9 would return the first pick, 10-24 would return the second pick, and 25-44 would
        /// return the third pick.
        /// </summary>
        /// <param name="weight">Weight to select the pick by.</param>
        /// <returns>The pick for the given weight.</returns>
        public Pick GetPickByWeight(uint weight)
        {
            return Pick.GetPickByWeight(weight);
        }

        /// <summary>
        /// Get the pick with the given name.
        /// </summary>
        /// <param name="requestedName">Name of the pick to get.</param>
        /// <returns>The pick with the given name.</returns>
        public Pick GetPickByName(string requestedName)
        {
            return Pick.GetPickByName(requestedName);
        }

        /// <summary>
        /// The total weight of all of the picks.
        /// </summary>
        public uint TotalWeight
        {
            get
            {
                return Pick.GetTotalWeight();
            }
        }

        /// <summary>
        /// Get a weight which will result in the specified pick when passed to GetPickByWeight.
        /// </summary>
        /// <param name="pickName">The name of the pick to get the weight for.</param>
        /// <returns>A weight which will result in the specified pick.</returns>
        public uint GetWeightForPick(string pickName)
        {
            return Pick.GetWeightForPick(pickName);
        }
    }

    /// <summary>
    /// Extensions to the type <seealso cref="Pick"/> and <seealso cref="IPick"/>.
    /// </summary>
    public static class PickListExtensions
    {
        /// <summary>
        /// Extend a list of picks with the contents of another one.
        /// </summary>
        /// <param name="picks">Pick list to extend.</param>
        /// <param name="picksToAdd">List of picks to add to the other list.</param>
        /// <remarks>Cannot be IList extensions as IList does not have AddRange.</remarks>
        public static void AddRangeCopy(this List<Pick> picks, IEnumerable<Pick> picksToAdd)
        {
            picks.AddRange(picksToAdd.Select(pick => new Pick(pick)));
        }

        /// <summary>
        /// Get pick based on the given weight. When given a number between 0 and the sum of all pick weights, this
        /// function returns the pick in which that weight falls. So if there are 3 picks with weights of 10, 15, and
        /// 20 a passed weight of 0-9 would return the first pick, 10-24 would return the second pick, and 25-44 would
        /// return the third pick.
        /// </summary>
        /// <param name="picks">List of picks to find the pick in.</param>
        /// <param name="weight">Weight to select the pick by.</param>
        /// <returns>The pick for the given weight.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when the desired weight it out of range of the total weight.
        /// </exception>
        public static Pick GetPickByWeight(this IList<Pick> picks, uint weight)
        {
            return picks.Select(pick => new PickAdapter(pick)).GetPickByWeight(weight).PickObject;
        }

        /// <summary>
        /// Get pick based on the given weight. When given a number between 0 and the sum of all pick weights, this
        /// function returns the pick in which that weight falls. So if there are 3 picks with weights of 10, 15, and
        /// 20 a passed weight of 0-9 would return the first pick, 10-24 would return the second pick, and 25-44 would
        /// return the third pick.
        /// </summary>
        /// <param name="picks">List of picks to find the pick in.</param>
        /// <param name="weight">Weight to select the pick by.</param>
        /// <returns>The pick for the given weight.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when the desired weight it out of range of the total weight.
        /// </exception>
        public static T GetPickByWeight<T>(this IEnumerable<T> picks, uint weight) where T : IPick
        {
            var pickList = picks as IList<T> ?? picks.ToList();
            var pickIndex = pickList.GetIndexForWeight(weight);

            if (pickIndex == null)
            {
                throw new IndexOutOfRangeException(
                    "The specified weight is not in range of the total weight of the pick section. Weight: " + weight);
            }

            return pickList.ElementAt((int)pickIndex);
        }

        /// <summary>
        /// Get the index of the pick with the given weight.
        /// </summary>
        /// <param name="picks">List of picks to look through.</param>
        /// <param name="weight">The weight being looked for.</param>
        /// <returns>
        /// A nullable int containing the index. If the index cannot be found, then null is returned.
        /// </returns>
        public static int? GetIndexForWeight(this IEnumerable<Pick> picks, uint weight)
        {
            return picks.Select(pick => new PickAdapter(pick)).GetIndexForWeight(weight);
        }

        /// <summary>
        /// Get the index of the pick with the given weight.
        /// </summary>
        /// <param name="picks">List of picks to look through.</param>
        /// <param name="weight">The weight being looked for.</param>
        /// <returns>
        /// A nullable int containing the index. If the index cannot be found, then null is returned.
        /// </returns>
        public static int? GetIndexForWeight<T>(this IEnumerable<T> picks, uint weight) where T : IPick
        {
            uint currentWeight = 0;
            int? indexOfPick = null;
            int currentIndex = 0;

            foreach (var pick in picks)
            {
                currentWeight += pick.Weight;
                if (weight < currentWeight)
                {
                    indexOfPick = currentIndex;
                    break;
                }
                currentIndex++;
            }

            return indexOfPick;
        }

        /// <summary>
        /// Get the total weight of all of the picks.
        /// </summary>
        /// <param name="picks">List of picks to get the weight for.</param>
        /// <returns>The total weight of all of the picks.</returns>
        public static uint GetTotalWeight(this IEnumerable<Pick> picks)
        {
            return picks.Select(pick => new PickAdapter(pick)).GetTotalWeight();
        }

        /// <summary>
        /// Get the total weight of all of the picks.
        /// </summary>
        /// <param name="picks">List of picks to get the weight for.</param>
        /// <returns>The total weight of all of the picks.</returns>
        public static uint GetTotalWeight<T>(this IEnumerable<T> picks) where T : IPick
        {
            return picks.Aggregate<T, uint>(0, (current, pick) => current + pick.Weight);
        }

        /// <summary>
        /// Get the pick with the given name.
        /// </summary>
        /// <param name="picks">List of picks to find the pick in.</param>
        /// <param name="requestedName">Name of the pick to get.</param>
        /// <returns>The pick with the given name.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when there is not a pick with the given name.
        /// </exception>
        public static T GetPickByName<T>(this IEnumerable<T> picks, string requestedName) where T : Pick
        {
            var pickForName = (from pick in picks where pick.name == requestedName select pick).FirstOrDefault();

            if (pickForName == null)
            {
                throw new KeyNotFoundException("Could not find pick for name: " + requestedName);
            }

            return pickForName;
        }

        /// <summary>
        /// Get a weight which will result in the specified pick when passed to GetPickByWeight.
        /// </summary>
        /// <param name="picks">List of picks containing the desired pick.</param>
        /// <param name="pickName">The name of the pick to get the weight for.</param>
        /// <returns>A weight which will result in the specified pick.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the pick list does not contain the specified pick.
        /// </exception>
        public static uint GetWeightForPick(this IEnumerable<Pick> picks, string pickName)
        {
            return picks.Select(pick => new PickAdapter(pick)).GetWeightForPick(pickName);
        }

        /// <summary>
        /// Get a weight which will result in the specified pick when passed to GetPickByWeight.
        /// </summary>
        /// <param name="picks">List of picks containing the desired pick.</param>
        /// <param name="pickName">The name of the pick to get the weight for.</param>
        /// <returns>A weight which will result in the specified pick.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the pick list does not contain the specified pick.
        /// </exception>
        public static uint GetWeightForPick<T>(this IEnumerable<T> picks, string pickName) where T : IPick
        {
            uint currentWeight = 0;
            var pickFound = false;
            var pickList = picks as IList<T> ?? picks.ToList();
            for(var pickIndex = 0; pickIndex < pickList.Count && !pickFound; pickIndex++)
            {
                if(pickList[pickIndex].Name == pickName)
                {
                    pickFound = true;
                }
                else
                {
                    currentWeight += pickList[pickIndex].Weight;
                }
            }

            if(!pickFound)
            {
                throw new KeyNotFoundException("Could not find pick: " + pickName);
            }

            return currentWeight;
        }
    }
}
