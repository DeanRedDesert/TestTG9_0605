// -----------------------------------------------------------------------
// <copyright file = "CriticalDataCache.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform
{
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces;

    /// <summary>
    /// This class manages the critical data caching for a critical data store.
    /// </summary>
    /// <devdoc>
    /// This implementation skips all parameter checking as this is an internal class,
    /// all parameters should have been validated by the callers.
    /// </devdoc>
    internal sealed class CriticalDataCache
    {
        #region Private Fields

        /// <summary>
        /// The collection of the cached critical data.
        /// </summary>
        private readonly Dictionary<string, CachedData> cache = new Dictionary<string, CachedData>();

        #endregion

        #region Public Methods

        /// <summary>
        /// Reads multiple critical data items from the cache.
        /// </summary>
        /// <param name="nameList">
        /// Identifies a list of the names of critical data to read.
        /// </param>
        /// <returns>
        /// A critical data block which contains all the critical data that are available in the cache.
        /// </returns>
        public ICriticalDataBlock Read(IList<string> nameList)
        {
            var foundNames = nameList.Intersect(cache.Keys).ToList();

            var readData = foundNames.ToDictionary(name => name, name => cache[name].Data);

            return new CriticalDataBlock(readData);
        }

        /// <summary>
        /// Reads multiple critical data items from the cache, and set them in the
        /// given critical data block.
        /// </summary>
        /// <param name="nameList">
        /// List of names of the critical data to read from the cache.
        /// </param>
        /// <param name="criticalDataBlock">
        /// The critical data block to hold the reading result.
        /// </param>
        /// <returns>
        /// A list of critical data names that have been filled.
        /// </returns>
        public IList<string> Fill(IList<string> nameList, ICriticalDataBlock criticalDataBlock)
        {
            var foundNames = nameList.Intersect(cache.Keys).ToList();

            foreach(var name in foundNames)
            {
                criticalDataBlock.SetSerializedData(name, cache[name].Data);
            }

            return foundNames;
        }

        /// <summary>
        /// Removes multiple critical data items from the cache.
        /// </summary>
        /// <param name="nameList">
        /// Identifies a list of the names of critical data to remove.
        /// </param>
        public void Remove(IList<string> nameList)
        {
            foreach(var name in nameList)
            {
                cache.Remove(name);
            }
        }

        /// <summary>
        /// Writes multiple critical data items to the cache.
        /// </summary>
        /// <param name="criticalDataBlock">
        /// Specifies a data block interface that contains all the serialized critical data to write.
        /// </param>
        /// <param name="forceWrite">
        /// The flag indicating whether to force a write to the Foundation safe storage when adding a new cached data.
        /// If the call is to cache a data that is just read from Foundation, then there is no need to force a write.
        /// </param>
        public void Write(ICriticalDataBlock criticalDataBlock, bool forceWrite)
        {
            foreach(var name in criticalDataBlock.GetNameList())
            {
                // If cached data already exists, just set the .Data property.
                // This will record a pending write inside the CachedData object.
                if(cache.ContainsKey(name))
                {
                    cache[name].Data = criticalDataBlock.GetSerializedData(name);
                }
                // Create a new cached data, if it doesn't already exist.
                else
                {
                    cache[name] = new CachedData(criticalDataBlock.GetSerializedData(name));

                    // If told to force a critical data write, pass it to the cache.
                    // This will turn on the PendingWriteExists flag for the cached data.
                    if(forceWrite)
                    {
                        cache[name].ForceWrite = true;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the pending writes in the cache.
        /// After calling this, the returned results should be written to the Foundation safe storage.
        /// </summary>
        /// <returns>
        /// A critical data block which contains all pending writes to write the Foundation safe storage.
        /// </returns>
        public ICriticalDataBlock GetPendingWrites()
        {
            var pendingWrites = cache.Where(entry => entry.Value.PendingWriteExists)
                                     .ToDictionary(entry => entry.Key, entry => entry.Value.Data);

            return new CriticalDataBlock(pendingWrites);
        }

        /// <summary>
        /// Flushes all pending writes in the cache.
        /// This copies pending writes to the cached data, and clear all pending writes flags.
        /// </summary>
        public void FlushPendingWrites()
        {
            foreach(var entry in cache)
            {
                entry.Value.FlushPendingWrite();
            }
        }

        /// <summary>
        /// Clears the cached data.
        /// </summary>
        public void Clear()
        {
            cache.Clear();
        }

        #endregion
    }
}