// -----------------------------------------------------------------------
// <copyright file = "SystemActivationCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using Ascent.Communication.Platform.ExtensionBinLib.Interfaces;
    using Ascent.Communication.Platform.ExtensionLib.Interfaces;
    using F2X;
    using F2XTransport;

    /// <summary>
    /// Handles SystemActivation category callbacks by posting events to <see cref="IEventCallbacks"/>.
    /// </summary>
    internal class SystemActivationCallbackHandler : ISystemActivationCategoryCallbacks
    {
        private readonly IEventCallbacks eventCallbacks;

        /// <summary>
        /// Instantiates a new <see cref="SystemActivationCallbackHandler"/>.
        /// </summary>
        /// <param name="eventCallbacks">The callback interface for handling events.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacks"/> is null.
        /// </exception>
        public SystemActivationCallbackHandler(IEventCallbacks eventCallbacks)
        {
            this.eventCallbacks = eventCallbacks ?? throw new ArgumentNullException(nameof(eventCallbacks));
        }

        #region ISystemActivationCategoryCallbacks Members

        /// <inheritdoc/>
        public string ProcessNewSystemContext()
        {
            // This event is used by ExtensionLib.  Can be removed when ExtensionLib is phased out.
            eventCallbacks.PostEvent(new NewSystemExtensionContextEventArgs());

            // This event is used by ExtensionBinLib.
            eventCallbacks.PostEvent(new NewInnerContextEventArgs<ISystemExtensionContext>());
            return null;
        }

        /// <inheritdoc/>
        public string ProcessActivateSystemContext()
        {
            // This event is used by ExtensionLib.  Can be removed when ExtensionLib is phased out.
            eventCallbacks.PostEvent(new ActivateSystemExtensionContextEventArgs());

            // This event is used by ExtensionBinLib.
            eventCallbacks.PostEvent(new ActivateInnerContextEventArgs<ISystemExtensionContext>());
            return null;
        }

        /// <inheritdoc/>
        public string ProcessInactivateSystemContext()
        {
            // This event is used by ExtensionLib.  Can be removed when ExtensionLib is phased out.
            eventCallbacks.PostEvent(new InactivateSystemExtensionContextEventArgs());

            // This event is used by ExtensionBinLib.
            eventCallbacks.PostEvent(new InactivateInnerContextEventArgs<ISystemExtensionContext>());
            return null;
        }

        #endregion
    }
}
