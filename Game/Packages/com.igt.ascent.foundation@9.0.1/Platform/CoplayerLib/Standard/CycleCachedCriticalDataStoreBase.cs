// -----------------------------------------------------------------------
// <copyright file = "CycleCachedCriticalDataStoreBase.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.CoplayerLib.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Game.Core.Communication.Foundation.F2X;
    using Interfaces;
    using Platform.Interfaces;

    /// <summary>
    /// This generic base class encapsulates the basic operations on accessing the key-value store
    /// which has a caching capability, and the caching behavior is tied to the game cycle.
    /// </summary>
    /// <typeparam name="TCategory">
    /// The generic interface type of key-value store category.
    /// </typeparam>
    internal abstract class CycleCachedCriticalDataStoreBase<TCategory> : CachedCriticalDataStoreBase<TCategory>
        where TCategory : IKeyValueStoreCategory
    {
        #region Fields

        /// <summary>
        /// Flag indicating if a cycle has been started after boot. If a cycle has been started, then we know that
        /// the game play store can use default read results when no value is cached.
        /// </summary>
        private bool cycleAfterBoot;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor of the generic base class.
        /// </summary>
        /// <param name="storeAccessValidator">
        /// The interface to validate the critical data access to the GamePlayStore.
        /// </param>
        /// <param name="gameCyclePlayRestricted">
        /// The interface to monitor the game cycle state transitions.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="gameCyclePlayRestricted"/> is null.
        /// </exception>
        protected CycleCachedCriticalDataStoreBase(ICriticalDataStoreAccessValidator storeAccessValidator,
                                                   IGameCyclePlayRestricted gameCyclePlayRestricted)
            : base(storeAccessValidator)
        {
            if(gameCyclePlayRestricted == null)
            {
                throw new ArgumentNullException(nameof(gameCyclePlayRestricted));
            }

            gameCyclePlayRestricted.GameCycleStateTransitioned += HandleGameCycleStateTransitioned;
        }

        #endregion

        #region Base Overrides

        /// <inheritdoc/>
        public override void ResetCache()
        {
            base.ResetCache();

            // Game context can change in the middle of a game cycle,
            // such as entering the History menu while a game cycle is playing.
            cycleAfterBoot = false;
        }

        /// <inheritdoc/>
        protected override ICriticalDataBlock DoRead(IList<CriticalDataName> nameList)
        {
            var names = nameList.Select(name => (string)name).ToList();
            var result = CriticalDataCache.Read(names);

            var missingNames = nameList.Except(result.GetNameList()).ToList();

            if(missingNames.Any())
            {
                if(cycleAfterBoot)
                {
                    // The game cycle store is cleared with each game cycle. This means, during a game cycle, if
                    // a value "X" has not been written, then reads from "X" will return null.
                    // Usually the game should never make an external read call from game cycle critical data except for
                    // some power hit scenarios.  That is, if a game cycle has been started since the loading of the game,
                    // when the game tries to read a data "Y", "Y" should have already been written to the cache,
                    // hence missingNames should be empty at this point.
                    foreach(var name in missingNames)
                    {
                        // Merge the new data into the result.
                        result.SetSerializedData(name, null);
                    }
                }
                else
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
                if(cycleAfterBoot)
                {
                    // The game cycle store is cleared with each game cycle. This means, during a game cycle, if
                    // a value "X" has not been written, then reads from "X" will return null.
                    // Usually the game should never make an external read call from game cycle critical data except for
                    // some power hit scenarios.  That is, if a game cycle has been started since the loading of the game,
                    // when the game tries to read a data "Y", "Y" should have already been written to the cache,
                    // hence missingNames should be empty at this point.
                    foreach(var name in missingNames)
                    {
                        // Fill the new data in the block.
                        criticalDataBlock.SetSerializedData(name, null);
                    }
                }
                else
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
        }

        #endregion

        #region  Private Methods

        /// <summary>
        /// Handles the game cycle transitioned event.
        /// </summary>
        /// <remarks>
        /// Transitions to be handled:
        /// <list type="number">
        /// <item>
        /// Game play store is cleared by Foundation when entering Committed state,
        /// hence the cache needs to be cleared as well.
        /// </item>
        /// <item>
        /// Once a cycle is started after the loading of the game, it would be safe to assume
        /// can use default read results when no value is cached
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void HandleGameCycleStateTransitioned(object sender, GameCycleStateTransitionedEventArgs eventArgs)
        {
            switch(eventArgs.ToState)
            {
                case GameCycleState.Committed:
                    CriticalDataCache.Clear();
                    break;
                case GameCycleState.Idle:
                    cycleAfterBoot = true;
                    break;
            }
        }

        #endregion
    }
}