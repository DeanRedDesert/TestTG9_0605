// -----------------------------------------------------------------------
// <copyright file = "ThemeStore.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.CoplayerLib.Standard
{
    using Game.Core.Communication.Foundation.F2X;

    /// <summary>
    /// This class provides operations to access the ThemeStore of critical data.
    /// </summary>
    internal sealed class ThemeStore : CachedCriticalDataStoreBase<IThemeStoreCategory>
    {
        /// <inheritdoc/>
        public ThemeStore(ICriticalDataStoreAccessValidator storeAccessValidator)
            : base(storeAccessValidator)
        {
        }

        /// <inheritdoc/>
        protected override string StoreName => nameof(ThemeStore);
    }
}
