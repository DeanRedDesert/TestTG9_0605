// -----------------------------------------------------------------------
// <copyright file = "NonTransactionalEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <inheritdoc/>
    /// <summary>
    /// Base class for all non transactional events.
    /// </summary>
    [Serializable]
    public abstract class NonTransactionalEventArgs : PlatformEventArgs
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of <see cref="NonTransactionalEventArgs" />.
        /// </summary>
        protected NonTransactionalEventArgs() : base(TransactionWeight.None)
        {
        }
    }
}