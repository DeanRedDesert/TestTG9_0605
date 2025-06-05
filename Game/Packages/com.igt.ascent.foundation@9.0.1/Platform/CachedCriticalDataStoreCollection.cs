// -----------------------------------------------------------------------
// <copyright file = "CachedCriticalDataStoreCollection.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform
{
    using System.Collections.Generic;

    /// <summary>
    /// A wrapper class that manages a collection of <see cref="ICachedCriticalDataStore"/>.
    /// </summary>
    public class CachedCriticalDataStoreCollection
    {
        #region Private Fields

        private readonly List<ICachedCriticalDataStore> stores = new List<ICachedCriticalDataStore>();

        #endregion

        #region Public Methods

        /// <summary>
        /// Checks if an object is a critical data store cache, if yes, add it to the list.
        /// </summary>
        /// <param name="candidate">The object to check and add.</param>
        public void Add(object candidate)
        {
            if(candidate is ICachedCriticalDataStore cachedCriticalDataStore)
            {
                stores.Add(cachedCriticalDataStore);
            }
        }

        /// <summary>
        /// Resets all critical data store caches.
        /// </summary>
        public void ResetAll()
        {
            foreach(var store in stores)
            {
                store.ResetCache();
            }
        }

        /// <summary>
        /// Commits all pending writes from all critical data store caches.
        /// </summary>
        /// <param name="isHeavyweight">
        /// The flag indicating whether current transaction is heavyweight.
        /// </param>
        public void CommitAllPendingWrites(bool isHeavyweight)
        {
            foreach(var store in stores)
            {
                store.CommitPendingWrites(isHeavyweight);
            }
        }

        #endregion
    }
}