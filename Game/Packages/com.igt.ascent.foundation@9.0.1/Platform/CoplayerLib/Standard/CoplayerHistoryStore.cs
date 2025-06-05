// -----------------------------------------------------------------------
// <copyright file = "CoplayerHistoryStore.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.CoplayerLib.Standard
{
    using Game.Core.Communication.Foundation.F2X;
    using Interfaces;

    /// <summary>
    /// This class provides operations to access the CoplayerHistoryStore of critical data.
    /// </summary>
    /// <remarks>
    /// A new section of data is allocated for coplayer history for each game cycle.
    /// Therefore its caching behaviors are the same as that of game play store.
    /// </remarks>
    internal sealed class CoplayerHistoryStore : CycleCachedCriticalDataStoreBase<ICoplayerHistoryStoreCategory>
    {
        /// <inheritdoc/>
        public CoplayerHistoryStore(ICriticalDataStoreAccessValidator storeAccessValidator,
                                    IGameCyclePlayRestricted gameCyclePlayRestricted)
            : base(storeAccessValidator, gameCyclePlayRestricted)
        {
        }

        /// <inheritdoc/>
        protected override string StoreName => nameof(CoplayerHistoryStore);
    }
}
