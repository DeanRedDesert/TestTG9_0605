//-----------------------------------------------------------------------
// <copyright file = "DefaultRngAdapter.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.GameLib.Interfaces;

    /// <summary>
    /// An implementation of <see cref="IRandomNumberGenerator"/> that uses the <see cref="Random"/> class.
    /// </summary>
    internal class DefaultRngAdapter : IRandomNumberGenerator
    {

        /// <summary>
        /// Random number generator, used to seed the IGT RNG and as a backup generator
        /// when IGT server isn't used.
        /// </summary>
        private readonly Random random;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRngAdapter"/> class using an instance of <see cref="CryptoRandom"/>
        /// created with a time-dependent default seed value.
        /// </summary>
        internal DefaultRngAdapter()
            : this(new CryptoRandom())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRngAdapter"/> class.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> object to use for random number generation.</param>
        internal DefaultRngAdapter(Random random)
        {
            this.random = random;
        }

        /// <inheritdoc/>
        public void Cycle()
        {
        }

        /// <inheritdoc/>
        public void ReseedWithChaos()
        {
        }

        /// <inheritdoc/>
        public ICollection<int> GetRandomNumbers(RandomValueRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            
            var randomNumbers = new List<int>();

            uint preCount = 0;

            // Add the pre picked numbers at the head of the returned list.
            if (request.PrePickedNumbers != null)
            {
                randomNumbers.AddRange(request.PrePickedNumbers);
                preCount = (uint)request.PrePickedNumbers.Count;
            }

            // Random takes maxValue as the exclusive upper bound, while
            // RangeMax is the inclusive upper bound.
            var maxValue = request.RangeMax + 1;
            var minValue = request.RangeMin;
            var offset = 0;

            // If maxValue has overflowed to int.MinValue, and if it's possible to
            // reduce both the minValue and maxValue by 1 to correct this, do so
            // and note that the resulting random number must be offset by 1.
            // Note: If the minValue cannot be reduced, then the request is for
            // an unscaled 32-bit number.  This case is handled below.
            if (maxValue == int.MinValue && minValue > int.MinValue)
            {
                maxValue--;
                minValue--;
                offset = 1;
            }

            var countToGenerate = request.Count - preCount;

            // The number of occurrences allowed is the duplicates plus one.
            var maxInstances = request.MaxDuplicates + 1;

            int randNum;
            int duplicateCount;

            // If request.RangeMin = int.MinValue, then the request is for an unscaled
            // 32-bit number.  random.Next() cannot provide that, so use random.NextBytes().
            if (maxValue == int.MinValue)
            {
                var buffer = new byte[4];

                // This is essentially the same loop as below, but is done this way
                // to avoid checking maxValue == int.MinValue inside a nested loop.
                for (uint count = 0; count < countToGenerate; count++)
                {
                    do
                    {
                        random.NextBytes(buffer);
                        randNum = BitConverter.ToInt32(buffer, 0);

                        duplicateCount = randomNumbers.Count(n => n == randNum);
                    } while (duplicateCount >= maxInstances);

                    randomNumbers.Add(randNum);
                }
            }
            else
            {
                // This is essentially the same loop as above, but is done this way
                // to avoid checking maxValue == int.MinValue inside a nested loop.
                for (uint count = 0; count < countToGenerate; count++)
                {
                    do
                    {
                        randNum = random.Next(minValue, maxValue) + offset;
                        duplicateCount = randomNumbers.Count(n => n == randNum);
                    } while (duplicateCount >= maxInstances);

                    randomNumbers.Add(randNum);
                }
            }

            return randomNumbers;
        }
    }
}
