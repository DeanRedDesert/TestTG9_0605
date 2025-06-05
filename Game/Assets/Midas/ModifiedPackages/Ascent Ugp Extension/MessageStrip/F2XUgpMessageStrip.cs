//-----------------------------------------------------------------------
// <copyright file = "F2XUgpMessageStrip.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MessageStrip
{
    using System;
    using System.Collections.Generic;
    using Ascent.Restricted.EventManagement.Interfaces;
    using Interfaces;

    /// <summary>
    /// Implementation of the UgpMessageStrip extended interface that is backed by F2X.
    /// </summary>
    internal class F2XUgpMessageStrip : IUgpMessageStrip, IInterfaceExtension
    {
        #region Private Fields

        /// <summary>
        /// The UgpMessageStrip category handler.
        /// </summary>
        private readonly IUgpMessageStripCategory ugpMessageStripCategory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="F2XUgpMessageStrip"/>.
        /// </summary>
        /// <param name="ugpMessageStripCategory">
        /// The UgpMessageStrip category used to communicate with the foundation.
        /// </param>
        /// <param name="transactionalEventDispatcher">
        /// Interface for processing a transactional event.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the arguments is null.
        /// </exception>
        public F2XUgpMessageStrip(IUgpMessageStripCategory ugpMessageStripCategory,
                                  IEventDispatcher transactionalEventDispatcher)
        {
            if(transactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(transactionalEventDispatcher));
            }

            this.ugpMessageStripCategory = ugpMessageStripCategory ?? throw new ArgumentNullException(nameof(ugpMessageStripCategory));

            transactionalEventDispatcher.EventDispatchedEvent +=
                            (sender, dispatchedEvent) => dispatchedEvent.RaiseWith(this, MessageAdded);
            transactionalEventDispatcher.EventDispatchedEvent +=
                            (sender, dispatchedEvent) => dispatchedEvent.RaiseWith(this, MessageRemoved);
        }

        #endregion

        #region IUgpMessageStrip Implementation

        /// <inheritdoc/>
        public event EventHandler<MessageAddedEventArgs> MessageAdded;

        /// <inheritdoc/>
        public event EventHandler<MessageRemovedEventArgs> MessageRemoved;

        /// <inheritdoc/>
        public IEnumerable<string> GetMessages()
        {
            return ugpMessageStripCategory.GetMessages();
        }

        #endregion
    }
}
