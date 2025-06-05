// -----------------------------------------------------------------------
// <copyright file = "PayvarStore.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.CoplayerLib.Standard
{
    using Game.Core.Communication.Foundation.F2X;

    /// <summary>
    /// This class provides operations to access the PayvarStore of critical data.
    /// </summary>
    internal sealed class PayvarStore : CachedCriticalDataStoreBase<IPayvarStoreCategory>
    {
        /// <inheritdoc/>
        public PayvarStore(ICriticalDataStoreAccessValidator storeAccessValidator)
            : base(storeAccessValidator)
        {
        }

        /// <inheritdoc/>
        protected override string StoreName => nameof(PayvarStore);
    }
}
