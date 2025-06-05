//-----------------------------------------------------------------------
// <copyright file = "NumberSequence.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.RandomNumbers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Evaluator;

    /// <summary>
    /// An implementation of <see cref="INumberSequence"/> that manages a queue of numbers.
    /// </summary>
    public class NumberSequence : INumberSequence
    {
        private readonly Queue<int> sequenceQueue = new Queue<int>();

        #region INumberSequence implementation

        /// <inheritdoc/>
        public void AddNumberSequence(IEnumerable<int> sequence)
        {
            if(sequence == null)
            {
                throw new ArgumentNullException("sequence");
            }

            foreach(var value in sequence)
            {
                sequenceQueue.Enqueue(value);
            }
        }

        #endregion

        /// <summary>
        /// Gets a collection of numbers from this sequence to fill the given request.
        /// </summary>
        /// <param name="request">The <see cref="RandomValueRequest"/> to fill.</param>
        /// <returns>
        /// A collection of numbers from the sequence which may not fully fill the request.
        /// </returns>
        /// <remarks>
        /// If there are not enough numbers available then it will be necessary to create a new request from the 
        /// original one using the sequenced numbers as pre-picked values.
        /// </remarks>
        /// <exception cref="SequenceDuplicateCountException">
        /// Thrown if the sequence contains more duplicates than the requests specifies in 
        /// <see cref="RandomValueRequest.MaxDuplicates"/>.
        /// </exception>
        /// <exception cref="SequenceRangeException">
        /// Thrown if any number pulled from the sequence is out of range for the request.
        /// </exception>
        public ICollection<int> GetSequencedNumbers(RandomValueRequest request)
        {
            // Create the new list of prepicked numbers, using any existing ones from the request.
            var sequencedNumbers = request.PrePickedNumbers == null
                ? new List<int>((int)request.Count)
                : new List<int>(request.PrePickedNumbers);

            if(sequenceQueue.Count == 0)
            {
                return sequencedNumbers;
            }

            while(sequencedNumbers.Count < request.Count && sequenceQueue.Count > 0)
            {
                // Don't modify the queue until we know the number is valid.
                var number = sequenceQueue.Peek();

                if(number <= request.RangeMax && number >= request.RangeMin)
                {
                    // MaxDuplicates indicates the number of allowed duplicates, which is 1 less than the count.
                    // This means that MaxDuplicates of 1 will allow a value to appear 2 times.
                    var duplicates = sequencedNumbers.Count(value => value == number);

                    if(duplicates > request.MaxDuplicates)
                    {
                        throw new SequenceDuplicateCountException(request.MaxDuplicates, number);
                    }

                    // Add a number from the sequence to the filled request.
                    sequencedNumbers.Add(number);

                    // Remove the number from the queue.
                    sequenceQueue.Dequeue();
                }
                else
                {
                    throw new SequenceRangeException(request.RangeMin, request.RangeMax, number);
                }
            }
            return sequencedNumbers;
        }
    }
}