//-----------------------------------------------------------------------
// <copyright file = "UgpMessageStripCategoryCallbacks.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MessageStrip
{
    using System;
	using System.Collections.Generic;
    using F2XTransport;

    /// <summary>
    /// This class is responsible for handling callbacks from the UgpMessageStrip category.
    /// </summary>
    class UgpMessageStripCategoryCallbacks : IUgpMessageStripCategoryCallbacks
    {
        /// <summary>
        /// The callback interface for handling transactional events.
        /// </summary>
        private readonly IEventCallbacks eventCallbacksInterface;

        /// <summary>
        /// Initializes an instance of <see cref="UgpMessageStripCategoryCallbacks"/>.
        /// </summary>
        /// <param name="eventCallbacksInterface">
        /// The callback interface for the handling transactional events.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacksInterface"/> is null.
        /// </exception>
        public UgpMessageStripCategoryCallbacks(IEventCallbacks eventCallbacksInterface)
        {
            this.eventCallbacksInterface = eventCallbacksInterface ?? throw new ArgumentNullException(nameof(eventCallbacksInterface));
        }

        #region IUgpMessageStripCategoryCallbacks Implementation

        /// <inheritdoc />
        public string ProcessAddMessage(List<string> messages)
        {
            eventCallbacksInterface.PostEvent(new MessageAddedEventArgs(messages));
            return null;
        }

        /// <inheritdoc />
        public string ProcessRemoveMessage(List<string> messages)
        {
            eventCallbacksInterface.PostEvent(new MessageRemovedEventArgs(messages));
            return null;
        }

        #endregion
    }
}
