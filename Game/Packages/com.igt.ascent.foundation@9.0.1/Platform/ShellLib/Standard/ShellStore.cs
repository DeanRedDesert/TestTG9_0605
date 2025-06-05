// -----------------------------------------------------------------------
// <copyright file = "ShellStore.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Standard
{
    using Game.Core.Communication.Foundation.F2X;

    /// <summary>
    /// This class provides operations to access the ShellStore of critical data.
    /// </summary>
    internal sealed class ShellStore : CachedCriticalDataStoreBase<IShellStoreCategory>
    {
        /// <inheritdoc/>
        public ShellStore(ICriticalDataStoreAccessValidator storeAccessValidator)
            : base(storeAccessValidator)
        {
        }

        /// <inheritdoc/>
        protected override string StoreName => nameof(ShellStore);
    }
}
