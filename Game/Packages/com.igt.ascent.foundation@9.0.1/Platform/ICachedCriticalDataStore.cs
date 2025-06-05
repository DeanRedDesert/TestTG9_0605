// -----------------------------------------------------------------------
// <copyright file = "ICachedCriticalDataStore.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform
{
    using Interfaces;

    /// <summary>
    /// This interface defines the functionality of a critical data store that supports caching mechanism.
    /// </summary>
    internal interface ICachedCriticalDataStore : ICriticalDataStore
    {
        /// <summary>
        /// Resets the cached data.
        /// </summary>
        void ResetCache();

        /// <summary>
        /// Commits all pending critical data writes to the Foundation safe storage.
        /// </summary>
        /// <param name="isHeavyweight">
        /// The flag indicating whether current transaction is heavyweight.
        /// For some stores, such as PayvarSharedStore and ThemeSharedStore etc. in the road map,
        /// committing pending writes can only be done in heavyweight transactions.
        /// </param>
        /// <remarks>
        /// This should be called when a transaction closes, to commit all changes accumulated within that transaction.
        /// Only those changes which are different from cached data will be written, e.g., if, within a single transaction,
        /// a game changes some critical data, then changes it back to its original state, no critical data change will be written.
        /// This also updates the critical data cache.
        /// </remarks>
        // ReSharper disable once UnusedParameter.Global
        void CommitPendingWrites(bool isHeavyweight);
    }
}