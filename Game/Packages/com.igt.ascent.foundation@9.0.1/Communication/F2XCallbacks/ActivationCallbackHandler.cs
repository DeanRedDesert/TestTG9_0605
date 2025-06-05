// -----------------------------------------------------------------------
// <copyright file = "ActivationCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using Ascent.Communication.Platform.ExtensionLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using F2X;
    using F2XTransport;

    /// <summary>
    /// Handles activate category callbacks by posting events to <see cref="IEventCallbacks"/>.
    /// </summary>
    internal class ActivationCallbackHandler : IActivationCategoryCallbacks
    {
        private readonly IEventCallbacks eventCallbacks;

        /// <summary>
        /// Instantiates a new <see cref="ActivationCallbackHandler"/>.
        /// </summary>
        /// <param name="eventCallbacks">The callback interface for handling events.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacks"/> is null.
        /// </exception>
        public ActivationCallbackHandler(IEventCallbacks eventCallbacks)
        {
            this.eventCallbacks = eventCallbacks ?? throw new ArgumentNullException(nameof(eventCallbacks));
        }

        #region Implementation of IActivationCategoryCallbacks

        /// <inheritdoc/>
        public string ProcessActivateContext()
        {
            eventCallbacks.PostEvent(new ActivateContextEventArgs());
            return null;
        }

        /// <inheritdoc/>
        public string ProcessInactivateContext()
        {
            eventCallbacks.PostEvent(new InactivateContextEventArgs());
            return null;
        }

        /// <inheritdoc/>
        public string ProcessNewContext()
        {
            eventCallbacks.PostEvent(new NewContextEventArgs());
            return null;
        }

        #endregion
    }
}
