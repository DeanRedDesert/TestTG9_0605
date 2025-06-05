//-----------------------------------------------------------------------
// <copyright file = "RandomValueRequest.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// The Random Value Request is used to request for random numbers.
    /// It indicates how many number from the specified range are desired,
    /// the minimum for each number, the maximum for each number, and
    /// how many duplicate values are allowed.
    /// 
    /// By default, all numbers obtained could be duplicate.
    /// </summary>
    [Serializable]
    public class RandomValueRequest
    {
        #region Properties

        /// <summary>
        /// Specifies how many random numbers requested.
        /// It includes the PrePickedNumbers.
        /// </summary>
        public uint Count { get; private set; }

        /// <summary>
        /// The inclusive lower bound of the random numbers returned.
        /// </summary>
        public int RangeMin { get; private set; }

        /// <summary>
        /// The inclusive upper bound of the random numbers returned.
        /// RangeMax must be greater than or equal to RangeMin.
        /// </summary>
        public int RangeMax { get; private set; }

        /// <summary>
        /// Specifies how many duplicate copies of a value are allowed.
        /// 0 means each number will occur once, i.e. no duplicate.
        /// 1 allows for two occurrences of the same value.
        /// 2 allows for three occurrences of the same value, and so on.
        /// It applies to the PrePickedNumbers as well as the newly generated numbers.
        /// </summary>
        public uint MaxDuplicates { get; private set; }

        /// <summary>
        /// A string identification chosen by the game, and is used to assist in
        /// testing and demonstration applications.
        /// </summary>
        public string GeneratorName { get; private set; }

        /// <summary>
        /// A list of numbers to be added at the head of the list of returned numbers.
        /// </summary>
        public ICollection<int> PrePickedNumbers { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Construct a request for random numbers without duplicates.
        /// </summary>
        /// <param name="count">How many random numbers to obtain.</param>
        /// <param name="rangeMin">The inclusive lower bound of the random numbers returned.</param>
        /// <param name="rangeMax">The inclusive upper bound of the random numbers returned.
        ///                        RangeMax must be greater than or equal to RangeMin.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when value of count, ranges and duplicates make the request invalid.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the count, values and duplicates of the pre picked numbers make the request invalid.
        /// </exception>
        public RandomValueRequest(uint count, int rangeMin, int rangeMax)
            : this(count, rangeMin, rangeMax, 0, null, null)
        {
        }

        /// <summary>
        /// Construct a request for random numbers with duplicates.
        /// </summary>
        /// <param name="count">
        /// How many random numbers to obtain. It includes the PrePickedNumbers.
        /// </param>
        /// <param name="rangeMin">
        /// The inclusive lower bound of the random numbers returned.
        /// </param>
        /// <param name="rangeMax">
        /// The inclusive upper bound of the random numbers returned.
        /// RangeMax must be greater than or equal to RangeMin.
        /// </param>
        /// <param name="maxDuplicates">
        /// How many duplicate copies of a value are allowed.
        /// 0 means each number will occur once, i.e. no duplicate.
        /// 1 allows for two occurrences of the same value.
        /// 2 allows for three occurrences of the same value, and so on.
        /// It applies to the PrePickedNumbers as well as the newly generated numbers.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when value of count, ranges and duplicates make the request invalid.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the count, values and duplicates of the pre picked numbers make the request invalid.
        /// </exception>
        public RandomValueRequest(uint count, int rangeMin, int rangeMax, uint maxDuplicates)
            : this(count, rangeMin, rangeMax, maxDuplicates, null, null)
        {
        }

        /// <summary>
        /// Construct a request for random numbers with pre picked numbers.
        /// </summary>
        /// <param name="count">
        /// How many random numbers to obtain. It includes the PrePickedNumbers.
        /// </param>
        /// <param name="rangeMin">
        /// The inclusive lower bound of the random numbers returned.
        /// </param>
        /// <param name="rangeMax">
        /// The inclusive upper bound of the random numbers returned.
        /// RangeMax must be greater than or equal to RangeMin.
        /// </param>
        /// <param name="maxDuplicates">
        /// How many duplicate copies of a value are allowed.
        /// 0 means each number will occur once, i.e. no duplicate.
        /// 1 allows for two occurrences of the same value.
        /// 2 allows for three occurrences of the same value, and so on.
        /// It applies to the PrePickedNumbers as well as the newly generated numbers.
        /// </param>
        /// <param name="prePickedNumbers">
        /// A list of numbers to be added at the head of the list of returned numbers.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when value of count, ranges and duplicates make the request invalid.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the count, values and duplicates of the pre picked numbers make the request invalid.
        /// </exception>
        public RandomValueRequest(uint count, int rangeMin, int rangeMax, uint maxDuplicates,
                                  ICollection<int> prePickedNumbers)
            : this(count, rangeMin, rangeMax, maxDuplicates, prePickedNumbers, null)
        {
        }

        /// <summary>
        /// Construct a request for random numbers.
        /// </summary>
        /// <param name="count">
        /// How many random numbers to obtain. It includes the PrePickedNumbers.
        /// </param>
        /// <param name="rangeMin">
        /// The inclusive lower bound of the random numbers returned.
        /// </param>
        /// <param name="rangeMax">
        /// The inclusive upper bound of the random numbers returned.
        /// RangeMax must be greater than or equal to RangeMin.
        /// </param>
        /// <param name="maxDuplicates">
        /// How many duplicate copies of a value are allowed.
        /// 0 means each number will occur once, i.e. no duplicate.
        /// 1 allows for two occurrences of the same value.
        /// 2 allows for three occurrences of the same value, and so on.
        /// It applies to the PrePickedNumbers as well as the newly generated numbers.
        /// </param>
        /// <param name="prePickedNumbers">
        /// A list of numbers to be added at the head of the list of returned numbers.
        /// </param>
        /// <param name="generatorName">
        /// A string identification chosen by the game, which is used to
        /// assist in testing and demonstration applications.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when value of count, ranges and duplicates make the request invalid.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the count, values and duplicates of the pre picked numbers make the request invalid.
        /// </exception>
        public RandomValueRequest(uint count, int rangeMin, int rangeMax, uint maxDuplicates,
                                  ICollection<int> prePickedNumbers, string generatorName)
        {
            // Sanity check on range.
            if (rangeMax < rangeMin)
            {
                throw new ArgumentException("rangeMax must be greater than or equal to rangeMin", nameof(rangeMax));
            }

            // The number of occurrences allowed is the duplicates plus one.
            var maxInstances = maxDuplicates + 1;

            // Sanity check on pre picked numbers
            if (prePickedNumbers != null)
            {
                // Check if the count of the pre picked numbers exceeds or equals the count specified in the request.
                if (prePickedNumbers.Count >= count)
                {
                    throw new ArgumentOutOfRangeException(nameof(prePickedNumbers),
                        "Count of numbers in the pre picked list exceeds or equals to the count specified in the request.");
                }

                // Check if pre picked numbers are within the specified range.
                var items = from number in prePickedNumbers
                            where number < rangeMin || rangeMax < number
                            select number;

                if (items.Count() != 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(prePickedNumbers),
                        "Numbers in the pre picked list are not within the range specified in the request.");
                }

                // Check if count of the pre picked duplicate numbers exceeds the max duplicates of the request.
                foreach (var number in prePickedNumbers)
                {
                    var duplicateCount = prePickedNumbers.Count(n => n == number);

                    if (duplicateCount > maxInstances)
                    {
                        throw new ArgumentOutOfRangeException(nameof(prePickedNumbers),
                            "Count duplicate numbers in the pre picked list exceeds the maxDuplicates specified in the request.");
                    }
                }
            }

            // Check if there are enough distinctive values to accommodate the request.
            if (Math.Ceiling((double)count / maxInstances) > rangeMax - rangeMin + 1
                && rangeMax - rangeMin + 1 > 0) // Don't error if the range covers the entire range of an int.
            {
                throw new ArgumentException("The range is not sufficient for the distinctive values required by the count and the duplicates allowed.");
            }

            Count = count;
            RangeMin = rangeMin;
            RangeMax = rangeMax;
            MaxDuplicates = maxDuplicates;
            PrePickedNumbers = prePickedNumbers;
            GeneratorName = generatorName ?? "GAME";
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("RandomValueRequest -");
            builder.AppendLine("\t Count = " + Count);
            builder.AppendLine("\t Range Min = " + RangeMin);
            builder.AppendLine("\t Range Max = " + RangeMax);
            builder.AppendLine("\t Duplicates Allowed = " + MaxDuplicates);
            builder.AppendLine("\t Pre Picked Numbers = " + PrePickedNumbers);
            builder.AppendLine("\t RNG Name = " + GeneratorName);

            return builder.ToString();
        }
        #endregion
    }
}
