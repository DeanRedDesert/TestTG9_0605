//-----------------------------------------------------------------------
// <copyright file = "CircularBuffer.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.RandomNumbers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A circular buffer for storing the most recent n items.
    /// </summary>
    internal class CircularBuffer<T>
    {
        /// <summary>The circular buffer.</summary>
        private readonly T[] buffer;

        /// <summary>The index of the most recent entry in the buffer.</summary>
        private int lastEntryIndex;

        /// <summary>The number of entries stored in the buffer.</summary>
        private int entriesCount;

        /// <summary>
        /// Create a new <see cref="CircularBuffer{T}"/>.
        /// </summary>
        /// <param name="maxEntries">The maximum number of entries to store.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="maxEntries"/> is zero, negative, or too large to create the buffer.
        /// </exception>
        public CircularBuffer(int maxEntries)
        {
            if(maxEntries <= 0)
            {
                throw new ArgumentOutOfRangeException("maxEntries", "maxEntries must be a positive number");
            }

            MaxEntries = maxEntries;
            try
            {
                buffer = new T[MaxEntries];
            }
            catch(Exception)
            {
                throw new ArgumentOutOfRangeException("maxEntries", "maxEntries is too large");
            }
            Clear();
        }

        /// <summary>
        /// Gets the number of entries in the circular buffer.
        /// </summary>
        public int MaxEntries { get; private set; }

        /// <summary>
        /// Record an entry in the buffer
        /// </summary>
        /// <param name="entry">The entry to record.</param>
        public void AddEntry(T entry)
        {
            lastEntryIndex = (lastEntryIndex + 1) % MaxEntries;
            buffer[lastEntryIndex] = entry;

            if(entriesCount < MaxEntries)
            {
                ++entriesCount;
            }
        }

        /// <summary>
        /// Record a collection of entries in the buffer.
        /// </summary>
        /// <param name="entries">A collection of entries to record.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entries"/> is null.</exception>
        public void AddEntries(IEnumerable<T> entries)
        {
            if(entries == null)
            {
                throw new ArgumentNullException("entries");
            }

            var entriesAdded = 0;

            foreach(var entry in entries)
            {
                lastEntryIndex = (lastEntryIndex + 1) % MaxEntries;
                buffer[lastEntryIndex] = entry;
                ++entriesAdded;
            }

            entriesCount += entriesAdded;
            if(entriesCount > MaxEntries)
            {
                entriesCount = MaxEntries;
            }
        }

        /// <summary>
        /// Get a list of up to the last <see cref="MaxEntries"/> numbers recorded.
        /// </summary>
        /// <returns>A list of up to the last maxEntries numbers recorded.</returns>
        public IList<T> GetEntries()
        {
            var tempList = new List<T>();

            if(entriesCount > 0)
            {
                var firstEntryIndex = (lastEntryIndex + MaxEntries + 1 - entriesCount) % MaxEntries;

                // If there is one contiguous range, select that range.
                if(firstEntryIndex <= lastEntryIndex)
                {
                    tempList.AddRange(buffer.Skip(firstEntryIndex).Take(entriesCount));

                }
                // If the circular buffer wraps around the physical buffer, retrieve it in chunks.
                else
                {
                    tempList.AddRange(buffer.Skip(firstEntryIndex).Take(MaxEntries - firstEntryIndex));
                    tempList.AddRange(buffer.Take(lastEntryIndex + 1));
                }
            }

            return tempList;
        }

        /// <summary>
        /// Clear out the buffer.
        /// </summary>
        public void Clear()
        {
            lastEntryIndex = lastEntryIndex - 1;
            entriesCount = 0;
        }
    }
}