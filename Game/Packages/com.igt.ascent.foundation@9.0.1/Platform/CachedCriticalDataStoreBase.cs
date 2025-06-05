// -----------------------------------------------------------------------
// <copyright file = "CachedCriticalDataStoreBase.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform
{
    using System.Collections.Generic;
    using System.Linq;
    using Game.Core.Communication.Foundation.F2X;
    using Interfaces;

    /// <summary>
    /// This generic base class encapsulates the basic operations on accessing the key-value store
    /// which has a caching capability.
    /// Critical data reading will try to read from the cache first;  Critical data writing will be cached
    /// until all pending writes are committed to Foundation safe storage at the end of a transaction.
    /// </summary>
    /// <typeparam name="TCategory">
    /// The generic interface type of key-value store category.
    /// </typeparam>
    internal abstract class CachedCriticalDataStoreBase<TCategory> : CriticalDataStoreBase<TCategory>,
                                                                     ICachedCriticalDataStore
        where TCategory : IKeyValueStoreCategory
    {
        #region Private Fields

        protected readonly CriticalDataCache CriticalDataCache = new CriticalDataCache();

        #endregion

        #region Constructors

        /// <inheritdoc/>
        protected CachedCriticalDataStoreBase(ICriticalDataStoreAccessValidator storeAccessValidator)
            : base(storeAccessValidator)
        {
        }

        #endregion

        #region ICriticalDataStoreCache Implementation

        /// <inheritdoc/>
        public virtual void ResetCache()
        {
            CriticalDataCache.Clear();
        }

        /// <inheritdoc/>
        /// <devdoc>
        /// isHeavyweight is currently not used.
        /// </devdoc>
        public virtual void CommitPendingWrites(bool isHeavyweight)
        {
            var pendingWritesBlock = CriticalDataCache.GetPendingWrites();

            if(pendingWritesBlock.HasData)
            {
                WriteToFoundation(pendingWritesBlock);
            }

            CriticalDataCache.FlushPendingWrites();
        }

        #endregion

        #region CriticalDataStoreBase Overrides

        /// <inheritdoc/>
        protected override ICriticalDataBlock DoRead(IList<CriticalDataName> nameList)
        {
            var names = nameList.Select(name => (string)name).ToList();
            var result = CriticalDataCache.Read(names);

            var missingNames = nameList.Except(result.GetNameList()).ToList();

            if(missingNames.Any())
            {
                // If the data was not cached, then read from Foundation...
                var dataBlock = ReadFromFoundation(missingNames);

                // ... and cache it without forcing a (unnecessary) write.
                CriticalDataCache.Write(dataBlock, false);

                foreach(var name in missingNames)
                {
                    // Merge the new data into the result.
                    result.SetSerializedData(name, dataBlock.GetSerializedData(name));
                }
            }

            return result;
        }

        /// <inheritdoc/>
        protected override void DoFill(IList<CriticalDataName> nameList, ICriticalDataBlock criticalDataBlock)
        {
            var names = nameList.Select(name => (string)name).ToList();
            var cachedNames = CriticalDataCache.Fill(names, criticalDataBlock).Select(name => (CriticalDataName)name);

            var missingNames = nameList.Except(cachedNames).ToList();

            if(missingNames.Any())
            {
                // If the data was not cached, then read from Foundation...
                var dataBlock = ReadFromFoundation(missingNames);

                // ... and cache it without forcing a (unnecessary) write.
                CriticalDataCache.Write(dataBlock, false);

                foreach(var name in missingNames)
                {
                    // Fill the new data in the block.
                    criticalDataBlock.SetSerializedData(name, dataBlock.GetSerializedData(name));
                }
            }
        }

        /// <inheritdoc/>
        protected override void DoRemove(IList<CriticalDataName> nameList)
        {
            var names = nameList.Select(name => (string)name).ToList();
            CriticalDataCache.Remove(names);

            // Tell Foundation to remove right away.
            RemoveFromFoundation(nameList);
        }

        /// <inheritdoc/>
        protected override void DoWrite(ICriticalDataBlock criticalDataBlock)
        {
            // Cache the given data.  If the data doesn't already exist, force a critical data write.
            CriticalDataCache.Write(criticalDataBlock, true);
        }

        #endregion
    }
}