//-----------------------------------------------------------------------
// <copyright file = "CriticalDataCache.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;
    using F2XTransport;

    /// <summary>
    /// Class which caches critical data access.
    /// </summary>
    internal class CriticalDataCache
    {
        #region fields

        /// <summary>
        /// Dictionary which stores all cached data as it appears in critical data.
        /// </summary>
        private readonly Dictionary<CriticalDataScope, Dictionary<string, CachedData>> cache =
            new Dictionary<CriticalDataScope, Dictionary<string, CachedData>>();

        /// <summary>
        /// Scopes that will be cached.
        /// </summary>
        private readonly List<CriticalDataScope> scopesForCaching = new List<CriticalDataScope>
        {
            CriticalDataScope.Theme,
            CriticalDataScope.GameCycle,
            CriticalDataScope.Payvar,
            CriticalDataScope.ThemePersistent,
            CriticalDataScope.PayvarPersistent,
            CriticalDataScope.PayvarAnalytics,
            CriticalDataScope.ThemeAnalytics,
            CriticalDataScope.Extension,
            CriticalDataScope.ExtensionAnalytics,
            CriticalDataScope.ExtensionPersistent,
            CriticalDataScope.History
        };

        /// <summary>
        /// Scopes that do not need to be cached.
        /// </summary>
        private readonly List<CriticalDataScope> scopesExcludedForCaching = new List<CriticalDataScope>
        {
            CriticalDataScope.Feature
        };

        /// <summary>
        /// Scopes that are cleared with each game cycle.
        /// </summary>
        private readonly List<CriticalDataScope> gameCycleScopes = new List<CriticalDataScope>
        {
            CriticalDataScope.GameCycle,
            CriticalDataScope.History
        };

        /// <summary>
        /// Game modes in which the critical data cache should not be used.
        /// </summary>
        private readonly List<GameMode> nonCachedModes = new List<GameMode>
        {
            GameMode.History,
            GameMode.Invalid
        };

        /// <summary>
        /// Flag indicating if a cycle has been started after boot. If a cycle has been started, then we know that
        /// the game cycle and history scope can use default read results when no value is cached.
        /// </summary>
        private bool cycleAfterBoot;

        /// <summary>
        /// The current game mode. Used to enable and disable caching in different modes.
        /// </summary>
        private GameMode gameMode = GameMode.Invalid;

        #endregion

        #region private properties

        /// <summary>
        /// Get a flag indicating if the current mode uses cache.
        /// </summary>
        private bool IsCachedMode
        {
            get { return !nonCachedModes.Contains(gameMode); }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Construct a new cache.
        /// </summary>
        public CriticalDataCache()
        {
            foreach(CriticalDataScope scope in Enum.GetValues(typeof(CriticalDataScope)))
            {
                if(scopesForCaching.Contains(scope))
                {
                    cache[scope] = new Dictionary<string, CachedData>();
                }
                else if(!scopesExcludedForCaching.Contains(scope))
                {
                    throw new ApplicationException(
                        string.Format(
                            "Scope not accounted for. {0} was not found in member lists scopeForCaching or scopesExcludedForCaching. Please add {0} to the appropriate list",
                            scope));
                }
            }
        }

        /// <summary>
        /// Cache the given data at the given scope and path.
        /// </summary>
        /// <param name="scope">Scope of the data to cache.</param>
        /// <param name="path">Path of the data to cache.</param>
        /// <param name="data">Data to cache.</param>
        /// <remarks>
        /// The critical data path should be validated before calling this function.
        /// A null value may be cached in order to prevent multiple lookups for an unset value.
        /// </remarks>
        public void CacheData(CriticalDataScope scope, string path, byte[] data)
        {
            //Do not cache the data in a non-cached mode.
            if(IsCachedMode)
            {
                CacheData(scope, path, data, false); // false = Don't force a critical data write.
            }
        }

        /// <summary>
        /// Cache the given data at the given scope and path.
        /// If the data doesn't already exist, force a critical data write.
        /// </summary>
        /// <param name="scope">Scope of the data to cache.</param>
        /// <param name="path">Path of the data to cache.</param>
        /// <param name="data">Data to cache.</param>
        /// <exception cref="InvalidGameModeException">
        /// Thrown when attempting to write data in a non-cached mode. If data is written in these modes, then it will
        /// not get flushed. Also these modes are readonly.
        /// </exception>
        /// <remarks>
        /// The critical data path should be validated before calling this function.
        /// A null value may be cached in order to prevent multiple lookups for an unset value.
        /// </remarks>
        public void WriteData(CriticalDataScope scope, string path, byte[] data)
        {
            if(IsCachedMode)
            {
                CacheData(scope, path, data, true); // true = Force a critical data write.
            }
            else
            {
                throw new InvalidGameModeException("Cannot write data when in a non-cached mode. Mode: " + gameMode);
            }
        }

        /// <summary>
        /// Check if the specified data is cached.
        /// </summary>
        /// <param name="scope">Scope of the data.</param>
        /// <param name="path">Path of the data.</param>
        /// <returns>True if the data is cached.</returns>
        /// <remarks>The critical data path should be validated before calling this function.</remarks>
        public bool IsDataCached(CriticalDataScope scope, string path)
        {
            //If the data is in a cache, then we say it is cached. If it is a cycle scope, and we are after a cycle,
            //then non-cached data can be defaulted.
            return IsCachedMode &&
                   (IsCachedScope(scope) && cache[scope].ContainsKey(path) || IsCycleScope(scope) && cycleAfterBoot);
        }

        /// <summary>
        /// Get the requested data from the cache. This function should only be called for items that are known to be in
        /// the cache.
        /// </summary>
        /// <param name="scope">Critical data scope of the item.</param>
        /// <param name="path">Path to the item in the scope.</param>
        /// <returns>The requested cached data.</returns>
        /// <exception cref="CacheConsistencyException">
        /// Thrown when the contents of the cache are not consistent.
        /// </exception>
        /// <exception cref="InvalidGameModeException">
        /// Thrown when attempting to read from the cache in a non-cached mode.
        /// </exception>
        /// <remarks>The critical data path should be validated before calling this function.</remarks>
        public byte[] GetCachedData(CriticalDataScope scope, string path)
        {
            if(IsCachedMode)
            {
                try
                {
                    //This adds some minor performance impact, but should be less than triggering exceptions on game cycle
                    //scoped data each cycle.
                    if(cycleAfterBoot && IsCycleScope(scope))
                    {
                        if(cache[scope].ContainsKey(path))
                        {
                            return cache[scope][path].Data;
                        }

                        //The game cycle scope is cleared with each game cycle. This means, during a game cycle, if
                        //a value "X" has not been written, then reads from "X" will return null. If a game cycle
                        //has been started since the loading of the game, then the game will have cached all writes
                        //to the game cycle scope. This means that if "Y" was written, then "Y" will be in the cache.
                        //In normal operation, cases where there are not power hits, the game should never make an
                        //external read call from game cycle critical data.
                        //In the case of the history scope a new section of data is allocated for each game cycle.
                        //This means, for the purposes of caching, it is identical to the game cycle scope.
                        return null;
                    }
                    //The exception is faster than checking if it is actually cached at this point. Client code currently
                    //does that check.
                    return cache[scope][path].Data;
                }
                catch(CacheCrcException exception)
                {
                    throw new CacheConsistencyException(
                        string.Format("Error accessing data. Scope: {0} Path: {1} Reason: {2}", scope, path, exception));
                }
            }

            //If the game is not in a mode which supports caching, then the cache should not be used.
            throw new InvalidGameModeException("The critical data cache cannot be used in the current game mode." +
                                               gameMode);
        }

        /// <summary>
        /// Remove a cached item.
        /// </summary>
        /// <param name="scope">The scope of the item to remove.</param>
        /// <param name="path">The path of the item to remove.</param>
        /// <remarks>The critical data path should be validated before calling this function.</remarks>
        public void RemoveCachedData(CriticalDataScope scope, string path)
        {
            if(IsCachedScope(scope))
            {
                cache[scope].Remove(path);
            }
        }

        /// <summary>
        /// Get the pending writes to critical data.
        /// </summary>
        /// <returns>
        /// A dictionary of data elements, indexed by CriticalDataScope and path.
        /// </returns>
        /// <remarks>
        /// This removes useless pending writes prior to returning the dictionary.
        /// After calling this, the returned results should be written to critical data.
        /// </remarks>
        public Dictionary<CriticalDataScope, Dictionary<string, byte[]>> GetPendingWrites()
        {
            var writesPending = new Dictionary<CriticalDataScope, Dictionary<string, byte[]>>();

            foreach(var scope in cache.Keys)
            {
                var pendingData =
                    cache[scope].Where(entry => entry.Value.PendingWriteExists)
                        .ToDictionary(entry => entry.Key, entry => entry.Value.Data);

                if(pendingData.Any())
                {
                    writesPending[scope] = pendingData;
                }
            }

            return writesPending;
        }

        /// <summary>
        /// Flush all pending writes.
        /// </summary>
        /// <remarks>Copies pending writes to the cache.</remarks>
        public void FlushPendingWrites()
        {
            foreach(var cachedData in cache.Values.SelectMany(scopedCache => scopedCache.Values))
            {
                cachedData.FlushPendingWrite();
            }
        }

        /// <summary>
        /// Check if the specified scope is cached.
        /// </summary>
        /// <param name="scope">The scope to check.</param>
        /// <returns>True if the scope is cached.</returns>
        public bool IsCachedScope(CriticalDataScope scope)
        {
            return cache.ContainsKey(scope);
        }

        /// <summary>
        /// Inform the cache that a cycle has elapsed. This allows the cache to manage scopes associated with a game
        /// cycle.
        /// </summary>
        public void CycleElapsed()
        {
            ClearGameCycleCache();
            cycleAfterBoot = true;
        }

        /// <summary>
        /// Inform the cache of the current game mode. Can be used to make decisions about caching. In history
        /// mode, for instance, the history scope should no longer be cached. When changing the context the cache
        /// is cleared. A new context may have new data or rules.
        /// </summary>
        /// <param name="mode">The current theme context.</param>
        public void SetGameMode(GameMode mode)
        {
            ClearAllCache();
            gameMode = mode;
            //Game mode can change in the middle of a game cycle,
            //such as entering the History menu while a game cycle is playing.
            cycleAfterBoot = false;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Cache the given data at the given scope and path.
        /// </summary>
        /// <param name="scope">Scope of the data to cache.</param>
        /// <param name="path">Path of the data to cache.</param>
        /// <param name="data">Data to cache.</param>
        /// <param name="forceWrite">Tells whether to force the cached data to be written to critical data.</param>
        /// <remarks>
        /// The critical data path should be validated before calling this function.
        /// A null value may be cached in order to prevent multiple lookups for an unset value.
        /// </remarks>
        private void CacheData(CriticalDataScope scope, string path, byte[] data, bool forceWrite)
        {
            if(IsCachedScope(scope))
            {
                // If cached data already exists, just set the .Data property.
                // This will record a pending write inside the CachedData object.
                if(cache[scope].ContainsKey(path))
                {
                    cache[scope][path].Data = data;
                }
                // Create a new cached data, if it doesn't already exist.
                else
                {
                    cache[scope][path] = new CachedData(data);

                    // If told to force a critical data write, pass it to the cache.
                    // Otherwise, let cache[scope][path].ForceWrite take its default value.
                    if(forceWrite)
                    {
                        cache[scope][path].ForceWrite = true;
                    }
                }
            }
        }

        /// <summary>
        /// Clear the cache.
        /// </summary>
        private void ClearAllCache()
        {
            foreach(var scope in cache.Keys)
            {
                cache[scope].Clear();
            }
        }

        /// <summary>
        /// Clear the game cycle cache. This includes history as it is associated with the cycle.
        /// </summary>
        private void ClearGameCycleCache()
        {
            foreach(var criticalDataScope in gameCycleScopes)
            {
                cache[criticalDataScope].Clear();
            }
        }

        /// <summary>
        /// Check if a scope is a game cycle scope. Meaning that scope is cleared each cycle.
        /// </summary>
        /// <param name="scope">The scope to check.</param>
        /// <returns>True if the scope is a cycle scope.</returns>
        private bool IsCycleScope(CriticalDataScope scope)
        {
            return gameCycleScopes.Contains(scope);
        }

        #endregion
    }
}
