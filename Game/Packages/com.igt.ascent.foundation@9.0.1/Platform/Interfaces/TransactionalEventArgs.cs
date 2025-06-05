// -----------------------------------------------------------------------
// <copyright file = "TransactionalEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <inheritdoc/>
    /// <summary>
    /// Base class for all transactional events.
    /// </summary>
    [Serializable]
    public abstract class TransactionalEventArgs : PlatformEventArgs
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of <see cref="TransactionalEventArgs" />.
        /// </summary>
        /// <param name="isHeavy">
        /// The flag indicating whether the transactional event is heavyweight.
        /// This parameter is optional.  If not specified, it defaults to true.
        /// </param>
        protected TransactionalEventArgs(bool isHeavy = true)
            : base(isHeavy ? TransactionWeight.Heavy : TransactionWeight.Light)
        {
        }
    }
}