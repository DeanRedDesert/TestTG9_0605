// -----------------------------------------------------------------------
// <copyright file = "GamePlayStore.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.CoplayerLib.Standard
{
    using Game.Core.Communication.Foundation.F2X;
    using Interfaces;

    /// <summary>
    /// This class provides operations to access the GamePlayStore of critical data.
    /// </summary>
    internal sealed class GamePlayStore : CycleCachedCriticalDataStoreBase<IGamePlayStoreCategory>
    {
        /// <inheritdoc/>
        public GamePlayStore(ICriticalDataStoreAccessValidator storeAccessValidator,
                             IGameCyclePlayRestricted gameCyclePlayRestricted)
            : base(storeAccessValidator, gameCyclePlayRestricted)
        {
        }

        /// <inheritdoc/>
        protected override string StoreName => nameof(GamePlayStore);
    }
}
