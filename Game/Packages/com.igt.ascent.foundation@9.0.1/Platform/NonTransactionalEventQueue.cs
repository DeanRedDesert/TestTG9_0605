// -----------------------------------------------------------------------
// <copyright file = "NonTransactionalEventQueue.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform
{
    using System;
    using Game.Core.Communication.Foundation.F2XTransport;
    using Restricted.EventManagement;

    /// <inheritdoc cref="DuplexEventQueue"/>
    /// <inheritdoc cref="INonTransactionalEventCallbacks"/>
    /// <summary>
    /// The event queue for non transactional events sent by Foundation.
    /// </summary>
    /// <devdoc>
    /// This class is added so that it would implement INonTransactionalEventCallbacks.
    /// </devdoc>
    public sealed class NonTransactionalEventQueue : DuplexEventQueue, INonTransactionalEventCallbacks
    {
        #region Implementation of INonTransactionalEventCallbacks

        /// <inheritdoc />
        void INonTransactionalEventCallbacks.PostEvent(EventArgs foundationEvent)
        {
            PostEvent(foundationEvent);
        }

        #endregion
    }
}