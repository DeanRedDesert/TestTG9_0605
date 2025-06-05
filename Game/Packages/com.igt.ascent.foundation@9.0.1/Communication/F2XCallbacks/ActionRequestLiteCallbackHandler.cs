//-----------------------------------------------------------------------
// <copyright file = "ActionRequestLiteCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using System.Text;
    using F2X;
    using F2XTransport;
    using IGT.Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Callback handler for the Action Request Lite F2X category.
    /// </summary>
    internal class ActionRequestLiteCallbackHandler : IActionRequestLiteCategoryCallbacks
    {
        private readonly IEventCallbacks eventCallbacks;

        /// <summary>
        /// Initialize a new instance with the given transaction callbacks.
        /// </summary>
        /// <param name="eventCallbacks">The callback handler for transactional events.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="eventCallbacks"/> argument is null.
        /// </exception>
        internal ActionRequestLiteCallbackHandler(IEventCallbacks eventCallbacks)
        {
            this.eventCallbacks = eventCallbacks ?? throw new ArgumentNullException(nameof(eventCallbacks));
        }

        #region IActionRequestLiteCategoryCallbacks Members

        /// <inheritdoc/>
        public string ProcessActionResponseLite(byte[] payload)
        {
            var transactionName = payload == null
                                      ? null
                                      : Encoding.ASCII.GetString(payload);

            eventCallbacks.PostEvent(new ActionResponseLiteEventArgs(transactionName));
            return null;
        }

        #endregion
    }
}
