// -----------------------------------------------------------------------
// <copyright file = "TransactionalEventQueue.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform
{
    using System;
    using Game.Core.Communication.Foundation.F2XTransport;
    using Restricted.EventManagement;

    /// <inheritdoc cref="BlockingEventQueue"/>
    /// <inheritdoc cref="IEventCallbacks"/>
    /// <summary>
    /// The event queue for transactional events sent by Foundation.
    /// </summary>
    /// <devdoc>
    /// This class is added so that it would implement IEventCallbacks.
    /// </devdoc>
    public sealed class TransactionalEventQueue : BlockingEventQueue, IEventCallbacks
    {
        #region Implementation of IEventCallbacks

        /// <inheritdoc />
        void IEventCallbacks.PostEvent(EventArgs foundationEvent)
        {
            PostEvent(foundationEvent);
        }

        #endregion
    }
}